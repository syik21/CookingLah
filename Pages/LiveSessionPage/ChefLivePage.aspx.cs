using CookingLah.F_Services;
using Microsoft.AspNet.SignalR;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CookingLah.Pages.LiveSessionPage
{
    public partial class ChefLivePage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int chefId;
            if (!string.Equals(Convert.ToString(Session["Role"]), "Instructor", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(Convert.ToString(Session["UserId"]), out chefId)) return;

            if (IsPostBack) return;

            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT s.LiveSessionID,s.Title,s.Description,s.JitsiRoomName,u.Username,
                       (SELECT COUNT(1) FROM LiveSessionWarnings warning
                        WHERE warning.LiveSessionID=s.LiveSessionID) AS WarningCount
                FROM LiveSessions s
                INNER JOIN Users u ON u.UserID=s.ChefID
                WHERE s.ChefID=@chefId AND s.Status='Live'", connection))
            {
                command.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hfLiveSessionID.Value = Convert.ToString(reader["LiveSessionID"]);
                        hfJitsiRoomName.Value = Convert.ToString(reader["JitsiRoomName"]);
                        hfChefDisplayName.Value = Convert.ToString(reader["Username"]);
                        litLiveTitle.Text = Server.HtmlEncode(Convert.ToString(reader["Title"]));
                        litLiveDescription.Text = Server.HtmlEncode(Convert.ToString(reader["Description"]));
                        lblWarningCount.Text = Convert.ToString(reader["WarningCount"]);
                    }
                    else
                    {
                        hfLiveSessionID.Value = "";
                        hfJitsiRoomName.Value = "";
                        hfChefDisplayName.Value = "";
                    }
                }
            }

            LoadMessages();
        }

        private void LoadMessages()
        {
            int chefId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out chefId)) return;

            hfChefID.Value = chefId.ToString();

            using (SqlConnection conn = DatabaseManager.ConnectDB())
            {
                string query = @"
                SELECT u.Username,u.Role,m.Message,m.CreatedAt
                FROM LiveSessionMessages m
                INNER JOIN Users u ON u.UserID = m.UserID
                INNER JOIN LiveSessions s ON s.LiveSessionID = m.LiveSessionID
                WHERE s.ChefID = @chefId
                  AND s.Status = 'Live'
                ORDER BY m.CreatedAt ASC, m.MessageID ASC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        adapter.Fill(dt);

                        rptMessages.DataSource = dt;
                        rptMessages.DataBind();
                    }
                }
            }
        }


        protected void btnEndLive_Click(object sender, EventArgs e)
        {
            int sessionId;
            int chefId;
            if (!int.TryParse(hfLiveSessionID.Value, out sessionId) ||
                !int.TryParse(Convert.ToString(Session["UserId"]), out chefId))
            {
                return;
            }

            bool endedByChef = false;
            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB())
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (SqlCommand endSession = new SqlCommand(@"
                            UPDATE LiveSessions
                            SET Status='Offline',
                                EndedAt=GETDATE(),
                                UpdatedAt=GETDATE()
                            WHERE LiveSessionID=@sessionId
                              AND ChefID=@chefId
                              AND Status='Live'", connection, transaction))
                        {
                            endSession.Parameters.Add("@sessionId", SqlDbType.Int).Value = sessionId;
                            endSession.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                            endedByChef = endSession.ExecuteNonQuery() == 1;
                        }

                        if (endedByChef)
                        {
                            using (SqlCommand deleteTemporaryData = new SqlCommand(@"
                                DELETE FROM LiveSessionMessages
                                WHERE LiveSessionID=@sessionId;

                                DELETE FROM LiveSessionWarnings
                                WHERE LiveSessionID=@sessionId;", connection, transaction))
                            {
                                deleteTemporaryData.Parameters.Add("@sessionId", SqlDbType.Int).Value = sessionId;
                                deleteTemporaryData.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Ending live session failed: {0}", ex.Number);
                ClientScript.RegisterStartupScript(GetType(), "endLiveError",
                    "alert('Unable to end the live session. Please try again.');", true);
                return;
            }

            if (endedByChef)
            {
                var hubContext =
                    GlobalHost.ConnectionManager.GetHubContext<CookingLah.Hubs.LiveSessionHub>();

                hubContext.Clients.Group(CookingLah.Hubs.LiveSessionHub.GroupPrefix + sessionId)
                    .liveEnded(sessionId);
            }

            Response.Redirect(ResolveUrl("~/Pages/LiveSessionPage/ChefStartLive.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
