using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Shared.CourseSearch
{
    public sealed class CourseActionEventArgs : EventArgs
    {
        public int CourseId { get; private set; }
        public CourseActionEventArgs(int courseId) { CourseId = courseId; }
    }

    public partial class ViewCourseOverivew : UserControl
    {
        private const int ReviewPageSize = 5;
        private int CourseId { get { return (int?)ViewState["CourseId"] ?? 0; } set { ViewState["CourseId"] = value; } }
        private int ReviewPage { get { return (int?)ViewState["ReviewPage"] ?? 0; } set { ViewState["ReviewPage"] = value; } }
        private bool IsEditingReview { get { return (bool?)ViewState["IsEditingReview"] ?? false; } set { ViewState["IsEditingReview"] = value; } }
        public bool IsLearner { get; set; }
        public event EventHandler BackRequested;
        public event EventHandler<CourseActionEventArgs> StartCourseRequested;

        public void LoadCourse(int courseId)
        {
            CourseId = courseId;
            LoadCourseDetails(courseId);
            LoadRecipe(courseId);
            LoadLessons(courseId);
            LoadQuizzes(courseId);
            LoadActivities(courseId);
            LoadReviews(courseId);
            LoadLearnerState(courseId);
        }

        private void LoadCourseDetails(int courseId)
        {
            DataTable rows = Read(@"SELECT c.CourseID,c.Title,c.Description,c.Thumbnail,c.DifficultyLevel,
                cat.CategoryName,u.FullName AS ChefName,fc.DishName AS FoodCultureName,
                (SELECT COUNT(*) FROM Lessons l WHERE l.CourseID=c.CourseID) AS LessonCount,
                (SELECT COUNT(*) FROM Quizzes q WHERE q.CourseID=c.CourseID) AS QuizCount,
                (SELECT COUNT(*) FROM PracticalActivities a WHERE a.CourseID=c.CourseID) AS ActivityCount
                FROM Courses c INNER JOIN Categories cat ON cat.CategoryID=c.CategoryID
                INNER JOIN Users u ON u.UserID=c.InstructorID
                LEFT JOIN FoodCultures fc ON fc.FoodCultureID=c.FoodCultureID
                WHERE c.CourseID=@courseId AND c.Status='Published'", Parameter("@courseId", courseId));
            if (rows.Rows.Count == 0) throw new HttpException(404, "This course is not available.");
            DataRow course = rows.Rows[0];
            litCourseTitle.Text = Convert.ToString(course["Title"]);
            litCategory.Text = Convert.ToString(course["CategoryName"]);
            litFoodCulture.Text = Convert.ToString(course["FoodCultureName"]);
            phFoodCulture.Visible = litFoodCulture.Text.Length > 0;
            litDifficulty.Text = Convert.ToString(course["DifficultyLevel"]);
            litChefName.Text = Convert.ToString(course["ChefName"]);
            litDescription.Text = Convert.ToString(course["Description"]);
            SetCount(litLessonCount, litLessonLabel, course["LessonCount"], "Lesson", "No Lessons");
            SetCount(litQuizCount, litQuizLabel, course["QuizCount"], "Quiz", "No Quiz");
            SetCount(litActivityCount, litActivityLabel, course["ActivityCount"], "Practical Activity", "No Practical Activity");
            string thumbnail = MediaUrl(Convert.ToString(course["Thumbnail"]));
            imgCourseThumbnail.ImageUrl = thumbnail;
            imgCourseThumbnail.AlternateText = Convert.ToString(course["Title"]) + " course thumbnail";
            imgCourseThumbnail.Visible = thumbnail.Length > 0;
            pnlCourseImagePlaceholder.Visible = thumbnail.Length == 0;
        }

        private static void SetCount(Literal countControl, Literal labelControl, object value, string singular, string zeroLabel)
        {
            int count = Convert.ToInt32(value);
            countControl.Text = count == 0 ? "" : Convert.ToString(count);
            labelControl.Text = count == 0 ? zeroLabel : singular + (count == 1 ? "" : "s");
        }

        private void LoadRecipe(int id) { Bind(rptRecipe, pnlRecipeEmpty, Read("SELECT TOP 1 * FROM CourseRecipes WHERE CourseID=@id ORDER BY RecipeID", Parameter("@id", id))); }
        private void LoadLessons(int id) { Bind(rptLessonOverview, pnlLessonsEmpty, Read("SELECT LessonOrder,Title FROM Lessons WHERE CourseID=@id ORDER BY LessonOrder,LessonID", Parameter("@id", id))); }
        private void LoadQuizzes(int id) { Bind(rptQuizOverview, pnlQuizEmpty, Read("SELECT q.Title,q.Description,q.PassingScore,(SELECT COUNT(*) FROM QuizQuestions qq WHERE qq.QuizID=q.QuizID) AS QuestionCount FROM Quizzes q WHERE q.CourseID=@id ORDER BY q.QuizID", Parameter("@id", id))); }
        private void LoadActivities(int id) { Bind(rptActivityOverview, pnlActivityEmpty, Read("SELECT Title,Instructions AS ShortInstructions FROM PracticalActivities WHERE CourseID=@id ORDER BY ActivityID", Parameter("@id", id))); }
        private static void Bind(Repeater repeater, Panel empty, DataTable data) { repeater.DataSource = data; repeater.DataBind(); empty.Visible = data.Rows.Count == 0; }

        private void LoadReviews(int courseId)
        {
            DataTable summary = Read(@"SELECT COUNT(*) AS ReviewCount,AVG(CAST(Rating AS DECIMAL(5,2))) AS AverageRating,
                SUM(CASE WHEN Rating=5 THEN 1 ELSE 0 END) AS FiveCount,SUM(CASE WHEN Rating=4 THEN 1 ELSE 0 END) AS FourCount,
                SUM(CASE WHEN Rating=3 THEN 1 ELSE 0 END) AS ThreeCount,SUM(CASE WHEN Rating=2 THEN 1 ELSE 0 END) AS TwoCount,
                SUM(CASE WHEN Rating=1 THEN 1 ELSE 0 END) AS OneCount FROM Reviews WHERE CourseID=@id", Parameter("@id", courseId));
            DataRow stats = summary.Rows[0];
            int total = Convert.ToInt32(stats["ReviewCount"]);
            pnlHasRatings.Visible = total > 0; pnlNoRatings.Visible = total == 0;
            if (total > 0)
            {
                decimal average = Convert.ToDecimal(stats["AverageRating"]);
                litAverageRating.Text = average.ToString("0.0"); litReviewCount.Text = Convert.ToString(total);
                litRatingStars.Text = StarText((int)Math.Round(average, MidpointRounding.AwayFromZero));
                DataTable bars = new DataTable(); bars.Columns.Add("Stars", typeof(int)); bars.Columns.Add("Count", typeof(int)); bars.Columns.Add("Percentage", typeof(int));
                string[] names = { "FiveCount", "FourCount", "ThreeCount", "TwoCount", "OneCount" };
                for (int stars = 5; stars >= 1; stars--) { int count = Convert.ToInt32(stats[names[5 - stars]]); bars.Rows.Add(stars, count, total == 0 ? 0 : (int)Math.Round(count * 100.0 / total)); }
                rptRatingBreakdown.DataSource = bars; rptRatingBreakdown.DataBind();
            }
            DataTable reviews = Read(@"SELECT r.ReviewID,r.Rating,r.Comment,r.CreatedAt,u.FullName AS LearnerName
                FROM Reviews r INNER JOIN Users u ON u.UserID=r.LearnerID WHERE r.CourseID=@id ORDER BY r.CreatedAt DESC,r.ReviewID DESC", Parameter("@id", courseId));
            pnlReviewsEmpty.Visible = reviews.Rows.Count == 0; pnlRecentReviewsEmpty.Visible = reviews.Rows.Count == 0;
            rptRecentReviews.DataSource = reviews.AsEnumerable().Take(3).CopyToDataTableOrEmpty(reviews); rptRecentReviews.DataBind();
            int pageCount = Math.Max(1, (reviews.Rows.Count + ReviewPageSize - 1) / ReviewPageSize); ReviewPage = Math.Min(ReviewPage, pageCount - 1);
            PagedDataSource page = new PagedDataSource { DataSource = reviews.DefaultView, AllowPaging = true, PageSize = ReviewPageSize, CurrentPageIndex = ReviewPage };
            rptAllReviews.DataSource = page; rptAllReviews.DataBind(); pnlReviewPager.Visible = reviews.Rows.Count > ReviewPageSize;
            btnReviewsPrevious.Enabled = ReviewPage > 0; btnReviewsNext.Enabled = ReviewPage < pageCount - 1; lblReviewPage.Text = string.Format("Page {0} of {1}", ReviewPage + 1, pageCount);
        }

        private void LoadLearnerState(int courseId)
        {
            btnStartCourse.Visible = true; btnStartCourse.Text = "Learn this Course"; lnkReviewAction.Visible = false; lblCompletionMessage.Visible = false; lnkReviewCourseContent.Visible = false;
            pnlReviewForm.Visible = pnlMyReview.Visible = false; pnlReviewUnavailable.Visible = true;
            litReviewUnavailable.Text = IsLearner ? "Complete this Course to leave a review." : "Sign in as a learner and complete this Course to leave a review.";
            int learnerId; if (!TryGetLearnerId(out learnerId)) return;
            DataTable progress = Read("SELECT Status FROM CourseProgress WHERE CourseID=@id AND LearnerID=@learnerId", Parameter("@id", courseId), Parameter("@learnerId", learnerId));
            if (progress.Rows.Count == 0) { btnStartCourse.Text = "Start Course"; return; }
            string status = Convert.ToString(progress.Rows[0]["Status"]);
            if (status != "Completed")
            {
                btnStartCourse.Text = status == "Submitted" ? "View Course" : (status == "In Progress" ? "Continue Course" : "Start Course");
                if (status == "Submitted") litReviewUnavailable.Text = "Your course is waiting for Chef approval. You can leave a review after it is completed.";
                return;
            }
            btnStartCourse.Visible = false; lnkReviewAction.Visible = true; lblCompletionMessage.Visible = true; lnkReviewCourseContent.Visible = true;
            lnkReviewCourseContent.NavigateUrl = ResolveUrl("~/Pages/Learner/CourseLearning/CourseLearningPage.aspx?courseId=" + courseId);
            DataTable own = Read("SELECT ReviewID,Rating,Comment,CreatedAt FROM Reviews WHERE CourseID=@id AND LearnerID=@learnerId", Parameter("@id", courseId), Parameter("@learnerId", learnerId));
            bool hasReview = own.Rows.Count > 0; lnkReviewAction.Text = hasReview ? "View My Review" : "Leave a Review"; pnlReviewUnavailable.Visible = false;
            if (!hasReview) { pnlReviewForm.Visible = true; btnSaveReview.Text = "Submit Review"; btnCancelReviewEdit.Visible = false; return; }
            DataRow review = own.Rows[0];
            if (IsEditingReview) { pnlReviewForm.Visible = true; btnSaveReview.Text = "Save Review"; btnCancelReviewEdit.Visible = true; if (!IsPostBack || string.IsNullOrEmpty(rblRating.SelectedValue)) { rblRating.SelectedValue = Convert.ToString(review["Rating"]); txtReviewComment.Text = Convert.ToString(review["Comment"]); } }
            else { pnlMyReview.Visible = true; litMyReviewStars.Text = StarText(review["Rating"]); litMyReviewComment.Text = Convert.ToString(review["Comment"]); litMyReviewDate.Text = ReviewDate(review["CreatedAt"]); }
        }

        private bool TryGetLearnerId(out int learnerId)
        {
            learnerId = 0;
            return IsLearner && string.Equals(Convert.ToString(Session["Role"]), "Learner", StringComparison.OrdinalIgnoreCase) && int.TryParse(Convert.ToString(Session["UserId"]), out learnerId) && learnerId > 0;
        }

        private bool CanReview(int learnerId)
        {
            return Read(@"SELECT c.CourseID FROM Courses c INNER JOIN CourseProgress cp ON cp.CourseID=c.CourseID
                INNER JOIN Users u ON u.UserID=cp.LearnerID WHERE c.CourseID=@id AND c.Status='Published' AND cp.LearnerID=@learnerId
                AND cp.Status='Completed' AND u.Role='Learner' AND u.AccountStatus='Active'", Parameter("@id", CourseId), Parameter("@learnerId", learnerId)).Rows.Count == 1;
        }

        protected void btnSaveReview_Click(object sender, EventArgs e)
        {
            Page.Validate("Review"); int learnerId, rating; string comment = txtReviewComment.Text.Trim();
            if (!Page.IsValid || !TryGetLearnerId(out learnerId) || !CanReview(learnerId) || !int.TryParse(rblRating.SelectedValue, out rating) || rating < 1 || rating > 5 || comment.Length == 0 || comment.Length > 2000) { ShowFeedback("Enter a rating from 1 to 5 and a review up to 2,000 characters."); ScrollToReview(); return; }
            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB()) using (SqlCommand command = connection.CreateCommand())
                {
                    const string eligible = " EXISTS(SELECT 1 FROM Courses c JOIN CourseProgress cp ON cp.CourseID=c.CourseID JOIN Users u ON u.UserID=cp.LearnerID WHERE c.CourseID=@id AND c.Status='Published' AND cp.LearnerID=@learnerId AND cp.Status='Completed' AND u.Role='Learner' AND u.AccountStatus='Active')";
                    command.CommandText = IsEditingReview
                        ? "UPDATE Reviews SET Rating=@rating,Comment=@comment WHERE CourseID=@id AND LearnerID=@learnerId AND" + eligible
                        : "INSERT INTO Reviews(CourseID,LearnerID,Rating,Comment) SELECT @id,@learnerId,@rating,@comment WHERE" + eligible + " AND NOT EXISTS(SELECT 1 FROM Reviews WHERE CourseID=@id AND LearnerID=@learnerId)";
                    command.Parameters.Add("@id", SqlDbType.Int).Value = CourseId; command.Parameters.Add("@learnerId", SqlDbType.Int).Value = learnerId; command.Parameters.Add("@rating", SqlDbType.Int).Value = rating; command.Parameters.Add("@comment", SqlDbType.NVarChar, -1).Value = comment;
                    connection.Open(); int changed = command.ExecuteNonQuery(); if (changed != 1) { ShowFeedback("Your existing review has been loaded instead."); IsEditingReview = false; ScrollToReview(); return; }
                }
                IsEditingReview = false; ShowFeedback("Your review has been saved."); ScrollToReview();
            }
            catch (SqlException ex) { if (ex.Number == 2601 || ex.Number == 2627) { IsEditingReview = false; ShowFeedback("You have already reviewed this course."); ScrollToReview(); } else throw; }
        }

        protected void btnDeleteReview_Click(object sender, EventArgs e)
        {
            int learnerId; if (!TryGetLearnerId(out learnerId) || !CanReview(learnerId)) return;
            using (SqlConnection connection = DatabaseManager.ConnectDB()) using (SqlCommand command = new SqlCommand(@"DELETE r FROM Reviews r JOIN Courses c ON c.CourseID=r.CourseID JOIN CourseProgress cp ON cp.CourseID=r.CourseID AND cp.LearnerID=r.LearnerID JOIN Users u ON u.UserID=r.LearnerID WHERE r.CourseID=@id AND r.LearnerID=@learnerId AND c.Status='Published' AND cp.Status='Completed' AND u.Role='Learner' AND u.AccountStatus='Active'", connection)) { command.Parameters.Add("@id", SqlDbType.Int).Value = CourseId; command.Parameters.Add("@learnerId", SqlDbType.Int).Value = learnerId; connection.Open(); command.ExecuteNonQuery(); }
            IsEditingReview = false; rblRating.ClearSelection(); txtReviewComment.Text = ""; ShowFeedback("Your review has been deleted."); ScrollToReview();
        }

        protected void btnEditReview_Click(object sender, EventArgs e) { int learnerId; if (TryGetLearnerId(out learnerId) && CanReview(learnerId)) { IsEditingReview = true; rblRating.ClearSelection(); ScrollToReview(); } }
        protected void btnCancelReviewEdit_Click(object sender, EventArgs e) { IsEditingReview = false; rblRating.ClearSelection(); txtReviewComment.Text = ""; ScrollToReview(); }
        protected void btnReviewsPrevious_Click(object sender, EventArgs e) { ReviewPage = Math.Max(0, ReviewPage - 1); ScrollToReviews(); }
        protected void btnReviewsNext_Click(object sender, EventArgs e) { ReviewPage++; ScrollToReviews(); }
        private void ShowFeedback(string text) { pnlReviewFeedback.Visible = true; litReviewFeedback.Text = text; }
        private void ScrollToReview() { ScriptManager.RegisterStartupScript(Page, Page.GetType(), "reviewAnchor", "location.hash='your-review';", true); }
        private void ScrollToReviews() { ScriptManager.RegisterStartupScript(Page, Page.GetType(), "reviewsAnchor", "location.hash='course-reviews';", true); }

        protected internal string StarText(object value) { int rating = Convert.ToInt32(value); return new string('★', rating) + new string('☆', 5 - rating); }
        protected internal string ReviewDate(object value) { return Convert.ToDateTime(value).ToString("d MMM yyyy"); }
        protected internal string ReviewDateValue(object value) { return Convert.ToDateTime(value).ToString("yyyy-MM-dd"); }
        public void ShowGuestLoginPrompt() { pnlGuestLogin.Visible = true; }
        protected void btnBackToSearch_Click(object sender, EventArgs e) { if (BackRequested != null) BackRequested(this, EventArgs.Empty); }
        protected void btnStartCourse_Click(object sender, EventArgs e) { if (!IsLearner) { ShowGuestLoginPrompt(); return; } if (StartCourseRequested != null) StartCourseRequested(this, new CourseActionEventArgs(CourseId)); }
        protected void btnCloseGuestPrompt_Click(object sender, EventArgs e) { pnlGuestLogin.Visible = false; }
    }

    internal static class ReviewTableExtensions
    {
        internal static DataTable CopyToDataTableOrEmpty(this System.Collections.Generic.IEnumerable<DataRow> rows, DataTable schema)
        {
            DataTable result = schema.Clone(); foreach (DataRow row in rows) result.ImportRow(row); return result;
        }
    }
}
