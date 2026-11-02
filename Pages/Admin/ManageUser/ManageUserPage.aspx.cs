using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace CookingLah.Pages.Admin.ManageUser
{
    public partial class ManageUserPage : System.Web.UI.Page
    {
        protected string Feedback = "", ErrorMessage = "", Mode = "", TargetRole = "", TargetName = "";
        protected int TargetId, Total, PageNumber = 1;
        protected const int PageSize = 12;
        protected string Search = "", RoleFilter = "", StatusFilter = "";
        protected string NameValue = "", UsernameValue = "", EmailValue = "", CountryValue = "", ProfessionValue = "", StatusValue = "Active", RoleValue = "Learner";
        protected string CsrfToken
        {
            get
            {
                return Convert.ToString(Session["ManageUsersCsrf"]);
            }
        }

        protected int ActorId
        {
            get
            {
                return Convert.ToInt32(Session["UserId"]);
            }
        }

        protected static readonly string[] Roles = { "Learner", "Instructor", "Admin" };
        protected static readonly string[] Statuses = { "Active", "Inactive", "Suspended" };
        protected static readonly string[] Countries = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(c => new RegionInfo(c.Name).EnglishName).Distinct().OrderBy(c => c).ToArray();
        protected string H(object value)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(value));
        }

        protected string RoleLabel(string role)
        {
            return role;
        }

        // Return the CSS class for each account status.
        protected string GetStatusCss(object status)
        {
            switch (Convert.ToString(status))
            {
                case "Active":
                    return "mu-status mu-active";
                case "Inactive":
                    return "mu-status mu-inactive";
                default:
                    return "mu-status mu-suspended";
            }
        }

        // Choose the action label based on the user role.
        protected string GetRemoveLabel(object role)
        {
            return Convert.ToString(role) == "Learner" ? "Delete" : "Deactivate";
        }

        // Prevent admins from removing their own account.
        protected bool CanRemoveUser(object userId, object role, object status)
        {
            return Convert.ToInt32(userId) != ActorId &&
                (Convert.ToString(role) == "Learner" || Convert.ToString(status) != "Inactive");
        }

        // Build a URL that keeps the current filters and page.
        protected string PageUrl(int page)
        {
            return "ManageUserPage.aspx?q=" + HttpUtility.UrlEncode(Search)
                + "&role=" + HttpUtility.UrlEncode(RoleFilter)
                + "&status=" + HttpUtility.UrlEncode(StatusFilter)
                + "&page=" + page;
        }

        // Initialize the CSRF token for this page.
        protected override void OnInit(EventArgs e)
        {
            if (Session["ManageUsersCsrf"] == null)
            {
                Session["ManageUsersCsrf"] = Guid.NewGuid().ToString("N");
            }
            ViewStateUserKey = CsrfToken;
            base.OnInit(e);
        }

        // Display any validation errors before rendering.
        protected override void OnPreRender(EventArgs e)
        {
            lblError.Text = H(ErrorMessage);
            if (ErrorMessage == "")
            {
                lblError.Attributes["hidden"] = "hidden";
            }
            else
            {
                lblError.Attributes.Remove("hidden");
            }
            base.OnPreRender(e);
        }

        // Convert database errors into readable messages.
        private void ShowDatabaseError(SqlException ex)
        {
            System.Diagnostics.Trace.TraceError("Manage users: {0}", ex);
            ErrorMessage = ex.Number == 2601 || ex.Number == 2627 ? "That username or email is already in use. Choose another."
                : ex.Number == 547 ? "This account has linked records that prevent this change. No changes were saved."
                : "We could not complete the request. Please try again.";
        }

        // Confirm that the current user is an active admin.
        private bool IsAdmin(SqlConnection con, SqlTransaction tx)
        {
            int id;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out id))
            {
                return false;
            }
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users WHERE UserID=@id AND Role='Admin' AND AccountStatus='Active'", con, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                return (int)cmd.ExecuteScalar() == 1;
            }
        }

        // Load filters, handle requests, and display users.
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Search = (Request.QueryString["q"] ?? "").Trim();
            if (Search.Length > 100)
            {
                Search = Search.Substring(0, 100);
            }
            RoleFilter = Roles.Contains(Request.QueryString["role"]) ? Request.QueryString["role"] : "";
            StatusFilter = Statuses.Contains(Request.QueryString["status"]) ? Request.QueryString["status"] : "";
            int.TryParse(Request.QueryString["page"], out PageNumber);
            PageNumber = Math.Max(1, PageNumber);
            try
            {
                using (var con = DatabaseManager.ConnectDB())
                {
                    con.Open();
                    if (!IsAdmin(con, null))
                    {
                        Response.Redirect(ResolveUrl("~/Main.aspx"));
                        return;
                    }
                    if (Request.HttpMethod == "POST" && Request.Form["userAction"] != null)
                    {
                        // Keep the directory loaded behind the popup when a save fails.
                        try
                        {
                            HandleAction(con);
                        }
                        catch (SqlException ex)
                        {
                            ShowDatabaseError(ex);
                        }
                    }
                    if (Request.HttpMethod != "POST")
                    {
                        Feedback = Convert.ToString(Session["ManageUsersFeedback"]);
                        Session.Remove("ManageUsersFeedback");
                        Mode = Request.QueryString["mode"] ?? "";
                        if (Mode != "create" && Mode != "edit" && Mode != "remove")
                            Mode = "";
                        int.TryParse(Request.QueryString["id"], out TargetId);
                        if (Mode == "edit" || Mode == "remove")
                            LoadTarget(con);
                    }
                    LoadUsers(con);
                }
            }
            catch (SqlException ex)
            {
                ShowDatabaseError(ex);
            }
        }

        // Load the selected user for editing or removal.
        private void LoadTarget(SqlConnection con)
        {
            using (var cmd = new SqlCommand("SELECT u.*, p.Profession FROM dbo.Users u LEFT JOIN dbo.ChefProfession p ON p.ChefID=u.UserID WHERE u.UserID=@id", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = TargetId;
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        Mode = "";
                        ErrorMessage = "This user no longer exists.";
                        return;
                    }
                    TargetRole = RoleValue = Convert.ToString(r["Role"]);
                    TargetName = NameValue = Convert.ToString(r["FullName"]);
                    UsernameValue = Convert.ToString(r["Username"]);
                    EmailValue = Convert.ToString(r["Email"]);
                    CountryValue = Convert.ToString(r["Country"]);
                    ProfessionValue = Convert.ToString(r["Profession"]);
                    StatusValue = Convert.ToString(r["AccountStatus"]);
                }
            }
            if (Mode == "remove" && TargetId == ActorId)
            {
                Mode = "";
                ErrorMessage = "You cannot remove your own admin account.";
            }
        }

        // Load the filtered user list with pagination.
        private void LoadUsers(SqlConnection con)
        {
            const string where = " WHERE (@q='' OR CHARINDEX(@q,Username)>0 OR CHARINDEX(@q,Email)>0) AND (@role='' OR Role=@role) AND (@status='' OR AccountStatus=@status)";
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users" + where, con))
            {
                AddFilters(cmd);
                Total = (int)cmd.ExecuteScalar();
                PageNumber = Math.Min(PageNumber, Math.Max(1, (Total + PageSize - 1) / PageSize));
                cmd.CommandText = "SELECT UserID,FullName,Username,Email,Role,AccountStatus FROM dbo.Users" + where + " ORDER BY CreatedAt DESC,UserID DESC OFFSET @offset ROWS FETCH NEXT 12 ROWS ONLY";
                cmd.Parameters.Add("@offset", SqlDbType.Int).Value = (PageNumber - 1) * PageSize;
                // Load the current page, then repeat ItemTemplate once for each user.
                var users = new DataTable();
                using (var reader = cmd.ExecuteReader())
                {
                    users.Load(reader);
                }
                rptUsers.DataSource = users;
                rptUsers.DataBind();
            }
        }

        // Add the search and filter parameters to the query.
        private void AddFilters(SqlCommand cmd)
        {
            cmd.Parameters.Add("@q", SqlDbType.NVarChar, 100).Value = Search;
            cmd.Parameters.Add("@role", SqlDbType.NVarChar, 20).Value = RoleFilter;
            cmd.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = StatusFilter;
        }

        // Validate and process create, edit, or removal requests.
        private void HandleAction(SqlConnection con)
        {
            string action = Request.Form["userAction"];
            Mode = action == "create" ? "create" : action == "save" ? "edit" : action == "remove" ? "remove" : "";
            if (Mode == "")
            {
                ErrorMessage = "Unknown action. Reload the page.";
                return;
            }
            int.TryParse(Request.Form["userId"], out TargetId);
            if (Mode != "create")
            {
                LoadTarget(con);
                if (Mode == "")
                {
                    return;
                }
            }
            if (action != "remove")
            {
                NameValue = (Request.Form["fullName"] ?? "").Trim();
                UsernameValue = (Request.Form["username"] ?? "").Trim();
                EmailValue = (Request.Form["email"] ?? "").Trim();
                CountryValue = Request.Form["country"] ?? "";
                ProfessionValue = (Request.Form["profession"] ?? "").Trim();
                StatusValue = Request.Form["accountStatus"] ?? "";
                if (action == "create")
                {
                    RoleValue = Request.Form["newRole"] ?? "";
                }
                if (NameValue.Length == 0 || NameValue.Length > 50 || UsernameValue.Length == 0 || UsernameValue.Length > 50 ||
                    EmailValue.Length > 100 || !UserManager.IsEmail(EmailValue) || !Countries.Contains(CountryValue) ||
                    !Roles.Contains(RoleValue) || !Statuses.Contains(StatusValue) ||
                    (RoleValue == "Instructor" && (ProfessionValue.Length == 0 || ProfessionValue.Length > 100)))
                {
                    ErrorMessage = "Check the required fields: name, username, valid email, country, role, status and instructor profession.";
                    return;
                }
                string password = Request.Form["password"] ?? "";
                if (action == "create" && (!PasswordManager.MeetsPasswordPolicy(password)))
                {
                    ErrorMessage = "Use at least 8 characters including a letter, number and symbol, and no more than 72 UTF-8 bytes.";
                    return;
                }
                if (action == "create" && password != Request.Form["confirmPassword"])
                {
                    ErrorMessage = "The passwords do not match.";
                    return;
                }
            }
            if (Request.Form["userCsrf"] != CsrfToken)
            {
                ErrorMessage = "Your session changed. Please try again.";
                return;
            }
            using (var tx = con.BeginTransaction(IsolationLevel.Serializable))
            {
                if (!IsAdmin(con, tx))
                {
                    ErrorMessage = "An active admin account is required.";
                    return;
                }
                if (action != "create")
                {
                    using (var check = new SqlCommand("SELECT Role FROM dbo.Users WITH (UPDLOCK) WHERE UserID=@id", con, tx))
                    {
                        check.Parameters.Add("@id", SqlDbType.Int).Value = TargetId;
                        string currentRole = Convert.ToString(check.ExecuteScalar());
                        if (currentRole == "" || currentRole != TargetRole)
                        {
                            ErrorMessage = "This account changed. Reload the page before continuing.";
                            return;
                        }
                    }
                    if (TargetId == ActorId && (action == "remove" || StatusValue != "Active"))
                    {
                        ErrorMessage = "Keep your own admin account active. Another admin can manage its status.";
                        return;
                    }
                }
                if (action == "remove")
                {
                    if (TargetRole == "Learner")
                    {
                        using (var cmd = new SqlCommand("DELETE FROM dbo.PasswordResetTokens WHERE UserID=@id", con, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = TargetId;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    using (var cmd = new SqlCommand(TargetRole == "Learner"
                        ? "DELETE FROM dbo.Users WHERE UserID=@id AND Role='Learner'"
                        : "UPDATE dbo.Users SET AccountStatus='Inactive',UpdatedAt=GETDATE() WHERE UserID=@id AND Role IN ('Instructor','Admin')", con, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = TargetId;
                        if (cmd.ExecuteNonQuery() != 1)
                        {
                            ErrorMessage = "The account could not be removed. Reload the page.";
                            return;
                        }
                    }
                }
                else
                {
                    // Exclude the edited user so an unchanged username/email remains valid.
                    using (var duplicate = new SqlCommand(
"SELECT TOP (1) CASE WHEN Username=@username THEN 'username' ELSE 'email' END FROM dbo.Users WHERE UserID<>@id AND (Username=@username OR Email=@email) ORDER BY CASE WHEN Username=@username THEN 0 ELSE 1 END",
con,
tx))
                    {
                        duplicate.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = UsernameValue;
                        duplicate.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = EmailValue;
                        duplicate.Parameters.Add("@id", SqlDbType.Int).Value = action == "create" ? 0 : TargetId;
                        string field = Convert.ToString(duplicate.ExecuteScalar());
                        if (field != "")
                        {
                            ErrorMessage = field == "username" ? "That username is already in use. Choose another username."
                                : "That email is already in use. Choose another email address.";
                            return;
                        }
                    }
                    using (var cmd = new SqlCommand(action == "create"
                        ? "INSERT INTO dbo.Users (FullName,Username,Email,Country,Role,AccountStatus,Password) VALUES (@name,@username,@email,@country,@role,@status,@password); SELECT CAST(SCOPE_IDENTITY() AS INT);"
                        : "UPDATE dbo.Users SET FullName=@name,Username=@username,Email=@email,Country=@country,AccountStatus=@status,UpdatedAt=GETDATE() WHERE UserID=@id", con, tx))
                    {
                        cmd.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = NameValue;
                        cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = UsernameValue;
                        cmd.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = EmailValue;
                        cmd.Parameters.Add("@country", SqlDbType.NVarChar, 100).Value = CountryValue;
                        cmd.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = StatusValue;
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = TargetId;
                        if (action == "create")
                        {
                            cmd.Parameters.Add("@role", SqlDbType.NVarChar, 20).Value = RoleValue;
                            cmd.Parameters.Add("@password", SqlDbType.NVarChar, 255).Value = PasswordManager.HashPassword(Request.Form["password"]);
                            TargetId = (int)cmd.ExecuteScalar();
                        }
                        else if (cmd.ExecuteNonQuery() != 1)
                        {
                            ErrorMessage = "This user no longer exists.";
                            return;
                        }
                    }
                    if (RoleValue == "Instructor")
                    {
                        using (var cmd = new SqlCommand(
"UPDATE dbo.ChefProfession SET Profession=@profession WHERE ChefID=@id; IF @@ROWCOUNT=0 INSERT INTO dbo.ChefProfession (ChefID,Profession) VALUES (@id,@profession);",
con,
tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = TargetId;
                            cmd.Parameters.Add("@profession", SqlDbType.NVarChar, 100).Value = ProfessionValue;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                tx.Commit();
            }
            if (action == "save" && TargetId == ActorId)
            {
                Session["Username"] = UsernameValue;
                Session["Email"] = EmailValue;
            }
            Session["ManageUsersFeedback"] = action == "create" ? "User created successfully." : action == "save" ? "Profile updated successfully."
                : TargetRole == "Learner" ? "Learner and their learning records permanently deleted." : "Account set to Inactive. Their records have been kept.";
            Response.Redirect(PageUrl(PageNumber));
        }
    }
}
