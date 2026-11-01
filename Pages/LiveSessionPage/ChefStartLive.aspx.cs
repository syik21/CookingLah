using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CookingLah.Pages.LiveSessionPage
{
    public partial class ChefStartLive : System.Web.UI.Page
    {
        private const int MaximumThumbnailBytes = 3 * 1024 * 1024;
        private const string ThumbnailFolder = "~/F_Assets/LiveSessionThumbnails/";
        private int currentChefId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!TryGetCurrentChefId(out currentChefId))
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (IsPostBack)
            {
                return;
            }

            try
            {
                EnsureChefLiveSessionExists(currentChefId);
                RefreshSuspensionStatus(currentChefId);
                LoadChefProfile(currentChefId);
                LoadLiveSessionState(currentChefId);
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Loading Chef livestream failed: {0}", ex.Number);
                ShowMessage("Unable to load your livestream right now. Please try again.", "error");
                pnlGoLiveForm.Visible = false;
            }
        }

        private bool TryGetCurrentChefId(out int chefId)
        {
            chefId = 0;
            return string.Equals(Convert.ToString(Session["Role"]), "Instructor", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(Convert.ToString(Session["UserId"]), out chefId)
                && chefId > 0;
        }

        // Create the persistent Offline row the first time a Chef uses livestreaming.
        private void EnsureChefLiveSessionExists(int chefId)
        {
            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB())
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                    using (SqlCommand command = new SqlCommand(@"
                        IF NOT EXISTS (SELECT 1 FROM LiveSessions WITH (UPDLOCK, HOLDLOCK) WHERE ChefID=@chefId)
                        BEGIN
                            INSERT INTO LiveSessions
                                (ChefID,Title,Description,ThumbnailPath,JitsiRoomName,StartedAt,EndedAt,Status,AdminCloseReason,CreatedAt,UpdatedAt)
                            VALUES
                                (@chefId,'','',NULL,'',NULL,NULL,'Offline',NULL,GETDATE(),GETDATE())
                        END", connection, transaction))
                    {
                        command.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number != 2601 && ex.Number != 2627)
                {
                    throw;
                }
            }
        }

        // Keep the livestream status consistent with active penalties.
        private void RefreshSuspensionStatus(int chefId)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                using (SqlCommand command = new SqlCommand(@"
                    UPDATE LiveSessions
                    SET Status='Suspended',UpdatedAt=GETDATE()
                    WHERE ChefID=@chefId
                      AND Status='Offline'
                      AND EXISTS (
                          SELECT 1
                          FROM LiveStreamingPenalties
                          WHERE ChefID=@chefId AND EndsAt>GETDATE()
                      );

                    UPDATE LiveSessions
                    SET Status='Offline',UpdatedAt=GETDATE()
                    OUTPUT INSERTED.LiveSessionID
                    WHERE ChefID=@chefId
                      AND Status='Suspended'
                      AND NOT EXISTS (
                          SELECT 1
                          FROM LiveStreamingPenalties
                          WHERE ChefID=@chefId AND EndsAt>GETDATE()
                      )", connection, transaction))
                {
                    command.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        using (SqlCommand deleteWarnings = new SqlCommand(@"
                            DELETE FROM LiveSessionWarnings
                            WHERE LiveSessionID=@liveSessionId", connection, transaction))
                        {
                            deleteWarnings.Parameters.Add("@liveSessionId", SqlDbType.Int).Value = Convert.ToInt32(result);
                            deleteWarnings.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        private void LoadChefProfile(int chefId)
        {
            string username;
            string profileImage;
            if (!DatabaseManager.TryLoadMasterProfile(chefId, out username, out profileImage))
            {
                username = Convert.ToString(Session["Username"]);
                profileImage = "~/F_Assets/mascot.png";
            }

            lblChefName.Text = Server.HtmlEncode(username);
            imgChefAvatar.ImageUrl = profileImage;
        }

        // Load the Chef's single livestream state and render the matching view.
        private void LoadLiveSessionState(int chefId)
        {
            LiveSessionState state = null;
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT LiveSessionID,Title,Status
                FROM LiveSessions
                WHERE ChefID=@chefId", connection))
            {
                command.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        state = new LiveSessionState
                        {
                            LiveSessionId = Convert.ToInt32(reader["LiveSessionID"]),
                            Title = Convert.ToString(reader["Title"]),
                            Status = Convert.ToString(reader["Status"])
                        };
                    }
                }
            }

            if (state == null)
            {
                ShowMessage("Unable to find your livestream settings.", "error");
                pnlGoLiveForm.Visible = false;
                return;
            }

            if (string.Equals(state.Status, "Live", StringComparison.OrdinalIgnoreCase))
            {
                ApplyLiveState(state);
                return;
            }

            if (string.Equals(state.Status, "Suspended", StringComparison.OrdinalIgnoreCase))
            {
                ApplySuspendedState(chefId);
                return;
            }

            ApplyOfflineState();
        }

        private void ApplyOfflineState()
        {
            SetStatus("Offline", "Chef is currently offline.", "Get ready to share your cooking knowledge with the Cooking LAH community.");
            litPreviewStatus.Text = "OFFLINE";
            litPreviewTitle.Text = "Stream is offline.";
            litPreviewText.Text = "Your live video will appear here once you start streaming.";
            lblStatusBadge.CssClass = "chef-live-status-badge chef-live-status-badge--offline";
            pnlGoLiveForm.Visible = true;
            pnlUnavailable.Visible = false;
            pnlStreamingPenalty.Visible = false;
            btnReturnToLive.Visible = false;
            lblCurrentTitle.Visible = false;
        }

        private void ApplyLiveState(LiveSessionState state)
        {
            SetStatus("Live", "You currently have an active livestream.", "");
            lblCurrentTitle.Text = Server.HtmlEncode(state.Title);
            lblCurrentTitle.Visible = true;
            litPreviewStatus.Text = "LIVE";
            litPreviewTitle.Text = Server.HtmlEncode(state.Title);
            litPreviewText.Text = "Return to your active cooking livestream.";
            lblStatusBadge.CssClass = "chef-live-status-badge chef-live-status-badge--live";
            pnlGoLiveForm.Visible = false;
            pnlUnavailable.Visible = true;
            lblUnavailableTitle.Text = "You already have an active livestream.";
            lblUnavailableText.Text = "Return to the live room before starting another stream.";
            pnlStreamingPenalty.Visible = false;
            btnReturnToLive.Visible = true;
        }

        private void ApplySuspendedState(int chefId)
        {
            SetStatus("Suspended", "Livestreaming is temporarily unavailable.", "");
            litPreviewStatus.Text = "SUSPENDED";
            litPreviewTitle.Text = "Streaming unavailable";
            litPreviewText.Text = "Your livestream access will return after the active penalty ends.";
            lblStatusBadge.CssClass = "chef-live-status-badge chef-live-status-badge--suspended";
            pnlGoLiveForm.Visible = false;
            pnlUnavailable.Visible = true;
            lblUnavailableTitle.Text = "Livestreaming is temporarily unavailable.";
            lblUnavailableText.Text = "Review the penalty details above.";
            btnReturnToLive.Visible = false;
            lblCurrentTitle.Visible = false;
            LoadActivePenalty(chefId);
        }

        private void SetStatus(string status, string title, string text)
        {
            lblStatusBadge.Text = status.ToUpperInvariant();
            litStatusTitle.Text = Server.HtmlEncode(title);
            litStatusText.Text = Server.HtmlEncode(text);
        }

        private void LoadActivePenalty(int chefId)
        {
            pnlStreamingPenalty.Visible = false;
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT TOP (1) Reason,EndsAt
                FROM LiveStreamingPenalties
                WHERE ChefID=@chefId AND EndsAt>GETDATE()
                ORDER BY EndsAt DESC", connection))
            {
                command.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return;
                    }

                    lblPenaltyEndTime.Text = Convert.ToDateTime(reader["EndsAt"]).ToString("dd MMM yyyy, h:mm tt");
                    lblPenaltyReason.Text = Server.HtmlEncode(Convert.ToString(reader["Reason"]));
                    pnlStreamingPenalty.Visible = true;
                }
            }
        }

        protected void btnStartLive_Click(object sender, EventArgs e)
        {
            int chefId;
            if (!TryGetCurrentChefId(out chefId))
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            string title = txtLiveTitle.Text.Trim();
            string description = txtLiveDescription.Text.Trim();
            if (!ValidateStreamDetails(title, description))
            {
                return;
            }

            string thumbnailPath = null;
            try
            {
                RefreshSuspensionStatus(chefId);
                if (ChefHasActivePenalty(chefId))
                {
                    LoadLiveSessionState(chefId);
                    ShowMessage("Livestreaming is temporarily unavailable.", "warning");
                    return;
                }

                thumbnailPath = SaveThumbnail();
                int sessionId;
                if (!TryStartLive(chefId, title, description, thumbnailPath, out sessionId))
                {
                    DeleteNewThumbnail(thumbnailPath);
                    LoadLiveSessionState(chefId);
                    ShowMessage("Another livestream is already active.", "warning");
                    return;
                }

                Response.Redirect(ResolveUrl("~/Pages/LiveSessionPage/ChefLivePage.aspx?sessionId=" + sessionId), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (InvalidOperationException ex)
            {
                DeleteNewThumbnail(thumbnailPath);
                ShowMessage(ex.Message, "error");
            }
            catch (SqlException ex)
            {
                DeleteNewThumbnail(thumbnailPath);
                System.Diagnostics.Trace.TraceError("Starting livestream failed: {0}", ex.Number);
                ShowMessage("Unable to start your livestream. Please try again.", "error");
            }
        }

        private bool ValidateStreamDetails(string title, string description)
        {
            if (title.Length == 0)
            {
                ShowMessage("Please enter a stream title.", "error");
                return false;
            }
            if (title.Length > 150)
            {
                ShowMessage("The stream title must be 150 characters or fewer.", "error");
                return false;
            }
            if (description.Length > 1000)
            {
                ShowMessage("The description must be 1000 characters or fewer.", "error");
                return false;
            }
            return true;
        }

        // Only change Offline sessions to Live, so two tabs cannot both start a stream.
        private bool TryStartLive(int chefId, string title, string description, string thumbnailPath, out int sessionId)
        {
            sessionId = 0;
            string oldThumbnailPath = null;
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand(@"
                        UPDATE LiveSessions
                        SET Title=@title,
                            Description=@description,
                            ThumbnailPath=COALESCE(@thumbnailPath,ThumbnailPath),
                            JitsiRoomName=@jitsiRoomName,
                            StartedAt=GETDATE(),
                            EndedAt=NULL,
                            Status='Live',
                            AdminCloseReason=NULL,
                            UpdatedAt=GETDATE()
                        OUTPUT deleted.ThumbnailPath
                        WHERE ChefID=@chefId
                          AND Status='Offline'
                          AND NOT EXISTS (
                              SELECT 1 FROM LiveStreamingPenalties
                              WHERE ChefID=@chefId AND EndsAt>GETDATE()
                          )", connection, transaction))
                    {
                        command.Parameters.Add("@title", SqlDbType.NVarChar, 150).Value = title;
                        command.Parameters.Add("@description", SqlDbType.NVarChar, 1000).Value = description;
                        command.Parameters.Add("@thumbnailPath", SqlDbType.NVarChar, 500).Value = (object)thumbnailPath ?? DBNull.Value;
                        command.Parameters.Add("@jitsiRoomName", SqlDbType.NVarChar, 200).Value = GenerateJitsiRoomName(chefId);
                        command.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                        object oldPath = command.ExecuteScalar();
                        if (oldPath == null)
                        {
                            transaction.Rollback();
                            return false;
                        }
                        oldThumbnailPath = Convert.ToString(oldPath);
                    }

                    using (SqlCommand idCommand = new SqlCommand("SELECT LiveSessionID FROM LiveSessions WHERE ChefID=@chefId", connection, transaction))
                    {
                        idCommand.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                        sessionId = Convert.ToInt32(idCommand.ExecuteScalar());
                    }
                    transaction.Commit();
                }
            }
            if (!string.IsNullOrEmpty(thumbnailPath) && !string.Equals(oldThumbnailPath, thumbnailPath, StringComparison.OrdinalIgnoreCase))
                DeleteNewThumbnail(oldThumbnailPath);
            return true;
        }

        private bool ChefHasActivePenalty(int chefId)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT COUNT(1)
                FROM LiveStreamingPenalties
                WHERE ChefID=@chefId AND EndsAt>GETDATE()", connection))
            {
                command.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        protected void btnReturnToLive_Click(object sender, EventArgs e)
        {
            int chefId;
            if (!TryGetCurrentChefId(out chefId))
            {
                return;
            }

            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT LiveSessionID
                FROM LiveSessions
                WHERE ChefID=@chefId AND Status='Live'", connection))
            {
                command.Parameters.Add("@chefId", SqlDbType.Int).Value = chefId;
                connection.Open();
                object result = command.ExecuteScalar();
                if (result == null)
                {
                    LoadLiveSessionState(chefId);
                    ShowMessage("This livestream is no longer active.", "warning");
                    return;
                }

                Response.Redirect(ResolveUrl("~/Pages/LiveSessionPage/ChefLivePage.aspx?sessionId=" + result), false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private string GenerateJitsiRoomName(int chefId)
        {
            return "CookingLahLive_" + chefId + "_" + Guid.NewGuid().ToString("N");
        }

        private string SaveThumbnail()
        {
            if (!fuThumbnail.HasFile)
            {
                return null;
            }
            if (fuThumbnail.PostedFile.ContentLength <= 0 || fuThumbnail.PostedFile.ContentLength > MaximumThumbnailBytes)
            {
                throw new InvalidOperationException("Choose a thumbnail up to 3 MB.");
            }

            string extension = Path.GetExtension(fuThumbnail.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp")
            {
                throw new InvalidOperationException("The selected thumbnail format is not supported.");
            }

            ValidateThumbnailContent(extension);
            string storedExtension = extension == ".jpeg" ? ".jpg" : extension;
            string relativePath = ThumbnailFolder + Guid.NewGuid().ToString("N") + storedExtension;
            Directory.CreateDirectory(Server.MapPath(ThumbnailFolder));
            fuThumbnail.SaveAs(Server.MapPath(relativePath));
            return relativePath;
        }

        private void ValidateThumbnailContent(string extension)
        {
            Stream stream = fuThumbnail.PostedFile.InputStream;
            if (extension == ".webp")
            {
                byte[] header = new byte[12];
                int bytesRead = stream.Read(header, 0, header.Length);
                stream.Position = 0;
                bool isWebP = bytesRead == 12
                    && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF"
                    && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP";
                if (!isWebP)
                {
                    throw new InvalidOperationException("The selected thumbnail is not a valid WEBP image.");
                }
                return;
            }

            try
            {
                using (Image image = Image.FromStream(stream, true, true))
                {
                    bool validFormat = extension == ".png"
                        ? image.RawFormat.Guid == ImageFormat.Png.Guid
                        : image.RawFormat.Guid == ImageFormat.Jpeg.Guid;
                    if (!validFormat || image.Width > 4096 || image.Height > 4096 || (long)image.Width * image.Height > 16000000)
                    {
                        throw new InvalidOperationException("Choose a valid thumbnail no larger than 4096 pixels per side.");
                    }
                }
                stream.Position = 0;
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException("The selected thumbnail is not a readable image.");
            }
            catch (OutOfMemoryException)
            {
                throw new InvalidOperationException("The selected thumbnail could not be processed.");
            }
        }

        private void DeleteNewThumbnail(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) || !System.Text.RegularExpressions.Regex.IsMatch(relativePath,
                "^" + System.Text.RegularExpressions.Regex.Escape(ThumbnailFolder) + "[a-f0-9]{32}\\.(?:jpg|png|webp)$"))
            {
                return;
            }
            string physicalPath = Server.MapPath(relativePath);
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = "chef-live-message chef-live-message--" + type;
            lblMessage.Text = Server.HtmlEncode(message);
        }

        private sealed class LiveSessionState
        {
            public int LiveSessionId { get; set; }
            public string Title { get; set; }
            public string Status { get; set; }
        }
    }
}
