using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace CookingLah.Pages.Shared.Profile
{
    public partial class ProfilePage : System.Web.UI.Page
    {
        protected string DeleteFeedback = "";
        private string ProfileRole = "";
        protected bool CanDeleteProfile { get { return ProfileRole == "Learner" || ProfileRole == "Instructor"; } }
        protected string DeleteActionLabel { get { return ProfileRole == "Instructor" ? "Deactivate profile" : "Delete profile"; } }
        protected string DeleteDescription
        {
            get
            {
                if (ProfileRole == "Learner") return "Permanently delete your account and its associated learning records. This cannot be undone.";
                if (ProfileRole == "Instructor") return "Deactivate your account and stop access to Cooking LAH. Your profile and courses will be retained. Contact an administrator if you need to reactivate your account.";
                return "To close an admin account, ask another administrator to deactivate it in Manage users.";
            }
        }
        protected string DetailsFeedback = "";
        protected string PasswordFeedback = "";
        protected bool PasswordFeedbackIsError
        {
            get
            {
                return PasswordFeedback.StartsWith("Enter", StringComparison.OrdinalIgnoreCase) ||
                    PasswordFeedback.StartsWith("Your current password is incorrect", StringComparison.OrdinalIgnoreCase) ||
                    PasswordFeedback.StartsWith("Please verify", StringComparison.OrdinalIgnoreCase) ||
                    PasswordFeedback.StartsWith("Use at least", StringComparison.OrdinalIgnoreCase) ||
                    PasswordFeedback.StartsWith("Choose a password", StringComparison.OrdinalIgnoreCase) ||
                    PasswordFeedback.StartsWith("Your password changed elsewhere", StringComparison.OrdinalIgnoreCase);
            }
        }
        protected string PhotoFeedback = "";
        protected string CsrfToken { get { return Convert.ToString(Session["ProfileCsrf"]); } }
        private int UserId { get { return Convert.ToInt32(Session["UserId"]); } }
        private const string PhotoFolder = "~/F_Assets/ProfilePicture/";

        protected override void OnInit(EventArgs e)
        {
            if (Session["ProfileCsrf"] == null)
                Session["ProfileCsrf"] = Guid.NewGuid().ToString("N");
            ViewStateUserKey = CsrfToken;
            base.OnInit(e);
        }
        protected string FullName { get { return (string)ViewState["FullName"] ?? ""; } }
        protected string Username { get { return (string)ViewState["Username"] ?? ""; } }
        protected string Email { get { return (string)ViewState["Email"] ?? ""; } }
        protected string ProfileImageUrl
        {
            get { return (string)ViewState["ProfileImageUrl"] ?? ResolveUrl("~/F_Assets/mascot.png"); }
        }

        protected override void OnPreInit(EventArgs e)
        {
            // Choose only known master files; never accept a master path from the browser.
            string role = Convert.ToString(Session["Role"]).Trim();
            switch (role.ToLowerInvariant())
            {
                case "learner":
                    MasterPageFile = "~/Pages/Learner/Learner.Master";
                    break;
                case "instructor":
                case "chef":
                    MasterPageFile = "~/Pages/Chef/Chef.Master";
                    break;
                case "admin":
                    MasterPageFile = "~/Pages/Admin/Admin.Master";
                    break;
                default:
                    break;
            }
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int userId;
            if (Session["Role"] == null ||
                !int.TryParse(Convert.ToString(Session["UserId"]), out userId) || userId <= 0)
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"));
                return;
            }
            Form.Enctype = "multipart/form-data";
            string action = Request.Form["profileAction"];
            if (Request.HttpMethod == "POST" && !string.IsNullOrEmpty(action))
            {
                if (Request.Form["profileCsrf"] != CsrfToken)
                {
                    SetFeedback(action, "Your session has changed. Reload the page and try again.");
                }
                else
                {
                    try
                    {
                        switch (action)
                        {
                            case "details": SaveProfile_Click(sender, e); break;
                            case "upload": UploadPhoto_Click(sender, e); break;
                            case "remove": RemovePhoto_Click(sender, e); break;
                            case "delete": if (DeleteProfile()) return; break;
                        }
                    }
                    catch (SqlException ex)
                    {
                        SetFeedback(action, ex.Number == 2601 || ex.Number == 2627
                            ? "That username or email is already in use. Please choose another."
                            : "We could not save your changes. Please try again.");
                    }
                    catch (IOException) { SetFeedback(action, "The photo could not be saved. Please try again."); }
                    catch (UnauthorizedAccessException) { SetFeedback(action, "The photo folder is not writable. Please contact the administrator."); }
                }
            }
            LoadProfile();
        }

        private void LoadProfile()
        {
            using (var con = DatabaseManager.ConnectDB())
            using (var cmd = new SqlCommand(
                "SELECT FullName, Username, Email, ProfileImage, Role FROM dbo.Users WHERE UserID = @userId", con))
            {
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = Convert.ToInt32(Session["UserId"]);
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        Session.Clear();
                        Response.Redirect(NavigationHelper.GetUrl("Main"));
                        return;
                    }
                    ProfileRole = Convert.ToString(reader["Role"]);
                    ViewState["FullName"] = Convert.ToString(reader["FullName"]);
                    ViewState["Username"] = Convert.ToString(reader["Username"]);
                    ViewState["Email"] = Convert.ToString(reader["Email"]);
                    string photo = Convert.ToString(reader["ProfileImage"]).Trim();
                    Uri absoluteUrl;
                    if (!string.IsNullOrEmpty(photo) && !photo.StartsWith("//") && !photo.Contains("\\") &&
                        (!Uri.TryCreate(photo, UriKind.Absolute, out absoluteUrl) ||
                         absoluteUrl.Scheme == Uri.UriSchemeHttp || absoluteUrl.Scheme == Uri.UriSchemeHttps))
                    {
                        ViewState["ProfileImageUrl"] = ResolveUrl(photo);
                    }
                }
            }
        }

        protected void SaveProfile_Click(object sender, EventArgs e)
        {
            string name = (Request.Form["fullName"] ?? "").Trim();
            string username = (Request.Form["username"] ?? "").Trim();
            string email = (Request.Form["email"] ?? "").Trim();
            if (name.Length == 0 || name.Length > 100 || username.Length == 0 || username.Length > 50 ||
                email.Length == 0 || email.Length > 100 || !UserManager.IsEmail(email))
            {
                DetailsFeedback = "Enter a full name, username, and valid email within the allowed lengths.";
                return;
            }
            using (var con = DatabaseManager.ConnectDB())
            using (var cmd = new SqlCommand("UPDATE dbo.Users SET FullName=@name, Username=@username, Email=@email, UpdatedAt=GETDATE() WHERE UserID=@id", con))
            {
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = name;
                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username;
                cmd.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = email;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = UserId;
                con.Open();
                // Exclude this account so keeping its existing username/email is allowed.
                // Unique database constraints still protect against simultaneous saves.
                using (var duplicate = new SqlCommand(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE Username=@username AND UserID<>@id) THEN 1 ELSE 0 END, " +
                    "CASE WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE Email=@email AND UserID<>@id) THEN 1 ELSE 0 END", con))
                {
                    duplicate.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username;
                    duplicate.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = email;
                    duplicate.Parameters.Add("@id", SqlDbType.Int).Value = UserId;
                    using (var reader = duplicate.ExecuteReader())
                    {
                        reader.Read();
                        bool usernameTaken = reader.GetInt32(0) == 1;
                        bool emailTaken = reader.GetInt32(1) == 1;
                        if (usernameTaken || emailTaken)
                        {
                            DetailsFeedback = usernameTaken && emailTaken
                                ? "That username and email are already in use. Please choose different ones."
                                : usernameTaken ? "That username is already in use. Please choose another."
                                : "That email is already in use. Please choose another.";
                            return;
                        }
                    }
                }
                if (cmd.ExecuteNonQuery() != 1) { DetailsFeedback = "Your account could not be updated."; return; }
            }
            Session["Username"] = username;
            Session["Email"] = email;
            DetailsFeedback = "Your profile has been saved.";
        }

        protected void UploadPhoto_Click(object sender, EventArgs e)
        {
            var file = Request.Files["profilePhoto"];
            if (file == null || file.ContentLength == 0 || file.ContentLength > 2 * 1024 * 1024)
            { PhotoFeedback = "Choose a JPG or PNG image up to 2 MB."; return; }
            string newPath = PhotoFolder + UserId + "_" + Guid.NewGuid().ToString("N") + ".png";
            byte[] encoded;
            try
            {
                using (var source = Image.FromStream(file.InputStream, true, true))
                {
                    if ((source.RawFormat.Guid != ImageFormat.Jpeg.Guid && source.RawFormat.Guid != ImageFormat.Png.Guid) ||
                        source.Width > 4096 || source.Height > 4096 || (long)source.Width * source.Height > 16000000)
                    { PhotoFeedback = "Choose a JPG or PNG no larger than 4096 pixels per side and 16 megapixels."; return; }
                    using (var clean = new Bitmap(source.Width, source.Height))
                    using (var graphics = Graphics.FromImage(clean))
                    using (var output = new MemoryStream())
                    {
                        graphics.DrawImage(source, 0, 0, source.Width, source.Height);
                        clean.Save(output, ImageFormat.Png);
                        encoded = output.ToArray();
                    }
                }
            }
            catch (ArgumentException) { PhotoFeedback = "This file is not a readable JPG or PNG image."; return; }
            catch (System.Runtime.InteropServices.ExternalException) { PhotoFeedback = "This image could not be processed. Choose another JPG or PNG."; return; }
            catch (OutOfMemoryException) { PhotoFeedback = "This image could not be processed. Choose a smaller JPG or PNG."; return; }
            Directory.CreateDirectory(Server.MapPath(PhotoFolder));
            File.WriteAllBytes(Server.MapPath(newPath), encoded);
            try { ReplacePhoto(newPath); }
            catch { DeleteOwnedPhoto(newPath); throw; }
            PhotoFeedback = "Your profile photo has been saved.";
        }

        protected void RemovePhoto_Click(object sender, EventArgs e)
        {
            ReplacePhoto(null);
            PhotoFeedback = "Your profile photo has been removed.";
        }

        protected void btnStartPasswordChange_Click(object sender, EventArgs e)
        {
            ClearPasswordVerification();
            pnlPasswordStart.Visible = false;
            pnlPasswordVerify.Visible = true;
            pnlPasswordNew.Visible = false;
            PasswordFeedback = "Enter your current password to continue.";
        }

        protected void btnVerifyCurrentPassword_Click(object sender, EventArgs e)
        {
            string current = txtCurrentPassword.Text;
            if (current.Length == 0 || Encoding.UTF8.GetByteCount(current) > 72)
            {
                PasswordFeedback = "Enter your current password to continue.";
                pnlPasswordVerify.Visible = true;
                pnlPasswordNew.Visible = false;
                ScriptManager.GetCurrent(Page).SetFocus(txtCurrentPassword);
                return;
            }

            string passwordHash;
            using (var con = DatabaseManager.ConnectDB())
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT Password FROM dbo.Users WHERE UserID=@id", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = UserId;
                    passwordHash = Convert.ToString(cmd.ExecuteScalar());
                }
            }

            if (string.IsNullOrEmpty(passwordHash) || !PasswordManager.isPasswordValid(current, passwordHash))
            {
                ClearPasswordVerification();
                pnlPasswordStart.Visible = false;
                pnlPasswordVerify.Visible = true;
                pnlPasswordNew.Visible = false;
                PasswordFeedback = "Your current password is incorrect.";
                ScriptManager.GetCurrent(Page).SetFocus(txtCurrentPassword);
                return;
            }

            Session[PasswordChangeUserIdKey] = UserId;
            Session[PasswordChangeHashKey] = passwordHash;
            Session[PasswordChangeExpiryKey] = DateTime.UtcNow.AddMinutes(5);
            pnlPasswordStart.Visible = false;
            pnlPasswordVerify.Visible = false;
            pnlPasswordNew.Visible = true;
            PasswordFeedback = "Current password verified. Choose a new password.";
            ScriptManager.GetCurrent(Page).SetFocus(txtNewPassword);
        }

        protected void btnSaveNewPassword_Click(object sender, EventArgs e)
        {
            int verifiedUserId;
            DateTime expiresAt;
            string verifiedHash = Convert.ToString(Session[PasswordChangeHashKey]);
            bool verificationIsValid =
                int.TryParse(Convert.ToString(Session[PasswordChangeUserIdKey]), out verifiedUserId) &&
                DateTime.TryParse(Convert.ToString(Session[PasswordChangeExpiryKey]), out expiresAt) &&
                verifiedUserId == UserId &&
                expiresAt > DateTime.UtcNow &&
                !string.IsNullOrEmpty(verifiedHash);

            if (!verificationIsValid)
            {
                ClearPasswordVerification();
                pnlPasswordStart.Visible = false;
                pnlPasswordVerify.Visible = true;
                pnlPasswordNew.Visible = false;
                PasswordFeedback = "Please verify your current password again.";
                ScriptManager.GetCurrent(Page).SetFocus(txtCurrentPassword);
                return;
            }

            string next = txtNewPassword.Text;
            string confirmation = txtConfirmPassword.Text;
            if (next.Length < 8 || Encoding.UTF8.GetByteCount(next) > 72 || next != confirmation)
            {
                pnlPasswordStart.Visible = false;
                pnlPasswordVerify.Visible = false;
                pnlPasswordNew.Visible = true;
                PasswordFeedback = "Use at least 8 characters and at most 72 UTF-8 bytes, and make sure both new passwords match.";
                ScriptManager.GetCurrent(Page).SetFocus(txtNewPassword);
                return;
            }

            if (PasswordManager.isPasswordValid(next, verifiedHash))
            {
                pnlPasswordStart.Visible = false;
                pnlPasswordVerify.Visible = false;
                pnlPasswordNew.Visible = true;
                PasswordFeedback = "Choose a password different from your current password.";
                ScriptManager.GetCurrent(Page).SetFocus(txtNewPassword);
                return;
            }

            using (var con = DatabaseManager.ConnectDB())
            using (var cmd = new SqlCommand("UPDATE dbo.Users SET Password=@password, UpdatedAt=GETDATE() WHERE UserID=@id AND Password=@old", con))
            {
                cmd.Parameters.Add("@password", SqlDbType.NVarChar, 255).Value = PasswordManager.HashPassword(next);
                cmd.Parameters.Add("@old", SqlDbType.NVarChar, 255).Value = verifiedHash;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = UserId;
                con.Open();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    PasswordFeedback = "Your password has been changed.";
                    pnlPasswordStart.Visible = true;
                    pnlPasswordVerify.Visible = false;
                    pnlPasswordNew.Visible = false;
                }
                else
                {
                    PasswordFeedback = "Your password changed elsewhere. Verify your current password again.";
                    pnlPasswordStart.Visible = false;
                    pnlPasswordVerify.Visible = true;
                    pnlPasswordNew.Visible = false;
                }
            }
            ClearPasswordVerification();
        }

        protected void btnCancelPasswordChange_Click(object sender, EventArgs e)
        {
            ClearPasswordVerification();
            pnlPasswordStart.Visible = true;
            pnlPasswordVerify.Visible = false;
            pnlPasswordNew.Visible = false;
            PasswordFeedback = "Password change cancelled.";
        }

        private const string PasswordChangeUserIdKey = "ProfilePasswordChangeUserId";
        private const string PasswordChangeHashKey = "ProfilePasswordChangeHash";
        private const string PasswordChangeExpiryKey = "ProfilePasswordChangeExpiryUtc";

        private void ClearPasswordVerification()
        {
            Session.Remove(PasswordChangeUserIdKey);
            Session.Remove(PasswordChangeHashKey);
            Session.Remove(PasswordChangeExpiryKey);
        }

        // Only the signed-in account can be removed. Database role and password are
        // checked again inside the transaction, rather than trusting posted fields.
        private bool DeleteProfile()
        {
            string password = Request.Form["deletePassword"] ?? "";
            if (Request.Form["confirmDeleteProfile"] != "yes")
            { DeleteFeedback = "Please confirm that you understand before continuing."; return false; }
            if (password.Length == 0 || Encoding.UTF8.GetByteCount(password) > 72)
            { DeleteFeedback = "Enter your current password to confirm."; return false; }

            string oldPhoto = null;
            bool permanentlyDeleted = false;
            using (var connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    string role, hash, status;
                    using (var command = new SqlCommand("SELECT Role,Password,AccountStatus,ProfileImage FROM dbo.Users WITH (UPDLOCK,HOLDLOCK) WHERE UserID=@id", connection, transaction))
                    {
                        command.Parameters.Add("@id", SqlDbType.Int).Value = UserId;
                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read()) { DeleteFeedback = "Your account is no longer available."; return false; }
                            role = Convert.ToString(reader["Role"]);
                            hash = Convert.ToString(reader["Password"]);
                            status = Convert.ToString(reader["AccountStatus"]);
                            oldPhoto = Convert.ToString(reader["ProfileImage"]);
                        }
                    }
                    if (status != "Active" || (role != "Learner" && role != "Instructor"))
                    { DeleteFeedback = "This account cannot be closed here. Please contact another administrator."; return false; }
                    if (string.IsNullOrEmpty(hash) || !PasswordManager.isPasswordValid(password, hash))
                    { DeleteFeedback = "Your current password is incorrect."; return false; }
                    permanentlyDeleted = role == "Learner";
                    using (var command = new SqlCommand(permanentlyDeleted
                        ? "DELETE FROM dbo.Users WHERE UserID=@id AND Role='Learner'"
                        : "UPDATE dbo.Users SET AccountStatus='Inactive',UpdatedAt=GETDATE() WHERE UserID=@id AND Role='Instructor'", connection, transaction))
                    {
                        command.Parameters.Add("@id", SqlDbType.Int).Value = UserId;
                        if (command.ExecuteNonQuery() != 1)
                        { DeleteFeedback = "Your account changed. Reload the page and try again."; return false; }
                    }
                    transaction.Commit();
                }
            }
            if (permanentlyDeleted) DeleteOwnedPhoto(oldPhoto);
            System.Web.Security.FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            Response.Redirect(NavigationHelper.GetUrl("Main"), false);
            Context.ApplicationInstance.CompleteRequest();
            return true;
        }

        private void SetFeedback(string action, string message)
        {
            if (action == "delete") DeleteFeedback = message;
            else if (action == "password") PasswordFeedback = message;
            else if (action == "upload" || action == "remove") PhotoFeedback = message;
            else DetailsFeedback = message;
        }

        private void ReplacePhoto(string path)
        {
            string oldPath;
            using (var con = DatabaseManager.ConnectDB())
            using (var cmd = new SqlCommand("UPDATE dbo.Users SET ProfileImage=@photo, UpdatedAt=GETDATE() OUTPUT deleted.ProfileImage WHERE UserID=@id", con))
            {
                cmd.Parameters.Add("@photo", SqlDbType.NVarChar, 500).Value = (object)path ?? DBNull.Value;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = UserId;
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result == null) throw new InvalidOperationException("The profile account no longer exists.");
                oldPath = Convert.ToString(result);
            }
            DeleteOwnedPhoto(oldPath);
        }

        private void DeleteOwnedPhoto(string path)
        {
            if (string.IsNullOrEmpty(path) || !Regex.IsMatch(path,
                "^" + Regex.Escape(PhotoFolder + UserId + "_") + "[a-f0-9]{32}\\.png$")) return;
            try { File.Delete(Server.MapPath(path)); }
            catch (IOException) { /* A locked obsolete photo can be cleaned up later. */ }
            catch (UnauthorizedAccessException) { /* Keep the successfully saved profile. */ }
        }
    }
}
