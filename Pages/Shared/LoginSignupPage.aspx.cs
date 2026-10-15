using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace CookingLah.Pages.Shared
{
    public partial class LoginPage : System.Web.UI.Page
    {
        // Connection string name in Web.config
        static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["CookingLahDB"].ConnectionString; }
        }

        // Where each role goes after login. Change these to your real pages.
        static readonly Dictionary<string, string> DashUrl = new Dictionary<string, string>
        {
            { "Learner", "~/Learner/Dashboard.aspx" },
            { "Chef",    "~/Chef/Dashboard.aspx" },
            { "Admin",   "~/Admin/Dashboard.aspx" }
        };

        static readonly Dictionary<string, string> RoleLabel = new Dictionary<string, string>
        {
            { "Learner", "Learner" },
            { "Chef", "Chef / Instructor" },
            { "Admin", "Admin" }
        };

        static readonly Dictionary<string, string> Blurb = new Dictionary<string, string>
        {
            { "Learner", "Your lessons, quiz results and learning progress are ready." },
            { "Chef",    "Manage your courses, recipes, videos and learner feedback." },
            { "Admin",   "Review users, approve chef content and see platform stats." }
        };

        protected void Page_Load(object sender, EventArgs e)
        {
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

            string email = txtLoginEmail.Text.Trim();
            string pw = txtLoginPass.Text;
            string role = rblLoginRole.SelectedValue;

            if (!IsEmail(email) || pw.Length == 0)
            {
                Err(lblLoginError, "Please enter a valid email and your password.");
                return;
            }

            int id = 0;
            string name = null;
            string hash = null;
            string dbRole = null;
            bool active = false;

            try
            {
                using (var con = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand("SELECT UserId, FullName, PasswordHash, Role, IsActive FROM Users WHERE Email = @e", con))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    con.Open();

                    using (var r = cmd.ExecuteReader())
                        if (r.Read())
                        {
                            id = r.GetInt32(0);
                            name = r.GetString(1);
                            hash = r.GetString(2);
                            dbRole = r.GetString(3);
                            active = r.GetBoolean(4);
                        }
                }
            }
            catch (Exception)
            {
                Err(lblLoginError, "Something went wrong. Please try again.");
                return;
            }

            if (hash == null || !Verify(pw, hash) || dbRole != role)
            {
                Err(lblLoginError, "Email, password or role is incorrect.");
                return;
            }

            if (!active)
            {
                Err(lblLoginError, "This account is disabled. Please contact an admin.");
                return;
            }

            SignIn(id, name, email, dbRole, chkRemember.Checked);
            Success(dbRole, "You're in!", Blurb[dbRole]);
        }

        // ---------- SIGN UP ----------
        protected void btnSignup_Click(object sender, EventArgs e)
        {
            lblSignupError.Visible = false;

            string name = txtSignName.Text.Trim();
            string email = txtSignEmail.Text.Trim();
            string pw = txtSignPass.Text;
            string role = rblSignupRole.SelectedValue;

            if (name.Length < 2)
            {
                Err(lblSignupError, "Tell us what to call you.");
                return;
            }

            if (!IsEmail(email))
            {
                Err(lblSignupError, "Please enter a valid email.");
                return;
            }

            if (pw.Length < 8)
            {
                Err(lblSignupError, "Use at least 8 characters for your password.");
                return;
            }

            if (role != "Learner" && role != "Chef")
            {
                Err(lblSignupError, "Please choose Learner or Chef / Instructor.");
                return;
            } // Admin can't self-register

            int id;

            try
            {
                using (var con = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(
                    "INSERT INTO Users (FullName, Email, PasswordHash, Role) VALUES (@n, @e, @p, @r); SELECT CAST(SCOPE_IDENTITY() AS INT);", con))
                {
                    cmd.Parameters.AddWithValue("@n", name);
                    cmd.Parameters.AddWithValue("@e", email);
                    cmd.Parameters.AddWithValue("@p", Hash(pw));
                    cmd.Parameters.AddWithValue("@r", role);
                    con.Open();
                    id = (int)cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                Err(lblSignupError, "That email is already registered. Try logging in.");
                return;
            }
            catch (Exception)
            {
                Err(lblSignupError, "Something went wrong. Please try again.");
                return;
            }

            SignIn(id, name, email, role, false);

            string first = name.Split(' ')[0];
            Success(role, "Welcome, " + first + "!",
                (role == "Chef" ? "Account created. Your content will be reviewed before it goes live. " : "Account created. ") + Blurb[role]);
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

                    object uid;
                    using (var find = new SqlCommand("SELECT UserId FROM Users WHERE Email = @e AND IsActive = 1", con))
                    {
                        find.Parameters.AddWithValue("@e", email);
                        uid = find.ExecuteScalar();
                    }

                    if (uid != null)
                    {
                        var bytes = new byte[32];
                        using (var rng = RandomNumberGenerator.Create())
                            rng.GetBytes(bytes);

                        string token = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');

                        using (var ins = new SqlCommand("INSERT INTO PasswordResets (UserId, TokenHash, ExpiresAt) VALUES (@u, @t, DATEADD(HOUR, 1, SYSUTCDATETIME()))", con))
                        {
                            ins.Parameters.AddWithValue("@u", (int)uid);
                            ins.Parameters.AddWithValue("@t", Sha256(token));
                            ins.ExecuteNonQuery();
                        }

                        string link = Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/ResetPassword.aspx") + "?token=" + token;
                        SendResetEmail(email, link);
                    }
                }
            }
            catch (Exception)
            {
                Err(lblForgotError, "Something went wrong. Please try again.");
                return;
            }

            // Same message whether or not the email exists (so nobody can probe for accounts)
            lblOkTitle.Text = "Check your inbox";
            lblOkMsg.Text = "If " + Server.HtmlEncode(email) + " has an account, a password reset link is on its way.";
            hlDash.Visible = false;
            ShowView(3);
        }

        // TODO: configure SMTP in Web.config (<system.net><mailSettings>), then uncomment.
        static void SendResetEmail(string to, string link)
        {
            // using (var m = new System.Net.Mail.MailMessage("no-reply@cookinglah.com", to, "Reset your Cooking LAH password",
            //        "Click this link to choose a new password (valid for 1 hour): " + link))
            // using (var smtp = new System.Net.Mail.SmtpClient()) smtp.Send(m);
        }

        // ---------- helpers ----------
        void SignIn(int id, string name, string email, string role, bool remember)
        {
            Session["UserId"] = id;
            Session["UserName"] = name;
            Session["Role"] = role;
            FormsAuthentication.SetAuthCookie(email, remember);
        }

        void Success(string role, string title, string msg)
        {
            lblOkTitle.Text = title;
            lblOkMsg.Text = Server.HtmlEncode(msg);
            hlDash.NavigateUrl = DashUrl[role];
            hlDash.Text = "Go to my " + RoleLabel[role].ToLower() + " dashboard";
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
            return s.Length <= 256 && Regex.IsMatch(s, @"^\S+@\S+\.\S+$");
        }

        static string Sha256(string s)
        {
            using (var h = SHA256.Create())
                return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(s)));
        }

        // Passwords are stored as "iterations.salt.hash" (PBKDF2), never as plain text.
        static string Hash(string pw)
        {
            const int it = 100000;
            var salt = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            return it + "." + Convert.ToBase64String(salt) + "." + Convert.ToBase64String(Pbkdf2(pw, salt, it, 32));
        }

        static bool Verify(string pw, string stored)
        {
            try
            {
                var p = stored.Split('.');
                if (p.Length != 3)
                    return false;

                var expected = Convert.FromBase64String(p[2]);
                var actual = Pbkdf2(pw, Convert.FromBase64String(p[1]), int.Parse(p[0]), expected.Length);

                uint diff = (uint)expected.Length ^ (uint)actual.Length;
                for (int i = 0; i < expected.Length && i < actual.Length; i++)
                    diff |= (uint)(expected[i] ^ actual[i]);

                return diff == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static byte[] Pbkdf2(string pw, byte[] salt, int it, int len)
        {
            using (var d = new Rfc2898DeriveBytes(pw, salt, it))
                return d.GetBytes(len);
        }
    }
}
