using System;
using System.Data;
using System.Web.UI;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Learner
{
    public partial class LearnerDashboard : Page
    {
        private int LearnerId { get; set; }
        protected int ContinueProgress { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            int learnerId;
            if (!string.Equals(Convert.ToString(Session["Role"]), "Learner", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(Convert.ToString(Session["UserId"]), out learnerId) || learnerId <= 0 ||
                Read("SELECT UserID FROM Users WHERE UserID=@id AND Role='Learner' AND AccountStatus='Active'", Parameter("@id", learnerId)).Rows.Count != 1)
            {
                Response.Redirect(ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            LearnerId = learnerId;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (LearnerId <= 0) return;
            if (!IsPostBack) LoadLearnerDashboard();
            ContinueProgress = (int?)ViewState["ContinueProgress"] ?? 0;
        }

        private void LoadLearnerDashboard()
        {
            DataRow learner = Read("SELECT COALESCE(NULLIF(FullName,''),Username) AS DisplayName FROM Users WHERE UserID=@id", Parameter("@id", LearnerId)).Rows[0];
            lblLearnerName.Text = Convert.ToString(learner["DisplayName"]);
            LoadDashboardSummary();
            LoadContinueLearning();
            LoadCurrentCourses();
            LoadRecentQuizResults();
            LoadPracticalActivities();
            LoadExploreCourses();
        }

        private void LoadDashboardSummary()
        {
            DataRow row = Read(@"SELECT
                (SELECT COUNT(*) FROM CourseProgress cp JOIN Courses c ON c.CourseID=cp.CourseID WHERE cp.LearnerID=@id AND cp.Status IN ('In Progress','Submitted') AND c.Status='Published') AS InProgress,
                (SELECT COUNT(*) FROM CourseProgress cp JOIN Courses c ON c.CourseID=cp.CourseID WHERE cp.LearnerID=@id AND cp.Status='Completed' AND c.Status='Published') AS Completed,
                (SELECT COUNT(*) FROM QuizAttempts WHERE LearnerID=@id) AS QuizCount,
                (SELECT AVG(CAST(Score AS DECIMAL(5,1))) FROM QuizAttempts WHERE LearnerID=@id) AS AverageScore", Parameter("@id", LearnerId)).Rows[0];
            lblCoursesInProgress.Text = Convert.ToString(row["InProgress"]);
            lblCoursesCompleted.Text = Convert.ToString(row["Completed"]);
            lblQuizzesTaken.Text = Convert.ToString(row["QuizCount"]);
            lblAverageQuizScore.Text = row["AverageScore"] == DBNull.Value ? "—" : Convert.ToDecimal(row["AverageScore"]).ToString("0") + "%";
        }

        private void LoadContinueLearning()
        {
            DataTable rows = Read(@"SELECT TOP 1 c.CourseID,c.Title,c.Thumbnail,c.DifficultyLevel,(SELECT TOP 1 CookingTime FROM CourseRecipes cr WHERE cr.CourseID=c.CourseID) AS CookingTime,cat.CategoryName,cp.ProgressPercentage,cp.LastAccessedAt
                FROM CourseProgress cp JOIN Courses c ON c.CourseID=cp.CourseID JOIN Categories cat ON cat.CategoryID=c.CategoryID
                WHERE cp.LearnerID=@id AND cp.Status IN ('In Progress','Submitted') AND c.Status='Published'
                ORDER BY cp.LastAccessedAt DESC,cp.ProgressID DESC", Parameter("@id", LearnerId));
            if (rows.Rows.Count == 0)
            {
                lblContinueCourseTitle.Text = "Choose your next course";
                lblContinueDifficulty.Text = "Ready when you are";
                lblContinueCategory.Text = "Explore Cooking LAH";
                lblContinueProgress.Text = "0%";
                lblLastAccessed.Text = "Not started yet";
                imgContinueCourse.ImageUrl = ResolveUrl("~/F_Assets/mascot.png");
                imgContinueCourse.AlternateText = "Cooking LAH mascot";
                btnContinueCourse.Text = "Explore Courses";
                ViewState["ContinueCourseId"] = 0;
                SetContinueProgress(0);
                return;
            }
            DataRow course = rows.Rows[0];
            int progress = Math.Max(0, Math.Min(100, Convert.ToInt32(course["ProgressPercentage"])));
            lblContinueCourseTitle.Text = Convert.ToString(course["Title"]);
            lblContinueDifficulty.Text = Convert.ToString(course["DifficultyLevel"]);
            lblContinueCategory.Text = Convert.ToString(course["CategoryName"]);
            lblContinueProgress.Text = progress + "%";
            lblLastAccessed.Text = Convert.ToDateTime(course["LastAccessedAt"]).ToString("d MMM yyyy");
            string image = CourseImage(course["Thumbnail"]);
            imgContinueCourse.ImageUrl = image.Length == 0 ? ResolveUrl("~/F_Assets/mascot.png") : image;
            imgContinueCourse.AlternateText = Convert.ToString(course["Title"]) + " course thumbnail";
            ViewState["ContinueCourseId"] = Convert.ToInt32(course["CourseID"]);
            SetContinueProgress(progress);
        }

        private void SetContinueProgress(int progress) { ContinueProgress = progress; ViewState["ContinueProgress"] = progress; }

        private void LoadCurrentCourses()
        {
            Bind(rptCurrentCourses, Read(@"SELECT TOP 3 c.CourseID,c.Title,c.Thumbnail,c.DifficultyLevel,(SELECT TOP 1 CookingTime FROM CourseRecipes cr WHERE cr.CourseID=c.CourseID) AS CookingTime,cp.ProgressPercentage
                FROM CourseProgress cp JOIN Courses c ON c.CourseID=cp.CourseID
                WHERE cp.LearnerID=@id AND cp.Status IN ('In Progress','Submitted') AND c.Status='Published'
                ORDER BY cp.LastAccessedAt DESC,cp.ProgressID DESC", Parameter("@id", LearnerId)));
        }

        private void LoadRecentQuizResults()
        {
            Bind(rptRecentQuizzes, Read(@"SELECT TOP 3 q.Title,qa.Score,qa.Passed
                FROM QuizAttempts qa JOIN Quizzes q ON q.QuizID=qa.QuizID JOIN Courses c ON c.CourseID=q.CourseID
                WHERE qa.LearnerID=@id AND c.Status='Published' ORDER BY qa.AttemptID DESC", Parameter("@id", LearnerId)));
        }

        private void LoadPracticalActivities()
        {
            Bind(rptPracticalActivities, Read(@"SELECT TOP 3 a.Title,c.Title AS CourseTitle,c.CourseID,s.SubmissionID,s.Status AS SubmissionStatus
                FROM CourseProgress cp JOIN Courses c ON c.CourseID=cp.CourseID JOIN PracticalActivities a ON a.CourseID=c.CourseID
                OUTER APPLY (SELECT TOP 1 x.SubmissionID,x.Status FROM ActivitySubmissions x WHERE x.ActivityID=a.ActivityID AND x.LearnerID=@id ORDER BY x.SubmittedAt DESC,x.SubmissionID DESC) s
                WHERE cp.LearnerID=@id AND cp.Status IN ('In Progress','Submitted','Completed') AND c.Status='Published'
                ORDER BY CASE WHEN s.SubmissionID IS NULL THEN 0 ELSE 1 END,cp.LastAccessedAt DESC,a.ActivityID DESC", Parameter("@id", LearnerId)));
        }

        private void LoadExploreCourses()
        {
            Bind(rptExploreCourses, Read(@"SELECT TOP 3 c.CourseID,c.Title,c.Thumbnail,c.DifficultyLevel,cat.CategoryName,
                (SELECT TOP 1 CookingTime FROM CourseRecipes cr WHERE cr.CourseID=c.CourseID) AS CookingTime,
                COALESCE(CONVERT(DECIMAL(3,1),AVG(CONVERT(DECIMAL(5,2),r.Rating))),0) AS AverageRating
                FROM Courses c JOIN Categories cat ON cat.CategoryID=c.CategoryID LEFT JOIN Reviews r ON r.CourseID=c.CourseID
                WHERE c.Status='Published' AND NOT EXISTS (SELECT 1 FROM CourseProgress cp WHERE cp.CourseID=c.CourseID AND cp.LearnerID=@id)
                GROUP BY c.CourseID,c.Title,c.Thumbnail,c.DifficultyLevel,cat.CategoryName,c.CreatedAt
                ORDER BY AVG(CONVERT(DECIMAL(5,2),r.Rating)) DESC,c.CreatedAt DESC,c.CourseID DESC", Parameter("@id", LearnerId)));
        }

        private static void Bind(System.Web.UI.WebControls.Repeater repeater, DataTable rows) { repeater.DataSource = rows; repeater.DataBind(); }
        protected string CourseImage(object value) { return MediaUrl(Convert.ToString(value)); }
        protected bool HasCourseImage(object value) { return CourseImage(value).Length > 0; }
        protected string CourseLearningUrl(object courseId) { return ResolveUrl("~/Pages/Learner/CourseLearning/CourseLearningPage.aspx?courseId=" + Convert.ToInt32(courseId)); }
        protected string CourseOverviewUrl(object courseId) { return ResolveUrl("~/Pages/Learner/SearchCourse/SearchCoursePage.aspx?courseId=" + Convert.ToInt32(courseId)); }
        protected string ActivityStatus(object value) { return value == DBNull.Value ? "Not Submitted" : Convert.ToString(value); }
        protected string ActivityStatusClass(object value) { string status = ActivityStatus(value); return status == "Approved" ? "status-success" : status == "Rejected" ? "status-danger" : "status-pending"; }
        protected string ActivityUrl(object courseId, object submissionId) { return submissionId == DBNull.Value ? CourseLearningUrl(courseId) + "&section=activity" : ResolveUrl("~/Pages/Learner/LearnMyActivity/LearnerMyActivityPage.aspx"); }

        protected void ContinueCourse_Click(object sender, EventArgs e)
        {
            int courseId = (int?)ViewState["ContinueCourseId"] ?? 0;
            Response.Redirect(courseId > 0 ? CourseLearningUrl(courseId) : ResolveUrl("~/Pages/Learner/SearchCourse/SearchCoursePage.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void ExploreFoodMap_Click(object sender, EventArgs e)
        {
            Response.Redirect(ResolveUrl("~/Pages/Learner/Map/Map.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
