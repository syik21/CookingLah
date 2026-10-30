using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CookingLah.Pages.LiveSessionPage
{
    public partial class SearchLiveStream : System.Web.UI.Page
    {
        protected bool IsLearner
        {
            get { return string.Equals(Convert.ToString(Session["Role"]), "Learner", StringComparison.OrdinalIgnoreCase); }
        }

        protected void Page_PreInit(object sender, EventArgs e)
        {
            string role = Convert.ToString(Session["Role"]);
            if (string.Equals(role, "Learner", StringComparison.OrdinalIgnoreCase))
            {
                MasterPageFile = "~/Pages/Learner/Learner.Master";
                return;
            }

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                MasterPageFile = "~/Pages/Admin/Admin.Master";
                return;
            }

            RedirectToLogin();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!ValidatePageAccess()) return;

            litSubtitle.Text = IsLearner
                ? "<p class=\"live-search__subtitle\">Watch Chefs cook, interact and learn in real time.</p>"
                : "<p class=\"live-search__subtitle\">Browse and monitor currently active livestreams.</p>";
            if (!IsPostBack)
            {
                LoadLiveStreams("");
            }
        }

        private bool ValidatePageAccess()
        {
            int userId;
            string role = Convert.ToString(Session["Role"]);
            bool validRole = string.Equals(role, "Learner", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (validRole && int.TryParse(Convert.ToString(Session["UserId"]), out userId) && userId > 0)
            {
                return true;
            }

            RedirectToLogin();
            return false;
        }

        // Load currently active livestreams.
        private void LoadLiveStreams(string searchText)
        {
            searchText = (searchText ?? "").Trim();
            pnlMessage.Visible = false;

            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB())
                using (SqlCommand command = new SqlCommand(@"
                    SELECT ls.LiveSessionID,ls.Title,ls.ThumbnailPath,ls.StartedAt,
                           COALESCE(NULLIF(u.FullName,''),u.Username) AS ChefName,
                           u.ProfileImage
                    FROM LiveSessions ls
                    INNER JOIN Users u ON u.UserID=ls.ChefID
                    WHERE ls.Status='Live'
                      AND (@searchText=''
                           OR ls.Title LIKE @searchPattern
                           OR u.Username LIKE @searchPattern
                           OR u.FullName LIKE @searchPattern)
                    ORDER BY ls.StartedAt DESC,ls.LiveSessionID DESC", connection))
                {
                    command.Parameters.Add("@searchText", SqlDbType.NVarChar, 150).Value = searchText;
                    command.Parameters.Add("@searchPattern", SqlDbType.NVarChar, 152).Value = "%" + searchText + "%";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable streams = new DataTable();
                        adapter.Fill(streams);
                        BindLiveStreams(streams, searchText);
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Loading live stream search failed: {0}", ex.Number);
                rptLiveStreams.DataSource = null;
                rptLiveStreams.DataBind();
                pnlNoLiveStreams.Visible = false;
                pnlNoSearchResults.Visible = false;
                pnlMessage.Visible = true;
                lblMessage.Text = "Unable to load livestreams right now. Please try again.";
            }
        }

        private void BindLiveStreams(DataTable streams, string searchText)
        {
            rptLiveStreams.DataSource = streams;
            rptLiveStreams.DataBind();

            bool hasSearch = searchText.Length > 0;
            bool hasResults = streams.Rows.Count > 0;
            pnlNoLiveStreams.Visible = !hasSearch && !hasResults;
            pnlNoSearchResults.Visible = hasSearch && !hasResults;
            litSearchTerm.Text = Server.HtmlEncode(searchText);

            lblResultSummary.Text = hasSearch
                ? string.Format("{0} {1} for “{2}”", streams.Rows.Count, streams.Rows.Count == 1 ? "result" : "results", Server.HtmlEncode(searchText))
                : string.Format("{0} live {1}", streams.Rows.Count, streams.Rows.Count == 1 ? "stream" : "streams");
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadLiveStreams(txtLiveSearch.Text);
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtLiveSearch.Text = "";
            LoadLiveStreams("");
        }

        // Send Learners and Admins to their own live-view page.
        protected string GetLiveStreamUrl(int liveSessionId)
        {
            string page = IsLearner ? "LearnerWatchLivePage.aspx" : "AdminLivePage.aspx";
            return ResolveUrl("~/Pages/LiveSessionPage/" + page + "?liveSessionId=" + liveSessionId);
        }

        protected string GetActionText()
        {
            return IsLearner ? "Watch Live →" : "Monitor Live →";
        }

        protected bool HasImage(object pathValue)
        {
            return SafeLocalImage(Convert.ToString(pathValue), "").Length > 0;
        }

        protected string GetThumbnailUrl(object pathValue)
        {
            return SafeLocalImage(Convert.ToString(pathValue), "");
        }

        protected string GetProfileImageUrl(object pathValue)
        {
            return SafeLocalImage(Convert.ToString(pathValue), "~/F_Assets/mascot.png");
        }

        private string SafeLocalImage(string path, string fallback)
        {
            path = (path ?? "").Trim();
            if (path.Length == 0 || path.StartsWith("//") || path.Contains("\\")) return fallback;

            Uri absoluteUrl;
            if (Uri.TryCreate(path, UriKind.Absolute, out absoluteUrl)) return fallback;
            return path.StartsWith("~/", StringComparison.Ordinal) ? path : fallback;
        }

        protected string FormatStartedAt(object value)
        {
            if (value == null || value == DBNull.Value) return "Live now";
            return "Started " + Convert.ToDateTime(value).ToString("h:mm tt");
        }

        private void RedirectToLogin()
        {
            Response.Redirect(NavigationHelper.GetUrl("Main"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
