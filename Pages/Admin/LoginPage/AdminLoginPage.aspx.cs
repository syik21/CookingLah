using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace CookingLah.Pages.Admin.LoginPage
{
    public partial class AdminLoginPage : System.Web.UI.Page
    {
        protected string CsrfToken { get { return Convert.ToString(Session["AdminLoginCsrf"]); } }

        protected override void OnInit(EventArgs e)
        {
            if (Session["AdminLoginCsrf"] == null)
                Session["AdminLoginCsrf"] = Guid.NewGuid().ToString("N");
            ViewStateUserKey = CsrfToken;
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var entryExpiry = Session["AdminLoginEntryExpiresUtc"] as DateTime?;
                Session.Remove("AdminLoginEntryExpiresUtc");
                ViewState["AdminEntryAllowed"] = entryExpiry.HasValue && entryExpiry.Value > DateTime.UtcNow;
                if ((bool)ViewState["AdminEntryAllowed"])
                    ViewState["AdminEntryExpiresUtc"] = entryExpiry.Value;
            }

            var formExpiry = ViewState["AdminEntryExpiresUtc"] as DateTime?;
            if (!Equals(ViewState["AdminEntryAllowed"], true) ||
                !formExpiry.HasValue || formExpiry.Value <= DateTime.UtcNow)
            {
                Response.Redirect("~/Pages/Shared/LoginSignupPage.aspx");
                return;
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            lblLoginError.Visible = false;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (Request.Form["adminLoginCsrf"] != CsrfToken)
            {
                ShowError("Your session has changed. Refresh the page and try again.");
                return;
            }
            string login = txtLoginEmail.Text.Trim();
            string password = txtLoginPass.Text;
            txtLoginPass.Text = string.Empty;
            if (login.Length == 0 || login.Length > 100 || password.Length == 0)
            {
                ShowError("Enter your username or email and password.");
                return;
            }
            try
            {
                int userId;
                string username, email, hash, role, status;
                using (var con = DatabaseManager.ConnectDB())
                using (var cmd = new SqlCommand(
                    "SELECT TOP (1) UserID,Username,Email,Password,Role,AccountStatus FROM dbo.Users " +
                    "WHERE Role='Admin' AND (Username=@login OR Email=@email) " +
                    "ORDER BY CASE WHEN Email=@email THEN 0 ELSE 1 END,UserID", con))
                {
                    cmd.Parameters.Add("@login", SqlDbType.NVarChar, 100).Value = login;
                    cmd.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = login.ToLowerInvariant();
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) { ShowError("Unable to sign in. Check your admin credentials and account status."); return; }
                        userId = Convert.ToInt32(reader["UserID"]);
                        username = Convert.ToString(reader["Username"]);
                        email = Convert.ToString(reader["Email"]);
                        hash = Convert.ToString(reader["Password"]);
                        role = Convert.ToString(reader["Role"]);
                        status = Convert.ToString(reader["AccountStatus"]);
                    }
                }
                // Never establish an admin session for learners, chefs or disabled accounts.
                if (role != "Admin" || status != "Active" || !PasswordManager.isPasswordValid(password, hash))
                {
                    ShowError("Unable to sign in. Check your admin credentials and account status.");
                    return;
                }
                Session.Clear();
                Session["UserId"] = userId;
                Session["Username"] = username;
                Session["Email"] = email;
                Session["Role"] = role;
                AuthenticationManager.SignIn(Context, email, false);
                txtLoginEmail.Text = string.Empty;
                pnlLogin.Visible = false;
                pnlSuccess.Visible = true;
                string ipAddress = Request.UserHostAddress;
                string deviceInfo = Request.UserAgent;

                SaveDailyLoginLog(userId, ipAddress, deviceInfo);
                UpdateLastLoginTime(userId);

                Response.Redirect("~/Pages/Admin/AdminDashboard.aspx");
                // Navigation is intentionally left for the project owner to configure.
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Admin login database error: {0}", ex.Number);
                ShowError("Sign-in is temporarily unavailable. Please try again.");
            }
            catch (BCrypt.Net.SaltParseException)
            {
                ShowError("Unable to sign in. Contact another administrator for help with your account.");
            }
        }
        private void UpdateLastLoginTime(int userId)
        {
            string query = @"
                            UPDATE Users
                            SET LastLogin= GETDATE()
                            WHERE UserID = @UserID;";

            using (var con = DatabaseManager.ConnectDB())
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Save one login log per user for each day.
        private void SaveDailyLoginLog(
            int userId,
            string ipAddress,
            string deviceInfo)
        {
            string query = @"
        IF NOT EXISTS
        (
            SELECT 1
            FROM UserLoginLog
            WHERE UserID = @UserID
              AND LoginDate = CAST(GETDATE() AS DATE)
        )
        BEGIN
            INSERT INTO UserLoginLog
            (
                UserID,
                LoginDate,
                LoginTime,
                IPAddress,
                DeviceInfo
            )
            VALUES
            (
                @UserID,
                CAST(GETDATE() AS DATE),
                GETDATE(),
                @IPAddress,
                @DeviceInfo
            );
        END;";

            using (var con = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.Parameters.AddWithValue("@UserID", userId);

                command.Parameters.AddWithValue(
                    "@IPAddress",
                    string.IsNullOrWhiteSpace(ipAddress)
                        ? (object)DBNull.Value
                        : ipAddress
                );

                command.Parameters.AddWithValue(
                    "@DeviceInfo",
                    string.IsNullOrWhiteSpace(deviceInfo)
                        ? (object)DBNull.Value
                        : deviceInfo
                );

                con.Open();
                command.ExecuteNonQuery();
            }
        }

        private void ShowError(string message)
        {
            lblLoginError.Text = message;
            lblLoginError.Visible = true;
        }
    }
}
