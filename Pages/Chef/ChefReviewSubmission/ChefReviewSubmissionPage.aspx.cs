using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.ChefReviewSubmission
{
    public partial class ChefReviewSubmissionPage : Page
    {
        private const int PageSize = 8;
        private const int MaximumFeedbackLength = 2000;

        private int ChefId { get; set; }

        private int CurrentPage
        {
            get
            {
                return (int?)ViewState["CurrentPage"] ?? 0;
            }
            set
            {
                ViewState["CurrentPage"] = value;
            }
        }

        private int SelectedSubmissionId
        {
            get
            {
                return (int?)ViewState["SelectedSubmissionId"] ?? 0;
            }
            set
            {
                ViewState["SelectedSubmissionId"] = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ChefId = GetCurrentChefId();

            if (ChefId <= 0)
            {
                throw new HttpException(403, "Please sign in as an active Chef.");
            }

            ViewStateUserKey = Session.SessionID;
            Response.Cache.SetNoStore();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCourseFilter();
                SelectSubmissionFromQueryString();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            LoadSubmissionStatistics();

            if (SelectedSubmissionId > 0)
            {
                LoadSubmissionDetails(SelectedSubmissionId);
            }
            else
            {
                LoadSubmissions();
            }

            base.OnPreRender(e);
        }

        // Verify the current session belongs to an active Chef.
        private int GetCurrentChefId()
        {
            int chefId = 0;
            bool validSession =
                string.Equals(Convert.ToString(Session["Role"]), "Instructor", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(Convert.ToString(Session["UserId"]), out chefId) &&
                chefId > 0;

            if (!validSession)
            {
                return 0;
            }

            DataTable chef = Read(@"
                SELECT UserID
                FROM Users
                WHERE UserID=@chefId
                  AND Role='Instructor'
                  AND AccountStatus='Active'",
                Parameter("@chefId", chefId));

            return chef.Rows.Count == 1 ? chefId : 0;
        }

        private void SelectSubmissionFromQueryString()
        {
            int submissionId;

            if (int.TryParse(Request.QueryString["submissionId"], out submissionId) &&
                submissionId > 0)
            {
                SelectedSubmissionId = submissionId;
            }
        }

        // Load Courses owned by the current Chef.
        private void LoadCourseFilter()
        {
            ddlCourse.DataSource = Read(@"
                SELECT CourseID,Title
                FROM Courses
                WHERE InstructorID=@chefId
                ORDER BY Title",
                Parameter("@chefId", ChefId));
            ddlCourse.DataTextField = "Title";
            ddlCourse.DataValueField = "CourseID";
            ddlCourse.DataBind();
            ddlCourse.Items.Insert(0, new ListItem("All Courses", "0"));
        }

        // Calculate submission counts for Courses owned by the Chef.
        private void LoadSubmissionStatistics()
        {
            DataTable counts = Read(@"
                SELECT
                    SUM(CASE WHEN s.Status='Submitted' THEN 1 ELSE 0 END) AS SubmittedCount,
                    SUM(CASE WHEN s.Status='Approved' THEN 1 ELSE 0 END) AS ApprovedCount,
                    SUM(CASE WHEN s.Status='Resubmit Required' THEN 1 ELSE 0 END) AS ResubmitCount
                FROM ActivitySubmissions s
                INNER JOIN PracticalActivities a
                    ON a.ActivityID=s.ActivityID
                INNER JOIN Courses c
                    ON c.CourseID=a.CourseID
                WHERE c.InstructorID=@chefId",
                Parameter("@chefId", ChefId));

            DataRow row = counts.Rows[0];
            litSubmittedCount.Text = CountValue(row["SubmittedCount"]);
            litApprovedCount.Text = CountValue(row["ApprovedCount"]);
            litResubmitCount.Text = CountValue(row["ResubmitCount"]);
        }

        private string CountValue(object value)
        {
            return value == DBNull.Value ? "0" : Convert.ToString(value);
        }

        protected string SubmissionImageUrl(object value)
        {
            return MediaUrl(Convert.ToString(value));
        }

        // Load submissions from Courses owned by the current Chef.
        private void LoadSubmissions()
        {
            int courseId = GetSelectedCourseId();
            string status = GetSelectedStatus();
            string searchText = txtSearch.Text.Trim();
            string orderBy = ddlSort.SelectedValue == "oldest"
                ? "s.SubmittedAt ASC,s.SubmissionID ASC"
                : "s.SubmittedAt DESC,s.SubmissionID DESC";

            DataTable submissions = Read(@"
                SELECT
                    s.SubmissionID,
                    s.Status,
                    s.SubmittedAt,
                    s.ImagePath,
                    learner.FullName AS LearnerName,
                    c.Title AS CourseTitle,
                    a.Title AS ActivityTitle
                FROM ActivitySubmissions s
                INNER JOIN PracticalActivities a
                    ON a.ActivityID=s.ActivityID
                INNER JOIN Courses c
                    ON c.CourseID=a.CourseID
                INNER JOIN Users learner
                    ON learner.UserID=s.LearnerID
                WHERE c.InstructorID=@chefId
                  AND (@courseId=0 OR c.CourseID=@courseId)
                  AND (@status='' OR s.Status=@status)
                  AND (
                      @searchText=''
                      OR learner.FullName LIKE '%' + @searchText + '%'
                      OR c.Title LIKE '%' + @searchText + '%'
                  )
                ORDER BY CASE WHEN s.Status='Submitted' THEN 0 ELSE 1 END," + orderBy,
                Parameter("@chefId", ChefId),
                Parameter("@courseId", courseId),
                Parameter("@status", status),
                Parameter("@searchText", searchText));

            BindSubmissions(submissions);
        }

        // Bind one page of submission results.
        private void BindSubmissions(DataTable submissions)
        {
            int filteredCount = submissions.Rows.Count;
            int totalCount = GetTotalSubmissionCount();
            int pageCount = Math.Max(1, (filteredCount + PageSize - 1) / PageSize);

            CurrentPage = Math.Min(CurrentPage, pageCount - 1);

            PagedDataSource page = new PagedDataSource
            {
                DataSource = submissions.DefaultView,
                AllowPaging = true,
                PageSize = PageSize,
                CurrentPageIndex = CurrentPage
            };

            rptSubmissions.DataSource = page;
            rptSubmissions.DataBind();

            pnlSubmissionList.Visible = true;
            pnlReviewWorkspace.Visible = false;
            pnlSubmissionResults.Visible = filteredCount > 0;
            pnlNoSubmissions.Visible = totalCount == 0;
            pnlNoResults.Visible = totalCount > 0 && filteredCount == 0;
            pnlPagination.Visible = filteredCount > PageSize;

            litResultCount.Text = filteredCount == 1
                ? "1 Submission"
                : filteredCount + " Submissions";
            lblPageStatus.Text = string.Format("Page {0} of {1}", CurrentPage + 1, pageCount);
            btnPrevious.Enabled = CurrentPage > 0;
            btnNext.Enabled = CurrentPage < pageCount - 1;
        }

        private int GetTotalSubmissionCount()
        {
            DataTable count = Read(@"
                SELECT COUNT(s.SubmissionID) AS SubmissionCount
                FROM ActivitySubmissions s
                INNER JOIN PracticalActivities a
                    ON a.ActivityID=s.ActivityID
                INNER JOIN Courses c
                    ON c.CourseID=a.CourseID
                WHERE c.InstructorID=@chefId",
                Parameter("@chefId", ChefId));

            return Convert.ToInt32(count.Rows[0]["SubmissionCount"]);
        }

        private int GetSelectedCourseId()
        {
            int courseId;
            return int.TryParse(ddlCourse.SelectedValue, out courseId) && courseId > 0
                ? courseId
                : 0;
        }

        private string GetSelectedStatus()
        {
            string status = ddlStatus.SelectedValue;
            string[] allowed = { "", "Submitted", "Approved", "Resubmit Required" };
            return Array.IndexOf(allowed, status) >= 0 ? status : "";
        }

        // Load the selected learner submission for review.
        private void LoadSubmissionDetails(int submissionId)
        {
            DataTable details = Read(@"
                SELECT
                    s.SubmissionID,
                    s.ImagePath,
                    s.Description,
                    s.Status,
                    s.InstructorFeedback,
                    s.SubmittedAt,
                    s.ReviewedAt,
                    learner.FullName AS LearnerName,
                    c.Title AS CourseTitle,
                    a.Title AS ActivityTitle,
                    a.Instructions
                FROM ActivitySubmissions s
                INNER JOIN PracticalActivities a
                    ON a.ActivityID=s.ActivityID
                INNER JOIN Courses c
                    ON c.CourseID=a.CourseID
                INNER JOIN Users learner
                    ON learner.UserID=s.LearnerID
                WHERE s.SubmissionID=@submissionId
                  AND c.InstructorID=@chefId",
                Parameter("@submissionId", submissionId),
                Parameter("@chefId", ChefId));

            if (details.Rows.Count != 1)
            {
                SelectedSubmissionId = 0;
                ShowMessage("Unable to access this submission.", true);
                LoadSubmissions();
                return;
            }

            DataRow submission = details.Rows[0];
            string status = Convert.ToString(submission["Status"]);
            string imageUrl = MediaUrl(Convert.ToString(submission["ImagePath"]));
            string description = Convert.ToString(submission["Description"]);
            string feedback = Convert.ToString(submission["InstructorFeedback"]);

            pnlSubmissionList.Visible = false;
            pnlReviewWorkspace.Visible = true;
            litLearnerName.Text = Convert.ToString(submission["LearnerName"]);
            litCourseTitle.Text = Convert.ToString(submission["CourseTitle"]);
            litActivityTitle.Text = Convert.ToString(submission["ActivityTitle"]);
            litSubmittedDate.Text = DisplayDate(submission["SubmittedAt"]);
            lblDetailStatus.Text = status;
            lblDetailStatus.CssClass = StatusClass(status);
            imgSubmission.ImageUrl = imageUrl;
            imgSubmission.AlternateText = "Submitted practical activity by " + Convert.ToString(submission["LearnerName"]);
            imgSubmission.Visible = imageUrl.Length > 0;
            pnlNoImage.Visible = imageUrl.Length == 0;
            litDescription.Text = string.IsNullOrWhiteSpace(description) ? "No description was provided." : description;
            litRequirementTitle.Text = Convert.ToString(submission["ActivityTitle"]);
            litInstructions.Text = Convert.ToString(submission["Instructions"]);

            pnlActiveReview.Visible = status == "Submitted";
            pnlReadOnlyReview.Visible = status != "Submitted";
            pnlCompletedProgress.Visible = status == "Approved";

            if (!IsPostBack || status != "Submitted")
            {
                txtFeedback.Text = feedback;
            }

            ConfigureReadOnlyState(status, feedback);
        }

        private void ConfigureReadOnlyState(string status, string feedback)
        {
            if (status == "Approved")
            {
                litReviewStateTitle.Text = "Activity Approved";
                litReviewStateMessage.Text = "This learner has completed the course.";
                litSavedFeedback.Text = string.IsNullOrWhiteSpace(feedback)
                    ? "No additional feedback was provided."
                    : feedback;
                return;
            }

            litReviewStateTitle.Text = "Resubmission Required";
            litReviewStateMessage.Text = "Waiting for the learner to resubmit their work.";
            litSavedFeedback.Text = feedback;
        }

        // Check that the submission belongs to the current Chef.
        private bool ValidateSubmissionOwnership(int submissionId)
        {
            DataTable owned = Read(@"
                SELECT s.SubmissionID
                FROM ActivitySubmissions s
                INNER JOIN PracticalActivities a
                    ON a.ActivityID=s.ActivityID
                INNER JOIN Courses c
                    ON c.CourseID=a.CourseID
                WHERE s.SubmissionID=@submissionId
                  AND c.InstructorID=@chefId",
                Parameter("@submissionId", submissionId),
                Parameter("@chefId", ChefId));

            return owned.Rows.Count == 1;
        }

        // Approve the activity and complete the learner's Course.
        private bool ApproveSubmission(int submissionId, string feedback)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        object ownership = Execute(connection, transaction, @"
                            SELECT CAST(s.LearnerID AS NVARCHAR(20)) + ':' + CAST(c.CourseID AS NVARCHAR(20))
                            FROM ActivitySubmissions s WITH (UPDLOCK,HOLDLOCK)
                            INNER JOIN PracticalActivities a
                                ON a.ActivityID=s.ActivityID
                            INNER JOIN Courses c
                                ON c.CourseID=a.CourseID
                            WHERE s.SubmissionID=@submissionId
                              AND s.Status='Submitted'
                              AND c.InstructorID=@chefId",
                            Parameter("@submissionId", submissionId),
                            Parameter("@chefId", ChefId));

                        if (ownership == null)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        string[] identifiers = Convert.ToString(ownership).Split(':');
                        int learnerId = Convert.ToInt32(identifiers[0]);
                        int courseId = Convert.ToInt32(identifiers[1]);

                        object updatedSubmission = Execute(connection, transaction, @"
                            UPDATE ActivitySubmissions
                            SET Status='Approved',
                                InstructorFeedback=@feedback,
                                ReviewedBy=@chefId,
                                ReviewedAt=GETDATE()
                            WHERE SubmissionID=@submissionId
                              AND Status='Submitted';
                            SELECT @@ROWCOUNT;",
                            Parameter("@feedback", string.IsNullOrWhiteSpace(feedback) ? (object)DBNull.Value : feedback),
                            Parameter("@chefId", ChefId),
                            Parameter("@submissionId", submissionId));

                        object updatedProgress = Execute(connection, transaction, @"
                            UPDATE CourseProgress
                            SET ProgressPercentage=100,
                                Status='Completed',
                                CompletedAt=COALESCE(CompletedAt,GETDATE()),
                                LastAccessedAt=GETDATE()
                            WHERE LearnerID=@learnerId
                              AND CourseID=@courseId;
                            SELECT @@ROWCOUNT;",
                            Parameter("@learnerId", learnerId),
                            Parameter("@courseId", courseId));

                        if (Convert.ToInt32(updatedSubmission) != 1 ||
                            Convert.ToInt32(updatedProgress) != 1)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Save feedback and ask the learner to resubmit.
        private bool RequestResubmission(int submissionId, string feedback)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        object identifiers = Execute(connection, transaction, @"
                            SELECT CAST(s.LearnerID AS NVARCHAR(20)) + ':' + CAST(c.CourseID AS NVARCHAR(20))
                            FROM ActivitySubmissions s WITH (UPDLOCK,HOLDLOCK)
                            INNER JOIN PracticalActivities a ON a.ActivityID=s.ActivityID
                            INNER JOIN Courses c ON c.CourseID=a.CourseID
                            WHERE s.SubmissionID=@submissionId
                              AND s.Status='Submitted'
                              AND c.InstructorID=@chefId",
                            Parameter("@submissionId", submissionId),
                            Parameter("@chefId", ChefId));

                        if (identifiers == null)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        string[] values = Convert.ToString(identifiers).Split(':');
                        int learnerId = Convert.ToInt32(values[0]);
                        int courseId = Convert.ToInt32(values[1]);

                        object updatedSubmission = Execute(connection, transaction, @"
                            UPDATE s
                            SET s.Status='Resubmit Required',
                                s.InstructorFeedback=@feedback,
                                s.ReviewedBy=@chefId,
                                s.ReviewedAt=GETDATE()
                            FROM ActivitySubmissions s
                            WHERE s.SubmissionID=@submissionId AND s.Status='Submitted';
                            SELECT @@ROWCOUNT;",
                            Parameter("@feedback", feedback),
                            Parameter("@chefId", ChefId),
                            Parameter("@submissionId", submissionId));

                        object updatedProgress = Execute(connection, transaction, @"
                            UPDATE CourseProgress
                            SET Status='In Progress',
                                CompletedAt=NULL,
                                LastAccessedAt=GETDATE()
                            WHERE LearnerID=@learnerId
                              AND CourseID=@courseId
                              AND Status='Submitted';
                            SELECT @@ROWCOUNT;",
                            Parameter("@learnerId", learnerId),
                            Parameter("@courseId", courseId));

                        if (Convert.ToInt32(updatedSubmission) != 1 || Convert.ToInt32(updatedProgress) != 1)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        protected void rptSubmissions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int submissionId;

            if (e.CommandName != "OpenSubmission" ||
                !int.TryParse(Convert.ToString(e.CommandArgument), out submissionId) ||
                submissionId <= 0 ||
                !ValidateSubmissionOwnership(submissionId))
            {
                ShowMessage("Unable to access this submission.", true);
                return;
            }

            SelectedSubmissionId = submissionId;
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            string feedback = txtFeedback.Text.Trim();

            if (!ValidateFeedbackLength(feedback))
            {
                return;
            }

            try
            {
                if (!ValidateSubmissionOwnership(SelectedSubmissionId) ||
                    !ApproveSubmission(SelectedSubmissionId, feedback))
                {
                    ShowMessage("Unable to approve this activity.", true);
                    return;
                }

                ShowMessage("Activity approved successfully. The learner's course has been marked as Completed.", false);
            }
            catch (SqlException)
            {
                ShowMessage("Unable to approve the activity. Please try again.", true);
            }
        }

        protected void btnRequestResubmission_Click(object sender, EventArgs e)
        {
            string feedback = txtFeedback.Text.Trim();

            if (string.IsNullOrWhiteSpace(feedback))
            {
                ShowMessage("Please provide feedback explaining what the learner should improve before resubmitting.", true);
                return;
            }

            if (!ValidateFeedbackLength(feedback))
            {
                return;
            }

            try
            {
                if (!ValidateSubmissionOwnership(SelectedSubmissionId) ||
                    !RequestResubmission(SelectedSubmissionId, feedback))
                {
                    ShowMessage("Unable to request resubmission.", true);
                    return;
                }

                ShowMessage("Resubmission requested successfully. The learner will be able to view your feedback.", false);
            }
            catch (SqlException)
            {
                ShowMessage("Unable to request resubmission. Please try again.", true);
            }
        }

        private bool ValidateFeedbackLength(string feedback)
        {
            if (feedback.Length <= MaximumFeedbackLength)
            {
                return true;
            }

            ShowMessage("Keep feedback within 2,000 characters.", true);
            return false;
        }

        protected void btnBackToSubmissions_Click(object sender, EventArgs e)
        {
            SelectedSubmissionId = 0;
            txtFeedback.Text = "";
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            CurrentPage = 0;
        }

        protected void FiltersChanged(object sender, EventArgs e)
        {
            CurrentPage = 0;
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlCourse.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            ddlSort.SelectedIndex = 0;
            CurrentPage = 0;
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            CurrentPage = Math.Max(0, CurrentPage - 1);
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            CurrentPage++;
        }

        protected string DisplayDate(object value)
        {
            return Convert.ToDateTime(value).ToString("dd MMM yyyy");
        }

        protected string DateValue(object value)
        {
            return Convert.ToDateTime(value).ToString("yyyy-MM-dd");
        }

        protected string ActionLabel(object status)
        {
            return Convert.ToString(status) == "Submitted" ? "Review" : "View";
        }

        protected string StatusClass(object status)
        {
            string value = Convert.ToString(status);

            if (value == "Approved")
            {
                return "submission-status is-approved";
            }

            if (value == "Resubmit Required")
            {
                return "submission-status is-resubmit";
            }

            return "submission-status is-submitted";
        }

        private void ShowMessage(string message, bool isError)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = isError
                ? "submission-message is-error"
                : "submission-message is-success";
            pnlMessage.Attributes["role"] = isError ? "alert" : "status";
            litMessage.Text = message;
        }
    }
}
