using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web;
using System.Web.UI.WebControls;

namespace CookingLah.Pages.Admin.CourseContentReview
{
    public partial class ReviewCoursesPage : System.Web.UI.Page
    {
        private const int PageSize = 8;
        private int CurrentPage
        {
            get
            {
                return (int)(ViewState["CurrentPage"] ?? 0);
            }
            set
            {
                ViewState["CurrentPage"] = value;
            }
        }

        private string CurrentStatusFilter
        {
            get
            {
                return (string)(ViewState["CurrentStatusFilter"] ?? "All");
            }
            set
            {
                ViewState["CurrentStatusFilter"] = value;
            }
        }
        // Verify that the current user is an active administrator.
        protected override void OnInit(EventArgs e)
        {
            if (Session["ReviewCsrf"] == null)
                Session["ReviewCsrf"] = Guid.NewGuid().ToString("N");
            ViewStateUserKey = Convert.ToString(Session["ReviewCsrf"]);
            base.OnInit(e);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            int userId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out userId))
            {
                Debug.WriteLine("An active admin account is required.");
                Response.Redirect(NavigationHelper.GetUrl("Main"));
                return;
            }
            try
            {
                var admin = ReviewData.Read("SELECT UserID FROM Users WHERE UserID=@id AND Role='Admin' AND AccountStatus='Active'", ReviewData.Int("@id", userId));
                if (admin.Rows.Count != 1)
                {
                    Debug.WriteLine("An active admin account is required.");
                    Response.Redirect(NavigationHelper.GetUrl("Main"));
                    return;
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Course review authorization: {0}", ex);
                throw new HttpException(503, "Course review is temporarily unavailable. Please try again.");
            }
            ViewCourse1.BackRequested += delegate
            {
                ShowCourseList();
            };
            ViewCourse1.CourseReviewed += delegate
            {
                ShowMessage(ViewCourse1.ReviewMessage);
                ShowCourseList();
            };
        }
        // Load the chef filter when the page first opens.
        protected void Page_Load(object sender, EventArgs e)
        {
            pnlMessage.Visible = false;
            if (!IsPostBack)
            {
                RunRead(delegate
                {
                    LoadChefFilter();

                    if (string.Equals(Request.QueryString["status"], "Pending", StringComparison.OrdinalIgnoreCase))
                        CurrentStatusFilter = "Pending";

                    int courseId;
                    if (int.TryParse(Request.QueryString["courseId"], out courseId) && courseId > 0 &&
                        ReviewData.Read("SELECT CourseID FROM Courses WHERE CourseID=@id AND Status='Pending'", ReviewData.Int("@id", courseId)).Rows.Count == 1)
                        ShowCourseDetails(courseId);
                });
            }
        }
        // Refresh the course list and status counts before rendering.
        protected override void OnPreRender(EventArgs e)
        {
            if (pnlCourseList.Visible)
                RunRead(delegate
                {
                    LoadStatusCounts();
                    LoadCourses();
                });
            base.OnPreRender(e);
        }
        // Handle database errors when loading review data.
        private void RunRead(Action action)
        {
            try
            {
                action();
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Course review list: {0}", ex);
                rptCourses.DataSource = null;
                rptCourses.DataBind();
                pnlPagination.Visible = false;
                ShowMessage("Courses could not be loaded. Please try again.");
            }
        }
        // Populate the chef dropdown list.
        private void LoadChefFilter()
        {
            ddlChefFilter.DataSource = ReviewData.Read("SELECT UserID,Username FROM Users WHERE Role='Instructor' ORDER BY Username");
            ddlChefFilter.DataTextField = "Username";
            ddlChefFilter.DataValueField = "UserID";
            ddlChefFilter.DataBind();
            ddlChefFilter.Items.Insert(0, new ListItem("All Chefs", ""));
        }
        // Count courses in each review status.
        private void LoadStatusCounts()
        {
            var counts = ReviewData.Read("SELECT Status,COUNT(*) AS Total FROM Courses WHERE Status IN ('Pending','Published','Rejected') GROUP BY Status");
            int all = 0, pending = 0, approved = 0, rejected = 0;
            foreach (DataRow row in counts.Rows)
            {
                int count = Convert.ToInt32(row["Total"]);
                all += count;
                switch (Convert.ToString(row["Status"]))
                {
                    case "Pending":
                        pending = count;
                        break;
                    case "Published":
                        approved = count;
                        break;
                    case "Rejected":
                        rejected = count;
                        break;
                }
            }
            SetStatusButton(btnAllCourses, "All", "All Courses", all);
            SetStatusButton(btnPending, "Pending", "Pending", pending);
            SetStatusButton(btnApproved, "Published", "Approved", approved);
            SetStatusButton(btnRejected, "Rejected", "Rejected", rejected);
        }
        // Update the status filter text and active style.
        private void SetStatusButton(LinkButton button, string status, string label, int count)
        {
            button.Text = label + " <span>" + count + "</span>";
            button.CssClass = "review-status-filter" + (CurrentStatusFilter == status ? " active" : "");
            button.Attributes["aria-current"] = CurrentStatusFilter == status ? "true" : "false";
        }
        // Search, filter, and paginate the course list.
        private void LoadCourses()
        {
            int chefId;
            int.TryParse(ddlChefFilter.SelectedValue, out chefId);
            string search = txtSearchCourses.Text.Trim();
            string pattern = "%" + search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
            var rows = ReviewData.Read(@"SELECT c.CourseID,c.Title,c.Thumbnail,c.DifficultyLevel,(SELECT TOP 1 CookingTime FROM CourseRecipes cr WHERE cr.CourseID=c.CourseID) AS CookingTime,c.Status,
                COALESCE(c.UpdatedAt,c.CreatedAt) AS UpdatedAt,u.Username AS ChefName,cat.CategoryName,
                (SELECT COUNT(*) FROM Lessons WHERE CourseID=c.CourseID) AS LessonCount,
                (SELECT COUNT(*) FROM Quizzes WHERE CourseID=c.CourseID) AS QuizCount,
                (SELECT COUNT(*) FROM PracticalActivities WHERE CourseID=c.CourseID) AS ActivityCount
                FROM Courses c JOIN Users u ON u.UserID=c.InstructorID JOIN Categories cat ON cat.CategoryID=c.CategoryID
                WHERE c.Status IN ('Pending','Published','Rejected') AND (@status='All' OR c.Status=@status)
                AND (@chef=0 OR c.InstructorID=@chef) AND (c.Title LIKE @search OR u.Username LIKE @search OR cat.CategoryName LIKE @search)
                ORDER BY COALESCE(c.UpdatedAt,c.CreatedAt) DESC,c.CourseID DESC",
                new SqlParameter("@status", SqlDbType.NVarChar, 20)
                {
                    Value = CurrentStatusFilter
                },
                ReviewData.Int("@chef", chefId),
                new SqlParameter("@search", SqlDbType.NVarChar, 1000)
                {
                    Value = pattern
                });
            int pageCount = Math.Max(1, (rows.Rows.Count + PageSize - 1) / PageSize);
            CurrentPage = Math.Max(0, Math.Min(CurrentPage, pageCount - 1));
            var paged = new PagedDataSource
            {
                DataSource = rows.DefaultView,
                AllowPaging = true,
                PageSize = PageSize,
                CurrentPageIndex = CurrentPage
            };
            rptCourses.DataSource = paged;
            rptCourses.DataBind();
            pnlEmpty.Visible = rows.Rows.Count == 0;
            litEmpty.Text = search.Length > 0 ? "No courses match your search." : CurrentStatusFilter == "Pending" && chefId == 0 ? "No courses are waiting for review." : "No courses match these filters.";
            btnClearSearch.Visible = search.Length > 0 || chefId > 0;
            litResults.Text = rows.Rows.Count == 0 ? "0 courses" : string.Format("Showing {0}–{1} of {2} courses", CurrentPage * PageSize + 1, Math.Min((CurrentPage + 1) * PageSize, rows.Rows.Count), rows.Rows.Count);
            pnlPagination.Visible = pageCount > 1;
            btnPrevious.Enabled = CurrentPage > 0;
            btnNext.Enabled = CurrentPage < pageCount - 1;
            litPage.Text = string.Format("Page {0} of {1}", CurrentPage + 1, pageCount);
        }
        // Change the selected status and reset pagination.
        private void SelectStatus(string status)
        {
            CurrentStatusFilter = status;
            CurrentPage = 0;
        }

        protected void btnAllCourses_Click(object sender, EventArgs e)
        {
            SelectStatus("All");
        }

        protected void btnPending_Click(object sender, EventArgs e)
        {
            SelectStatus("Pending");
        }

        protected void btnApproved_Click(object sender, EventArgs e)
        {
            SelectStatus("Published");
        }

        protected void btnRejected_Click(object sender, EventArgs e)
        {
            SelectStatus("Rejected");
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            CurrentPage = 0;
        }

        protected void ClearSearch_Click(object sender, EventArgs e)
        {
            txtSearchCourses.Text = "";
            ddlChefFilter.SelectedIndex = 0;
            CurrentPage = 0;
        }

        protected void Previous_Click(object sender, EventArgs e)
        {
            CurrentPage--;
        }

        protected void Next_Click(object sender, EventArgs e)
        {
            CurrentPage++;
        }
        // Open the selected course for review.
        protected void rptCourses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int courseId;
            if (e.CommandName == "ViewCourse" && int.TryParse(Convert.ToString(e.CommandArgument), out courseId))
                ShowCourseDetails(courseId);
        }
        // Switch between the course list and course details.
        private void ShowCourseDetails(int courseId)
        {
            pnlCourseList.Visible = false;
            pnlViewCourse.Visible = true;
            ViewCourse1.LoadCourse(courseId);
        }

        private void ShowCourseList()
        {
            pnlViewCourse.Visible = false;
            pnlCourseList.Visible = true;
        }

        private void ShowMessage(string message)
        {
            pnlMessage.Visible = true;
            litMessage.Text = message;
        }

        protected string MediaUrl(object value)
        {
            return ReviewData.MediaUrl(value);
        }

        protected string StatusLabel(object value)
        {
            return ReviewData.StatusLabel(value);
        }

        protected string StatusClass(object value)
        {
            return Convert.ToString(value) == "Published" ? "approved" : Convert.ToString(value) == "Rejected" ? "rejected" : "pending";
        }
    }
    // Shared helper methods for course review database queries.
    internal static class ReviewData
    {
        internal static SqlParameter Int(string name, int value)
        {
            return new SqlParameter(name, SqlDbType.Int)
            {
                Value = value
            };
        }

        internal static DataTable Read(string sql, params SqlParameter[] parameters)
        {
            using (var connection = DatabaseManager.ConnectDB())
            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                command.Parameters.AddRange(parameters);
                var rows = new DataTable();
                adapter.Fill(rows);
                return rows;
            }
        }

        internal static string StatusLabel(object status)
        {
            return Convert.ToString(status) == "Published" ? "Approved" : Convert.ToString(status);
        }

        internal static string MediaUrl(object value)
        {
            return CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(value));
        }
    }
}
