using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CookingLah.F_Services;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Learner.CourseLearning
{
    public partial class CourseLearningPage : Page
    {
        private int CourseId { get; set; }
        private int LearnerId { get; set; }
        private bool CourseAccessGranted { get; set; }
        private DataTable CourseLessons { get; set; }
        private int LastCompletedLessonOrder { get; set; }
        private bool ChefPreview { get; set; }
        protected bool IsChefPreviewMode { get { return ChefPreview; } }

        private int SelectedLessonId
        {
            get { return (int?)ViewState["SelectedLessonId"] ?? 0; }
            set { ViewState["SelectedLessonId"] = value; }
        }

        private string CurrentSection
        {
            get
            {
                string section = Convert.ToString(ViewState["CurrentSection"]);
                return string.IsNullOrEmpty(section) ? "Lesson" : section;
            }
            set { ViewState["CurrentSection"] = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            LessonContent1.PreviousLessonRequested += LessonContent_PreviousLessonRequested;
            LessonContent1.NextLessonRequested += LessonContent_NextLessonRequested;
            LessonContent1.LessonCompleted += LessonContent_LessonCompleted;
            QuizSection1.QuizPassed += QuizSection_QuizPassed;
            QuizSection1.ReviewLessonsRequested += QuizSection_ReviewLessonsRequested;
            QuizSection1.OpenActivityRequested += QuizSection_OpenActivityRequested;
            SubmitActivity1.ActivitySubmitted += SubmitActivity_ActivitySubmitted;

            int learnerId = 0;
            ChefPreview = string.Equals(Request.QueryString["preview"], "chef", StringComparison.OrdinalIgnoreCase);
            LessonContent1.ChefPreview = ChefPreview;
            if (ChefPreview)
            {
                int chefId;
                if (!int.TryParse(Convert.ToString(Session["UserId"]), out chefId) || chefId <= 0)
                {
                    RedirectToLogin();
                    return;
                }
                LearnerId = chefId;
                CourseId = GetCourseId();
                CourseAccessGranted = ValidateChefPreviewAccess(chefId, CourseId);
                return;
            }
            bool hasLearnerSession =
                string.Equals(Convert.ToString(Session["Role"]), "Learner", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(Convert.ToString(Session["UserId"]), out learnerId) &&
                learnerId > 0;

            if (!hasLearnerSession)
            {
                RedirectToLogin();
                return;
            }

            LearnerId = learnerId;
            CourseId = GetCourseId();
            if (CourseId <= 0)
            {
                CourseAccessGranted = false;
                return;
            }

            CourseAccessGranted = ValidateCourseAccess(LearnerId, CourseId);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (LearnerId <= 0) return;

            Form.Enctype = "multipart/form-data";
            if (!CourseAccessGranted)
            {
                ShowCourseUnavailable("You do not have access to this course, or it is no longer available.");
                return;
            }

            pnlLearningContent.Visible = true;
            pnlChefPreview.Visible = ChefPreview;
            pnlLearningStatus.Visible = !ChefPreview;
            lnkLearningBack.HRef = ChefPreview
                ? ResolveUrl("~/Pages/Chef/ManageCourse/ManageCoursePage.aspx?courseId=" + CourseId)
                : ResolveUrl("~/Pages/Learner/MyCourse/LearnerMyCourse.aspx");
            lnkLearningBack.Visible = !ChefPreview;
            if (!ChefPreview) TouchCourseProgress();
            if (!LoadCourseHeader())
            {
                ShowCourseUnavailable("This course is no longer available.");
                return;
            }
            LoadRecipe();

            LoadCurriculum();
            LoadLearningComponents();
            if (!IsPostBack)
            {
                RestoreCurrentSection();
            }

            if (!ChefPreview) RecalculateCourseProgress();
            UpdateCurriculumStatus();
            ApplyCurrentSection();
        }

        private int GetCourseId()
        {
            int courseId;
            if (!int.TryParse(Request.QueryString["courseId"], out courseId))
            {
                return 0;
            }

            return courseId;
        }

        private void RedirectToLogin()
        {
            Response.Redirect(ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private bool ValidateCourseAccess(int learnerId, int courseId)
        {
            DataTable access = Read(@"
                SELECT c.CourseID
                FROM Courses c
                INNER JOIN CourseProgress cp ON cp.CourseID=c.CourseID
                INNER JOIN Users u ON u.UserID=cp.LearnerID
                WHERE c.CourseID=@courseId
                  AND c.Status='Published'
                  AND cp.LearnerID=@learnerId
                  AND u.Role='Learner'
                  AND u.AccountStatus='Active'",
                Parameter("@courseId", courseId),
                Parameter("@learnerId", learnerId));

            return access.Rows.Count == 1;
        }

        private bool ValidateChefPreviewAccess(int chefId, int courseId)
        {
            if (courseId <= 0) return false;
            return Read(@"SELECT CourseID FROM Courses WHERE CourseID=@courseId AND InstructorID=@chefId
                    AND EXISTS (SELECT 1 FROM Users WHERE UserID=@chefId AND Role='Instructor' AND AccountStatus='Active')",
                Parameter("@courseId", courseId), Parameter("@chefId", chefId)).Rows.Count == 1;
        }

        private bool LoadCourseHeader()
        {
            if (ChefPreview)
            {
                DataTable previewRows = Read(@"SELECT c.Title,c.Description,c.Thumbnail,c.CreatedAt,cat.CategoryName,c.DifficultyLevel,
                    chef.FullName AS ChefName,fc.DishName AS TraditionalDish
                    FROM Courses c INNER JOIN Categories cat ON cat.CategoryID=c.CategoryID
                    INNER JOIN Users chef ON chef.UserID=c.InstructorID
                    LEFT JOIN FoodCultures fc ON fc.FoodCultureID=c.FoodCultureID
                    WHERE c.CourseID=@courseId AND c.InstructorID=@chefId",
                    Parameter("@courseId", CourseId), Parameter("@chefId", LearnerId));
                if (previewRows.Rows.Count == 0) return false;
                BindCourseHeader(previewRows.Rows[0]);
                courseStatusBadge.InnerText = "Chef preview";
                return true;
            }
            DataTable rows = Read(@"
                SELECT c.Title,c.Description,c.Thumbnail,c.CreatedAt,cat.CategoryName,c.DifficultyLevel,
                       chef.FullName AS ChefName,fc.DishName AS TraditionalDish,
                       cp.ProgressPercentage,cp.Status AS ProgressStatus,
                       cp.LastCompletedLessonOrder
                FROM Courses c
                INNER JOIN Categories cat ON cat.CategoryID=c.CategoryID
                INNER JOIN Users chef ON chef.UserID=c.InstructorID
                LEFT JOIN FoodCultures fc ON fc.FoodCultureID=c.FoodCultureID
                INNER JOIN Users learner ON learner.UserID=@learnerId
                INNER JOIN CourseProgress cp ON cp.CourseID=c.CourseID AND cp.LearnerID=learner.UserID
                WHERE c.CourseID=@courseId
                  AND c.Status='Published'
                  AND learner.Role='Learner'
                  AND learner.AccountStatus='Active'",
                Parameter("@courseId", CourseId),
                Parameter("@learnerId", LearnerId));

            if (rows.Rows.Count == 0) return false;

            DataRow course = rows.Rows[0];
            LastCompletedLessonOrder = Convert.ToInt32(course["LastCompletedLessonOrder"]);
            BindCourseHeader(course);
            int progressPercentage = Convert.ToInt32(course["ProgressPercentage"]);
            if (Convert.ToString(course["ProgressStatus"]) == "Completed") progressPercentage = 100;
            DisplayProgress(progressPercentage, Convert.ToString(course["ProgressStatus"]), 0, 0);
            return true;
        }

        private void BindCourseHeader(DataRow course)
        {
            litCourseTitle.Text = Convert.ToString(course["Title"]);
            litCompletionCourse.Text = "You've completed " + Convert.ToString(course["Title"]) + ".";
            string thumbnail = MediaUrl(Convert.ToString(course["Thumbnail"]));
            imgCourseThumbnail.ImageUrl = thumbnail;
            imgCourseThumbnail.AlternateText = Convert.ToString(course["Title"]) + " course thumbnail";
            imgCourseThumbnail.Visible = thumbnail.Length > 0;
            pnlCourseImagePlaceholder.Visible = !imgCourseThumbnail.Visible;
            litCourseCategory.Text = Convert.ToString(course["CategoryName"]);
            litCourseDifficulty.Text = Convert.ToString(course["DifficultyLevel"]);
            litCourseCreated.Text = Convert.ToDateTime(course["CreatedAt"]).ToString("dd MMM yyyy");
            string traditionalDish = Convert.ToString(course["TraditionalDish"]);
            phTraditionalDish.Visible = !string.IsNullOrWhiteSpace(traditionalDish);
            litTraditionalDish.Text = traditionalDish;
            litChefName.Text = Convert.ToString(course["ChefName"]);
            litCourseDescription.Text = Convert.ToString(course["Description"]);

        }

        private void LoadRecipe()
        {
            DataTable recipe = Read("SELECT TOP 1 Ingredients,CookingTime,ServingSize,RecipeImage FROM CourseRecipes WHERE CourseID=@courseId ORDER BY RecipeID",
                Parameter("@courseId", CourseId));
            pnlRecipeEmpty.Visible = recipe.Rows.Count == 0;
            rptRecipe.DataSource = recipe;
            rptRecipe.DataBind();
        }

        protected string FormatIngredients(object value)
        {
            return CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.FormatIngredients(value);
        }

        protected string RecipeIngredientsHtml(object value)
        {
            var ingredients = CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.ReadIngredients(Convert.ToString(value));
            if (ingredients.Length == 0) return "<p class=\"learning-ingredients-empty\">No ingredients listed.</p>";
            var html = new System.Text.StringBuilder("<ul class=\"learning-ingredients\">");
            foreach (var ingredient in ingredients)
            {
                html.Append("<li><span>").Append(HttpUtility.HtmlEncode(ingredient.name)).Append("</span><strong>")
                    .Append(HttpUtility.HtmlEncode(ingredient.amount)).Append("</strong></li>");
            }
            return html.Append("</ul>").ToString();
        }

        protected string RecipeMediaUrl(object value)
        {
            return CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(value));
        }

        protected string RecipeNumber(object value, string suffix)
        {
            return value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(Convert.ToString(value))
                ? "Not specified"
                : Convert.ToString(value) + suffix;
        }

        private void LoadCurriculum()
        {
            CourseLessons = Read(@"
                SELECT LessonID,LessonOrder,Title
                FROM Lessons
                WHERE CourseID=@courseId
                ORDER BY LessonOrder,LessonID",
                Parameter("@courseId", CourseId));

            int totalLessons = CourseLessons.Rows.Count;
            pnlLessonEmpty.Visible = totalLessons == 0;
            LessonContent1.Visible = totalLessons > 0;
            litLessonCount.Text = totalLessons == 1
                ? "1 lesson"
                : totalLessons + " lessons";

            if (!IsLessonAccessible(SelectedLessonId))
            {
                SelectedLessonId = GetNextIncompleteLessonId();
                if (SelectedLessonId == 0 && totalLessons > 0 &&
                    IsLessonAccessible(Convert.ToInt32(CourseLessons.Rows[totalLessons - 1]["LessonID"])))
                {
                    SelectedLessonId = Convert.ToInt32(CourseLessons.Rows[totalLessons - 1]["LessonID"]);
                }
            }

            rptCourseLessons.DataSource = CourseLessons;
            rptCourseLessons.DataBind();

            int completedCount = GetCompletedLessonCount();
            litProgressSummary.Text = completedCount == 0
                ? "Your lesson progress is saved to your account."
                : string.Format("{0} of {1} lessons completed.", completedCount, totalLessons);
        }

        private void LoadLearningComponents()
        {
            LessonContent1.ChefPreview = ChefPreview;
            if (SelectedLessonId > 0)
            {
                LessonContent1.LoadLesson(
                    CourseId,
                    LearnerId,
                    SelectedLessonId,
                    GetAdjacentLessonId(SelectedLessonId, false),
                    GetAdjacentLessonId(SelectedLessonId, true),
                    IsLessonComplete(SelectedLessonId));
            }
            else
            {
                LessonContent1.Visible = false;
            }

            QuizSection1.LoadQuiz(CourseId, LearnerId, ChefPreview);
            SubmitActivity1.LoadActivity(CourseId, LearnerId, ChefPreview);
        }

        private void TouchCourseProgress()
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                UPDATE CourseProgress
                SET LastAccessedAt=GETDATE(),
                    StartedAt=COALESCE(StartedAt,GETDATE()),
                    Status=CASE WHEN Status='Not Started' THEN 'In Progress' ELSE Status END
                WHERE LearnerID=@learnerId
                  AND CourseID=@courseId
                  AND EXISTS (
                      SELECT 1 FROM Courses c
                      INNER JOIN Users u ON u.UserID=@learnerId
                      WHERE c.CourseID=@courseId
                        AND c.Status='Published'
                        AND u.Role='Learner'
                        AND u.AccountStatus='Active')", connection))
            {
                command.Parameters.Add("@learnerId", SqlDbType.Int).Value = LearnerId;
                command.Parameters.Add("@courseId", SqlDbType.Int).Value = CourseId;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void RecalculateCourseProgress()
        {
            DataTable progressData = Read(@"
                SELECT cp.LastCompletedLessonOrder,
                       (SELECT COUNT(*)
                        FROM Lessons l
                        WHERE l.CourseID=cp.CourseID) AS TotalLessons,
                       (SELECT COUNT(*)
                        FROM Lessons l
                        WHERE l.CourseID=cp.CourseID
                          AND l.LessonOrder<=cp.LastCompletedLessonOrder) AS CompletedLessons,
                       (SELECT COUNT(*)
                        FROM Quizzes q
                        WHERE q.CourseID=cp.CourseID) AS TotalQuizzes,
                       (SELECT COUNT(DISTINCT q.QuizID)
                        FROM Quizzes q
                        INNER JOIN QuizAttempts qa ON qa.QuizID=q.QuizID
                        WHERE q.CourseID=cp.CourseID
                          AND qa.LearnerID=cp.LearnerID
                          AND qa.Passed=1) AS PassedQuizzes,
                       (SELECT COUNT(*)
                        FROM PracticalActivities a
                        WHERE a.CourseID=cp.CourseID) AS TotalActivities,
                       (SELECT COUNT(*)
                        FROM PracticalActivities a
                        WHERE a.CourseID=cp.CourseID
                          AND EXISTS (
                              SELECT 1
                              FROM ActivitySubmissions s
                              WHERE s.ActivityID=a.ActivityID
                                AND s.LearnerID=cp.LearnerID
                                AND s.Status='Approved')) AS ApprovedActivities
                FROM CourseProgress cp
                INNER JOIN Courses c ON c.CourseID=cp.CourseID
                INNER JOIN Users u ON u.UserID=cp.LearnerID
                WHERE cp.LearnerID=@learnerId
                  AND cp.CourseID=@courseId
                  AND c.Status='Published'
                  AND u.Role='Learner'
                  AND u.AccountStatus='Active'",
                Parameter("@learnerId", LearnerId),
                Parameter("@courseId", CourseId));

            if (progressData.Rows.Count == 0) return;

            DataRow row = progressData.Rows[0];
            LastCompletedLessonOrder = Convert.ToInt32(row["LastCompletedLessonOrder"]);

            int totalSteps = Convert.ToInt32(row["TotalLessons"])
                + Convert.ToInt32(row["TotalQuizzes"])
                + Convert.ToInt32(row["TotalActivities"]);
            int completedSteps = Convert.ToInt32(row["CompletedLessons"])
                + Convert.ToInt32(row["PassedQuizzes"])
                + Convert.ToInt32(row["ApprovedActivities"]);

            int percentage = totalSteps == 0
                ? 0
                : (int)Math.Round(completedSteps * 100.0 / totalSteps);

            using (SqlConnection connection = DatabaseManager.ConnectDB())
            {
                using (SqlCommand command = new SqlCommand(@"
                UPDATE CourseProgress
                SET ProgressPercentage=CASE
                        WHEN Status='Completed' THEN 100
                        ELSE @percentage
                    END,
                    Status=CASE
                        WHEN Status='Completed' THEN 'Completed'
                        WHEN Status='Submitted' THEN 'Submitted'
                        ELSE 'In Progress'
                    END,
                    CompletedAt=CASE
                        WHEN Status='Completed' THEN CompletedAt
                        ELSE NULL
                    END,
                    LastAccessedAt=GETDATE()
                WHERE LearnerID=@learnerId
                  AND CourseID=@courseId
                  AND EXISTS (
                      SELECT 1
                      FROM Courses c
                      INNER JOIN Users u ON u.UserID=@learnerId
                      WHERE c.CourseID=@courseId
                        AND c.Status='Published'
                        AND u.Role='Learner'
                        AND u.AccountStatus='Active')", connection))
                {
                    command.Parameters.Add("@percentage", SqlDbType.Int).Value = percentage;
                    command.Parameters.Add("@learnerId", SqlDbType.Int).Value = LearnerId;
                    command.Parameters.Add("@courseId", SqlDbType.Int).Value = CourseId;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            DataTable progress = Read(@"
                SELECT ProgressPercentage,Status
                FROM CourseProgress
                WHERE LearnerID=@learnerId AND CourseID=@courseId",
                Parameter("@learnerId", LearnerId),
                Parameter("@courseId", CourseId));

            if (progress.Rows.Count == 0) return;

            int savedPercentage = Convert.ToInt32(progress.Rows[0]["ProgressPercentage"]);
            if (Convert.ToString(progress.Rows[0]["Status"]) == "Completed") savedPercentage = 100;

            DisplayProgress(savedPercentage, Convert.ToString(progress.Rows[0]["Status"]), completedSteps, totalSteps);
        }

        private void DisplayProgress(int percentage, string status, int completedSteps, int totalSteps)
        {
            percentage = Math.Max(0, Math.Min(100, percentage));
            litProgressPercent.Text = Convert.ToString(percentage);
            bool isCompleted = status == "Completed";
            bool isSubmitted = status == "Submitted";
            courseStatusBadge.InnerText = isSubmitted ? "Submitted" : (isCompleted ? "Completed" : "In progress");
            courseStatusBadge.Attributes["class"] = isCompleted
                ? "learning-course-status is-completed"
                : "learning-course-status is-in-progress";
            courseProgressBar.Attributes["aria-valuenow"] = Convert.ToString(percentage);
            courseProgressFill.Style["width"] = percentage + "%";
            pnlCourseSubmitted.Visible = isSubmitted;
            pnlCourseComplete.Visible = isCompleted;
            if (isCompleted)
            {
                string overviewUrl = ResolveUrl("~/Pages/Learner/SearchCourse/SearchCoursePage.aspx?courseId=" + CourseId);
                lnkBackToOverview.NavigateUrl = overviewUrl;
                lnkCompletionReview.NavigateUrl = overviewUrl + "#your-review";
                DataTable review = Read("SELECT ReviewID FROM Reviews WHERE CourseID=@courseId AND LearnerID=@learnerId",
                    Parameter("@courseId", CourseId), Parameter("@learnerId", LearnerId));
                lnkCompletionReview.Text = review.Rows.Count > 0 ? "View My Review" : "Leave a Review";
            }

            if (totalSteps > 0)
            {
                litProgressSummary.Text = string.Format(
                    "{0} of {1} course steps completed. Progress is saved automatically.",
                    completedSteps,
                    totalSteps
                );
            }
            else
            {
                litProgressSummary.Text = "No lessons, quiz, or practical activity are available yet.";
            }
        }

        private void ShowCourseUnavailable(string message)
        {
            pnlLearningContent.Visible = false;
            pnlCourseUnavailable.Visible = true;
            litAccessMessage.Text = message;
        }

        protected string LessonItemClass(object lessonId)
        {
            int id = Convert.ToInt32(lessonId);
            if (!IsLessonAccessible(id)) return "curriculum-item is-locked";
            if (CurrentSection == "Lesson" && id == SelectedLessonId) return "curriculum-item is-active";
            return IsLessonComplete(id)
                ? "curriculum-item is-completed"
                : "curriculum-item";
        }

        protected string LessonStateIcon(object lessonId)
        {
            int id = Convert.ToInt32(lessonId);
            if (!IsLessonAccessible(id)) return "\uD83D\uDD12";
            if (IsLessonComplete(id)) return "\u2713";
            return CurrentSection == "Lesson" && id == SelectedLessonId ? "\u25CF" : "\u25CB";
        }

        protected bool IsLessonAccessible(object lessonId)
        {
            return IsLessonAccessible(Convert.ToInt32(lessonId));
        }

        private bool IsLessonAccessible(int lessonId)
        {
            int lessonOrder = GetLessonOrder(lessonId);
            return lessonOrder > 0 && (ChefPreview || lessonOrder <= LastCompletedLessonOrder + 1);
        }

        private bool IsQuizUnlocked()
        {
            return ChefPreview || (CourseLessons != null && GetCompletedLessonCount() == CourseLessons.Rows.Count);
        }

        private bool IsActivityUnlocked()
        {
            return ChefPreview || QuizSection1.HasPassed;
        }

        private bool ContainsLesson(int lessonId)
        {
            if (CourseLessons == null || lessonId <= 0) return false;

            foreach (DataRow lesson in CourseLessons.Rows)
            {
                if (Convert.ToInt32(lesson["LessonID"]) == lessonId) return true;
            }

            return false;
        }

        private int GetAdjacentLessonId(int lessonId, bool next)
        {
            for (int index = 0; index < CourseLessons.Rows.Count; index++)
            {
                if (Convert.ToInt32(CourseLessons.Rows[index]["LessonID"]) != lessonId) continue;

                int adjacentIndex = next ? index + 1 : index - 1;
                if (adjacentIndex < 0 || adjacentIndex >= CourseLessons.Rows.Count) return 0;

                int adjacentLessonId = Convert.ToInt32(CourseLessons.Rows[adjacentIndex]["LessonID"]);
                return IsLessonAccessible(adjacentLessonId) ? adjacentLessonId : 0;
            }

            return 0;
        }

        private bool IsLessonComplete(int lessonId)
        {
            if (CourseLessons == null || lessonId <= 0) return false;

            foreach (DataRow lesson in CourseLessons.Rows)
            {
                if (Convert.ToInt32(lesson["LessonID"]) != lessonId) continue;

                return Convert.ToInt32(lesson["LessonOrder"]) <= LastCompletedLessonOrder;
            }

            return false;
        }

        private int GetCompletedLessonCount()
        {
            if (CourseLessons == null) return 0;

            int completedCount = 0;
            foreach (DataRow lesson in CourseLessons.Rows)
            {
                if (Convert.ToInt32(lesson["LessonOrder"]) <= LastCompletedLessonOrder)
                {
                    completedCount++;
                }
            }

            return completedCount;
        }

        private int GetNextIncompleteLessonId()
        {
            if (CourseLessons == null) return 0;

            foreach (DataRow lesson in CourseLessons.Rows)
            {
                if (Convert.ToInt32(lesson["LessonOrder"]) == LastCompletedLessonOrder + 1)
                {
                    return Convert.ToInt32(lesson["LessonID"]);
                }
            }

            return 0;
        }

        private int GetLessonOrder(int lessonId)
        {
            if (CourseLessons == null) return 0;

            foreach (DataRow lesson in CourseLessons.Rows)
            {
                if (Convert.ToInt32(lesson["LessonID"]) == lessonId)
                {
                    return Convert.ToInt32(lesson["LessonOrder"]);
                }
            }

            return 0;
        }

        private int? MarkLessonComplete(int learnerId, int courseId, int lessonId, int lessonOrder)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                UPDATE cp
                SET LastCompletedLessonOrder=CASE
                        WHEN @lessonOrder>cp.LastCompletedLessonOrder THEN @lessonOrder
                        ELSE cp.LastCompletedLessonOrder
                    END,
                    LastAccessedAt=GETDATE()
                OUTPUT INSERTED.LastCompletedLessonOrder
                FROM CourseProgress cp
                WHERE cp.LearnerID=@learnerId
                  AND cp.CourseID=@courseId
                  AND @lessonOrder<=cp.LastCompletedLessonOrder+1
                  AND EXISTS (
                      SELECT 1
                      FROM Lessons l
                      INNER JOIN Courses c ON c.CourseID=l.CourseID
                      INNER JOIN Users u ON u.UserID=cp.LearnerID
                      WHERE l.LessonID=@lessonId
                        AND l.CourseID=cp.CourseID
                        AND l.LessonOrder=@lessonOrder
                        AND c.Status='Published'
                        AND u.Role='Learner'
                        AND u.AccountStatus='Active')", connection))
            {
                command.Parameters.Add("@learnerId", SqlDbType.Int).Value = learnerId;
                command.Parameters.Add("@courseId", SqlDbType.Int).Value = courseId;
                command.Parameters.Add("@lessonId", SqlDbType.Int).Value = lessonId;
                command.Parameters.Add("@lessonOrder", SqlDbType.Int).Value = lessonOrder;

                connection.Open();
                object savedOrder = command.ExecuteScalar();
                if (savedOrder == null || savedOrder == DBNull.Value) return null;

                return Convert.ToInt32(savedOrder);
            }
        }

        private void RestoreCurrentSection()
        {
            if (string.Equals(Request.QueryString["section"], "activity", StringComparison.OrdinalIgnoreCase) && SubmitActivity1.HasActivity && IsActivityUnlocked())
            {
                CurrentSection = "Activity";
                return;
            }

            int nextLessonId = GetNextIncompleteLessonId();
            if (nextLessonId > 0)
            {
                SelectedLessonId = nextLessonId;
                CurrentSection = "Lesson";
                return;
            }

            if (QuizSection1.HasQuiz && IsQuizUnlocked() && !QuizSection1.HasPassed)
            {
                CurrentSection = "Quiz";
                return;
            }

            if (SubmitActivity1.HasActivity && !SubmitActivity1.HasSubmission)
            {
                CurrentSection = "Activity";
                return;
            }

            if (CourseLessons != null && CourseLessons.Rows.Count > 0)
            {
                SelectedLessonId = Convert.ToInt32(CourseLessons.Rows[CourseLessons.Rows.Count - 1]["LessonID"]);
            }

            CurrentSection = SubmitActivity1.HasActivity
                ? "Activity"
                : QuizSection1.HasQuiz ? "Quiz" : "Lesson";
        }

        private void UpdateCurriculumStatus()
        {
            rptCourseLessons.DataSource = CourseLessons;
            rptCourseLessons.DataBind();

            litQuizState.Text = QuizSection1.HasPassed
                ? "\u2713"
                : !IsQuizUnlocked() ? "\uD83D\uDD12" : CurrentSection == "Quiz" ? "\u25CF" : "\u25CB";
            litActivityState.Text = SubmitActivity1.HasSubmission
                ? "\u2713"
                : !IsActivityUnlocked() ? "\uD83D\uDD12" : CurrentSection == "Activity" ? "\u25CF" : "\u25CB";

            btnOpenQuiz.Enabled = IsQuizUnlocked();
            btnOpenActivity.Enabled = IsActivityUnlocked();

            pnlQuizCurriculumItem.CssClass = !IsQuizUnlocked()
                ? "curriculum-special-item is-locked"
                : CurrentSection == "Quiz"
                ? "curriculum-special-item is-active"
                : QuizSection1.HasPassed ? "curriculum-special-item is-completed" : "curriculum-special-item";
            pnlActivityCurriculumItem.CssClass = !IsActivityUnlocked()
                ? "curriculum-special-item is-locked"
                : CurrentSection == "Activity"
                ? "curriculum-special-item is-active"
                : SubmitActivity1.HasSubmission ? "curriculum-special-item is-completed" : "curriculum-special-item";
        }

        private void ApplyCurrentSection()
        {
            if (CurrentSection == "Quiz" && !IsQuizUnlocked()) CurrentSection = "Lesson";
            if (CurrentSection == "Activity" && !IsActivityUnlocked())
                CurrentSection = QuizSection1.HasQuiz && IsQuizUnlocked() ? "Quiz" : "Lesson";
            pnlLesson.Visible = CurrentSection == "Lesson";
            pnlQuiz.Visible = CurrentSection == "Quiz";
            pnlActivity.Visible = CurrentSection == "Activity";
        }

        protected void rptCourseLessons_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "SelectLesson") return;

            int lessonId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out lessonId) || !IsLessonAccessible(lessonId))
            {
                return;
            }

            SelectedLessonId = lessonId;
            CurrentSection = "Lesson";
            RefreshLessonContent();
        }

        protected void btnOpenQuiz_Click(object sender, EventArgs e)
        {
            if (!IsQuizUnlocked()) return;
            CurrentSection = "Quiz";
            UpdateCurriculumStatus();
            ApplyCurrentSection();
        }

        protected void btnOpenActivity_Click(object sender, EventArgs e)
        {
            if (!IsActivityUnlocked()) return;
            CurrentSection = "Activity";
            UpdateCurriculumStatus();
            ApplyCurrentSection();
        }

        private void LessonContent_PreviousLessonRequested(object sender, EventArgs e)
        {
            SelectLessonFromControl();
        }

        private void LessonContent_NextLessonRequested(object sender, EventArgs e)
        {
            SelectLessonFromControl();
        }

        private void SelectLessonFromControl()
        {
            int requestedLessonId = LessonContent1.RequestedLessonId;
            if (!IsLessonAccessible(requestedLessonId)) return;

            SelectedLessonId = requestedLessonId;
            CurrentSection = "Lesson";
            RefreshLessonContent();
        }

        private void LessonContent_LessonCompleted(object sender, EventArgs e)
        {
            int completedLessonId = LessonContent1.CurrentLessonId;
            if (!ContainsLesson(completedLessonId)) return;

            int lessonOrder = GetLessonOrder(completedLessonId);
            if (lessonOrder <= 0) return;

            int? savedOrder = MarkLessonComplete(LearnerId, CourseId, completedLessonId, lessonOrder);
            if (!savedOrder.HasValue) return;

            LastCompletedLessonOrder = savedOrder.Value;
            RecalculateCourseProgress();
            RestoreCurrentSection();
            rptCourseLessons.DataSource = CourseLessons;
            rptCourseLessons.DataBind();
            RefreshLessonContent();
        }

        private void RefreshLessonContent()
        {
            LessonContent1.ChefPreview = ChefPreview;
            pnlLessonEmpty.Visible = CourseLessons.Rows.Count == 0;
            LessonContent1.Visible = SelectedLessonId > 0;
            if (SelectedLessonId > 0)
            {
                LessonContent1.LoadLesson(
                    CourseId,
                    LearnerId,
                    SelectedLessonId,
                    GetAdjacentLessonId(SelectedLessonId, false),
                    GetAdjacentLessonId(SelectedLessonId, true),
                    IsLessonComplete(SelectedLessonId));
            }

            rptCourseLessons.DataSource = CourseLessons;
            rptCourseLessons.DataBind();
            LoadCurriculumSummary();
            UpdateCurriculumStatus();
            ApplyCurrentSection();
        }

        private void LoadCurriculumSummary()
        {
            RecalculateCourseProgress();
        }

        private void QuizSection_QuizPassed(object sender, EventArgs e)
        {
            RecalculateCourseProgress();
            CurrentSection = "Quiz";
            UpdateCurriculumStatus();
            ApplyCurrentSection();
        }

        private void QuizSection_ReviewLessonsRequested(object sender, EventArgs e)
        {
            CurrentSection = "Lesson";
            UpdateCurriculumStatus();
            ApplyCurrentSection();
        }

        private void QuizSection_OpenActivityRequested(object sender, EventArgs e)
        {
            if (!IsActivityUnlocked()) return;
            CurrentSection = "Activity";
            UpdateCurriculumStatus();
            ApplyCurrentSection();
        }

        private void SubmitActivity_ActivitySubmitted(object sender, EventArgs e)
        {
            RecalculateCourseProgress();
            RestoreCurrentSection();
            UpdateCurriculumStatus();
            ApplyCurrentSection();
        }
    }
}
