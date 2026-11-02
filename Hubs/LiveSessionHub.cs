using CookingLah.F_Services;
using Microsoft.AspNet.SignalR;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace CookingLah.Hubs
{
    public class LiveSessionHub : Hub
    {
        public const string GroupPrefix = "live-chat-session-";
        private static readonly ConcurrentDictionary<string, int> ChefConnections =
            new ConcurrentDictionary<string, int>();
        private static readonly ConcurrentDictionary<string, int> AdminConnections =
            new ConcurrentDictionary<string, int>();

        // Join the chat after checking access to the live session.
        public Task JoinChat(int liveSessionId)
        {
            int userId = GetUserId();
            if (!CanUseLiveSession(userId, liveSessionId))
                throw new HubException("This live session is unavailable.");

            return Groups.Add(Context.ConnectionId, GroupName(liveSessionId)).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    return;
                Clients.Caller.streamPresence(ChefConnections.Values.Contains(liveSessionId));
            });
        }

        // Tell viewers when the session's Chef is connected.
        public void ReportChefPresent(int liveSessionId)
        {
            int userId = GetUserId();
            if (!ChefOwnsLiveSession(userId, liveSessionId))
                throw new HubException("This live session is unavailable.");

            ChefConnections[Context.ConnectionId] = liveSessionId;
            Clients.Group(GroupName(liveSessionId)).streamPresence(true);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            int liveSessionId;
            if (ChefConnections.TryRemove(Context.ConnectionId, out liveSessionId) &&
                !ChefConnections.Values.Contains(liveSessionId))
            {
                Clients.Group(GroupName(liveSessionId)).streamPresence(false);
            }

            if (AdminConnections.TryRemove(Context.ConnectionId, out liveSessionId) &&
                !AdminConnections.Values.Contains(liveSessionId))
            {
                Clients.Group(GroupName(liveSessionId))
                    .systemMessage("🛡️ Admin has left the live session.", "admin");
            }

            return base.OnDisconnected(stopCalled);
        }

        public void ReportAdminJoined(int liveSessionId)
        {
            int adminId = GetAdminId();
            if (!CanUseLiveSession(adminId, liveSessionId))
                throw new HubException("This live session is unavailable.");

            if (AdminConnections.TryAdd(Context.ConnectionId, liveSessionId))
            {
                Clients.Group(GroupName(liveSessionId))
                    .systemMessage("🛡️ Admin has joined the live session.", "admin");
            }
        }

        // Record an admin warning for the live session.
        public void IssueWarning(int liveSessionId, string reason)
        {
            int adminId = GetAdminId();
            reason = ValidateModerationReason(reason);
            int warningNumber;
            int chefId;

            using (SqlConnection connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    using (SqlCommand sessionCommand = new SqlCommand(@"
                        SELECT ChefID
                        FROM LiveSessions WITH (UPDLOCK,HOLDLOCK)
                        WHERE LiveSessionID=@liveSessionId AND Status='Live'", connection, transaction))
                    {
                        sessionCommand.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                        object result = sessionCommand.ExecuteScalar();
                        if (result == null)
                            throw new HubException("This live session is unavailable.");
                        chefId = Convert.ToInt32(result);
                    }

                    using (SqlCommand warningCommand = new SqlCommand(@"
                        SELECT ISNULL(MAX(WarningNumber),0)+1
                        FROM LiveSessionWarnings WITH (UPDLOCK,HOLDLOCK)
                        WHERE LiveSessionID=@liveSessionId", connection, transaction))
                    {
                        warningCommand.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                        warningNumber = Convert.ToInt32(warningCommand.ExecuteScalar());
                    }

                    if (warningNumber > 3)
                        throw new HubException("This session already has three warnings.");

                    using (SqlCommand insertCommand = new SqlCommand(@"
                        INSERT INTO LiveSessionWarnings
                            (LiveSessionID,AdminID,WarningNumber,Reason,CreatedAt)
                        VALUES
                            (@liveSessionId,@adminId,@warningNumber,@reason,GETDATE());", connection, transaction))
                    {
                        insertCommand.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                        insertCommand.Parameters.Add("@adminId", SqlDbType.Int).Value = adminId;
                        insertCommand.Parameters.Add("@warningNumber", SqlDbType.Int).Value = warningNumber;
                        insertCommand.Parameters.Add("@reason", SqlDbType.NVarChar, 500).Value = reason;
                        insertCommand.ExecuteNonQuery();
                    }

                    if (warningNumber == 3)
                    {
                        using (SqlCommand suspendCommand = new SqlCommand(@"
                            UPDATE LiveSessions
                            SET Status='Suspended',AdminCloseReason=@reason,EndedAt=GETDATE(),UpdatedAt=GETDATE()
                            WHERE LiveSessionID=@liveSessionId AND Status='Live';", connection, transaction))
                        {
                            suspendCommand.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                            suspendCommand.Parameters.Add("@reason", SqlDbType.NVarChar, 500).Value = reason;
                            if (suspendCommand.ExecuteNonQuery() != 1)
                                throw new HubException("This live session is unavailable.");
                        }

                        using (SqlCommand penaltyCommand = new SqlCommand(@"
                            INSERT INTO LiveStreamingPenalties
                                (ChefID,LiveSessionID,IssuedByAdminID,PenaltyType,Reason,StartsAt,EndsAt,CreatedAt)
                            VALUES
                                (@chefId,@liveSessionId,@adminId,'Third Warning',@reason,GETDATE(),DATEADD(HOUR,2,GETDATE()),GETDATE());

                            DELETE FROM LiveSessionMessages
                            WHERE LiveSessionID=@liveSessionId;", connection, transaction))
                        {
                            penaltyCommand.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                            penaltyCommand.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                            penaltyCommand.Parameters.Add("@adminId", SqlDbType.Int).Value = adminId;
                            penaltyCommand.Parameters.Add("@reason", SqlDbType.NVarChar, 500).Value = reason;
                            penaltyCommand.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }

            Clients.Group(GroupName(liveSessionId)).adminWarning(reason, warningNumber);
            if (warningNumber == 3)
                Clients.Group(GroupName(liveSessionId)).liveSuspended("Third warning: " + reason);
        }

        public void SuspendLive(int liveSessionId, string reason)
        {
            int adminId = GetAdminId();
            reason = ValidateModerationReason(reason);

            using (SqlConnection connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    int chefId;
                    using (SqlCommand sessionCommand = new SqlCommand(@"
                        SELECT ChefID
                        FROM LiveSessions WITH (UPDLOCK,HOLDLOCK)
                        WHERE LiveSessionID=@liveSessionId AND Status='Live'", connection, transaction))
                    {
                        sessionCommand.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                        object result = sessionCommand.ExecuteScalar();
                        if (result == null)
                            throw new HubException("This live session is unavailable.");
                        chefId = Convert.ToInt32(result);
                    }

                    using (SqlCommand suspendCommand = new SqlCommand(@"
                        UPDATE LiveSessions
                        SET Status='Suspended',AdminCloseReason=@reason,EndedAt=GETDATE(),UpdatedAt=GETDATE()
                        WHERE LiveSessionID=@liveSessionId AND Status='Live';

                        INSERT INTO LiveStreamingPenalties
                            (ChefID,LiveSessionID,IssuedByAdminID,PenaltyType,Reason,StartsAt,EndsAt,CreatedAt)
                        VALUES
                            (@chefId,@liveSessionId,@adminId,'Admin Close',@reason,GETDATE(),DATEADD(HOUR,2,GETDATE()),GETDATE());

                        DELETE FROM LiveSessionMessages WHERE LiveSessionID=@liveSessionId;", connection, transaction))
                    {
                        suspendCommand.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                        suspendCommand.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                        suspendCommand.Parameters.Add("@adminId", SqlDbType.Int).Value = adminId;
                        suspendCommand.Parameters.Add("@reason", SqlDbType.NVarChar, 500).Value = reason;
                        suspendCommand.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }

            Clients.Group(GroupName(liveSessionId)).liveSuspended(reason);
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
            string role;
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
                    SELECT Username,Role FROM Users WHERE UserID=@userId;
                END", connection))
            {
                command.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@message", SqlDbType.NVarChar, 500).Value = message;
                command.Parameters.Add("@createdAt", SqlDbType.DateTime).Value = createdAt;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new HubException("This live session is unavailable.");
                    username = Convert.ToString(reader["Username"]);
                    role = Convert.ToString(reader["Role"]);
                }
            }

            if (string.IsNullOrEmpty(username))
                throw new HubException("This live session is unavailable.");

            Clients.Group(GroupName(liveSessionId))
                .receiveChatMessage(username, message, createdAt, role);
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
                  AND Role IN ('Learner','Instructor','Admin')", connection))
            {
                command.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = Context.User.Identity.Name;
                connection.Open();
                object userId = command.ExecuteScalar();
                if (userId == null)
                    throw new HubException("Please sign in again.");
                return Convert.ToInt32(userId);
            }
        }

        private int GetAdminId()
        {
            int userId = GetUserId();
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT COUNT(1)
                FROM Users
                WHERE UserID=@userId AND Role='Admin' AND AccountStatus='Active'", connection))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                connection.Open();
                if (Convert.ToInt32(command.ExecuteScalar()) != 1)
                    throw new HubException("Administrator access is required.");
                return userId;
            }
        }

        private static string ValidateModerationReason(string reason)
        {
            reason = (reason ?? "").Trim();
            if (reason.Length == 0 || reason.Length > 500)
                throw new HubException("Enter a reason between 1 and 500 characters.");
            return reason;
        }

        private static bool CanUseLiveSession(int userId, int liveSessionId)
        {
            if (liveSessionId <= 0)
                return false;
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

        private static bool ChefOwnsLiveSession(int userId, int liveSessionId)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT COUNT(1)
                FROM LiveSessions
                WHERE LiveSessionID=@liveSessionId
                  AND ChefID=@userId
                  AND Status='Live'", connection))
            {
                command.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = liveSessionId;
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
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
