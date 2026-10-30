using CookingLah.F_Services;
using Microsoft.AspNet.SignalR;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CookingLah.Hubs
{
    public class LiveSessionHub : Hub
    {
        public const string GroupPrefix = "live-chat-session-";

        public Task JoinChat(int liveSessionId)
        {
            int userId = GetUserId();
            if (!CanUseLiveSession(userId, liveSessionId))
                throw new HubException("This live session is unavailable.");

            return Groups.Add(Context.ConnectionId, GroupName(liveSessionId));
        }

        public Task LeaveChat(int liveSessionId)
        {
            return Groups.Remove(Context.ConnectionId, GroupName(liveSessionId));
        }

        public void SendChatMessage(int liveSessionId, string message)
        {
            int userId = GetUserId();
            message = (message ?? "").Trim();
            if (message.Length == 0 || message.Length > 500)
                throw new HubException("Messages must contain 1 to 500 characters.");

            string username;
            DateTime createdAt = DateTime.Now;

            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                IF EXISTS (
                    SELECT 1
                    FROM Users u
                    CROSS JOIN LiveSessions s
                    WHERE u.UserID=@userId
                      AND u.AccountStatus='Active'
                      AND s.LiveSessionID=@liveSessionId
                      AND s.Status='Live')
                BEGIN
                    INSERT INTO LiveSessionMessages (LiveSessionID,UserID,Message,CreatedAt)
                    VALUES (@liveSessionId,@userId,@message,@createdAt);
                    SELECT Username FROM Users WHERE UserID=@userId;
                END", connection))
            {
                command.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@message", SqlDbType.NVarChar, 500).Value = message;
                command.Parameters.Add("@createdAt", SqlDbType.DateTime).Value = createdAt;
                connection.Open();
                username = Convert.ToString(command.ExecuteScalar());
            }

            if (string.IsNullOrEmpty(username))
                throw new HubException("This live session is unavailable.");

            Clients.Group(GroupName(liveSessionId))
                .receiveChatMessage(username, message, createdAt);
        }

        private int GetUserId()
        {
            if (Context.User == null || Context.User.Identity == null || !Context.User.Identity.IsAuthenticated)
                throw new HubException("Please sign in again.");

            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT UserID
                FROM Users
                WHERE Email=@email
                  AND AccountStatus='Active'
                  AND Role IN ('Learner','Instructor')", connection))
            {
                command.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = Context.User.Identity.Name;
                connection.Open();
                object userId = command.ExecuteScalar();
                if (userId == null) throw new HubException("Please sign in again.");
                return Convert.ToInt32(userId);
            }
        }

        private static bool CanUseLiveSession(int userId, int liveSessionId)
        {
            if (liveSessionId <= 0) return false;
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT COUNT(*)
                FROM Users u
                CROSS JOIN LiveSessions s
                WHERE u.UserID=@userId
                  AND u.AccountStatus='Active'
                  AND s.LiveSessionID=@liveSessionId
                  AND s.Status='Live'", connection))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        private static string GroupName(int liveSessionId)
        {
            return GroupPrefix + liveSessionId;
        }
    }
}
