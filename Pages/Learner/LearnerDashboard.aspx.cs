using CookingLah.F_Helpers;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Xml.Linq;

namespace CookingLah.Pages.Learner
{
    public partial class LearnerDashboard : Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // TODO: Load learner dashboard data.
                // Preview values and inline XML sources are explicitly markup-only.
            }
            if (Session["Username"] != null)
            {
                lblLearnerName.Text = "Hi," + Session["Username"].ToString();
            }
        }

        private void LoadLearnerDashboard()
        {
            // TODO: Call all learner dashboard loading methods.
            // TODO: Bind the welcome name from the authenticated Users record.
            // Remove preview DataSourceID attributes before assigning real DataSource values.
        }

        private void LoadDashboardSummary()
        {
            // TODO: Load courses in progress/completed from CourseProgress,
            // quizzes taken and average quiz score from QuizAttempts.
        }

        private void LoadContinueLearning()
        {
            // TODO: Load the most recently accessed in-progress course.
            // Use Courses, Categories and CourseProgress; bind image/title/level/category,
            // percentage (label and progress value), last access and destination course ID.
        }

        private void LoadCurrentCourses()
        {
            // TODO: Load up to three current learner courses and bind rptCurrentCourses.
        }

        private void LoadRecentQuizResults()
        {
            // TODO: Load the latest three attempts from Quizzes and QuizAttempts.
        }

        private void LoadPracticalActivities()
        {
            // TODO: Load PracticalActivities with learner-specific ActivitySubmissions.
            // No submission record means the display-only status "Not Submitted".
            // Existing records support Submitted and Reviewed; never store Not Submitted.
        }

        private void LoadExploreCourses()
        {
            // TODO: Load up to three published Courses with Categories and Reviews averages.
            // No recommendation algorithm is required.
        }

        protected void ContinueCourse_Click(object sender, EventArgs e)
        {
            // TODO: Redirect to the selected course. Connect navigation when the destination is available.
        }

        protected void ExploreFoodMap_Click(object sender, EventArgs e)
        {
            // TODO: Redirect to Food Culture Map. Connect navigation when the destination is available.
        }
    }
}
