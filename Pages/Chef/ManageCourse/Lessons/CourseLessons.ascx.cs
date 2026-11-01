using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;
using static CookingLah.Pages.Chef.ManageCourse.CourseManagementData;

namespace CookingLah.Pages.Chef.ManageCourse.Lessons
{
    public partial class CourseLessons : CourseManagementControl
    {
        protected int SelectedLessonId { get; private set; }
        public int SelectionId { get { return SelectedLessonId; } }
        protected int LessonCount { get; private set; }
        protected int BlockCount { get; private set; }
        private CourseReviewDraft draft;
        private string structure;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // Recreate repeater inputs before Web Forms loads posted values.
            Page.InitComplete += delegate {
                Run(() => {
                    draft = (CanEdit ? GetReviewDraft(CourseId, false) : null) ?? CourseReviewDraft.Load(CourseId);
                    int selected;
                    int.TryParse(Request.Form[hfSelectedLesson.UniqueID] ?? Request.QueryString["lessonId"], out selected);
                    SelectedLessonId = CourseReviewDraft.FindRow(draft.Lessons, "LessonID", selected) == null ? 0 : selected;
                    BindBuilder();
                });
            };
            Page.Load += delegate {
                if (!IsPostBack || !CanEdit) return;
                string target = Request.Form["__EVENTTARGET"] ?? "";
                bool ownAction = target.StartsWith(UniqueID + "$", StringComparison.Ordinal) ||
                    Request.Form.AllKeys.Any(key => key != null && key.StartsWith(UniqueID + "$btn", StringComparison.Ordinal));
                bool courseAction = Request.Form.AllKeys.Any(key => key != null &&
                    (key.EndsWith("$btnCancel", StringComparison.Ordinal) || key.EndsWith("$btnSaveChanges", StringComparison.Ordinal)));
                if (!ownAction && !courseAction) Run(() => CaptureLesson(false));
            };
        }

        protected string Media(object value) { return MediaUrl(Convert.ToString(value)); }
        protected string LessonTitle(object value) { return string.IsNullOrWhiteSpace(Convert.ToString(value)) ? "New lesson" : Convert.ToString(value); }

        private void BindBuilder()
        {
            LessonCount = draft.Lessons.Rows.Count;
            var lessons = draft.Lessons.Copy();
            lessons.Columns.Add("Ready", typeof(bool));
            foreach (DataRow row in lessons.Rows) row["Ready"] = draft.LessonReady((int)row["LessonID"]);
            rptLessons.DataSource = new DataView(lessons) { Sort = "LessonOrder ASC" };
            rptLessons.DataBind();
            ScriptManager manager = ScriptManager.GetCurrent(Page);
            manager.RegisterAsyncPostBackControl(btnAddLesson);
            manager.RegisterAsyncPostBackControl(btnAddArticle);
            manager.RegisterAsyncPostBackControl(btnAddImage);
            manager.RegisterAsyncPostBackControl(btnAddVideo);
            foreach (RepeaterItem item in rptLessons.Items)
            {
                manager.RegisterAsyncPostBackControl(item.FindControl("btnSelectLesson"));
                Control deleteLesson = item.FindControl("btnDeleteLesson");
                if (deleteLesson != null) manager.RegisterAsyncPostBackControl(deleteLesson);
            }
            hfSelectedLesson.Value = SelectedLessonId.ToString();
            var lesson = CourseReviewDraft.FindRow(draft.Lessons, "LessonID", SelectedLessonId);
            pnlSelectedLesson.Visible = lesson != null;
            pnlNoSelection.Visible = lesson == null;
            pnlNoLessons.Visible = LessonCount == 0;
            btnAddLesson.Visible = CanEdit;
            pnlContentToolbox.Visible = CanEdit;
            btnAddArticle.Enabled = btnAddImage.Enabled = btnAddVideo.Enabled = lesson != null;
            pnlLessonFields.Visible = CanEdit;
            pnlLessonView.Visible = !CanEdit;
            var blocks = draft.LessonContent.Select("LessonID=" + SelectedLessonId, "ContentOrder ASC");
            BlockCount = blocks.Length;
            structure = string.Join(",", draft.Lessons.Select("", "LessonOrder ASC").Select(row => row["LessonID"].ToString())) + "|" +
                string.Join(",", blocks.Select(row => row["LessonContentID"].ToString()));
            hfStructure.Value = structure;
            if (lesson == null) return;
            litLessonNumber.Text = "Lesson " + lesson["LessonOrder"];
            litTitle.Text = LessonTitle(lesson["Title"]);
            litDescription.Text = Convert.ToString(lesson["Description"]);
            txtTitle.Text = Convert.ToString(lesson["Title"]);
            txtDescription.Text = Convert.ToString(lesson["Description"]);
            pnlEmptyContent.Visible = BlockCount == 0;
            rptLessonContent.DataSource = new DataView(draft.LessonContent) { RowFilter = "LessonID=" + SelectedLessonId, Sort = "ContentOrder ASC" };
            rptLessonContent.DataBind();
            foreach (RepeaterItem item in rptLessonContent.Items)
            {
                Control deleteBlock = item.FindControl("btnDeleteBlock");
                if (deleteBlock != null) manager.RegisterAsyncPostBackControl(deleteBlock);
            }
        }

        private void CheckStructure()
        {
            if (IsPostBack && Request.Form[hfStructure.UniqueID] != structure)
                throw new InvalidOperationException("The lesson list changed in another tab. Reload before editing.");
        }

        private void Edit(Action action)
        {
            Run(() => {
                EnsureEditable();
                CheckStructure();
                draft = GetReviewDraft(CourseId, true);
                action();
            });
        }

        // Incomplete inputs stay in the working copy, never in the database.
        public bool CaptureLesson(bool validate)
        {
            if (!CanEdit || SelectedLessonId == 0) return true;
            EnsureEditable();
            CheckStructure();
            draft = GetReviewDraft(CourseId, true);
            var lesson = CourseReviewDraft.FindRow(draft.Lessons, "LessonID", SelectedLessonId);
            if (lesson == null) throw new InvalidOperationException("Lesson not found in this course.");
            var errors = new List<string>();
            errTitle.Text = errDescription.Text = errContent.Text = "";
            string title = txtTitle.Text.Trim(), description = txtDescription.Text.Trim();
            if (title.Length > 150 || (validate && title.Length == 0)) AddError(errTitle, "Lesson Title is required and must be within 150 characters.", errors);
            if (description.Length > 500 || (validate && description.Length == 0)) AddError(errDescription, "Lesson Description is required and must be within 500 characters.", errors);
            if (validate && rptLessonContent.Items.Count == 0) AddError(errContent, "Add at least one Article, Image, or Video to this Lesson.", errors);
            var changes = draft.LessonContent.Clone();
            foreach (RepeaterItem item in rptLessonContent.Items)
            {
                int id = (int)draft.LessonContent.Select("LessonID=" + SelectedLessonId, "ContentOrder ASC")[item.ItemIndex]["LessonContentID"];
                var source = CourseReviewDraft.FindRow(draft.LessonContent, "LessonContentID", id);
                if (source == null || (int)source["LessonID"] != SelectedLessonId) throw new InvalidOperationException("Content not found in this lesson.");
                changes.ImportRow(source);
                var row = changes.Rows[changes.Rows.Count - 1];
                var error = (Label)item.FindControl("errBlock");
                error.Text = "";
                row["ContentTitle"] = ((TextBox)item.FindControl("txtContentTitle")).Text.Trim();
                row["ContentText"] = ((TextBox)item.FindControl("txtContentText")).Text.Trim();
                row["Caption"] = ((TextBox)item.FindControl("txtCaption")).Text.Trim();
                string type = Convert.ToString(row["ContentType"]);
                if (Convert.ToString(row["ContentTitle"]).Length > 150 || Convert.ToString(row["Caption"]).Length > 500)
                    AddError(error, "Keep the title within 150 characters and caption within 500 characters.", errors);
                if (type == "Article")
                {
                    if (validate && string.IsNullOrWhiteSpace(Convert.ToString(row["ContentText"]))) AddError(error, "Article content is required.", errors);
                }
                else if (type == "Image" || type == "Video")
                {
                    var upload = (FileUpload)item.FindControl("fuMedia");
                    try
                    {
                        if (upload.PostedFile != null && !string.IsNullOrEmpty(upload.FileName))
                        {
                            string extension = Path.GetExtension(upload.FileName).ToLowerInvariant();
                            if (upload.PostedFile.ContentLength == 0 || (type == "Image" && extension != ".jpg" && extension != ".jpeg" && extension != ".png"))
                                throw new InvalidOperationException("Please select a valid " + type.ToLowerInvariant() + ".");
                            row["MediaPath"] = SaveUpload(upload, Convert.ToString(row["MediaPath"]), "Lesson", type == "Video");
                            draft.TrackMediaUpload(Convert.ToString(row["MediaPath"]));
                        }
                        if (validate && string.IsNullOrWhiteSpace(Media(row["MediaPath"]))) throw new InvalidOperationException("Please select a valid " + type.ToLowerInvariant() + ".");
                    }
                    catch (InvalidOperationException ex) { AddError(error, ex.Message, errors); }
                }
                else AddError(error, "Unsupported content type.", errors);
            }
            pnlValidation.Visible = errors.Count > 0;
            if (errors.Count > 0)
            {
                litValidation.Text = "Unable to save Lesson. Check the marked fields. If you chose a file, select it again before saving.";
                return false;
            }
            lesson["Title"] = title;
            lesson["Description"] = description;
            foreach (DataRow change in changes.Rows)
            {
                var row = CourseReviewDraft.FindRow(draft.LessonContent, "LessonContentID", (int)change["LessonContentID"]);
                foreach (string column in new[] { "ContentTitle", "ContentText", "Caption", "MediaPath" }) row[column] = change[column];
            }
            return true;
        }

        private static void AddError(Label label, string message, List<string> errors)
        {
            label.Text = message;
            errors.Add(message);
        }

        protected void btnAddLesson_Click(object sender, EventArgs e)
        {
            Edit(() => {
                if (!CaptureLesson(false)) return;
                var row = draft.Lessons.NewRow();
                SelectedLessonId = CourseReviewDraft.NewId(draft.Lessons, "LessonID");
                row["LessonID"] = SelectedLessonId; row["CourseID"] = CourseId;
                row["Title"] = ""; row["Description"] = "";
                row["LessonOrder"] = draft.Lessons.Rows.Count + 1;
                draft.Lessons.Rows.Add(row);
                BindBuilder();
                UpdateLessonPanel();
                litState.Text = "New lesson — unsaved changes";
            });
        }

        protected void rptLessons_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            Run(() => {
                CheckStructure();
                int id = Convert.ToInt32(e.CommandArgument);
                if (CanEdit && !CaptureLesson(false)) return;
                var lesson = CourseReviewDraft.FindRow(draft.Lessons, "LessonID", id);
                if (lesson == null) throw new InvalidOperationException("Lesson not found in this course.");
                if (e.CommandName == "SelectLesson") SelectedLessonId = id;
                else if (e.CommandName == "DeleteLesson")
                {
                    EnsureEditable(); draft = GetReviewDraft(CourseId, true);
                    foreach (DataRow block in draft.LessonContent.Select("LessonID=" + id)) draft.LessonContent.Rows.Remove(block);
                    draft.Lessons.Rows.Remove(CourseReviewDraft.FindRow(draft.Lessons, "LessonID", id));
                    draft.NormalizeLessons();
                    if (SelectedLessonId == id) SelectedLessonId = 0;
                    litState.Text = "Lesson removed from working copy.";
                }
                BindBuilder();
                UpdateLessonPanel();
            });
        }

        private void UpdateLessonPanel()
        {
            Control parent = Parent;
            while (parent != null && !(parent is UpdatePanel)) parent = parent.Parent;
            if (parent != null) ((UpdatePanel)parent).Update();
        }

        protected void AddContent_Command(object sender, CommandEventArgs e)
        {
            Edit(() => {
                if (SelectedLessonId == 0 || CourseReviewDraft.FindRow(draft.Lessons, "LessonID", SelectedLessonId) == null) throw new InvalidOperationException("Select a lesson first.");
                string type = Convert.ToString(e.CommandArgument);
                if (!new[] { "Article", "Image", "Video" }.Contains(type)) throw new InvalidOperationException("Invalid content type.");
                if (!CaptureLesson(false)) return;
                var row = draft.LessonContent.NewRow();
                row["LessonContentID"] = CourseReviewDraft.NewId(draft.LessonContent, "LessonContentID");
                row["LessonID"] = SelectedLessonId; row["ContentType"] = type;
                row["ContentOrder"] = draft.LessonContent.Select("LessonID=" + SelectedLessonId).Length + 1;
                draft.LessonContent.Rows.Add(row);
                BindBuilder();
                UpdateLessonPanel();
                litState.Text = "Unsaved changes";
            });
        }

        protected void rptLessonContent_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            Edit(() => {
                if (!CaptureLesson(false)) return;
                var row = CourseReviewDraft.FindRow(draft.LessonContent, "LessonContentID", Convert.ToInt32(e.CommandArgument));
                if (row == null || (int)row["LessonID"] != SelectedLessonId) throw new InvalidOperationException("Content not found in this lesson.");
                draft.LessonContent.Rows.Remove(row);
                var blocks = draft.LessonContent.Select("LessonID=" + SelectedLessonId, "ContentOrder ASC");
                for (int i = 0; i < blocks.Length; i++) blocks[i]["ContentOrder"] = i + 1;
                BindBuilder(); UpdateLessonPanel(); litState.Text = "Content removed — unsaved changes";
            });
        }

        protected void btnSaveOrder_Click(object sender, EventArgs e)
        {
            Edit(() => {
                if (!CaptureLesson(false)) return;
                if (hfOrderKind.Value != "lessons" && hfOrderKind.Value != "content") throw new InvalidOperationException("Invalid order request.");
                draft.Reorder(hfOrderKind.Value, SelectedLessonId, hfOrderIds.Value);
                BindBuilder();
                litState.Text = "✓ Order saved to working copy. Save Changes saves the course.";
            });
        }
    }
}
