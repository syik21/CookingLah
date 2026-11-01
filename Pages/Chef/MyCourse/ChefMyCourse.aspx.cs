using CookingLah.F_Helpers;
using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;
namespace CookingLah.Pages.Chef.MyCourse
{
    public partial class ChefMyCourse : System.Web.UI.Page
    {
        private const int PageSize = 9;
        private int InstructorId { get { return Convert.ToInt32(Session["UserId"]); } }
        private int ListPage { get { return (int?)ViewState["ListPage"] ?? 0; } set { ViewState["ListPage"] = value; } }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            int instructorId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out instructorId))
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"));
                throw new HttpException(401, "Please sign in as a Chef.");
            }
            if (Read("SELECT UserID FROM Users WHERE UserID=@id AND Role='Instructor' AND AccountStatus='Active'", Parameter("@id", instructorId)).Rows.Count == 0)
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"));
                throw new HttpException(403, "Please sign in as a Chef.");
            }
            ViewStateUserKey = Session.SessionID;
            Response.Cache.SetNoStore();
            if (!IsPostBack)
            {
                ddlCategory.DataSource = Read("SELECT CategoryID,CategoryName FROM Categories ORDER BY CategoryName");
                ddlCategory.DataTextField = "CategoryName";
                ddlCategory.DataValueField = "CategoryID";
                ddlCategory.DataBind();
                ddlCategory.Items.Insert(0, new ListItem("All categories", "0"));
            }
        }
        protected override void OnPreRender(EventArgs e) { BindMyCourses(); base.OnPreRender(e); }
        private string ListStatus { get { return (string)ViewState["ListStatus"] ?? "All"; } set { ViewState["ListStatus"] = value; } }
        protected void CourseStatus_Command(object sender, CommandEventArgs e)
        {
            string status = Convert.ToString(e.CommandArgument);
            if (Array.IndexOf(new[] { "All", "Draft", "Pending", "Published", "Rejected" }, status) < 0) return;
            ListStatus = status;
            ListPage = 0;
        }

        private void BindMyCourses()
        {
            string search = txtCourseSearch.Text.Trim();
            var rows = Read(@"SELECT c.CourseID,c.Title,c.Description,c.Thumbnail,c.Status,c.DifficultyLevel,c.UpdatedAt,cat.CategoryName,
                (SELECT COUNT(*) FROM Lessons WHERE CourseID=c.CourseID) AS LessonCount,
                (SELECT COUNT(*) FROM Quizzes WHERE CourseID=c.CourseID) AS QuizCount,
                (SELECT COUNT(*) FROM PracticalActivities WHERE CourseID=c.CourseID) AS ActivityCount FROM Courses c JOIN Categories cat ON cat.CategoryID=c.CategoryID WHERE c.InstructorID=@user AND (@category=0 OR c.CategoryID=@category) AND (@search='' OR CHARINDEX(@search,c.Title)>0) ORDER BY c.UpdatedAt DESC,c.CourseID DESC", Parameter("@user", InstructorId), Parameter("@search", search), Parameter("@category", int.Parse(ddlCategory.SelectedValue)));
            var buttons = new[] { btnStatusAll, btnStatusDraft, btnStatusPending, btnStatusPublished, btnStatusRejected };
            var labels = new[] { "All courses", "Draft", "Pending review", "Published", "Rejected" };
            var descriptions = new[] { "Manage all your courses in one place.", "Continue editing and submit when ready.", "Awaiting review. Editing is temporarily disabled.", "Live courses available to learners.", "Review the feedback, make changes, and resubmit." };
            for (int i = 0; i < buttons.Length; i++)
            {
                string status = buttons[i].CommandArgument;
                int count = status == "All" ? rows.Rows.Count : rows.Select("Status='" + status + "'").Length;
                bool selected = status == ListStatus;
                buttons[i].Text = labels[i] + " (" + count + ")";
                buttons[i].CssClass = "mc-status-filter" + (selected ? " is-selected" : "");
                buttons[i].Attributes["aria-pressed"] = selected ? "true" : "false";
                if (selected)
                {
                    litListHeading.Text = labels[i] + " · " + count;
                    litListDescription.Text = descriptions[i] + (search.Length > 0 ? " Counts reflect your search." : "");
                }
            }
            var view = new DataView(rows);
            if (ListStatus != "All") view.RowFilter = "Status='" + ListStatus + "'";
            int totalPages = Math.Max(1, (view.Count + PageSize - 1) / PageSize);
            ListPage = Math.Min(ListPage, totalPages - 1);
            rptCourses.DataSource = new PagedDataSource { DataSource = view, AllowPaging = true, PageSize = PageSize, CurrentPageIndex = ListPage };
            rptCourses.DataBind();
            pnlNoCourses.Visible = rows.Rows.Count == 0 && search.Length == 0;
            pnlNoCourseMatches.Visible = view.Count == 0 && !pnlNoCourses.Visible;
            btnClearCourseSearch.Visible = search.Length > 0;
            pnlCoursePager.Visible = view.Count > PageSize;
            btnPreviousCourses.Enabled = ListPage > 0;
            btnNextCourses.Enabled = ListPage < totalPages - 1;
            lblCoursesPage.Text = "Page " + (ListPage + 1) + " of " + totalPages;
        }
        protected void btnPreviousCourses_Click(object sender, EventArgs e) { ListPage = Math.Max(0, ListPage - 1); }
        protected void btnSearchCourses_Click(object sender, EventArgs e) { ListPage = 0; }
        protected void btnClearCourseSearch_Click(object sender, EventArgs e) { txtCourseSearch.Text = ""; ListPage = 0; }
        protected void btnNextCourses_Click(object sender, EventArgs e) { ListPage++; }
    }
}
