using System;
using System.Data;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;
using static CookingLah.Pages.Chef.ManageCourse.CourseManagementData;

namespace CookingLah.Pages.Chef.ManageCourse.Lessons
{
    public partial class CourseLessons : CourseManagementControl
    {
        private int CurrentPage { get { return (int?)ViewState["CurrentPage"] ?? 0; } set { ViewState["CurrentPage"] = value; } }
        private int LessonId { get { return (int?)ViewState["LessonId"] ?? 0; } set { ViewState["LessonId"] = value; } }
        private string ImagePath { get { return (string)ViewState["ImagePath"] ?? ""; } set { ViewState["ImagePath"] = value; } }
        private string VideoPath { get { return (string)ViewState["VideoPath"] ?? ""; } set { ViewState["VideoPath"] = value; } }
        protected int LessonCount { get; private set; }

        public void LoadLessons(int courseId)
        {
            var rows = ReadDraftSection(courseId, "Lessons", "SELECT * FROM Lessons WHERE CourseID=@id ORDER BY LessonOrder,LessonID", Parameter("@id", courseId));
            LessonCount = rows.Rows.Count;
            int pageCount = Math.Max(1, (LessonCount + 4) / 5);
            CurrentPage = Math.Min(CurrentPage, pageCount - 1);
            rptLessons.DataSource = new PagedDataSource { DataSource = rows.DefaultView, AllowPaging = true, PageSize = 5, CurrentPageIndex = CurrentPage };
            rptLessons.DataBind();
            btnAddLesson.Visible = CanEdit;
            pnlEmpty.Visible = LessonCount == 0;
            pnlPager.Visible = LessonCount > 5;
            btnPrevious.Enabled = CurrentPage > 0;
            btnNext.Enabled = CurrentPage < pageCount - 1;
            lblPage.Text = string.Format("Showing Lessons {0}-{1} of {2}. Page {3} of {4}", CurrentPage * 5 + 1, Math.Min(LessonCount, (CurrentPage + 1) * 5), LessonCount, CurrentPage + 1, pageCount);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Visible || IsPostBack) return;
            int page;
            if (int.TryParse(Request.QueryString["page"], out page)) CurrentPage = Math.Max(0, page);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (Visible) Run(() => LoadLessons(CourseId));
            base.OnPreRender(e);
        }
        protected void btnAddLesson_Click(object sender, EventArgs e)
        {
            Run(() =>
            {
                EnsureEditable();
                LessonId = 0;
                txtTitle.Text = txtDescription.Text = txtInstructions.Text = ImagePath = VideoPath = "";
                litEditorTitle.Text = "Add Lesson";
                pnlEditor.Visible = true;
            });
        }
        protected void btnCancel_Click(object sender, EventArgs e) { pnlEditor.Visible = pnlDelete.Visible = false; }
        protected void rptLessons_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            Run(() =>
            {
                EnsureEditable();
                int lessonId = Convert.ToInt32(e.CommandArgument);
                DataRow row = null;
                if (HasChangesAwaitingReview(CourseId))
                    row = CourseReviewDraft.FindRow(GetReviewDraft(CourseId, false).Lessons, "LessonID", lessonId);
                else
                {
                    var rows = Read("SELECT * FROM Lessons WHERE LessonID=@lesson AND CourseID=@course", Parameter("@lesson", lessonId), Parameter("@course", CourseId));
                    if (rows.Rows.Count > 0) row = rows.Rows[0];
                }
                if (row == null) throw new InvalidOperationException("This lesson no longer exists.");
                LessonId = lessonId;
                if (e.CommandName == "EditLesson")
                {
                    txtTitle.Text = Convert.ToString(row["Title"]);
                    txtDescription.Text = Convert.ToString(row["Description"]);
                    txtInstructions.Text = Convert.ToString(row["Instructions"]);
                    ImagePath = Convert.ToString(row["LessonImage"]);
                    VideoPath = Convert.ToString(row["LessonVideo"]);
                    litEditorTitle.Text = "Edit Lesson";
                    pnlEditor.Visible = true;
                }
                else if (e.CommandName == "DeleteLesson")
                {
                    litDeleteName.Text = Convert.ToString(row["Title"]);
                    pnlDelete.Visible = true;
                }
                else if (e.CommandName == "MoveUp" || e.CommandName == "MoveDown") MoveLesson(lessonId, e.CommandName == "MoveUp" ? -1 : 1);
            });
        }
        protected void btnSaveLesson_Click(object sender, EventArgs e)
        {
            if (!pnlEditor.Visible) return;
            Run(() =>
            {
                EnsureEditable();
                string title = Required(txtTitle.Text, "a lesson title", 150);
                string description = Optional(txtDescription.Text, "the lesson description", 500);
                string instructions = Required(txtInstructions.Text, "lesson instructions");
                if (CanEdit)
                {
                    var draft = GetReviewDraft(CourseId, true);
                    DataRow staged = LessonId == 0 ? null : CourseReviewDraft.FindRow(draft.Lessons, "LessonID", LessonId);
                    if (LessonId != 0 && staged == null) throw new InvalidOperationException("This lesson no longer exists. Reload the page.");
                    string image = SaveUpload(fuImage, ImagePath);
                    string video = SaveUpload(fuVideo, VideoPath, true);
                    draft.TrackMediaUpload(image);
                    draft.TrackMediaUpload(video);
                    if (staged == null)
                    {
                        staged = draft.Lessons.NewRow();
                        staged["LessonID"] = CourseReviewDraft.NewId(draft.Lessons, "LessonID");
                        staged["CourseID"] = CourseId;
                        staged["LessonOrder"] = draft.Lessons.Rows.Count + 1;
                        draft.Lessons.Rows.Add(staged);
                    }
                    staged["Title"] = title; staged["Description"] = description; staged["Instructions"] = instructions;
                    staged["LessonImage"] = image; staged["LessonVideo"] = video;
                    Saved("Lesson changes saved.", CurrentPage);
                    return;
                }
                EditCourse(CourseId, (connection, transaction) =>
                {
                    if (LessonId > 0 && Execute(connection, transaction, "SELECT LessonID FROM Lessons WHERE LessonID=@lesson AND CourseID=@course", Parameter("@lesson", LessonId), Parameter("@course", CourseId)) == null)
                        throw new InvalidOperationException("This lesson no longer exists.");
                    string image = SaveUpload(fuImage, ImagePath);
                    string video = SaveUpload(fuVideo, VideoPath, true);
                    string sql = LessonId == 0
                        ? "INSERT INTO Lessons (CourseID,Title,Description,Instructions,LessonImage,LessonVideo,LessonOrder) SELECT @course,@title,@description,@instructions,@image,@video,ISNULL(MAX(LessonOrder),0)+1 FROM Lessons WHERE CourseID=@course"
                        : "UPDATE Lessons SET Title=@title,Description=@description,Instructions=@instructions,LessonImage=@image,LessonVideo=@video WHERE LessonID=@lesson AND CourseID=@course";
                    Execute(connection, transaction, sql, Parameter("@course", CourseId), Parameter("@lesson", LessonId),
                        Parameter("@title", title), Parameter("@description", description), Parameter("@instructions", instructions), Parameter("@image", image), Parameter("@video", video));
                });
                Saved(LessonId == 0 ? "Lesson added successfully." : "Lesson updated successfully.", CurrentPage);
            });
        }
        private void MoveLesson(int lessonId, int direction)
        {
            if (CanEdit)
            {
                var draft = GetReviewDraft(CourseId, true);
                var rows = draft.Lessons.Select("", "LessonOrder ASC,LessonID ASC");
                int index = Array.FindIndex(rows, row => Convert.ToInt32(row["LessonID"]) == lessonId);
                int target = index + direction;
                if (index < 0 || target < 0 || target >= rows.Length) return;
                int order = Convert.ToInt32(rows[index]["LessonOrder"]);
                int targetOrder = Convert.ToInt32(rows[target]["LessonOrder"]);
                rows[index]["LessonOrder"] = targetOrder;
                rows[target]["LessonOrder"] = order;
                draft.NormalizeLessons();
                CurrentPage = Math.Max(0, target / 5);
                Saved("Lesson order changes saved.", CurrentPage);
                return;
            }
            EditCourse(CourseId, (connection, transaction) =>
            {
                object result = Execute(connection, transaction, "SELECT LessonOrder FROM Lessons WHERE LessonID=@lesson AND CourseID=@course",
                    Parameter("@lesson", lessonId), Parameter("@course", CourseId));
                if (result == null) throw new InvalidOperationException("This lesson no longer exists.");
                int order = Convert.ToInt32(result);
                int destination = order + direction;
                object neighbor = Execute(connection, transaction, "SELECT LessonID FROM Lessons WHERE CourseID=@course AND LessonOrder=@order", Parameter("@course", CourseId), Parameter("@order", destination));
                if (neighbor == null) return;
                // Reserve an unused positive order while swapping the two unique positions.
                Execute(connection, transaction, @"DECLARE @spare INT=(SELECT MAX(LessonOrder)+1 FROM Lessons WHERE CourseID=@course);
                    UPDATE Lessons SET LessonOrder=@spare WHERE LessonID=@lesson AND CourseID=@course;
                    UPDATE Lessons SET LessonOrder=@order WHERE LessonID=@neighbor AND CourseID=@course;
                    UPDATE Lessons SET LessonOrder=@destination WHERE LessonID=@lesson AND CourseID=@course;",
                    Parameter("@course", CourseId), Parameter("@lesson", lessonId), Parameter("@neighbor", neighbor), Parameter("@order", order), Parameter("@destination", destination));
                CurrentPage = (destination - 1) / 5;
            });
            Saved("Lesson order updated successfully.", CurrentPage);
        }
        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            if (!pnlDelete.Visible || LessonId == 0) return;
            Run(() =>
            {
                if (CanEdit)
                {
                    var draft = GetReviewDraft(CourseId, true);
                    var row = CourseReviewDraft.FindRow(draft.Lessons, "LessonID", LessonId);
                    if (row != null) draft.Lessons.Rows.Remove(row);
                    draft.NormalizeLessons();
                    Saved("Lesson changes saved.", CurrentPage);
                    return;
                }
                EditCourse(CourseId, (connection, transaction) =>
                {
                    Execute(connection, transaction, "DELETE FROM Lessons WHERE LessonID=@lesson AND CourseID=@course", Parameter("@lesson", LessonId), Parameter("@course", CourseId));
                    NormalizeLessons(connection, transaction, CourseId);
                });
                Saved("Lesson deleted successfully.", CurrentPage);
            });
        }
        protected void btnPrevious_Click(object sender, EventArgs e) { CurrentPage = Math.Max(0, CurrentPage - 1); }
        protected void btnNext_Click(object sender, EventArgs e) { CurrentPage++; }
    }
}
