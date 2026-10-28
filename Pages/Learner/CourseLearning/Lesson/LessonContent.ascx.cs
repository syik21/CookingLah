using System;
using System.Data;
using System.Web.UI;
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
        public bool ChefPreview { get; set; }

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

            DataTable lessons = ChefPreview ? Read(@"
                SELECT l.LessonID,l.LessonOrder,l.Title,l.Description
                FROM Lessons l INNER JOIN Courses c ON c.CourseID=l.CourseID
                WHERE l.LessonID=@lessonId AND l.CourseID=@courseId AND c.InstructorID=@learnerId",
                Parameter("@lessonId", lessonId), Parameter("@courseId", courseId), Parameter("@learnerId", learnerId)) : Read(@"
                SELECT l.LessonID,l.LessonOrder,l.Title,l.Description
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
            var blocks = ChefPreview ? Read(@"SELECT lc.* FROM LessonContent lc
                JOIN Lessons l ON l.LessonID=lc.LessonID JOIN Courses c ON c.CourseID=l.CourseID
                WHERE lc.LessonID=@lessonId AND l.CourseID=@courseId AND c.InstructorID=@learnerId",
                Parameter("@lessonId", lessonId), Parameter("@courseId", courseId), Parameter("@learnerId", learnerId)) : Read(@"SELECT lc.* FROM LessonContent lc
                JOIN Lessons l ON l.LessonID=lc.LessonID
                JOIN Courses c ON c.CourseID=l.CourseID
                JOIN CourseProgress cp ON cp.CourseID=c.CourseID
                JOIN Users u ON u.UserID=cp.LearnerID
                WHERE lc.LessonID=@lessonId AND l.CourseID=@courseId
                AND c.Status='Published' AND cp.LearnerID=@learnerId
                AND u.Role='Learner' AND u.AccountStatus='Active'",
                Parameter("@lessonId", lessonId), Parameter("@courseId", courseId), Parameter("@learnerId", learnerId));
            LessonBlocks.BindBlocks(blocks, lessonId);

            btnPreviousLesson.Visible = previousLessonId > 0;
            btnNextLesson.Visible = nextLessonId > 0;
            btnMarkComplete.Visible = !ChefPreview && !isCompleted;
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
            if (ChefPreview || CurrentLessonId <= 0) return;

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
