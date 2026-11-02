using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CookingLah.Pages.LiveSessionPage
{
    public partial class LearnerWatchLivePage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int liveSessionId;
            if (!int.TryParse(Request.QueryString["liveSessionId"], out liveSessionId) || liveSessionId <= 0)
            {
                hfLiveSessionID.Value = "";
                return;
            }

            hfLiveSessionID.Value = liveSessionId.ToString();
            if (!IsPostBack)
            {
                LoadSessionDetails(liveSessionId);
                LoadMessages(liveSessionId);
            }
        }

        private void LoadSessionDetails(int liveSessionId)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT s.Title,s.Description,s.JitsiRoomName,u.Username,u.FullName,
                       u.ProfileImage,u.Country,p.Profession
                FROM LiveSessions s
                INNER JOIN Users u ON u.UserID=s.ChefID
                LEFT JOIN ChefProfession p ON p.ChefID=s.ChefID
                WHERE s.LiveSessionID=@liveSessionId AND s.Status='Live'", connection))
            {
                command.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return;
                    litLiveTitle.Text = Server.HtmlEncode(Convert.ToString(reader["Title"]));
                    litLiveDescription.Text = Server.HtmlEncode(Convert.ToString(reader["Description"]));
                    hfJitsiRoomName.Value = Convert.ToString(reader["JitsiRoomName"]);
                    hfChefDisplayName.Value = Convert.ToString(reader["Username"]);
                    string fullName = Convert.ToString(reader["FullName"]).Trim();
                    string profession = Convert.ToString(reader["Profession"]).Trim();
                    string country = Convert.ToString(reader["Country"]).Trim();
                    litChefName.Text = Server.HtmlEncode(fullName.Length == 0 ? Convert.ToString(reader["Username"]) : fullName);
                    litChefProfession.Text = Server.HtmlEncode(profession.Length == 0 ? "Cooking Instructor" : profession);
                    litChefCountry.Text = Server.HtmlEncode(country.Length == 0 ? "Country not provided" : country);
                    imgChefProfile.ImageUrl = SafeProfileImageUrl(Convert.ToString(reader["ProfileImage"]));
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
                INNER JOIN LiveSessions s ON s.LiveSessionID=m.LiveSessionID
                WHERE m.LiveSessionID=@liveSessionId AND s.Status='Live'
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

        protected void btnEndLive_Click(object sender, EventArgs e)
        {
            Response.Redirect(ResolveUrl("~/Pages/LiveSessionPage/SearchLiveStream.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
