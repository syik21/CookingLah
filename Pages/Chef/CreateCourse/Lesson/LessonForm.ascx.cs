using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.CreateCourse.Lesson
{
    public partial class LessonForm : System.Web.UI.UserControl
    {
        private const int LessonPageSize = 5;
        public int CurrentPage { get { return (int?)ViewState["CurrentPage"] ?? 0; } set { ViewState["CurrentPage"] = value; } }
        public int PageSize { get { return LessonPageSize; } }
        public int TotalPages { get { return Math.Max(1, (LessonCount + PageSize - 1) / PageSize); } }
        protected int LessonCount { get { return Lessons.Rows.Count; } }
        private CreateCoursePage Builder { get { return (CreateCoursePage)Page; } }
        private DataTable Lessons
        {
            get
            {
                if (ViewState["Lessons"] == null)
                {
                    var lessons = new DataTable();
                    lessons.Columns.Add("LessonOrder", typeof(int));
                    foreach (string name in new[] { "Title", "Description", "Instructions", "LessonImage", "LessonVideo" })
                        lessons.Columns.Add(name, typeof(string));
                    ViewState["Lessons"] = lessons;
                }
                return (DataTable)ViewState["Lessons"];
            }
            set { ViewState["Lessons"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) BindLessons();
        }

        public void LoadLessons(int courseId)
        {
            Lessons = Read("SELECT LessonOrder,Title,Description,Instructions,LessonImage,LessonVideo FROM Lessons WHERE CourseID=@id ORDER BY LessonOrder", Parameter("@id", courseId));
            CurrentPage = 0;
            BindLessons();
        }

        private void BindLessons()
        {
            for (int index = 0; index < LessonCount; index++) Lessons.Rows[index]["LessonOrder"] = index + 1;
            CurrentPage = Math.Min(CurrentPage, TotalPages - 1);
            var pages = new PagedDataSource { DataSource = Lessons.DefaultView, AllowPaging = true, PageSize = PageSize, CurrentPageIndex = CurrentPage };
            rptLessons.DataSource = pages;
            rptLessons.DataBind();
            pnlEmptyLessons.Visible = LessonCount == 0;
            pnlLessonPager.Visible = LessonCount > PageSize;
            btnPreviousLessons.Enabled = CurrentPage > 0;
            btnNextLessons.Enabled = CurrentPage < TotalPages - 1;
            lblLessonPage.Text = "Page " + (CurrentPage + 1) + " of " + TotalPages + " (" + LessonCount + " lessons)";
            btnAddLesson.Text = LessonCount == 0 ? "+ Add first lesson" : "+ Add another lesson";
            Lessons = Lessons;
        }

        public void CaptureLessons()
        {
            Builder.MarkUnsaved();
            foreach (RepeaterItem item in rptLessons.Items)
            {
                var lesson = Lessons.Rows[CurrentPage * PageSize + item.ItemIndex];
                lesson["Title"] = ((TextBox)item.FindControl("txtLessonTitle")).Text.Trim();
                lesson["Description"] = ((TextBox)item.FindControl("txtLessonDescription")).Text.Trim();
                lesson["Instructions"] = ((TextBox)item.FindControl("txtLessonInstructions")).Text.Trim();
                lesson["LessonImage"] = SaveUpload((FileUpload)item.FindControl("fuLessonImage"), Convert.ToString(lesson["LessonImage"]));
                lesson["LessonVideo"] = SaveUpload((FileUpload)item.FindControl("fuLessonVideo"), Convert.ToString(lesson["LessonVideo"]), true);
            }
            Lessons = Lessons;
        }

        public List<string> ValidateLessons()
        {
            var errors = new List<string>();
            if (LessonCount == 0) errors.Add("Please add at least one lesson.");
            foreach (DataRow lesson in Lessons.Rows)
            {
                string number = Convert.ToString(lesson["LessonOrder"]);
                if (string.IsNullOrWhiteSpace(Convert.ToString(lesson["Title"]))) errors.Add("Please enter the title for Lesson " + number + ".");
                if (Convert.ToString(lesson["Title"]).Length > 150) errors.Add("Keep the title for Lesson " + number + " within 150 characters.");
                if (Convert.ToString(lesson["Description"]).Length > 500) errors.Add("Keep the description for Lesson " + number + " within 500 characters.");
                if (string.IsNullOrWhiteSpace(Convert.ToString(lesson["Instructions"]))) errors.Add("Please enter instructions for Lesson " + number + ".");
            }
            return errors;
        }

        public void SaveLessons(int courseId)
        {
            // Only editable drafts reach this transaction; replace their ordered lesson list together.
            EditDraft(courseId, (connection, transaction) => {
                Execute(connection, transaction, "DELETE FROM Lessons WHERE CourseID=@id", Parameter("@id", courseId));
                foreach (DataRow lesson in Lessons.Rows)
                    Execute(connection, transaction,
                        "INSERT INTO Lessons (CourseID,Title,Description,Instructions,LessonImage,LessonVideo,LessonOrder,CreatedAt) " +
                        "VALUES (@id,@title,@description,@instructions,@image,@video,@order,GETDATE())",
                        Parameter("@id", courseId), Parameter("@title", lesson["Title"]), Parameter("@description", lesson["Description"]),
                        Parameter("@instructions", lesson["Instructions"]), Parameter("@image", lesson["LessonImage"]),
                        Parameter("@video", lesson["LessonVideo"]), Parameter("@order", lesson["LessonOrder"]));
            });
            BindLessons();
        }

        protected void btnAddLesson_Click(object sender, EventArgs e)
        {
            Builder.RunAction(() => {
                CaptureLessons();
                Lessons.Rows.Add(LessonCount + 1, "", "", "", "", "");
                CurrentPage = TotalPages - 1;
                BindLessons();
                Builder.MarkUnsaved();
            });
        }

        protected void rptLessons_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            Builder.RunAction(() => {
                CaptureLessons();
                int index = CurrentPage * PageSize + e.Item.ItemIndex;
                if (e.CommandName == "DeleteLesson") Lessons.Rows.RemoveAt(index);
                else
                {
                    int destination = e.CommandName == "MoveUp" ? index - 1 : index + 1;
                    if (destination < 0 || destination >= LessonCount) return;
                    object[] lesson = Lessons.Rows[index].ItemArray;
                    Lessons.Rows[index].ItemArray = Lessons.Rows[destination].ItemArray;
                    Lessons.Rows[destination].ItemArray = lesson;
                    CurrentPage = destination / PageSize;
                }
                BindLessons();
                Builder.MarkUnsaved();
            });
        }

        private void ChangePage(int direction)
        {
            Builder.RunAction(() => {
                CaptureLessons();
                CurrentPage = Math.Max(0, Math.Min(TotalPages - 1, CurrentPage + direction));
                BindLessons();
            });
        }
        protected void btnPreviousLessons_Click(object sender, EventArgs e) { ChangePage(-1); }
        protected void btnNextLessons_Click(object sender, EventArgs e) { ChangePage(1); }
    }
}
