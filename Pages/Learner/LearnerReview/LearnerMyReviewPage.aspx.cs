using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Learner.LearnerReview
{
    public partial class LearnerMyReviewPage : Page
    {
        private const int PageSize = 5;
        private const int MaximumReviewLength = 2000;

        private int LearnerId { get; set; }

        private int CurrentPage
        {
            get { return (int?)ViewState["CurrentPage"] ?? 0; }
            set { ViewState["CurrentPage"] = value; }
        }

        private int EditingReviewId
        {
            get { return (int?)ViewState["EditingReviewId"] ?? 0; }
            set { ViewState["EditingReviewId"] = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            LearnerId = GetCurrentLearnerId();

            if (LearnerId <= 0)
            {
                RedirectToLogin();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (LearnerId <= 0)
            {
                return;
            }

            LoadReviewStatistics();
            LoadReviews();
        }

        // Verify the learner session and active account.
        private int GetCurrentLearnerId()
        {
            int learnerId = 0;
            bool hasLearnerSession =
                string.Equals(
                    Convert.ToString(Session["Role"]),
                    "Learner",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                int.TryParse(Convert.ToString(Session["UserId"]), out learnerId) &&
                learnerId > 0;

            if (!hasLearnerSession)
            {
                return 0;
            }

            DataTable learner = Read(@"
                SELECT UserID
                FROM Users
                WHERE UserID=@learnerId
                  AND Role='Learner'
                  AND AccountStatus='Active'",
                Parameter("@learnerId", learnerId));

            return learner.Rows.Count == 1 ? learnerId : 0;
        }

        private void RedirectToLogin()
        {
            Response.Redirect(
                ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login"),
                false
            );
            Context.ApplicationInstance.CompleteRequest();
        }

        // Calculate the learner's review count and average rating.
        private void LoadReviewStatistics()
        {
            DataTable statistics = Read(@"
                SELECT
                    COUNT(ReviewID) AS ReviewCount,
                    AVG(CAST(Rating AS DECIMAL(5,2))) AS AverageRating
                FROM Reviews
                WHERE LearnerID=@learnerId",
                Parameter("@learnerId", LearnerId));

            DataRow row = statistics.Rows[0];
            int reviewCount = Convert.ToInt32(row["ReviewCount"]);

            litReviewsWritten.Text = Convert.ToString(reviewCount);
            litAverageRating.Text = reviewCount == 0
                ? "—"
                : Convert.ToDecimal(row["AverageRating"]).ToString("0.0");
        }

        // Load reviews written by the current learner.
        private void LoadReviews()
        {
            int rating = GetSelectedRating();
            string searchText = txtSearch.Text.Trim();
            string orderBy = GetReviewOrderBy();

            string query = @"
                SELECT
                    r.ReviewID,
                    r.CourseID,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    c.Title,
                    c.Thumbnail,
                    c.DifficultyLevel,
                    cat.CategoryName
                FROM Reviews r
                INNER JOIN Courses c
                    ON c.CourseID=r.CourseID
                INNER JOIN Categories cat
                    ON cat.CategoryID=c.CategoryID
                WHERE r.LearnerID=@learnerId
                  AND (@rating=0 OR r.Rating=@rating)
                  AND (@searchText='' OR c.Title LIKE '%' + @searchText + '%')
                ORDER BY " + orderBy;

            DataTable reviews = Read(
                query,
                Parameter("@learnerId", LearnerId),
                Parameter("@rating", rating),
                Parameter("@searchText", searchText)
            );

            BindReviews(reviews);
        }

        // Bind the selected review page and update empty states.
        private void BindReviews(DataTable reviews)
        {
            int totalReviews = reviews.Rows.Count;
            int totalWritten = GetTotalReviewCount();
            int pageCount = Math.Max(1, (totalReviews + PageSize - 1) / PageSize);

            CurrentPage = Math.Min(CurrentPage, pageCount - 1);

            PagedDataSource page = new PagedDataSource
            {
                DataSource = reviews.DefaultView,
                AllowPaging = true,
                PageSize = PageSize,
                CurrentPageIndex = CurrentPage
            };

            rptReviews.DataSource = page;
            rptReviews.DataBind();

            pnlReviewResults.Visible = totalReviews > 0;
            pnlNoReviews.Visible = totalWritten == 0;
            pnlNoResults.Visible = totalWritten > 0 && totalReviews == 0;
            pnlPagination.Visible = totalReviews > PageSize;

            lblResultCount.Text = totalReviews == 1
                ? "1 Review"
                : totalReviews + " Reviews";
            lblPageStatus.Text = string.Format(
                "Page {0} of {1}",
                CurrentPage + 1,
                pageCount
            );

            btnPrevious.Enabled = CurrentPage > 0;
            btnNext.Enabled = CurrentPage < pageCount - 1;
        }

        // Count all reviews written by the learner before filters are applied.
        private int GetTotalReviewCount()
        {
            DataTable count = Read(@"
                SELECT COUNT(ReviewID) AS ReviewCount
                FROM Reviews
                WHERE LearnerID=@learnerId",
                Parameter("@learnerId", LearnerId));

            return Convert.ToInt32(count.Rows[0]["ReviewCount"]);
        }

        private int GetSelectedRating()
        {
            int rating;

            if (!int.TryParse(ddlRating.SelectedValue, out rating) ||
                rating < 1 ||
                rating > 5)
            {
                return 0;
            }

            return rating;
        }

        private string GetReviewOrderBy()
        {
            switch (ddlSort.SelectedValue)
            {
                case "oldest":
                    return "r.CreatedAt ASC, r.ReviewID ASC";
                case "highest":
                    return "r.Rating DESC, r.CreatedAt DESC, r.ReviewID DESC";
                case "lowest":
                    return "r.Rating ASC, r.CreatedAt DESC, r.ReviewID DESC";
                default:
                    return "r.CreatedAt DESC, r.ReviewID DESC";
            }
        }

        protected bool IsEditing(object reviewId)
        {
            return EditingReviewId == Convert.ToInt32(reviewId);
        }

        protected string ReviewCardClass(object reviewId)
        {
            return IsEditing(reviewId)
                ? "review-card is-editing"
                : "review-card";
        }

        protected string StarText(object ratingValue)
        {
            int rating = Convert.ToInt32(ratingValue);
            return new string('\u2605', rating) + new string('\u2606', 5 - rating);
        }

        protected string ReviewDate(object dateValue)
        {
            return Convert.ToDateTime(dateValue).ToString("d MMM yyyy");
        }

        protected string ReviewDateValue(object dateValue)
        {
            return Convert.ToDateTime(dateValue).ToString("yyyy-MM-dd");
        }

        protected string CourseOverviewUrl(object courseId)
        {
            return ResolveUrl(
                "~/Pages/Learner/SearchCourse/SearchCoursePage.aspx?courseId=" +
                Convert.ToInt32(courseId)
            );
        }

        protected string DeleteConfirmation(object courseTitle)
        {
            string title = HttpUtility.JavaScriptStringEncode(
                Convert.ToString(courseTitle)
            );

            return "return confirm('Delete this review?\\n\\nYour review for \"" +
                title +
                "\" will be permanently removed.\\n\\nYou can leave another review for this course later.');";
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ResetPageAndLoadReviews();
        }

        protected void FiltersChanged(object sender, EventArgs e)
        {
            ResetPageAndLoadReviews();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlRating.SelectedIndex = 0;
            ddlSort.SelectedIndex = 0;
            EditingReviewId = 0;
            ResetPageAndLoadReviews();
        }

        private void ResetPageAndLoadReviews()
        {
            CurrentPage = 0;
            LoadReviews();
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            EditingReviewId = 0;
            CurrentPage = Math.Max(0, CurrentPage - 1);
            LoadReviews();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            EditingReviewId = 0;
            CurrentPage++;
            LoadReviews();
        }

        protected void rptReviews_ItemCommand(
            object source,
            RepeaterCommandEventArgs e
        )
        {
            int reviewId;

            if (!int.TryParse(Convert.ToString(e.CommandArgument), out reviewId) ||
                reviewId <= 0)
            {
                ShowMessage("That review could not be found.");
                return;
            }

            if (e.CommandName == "EditReview")
            {
                EditingReviewId = reviewId;
                LoadReviews();
                return;
            }

            if (e.CommandName == "CancelEdit")
            {
                EditingReviewId = 0;
                LoadReviews();
                return;
            }

            if (e.CommandName == "SaveReview")
            {
                SaveReview(e.Item, reviewId);
                return;
            }

            if (e.CommandName == "DeleteReview")
            {
                DeleteReview(reviewId);
            }
        }

        // Validate and update a review owned by the learner.
        private void SaveReview(RepeaterItem item, int reviewId)
        {
            string validationGroup = "EditReview" + reviewId;
            Page.Validate(validationGroup);

            RadioButtonList ratingList =
                (RadioButtonList)item.FindControl("rblEditRating");
            TextBox commentBox =
                (TextBox)item.FindControl("txtEditComment");

            int rating;
            string reviewText = commentBox.Text.Trim();
            string validationMessage;
            bool reviewIsValid = ValidateReview(
                ratingList.SelectedValue,
                reviewText,
                out rating,
                out validationMessage
            );

            if (!Page.IsValid || !reviewIsValid)
            {
                ShowMessage(validationMessage);
                EditingReviewId = reviewId;
                return;
            }

            try
            {
                int updatedRows = UpdateReview(reviewId, rating, reviewText);

                if (updatedRows != 1)
                {
                    ShowMessage("That review is no longer available.");
                    EditingReviewId = 0;
                }
                else
                {
                    ShowMessage("Your review has been updated.");
                    EditingReviewId = 0;
                }
            }
            catch (SqlException)
            {
                ShowMessage("Your review could not be updated. Please try again.");
                EditingReviewId = reviewId;
            }

            LoadReviewStatistics();
            LoadReviews();
        }

        // Validate the review rating and comment.
        private bool ValidateReview(
            string selectedRating,
            string reviewText,
            out int rating,
            out string message
        )
        {
            if (!int.TryParse(selectedRating, out rating) ||
                rating < 1 ||
                rating > 5)
            {
                message = "Choose a rating from 1 to 5.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(reviewText))
            {
                message = "Enter your review before saving.";
                return false;
            }

            if (reviewText.Length > MaximumReviewLength)
            {
                message = "Keep your review within 2,000 characters.";
                return false;
            }

            message = "";
            return true;
        }

        // Update a review owned by the current learner.
        private int UpdateReview(int reviewId, int rating, string reviewText)
        {
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                UPDATE Reviews
                SET Rating=@rating,
                    Comment=@reviewText
                WHERE ReviewID=@reviewId
                  AND LearnerID=@learnerId", connection))
            {
                command.Parameters.Add("@rating", SqlDbType.Int).Value = rating;
                command.Parameters.Add("@reviewText", SqlDbType.NVarChar, -1).Value = reviewText;
                command.Parameters.Add("@reviewId", SqlDbType.Int).Value = reviewId;
                command.Parameters.Add("@learnerId", SqlDbType.Int).Value = LearnerId;

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        // Delete a review owned by the current learner.
        private void DeleteReview(int reviewId)
        {
            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB())
                using (SqlCommand command = new SqlCommand(@"
                    DELETE FROM Reviews
                    WHERE ReviewID=@reviewId
                      AND LearnerID=@learnerId", connection))
                {
                    command.Parameters.Add("@reviewId", SqlDbType.Int).Value = reviewId;
                    command.Parameters.Add("@learnerId", SqlDbType.Int).Value = LearnerId;

                    connection.Open();
                    int deletedRows = command.ExecuteNonQuery();

                    ShowMessage(
                        deletedRows == 1
                            ? "Your review has been deleted."
                            : "That review is no longer available."
                    );
                }
            }
            catch (SqlException)
            {
                ShowMessage("Your review could not be deleted. Please try again.");
            }

            EditingReviewId = 0;
            LoadReviewStatistics();
            LoadReviews();
        }

        private void ShowMessage(string message)
        {
            pnlMessage.Visible = true;
            litMessage.Text = message;
        }
    }
}
