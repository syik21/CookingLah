using CookingLah.F_Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;
using static CookingLah.Pages.Chef.ManageCourse.CourseManagementData;

namespace CookingLah.Pages.Chef.ManageCourse
{
    public partial class ManageCoursePage : System.Web.UI.Page
    {
        public int CourseId { get; private set; }
        public bool CanEdit { get; private set; }
        public DataRow Course { get; private set; }
        public string CurrentTab { get; private set; }
        private int InstructorId { get { return Convert.ToInt32(Session["UserId"]); } }
        private int ListPage { get { return (int?)ViewState["ListPage"] ?? 0; } set { ViewState["ListPage"] = value; } }

        protected override void OnInit(EventArgs e)
        {
            int courseId;
            if (Request.QueryString["courseId"] != null &&
                (!int.TryParse(Request.QueryString["courseId"], out courseId) || courseId <= 0))
                throw new HttpException(404, "Unable to find the selected course.");
            int.TryParse(Request.QueryString["courseId"], out courseId);
            CourseId = courseId;
            if (Session["ManageCourseToken"] == null) Session["ManageCourseToken"] = Guid.NewGuid().ToString("N");
            // Bind postback state to both the session and the URL's course.
            ViewStateUserKey = Convert.ToString(Session["ManageCourseToken"]) + ":" + CourseId;
            base.OnInit(e);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            int instructorId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out instructorId))
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"));
                return;
            }
            try
            {
                if (Read("SELECT UserID FROM Users WHERE UserID=@id AND Role='Instructor' AND AccountStatus='Active'",
                    Parameter("@id", instructorId)).Rows.Count == 0)
                    throw new HttpException(403, "Please sign in with an active Chef account.");
                if (CourseId > 0) LoadCourse();
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Manage course authorization: {0}", ex);
                throw new HttpException(503, "Course management is temporarily unavailable.");
            }
            CurrentTab = Request.QueryString["tab"] ?? "overview";
            if (Array.IndexOf(new[] { "overview", "recipe", "lessons", "quiz", "activity", "preview" }, CurrentTab) < 0) CurrentTab = "overview";
            pnlMyCourses.Visible = CourseId == 0;
            pnlWorkspace.Visible = CourseId > 0;
            Overview.Visible = CourseId > 0 && CurrentTab == "overview";
            Recipe.Visible = CourseId > 0 && CurrentTab == "recipe";
            Lessons.Visible = CourseId > 0 && CurrentTab == "lessons";
            Quiz.Visible = CourseId > 0 && CurrentTab == "quiz";
            Activity.Visible = CourseId > 0 && CurrentTab == "activity";
            Preview.Visible = CourseId > 0 && CurrentTab == "preview";
        }

        private void LoadCourse()
        {
            var rows = Read(@"SELECT c.*,cat.CategoryName,
                CASE WHEN fc.FoodCultureID IS NULL THEN N'Not linked to a Traditional Food'
                     ELSE fc.DishName + NCHAR(8212) + fc.Country END AS FoodDisplayName
                FROM Courses c
                JOIN Categories cat ON cat.CategoryID=c.CategoryID
                LEFT JOIN FoodCultures fc ON fc.FoodCultureID=c.FoodCultureID
                WHERE c.CourseID=@id AND c.InstructorID=@user",
                Parameter("@id", CourseId), Parameter("@user", InstructorId));
            if (rows.Rows.Count == 0) throw new HttpException(404, "Unable to find this course, or you do not have permission to manage it.");
            Course = rows.Rows[0];
            CourseManagementData.ApplyDraftCourse(CourseId, Course);
            CanEdit = Convert.ToString(Course["Status"]) != "Pending";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            pnlMessage.Visible = false;
            if (Session["Email"] == null)
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"));
            }
            if (IsPostBack && Request.Form["discardReview"] == "1")
            {
                CourseManagementData.DiscardChangesAwaitingReview(CourseId);
                string destination = Request.Form["reviewLeaveDestination"] ?? "";
                Uri target = null;
                bool safeDestination = destination.StartsWith("/", StringComparison.Ordinal) &&
                    !destination.StartsWith("//", StringComparison.Ordinal) &&
                    destination.IndexOf('\\') < 0 && destination.IndexOf('\r') < 0 && destination.IndexOf('\n') < 0 &&
                    Uri.TryCreate(Request.Url, destination, out target) &&
                    string.Equals(target.Scheme, Request.Url.Scheme, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(target.Authority, Request.Url.Authority, StringComparison.OrdinalIgnoreCase);
                if (!safeDestination) target = new Uri(Request.Url, ResolveUrl(TabUrl(CurrentTab)));
                Response.Redirect(target.AbsoluteUri, false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            if (!IsPostBack)
            {
                string message = Convert.ToString(Session["ManageCourseFeedback"]);
                Session.Remove("ManageCourseFeedback");
                if (message.Length > 0) ShowMessage(message, false);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            RunAction(() =>
            {
                if (CourseId == 0) BindMyCourses();
                else
                {
                    LoadCourse();
                    litCourseTitle.Text = Convert.ToString(Course["Title"]);
                    litCourseMeta.Text = Course["CategoryName"] + " \u00b7 " + Course["DifficultyLevel"];
                    lblCourseStatus.Text = Convert.ToString(Course["Status"]);
                    lblCourseStatus.CssClass = "mc-status mc-status-" + Convert.ToString(Course["Status"]).ToLowerInvariant();
                    pnlPending.Visible = !CanEdit;
                    btnDeleteCourse.Visible = CanEdit;
                    string courseStatus = Convert.ToString(Course["Status"]);
                    bool hasUnsubmittedChanges = CourseManagementData.HasChangesAwaitingReview(CourseId);
                    bool changedPublishedCourse = courseStatus == "Published" && hasUnsubmittedChanges;
                    btnSubmitReview.Visible = CanEdit && (courseStatus == "Draft" || courseStatus == "Rejected" || changedPublishedCourse);
                    pnlWorkspace.Attributes["data-review-required"] = hasUnsubmittedChanges ? "true" : "false";
                    if (Preview.Visible) Preview.LoadPreview(CourseId);
                }
            });
            base.OnPreRender(e);
        }

        public string TabUrl(string tab) { return "ManageCoursePage.aspx?courseId=" + CourseId + "&tab=" + tab; }
        protected string TabClass(string tab) { return "course-tab" + (CurrentTab == tab ? " active" : ""); }
        protected string TabCurrent(string tab) { return CurrentTab == tab ? "page" : "false"; }

        public void RunAction(Action action)
        {
            try { action(); }
            catch (InvalidOperationException ex) { ShowMessage(ex.Message, true); }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Manage course: {0}", ex);
                ShowMessage("Unable to save this change. The content may have changed. Reload the page and try again.", true);
            }
            catch (IOException ex)
            {
                System.Diagnostics.Trace.TraceError("Course media: {0}", ex);
                ShowMessage("Unable to save the uploaded file. Please try again.", true);
            }
        }

        private void ShowMessage(string message, bool error)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = "mc-message " + (error ? "mc-error" : "mc-success");
            pnlMessage.Attributes["role"] = error ? "alert" : "status";
            litMessage.Text = message;
        }

        public void Saved(string message, int? listPage = null)
        {
            if (Course != null && CourseManagementData.HasChangesAwaitingReview(CourseId))
                message += " Your changes are held in this session and have not been submitted for review.";
            Session["ManageCourseFeedback"] = message;
            Response.Redirect(TabUrl(CurrentTab) + (listPage.HasValue ? "&page=" + listPage.Value : ""));
        }

        protected void btnEditCourse_Click(object sender, EventArgs e)
        {
            Response.Redirect(TabUrl("overview") + "&edit=1");
        }

        protected void btnDeleteCourse_Click(object sender, EventArgs e)
        {
            RunAction(() =>
            {
                if (!CanEdit) throw new InvalidOperationException("This course cannot be edited while it is under review.");
                pnlDeleteCourse.Visible = true;
                litDeleteCourseName.Text = Convert.ToString(Course["Title"]);
            });
        }
        protected void btnCancelDeleteCourse_Click(object sender, EventArgs e) { pnlDeleteCourse.Visible = false; }
        protected void btnConfirmDeleteCourse_Click(object sender, EventArgs e)
        {
            if (!pnlDeleteCourse.Visible) return;
            RunAction(() =>
            {
                EditCourse(CourseId, (connection, transaction) =>
                    Execute(connection, transaction, "DELETE FROM Courses WHERE CourseID=@id AND InstructorID=@user",
                        Parameter("@id", CourseId), Parameter("@user", InstructorId)));
                Session["ManageCourseFeedback"] = "Course deleted successfully.";
                Response.Redirect("ManageCoursePage.aspx");
            });
        }

        protected void btnSubmitReview_Click(object sender, EventArgs e)
        {
            RunAction(() =>
            {
                bool changedPublishedCourse = Course != null && Convert.ToString(Course["Status"]) == "Published" &&
                    CourseManagementData.HasChangesAwaitingReview(CourseId, Course["UpdatedAt"]);
                EditCourse(CourseId, (connection, transaction) =>
                {
                    CourseReviewDraft pendingDraft = CourseManagementData.GetReviewDraft(CourseId, false);
                    if (pendingDraft != null)
                    {
                        pendingDraft.Apply(connection, transaction, CourseId);
                    }
                    int complete = Convert.ToInt32(Execute(connection, transaction, @"
                        SELECT COUNT(*) FROM Courses c WHERE c.CourseID=@id
                        AND (c.Status IN ('Draft','Rejected') OR (c.Status='Published' AND @changed=1))
                        AND LEN(LTRIM(RTRIM(c.Title)))>0 AND LEN(LTRIM(RTRIM(c.Description)))>0
                        AND EXISTS(SELECT 1 FROM CourseRecipes WHERE CourseID=c.CourseID)
                        AND EXISTS(SELECT 1 FROM Lessons WHERE CourseID=c.CourseID)
                        AND NOT EXISTS(SELECT 1 FROM Quizzes q WHERE q.CourseID=c.CourseID AND
                            (NOT EXISTS(SELECT 1 FROM QuizQuestions WHERE QuizID=q.QuizID)
                            OR EXISTS(SELECT 1 FROM QuizQuestions qq WHERE qq.QuizID=q.QuizID AND
                                ((SELECT COUNT(*) FROM QuizOptions WHERE QuestionID=qq.QuestionID)<2 OR
                                 (SELECT COUNT(*) FROM QuizOptions WHERE QuestionID=qq.QuestionID AND IsCorrect=1)<>1))))",
                        Parameter("@id", CourseId), Parameter("@changed", changedPublishedCourse ? 1 : 0)));
                    if (complete == 0) throw new InvalidOperationException("Before submitting, add a title, description, recipe and at least one lesson. Any quiz must have questions with at least two options and exactly one correct answer.");
                    Execute(connection, transaction, "UPDATE Courses SET Status='Pending',RejectionReason=NULL WHERE CourseID=@id", Parameter("@id", CourseId));
                }, submittingForReview: true);
                Saved("Course submitted for review.");
            });
        }

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
            var rows = Read("SELECT c.CourseID,c.Title,c.Thumbnail,c.Status,cat.CategoryName FROM Courses c JOIN Categories cat ON cat.CategoryID=c.CategoryID WHERE c.InstructorID=@user AND (@search='' OR CHARINDEX(@search,c.Title)>0) ORDER BY c.UpdatedAt DESC,c.CourseID DESC", Parameter("@user", InstructorId), Parameter("@search", search));
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
            int totalPages = Math.Max(1, (view.Count + 9) / 10);
            ListPage = Math.Min(ListPage, totalPages - 1);
            rptCourses.DataSource = new PagedDataSource { DataSource = view, AllowPaging = true, PageSize = 10, CurrentPageIndex = ListPage };
            rptCourses.DataBind();
            pnlNoCourses.Visible = rows.Rows.Count == 0 && search.Length == 0;
            pnlNoCourseMatches.Visible = view.Count == 0 && !pnlNoCourses.Visible;
            btnClearCourseSearch.Visible = search.Length > 0;
            pnlCoursePager.Visible = view.Count > 10;
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
