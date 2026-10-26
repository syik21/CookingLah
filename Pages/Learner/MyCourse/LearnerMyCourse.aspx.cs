using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Learner.MyCourse
{
    public partial class LearnerMyCourse : Page
    {
        private const int PageSize = 6;
        private int LearnerId { get; set; }

        private int CurrentPage
        {
            get { return (int?)ViewState["CurrentPage"] ?? 0; }
            set { ViewState["CurrentPage"] = value; }
        }

        private string CurrentStatusFilter
        {
            get
            {
                string status = Convert.ToString(ViewState["CurrentStatusFilter"]);
                return string.IsNullOrEmpty(status) ? "All" : status;
            }
            set { ViewState["CurrentStatusFilter"] = value; }
        }

        private int TotalStartedCourseCount { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            int learnerId = 0;
            bool hasLearnerSession =
                string.Equals(Convert.ToString(Session["Role"]), "Learner", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(Convert.ToString(Session["UserId"]), out learnerId) &&
                learnerId > 0;

            if (!hasLearnerSession)
            {
                RedirectToLogin();
                return;
            }

            DataTable learner = Read(@"
                SELECT UserID
                FROM Users
                WHERE UserID=@learnerId
                  AND Role='Learner'
                  AND AccountStatus='Active'",
                Parameter("@learnerId", learnerId));

            if (learner.Rows.Count == 0)
            {
                RedirectToLogin();
                return;
            }

            LearnerId = learnerId;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (LearnerId <= 0) return;

            if (!IsPostBack)
            {
                LoadCategories();
            }

            LoadStatusCounts();
            LoadMyCourses();
            UpdateStatusFilterUI();
        }

        private void RedirectToLogin()
        {
            Response.Redirect(ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void LoadCategories()
        {
            DataTable categories = Read(@"
                SELECT CategoryID,CategoryName
                FROM Categories
                ORDER BY CategoryName");

            ddlCategory.DataSource = categories;
            ddlCategory.DataTextField = "CategoryName";
            ddlCategory.DataValueField = "CategoryID";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert(0, new ListItem("All categories", "0"));
        }

        private void LoadStatusCounts()
        {
            DataTable counts = Read(@"
                SELECT COUNT(*) AS AllCount,
                       ISNULL(SUM(CASE WHEN cp.Status='In Progress' THEN 1 ELSE 0 END),0) AS InProgressCount,
                       ISNULL(SUM(CASE WHEN cp.Status='Completed' THEN 1 ELSE 0 END),0) AS CompletedCount
                FROM CourseProgress cp
                INNER JOIN Courses c ON c.CourseID=cp.CourseID
                WHERE cp.LearnerID=@learnerId
                  AND cp.Status IN ('In Progress','Completed')
                  AND c.Status='Published'",
                Parameter("@learnerId", LearnerId));

            if (counts.Rows.Count == 0) return;

            DataRow countRow = counts.Rows[0];
            lblAllCoursesCount.Text = Convert.ToString(countRow["AllCount"]);
            lblInProgressCount.Text = Convert.ToString(countRow["InProgressCount"]);
            lblCompletedCount.Text = Convert.ToString(countRow["CompletedCount"]);
            TotalStartedCourseCount = Convert.ToInt32(countRow["AllCount"]);
        }

        private void LoadMyCourses()
        {
            int categoryId;
            if (!int.TryParse(ddlCategory.SelectedValue, out categoryId))
            {
                categoryId = 0;
            }

            string searchText = txtSearchCourses.Text.Trim();
            string status = CurrentStatusFilter == "All" ? "" : CurrentStatusFilter;

            DataTable courses = Read(@"
                SELECT c.CourseID,
                       c.Title,
                       c.Thumbnail,
                       c.DifficultyLevel,
                       cat.CategoryName,
                       chef.FullName AS ChefName,
                       CASE WHEN cp.Status='Completed' THEN 100
                            ELSE cp.ProgressPercentage END AS ProgressPercentage,
                       cp.Status AS ProgressStatus,
                       (SELECT COUNT(*) FROM Lessons l WHERE l.CourseID=c.CourseID) AS LessonCount,
                       (SELECT COUNT(*) FROM Quizzes q WHERE q.CourseID=c.CourseID) AS QuizCount,
                       (SELECT COUNT(*) FROM PracticalActivities a WHERE a.CourseID=c.CourseID) AS ActivityCount
                FROM CourseProgress cp
                INNER JOIN Courses c ON c.CourseID=cp.CourseID
                INNER JOIN Categories cat ON cat.CategoryID=c.CategoryID
                INNER JOIN Users chef ON chef.UserID=c.InstructorID
                WHERE cp.LearnerID=@learnerId
                  AND cp.Status IN ('In Progress','Completed')
                  AND c.Status='Published'
                  AND (@status='' OR cp.Status=@status)
                  AND (@categoryId=0 OR c.CategoryID=@categoryId)
                  AND (@searchText='' OR c.Title LIKE '%' + @searchText + '%'
                       OR c.Description LIKE '%' + @searchText + '%'
                       OR cat.CategoryName LIKE '%' + @searchText + '%')
                ORDER BY CASE WHEN cp.Status='In Progress' THEN 0 ELSE 1 END,
                         cp.LastAccessedAt DESC,
                         c.Title",
                Parameter("@learnerId", LearnerId),
                Parameter("@status", status),
                Parameter("@categoryId", categoryId),
                Parameter("@searchText", searchText));

            BindMyCourses(courses);
        }

        private void BindMyCourses(DataTable courses)
        {
            int totalCourses = courses.Rows.Count;
            int pageCount = Math.Max(1, (totalCourses + PageSize - 1) / PageSize);
            CurrentPage = Math.Min(CurrentPage, pageCount - 1);

            var page = new PagedDataSource
            {
                DataSource = courses.DefaultView,
                AllowPaging = true,
                PageSize = PageSize,
                CurrentPageIndex = CurrentPage
            };

            rptMyCourses.DataSource = page;
            rptMyCourses.DataBind();

            pnlMyLearning.Visible = totalCourses > 0;
            pnlNoStartedCourses.Visible = TotalStartedCourseCount == 0;
            pnlNoResults.Visible = TotalStartedCourseCount > 0 && totalCourses == 0;
            pnlPagination.Visible = totalCourses > PageSize;

            int firstCourse = totalCourses == 0 ? 0 : CurrentPage * PageSize + 1;
            int lastCourse = Math.Min((CurrentPage + 1) * PageSize, totalCourses);
            lblCourseSummary.Text = string.Format(
                "Showing {0}-{1} of {2} courses",
                firstCourse,
                lastCourse,
                totalCourses);

            lblPageStatus.Text = string.Format("Page {0} of {1}", CurrentPage + 1, pageCount);
            btnPrevious.Enabled = CurrentPage > 0;
            btnNext.Enabled = CurrentPage < pageCount - 1;
        }

        private void UpdateStatusFilterUI()
        {
            SetFilterState(pnlAllCoursesFilter, btnAllCourses, lblAllCoursesCount, "All", "All courses");
            SetFilterState(pnlInProgressFilter, btnInProgress, lblInProgressCount, "In Progress", "In progress");
            SetFilterState(pnlCompletedFilter, btnCompleted, lblCompletedCount, "Completed", "Completed");
        }

        private void SetFilterState(Panel filterPanel, LinkButton button, Label countLabel, string status, string label)
        {
            bool isActive = CurrentStatusFilter == status;
            filterPanel.CssClass = isActive
                ? "my-course-status-filter active"
                : "my-course-status-filter";

            if (isActive)
            {
                button.Attributes["aria-current"] = "page";
            }
            else
            {
                button.Attributes.Remove("aria-current");
            }

            button.Attributes["aria-label"] = string.Format("{0}, {1} courses", label, countLabel.Text);
        }

        protected string StatusBadgeClass(object progressStatus)
        {
            return Convert.ToString(progressStatus) == "Completed"
                ? "my-course-status-badge is-completed"
                : "my-course-status-badge is-in-progress";
        }

        protected string CourseActionText(object progressStatus)
        {
            return Convert.ToString(progressStatus) == "Completed"
                ? "Review course"
                : "Continue learning";
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            CurrentPage = 0;
            LoadStatusCounts();
            LoadMyCourses();
        }

        protected void FiltersChanged(object sender, EventArgs e)
        {
            CurrentPage = 0;
            LoadStatusCounts();
            LoadMyCourses();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearchCourses.Text = "";
            ddlCategory.SelectedIndex = 0;
            CurrentStatusFilter = "All";
            CurrentPage = 0;
            LoadStatusCounts();
            LoadMyCourses();
            UpdateStatusFilterUI();
        }

        protected void btnAllCourses_Click(object sender, EventArgs e)
        {
            ApplyStatusFilter("All");
        }

        protected void btnInProgress_Click(object sender, EventArgs e)
        {
            ApplyStatusFilter("In Progress");
        }

        protected void btnCompleted_Click(object sender, EventArgs e)
        {
            ApplyStatusFilter("Completed");
        }

        private void ApplyStatusFilter(string status)
        {
            CurrentStatusFilter = status;
            CurrentPage = 0;
            LoadStatusCounts();
            LoadMyCourses();
            UpdateStatusFilterUI();
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            CurrentPage = Math.Max(0, CurrentPage - 1);
            LoadMyCourses();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            LoadMyCourses();
        }

        protected void rptMyCourses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "GoToCourse") return;

            int courseId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out courseId) || courseId <= 0)
            {
                return;
            }

            DataTable learnerCourse = Read(@"
                SELECT cp.CourseID
                FROM CourseProgress cp
                INNER JOIN Courses c ON c.CourseID=cp.CourseID
                WHERE cp.LearnerID=@learnerId
                  AND cp.CourseID=@courseId
                  AND cp.Status IN ('In Progress','Completed')
                  AND c.Status='Published'",
                Parameter("@learnerId", LearnerId),
                Parameter("@courseId", courseId));

            if (learnerCourse.Rows.Count == 0) return;

            NavigateToCourse(courseId);
        }

        private void NavigateToCourse(int courseId)
        {
            string courseUrl = ResolveUrl(
                "~/Pages/Learner/CourseLearning/CourseLearningPage.aspx?courseId=" + courseId
            );

            Response.Redirect(courseUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
