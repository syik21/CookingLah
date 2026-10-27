using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Learner.CourseLearning.SubmitActivity
{
    public partial class SubmitActivity : UserControl
    {
        public event EventHandler ActivitySubmitted;

        private int CourseId { get; set; }
        private int LearnerId { get; set; }
        private int ActivityId { get; set; }
        public bool HasSubmission { get; private set; }
        public bool HasActivity { get; private set; }

        public void LoadActivity(int courseId, int learnerId)
        {
            CourseId = courseId;
            LearnerId = learnerId;

            DataTable activities = Read(@"
                SELECT TOP 1 a.ActivityID,a.Title,a.Instructions
                FROM PracticalActivities a
                INNER JOIN Courses c ON c.CourseID=a.CourseID
                WHERE a.CourseID=@courseId AND c.Status='Published'
                ORDER BY a.ActivityID",
                Parameter("@courseId", courseId));

            if (activities.Rows.Count == 0)
            {
                ActivityId = 0;
                HasSubmission = false;
                HasActivity = false;
                pnlActivityUnavailable.Visible = true;
                pnlActivity.Visible = false;
                return;
            }

            DataRow activity = activities.Rows[0];
            HasActivity = true;
            ActivityId = Convert.ToInt32(activity["ActivityID"]);
            litActivityTitle.Text = Convert.ToString(activity["Title"]);
            litActivityInstructions.Text = Convert.ToString(activity["Instructions"]);
            litSubmissionFormTitle.Text = "Submit your work";
            pnlActivityUnavailable.Visible = false;
            pnlActivity.Visible = true;

            LoadExistingSubmission();
        }

        private void LoadExistingSubmission()
        {
            DataTable submissions = Read(@"
                SELECT TOP 1 ImagePath,Description,Status,InstructorFeedback,SubmittedAt,ReviewedAt
                FROM ActivitySubmissions
                WHERE ActivityID=@activityId AND LearnerID=@learnerId
                ORDER BY SubmittedAt DESC,SubmissionID DESC",
                Parameter("@activityId", ActivityId),
                Parameter("@learnerId", LearnerId));

            HasSubmission = submissions.Rows.Count > 0;
            pnlExistingSubmission.Visible = HasSubmission;
            if (!HasSubmission)
            {
                litSubmissionFormTitle.Text = "Submit your work";
                pnlSubmissionForm.Visible = true;
                return;
            }

            DataRow submission = submissions.Rows[0];
            string imageUrl = MediaUrl(Convert.ToString(submission["ImagePath"]));
            imgSubmittedWork.ImageUrl = imageUrl;
            imgSubmittedWork.AlternateText = "Your submitted practical activity";
            imgSubmittedWork.Visible = imageUrl.Length > 0;
            litSubmissionStatus.Text = Convert.ToString(submission["Status"]);
            litSubmissionDate.Text = Convert.ToDateTime(submission["SubmittedAt"]).ToString("dd MMM yyyy");
            litSubmissionNotes.Text = Convert.ToString(submission["Description"]);

            string feedback = Convert.ToString(submission["InstructorFeedback"]);
            pnlInstructorFeedback.Visible = !string.IsNullOrWhiteSpace(feedback);
            litInstructorFeedback.Text = feedback;
            litReviewedDate.Text = submission["ReviewedAt"] == DBNull.Value
                ? ""
                : Convert.ToDateTime(submission["ReviewedAt"]).ToString("dd MMM yyyy");

            string status = Convert.ToString(submission["Status"]);
            pnlSubmissionForm.Visible = status == "Resubmit Required";
            litSubmissionFormTitle.Text = "Resubmit your work";
            if (!Page.IsPostBack)
            {
                txtActivityDescription.Text = Convert.ToString(submission["Description"]);
            }
        }

        protected void btnSubmitActivity_Click(object sender, EventArgs e)
        {
            string description = txtActivityDescription.Text.Trim();
            if (description.Length > 1000)
            {
                ShowFeedback("Keep your notes to 1,000 characters or fewer.");
                return;
            }

            if (!ActivityBelongsToLearnerCourse())
            {
                ShowFeedback("This activity is no longer available to your account.");
                return;
            }

            string existingImagePath = LoadExistingImagePath();
            if (string.IsNullOrWhiteSpace(existingImagePath) && !fuActivityImage.HasFile)
            {
                ShowFeedback("Choose a photo of your finished dish before submitting.");
                return;
            }

            string imagePath;
            try
            {
                imagePath = SaveUpload(fuActivityImage, existingImagePath);
            }
            catch (InvalidOperationException ex)
            {
                ShowFeedback(ex.Message);
                return;
            }
            catch (IOException)
            {
                ShowFeedback("The image could not be saved. Please choose it again.");
                return;
            }
            catch (UnauthorizedAccessException)
            {
                ShowFeedback("The image could not be saved. Please contact the administrator.");
                return;
            }

            if (!SaveSubmission(imagePath, description))
            {
                ShowFeedback("Unable to submit your activity. Please try again.");
                return;
            }

            txtActivityDescription.Text = description;
            LoadExistingSubmission();
            litActivityFeedback.Text = "Activity submitted successfully.";
            pnlActivityFeedback.Visible = true;

            EventHandler handler = ActivitySubmitted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private bool ActivityBelongsToLearnerCourse()
        {
            DataTable access = Read(@"
                SELECT a.ActivityID
                FROM PracticalActivities a
                INNER JOIN Courses c ON c.CourseID=a.CourseID
                INNER JOIN CourseProgress cp ON cp.CourseID=c.CourseID
                INNER JOIN Users u ON u.UserID=cp.LearnerID
                WHERE a.ActivityID=@activityId
                  AND a.CourseID=@courseId
                  AND c.Status='Published'
                  AND cp.LearnerID=@learnerId
                  AND u.Role='Learner'
                  AND u.AccountStatus='Active'",
                Parameter("@activityId", ActivityId),
                Parameter("@courseId", CourseId),
                Parameter("@learnerId", LearnerId));

            return access.Rows.Count == 1;
        }

        private string LoadExistingImagePath()
        {
            DataTable submissions = Read(@"
                SELECT TOP 1 ImagePath
                FROM ActivitySubmissions
                WHERE ActivityID=@activityId AND LearnerID=@learnerId
                ORDER BY SubmittedAt DESC,SubmissionID DESC",
                Parameter("@activityId", ActivityId),
                Parameter("@learnerId", LearnerId));

            return submissions.Rows.Count == 0
                ? ""
                : Convert.ToString(submissions.Rows[0]["ImagePath"]);
        }

        private bool SaveSubmission(string imagePath, string description)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    object allowed = Execute(connection, transaction, @"
                        SELECT a.ActivityID
                        FROM PracticalActivities a WITH (UPDLOCK,HOLDLOCK)
                        INNER JOIN Courses c ON c.CourseID=a.CourseID
                        INNER JOIN CourseProgress cp ON cp.CourseID=c.CourseID
                        INNER JOIN Users u ON u.UserID=cp.LearnerID
                        WHERE a.ActivityID=@activityId
                          AND a.CourseID=@courseId
                          AND c.Status='Published'
                          AND cp.LearnerID=@learnerId
                          AND u.Role='Learner'
                          AND u.AccountStatus='Active'",
                        Parameter("@activityId", ActivityId),
                        Parameter("@courseId", CourseId),
                        Parameter("@learnerId", LearnerId));

                    if (allowed == null)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    object submissionId = Execute(connection, transaction, @"
                        SELECT TOP 1 SubmissionID
                        FROM ActivitySubmissions WITH (UPDLOCK,HOLDLOCK)
                        WHERE ActivityID=@activityId AND LearnerID=@learnerId
                        ORDER BY SubmittedAt DESC,SubmissionID DESC",
                        Parameter("@activityId", ActivityId),
                        Parameter("@learnerId", LearnerId));

                    if (submissionId == null)
                    {
                        Execute(connection, transaction, @"
                            INSERT INTO ActivitySubmissions
                                (ActivityID,LearnerID,ImagePath,Description,Status)
                            VALUES (@activityId,@learnerId,@imagePath,@description,'Submitted')",
                            Parameter("@activityId", ActivityId),
                            Parameter("@learnerId", LearnerId),
                            Parameter("@imagePath", imagePath),
                            Parameter("@description", description));
                    }
                    else
                    {
                        Execute(connection, transaction, @"
                            UPDATE ActivitySubmissions
                            SET ImagePath=@imagePath,
                                Description=@description,
                                Status='Submitted',
                                InstructorFeedback=NULL,
                                ReviewedBy=NULL,
                                SubmittedAt=GETDATE(),
                                ReviewedAt=NULL
                            WHERE SubmissionID=@submissionId
                              AND ActivityID=@activityId
                              AND LearnerID=@learnerId",
                            Parameter("@imagePath", imagePath),
                            Parameter("@description", description),
                            Parameter("@submissionId", submissionId),
                            Parameter("@activityId", ActivityId),
                            Parameter("@learnerId", LearnerId));
                    }

                    object updatedProgress = Execute(connection, transaction, @"
                        UPDATE cp
                        SET cp.Status='Submitted',
                            cp.CompletedAt=NULL,
                            cp.LastAccessedAt=GETDATE()
                        FROM CourseProgress cp
                        WHERE cp.LearnerID=@learnerId
                          AND cp.CourseID=@courseId
                          AND cp.Status<>'Completed'
                          AND NOT EXISTS (
                              SELECT 1 FROM Lessons l
                              WHERE l.CourseID=cp.CourseID
                                AND l.LessonOrder>cp.LastCompletedLessonOrder)
                          AND NOT EXISTS (
                              SELECT 1 FROM Quizzes q
                              WHERE q.CourseID=cp.CourseID
                                AND NOT EXISTS (
                                    SELECT 1 FROM QuizAttempts qa
                                    WHERE qa.QuizID=q.QuizID
                                      AND qa.LearnerID=cp.LearnerID
                                      AND qa.Passed=1));
                        SELECT @@ROWCOUNT;",
                        Parameter("@learnerId", LearnerId),
                        Parameter("@courseId", CourseId));

                    if (Convert.ToInt32(updatedProgress) != 1)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    transaction.Commit();
                    return true;
                }
            }
        }

        private void ShowFeedback(string message)
        {
            litActivityFeedback.Text = message;
            pnlActivityFeedback.Visible = true;
        }
    }
}
