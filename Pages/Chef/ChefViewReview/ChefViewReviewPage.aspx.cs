using CookingLah.F_Helpers;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.ChefViewReview
{
    public partial class ChefViewReviewPage : Page
    {
        private const int PageSize = 10;
        private int ChefId { get; set; }
        private int CurrentPage
        {
            get { return (int?)ViewState["CurrentPage"] ?? 0; }
            set { ViewState["CurrentPage"] = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ChefId = GetCurrentChefId();
            if (ChefId <= 0)
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (ChefId <= 0) return;
            LoadReviewStatistics();
            if (!IsPostBack)
            {
                LoadCourseFilter();
                LoadReviews();
            }
        }

        // Verify the Chef session and active Instructor account.
        private int GetCurrentChefId()
        {
            int chefId = 0;
            bool validSession = string.Equals(Convert.ToString(Session["Role"]), "Instructor", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(Convert.ToString(Session["UserId"]), out chefId)
                && chefId > 0;
            if (!validSession) return 0;

            DataTable chef = Read(@"SELECT UserID FROM Users WHERE UserID=@chefId AND Role='Instructor' AND AccountStatus='Active'", Parameter("@chefId", chefId));
            return chef.Rows.Count == 1 ? chefId : 0;
        }

        // Calculate statistics from courses owned by the current Chef.
        private void LoadReviewStatistics()
        {
            DataRow row = Read(@"
                SELECT COUNT(r.ReviewID) AS ReviewCount,
                       AVG(CAST(r.Rating AS DECIMAL(5,2))) AS AverageRating,
                       COUNT(DISTINCT r.CourseID) AS ReviewedCourseCount
                FROM Courses c
                LEFT JOIN Reviews r ON r.CourseID=c.CourseID
                WHERE c.InstructorID=@chefId", Parameter("@chefId", ChefId)).Rows[0];
            int reviewCount = Convert.ToInt32(row["ReviewCount"]);
            int courseCount = Convert.ToInt32(row["ReviewedCourseCount"]);
            litTotalReviews.Text = CountText(reviewCount, "Review");
            litAverageRating.Text = reviewCount == 0 ? "—" : "★ " + Convert.ToDecimal(row["AverageRating"]).ToString("0.0") + " / 5";
            litReviewedCourses.Text = CountText(courseCount, "Course");
        }

        // Load only courses owned by the current Chef.
        private void LoadCourseFilter()
        {
            ddlCourse.DataSource = Read("SELECT CourseID,Title FROM Courses WHERE InstructorID=@chefId ORDER BY Title", Parameter("@chefId", ChefId));
            ddlCourse.DataTextField = "Title";
            ddlCourse.DataValueField = "CourseID";
            ddlCourse.DataBind();
            ddlCourse.Items.Insert(0, new ListItem("All Courses", "0"));
        }

        // Load one filtered page of reviews from Chef-owned courses.
        private void LoadReviews()
        {
            int courseId = SelectedNumber(ddlCourse.SelectedValue, 0, int.MaxValue);
            int rating = SelectedNumber(ddlRating.SelectedValue, 0, 5);
            string searchText = txtSearch.Text.Trim();
            int totalReviews = GetReviewCount(courseId, rating, searchText);
            int pageCount = Math.Max(1, (totalReviews + PageSize - 1) / PageSize);
            CurrentPage = Math.Min(CurrentPage, pageCount - 1);
            string query = @"
                SELECT r.ReviewID,r.CourseID,r.Rating,r.Comment,r.CreatedAt,
                       c.Title AS CourseTitle,c.Thumbnail,c.DifficultyLevel,
                       cat.CategoryName,COALESCE(NULLIF(u.FullName,''),u.Username) AS LearnerName
                FROM Reviews r
                INNER JOIN Courses c ON c.CourseID=r.CourseID
                INNER JOIN Categories cat ON cat.CategoryID=c.CategoryID
                INNER JOIN Users u ON u.UserID=r.LearnerID
                WHERE c.InstructorID=@chefId
                  AND (@courseId=0 OR r.CourseID=@courseId)
                  AND (@rating=0 OR r.Rating=@rating)
                  AND (@searchText='' OR c.Title LIKE '%' + @searchText + '%' OR u.FullName LIKE '%' + @searchText + '%' OR u.Username LIKE '%' + @searchText + '%')
                ORDER BY " + ReviewOrderBy() + @"
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";
            DataTable reviews = Read(query, Parameter("@chefId", ChefId), Parameter("@courseId", courseId), Parameter("@rating", rating), Parameter("@searchText", searchText), Parameter("@offset", CurrentPage * PageSize), Parameter("@pageSize", PageSize));
            rptReviews.DataSource = reviews;
            rptReviews.DataBind();
            UpdatePagination(totalReviews, pageCount);
        }

        private int GetReviewCount(int courseId, int rating, string searchText)
        {
            DataTable count = Read(@"
                SELECT COUNT(r.ReviewID) AS ReviewCount
                FROM Reviews r
                INNER JOIN Courses c ON c.CourseID=r.CourseID
                INNER JOIN Users u ON u.UserID=r.LearnerID
                WHERE c.InstructorID=@chefId
                  AND (@courseId=0 OR r.CourseID=@courseId)
                  AND (@rating=0 OR r.Rating=@rating)
                  AND (@searchText='' OR c.Title LIKE '%' + @searchText + '%' OR u.FullName LIKE '%' + @searchText + '%' OR u.Username LIKE '%' + @searchText + '%')",
                Parameter("@chefId", ChefId), Parameter("@courseId", courseId), Parameter("@rating", rating), Parameter("@searchText", searchText));
            return Convert.ToInt32(count.Rows[0]["ReviewCount"]);
        }

        // Update result states and pagination controls.
        private void UpdatePagination(int resultCount, int pageCount)
        {
            int allReviewCount = Convert.ToInt32(Read(@"SELECT COUNT(r.ReviewID) FROM Reviews r INNER JOIN Courses c ON c.CourseID=r.CourseID WHERE c.InstructorID=@chefId", Parameter("@chefId", ChefId)).Rows[0][0]);
            pnlResults.Visible = resultCount > 0;
            pnlNoReviews.Visible = allReviewCount == 0;
            pnlNoResults.Visible = allReviewCount > 0 && resultCount == 0;
            pnlPagination.Visible = resultCount > PageSize;
            litResultCount.Text = CountText(resultCount, "Review");
            lblPageStatus.Text = string.Format("Page {0} of {1}", CurrentPage + 1, pageCount);
            btnPrevious.Enabled = CurrentPage > 0;
            btnNext.Enabled = CurrentPage < pageCount - 1;
        }

        private string ReviewOrderBy()
        {
            switch (ddlSort.SelectedValue)
            {
                case "oldest": return "r.CreatedAt ASC,r.ReviewID ASC";
                case "highest": return "r.Rating DESC,r.CreatedAt DESC,r.ReviewID DESC";
                case "lowest": return "r.Rating ASC,r.CreatedAt DESC,r.ReviewID DESC";
                default: return "r.CreatedAt DESC,r.ReviewID DESC";
            }
        }

        private static int SelectedNumber(string value, int minimum, int maximum)
        {
            int number;
            return int.TryParse(value, out number) && number >= minimum && number <= maximum ? number : 0;
        }
        private static string CountText(int count, string noun) { return count + " " + noun + (count == 1 ? "" : "s"); }
        protected string StarText(object value) { int rating = Convert.ToInt32(value); return new string('\u2605', rating) + new string('\u2606', 5 - rating); }
        protected string ReviewDate(object value) { return Convert.ToDateTime(value).ToString("d MMM yyyy"); }
        protected string ReviewDateValue(object value) { return Convert.ToDateTime(value).ToString("yyyy-MM-dd"); }
        protected string ManageCourseUrl(object courseId) { return ResolveUrl("~/Pages/Chef/ManageCourse/ManageCoursePage.aspx?courseId=" + Convert.ToInt32(courseId) + "&section=review"); }
        protected void btnSearch_Click(object sender, EventArgs e) { ResetAndLoadReviews(); }
        protected void FiltersChanged(object sender, EventArgs e) { ResetAndLoadReviews(); }
        protected void btnPrevious_Click(object sender, EventArgs e) { CurrentPage = Math.Max(0, CurrentPage - 1); LoadReviews(); }
        protected void btnNext_Click(object sender, EventArgs e) { CurrentPage++; LoadReviews(); }
        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlCourse.SelectedIndex = 0;
            ddlRating.SelectedIndex = 0;
            ddlSort.SelectedIndex = 0;
            ResetAndLoadReviews();
        }
        private void ResetAndLoadReviews() { CurrentPage = 0; LoadReviews(); }
    }
}
