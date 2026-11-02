using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Shared.CourseSearch
{
    public partial class SearchCourse : System.Web.UI.UserControl
    {
        private const int PageSize = 9;
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

        private int SelectedCourseId
        {
            get
            {
                return (int?)ViewState["SelectedCourseId"] ?? 0;
            }
            set
            {
                ViewState["SelectedCourseId"] = value;
            }
        }

        public bool IsLearner { get; set; }

        private int SelectedFoodCultureId
        {
            get
            {
                return (int?)ViewState["SelectedFoodCultureId"] ?? 0;
            }
            set
            {
                ViewState["SelectedFoodCultureId"] = value;
            }
        }

        protected string SelectedDishName
        {
            get
            {
                return (string)ViewState["SelectedDishName"] ?? "";
            }
            set
            {
                ViewState["SelectedDishName"] = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            int learnerId;
            bool hasLearnerSession =
                string.Equals(Convert.ToString(Session["Role"]), "Learner", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(Convert.ToString(Session["UserId"]), out learnerId) &&
                learnerId > 0;

            if (hasLearnerSession)
            {
                IsLearner = true;
            }

            ViewCourseOverivew1.IsLearner = IsLearner;
            ViewCourseOverivew1.BackRequested += ViewCourseOverivew1_BackRequested;
            ViewCourseOverivew1.StartCourseRequested += ViewCourseOverivew1_StartCourseRequested;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCategories();
                txtSearch.Text = (Request.QueryString["search"] ?? "").Trim();
                if (txtSearch.Text.Length > 100)
                    txtSearch.Text = txtSearch.Text.Substring(0, 100);
                int foodCultureId;
                if (int.TryParse(Request.QueryString["foodCultureId"], out foodCultureId) && foodCultureId > 0)
                {
                    SelectedFoodCultureId = foodCultureId;
                    DataTable dish = Read("SELECT DishName FROM FoodCultures WHERE FoodCultureID=@foodCultureId",
                        Parameter("@foodCultureId", foodCultureId));
                    SelectedDishName = dish.Rows.Count > 0
                        ? Convert.ToString(dish.Rows[0]["DishName"])
                        : "Unavailable traditional dish";
                }
            }

            // Rebind so Repeater commands are available on postback.
            LoadCourses();
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (pnlCourseOverview.Visible && SelectedCourseId > 0)
            {
                ViewCourseOverivew1.IsLearner = IsLearner;
                ViewCourseOverivew1.LoadCourse(SelectedCourseId);
            }

            base.OnPreRender(e);
        }

        private void LoadCategories()
        {
            ddlCategory.DataSource = Read("SELECT CategoryID,CategoryName FROM Categories ORDER BY CategoryName");
            ddlCategory.DataTextField = "CategoryName";
            ddlCategory.DataValueField = "CategoryID";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert(0, new ListItem("All Categories", "0"));
        }

        private void LoadCourses()
        {
            int categoryId;
            if (!int.TryParse(ddlCategory.SelectedValue, out categoryId))
                categoryId = 0;

            string searchText = txtSearch.Text.Trim();
            string difficulty = ddlDifficulty.SelectedValue;
            int minimumRating;
            if (!int.TryParse(ddlRating.SelectedValue, out minimumRating) || minimumRating < 1 || minimumRating > 4)
                minimumRating = 0;

            DataTable courses = Read(@"
                SELECT c.CourseID,c.Title,c.Thumbnail,c.DifficultyLevel,(SELECT TOP 1 CookingTime FROM CourseRecipes cr WHERE cr.CourseID=c.CourseID) AS CookingTime,
                       LEFT(c.Description,180) AS ShortDescription,
                       cat.CategoryName,u.FullName AS ChefName,fc.DishName,
                       ratings.AverageRating,COALESCE(ratings.ReviewCount,0) AS ReviewCount
                FROM Courses c
                INNER JOIN Categories cat ON cat.CategoryID=c.CategoryID
                INNER JOIN Users u ON u.UserID=c.InstructorID
                LEFT JOIN FoodCultures fc ON fc.FoodCultureID=c.FoodCultureID
                LEFT JOIN (
                    SELECT CourseID,AVG(CAST(Rating AS DECIMAL(5,2))) AS AverageRating,COUNT(ReviewID) AS ReviewCount
                    FROM Reviews GROUP BY CourseID
                ) ratings ON ratings.CourseID=c.CourseID
                WHERE c.Status='Published'
                  AND (@foodCultureId=0 OR c.FoodCultureID=@foodCultureId)
                  AND (@categoryId=0 OR c.CategoryID=@categoryId)
                  AND (@difficulty='' OR c.DifficultyLevel=@difficulty)
                  AND (@minimumRating=0 OR ratings.AverageRating>=@minimumRating)
                  AND (@searchText='' OR c.Title LIKE '%' + @searchText + '%'
                       OR c.Description LIKE '%' + @searchText + '%'
                       OR cat.CategoryName LIKE '%' + @searchText + '%')
                ORDER BY CASE WHEN ratings.ReviewCount IS NULL THEN 1 ELSE 0 END,
                         ratings.AverageRating DESC,ratings.ReviewCount DESC,c.UpdatedAt DESC,c.CourseID DESC",
                 Parameter("@foodCultureId", SelectedFoodCultureId),
                 Parameter("@categoryId", categoryId),
                Parameter("@difficulty", difficulty),
                Parameter("@minimumRating", minimumRating),
                Parameter("@searchText", searchText));

            int totalCourses = courses.Rows.Count;
            btnClearSearch.Visible = searchText.Length > 0 || categoryId > 0 || difficulty.Length > 0 || minimumRating > 0 || SelectedFoodCultureId > 0;
            int pageCount = Math.Max(1, (totalCourses + PageSize - 1) / PageSize);
            CurrentPage = Math.Min(CurrentPage, pageCount - 1);

            var page = new PagedDataSource
            {
                DataSource = courses.DefaultView,
                AllowPaging = true,
                PageSize = PageSize,
                CurrentPageIndex = CurrentPage
            };

            rptCourses.DataSource = page;
            rptCourses.DataBind();

            lblResultCount.Text = totalCourses == 1 ? "1 Course Found" : totalCourses + " Courses Found";
            pnlEmpty.Visible = totalCourses == 0;
            pnlPager.Visible = totalCourses > PageSize;
            btnPrevious.Enabled = CurrentPage > 0;
            btnNext.Enabled = CurrentPage < pageCount - 1;
            lblPage.Text = string.Format("Page {0} of {1}", CurrentPage + 1, pageCount);
        }

        private int GetLearnerIdForSearch()
        {
            int learnerId;
            if (!IsLearner ||
                !string.Equals(Convert.ToString(Session["Role"]), "Learner", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(Convert.ToString(Session["UserId"]), out learnerId))
            {
                return 0;
            }

            return learnerId;
        }

        protected bool HasStartedCourse(object progressStatus, object progressPercentage)
        {
            string status = Convert.ToString(progressStatus);
            int percentage;
            int.TryParse(Convert.ToString(progressPercentage), out percentage);

            return status == "In Progress" || status == "Completed" || percentage > 0;
        }

        protected int CourseProgressValue(object progressStatus, object progressPercentage)
        {
            if (Convert.ToString(progressStatus) == "Completed")
                return 100;

            int percentage;
            if (!int.TryParse(Convert.ToString(progressPercentage), out percentage))
                return 0;

            return Math.Max(0, Math.Min(100, percentage));
        }

        protected string CourseProgressLabel(object progressStatus)
        {
            return Convert.ToString(progressStatus) == "Completed"
                ? "Course completed"
                : "Your progress";
        }

        protected string CourseNotStartedMessage()
        {
            return IsLearner
                ? "Ready when you are — open the course to begin."
                : "Sign in as a learner to track your progress.";
        }

        protected string FormatRating(object averageRating, object reviewCount)
        {
            int count = Convert.ToInt32(reviewCount);
            if (count == 0 || averageRating == DBNull.Value)
                return "No ratings yet";
            return "★ " + Convert.ToDecimal(averageRating).ToString("0.0") + " · " + count + (count == 1 ? " review" : " reviews");
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            CurrentPage = 0;
            LoadCourses();
        }

        protected string FormatCourseRating(object averageRating, object reviewCount)
        {
            int count = Convert.ToInt32(reviewCount);
            if (count == 0 || averageRating == DBNull.Value)
                return "No reviews yet";
            return "★ " + Convert.ToDecimal(averageRating).ToString("0.0") + " • " + count + (count == 1 ? " review" : " reviews");
        }

        protected void FilterChanged(object sender, EventArgs e)
        {
            CurrentPage = 0;
            LoadCourses();
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            SelectedFoodCultureId = 0;
            SelectedDishName = "";
            txtSearch.Text = "";
            ddlCategory.SelectedIndex = 0;
            ddlDifficulty.SelectedIndex = 0;
            ddlRating.SelectedIndex = 0;
            CurrentPage = 0;
            LoadCourses();
        }

        protected void rptCourses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int courseId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out courseId) || courseId <= 0)
                return;

            if (e.CommandName == "ViewCourse")
            {
                ShowCourseOverview(courseId);
                return;
            }

        }

        private void ShowCourseOverview(int courseId)
        {
            pnlCourseSearch.Visible = false;
            pnlCourseOverview.Visible = true;
            SelectedCourseId = courseId;
            ViewCourseOverivew1.IsLearner = IsLearner;
            ViewCourseOverivew1.LoadCourse(courseId);
        }

        public void OpenCourse(int courseId)
        {
            if (courseId > 0)
                ShowCourseOverview(courseId);
        }

        private void ShowCourseSearch()
        {
            pnlCourseOverview.Visible = false;
            pnlCourseSearch.Visible = true;
            SelectedCourseId = 0;
            LoadCourses();
        }

        private void ViewCourseOverivew1_BackRequested(object sender, EventArgs e)
        {
            ShowCourseSearch();
        }

        private void ViewCourseOverivew1_StartCourseRequested(object sender, CourseActionEventArgs e)
        {
            if (!IsLearner)
            {
                ViewCourseOverivew1.ShowGuestLoginPrompt();
                return;
            }

            int learnerId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out learnerId) || learnerId <= 0)
            {
                Response.Redirect(ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            DataTable learner = Read(@"SELECT UserID FROM Users
                WHERE UserID=@learnerId AND Role='Learner' AND AccountStatus='Active'",
                Parameter("@learnerId", learnerId));
            if (learner.Rows.Count == 0)
            {
                Response.Redirect(ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            using (SqlConnection connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    object allowed = Execute(connection, transaction, @"
                        SELECT c.CourseID FROM Courses c
                        INNER JOIN Users u ON u.UserID=@learnerId
                        WHERE c.CourseID=@courseId AND c.Status='Published'
                          AND u.Role='Learner' AND u.AccountStatus='Active'",
                        Parameter("@learnerId", learnerId),
                        Parameter("@courseId", e.CourseId));

                    if (allowed == null)
                    {
                        transaction.Rollback();
                        Response.Redirect(ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login"), false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }

                    object progressId = Execute(connection, transaction, @"
                        SELECT ProgressID FROM CourseProgress WITH (UPDLOCK,HOLDLOCK)
                        WHERE LearnerID=@learnerId AND CourseID=@courseId",
                        Parameter("@learnerId", learnerId),
                        Parameter("@courseId", e.CourseId));

                    if (progressId == null)
                    {
                        Execute(connection, transaction, @"
                            INSERT INTO CourseProgress
                                (LearnerID,CourseID,ProgressPercentage,Status,StartedAt,LastAccessedAt)
                            VALUES (@learnerId,@courseId,0,'In Progress',GETDATE(),GETDATE())",
                            Parameter("@learnerId", learnerId),
                            Parameter("@courseId", e.CourseId));
                    }
                    else
                    {
                        Execute(connection, transaction, @"
                            UPDATE CourseProgress
                            SET LastAccessedAt=GETDATE(),
                                StartedAt=COALESCE(StartedAt,GETDATE()),
                                Status=CASE WHEN Status='Not Started' THEN 'In Progress' ELSE Status END
                            WHERE LearnerID=@learnerId AND CourseID=@courseId",
                            Parameter("@learnerId", learnerId),
                            Parameter("@courseId", e.CourseId));
                    }

                    transaction.Commit();
                }
            }

            Response.Redirect(ResolveUrl("~/Pages/Learner/CourseLearning/CourseLearningPage.aspx?courseId=" + e.CourseId), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            CurrentPage = Math.Max(0, CurrentPage - 1);
            LoadCourses();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            LoadCourses();
        }
    }
}

