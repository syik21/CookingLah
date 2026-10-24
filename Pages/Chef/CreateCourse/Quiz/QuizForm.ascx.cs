using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.CreateCourse.Quiz
{
    public partial class QuizForm : System.Web.UI.UserControl
    {
        private const int QuestionPageSize = 5;
        public int CurrentPage { get { return (int?)ViewState["CurrentPage"] ?? 0; } set { ViewState["CurrentPage"] = value; } }
        public int PageSize { get { return QuestionPageSize; } }
        public int TotalPages { get { return Math.Max(1, (Questions.Rows.Count + PageSize - 1) / PageSize); } }
        private CreateCoursePage Builder { get { return (CreateCoursePage)Page; } }
        private DataTable Questions { get { return QuizData.Tables["Questions"]; } }
        private DataTable Options { get { return QuizData.Tables["Options"]; } }
        private DataSet QuizData
        {
            get
            {
                if (ViewState["QuizData"] == null)
                {
                    var quiz = new DataSet();
                    var questions = quiz.Tables.Add("Questions");
                    questions.Columns.Add("QuestionKey", typeof(int));
                    questions.Columns.Add("QuestionOrder", typeof(int));
                    questions.Columns.Add("QuestionText", typeof(string));
                    var options = quiz.Tables.Add("Options");
                    options.Columns.Add("OptionKey", typeof(int));
                    options.Columns.Add("QuestionKey", typeof(int));
                    options.Columns.Add("OptionText", typeof(string));
                    options.Columns.Add("IsCorrect", typeof(bool));
                    ViewState["QuizData"] = quiz;
                }
                return (DataSet)ViewState["QuizData"];
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) BindQuestions();
        }

        public void LoadQuiz(int courseId)
        {
            txtQuizTitle.Text = "";
            txtQuizDescription.Text = "";
            txtPassingScore.Text = "60";
            Questions.Clear();
            Options.Clear();
            var rows = Read("SELECT TOP 1 * FROM Quizzes WHERE CourseID=@id ORDER BY QuizID", Parameter("@id", courseId));
            if (rows.Rows.Count == 0) { BindQuestions(); return; }
            var quiz = rows.Rows[0];
            txtQuizTitle.Text = Convert.ToString(quiz["Title"]);
            txtQuizDescription.Text = Convert.ToString(quiz["Description"]);
            txtPassingScore.Text = Convert.ToString(quiz["PassingScore"]);
            var questions = Read("SELECT QuestionID,QuestionOrder,QuestionText FROM QuizQuestions WHERE QuizID=@id ORDER BY QuestionOrder", Parameter("@id", quiz["QuizID"]));
            foreach (DataRow question in questions.Rows)
                Questions.Rows.Add(question["QuestionID"], question["QuestionOrder"], question["QuestionText"]);
            var options = Read("SELECT o.* FROM QuizOptions o JOIN QuizQuestions q ON q.QuestionID=o.QuestionID WHERE q.QuizID=@id ORDER BY o.OptionID", Parameter("@id", quiz["QuizID"]));
            foreach (DataRow option in options.Rows)
                Options.Rows.Add(option["OptionID"], option["QuestionID"], option["OptionText"], option["IsCorrect"]);
            CurrentPage = 0;
            BindQuestions();
        }

        private DataRow[] QuestionOptions(int questionKey)
        {
            return Options.Select("QuestionKey=" + questionKey, "OptionKey ASC");
        }

        protected string AnswerGroup(object questionKey)
        {
            return ClientID + "_answer_" + Convert.ToInt32(questionKey);
        }

        private void BindQuestions()
        {
            for (int index = 0; index < Questions.Rows.Count; index++) Questions.Rows[index]["QuestionOrder"] = index + 1;
            CurrentPage = Math.Min(CurrentPage, TotalPages - 1);
            var pages = new PagedDataSource { DataSource = Questions.DefaultView, AllowPaging = true, PageSize = PageSize, CurrentPageIndex = CurrentPage };
            rptQuestions.DataSource = pages;
            rptQuestions.DataBind();
            int count = Questions.Rows.Count;
            pnlEmptyQuestions.Visible = count == 0;
            pnlQuestionPager.Visible = count > PageSize;
            btnPreviousQuestions.Enabled = CurrentPage > 0;
            btnNextQuestions.Enabled = CurrentPage < TotalPages - 1;
            lblQuestionPage.Text = "Questions " + (CurrentPage * PageSize + 1) + "-" + Math.Min(count, (CurrentPage + 1) * PageSize) + " of " + count + ". Page " + (CurrentPage + 1) + " of " + TotalPages;
            btnAddQuestion.Text = count == 0 ? "+ Add first question" : "+ Add another question";
            ViewState["QuizData"] = QuizData;
        }

        protected void rptQuestions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            var question = ((DataRowView)e.Item.DataItem).Row;
            var options = (Repeater)e.Item.FindControl("rptQuestionOptions");
            options.DataSource = QuestionOptions((int)question["QuestionKey"]).Select(row => new {
                OptionKey = row["OptionKey"], QuestionKey = row["QuestionKey"], OptionText = row["OptionText"], IsCorrect = row["IsCorrect"]
            });
            options.DataBind();
        }

        public void CaptureQuiz()
        {
            Builder.MarkUnsaved();
            foreach (RepeaterItem item in rptQuestions.Items)
            {
                var question = Questions.Rows[CurrentPage * PageSize + item.ItemIndex];
                question["QuestionText"] = ((TextBox)item.FindControl("txtQuestionText")).Text.Trim();
                int questionKey = (int)question["QuestionKey"];
                DataRow[] options = QuestionOptions(questionKey);
                var optionRepeater = (Repeater)item.FindControl("rptQuestionOptions");
                foreach (RepeaterItem optionItem in optionRepeater.Items)
                {
                    var option = options[optionItem.ItemIndex];
                    option["OptionText"] = ((TextBox)optionItem.FindControl("txtOptionText")).Text.Trim();
                    option["IsCorrect"] = Request.Form[AnswerGroup(questionKey)] == Convert.ToString(option["OptionKey"]);
                }
            }
            ViewState["QuizData"] = QuizData;
        }

        private bool HasQuiz()
        {
            return Questions.Rows.Count > 0 || !string.IsNullOrWhiteSpace(txtQuizTitle.Text + txtQuizDescription.Text);
        }

        public List<string> ValidateQuiz()
        {
            var errors = new List<string>();
            if (!HasQuiz()) return errors;
            if (string.IsNullOrWhiteSpace(txtQuizTitle.Text)) errors.Add("Please enter the quiz title.");
            if (txtQuizTitle.Text.Length > 150) errors.Add("Keep the quiz title within 150 characters.");
            if (txtQuizDescription.Text.Length > 500) errors.Add("Keep the quiz description within 500 characters.");
            int passingScore;
            if (!int.TryParse(txtPassingScore.Text, out passingScore) || passingScore < 0 || passingScore > 100)
                errors.Add("Enter a passing score between 0 and 100.");
            if (Questions.Rows.Count == 0) errors.Add("Please add at least one question, or clear the quiz title and description to skip it.");
            foreach (DataRow question in Questions.Rows)
            {
                string number = Convert.ToString(question["QuestionOrder"]);
                if (string.IsNullOrWhiteSpace(Convert.ToString(question["QuestionText"]))) errors.Add("Please enter the text for Question " + number + ".");
                var options = QuestionOptions((int)question["QuestionKey"]);
                if (options.Length < 2) errors.Add("Add at least two options for Question " + number + ".");
                if (options.Any(option => string.IsNullOrWhiteSpace(Convert.ToString(option["OptionText"])))) errors.Add("Fill in every option for Question " + number + ".");
                if (options.Count(option => (bool)option["IsCorrect"]) != 1) errors.Add("Select exactly one correct answer for Question " + number + ".");
            }
            return errors;
        }

        public void SaveQuiz(int courseId)
        {
            int passingScore = 60;
            if (HasQuiz() && (!int.TryParse(txtPassingScore.Text, out passingScore) || passingScore < 0 || passingScore > 100))
                throw new InvalidOperationException("Enter a passing score between 0 and 100.");
            EditDraft(courseId, (connection, transaction) => {
                Execute(connection, transaction, "DELETE FROM Quizzes WHERE CourseID=@id", Parameter("@id", courseId));
                if (!HasQuiz()) return;
                int quizId = Convert.ToInt32(Execute(connection, transaction,
                    "INSERT INTO Quizzes (CourseID,Title,Description,PassingScore,CreatedAt) OUTPUT INSERTED.QuizID VALUES (@id,@title,@description,@score,GETDATE())",
                    Parameter("@id", courseId), Parameter("@title", txtQuizTitle.Text.Trim()),
                    Parameter("@description", txtQuizDescription.Text.Trim()), Parameter("@score", passingScore)));
                foreach (DataRow question in Questions.Rows)
                {
                    int questionId = Convert.ToInt32(Execute(connection, transaction,
                        "INSERT INTO QuizQuestions (QuizID,QuestionText,QuestionOrder) OUTPUT INSERTED.QuestionID VALUES (@quiz,@text,@order)",
                        Parameter("@quiz", quizId), Parameter("@text", question["QuestionText"]), Parameter("@order", question["QuestionOrder"])));
                    foreach (var option in QuestionOptions((int)question["QuestionKey"]))
                        Execute(connection, transaction, "INSERT INTO QuizOptions (QuestionID,OptionText,IsCorrect) VALUES (@question,@text,@correct)",
                            Parameter("@question", questionId), Parameter("@text", option["OptionText"]), Parameter("@correct", option["IsCorrect"]));
                }
            });
            BindQuestions();
        }

        private void AddOption(int questionKey)
        {
            int optionKey = Options.Rows.Count == 0 ? 1 : Options.AsEnumerable().Max(row => (int)row["OptionKey"]) + 1;
            Options.Rows.Add(optionKey, questionKey, "", false);
        }

        protected void btnAddQuestion_Click(object sender, EventArgs e)
        {
            Builder.RunAction(() => {
                CaptureQuiz();
                int questionKey = Questions.Rows.Count == 0 ? 1 : Questions.AsEnumerable().Max(row => (int)row["QuestionKey"]) + 1;
                Questions.Rows.Add(questionKey, Questions.Rows.Count + 1, "");
                AddOption(questionKey);
                AddOption(questionKey);
                CurrentPage = TotalPages - 1;
                BindQuestions();
                Builder.MarkUnsaved();
            });
        }

        protected void rptQuestions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            Builder.RunAction(() => {
                CaptureQuiz();
                var question = Questions.Rows[CurrentPage * PageSize + e.Item.ItemIndex];
                int questionKey = (int)question["QuestionKey"];
                if (e.CommandName == "AddOption") AddOption(questionKey);
                if (e.CommandName == "DeleteQuestion")
                {
                    foreach (var option in QuestionOptions(questionKey)) Options.Rows.Remove(option);
                    Questions.Rows.Remove(question);
                }
                BindQuestions();
                Builder.MarkUnsaved();
            });
        }

        protected void rptQuestionOptions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            Builder.RunAction(() => {
                CaptureQuiz();
                int optionKey = Convert.ToInt32(e.CommandArgument);
                var option = Options.Select("OptionKey=" + optionKey).FirstOrDefault();
                if (option != null) Options.Rows.Remove(option);
                BindQuestions();
                Builder.MarkUnsaved();
            });
        }

        private void ChangePage(int direction)
        {
            Builder.RunAction(() => {
                CaptureQuiz();
                CurrentPage = Math.Max(0, Math.Min(TotalPages - 1, CurrentPage + direction));
                BindQuestions();
            });
        }
        protected void btnPreviousQuestions_Click(object sender, EventArgs e) { ChangePage(-1); }
        protected void btnNextQuestions_Click(object sender, EventArgs e) { ChangePage(1); }

        protected override void OnPreRender(EventArgs e)
        {
            // Rebuild native radio markup from captured answers after every action.
            if (Visible) BindQuestions();
            base.OnPreRender(e);
        }
    }
}
