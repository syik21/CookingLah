using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Learner.LearnMyActivity
{
    public partial class LearnerMyActivityPage : Page
    {
        private const int PageSize = 5;
        private int LearnerId { get; set; }
        private int CurrentPage
        {
            get { return (int?)ViewState["CurrentPage"] ?? 0; }
            set { ViewState["CurrentPage"] = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            LearnerId = GetCurrentLearnerId();
            if (LearnerId <= 0)
            {
                RedirectToLogin();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (LearnerId <= 0)
            {
                return;
            }

            try
            {
                LoadSubmissionStatistics();
            }
            catch (SqlException)
            {
                ShowMessage("Unable to load your activity submission summary.");
            }

            LoadSubmissions();
        }

        // Verify the learner session and active account.
        private int GetCurrentLearnerId()
        {
            int learnerId = 0;
            bool hasSession = string.Equals(Convert.ToString(Session["Role"]), "Learner", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(Convert.ToString(Session["UserId"]), out learnerId) && learnerId > 0;
            if (!hasSession)
            {
                return 0;
            }

            DataTable learner = Read(@"
                SELECT UserID FROM Users
                WHERE UserID=@learnerId AND Role='Learner' AND AccountStatus='Active'",
                Parameter("@learnerId", learnerId));
            return learner.Rows.Count == 1 ? learnerId : 0;
        }

        private void RedirectToLogin()
        {
            Response.Redirect(ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        // Calculate the learner's submission status counts.
        private void LoadSubmissionStatistics()
        {
            DataRow row = Read(@"
                SELECT
                    ISNULL(SUM(CASE WHEN Status='Submitted' THEN 1 ELSE 0 END),0) AS SubmittedCount,
                    ISNULL(SUM(CASE WHEN Status='Approved' THEN 1 ELSE 0 END),0) AS ApprovedCount,
                    ISNULL(SUM(CASE WHEN Status='Resubmit Required' THEN 1 ELSE 0 END),0) AS ResubmitCount
                FROM ActivitySubmissions WHERE LearnerID=@learnerId",
                Parameter("@learnerId", LearnerId)).Rows[0];
            litSubmittedCount.Text = Convert.ToString(row["SubmittedCount"]);
            litApprovedCount.Text = Convert.ToString(row["ApprovedCount"]);
            litResubmitCount.Text = Convert.ToString(row["ResubmitCount"]);
        }

        // Load activity submissions for the current learner.
        private void LoadSubmissions()
        {
            string status = GetSelectedStatus();
            string searchText = txtSearch.Text.Trim();
            string orderBy = ddlSort.SelectedValue == "oldest"
                ? "s.SubmittedAt ASC, s.SubmissionID ASC"
                : "s.SubmittedAt DESC, s.SubmissionID DESC";
            string query = @"
                SELECT s.SubmissionID,s.Status,s.SubmittedAt,
                       a.Title AS ActivityTitle,c.CourseID,c.Title AS CourseTitle,c.Thumbnail
                FROM ActivitySubmissions s
                INNER JOIN PracticalActivities a ON a.ActivityID=s.ActivityID
                INNER JOIN Courses c ON c.CourseID=a.CourseID
                WHERE s.LearnerID=@learnerId
                  AND (@status='' OR s.Status=@status)
                  AND (@searchText='' OR c.Title LIKE '%' + @searchText + '%' OR a.Title LIKE '%' + @searchText + '%')
                ORDER BY " + orderBy;
            try
            {
                BindSubmissions(Read(query,
                    Parameter("@learnerId", LearnerId),
                    Parameter("@status", status),
                    Parameter("@searchText", searchText)));
            }
            catch (SqlException)
            {
                ShowMessage("Unable to load your activity submissions. Please try again.");
            }
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
            pnlSubmissionDetail.Visible = false;
            pnlSubmissionResults.Visible = filteredCount > 0;
            pnlNoSubmissions.Visible = totalCount == 0;
            pnlNoResults.Visible = totalCount > 0 && filteredCount == 0;
            pnlPagination.Visible = filteredCount > PageSize;
            litResultCount.Text = filteredCount == 1 ? "1 Submission" : filteredCount + " Submissions";
            lblPageStatus.Text = string.Format("Page {0} of {1}", CurrentPage + 1, pageCount);
            btnPrevious.Enabled = CurrentPage > 0;
            btnNext.Enabled = CurrentPage < pageCount - 1;
        }

        private int GetTotalSubmissionCount()
        {
            DataTable count = Read("SELECT COUNT(SubmissionID) AS SubmissionCount FROM ActivitySubmissions WHERE LearnerID=@learnerId",
                Parameter("@learnerId", LearnerId));
            return Convert.ToInt32(count.Rows[0]["SubmissionCount"]);
        }

        private string GetSelectedStatus()
        {
            string status = ddlStatus.SelectedValue;
            string[] allowed = { "", "Submitted", "Approved", "Resubmit Required" };
            return Array.IndexOf(allowed, status) >= 0 ? status : "";
        }

        // Load the selected submission and Chef feedback.
        private void LoadSubmissionDetails(int submissionId)
        {
            DataTable details = Read(@"
                SELECT s.SubmissionID,s.ImagePath,s.Description,s.Status,s.InstructorFeedback,s.SubmittedAt,
                       a.Title AS ActivityTitle,c.CourseID,c.Title AS CourseTitle,
                       cp.Status AS CourseProgressStatus,cp.ProgressPercentage
                FROM ActivitySubmissions s
                INNER JOIN PracticalActivities a ON a.ActivityID=s.ActivityID
                INNER JOIN Courses c ON c.CourseID=a.CourseID
                LEFT JOIN CourseProgress cp ON cp.CourseID=c.CourseID AND cp.LearnerID=s.LearnerID
                WHERE s.SubmissionID=@submissionId AND s.LearnerID=@learnerId",
                Parameter("@submissionId", submissionId), Parameter("@learnerId", LearnerId));
            if (details.Rows.Count != 1)
            {
                ShowMessage("Unable to load this submission. Please return to My Activity Submissions and try again.");
                LoadSubmissions();
                return;
            }

            DataRow submission = details.Rows[0];
            int courseId = Convert.ToInt32(submission["CourseID"]);
            string status = Convert.ToString(submission["Status"]);
            string imageUrl = MediaUrl(Convert.ToString(submission["ImagePath"]));
            string description = Convert.ToString(submission["Description"]);
            string feedback = Convert.ToString(submission["InstructorFeedback"]);
            pnlSubmissionList.Visible = false;
            pnlSubmissionDetail.Visible = true;
            litDetailCourseTitle.Text = Convert.ToString(submission["CourseTitle"]);
            litDetailActivityTitle.Text = Convert.ToString(submission["ActivityTitle"]);
            litDetailDate.Text = DisplayDate(submission["SubmittedAt"]);
            lblDetailStatus.Text = status;
            lblDetailStatus.CssClass = StatusClass(status);
            imgSubmission.ImageUrl = imageUrl;
            imgSubmission.AlternateText = "Your submitted work for " + Convert.ToString(submission["ActivityTitle"]);
            imgSubmission.Visible = imageUrl.Length > 0;
            pnlNoImage.Visible = imageUrl.Length == 0;
            litDescription.Text = string.IsNullOrWhiteSpace(description) ? "No description was provided." : description;
            ConfigureDetailState(status, feedback);
            ConfigureCourseProgress(submission, status);
            ConfigureDetailActions(courseId, status);
        }

        private void ConfigureDetailState(string status, string feedback)
        {
            pnlDetailState.CssClass = "activity-detail-state " + StatusModifier(status);
            if (status == "Approved")
            {
                litDetailStateTitle.Text = "Activity Approved";
                litDetailStateMessage.Text = "Your Chef has approved this practical activity.";
                litChefFeedback.Text = string.IsNullOrWhiteSpace(feedback) ? "No additional feedback was provided." : feedback;
            }
            else if (status == "Resubmit Required")
            {
                litDetailStateTitle.Text = "Resubmission Required";
                litDetailStateMessage.Text = "Your Chef has requested changes before approving your practical activity.";
                litChefFeedback.Text = string.IsNullOrWhiteSpace(feedback) ? "Review your activity requirements before resubmitting." : feedback;
            }
            else
            {
                litDetailStateTitle.Text = "Activity Submitted";
                litDetailStateMessage.Text = "Your practical activity was submitted successfully and is waiting for Chef review.";
                litChefFeedback.Text = "No feedback yet. Your submission is waiting for review.";
            }
        }

        private void ConfigureCourseProgress(DataRow submission, string submissionStatus)
        {
            string courseStatus = Convert.ToString(submission["CourseProgressStatus"]);
            int percentage = submission["ProgressPercentage"] == DBNull.Value ? 0 : Convert.ToInt32(submission["ProgressPercentage"]);
            bool completed = submissionStatus == "Approved" && courseStatus == "Completed" && percentage == 100;
            pnlCompletedCourse.Visible = completed;
            if (!completed)
            {
                return;
            }

            litCourseStatus.Text = courseStatus;
            litCourseProgress.Text = Convert.ToString(percentage);
        }

        private void ConfigureDetailActions(int courseId, string status)
        {
            string learningUrl = ResolveUrl("~/Pages/Learner/CourseLearning/CourseLearningPage.aspx?courseId=" + courseId);
            lnkCourseOverview.NavigateUrl = ResolveUrl("~/Pages/Learner/SearchCourse/SearchCoursePage.aspx?courseId=" + courseId);
            lnkCourseLearning.NavigateUrl = learningUrl;
            lnkCourseLearning.Visible = status == "Approved";
            lnkResubmit.NavigateUrl = learningUrl;
            lnkResubmit.Visible = status == "Resubmit Required";
        }

        // Check that the selected submission belongs to the learner.
        private bool ValidateSubmissionOwnership(int submissionId)
        {
            DataTable owned = Read(@"
                SELECT SubmissionID FROM ActivitySubmissions
                WHERE SubmissionID=@submissionId AND LearnerID=@learnerId",
                Parameter("@submissionId", submissionId), Parameter("@learnerId", LearnerId));
            return owned.Rows.Count == 1;
        }

        protected void rptSubmissions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int submissionId = 0;
            bool valid = e.CommandName == "OpenSubmission" && int.TryParse(Convert.ToString(e.CommandArgument), out submissionId) && submissionId > 0;

            try
            {
                if (!valid || !ValidateSubmissionOwnership(submissionId))
                {
                    ShowMessage("Unable to load this submission.");
                    LoadSubmissions();
                    return;
                }

                LoadSubmissionDetails(submissionId);
            }
            catch (SqlException)
            {
                ShowMessage("Unable to load this submission. Please try again.");
                LoadSubmissions();
            }
        }

        protected void btnBackToSubmissions_Click(object sender, EventArgs e)
        {
            LoadSubmissionStatistics();
            LoadSubmissions();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ResetPageAndLoad();
        }

        protected void FiltersChanged(object sender, EventArgs e)
        {
            ResetPageAndLoad();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlStatus.SelectedIndex = 0;
            ddlSort.SelectedIndex = 0;
            ResetPageAndLoad();
        }
        private void ResetPageAndLoad()
        {
            CurrentPage = 0;
            LoadSubmissions();
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            CurrentPage = Math.Max(0, CurrentPage - 1);
            LoadSubmissions();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            LoadSubmissions();
        }

        protected string Media(object value)
        {
            return MediaUrl(Convert.ToString(value));
        }

        protected string DisplayDate(object value)
        {
            return Convert.ToDateTime(value).ToString("dd MMM yyyy");
        }

        protected string DateValue(object value)
        {
            return Convert.ToDateTime(value).ToString("yyyy-MM-dd");
        }

        protected string StatusMessage(object status)
        {
            string value = Convert.ToString(status);
            if (value == "Approved")
            {
                return "Your practical activity has been approved.";
            }

            if (value == "Resubmit Required")
            {
                return "Your Chef has requested some changes.";
            }

            return "Waiting for Chef review.";
        }

        protected string ActionLabel(object status)
        {
            return Convert.ToString(status) == "Resubmit Required"
                ? "View Feedback & Resubmit"
                : "View Submission";
        }

        protected string ActionClass(object status)
        {
            return Convert.ToString(status) == "Resubmit Required"
                ? "btn btn-primary"
                : "btn";
        }

        protected string CardClass(object status)
        {
            return "activity-submission-card " + StatusModifier(Convert.ToString(status));
        }

        protected string StatusClass(object status)
        {
            return "activity-status " + StatusModifier(Convert.ToString(status));
        }

        private string StatusModifier(string status)
        {
            if (status == "Approved")
            {
                return "is-approved";
            }

            if (status == "Resubmit Required")
            {
                return "is-resubmit";
            }

            return "is-submitted";
        }

        private void ShowMessage(string message)
        {
            pnlMessage.Visible = true;
            litMessage.Text = message;
        }
    }
}
