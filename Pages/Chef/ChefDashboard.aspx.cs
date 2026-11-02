using CookingLah.F_Helpers;
using System;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef
{
    public partial class ChefDashboard : Page
    {
        private int ChefId { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ChefId = GetCurrentChefId();
            if (ChefId <= 0)
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            ViewStateUserKey = Session.SessionID;
            Response.Cache.SetNoStore();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (ChefId <= 0 || IsPostBack)
                return;

            LoadChefProfile();
            LoadCourseStatistics();
            LoadActivitySubmissions();
            LoadReviewStatistics();
            LoadRecentReviews();
        }

        // Verify the current session belongs to an active Chef.
        private int GetCurrentChefId()
        {
            int chefId = 0;
            bool validSession = string.Equals(Convert.ToString(Session["Role"]), "Instructor", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(Convert.ToString(Session["UserId"]), out chefId)
                && chefId > 0;
            if (!validSession)
                return 0;

            DataTable chef = Read("SELECT UserID FROM Users WHERE UserID=@chefId AND Role='Instructor' AND AccountStatus='Active'", Parameter("@chefId", chefId));
            return chef.Rows.Count == 1 ? chefId : 0;
        }

        // Load the current Chef's display name.
        private void LoadChefProfile()
        {
            DataTable chef = Read("SELECT COALESCE(NULLIF(FullName,''),Username) AS ChefName FROM Users WHERE UserID=@chefId", Parameter("@chefId", ChefId));
            litChefName.Text = chef.Rows.Count == 1 ? Convert.ToString(chef.Rows[0]["ChefName"]) : Convert.ToString(Session["Username"]);
        }

        // Load course statistics for the current Chef.
        private void LoadCourseStatistics()
        {
            DataRow row = Read(@"
                SELECT COUNT(CourseID) AS TotalCount,
                       SUM(CASE WHEN Status='Published' THEN 1 ELSE 0 END) AS PublishedCount,
                       SUM(CASE WHEN Status='Draft' THEN 1 ELSE 0 END) AS DraftCount,
                       SUM(CASE WHEN Status='Pending' THEN 1 ELSE 0 END) AS PendingCount,
                       SUM(CASE WHEN Status='Rejected' THEN 1 ELSE 0 END) AS RejectedCount
                FROM Courses
                WHERE InstructorID=@chefId", Parameter("@chefId", ChefId)).Rows[0];

            int total = Value(row["TotalCount"]);
            int published = Value(row["PublishedCount"]);
            int draft = Value(row["DraftCount"]);
            int pending = Value(row["PendingCount"]);
            int rejected = Value(row["RejectedCount"]);
            litTotalCourses.Text = Convert.ToString(total);
            litPublishedCourses.Text = Convert.ToString(published);
            pnlNoCourses.Visible = total == 0;

            var statuses = new[]
            {
                new { Status = "Published", Count = published, Percent = Percentage(published, total) },
                new { Status = "Draft", Count = draft, Percent = Percentage(draft, total) },
                new { Status = "Pending", Count = pending, Percent = Percentage(pending, total) },
                new { Status = "Rejected", Count = rejected, Percent = Percentage(rejected, total) }
            };
            rptCourseStatus.DataSource = statuses;
            rptCourseStatus.DataBind();
        }

        // Load recent activity submissions waiting for review.
        private void LoadActivitySubmissions()
        {
            int submissionCount = Convert.ToInt32(Read(@"
                SELECT COUNT(s.SubmissionID)
                FROM ActivitySubmissions s
                INNER JOIN PracticalActivities a ON a.ActivityID=s.ActivityID
                INNER JOIN Courses c ON c.CourseID=a.CourseID
                WHERE s.Status='Submitted' AND c.InstructorID=@chefId", Parameter("@chefId", ChefId)).Rows[0][0]);

            DataTable submissions = Read(@"
                SELECT TOP 5 s.SubmissionID,s.SubmittedAt,
                       COALESCE(NULLIF(u.FullName,''),u.Username) AS LearnerName,
                       c.Title AS CourseTitle,a.Title AS ActivityTitle
                FROM ActivitySubmissions s
                INNER JOIN PracticalActivities a ON a.ActivityID=s.ActivityID
                INNER JOIN Courses c ON c.CourseID=a.CourseID
                INNER JOIN Users u ON u.UserID=s.LearnerID
                WHERE s.Status='Submitted' AND c.InstructorID=@chefId
                ORDER BY s.SubmittedAt DESC,s.SubmissionID DESC", Parameter("@chefId", ChefId));

            litSubmittedActivities.Text = Convert.ToString(submissionCount);
            litSubmissionSummary.Text = submissionCount == 1 ? "1 submission waiting for review" : submissionCount + " submissions waiting for review";
            rptSubmissions.DataSource = submissions;
            rptSubmissions.DataBind();
            pnlSubmissions.Visible = submissionCount > 0;
            pnlNoSubmissions.Visible = submissionCount == 0;
        }

        // Calculate the Chef's average course rating.
        private void LoadReviewStatistics()
        {
            DataRow row = Read(@"
                SELECT COUNT(r.ReviewID) AS ReviewCount,
                       AVG(CAST(r.Rating AS DECIMAL(5,2))) AS AverageRating
                FROM Reviews r
                INNER JOIN Courses c ON c.CourseID=r.CourseID
                WHERE c.InstructorID=@chefId", Parameter("@chefId", ChefId)).Rows[0];
            int reviewCount = Convert.ToInt32(row["ReviewCount"]);
            litAverageRating.Text = reviewCount == 0 ? "No reviews yet" : "★ " + Convert.ToDecimal(row["AverageRating"]).ToString("0.0");
            litRatingReviewCount.Text = reviewCount == 1 ? "1 Review" : reviewCount + " Reviews";
        }

        // Load the latest reviews from the Chef's courses.
        private void LoadRecentReviews()
        {
            DataTable reviews = Read(@"
                SELECT TOP 3 r.Rating,r.Comment,r.CreatedAt,c.Title AS CourseTitle,
                       COALESCE(NULLIF(u.FullName,''),u.Username) AS LearnerName
                FROM Reviews r
                INNER JOIN Courses c ON c.CourseID=r.CourseID
                INNER JOIN Users u ON u.UserID=r.LearnerID
                WHERE c.InstructorID=@chefId
                ORDER BY r.CreatedAt DESC,r.ReviewID DESC", Parameter("@chefId", ChefId));
            rptReviews.DataSource = reviews;
            rptReviews.DataBind();
            pnlReviews.Visible = reviews.Rows.Count > 0;
            pnlNoReviews.Visible = reviews.Rows.Count == 0;
        }

        private static int Value(object value)
        {
            return value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private static int Percentage(int count, int total)
        {
            return total == 0 ? 0 : (int)Math.Round(count * 100m / total);
        }

        protected string StatusRowClass(object status, object count)
        {
            return Convert.ToString(status) == "Rejected" && Convert.ToInt32(count) > 0 ? "chef-status-row is-rejected" : "chef-status-row";
        }

        protected string StarText(object value)
        {
            int rating = Convert.ToInt32(value);
            return new string('\u2605', rating) + new string('\u2606', 5 - rating);
        }

        protected string DisplayDate(object value)
        {
            return Convert.ToDateTime(value).ToString("d MMM yyyy");
        }

        protected string DateValue(object value)
        {
            return Convert.ToDateTime(value).ToString("yyyy-MM-dd");
        }

        protected string SubmissionUrl(object value)
        {
            return ResolveUrl("~/Pages/Chef/ChefReviewSubmission/ChefReviewSubmissionPage.aspx?submissionId=" + Convert.ToInt32(value));
        }
    }
}
