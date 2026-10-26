using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Shared.CourseSearch
{
    public sealed class CourseActionEventArgs : EventArgs
    {
        public int CourseId { get; private set; }

        public CourseActionEventArgs(int courseId)
        {
            CourseId = courseId;
        }
    }

    public partial class ViewCourseOverivew : UserControl
    {
        private int CourseId
        {
            get { return (int?)ViewState["CourseId"] ?? 0; }
            set { ViewState["CourseId"] = value; }
        }

        public bool IsLearner { get; set; }

        public event EventHandler BackRequested;
        public event EventHandler<CourseActionEventArgs> StartCourseRequested;

        public void LoadCourse(int courseId)
        {
            CourseId = courseId;
            LoadCourseDetails(courseId);
            LoadRecipe(courseId);
            LoadLessons(courseId);
            LoadQuiz(courseId);
            LoadActivity(courseId);
            LoadLearnerAction(courseId);
        }

        private void LoadCourseDetails(int courseId)
        {
            DataTable rows = Read(@"
                SELECT c.CourseID,c.Title,c.Description,c.Thumbnail,c.DifficultyLevel,
                       cat.CategoryName,u.FullName AS ChefName,
                       (SELECT AVG(CAST(r.Rating AS DECIMAL(5,2))) FROM Reviews r WHERE r.CourseID=c.CourseID) AS AverageRating,
                       (SELECT COUNT(*) FROM Reviews r WHERE r.CourseID=c.CourseID) AS ReviewCount,
                       (SELECT COUNT(*) FROM Lessons l WHERE l.CourseID=c.CourseID) AS LessonCount,
                       (SELECT COUNT(*) FROM Quizzes q WHERE q.CourseID=c.CourseID) AS QuizCount,
                       (SELECT COUNT(*) FROM PracticalActivities a WHERE a.CourseID=c.CourseID) AS ActivityCount
                FROM Courses c
                INNER JOIN Categories cat ON cat.CategoryID=c.CategoryID
                INNER JOIN Users u ON u.UserID=c.InstructorID
                WHERE c.CourseID=@courseId AND c.Status='Published'",
                Parameter("@courseId", courseId));

            if (rows.Rows.Count == 0)
            {
                throw new HttpException(404, "This course is not available.");
            }

            DataRow course = rows.Rows[0];
            litCourseTitle.Text = Convert.ToString(course["Title"]);
            litCategory.Text = Convert.ToString(course["CategoryName"]);
            litDifficulty.Text = Convert.ToString(course["DifficultyLevel"]);
            litChefName.Text = Convert.ToString(course["ChefName"]);
            litDescription.Text = Convert.ToString(course["Description"]);
            litRating.Text = FormatRating(course["AverageRating"], course["ReviewCount"]);
            litLessonCount.Text = Convert.ToString(course["LessonCount"]);
            litQuizCount.Text = Convert.ToString(course["QuizCount"]);
            litActivityCount.Text = Convert.ToString(course["ActivityCount"]);

            string thumbnail = MediaUrl(Convert.ToString(course["Thumbnail"]));
            imgCourseThumbnail.ImageUrl = thumbnail;
            imgCourseThumbnail.AlternateText = Convert.ToString(course["Title"]) + " course thumbnail";
            imgCourseThumbnail.Visible = thumbnail.Length > 0;
            pnlCourseImagePlaceholder.Visible = thumbnail.Length == 0;
        }

        private void LoadRecipe(int courseId)
        {
            DataTable recipe = Read("SELECT TOP 1 * FROM CourseRecipes WHERE CourseID=@courseId ORDER BY RecipeID",
                Parameter("@courseId", courseId));
            rptRecipe.DataSource = recipe;
            rptRecipe.DataBind();
            pnlRecipeEmpty.Visible = recipe.Rows.Count == 0;
        }

        private void LoadLessons(int courseId)
        {
            DataTable lessons = Read("SELECT LessonOrder,Title FROM Lessons WHERE CourseID=@courseId ORDER BY LessonOrder,LessonID",
                Parameter("@courseId", courseId));
            rptLessonOverview.DataSource = lessons;
            rptLessonOverview.DataBind();
            pnlLessonsEmpty.Visible = lessons.Rows.Count == 0;
        }

        private void LoadQuiz(int courseId)
        {
            DataTable quizzes = Read(@"
                SELECT q.QuizID,q.Title,q.Description,q.PassingScore,
                       (SELECT COUNT(*) FROM QuizQuestions qq WHERE qq.QuizID=q.QuizID) AS QuestionCount
                FROM Quizzes q WHERE q.CourseID=@courseId ORDER BY q.QuizID",
                Parameter("@courseId", courseId));
            rptQuizOverview.DataSource = quizzes;
            rptQuizOverview.DataBind();
            pnlQuizEmpty.Visible = quizzes.Rows.Count == 0;
        }

        private void LoadActivity(int courseId)
        {
            DataTable activities = Read(@"
                SELECT ActivityID,Title,LEFT(Instructions,280) AS ShortInstructions
                FROM PracticalActivities WHERE CourseID=@courseId ORDER BY ActivityID",
                Parameter("@courseId", courseId));
            rptActivityOverview.DataSource = activities;
            rptActivityOverview.DataBind();
            pnlActivityEmpty.Visible = activities.Rows.Count == 0;
        }

        private void LoadLearnerAction(int courseId)
        {
            btnStartCourse.Text = "Start course";
            if (!IsLearner) return;

            int learnerId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out learnerId) || learnerId <= 0 ||
                Convert.ToString(Session["Role"]) != "Learner")
            {
                return;
            }

            DataTable progress = Read(@"
                SELECT cp.Status FROM CourseProgress cp
                INNER JOIN Users u ON u.UserID=cp.LearnerID
                WHERE cp.CourseID=@courseId AND cp.LearnerID=@learnerId
                  AND u.Role='Learner' AND u.AccountStatus='Active'",
                Parameter("@courseId", courseId),
                Parameter("@learnerId", learnerId));

            if (progress.Rows.Count == 0) return;
            string status = Convert.ToString(progress.Rows[0]["Status"]);
            if (status == "Completed") btnStartCourse.Text = "View course";
            else if (status == "In Progress") btnStartCourse.Text = "Continue learning";
        }

        private string FormatRating(object averageRating, object reviewCount)
        {
            int count = Convert.ToInt32(reviewCount);
            if (count == 0 || averageRating == DBNull.Value) return "No ratings yet";
            return "★ " + Convert.ToDecimal(averageRating).ToString("0.0") + " · " + count + (count == 1 ? " review" : " reviews");
        }

        public void ShowGuestLoginPrompt()
        {
            pnlGuestLogin.Visible = true;
        }

        private void RaiseStartCourseRequested()
        {
            var handler = StartCourseRequested;
            if (handler != null) handler(this, new CourseActionEventArgs(CourseId));
        }

        protected void btnBackToSearch_Click(object sender, EventArgs e)
        {
            var handler = BackRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected void btnStartCourse_Click(object sender, EventArgs e)
        {
            if (!IsLearner)
            {
                ShowGuestLoginPrompt();
                return;
            }

            RaiseStartCourseRequested();
        }

        protected void btnCloseGuestPrompt_Click(object sender, EventArgs e)
        {
            pnlGuestLogin.Visible = false;
        }
    }
}

