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
                SELECT Title,Description
                FROM LiveSessions
                WHERE LiveSessionID=@liveSessionId AND Status='Live'", connection))
            {
                command.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return;
                    litLiveTitle.Text = Server.HtmlEncode(Convert.ToString(reader["Title"]));
                    litLiveDescription.Text = Server.HtmlEncode(Convert.ToString(reader["Description"]));
                }
            }
        }

        private void LoadMessages(int liveSessionId)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT u.Username,m.Message,m.CreatedAt
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

        }
    }
}
