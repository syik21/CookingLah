using System;
using System.Data;
using System.Web;
using System.Web.UI;
using CookingLah.Pages.Chef.CreateCourse;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Learner.CourseLearning.Lesson
{
    public partial class LessonContent : UserControl
    {
        public event EventHandler PreviousLessonRequested;
        public event EventHandler NextLessonRequested;
        public event EventHandler LessonCompleted;

        public int CurrentLessonId { get; private set; }
        public int RequestedLessonId { get; private set; }

        public void LoadLesson(
            int courseId,
            int learnerId,
            int lessonId,
            int previousLessonId,
            int nextLessonId,
            bool isCompleted)
        {
            CurrentLessonId = lessonId;
            RequestedLessonId = 0;

            DataTable lessons = Read(@"
                SELECT l.LessonID,l.LessonOrder,l.Title,l.Description,l.Instructions,
                       l.LessonImage,l.LessonVideo
                FROM Lessons l
                INNER JOIN Courses c ON c.CourseID=l.CourseID
                INNER JOIN CourseProgress cp ON cp.CourseID=c.CourseID
                INNER JOIN Users u ON u.UserID=cp.LearnerID
                WHERE l.LessonID=@lessonId
                  AND l.CourseID=@courseId
                  AND c.Status='Published'
                  AND cp.LearnerID=@learnerId
                  AND u.Role='Learner'
                  AND u.AccountStatus='Active'",
                Parameter("@lessonId", lessonId),
                Parameter("@courseId", courseId),
                Parameter("@learnerId", learnerId));

            if (lessons.Rows.Count == 0)
            {
                pnlLessonUnavailable.Visible = true;
                pnlLessonContent.Visible = false;
                return;
            }

            DataRow lesson = lessons.Rows[0];
            litLessonOrder.Text = Convert.ToString(lesson["LessonOrder"]);
            litLessonTitle.Text = Convert.ToString(lesson["Title"]);
            litLessonDescription.Text = Convert.ToString(lesson["Description"]);
            litInstructions.Text = Convert.ToString(lesson["Instructions"]);

            string imageUrl = MediaUrl(Convert.ToString(lesson["LessonImage"]));
            imgLesson.ImageUrl = imageUrl;
            imgLesson.AlternateText = Convert.ToString(lesson["Title"]) + " lesson image";
            imgLesson.Visible = imageUrl.Length > 0;

            string videoUrl = MediaUrl(Convert.ToString(lesson["LessonVideo"]));
            pnlLessonVideo.Visible = videoUrl.Length > 0;
            litLessonVideoSource.Text = videoUrl.Length == 0
                ? ""
                : "<source src=\"" + HttpUtility.HtmlAttributeEncode(videoUrl) + "\" type=\"video/mp4\" />";

            btnPreviousLesson.Visible = previousLessonId > 0;
            btnNextLesson.Visible = nextLessonId > 0;
            btnMarkComplete.Visible = !isCompleted;
            btnMarkComplete.Enabled = !isCompleted;
            pnlLessonCompletionNote.Visible = isCompleted;
            pnlLessonUnavailable.Visible = false;
            pnlLessonContent.Visible = true;

            ViewState["PreviousLessonId"] = previousLessonId;
            ViewState["NextLessonId"] = nextLessonId;
        }

        protected void btnPreviousLesson_Click(object sender, EventArgs e)
        {
            RequestedLessonId = (int?)ViewState["PreviousLessonId"] ?? 0;
            RaiseLessonRequest(PreviousLessonRequested);
        }

        protected void btnNextLesson_Click(object sender, EventArgs e)
        {
            RequestedLessonId = (int?)ViewState["NextLessonId"] ?? 0;
            RaiseLessonRequest(NextLessonRequested);
        }

        protected void btnMarkComplete_Click(object sender, EventArgs e)
        {
            if (CurrentLessonId <= 0) return;

            EventHandler handler = LessonCompleted;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void RaiseLessonRequest(EventHandler handler)
        {
            if (RequestedLessonId <= 0 || handler == null) return;

            handler(this, EventArgs.Empty);
        }
    }
}
