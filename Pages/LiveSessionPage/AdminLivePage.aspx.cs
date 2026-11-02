using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CookingLah.Pages.LiveSessionPage
{
    public partial class AdminLivePage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int adminId;
            if (!string.Equals(Convert.ToString(Session["Role"]), "Admin", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(Convert.ToString(Session["UserId"]), out adminId) || adminId <= 0)
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            int liveSessionId;
            if (!int.TryParse(Request.QueryString["liveSessionId"], out liveSessionId) || liveSessionId <= 0 ||
                !LoadLiveSession(liveSessionId, adminId))
            {
                Response.Redirect(ResolveUrl("~/Pages/LiveSessionPage/SearchLiveStream.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadMessages(liveSessionId);
            }
        }

        private bool LoadLiveSession(int liveSessionId, int adminId)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT s.LiveSessionID,s.Title,s.Description,s.JitsiRoomName,
                       chef.Username AS ChefName,chef.FullName,chef.ProfileImage,chef.Country,
                       profession.Profession,admin.Username AS AdminName,
                       (SELECT COUNT(1) FROM LiveSessionWarnings warning
                        WHERE warning.LiveSessionID=s.LiveSessionID) AS WarningCount
                FROM LiveSessions s
                INNER JOIN Users chef ON chef.UserID=s.ChefID
                INNER JOIN Users admin ON admin.UserID=@adminId
                LEFT JOIN ChefProfession profession ON profession.ChefID=s.ChefID
                WHERE s.LiveSessionID=@liveSessionId
                  AND s.Status='Live'
                  AND admin.Role='Admin'
                  AND admin.AccountStatus='Active'", connection))
            {
                command.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                command.Parameters.Add("@adminId", SqlDbType.Int).Value = adminId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return false;

                    hfLiveSessionID.Value = Convert.ToString(reader["LiveSessionID"]);
                    hfJitsiRoomName.Value = Convert.ToString(reader["JitsiRoomName"]);
                    hfChefDisplayName.Value = Convert.ToString(reader["ChefName"]);
                    hfAdminDisplayName.Value = Convert.ToString(reader["AdminName"]);
                    hfJitsiDomain.Value = ConfigurationManager.AppSettings["JitsiDomain"];
                    litLiveTitle.Text = Server.HtmlEncode(Convert.ToString(reader["Title"]));
                    litLiveDescription.Text = Server.HtmlEncode(Convert.ToString(reader["Description"]));
                    string fullName = Convert.ToString(reader["FullName"]).Trim();
                    string profession = Convert.ToString(reader["Profession"]).Trim();
                    string country = Convert.ToString(reader["Country"]).Trim();
                    litChefName.Text = Server.HtmlEncode(fullName.Length == 0 ? Convert.ToString(reader["ChefName"]) : fullName);
                    litChefProfession.Text = Server.HtmlEncode(profession.Length == 0 ? "Cooking Instructor" : profession);
                    litChefCountry.Text = Server.HtmlEncode(country.Length == 0 ? "Country not provided" : country);
                    imgChefProfile.ImageUrl = SafeProfileImageUrl(Convert.ToString(reader["ProfileImage"]));
                    lblWarningCount.Text = Convert.ToString(reader["WarningCount"]);
                    return true;
                }
            }
        }

        private string SafeProfileImageUrl(string path)
        {
            path = (path ?? "").Trim();
            if (path.Length == 0 || path.StartsWith("//") || path.Contains("\\"))
                return "~/F_Assets/mascot.png";

            Uri absoluteUrl;
            if (Uri.TryCreate(path, UriKind.Absolute, out absoluteUrl) &&
                absoluteUrl.Scheme != Uri.UriSchemeHttp &&
                absoluteUrl.Scheme != Uri.UriSchemeHttps)
                return "~/F_Assets/mascot.png";

            return path;
        }

        private void LoadMessages(int liveSessionId)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT u.Username,u.Role,m.Message,m.CreatedAt
                FROM LiveSessionMessages m
                INNER JOIN Users u ON u.UserID=m.UserID
                WHERE m.LiveSessionID=@liveSessionId
                ORDER BY m.CreatedAt,m.MessageID", connection))
            {
                command.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable messages = new DataTable();
                    adapter.Fill(messages);
                    rptMessages.DataSource = messages;
                    rptMessages.DataBind();
                }
            }
        }

        protected void btnExitLive_Click(object sender, EventArgs e)
        {
            Response.Redirect(ResolveUrl("~/Pages/LiveSessionPage/SearchLiveStream.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
