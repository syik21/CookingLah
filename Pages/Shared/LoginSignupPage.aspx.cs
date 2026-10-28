using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace CookingLah.Pages.Shared
{
    public partial class LoginSignupPage : System.Web.UI.Page
    {
        // Connection string name in Web.config
        static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["CookingLahDB"].ConnectionString; }
        }

        static readonly Dictionary<string, string> Blurb = new Dictionary<string, string>
        {
            { "Learner", "Your lessons, quiz results and learning progress are ready." },
            { "Instructor", "Manage your courses, recipes, videos and learner feedback." }
        };

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (Session["SignupViewStateKey"] == null)
                Session["SignupViewStateKey"] = Guid.NewGuid().ToString("N");
            ViewStateUserKey = (string)Session["SignupViewStateKey"];
            ddlCountry.Items.Clear();
            ddlCountry.Items.Add(new ListItem("Select your country", ""));
            var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(c => new RegionInfo(c.Name))
                .Where(r => r.TwoLetterISORegionName.Length == 2)
                .GroupBy(r => r.TwoLetterISORegionName)
                .Select(g => g.First()).OrderBy(r => r.EnglishName, StringComparer.Ordinal);
            foreach (var region in regions)
                ddlCountry.Items.Add(new ListItem(region.EnglishName, region.TwoLetterISORegionName));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            lblLoginError.Visible = false;
            lblSignupError.Visible = false;
            if (!IsPostBack && Session["UserId"] != null)
            {
                string role = Convert.ToString(Session["Role"]);
                if (role == "Admin")
                {
                    FormsAuthentication.SignOut();
                    Session.Clear();
                    Session.Abandon();
                    Response.Redirect(NavigationHelper.GetUrl("Login"));
                    return;
                }
                RedirectAuthenticatedUser(role);
                return;
            }
            if (!IsPostBack)
                ShowView(Request.QueryString["tab"] == "signup" ? 1 : 0);
        }

        // 0 = log in, 1 = sign up, 2 = forgot password, 3 = success
        void ShowView(int i)
        {
            mv.ActiveViewIndex = i;
            pnlTabs.Visible = i <= 1;
            lnkTabLogin.CssClass = i == 0 ? "chip on" : "chip";
            lnkTabSignup.CssClass = i == 1 ? "chip on" : "chip";
        }

        protected void lnkTabLogin_Click(object s, EventArgs e)
        {
            ShowView(0);
        }

        protected void lnkTabSignup_Click(object s, EventArgs e)
        {
            ShowView(1);
        }

        protected void lnkForgot_Click(object s, EventArgs e)
        {
            ShowView(2);
        }

        protected void lnkBackToLogin_Click(object s, EventArgs e)
        {
            ShowView(0);
        }

        // ---------- LOG IN ----------
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblLoginError.Visible = false;

            string login = txtLoginEmail.Text.Trim();
            string pw = txtLoginPass.Text;

            if (login.Length == 0 || login.Length > 100 || pw.Length == 0)
            {
                Err(lblLoginError, "Please enter your username or email and your password.");
                return;
            }

            try
            {
                int userId;
                string username;
                string email;
                string accountStatus;
                string role;
                string password;
                using (var con = DatabaseManager.ConnectDB())
                using (var cmd = new SqlCommand(
                    "SELECT TOP (1) UserID, Password, AccountStatus, Role, Username, Email FROM dbo.Users " +
                    "WHERE Username = @login OR Email = @email " +
                    "ORDER BY CASE WHEN Email = @email THEN 0 ELSE 1 END;", con))
                {
                    cmd.Parameters.Add("@login", SqlDbType.NVarChar, 100).Value = login;
                    cmd.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = login.ToLowerInvariant();
                    con.Open();
                    using (var userData = cmd.ExecuteReader())
                    {
                        if (!userData.Read())
                        {
                            Err(lblLoginError, "Username or Email is not registered.");
                            return;
                        }
                        userId = Convert.ToInt32(userData["UserID"]);
                        password = userData["Password"].ToString();
                        username = userData["Username"].ToString().Trim();
                        email = userData["Email"].ToString().Trim();
                        accountStatus = userData["AccountStatus"].ToString().Trim();
                        role = userData["Role"].ToString().Trim();
                    }
                }

                if (!PasswordManager.isPasswordValid(pw, password))
                {
                    Err(lblLoginError, "Incorrect  password.");
                    return;
                }
                // Check Account Status. Only Active User can Login.
                if (!UserManager.isAccActive(accountStatus))
                {
                    Err(lblLoginError, "Your Account is Inactive.");
                    return;
                }

                // Session User.
                Session["UserId"] = userId;
                Session["Username"] = username;
                Session["Email"] = email;
                Session["Role"] = role;
                CookingLah.F_Services.AuthenticationManager.SignIn(Context, email, chkRemember.Checked);
                string ipAddress = Request.UserHostAddress;
                string deviceInfo = Request.UserAgent;

                SaveDailyLoginLog(userId, ipAddress, deviceInfo);
                UpdateLastLoginTime(userId);

                // Navigate to user dashboard.
                if (role == "Learner")
                {
                    Response.Redirect(NavigationHelper.GetUrl("Learner"), false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else if (role == "Instructor")
                {
                    Response.Redirect(NavigationHelper.GetUrl("Instructor"), false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else if (role == "Admin")
                {
                    Response.Redirect("~/Pages/Admin/AdminDashboard.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception)
            {
                Err(lblLoginError, "Something went wrong. Please try again.");
                return;
            }
        }

        private void RedirectAuthenticatedUser(string role)
        {
            if (role == "Learner") Response.Redirect(NavigationHelper.GetUrl("Learner"));
            else if (role == "Instructor") Response.Redirect(NavigationHelper.GetUrl("Instructor"));
            else if (role == "Admin") Response.Redirect("~/Pages/Admin/AdminDashboard.aspx");
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


        // ---------- SIGN UP ----------
        protected void btnSignup_Click(object sender, EventArgs e)
        {
            lblSignupError.Visible = false;

            string name = txtSignName.Text.Trim();
            string fullName = txtSignFullName.Text.Trim();
            string email = txtSignEmail.Text.Trim().ToLowerInvariant();
            string pw = txtSignPass.Text;
            string role = rblSignupRole.SelectedValue;

            if (name.Length < 2 || name.Length > 50)
            {
                Err(lblSignupError, "Use a username between 2 and 50 characters.");
                return;
            }

            if (fullName.Length == 0 || fullName.Length > 100)
            {
                Err(lblSignupError, "Please enter your full name (up to 100 characters).");
                return;
            }

            if (!IsEmail(email))
            {
                Err(lblSignupError, "Please enter a valid email.");
                return;
            }

            if (pw.Length < 8 || Encoding.UTF8.GetByteCount(pw) > 72)
            {
                Err(lblSignupError, "Use at least 8 characters and at most 72 UTF-8 bytes for your password.");
                return;
            }

            if (role != "Learner" && role != "Chef")
            {
                Err(lblSignupError, "Please choose Learner or Chef / Instructor.");
                return;
            } // Admin can't self-register

            ListItem country = ddlCountry.Items.FindByValue(ddlCountry.SelectedValue);
            if (country == null || string.IsNullOrEmpty(country.Value))
            {
                Err(lblSignupError, "Please select your country.");
                return;
            }

            string profession = null;
            if (role == "Chef")
            {
                string selectedProfession = ddlProfession.SelectedValue;
                if (string.IsNullOrEmpty(selectedProfession) || ddlProfession.Items.FindByValue(selectedProfession) == null)
                {
                    Err(lblSignupError, "Please choose your profession.");
                    return;
                }

                profession = selectedProfession == "Others" ? txtOtherProfession.Text.Trim() : selectedProfession;
                if (profession.Length == 0 || profession.Length > 100)
                {
                    Err(lblSignupError, "Please enter your profession (up to 100 characters).");
                    return;
                }
            }

            string databaseRole = role == "Chef" ? "Instructor" : "Learner";
            int id;

            try
            {
                string passwordHash = PasswordManager.HashPassword(pw);
                using (var con = DatabaseManager.ConnectDB())
                {
                    con.Open();
                    // Both records must succeed; a failed profession insert rolls back the account.
                    using (var transaction = con.BeginTransaction())
                    {
                        using (var check = new SqlCommand(
                            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @username) THEN 1 " +
                            "WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @email) THEN 2 ELSE 0 END;", con, transaction))
                        {
                            check.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = name;
                            check.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = email;
                            int duplicate = (int)check.ExecuteScalar();
                            if (duplicate != 0)
                            {
                                Err(lblSignupError, duplicate == 1
                                    ? "That username is already taken. Please choose another username."
                                    : "That email is already registered. Try logging in.");
                                return;
                            }
                        }

                        using (var cmd = new SqlCommand(
                            "INSERT INTO dbo.Users (Username, FullName, Email, Password, Role, Country) VALUES (@n, @fullName, @e, @p, @r, @country); SELECT CAST(SCOPE_IDENTITY() AS INT);", con, transaction))
                        {
                            cmd.Parameters.Add("@n", SqlDbType.NVarChar, 50).Value = name;
                            cmd.Parameters.Add("@fullName", SqlDbType.NVarChar, 100).Value = fullName;
                            cmd.Parameters.Add("@e", SqlDbType.NVarChar, 100).Value = email;
                            cmd.Parameters.Add("@p", SqlDbType.NVarChar, 255).Value = passwordHash;
                            cmd.Parameters.Add("@r", SqlDbType.NVarChar, 20).Value = databaseRole;
                            cmd.Parameters.Add("@country", SqlDbType.NVarChar, 100).Value = country.Text;
                            id = (int)cmd.ExecuteScalar();
                        }
                        if (role == "Chef")
                        {
                            using (var cmd = new SqlCommand(
                                "INSERT INTO dbo.ChefProfession (ChefID, Profession) VALUES (@id, @profession)", con, transaction))
                            {
                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                                cmd.Parameters.Add("@profession", SqlDbType.NVarChar, 100).Value = profession;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                Err(lblSignupError, "That username or email is already registered. Choose another username or try logging in.");
                return;
            }
            catch (Exception)
            {
                Err(lblSignupError, "Something went wrong. Please try again.");
                return;
            }

            string first = fullName.Split(' ')[0];
            Success(databaseRole, "Hi, " + first + "!",
                (role == "Chef" ? "Account created. Your content will be reviewed before it goes live. " : "Account created. ") + Blurb[databaseRole]);
        }

        // ---------- FORGOT PASSWORD ----------
        protected void btnForgot_Click(object sender, EventArgs e)
        {
            lblForgotError.Visible = false;

            string email = txtForgotEmail.Text.Trim();

            if (!IsEmail(email))
            {
                Err(lblForgotError, "Please enter a valid email.");
                return;
            }

            try
            {
                using (var con = new SqlConnection(ConnStr))
                {
                    con.Open();

                    int? userId = null;
                    using (var find = new SqlCommand("SELECT UserID FROM Users WHERE Email=@email AND AccountStatus='Active'", con))
                    {
                        find.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = email;
                        object result = find.ExecuteScalar();
                        if (result != null) userId = Convert.ToInt32(result);
                    }

                    if (userId.HasValue)
                    {
                        string token = PasswordResetTokenManager.GenerateResetToken();
                        int resetTokenId;
                        using (SqlTransaction transaction = con.BeginTransaction())
                        {
                            using (var expire = new SqlCommand("UPDATE PasswordResetTokens SET IsUsed=1 WHERE UserID=@userId AND IsUsed=0", con, transaction))
                            {
                                expire.Parameters.Add("@userId", SqlDbType.Int).Value = userId.Value;
                                expire.ExecuteNonQuery();
                            }
                            using (var insert = new SqlCommand(@"
                                INSERT INTO PasswordResetTokens (UserID,TokenHash,ExpiresAt)
                                OUTPUT inserted.ResetTokenID
                                VALUES (@userId,@tokenHash,DATEADD(MINUTE,15,SYSUTCDATETIME()))", con, transaction))
                            {
                                insert.Parameters.Add("@userId", SqlDbType.Int).Value = userId.Value;
                                insert.Parameters.Add("@tokenHash", SqlDbType.NVarChar, 255).Value = PasswordResetTokenManager.HashToken(token);
                                resetTokenId = Convert.ToInt32(insert.ExecuteScalar());
                            }
                            transaction.Commit();
                        }

                        string link = Request.Url.GetLeftPart(UriPartial.Authority)
                            + ResolveUrl("~/Pages/Guest/ResetPassword/ResetPasswordPage.aspx")
                            + "?token=" + Server.UrlEncode(token);
                        try
                        {
                            SendPasswordResetEmail(email, link);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.TraceError("Password reset email failed: {0}", ex.Message);
                            using (var expire = new SqlCommand("UPDATE PasswordResetTokens SET IsUsed=1 WHERE ResetTokenID=@id", con))
                            {
                                expire.Parameters.Add("@id", SqlDbType.Int).Value = resetTokenId;
                                expire.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Password reset request failed: {0}", ex.Message);
            }

            lblOkTitle.Text = "Check your inbox";
            lblOkMsg.Text = "If an account exists for this email, a password reset link has been sent.";
            hlDash.Visible = false;
            ShowView(3);
        }

        // Send the password reset link to the registered email.
        static void SendPasswordResetEmail(string recipientEmail, string resetLink)
        {
            string host = ConfigurationManager.AppSettings["SmtpHost"];
            string sender = ConfigurationManager.AppSettings["SmtpEmail"];
            string password = ConfigurationManager.AppSettings["SmtpAppPassword"];
            int port;
            bool enableSsl;
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(sender) || string.IsNullOrWhiteSpace(password) ||
                !int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out port) ||
                !bool.TryParse(ConfigurationManager.AppSettings["SmtpEnableSsl"], out enableSsl))
                throw new ConfigurationErrorsException("SMTP settings are incomplete.");

            string safeLink = System.Web.HttpUtility.HtmlAttributeEncode(resetLink);
            string body = @"<div style='font-family:Arial,sans-serif;color:#35030c;line-height:1.6'>
                <h1 style='margin-bottom:8px'>Cooking LAH</h1>
                <h2>Reset Your Password</h2>
                <p>We received a request to reset your Cooking LAH password.</p>
                <p><a href='" + safeLink + @"' style='display:inline-block;padding:12px 20px;background:#f93209;color:#fff;text-decoration:none;border-radius:8px'>Reset Password</a></p>
                <p>This link will expire in 15 minutes.</p>
                <p>If you did not request this password reset, you can safely ignore this email.</p>
                </div>";

            using (var message = new MailMessage())
            using (var smtp = new SmtpClient(host, port))
            {
                message.From = new MailAddress(sender, "Cooking LAH");
                message.To.Add(recipientEmail);
                message.Subject = "Reset your Cooking LAH password";
                message.Body = body;
                message.IsBodyHtml = true;
                smtp.EnableSsl = enableSsl;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(sender, password);
                smtp.Send(message);
            }
        }

        // ---------- helpers ----------

        void Success(string role, string title, string msg)
        {
            lblOkTitle.Text = Server.HtmlEncode(title);
            lblOkMsg.Text = Server.HtmlEncode(msg);
            hlDash.NavigateUrl = "~/Pages/Shared/LoginSignupPage.aspx";
            hlDash.Text = "Back to Login Page";
            hlDash.Visible = true;
            ShowView(3);
        }

        static void Err(Label l, string msg)
        {
            l.Text = msg;
            l.Visible = true;
        }

        static bool IsEmail(string s)
        {
            return s.Length <= 100 && Regex.IsMatch(s, @"^\S+@\S+\.\S+$");
        }

    }
}
