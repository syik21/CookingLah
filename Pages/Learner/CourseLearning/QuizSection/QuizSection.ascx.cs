using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Learner.CourseLearning.QuizSection
{
    public partial class QuizSection : UserControl
    {
        public event EventHandler QuizPassed;
        public event EventHandler ReviewLessonsRequested;
        public event EventHandler OpenActivityRequested;

        private int CourseId
        {
            get { return (int?)ViewState["CourseId"] ?? 0; }
            set { ViewState["CourseId"] = value; }
        }

        private int LearnerId
        {
            get { return (int?)ViewState["LearnerId"] ?? 0; }
            set { ViewState["LearnerId"] = value; }
        }

        private int QuizId
        {
            get { return (int?)ViewState["QuizId"] ?? 0; }
            set { ViewState["QuizId"] = value; }
        }

        private int PassingScore
        {
            get { return (int?)ViewState["PassingScore"] ?? 0; }
            set { ViewState["PassingScore"] = value; }
        }

        private bool QuizStarted
        {
            get { return (bool?)ViewState["QuizStarted"] ?? false; }
            set { ViewState["QuizStarted"] = value; }
        }

        private bool ShowResult
        {
            get { return (bool?)ViewState["ShowResult"] ?? false; }
            set { ViewState["ShowResult"] = value; }
        }

        private bool ReviewCorrectAnswers
        {
            get { return (bool?)ViewState["ReviewCorrectAnswers"] ?? false; }
            set { ViewState["ReviewCorrectAnswers"] = value; }
        }

        private bool LastAttemptPassed
        {
            get { return (bool?)ViewState["LastAttemptPassed"] ?? false; }
            set { ViewState["LastAttemptPassed"] = value; }
        }

        private int LastAttemptScore
        {
            get { return (int?)ViewState["LastAttemptScore"] ?? 0; }
            set { ViewState["LastAttemptScore"] = value; }
        }

        private int LastAttemptCorrectCount
        {
            get { return (int?)ViewState["LastAttemptCorrectCount"] ?? 0; }
            set { ViewState["LastAttemptCorrectCount"] = value; }
        }

        public bool HasPassed { get; private set; }
        public bool HasQuiz { get; private set; }
        private bool ChefPreview
        {
            get { return (bool?)ViewState["ChefPreview"] ?? false; }
            set { ViewState["ChefPreview"] = value; }
        }

        public void LoadQuiz(int courseId, int learnerId, bool chefPreview = false)
        {
            CourseId = courseId;
            LearnerId = learnerId;
            ChefPreview = chefPreview;
            if (ChefPreview)
            {
                QuizStarted = true;
                ShowResult = false;
            }

            DataTable quizzes = Read(@"
                SELECT TOP 1 q.QuizID,q.Title,q.Description,q.PassingScore,
                       (SELECT COUNT(*) FROM QuizQuestions qq WHERE qq.QuizID=q.QuizID) AS QuestionCount
                FROM Quizzes q
                INNER JOIN Courses c ON c.CourseID=q.CourseID
                WHERE q.CourseID=@courseId
                  AND (c.Status='Published' OR (@preview=1 AND c.InstructorID=@learnerId))
                ORDER BY q.QuizID",
                Parameter("@courseId", courseId), Parameter("@preview", chefPreview), Parameter("@learnerId", learnerId));

            if (quizzes.Rows.Count == 0)
            {
                QuizId = 0;
                HasPassed = false;
                HasQuiz = false;
                pnlNoQuiz.Visible = true;
                pnlQuiz.Visible = false;
                return;
            }

            DataRow quiz = quizzes.Rows[0];
            HasQuiz = true;
            QuizId = Convert.ToInt32(quiz["QuizID"]);
            PassingScore = Convert.ToInt32(quiz["PassingScore"]);
            litQuizTitle.Text = Convert.ToString(quiz["Title"]);
            litQuizDescription.Text = Convert.ToString(quiz["Description"]);
            litQuizQuestionCount.Text = Convert.ToString(quiz["QuestionCount"]);
            litPassingScore.Text = Convert.ToString(PassingScore);
            litQuizPassingScoreResult.Text = Convert.ToString(PassingScore);

            HasPassed = LoadLatestPassedState();
            if (HasPassed && !ChefPreview && !ReviewCorrectAnswers)
            {
                QuizStarted = false;
                ShowResult = true;
            }
            pnlNoQuiz.Visible = false;
            pnlQuiz.Visible = true;
            pnlQuizIntro.Visible = !QuizStarted && !ShowResult;
            pnlQuizQuestions.Visible = QuizStarted && !ShowResult;
            pnlQuizResult.Visible = ShowResult;
            bool readOnlyQuestions = ChefPreview || ReviewCorrectAnswers;
            btnStartQuiz.Visible = !ChefPreview && !HasPassed;
            btnSubmitQuiz.Visible = !readOnlyQuestions;
            btnPreviewNextQuestion.Visible = readOnlyQuestions;
            btnBackToQuizResult.Visible = ReviewCorrectAnswers;

            if (pnlQuizQuestions.Visible)
            {
                BindQuizQuestions();
            }

            if (ShowResult)
            {
                DisplayQuizResult();
            }
        }

        private bool LoadLatestPassedState()
        {
            DataTable attempts = Read(@"
                SELECT TOP 1 Score,TotalQuestions
                FROM QuizAttempts
                WHERE QuizID=@quizId AND LearnerID=@learnerId AND Passed=1",
                Parameter("@quizId", QuizId),
                Parameter("@learnerId", LearnerId));

            if (attempts.Rows.Count == 0) return false;
            LastAttemptPassed = true;
            LastAttemptScore = Convert.ToInt32(attempts.Rows[0]["Score"]);
            int totalQuestions = Convert.ToInt32(attempts.Rows[0]["TotalQuestions"]);
            LastAttemptCorrectCount = (int)Math.Round(LastAttemptScore * totalQuestions / 100.0);
            return true;
        }

        private void BindQuizQuestions()
        {
            DataTable questions = Read(@"
                SELECT QuestionID,QuestionText,QuestionOrder
                FROM QuizQuestions
                WHERE QuizID=@quizId
                ORDER BY QuestionOrder,QuestionID",
                Parameter("@quizId", QuizId));

            rptQuizQuestions.DataSource = questions;
            rptQuizQuestions.DataBind();
            litQuizQuestionCount.Text = Convert.ToString(questions.Rows.Count);
        }

        protected void rptQuizQuestions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            DataRowView question = (DataRowView)e.Item.DataItem;
            Repeater optionsRepeater = (Repeater)e.Item.FindControl("rptQuestionOptions");
            optionsRepeater.DataSource = Read(@"
                SELECT OptionID,QuestionID,OptionText
                FROM QuizOptions
                WHERE QuestionID=@questionId
                  AND (@correctOnly=0 OR IsCorrect=1)
                ORDER BY OptionID",
                Parameter("@questionId", question["QuestionID"]), Parameter("@correctOnly", ReviewCorrectAnswers));
            optionsRepeater.DataBind();
        }

        protected bool IsAnswerSelected(object questionId, object optionId)
        {
            string selectedOption = Request.Form["answer_" + Convert.ToString(questionId)];
            return selectedOption == Convert.ToString(optionId);
        }

        protected string AnswerInputAttributes(object questionId, object optionId)
        {
            if (ReviewCorrectAnswers) return "checked=\"checked\" disabled=\"disabled\"";
            return IsAnswerSelected(questionId, optionId) ? "checked=\"checked\"" : "";
        }

        protected void btnStartQuiz_Click(object sender, EventArgs e)
        {
            if (ChefPreview || HasPassed) return;
            if (QuizId <= 0 || !ValidateQuizAccess()) return;

            QuizStarted = true;
            ShowResult = false;
            pnlQuizIntro.Visible = false;
            pnlQuizQuestions.Visible = true;
            pnlQuizResult.Visible = false;
            BindQuizQuestions();
        }

        protected void btnSubmitQuiz_Click(object sender, EventArgs e)
        {
            if (ChefPreview) return;
            if (!ValidateQuizAccess())
            {
                ShowFeedback("This quiz is no longer available to your account.");
                return;
            }

            int totalQuestions;
            int correctAnswers;
            int score;
            if (!CalculateScore(out totalQuestions, out correctAnswers, out score))
            {
                return;
            }

            bool passed = score >= PassingScore;
            if (!SaveQuizAttempt(score, totalQuestions, passed))
            {
                ShowFeedback("Unable to submit the quiz. Please try again.");
                return;
            }

            LastAttemptPassed = passed;
            HasPassed = HasPassed || passed;
            LastAttemptScore = score;
            LastAttemptCorrectCount = correctAnswers;
            ReviewCorrectAnswers = false;
            ShowResult = true;
            pnlQuizQuestions.Visible = false;
            pnlQuizIntro.Visible = false;
            pnlQuizResult.Visible = true;
            DisplayQuizResult();
            pnlQuizFeedback.Visible = false;

            if (passed)
            {
                EventHandler handler = QuizPassed;
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }

        private bool CalculateScore(out int totalQuestions, out int correctAnswers, out int score)
        {
            DataTable questions = Read(@"
                SELECT QuestionID,QuestionOrder
                FROM QuizQuestions
                WHERE QuizID=@quizId
                ORDER BY QuestionOrder,QuestionID",
                Parameter("@quizId", QuizId));

            totalQuestions = questions.Rows.Count;
            correctAnswers = 0;
            score = 0;

            if (totalQuestions == 0)
            {
                ShowFeedback("This quiz does not have any questions yet.");
                return false;
            }

            foreach (DataRow question in questions.Rows)
            {
                int questionId = Convert.ToInt32(question["QuestionID"]);
                int optionId;
                if (!int.TryParse(Request.Form["answer_" + questionId], out optionId))
                {
                    ShowFeedback("Please answer Question " + Convert.ToString(question["QuestionOrder"]) + " before submitting.");
                    return false;
                }

                DataTable selectedOption = Read(@"
                    SELECT o.IsCorrect
                    FROM QuizOptions o
                    INNER JOIN QuizQuestions q ON q.QuestionID=o.QuestionID
                    WHERE q.QuizID=@quizId
                      AND q.QuestionID=@questionId
                      AND o.OptionID=@optionId",
                    Parameter("@quizId", QuizId),
                    Parameter("@questionId", questionId),
                    Parameter("@optionId", optionId));

                if (selectedOption.Rows.Count == 0)
                {
                    ShowFeedback("One of your selected answers is no longer available. Please reload the quiz.");
                    return false;
                }

                if (Convert.ToBoolean(selectedOption.Rows[0]["IsCorrect"]))
                {
                    correctAnswers++;
                }
            }

            score = (int)Math.Round(correctAnswers * 100.0 / totalQuestions);
            return true;
        }

        private bool ValidateQuizAccess()
        {
            DataTable access = Read(@"
                SELECT q.QuizID
                FROM Quizzes q
                INNER JOIN Courses c ON c.CourseID=q.CourseID
                INNER JOIN CourseProgress cp ON cp.CourseID=c.CourseID
                INNER JOIN Users u ON u.UserID=cp.LearnerID
                WHERE q.QuizID=@quizId
                  AND q.CourseID=@courseId
                  AND c.Status='Published'
                  AND cp.LearnerID=@learnerId
                  AND u.Role='Learner'
                  AND u.AccountStatus='Active'
                  AND NOT EXISTS (
                      SELECT 1 FROM Lessons l
                      WHERE l.CourseID=cp.CourseID
                        AND l.LessonOrder>cp.LastCompletedLessonOrder)",
                Parameter("@quizId", QuizId),
                Parameter("@courseId", CourseId),
                Parameter("@learnerId", LearnerId));

            return access.Rows.Count == 1;
        }

        private bool SaveQuizAttempt(int score, int totalQuestions, bool passed)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                INSERT INTO QuizAttempts (QuizID,LearnerID,Score,TotalQuestions,Passed)
                SELECT @quizId,@learnerId,@score,@totalQuestions,@passed
                WHERE EXISTS (
                    SELECT 1
                    FROM Quizzes q
                    INNER JOIN Courses c ON c.CourseID=q.CourseID
                    INNER JOIN CourseProgress cp ON cp.CourseID=c.CourseID
                    INNER JOIN Users u ON u.UserID=cp.LearnerID
                    WHERE q.QuizID=@quizId
                      AND q.CourseID=@courseId
                      AND c.Status='Published'
                      AND cp.LearnerID=@learnerId
                      AND u.Role='Learner'
                      AND u.AccountStatus='Active'
                      AND NOT EXISTS (
                          SELECT 1 FROM Lessons l
                          WHERE l.CourseID=cp.CourseID
                            AND l.LessonOrder>cp.LastCompletedLessonOrder))", connection))
            {
                command.Parameters.Add("@quizId", SqlDbType.Int).Value = QuizId;
                command.Parameters.Add("@learnerId", SqlDbType.Int).Value = LearnerId;
                command.Parameters.Add("@score", SqlDbType.Int).Value = score;
                command.Parameters.Add("@totalQuestions", SqlDbType.Int).Value = totalQuestions;
                command.Parameters.Add("@passed", SqlDbType.Bit).Value = passed;
                command.Parameters.Add("@courseId", SqlDbType.Int).Value = CourseId;

                connection.Open();
                return command.ExecuteNonQuery() == 1;
            }
        }

        private void DisplayQuizResult()
        {
            litQuizResultHeading.Text = LastAttemptPassed ? "You passed!" : "Keep practicing";
            litQuizScore.Text = Convert.ToString(LastAttemptScore);
            litQuizCorrectCount.Text = string.Format("{0} of {1}", LastAttemptCorrectCount, litQuizQuestionCount.Text);
            litQuizPassingScoreResult.Text = Convert.ToString(PassingScore);
            pnlQuizPassedMessage.Visible = LastAttemptPassed;
            pnlQuizFailedMessage.Visible = !LastAttemptPassed;
            btnReviewLessons.Visible = !LastAttemptPassed;
            btnTryAgain.Visible = !LastAttemptPassed;
            btnReviewAnswers.Visible = LastAttemptPassed;
            btnContinueActivity.Visible = LastAttemptPassed;
        }

        private void ShowFeedback(string message)
        {
            litQuizFeedback.Text = message;
            pnlQuizFeedback.Visible = true;
            pnlQuizQuestions.Visible = true;
            BindQuizQuestions();
        }

        protected void btnTryAgain_Click(object sender, EventArgs e)
        {
            QuizStarted = true;
            ShowResult = false;
            pnlQuizResult.Visible = false;
            pnlQuizQuestions.Visible = true;
            BindQuizQuestions();
        }

        protected void btnReviewAnswers_Click(object sender, EventArgs e)
        {
            if (!HasPassed || ChefPreview) return;
            ReviewCorrectAnswers = true;
            QuizStarted = true;
            ShowResult = false;
            pnlQuizResult.Visible = false;
            pnlQuizIntro.Visible = false;
            pnlQuizQuestions.Visible = true;
            btnSubmitQuiz.Visible = false;
            btnPreviewNextQuestion.Visible = true;
            btnBackToQuizResult.Visible = true;
            BindQuizQuestions();
        }

        protected void btnBackToQuizResult_Click(object sender, EventArgs e)
        {
            if (!HasPassed || ChefPreview) return;
            ReviewCorrectAnswers = false;
            QuizStarted = false;
            ShowResult = true;
            pnlQuizQuestions.Visible = false;
            pnlQuizIntro.Visible = false;
            pnlQuizResult.Visible = true;
            btnBackToQuizResult.Visible = false;
            DisplayQuizResult();
        }

        protected void btnReviewLessons_Click(object sender, EventArgs e)
        {
            EventHandler handler = ReviewLessonsRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected void btnContinueActivity_Click(object sender, EventArgs e)
        {
            EventHandler handler = OpenActivityRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
