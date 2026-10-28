using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace CookingLah.Pages.Admin
{
    public partial class AdminDashboard : Page
    {
        protected int TotalUserCount { get; private set; }
        protected int LearnerCount { get; private set; }
        protected int InstructorCount { get; private set; }
        protected int AdminCount { get; private set; }
        protected int TotalCourseCount { get; private set; }
        protected int PublishedCount { get; private set; }
        protected int DraftCount { get; private set; }
        protected int PendingCount { get; private set; }
        protected int RejectedCount { get; private set; }

        // Verify that the current session belongs to an active administrator.
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            int adminId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out adminId))
            {
                RedirectToLogin();
                return;
            }

            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB())
                using (SqlCommand command = new SqlCommand(@"
                    SELECT COUNT(*) FROM dbo.Users
                    WHERE UserID=@adminId AND Role='Admin' AND AccountStatus='Active'", connection))
                {
                    command.Parameters.Add("@adminId", SqlDbType.Int).Value = adminId;
                    connection.Open();
                    if ((int)command.ExecuteScalar() != 1) RedirectToLogin();
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Admin dashboard authorization: {0}", ex);
                RedirectToLogin();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack || Response.IsRequestBeingRedirected) return;
            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB())
                {
                    connection.Open();
                    LoadAdminProfile(connection);
                    LoadSummaryStatistics(connection);
                    LoadPlatformOverview(connection);
                    LoadPendingCourses(connection);
                    LoadRecentUserActivity(connection);
                    LoadPlatformPerformance(connection);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Admin dashboard: {0}", ex);
                pnlError.Visible = true;
                litError.Text = "Dashboard data could not be loaded. Please try again.";
            }
        }

        private void RedirectToLogin()
        {
            Response.Redirect(NavigationHelper.GetUrl("Main"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        // Load the name shown in the dashboard greeting.
        private void LoadAdminProfile(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand(@"
                SELECT COALESCE(NULLIF(FullName,''),Username)
                FROM dbo.Users WHERE UserID=@adminId", connection))
            {
                command.Parameters.Add("@adminId", SqlDbType.Int).Value = Convert.ToInt32(Session["UserId"]);
                lblAdminName.Text = Convert.ToString(command.ExecuteScalar());
            }
        }

        // Load the main Admin Dashboard statistics.
        private void LoadSummaryStatistics(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand(@"
                SELECT
                    (SELECT COUNT(*) FROM dbo.Users) AS TotalUsers,
                    (SELECT COUNT(*) FROM dbo.Users WHERE CreatedAt>=DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1)) AS NewUsers,
                    (SELECT COUNT(*) FROM dbo.Courses) AS TotalCourses,
                    (SELECT COUNT(*) FROM dbo.Courses WHERE Status='Published') AS PublishedCourses,
                    (SELECT COUNT(DISTINCT UserID) FROM dbo.UserLoginLog WHERE LoginDate>=CAST(DATEADD(day,-6,GETDATE()) AS date)) AS ActiveUsers,
                    (SELECT COUNT(*) FROM dbo.Courses WHERE Status='Pending') AS PendingCourses", connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                reader.Read();
                TotalUserCount = Convert.ToInt32(reader["TotalUsers"]);
                TotalCourseCount = Convert.ToInt32(reader["TotalCourses"]);
                PendingCount = Convert.ToInt32(reader["PendingCourses"]);
                litTotalUsers.Text = TotalUserCount.ToString("N0");
                litNewUsers.Text = "+" + Convert.ToInt32(reader["NewUsers"]).ToString("N0");
                litTotalCourses.Text = TotalCourseCount.ToString("N0");
                litPublishedCourses.Text = Convert.ToInt32(reader["PublishedCourses"]).ToString("N0");
                litActiveUsers.Text = Convert.ToInt32(reader["ActiveUsers"]).ToString("N0");
                litPendingCourses.Text = PendingCount.ToString("N0");
                pendingSummaryCard.Attributes["class"] = "summary-card pending" + (PendingCount > 0 ? " needs-attention" : "");
            }
        }

        // Load user roles and course status breakdowns.
        private void LoadPlatformOverview(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand(@"
                SELECT 'User' AS GroupName,Role AS ItemName,COUNT(*) AS Total FROM dbo.Users GROUP BY Role
                UNION ALL
                SELECT 'Course',Status,COUNT(*) FROM dbo.Courses GROUP BY Status", connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int total = Convert.ToInt32(reader["Total"]);
                    string item = Convert.ToString(reader["ItemName"]);
                    if (Convert.ToString(reader["GroupName"]) == "User")
                    {
                        if (item == "Learner") LearnerCount = total;
                        else if (item == "Instructor") InstructorCount = total;
                        else if (item == "Admin") AdminCount = total;
                    }
                    else
                    {
                        if (item == "Published") PublishedCount = total;
                        else if (item == "Draft") DraftCount = total;
                        else if (item == "Pending") PendingCount = total;
                        else if (item == "Rejected") RejectedCount = total;
                    }
                }
            }
            litLearners.Text = LearnerCount.ToString("N0");
            litInstructors.Text = InstructorCount.ToString("N0");
            litAdmins.Text = AdminCount.ToString("N0");
            litOverviewPublished.Text = PublishedCount.ToString("N0");
            litDraft.Text = DraftCount.ToString("N0");
            litOverviewPending.Text = PendingCount.ToString("N0");
            litRejected.Text = RejectedCount.ToString("N0");
        }

        // Load courses that are waiting for Admin review.
        private void LoadPendingCourses(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand(@"
                SELECT TOP (4) c.CourseID,c.Title,c.DifficultyLevel,
                       COALESCE(c.UpdatedAt,c.CreatedAt) AS SubmittedAt,
                       cat.CategoryName,u.FullName AS InstructorName,u.Username
                FROM dbo.Courses c
                INNER JOIN dbo.Categories cat ON cat.CategoryID=c.CategoryID
                INNER JOIN dbo.Users u ON u.UserID=c.InstructorID
                WHERE c.Status='Pending'
                ORDER BY COALESCE(c.UpdatedAt,c.CreatedAt) DESC,c.CourseID DESC", connection))
            {
                DataTable courses = new DataTable();
                using (SqlDataReader reader = command.ExecuteReader()) { courses.Load(reader); }
                rptPendingCourses.DataSource = courses;
                rptPendingCourses.DataBind();
            }
            lblPendingSummary.Text = PendingCount == 1
                ? "1 course requires Admin review"
                : PendingCount.ToString("N0") + " courses require Admin review";
            pnlNoPending.Visible = PendingCount == 0;
            lnkAllPending.Visible = PendingCount > 0;
            litPendingEmptyTitle.Text = TotalCourseCount == 0
                ? "No courses have been created yet."
                : "No courses are waiting for review.";
            litPendingEmptyText.Text = TotalCourseCount == 0 ? "" : "You're all caught up.";
        }

        // Load recently active system users.
        private void LoadRecentUserActivity(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand(@"
                SELECT TOP (5) FullName,Username,Role,LastLogin
                FROM dbo.Users WHERE LastLogin IS NOT NULL
                ORDER BY LastLogin DESC,UserID DESC", connection))
            {
                DataTable users = new DataTable();
                using (SqlDataReader reader = command.ExecuteReader()) { users.Load(reader); }
                rptRecentUsers.DataSource = users;
                rptRecentUsers.DataBind();
                pnlNoActivity.Visible = users.Rows.Count == 0;
            }
        }

        // Load overall enrolment and course performance statistics.
        private void LoadPlatformPerformance(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand(@"
                SELECT
                    (SELECT COUNT(ProgressID) FROM dbo.CourseProgress) AS TotalEnrolments,
                    (SELECT COUNT(ProgressID) FROM dbo.CourseProgress WHERE Status='Completed') AS CompletedEnrolments,
                    (SELECT AVG(CAST(Rating AS DECIMAL(5,2))) FROM dbo.Reviews) AS AverageRating,
                    (SELECT COUNT(ReviewID) FROM dbo.Reviews) AS TotalReviews", connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                reader.Read();
                int enrolments = Convert.ToInt32(reader["TotalEnrolments"]);
                int reviews = Convert.ToInt32(reader["TotalReviews"]);
                litEnrolments.Text = enrolments.ToString("N0");
                litCompleted.Text = Convert.ToInt32(reader["CompletedEnrolments"]).ToString("N0");
                litReviews.Text = reviews.ToString("N0");
                litAverageRating.Text = reader["AverageRating"] == DBNull.Value ? "No reviews yet" : "★ " + Convert.ToDecimal(reader["AverageRating"]).ToString("0.0");
                lblPerformanceEmpty.Text = enrolments == 0 ? "No course enrolments yet." : reviews == 0 ? "No course reviews yet." : "";
            }
        }

        protected string BarWidth(int value, int total)
        {
            int percentage = total == 0 ? 0 : (int)Math.Round(value * 100.0 / total);
            return "width:" + Math.Max(0, Math.Min(100, percentage)) + "%";
        }

        protected string DisplayName(object fullName, object username)
        {
            string name = Convert.ToString(fullName).Trim();
            return name.Length > 0 ? name : Convert.ToString(username);
        }

        protected string ReviewCourseUrl(object courseId)
        {
            return ResolveUrl("~/Pages/Admin/CourseContentReview/ReviewCoursesPage.aspx?courseId=" + Convert.ToInt32(courseId));
        }

        protected string DateTimeIso(object value) { return Convert.ToDateTime(value).ToString("s"); }

        protected string RelativeLogin(object value)
        {
            DateTime login = Convert.ToDateTime(value);
            if (login.Date == DateTime.Today) return "Today, " + login.ToString("h:mm tt");
            if (login.Date == DateTime.Today.AddDays(-1)) return "Yesterday, " + login.ToString("h:mm tt");
            return login.ToString("dd MMM yyyy, h:mm tt");
        }
    }
}
