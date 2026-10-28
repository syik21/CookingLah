using CookingLah.F_Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;
using static CookingLah.Pages.Chef.ManageCourse.CourseManagementData;

namespace CookingLah.Pages.Chef.ManageCourse
{
    public partial class ManageCoursePage : System.Web.UI.Page
    {
        public int CourseId { get; private set; }
        public bool CanEdit { get { return Course != null && Convert.ToString(Course["Status"]) != "Pending" && Session["BuilderEditing_" + CourseId] != null; } }
        public DataRow Course { get; private set; }
        public string CurrentTab { get; private set; }
        public int SetupPercent { get; private set; }
        public string ReorderToken { get; private set; }
        private CourseReviewDraft setup;
        private int InstructorId { get { return Convert.ToInt32(Session["UserId"]); } }

        protected override void OnInit(EventArgs e)
        {
            int instructorId, courseId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out instructorId))
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"));
                return;
            }
                
            if (!int.TryParse(Request.QueryString["courseId"], out courseId) || courseId <= 0)
                throw new HttpException(404, "Please select a course from My Courses.");
            CourseId = courseId;
            ReorderToken = Convert.ToString(Session["ManageCourseReorderToken"]);
            if (string.IsNullOrEmpty(ReorderToken))
            {
                ReorderToken = Guid.NewGuid().ToString("N");
                Session["ManageCourseReorderToken"] = ReorderToken;
            }
            ViewStateUserKey = Session.SessionID + ":" + CourseId;
            base.OnInit(e);
            Response.Cache.SetNoStore();
            if (Read("SELECT UserID FROM Users WHERE UserID=@id AND Role='Instructor' AND AccountStatus='Active'", Parameter("@id", instructorId)).Rows.Count == 0)
                throw new HttpException(403, "Please sign in as an active Chef.");
            LoadCourse();
            CurrentTab = Request.Form[hfActiveTab.UniqueID] ?? Request.QueryString["section"] ?? Request.QueryString["tab"] ?? "overview";
            if (!new[] { "overview", "recipe", "lessons", "quiz", "activity", "review" }.Contains(CurrentTab)) CurrentTab = "overview";
            // Render all sections; tabs switch them without a request.
            hfActiveTab.Value = CurrentTab;
        }

        private void LoadCourse()
        {
            var rows = Read(@"SELECT c.*,cat.CategoryName,
                CASE WHEN fc.FoodCultureID IS NULL THEN N'Not linked to a Traditional Food'
                ELSE fc.DishName + NCHAR(8212) + fc.Country END AS FoodDisplayName
                FROM Courses c JOIN Categories cat ON cat.CategoryID=c.CategoryID
                LEFT JOIN FoodCultures fc ON fc.FoodCultureID=c.FoodCultureID
                WHERE c.CourseID=@id AND c.InstructorID=@user", Parameter("@id", CourseId), Parameter("@user", InstructorId));
            if (rows.Rows.Count == 0) throw new HttpException(404, "Course not found or access denied.");
            Course = rows.Rows[0];
            if (Convert.ToString(Course["Status"]) == "Pending")
            {
                Session.Remove("BuilderEditing_" + CourseId);
                DiscardChangesAwaitingReview(CourseId);
            }
            if (CanEdit) ApplyDraftCourse(CourseId, Course);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Form.Enctype = "multipart/form-data";
            if (!IsPostBack && Session["ManageCourseFeedback"] != null)
            {
                ShowMessage(Convert.ToString(Session["ManageCourseFeedback"]), false);
                Session.Remove("ManageCourseFeedback");
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            RunAction(() => {
                LoadCourse();
                string status = Convert.ToString(Course["Status"]);
                litCourseTitle.Text = Convert.ToString(Course["Title"]);
                litCourseMeta.Text = Course["CategoryName"] + " · " + Course["DifficultyLevel"];
                object updated = Course["UpdatedAt"] == DBNull.Value ? Course["CreatedAt"] : Course["UpdatedAt"];
                litUpdated.Text = Convert.ToDateTime(updated).ToString("dd MMM yyyy, HH:mm");
                lblCourseStatus.Text = status;
                lblCourseStatus.CssClass = "mc-status mc-status-" + status.ToLowerInvariant();
                btnEditCourse.Visible = status != "Pending" && !CanEdit;
                btnSaveChanges.Visible = btnCancel.Visible = CanEdit;
                pnlPending.Visible = status == "Pending";
                pnlRejected.Visible = status == "Rejected";
                litRejection.Text = Convert.ToString(Course["RejectionReason"]);
                litSaveState.Text = CanEdit ? "Edit mode — apply section edits, then Save Changes to save the course." : "View mode";
                if (CanEdit && HasChangesAwaitingReview(CourseId)) litSaveState.Text = "Unsaved changes — Save Changes to persist all sections.";
                setup = CanEdit ? GetReviewDraft(CourseId, false) : null;
                setup = setup ?? CourseReviewDraft.Load(CourseId);
                var checks = setup.Readiness();
                SetupPercent = checks.Count(x => x.Value) * 20;
                rptSetup.DataSource = checks.Select(x => new { Name = x.Key, Marker = x.Value ? "✓" : "○" });
                rptSetup.DataBind();
                pnlReadiness.Visible = status == "Draft" || status == "Rejected";
                litReadiness.Text = SetupPercent == 100 ? "All sections are complete. Save any edits before submitting." : checks.Count(x => !x.Value) + " section(s) still need attention: " + string.Join(", ", checks.Where(x => !x.Value).Select(x => x.Key));
                btnSubmitReview.Text = status == "Rejected" ? "Resubmit for Review" : "Submit for Review";
                btnSubmitReview.Enabled = SetupPercent == 100 && !HasChangesAwaitingReview(CourseId);
                btnReorder.Visible = CanEdit;
            });
            base.OnPreRender(e);
        }

        public string TabUrl(string tab) { return "ManageCoursePage.aspx?courseId=" + CourseId + "&tab=" + tab; }
        public void SetCurrentTab(string tab)
        {
            if (!new[] { "overview", "recipe", "lessons", "quiz", "activity", "review" }.Contains(tab)) return;
            CurrentTab = tab;
            hfActiveTab.Value = tab;
        }
        public string LessonUrl(int lessonId) { return "~/Pages/Chef/ManageCourse/ManageCoursePage.aspx?courseId=" + CourseId + "&tab=lessons&lessonId=" + lessonId + "#courseContent"; }
        protected string TabClass(string tab) { return "course-tab" + (CurrentTab == tab ? " active" : ""); }

        public void RunAction(Action action)
        {
            try { action(); }
            catch (InvalidOperationException ex) { ShowMessage(ex.Message, true); }
            catch (SqlException ex) { System.Diagnostics.Trace.TraceError("Course builder: {0}", ex); ShowMessage("Unable to save. Reload the course and try again.", true); }
            catch (IOException ex) { System.Diagnostics.Trace.TraceError("Course upload: {0}", ex); ShowMessage("Unable to save the uploaded file.", true); }
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
            Session["ManageCourseFeedback"] = "Section changes applied. Use Save Changes to save the course.";
            RedirectCurrent();
        }
        public void RedirectCurrent()
        {
            string url = TabUrl(CurrentTab);
            if (CurrentTab == "lessons" && Lessons.SelectionId != 0) url += "&lessonId=" + Lessons.SelectionId;
            if (CurrentTab == "quiz" && Quiz.SelectionId != 0) url += "&questionId=" + Quiz.SelectionId;
            Response.Redirect(url);
        }
        protected void btnEditCourse_Click(object sender, EventArgs e)
        {
            RunAction(() => {
                if (Convert.ToString(Course["Status"]) == "Pending") throw new InvalidOperationException("Pending courses are read-only.");
                Session["BuilderEditing_" + CourseId] = true;
                RedirectCurrent();
            });
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            DiscardChangesAwaitingReview(CourseId);
            Session.Remove("BuilderEditing_" + CourseId);
            Response.Redirect(TabUrl("overview"));
        }
        protected void btnBackToCourses_Click(object sender, EventArgs e)
        {
            DiscardChangesAwaitingReview(CourseId);
            Session.Remove("BuilderEditing_" + CourseId);
            Response.Redirect("../MyCourse/ChefMyCourse.aspx");
        }
        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            RunAction(() => {
                if (!CanEdit) throw new InvalidOperationException("Click Edit Course before saving.");
                if (!Lessons.CaptureLesson(true))
                {
                    CurrentTab = "lessons";
                    hfActiveTab.Value = CurrentTab;
                    return;
                }
                if (!Quiz.CaptureQuiz(true))
                {
                    CurrentTab = "quiz";
                    hfActiveTab.Value = CurrentTab;
                    return;
                }
                EditCourse(CourseId, (connection, transaction) => {
                    var draft = GetReviewDraft(CourseId, false);
                    if (draft != null) draft.Apply(connection, transaction, CourseId);
                });
                DiscardChangesAwaitingReview(CourseId);
                Session.Remove("BuilderEditing_" + CourseId);
                Session["ManageCourseFeedback"] = "✓ Changes saved.";
                Response.Redirect(TabUrl(CurrentTab));
            });
        }
        protected void btnSubmitReview_Click(object sender, EventArgs e)
        {
            RunAction(() => {
                if (HasChangesAwaitingReview(CourseId)) throw new InvalidOperationException("Save Changes before submitting for review.");
                EditCourse(CourseId, (connection, transaction) => {
                    string status = Convert.ToString(Execute(connection, transaction, "SELECT Status FROM Courses WHERE CourseID=@id", Parameter("@id", CourseId)));
                    if (status != "Draft" && status != "Rejected") throw new InvalidOperationException("Only Draft or Rejected courses can be submitted.");
                    var checks = CourseReviewDraft.Load(CourseId).Readiness();
                    if (checks.Any(x => !x.Value)) throw new InvalidOperationException("Complete these sections before submitting: " + string.Join(", ", checks.Where(x => !x.Value).Select(x => x.Key)));
                    Execute(connection, transaction, "UPDATE Courses SET Status='Pending',RejectionReason=NULL,UpdatedAt=GETDATE() WHERE CourseID=@id", Parameter("@id", CourseId));
                }, submittingForReview: true);
                Session.Remove("BuilderEditing_" + CourseId);
                Session["ManageCourseFeedback"] = "Course submitted for review successfully.";
                RedirectCurrent();
            });
        }
        protected void btnReorder_Click(object sender, EventArgs e)
        {
            RunAction(() => {
                if (!CanEdit) throw new InvalidOperationException("Click Edit Course before changing order.");
                int parentId;
                int.TryParse(hfOrderParent.Value, out parentId);
                var draft = GetReviewDraft(CourseId, true);
                draft.Reorder(hfOrderKind.Value, parentId, hfOrderIds.Value);
                Session["ManageCourseFeedback"] = "Order updated. Use Save Changes to save the course.";
                RedirectCurrent();
            });
        }

    }
}
