using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;

namespace CookingLah.F_Services
{
    public static class AuthenticationManager
    {
        private const int RememberDays = 30;

        public static void SignIn(HttpContext context, string email, bool rememberMe)
        {
            DateTime issuedAt = DateTime.Now;
            DateTime expiresAt = rememberMe ? issuedAt.AddDays(RememberDays) : issuedAt.AddMinutes(FormsAuthentication.Timeout.TotalMinutes);
            var ticket = new FormsAuthenticationTicket(
                2, email, issuedAt, expiresAt, rememberMe, "", FormsAuthentication.FormsCookiePath);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL || context.Request.IsSecureConnection,
                Path = FormsAuthentication.FormsCookiePath,
                SameSite = SameSiteMode.Lax
            };
            if (rememberMe)
                cookie.Expires = expiresAt;
            context.Response.Cookies.Add(cookie);
        }

        // Restore server-side identity after a remembered browser session returns.
        public static bool RestoreSession(HttpContext context)
        {
            if (context.Session == null)
                return false;
            if (context.Session["UserId"] != null)
                return true;
            if (context.User == null || context.User.Identity == null || !context.User.Identity.IsAuthenticated)
                return false;

            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB())
                using (SqlCommand command = new SqlCommand(@"
                    SELECT UserID,Username,Email,Role
                    FROM Users
                    WHERE Email=@email
                      AND AccountStatus='Active'
                      AND Role IN ('Learner','Instructor','Admin')", connection))
                {
                    command.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = context.User.Identity.Name;
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            FormsAuthentication.SignOut();
                            context.Session.Clear();
                            return false;
                        }
                        if (Convert.ToString(reader["Role"]) == "Admin")
                        {
                            FormsAuthentication.SignOut();
                            context.Session.Clear();
                            return false;
                        }
                        context.Session["UserId"] = Convert.ToInt32(reader["UserID"]);
                        context.Session["Username"] = Convert.ToString(reader["Username"]);
                        context.Session["Email"] = Convert.ToString(reader["Email"]);
                        context.Session["Role"] = Convert.ToString(reader["Role"]);
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Remembered session restore failed: {0}", ex.Number);
                return false;
            }
        }
    }
}
