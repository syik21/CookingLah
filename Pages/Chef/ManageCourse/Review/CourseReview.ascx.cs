using System;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.ManageCourse.Review
{
    public partial class CourseReview : UserControl
    {
        private const int PageSize = 10;
        private int CurrentPage
        {
            get
            {
                return (int?)ViewState["CurrentPage"] ?? 0;
            }
            set
            {
                ViewState["CurrentPage"] = value;
            }
        }

        private ManageCoursePage ManagePage
        {
            get
            {
                return (ManageCoursePage)Page;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            LoadCourseReviews();
            base.OnPreRender(e);
        }

        // Load a maximum of 10 reviews for the selected owned course.
        private void LoadCourseReviews()
        {
            int courseId = ManagePage.CourseId;
            int chefId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out chefId))
                throw new HttpException(403, "Access denied.");

            DataTable summary = Read(@"
                SELECT COUNT(r.ReviewID) AS ReviewCount,
                       AVG(CAST(r.Rating AS DECIMAL(5,2))) AS AverageRating,
                       SUM(CASE WHEN r.Rating=5 THEN 1 ELSE 0 END) AS FiveCount,
                       SUM(CASE WHEN r.Rating=4 THEN 1 ELSE 0 END) AS FourCount,
                       SUM(CASE WHEN r.Rating=3 THEN 1 ELSE 0 END) AS ThreeCount,
                       SUM(CASE WHEN r.Rating=2 THEN 1 ELSE 0 END) AS TwoCount,
                       SUM(CASE WHEN r.Rating=1 THEN 1 ELSE 0 END) AS OneCount
                FROM Courses c
                LEFT JOIN Reviews r ON r.CourseID=c.CourseID
                WHERE c.CourseID=@courseId AND c.InstructorID=@chefId",
                Parameter("@courseId", courseId), Parameter("@chefId", chefId));
            if (summary.Rows.Count != 1)
                throw new HttpException(404, "Course not found or access denied.");

            DataRow totals = summary.Rows[0];
            int reviewCount = Convert.ToInt32(totals["ReviewCount"]);
            int pageCount = Math.Max(1, (reviewCount + PageSize - 1) / PageSize);
            CurrentPage = Math.Min(CurrentPage, pageCount - 1);
            BindSummary(totals, reviewCount);

            DataTable reviews = Read(@"
                SELECT r.ReviewID,r.Rating,r.Comment,r.CreatedAt,
                       COALESCE(NULLIF(u.FullName,''),u.Username) AS LearnerName
                FROM Reviews r
                INNER JOIN Courses c ON c.CourseID=r.CourseID
                INNER JOIN Users u ON u.UserID=r.LearnerID
                WHERE r.CourseID=@courseId AND c.InstructorID=@chefId
                ORDER BY r.CreatedAt DESC,r.ReviewID DESC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
                Parameter("@courseId", courseId), Parameter("@chefId", chefId), Parameter("@offset", CurrentPage * PageSize), Parameter("@pageSize", PageSize));
            rptReviews.DataSource = reviews;
            rptReviews.DataBind();
            UpdatePagination(reviewCount, pageCount);
        }

        private void BindSummary(DataRow row, int reviewCount)
        {
            pnlSummary.Visible = reviewCount > 0;
            pnlReviews.Visible = reviewCount > 0;
            pnlEmpty.Visible = reviewCount == 0;
            if (reviewCount == 0)
                return;

            litAverageRating.Text = "★ " + Convert.ToDecimal(row["AverageRating"]).ToString("0.0") + " / 5";
            litReviewCount.Text = CountText(reviewCount);
            int[] counts = { Convert.ToInt32(row["FiveCount"]), Convert.ToInt32(row["FourCount"]), Convert.ToInt32(row["ThreeCount"]), Convert.ToInt32(row["TwoCount"]), Convert.ToInt32(row["OneCount"]) };
            rptDistribution.DataSource = counts.Select((count, index) => new { Rating = 5 - index, Count = count, Percent = count * 100 / reviewCount });
            rptDistribution.DataBind();
        }

        // Update the Review pagination controls.
        private void UpdatePagination(int reviewCount, int pageCount)
        {
            pnlPagination.Visible = reviewCount > PageSize;
            lblPageStatus.Text = string.Format("Page {0} of {1}", CurrentPage + 1, pageCount);
            btnPrevious.Enabled = CurrentPage > 0;
            btnNext.Enabled = CurrentPage < pageCount - 1;
        }

        private static string CountText(int count)
        {
            return count + (count == 1 ? " Review" : " Reviews");
        }

        protected string StarText(object value)
        {
            int rating = Convert.ToInt32(value);
            return new string('\u2605', rating) + new string('\u2606', 5 - rating);
        }

        protected string ReviewDate(object value)
        {
            return Convert.ToDateTime(value).ToString("d MMM yyyy");
        }

        protected string ReviewDateValue(object value)
        {
            return Convert.ToDateTime(value).ToString("yyyy-MM-dd");
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            KeepReviewActive();
            CurrentPage = Math.Max(0, CurrentPage - 1);
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            KeepReviewActive();
            CurrentPage++;
        }

        private void KeepReviewActive()
        {
            ManagePage.SetCurrentTab("review");
        }
    }
}
