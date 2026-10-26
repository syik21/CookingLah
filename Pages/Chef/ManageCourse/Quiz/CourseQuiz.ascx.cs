using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;
using static CookingLah.Pages.Chef.ManageCourse.CourseManagementData;

namespace CookingLah.Pages.Chef.ManageCourse.Quiz
{
    public partial class CourseQuiz : CourseManagementControl
    {
        private int QuizId { get { return (int?)ViewState["QuizId"] ?? 0; } set { ViewState["QuizId"] = value; } }
        private int QuestionId { get { return (int?)ViewState["QuestionId"] ?? 0; } set { ViewState["QuestionId"] = value; } }
        private int CurrentPage { get { return (int?)ViewState["CurrentPage"] ?? 0; } set { ViewState["CurrentPage"] = value; } }
        private bool DeleteWholeQuiz { get { return (bool?)ViewState["DeleteWholeQuiz"] ?? false; } set { ViewState["DeleteWholeQuiz"] = value; } }
        private DataTable Options { get { return (DataTable)ViewState["Options"]; } set { ViewState["Options"] = value; } }
        protected string AnswerGroup { get { return UniqueID + "_correct"; } }

        private DataTable GetQuiz()
        {
            var quizzes = ReadDraftSection(CourseId, "Quizzes", "SELECT * FROM Quizzes WHERE CourseID=@course ORDER BY QuizID", Parameter("@course", CourseId));
            if (!HasChangesAwaitingReview(CourseId))
                return Read("SELECT TOP 1 q.*,(SELECT COUNT(*) FROM QuizQuestions WHERE QuizID=q.QuizID) AS QuestionCount FROM Quizzes q WHERE CourseID=@course ORDER BY QuizID", Parameter("@course", CourseId));
            var result = quizzes.Clone();
            if (!result.Columns.Contains("QuestionCount")) result.Columns.Add("QuestionCount", typeof(int));
            if (quizzes.Rows.Count > 0)
            {
                var quiz = quizzes.Rows[0];
                var row = result.NewRow();
                foreach (DataColumn column in quizzes.Columns) row[column.ColumnName] = quiz[column.ColumnName];
                row["QuestionCount"] = GetDraftQuestions(Convert.ToInt32(quiz["QuizID"])).Rows.Count;
                result.Rows.Add(row);
            }
            return result;
        }
        private DataTable GetDraftQuestions(int quizId)
        {
            if (!HasChangesAwaitingReview(CourseId))
                return Read("SELECT qq.* FROM QuizQuestions qq WHERE qq.QuizID=@quiz ORDER BY qq.QuestionOrder,qq.QuestionID", Parameter("@quiz", quizId));
            var all = ReadDraftSection(CourseId, "QuizQuestions", "", new System.Data.SqlClient.SqlParameter[0]);
            var result = all.Clone();
            foreach (DataRow row in all.Rows)
                if (Convert.ToInt32(row["QuizID"]) == quizId) result.ImportRow(row);
            return result;
        }
        public void LoadQuiz(int courseId)
        {
            var quiz = GetQuiz();
            rptQuiz.DataSource = quiz;
            rptQuiz.DataBind();
            pnlEmpty.Visible = quiz.Rows.Count == 0;
            pnlQuestions.Visible = quiz.Rows.Count > 0;
            btnEditQuiz.Visible = btnDeleteQuiz.Visible = CanEdit && quiz.Rows.Count > 0;
            btnAddQuiz.Visible = btnAddQuestion.Visible = CanEdit;
            if (quiz.Rows.Count > 0) BindQuestions(Convert.ToInt32(quiz.Rows[0]["QuizID"]));
        }
        private void BindQuestions(int quizId)
        {
            var rows = GetDraftQuestions(quizId);
            int count = rows.Rows.Count;
            int pages = Math.Max(1, (count + 4) / 5);
            CurrentPage = Math.Min(CurrentPage, pages - 1);
            rptQuestions.DataSource = new PagedDataSource { DataSource = rows.DefaultView, AllowPaging = true, PageSize = 5, CurrentPageIndex = CurrentPage };
            rptQuestions.DataBind();
            pnlEmptyQuestions.Visible = count == 0;
            pnlPager.Visible = count > 5;
            btnPrevious.Enabled = CurrentPage > 0;
            btnNext.Enabled = CurrentPage < pages - 1;
            lblPage.Text = string.Format("Showing Questions {0}-{1} of {2}. Page {3} of {4}", CurrentPage * 5 + 1, Math.Min(count, (CurrentPage + 1) * 5), count, CurrentPage + 1, pages);
        }
        protected void rptQuestions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            int questionId = Convert.ToInt32(((DataRowView)e.Item.DataItem)["QuestionID"]);
            var repeater = (Repeater)e.Item.FindControl("rptQuestionOptions");
            DataTable options;
            if (HasChangesAwaitingReview(CourseId))
            {
                var all = ReadDraftSection(CourseId, "QuizOptions", "", new System.Data.SqlClient.SqlParameter[0]);
                options = all.Clone();
                foreach (DataRow row in all.Rows) if (Convert.ToInt32(row["QuestionID"]) == questionId) options.ImportRow(row);
            }
            else options = Read("SELECT * FROM QuizOptions WHERE QuestionID=@question ORDER BY OptionID", Parameter("@question", questionId));
            repeater.DataSource = options;
            repeater.DataBind();
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Visible || IsPostBack) return;
            int page;
            if (int.TryParse(Request.QueryString["page"], out page)) CurrentPage = Math.Max(0, page);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (Visible) Run(() =>
            {
                LoadQuiz(CourseId);
                if (pnlQuestionEditor.Visible) BindOptions();
            });
            base.OnPreRender(e);
        }
        private void BindOptions() { rptEditOptions.DataSource = Options; rptEditOptions.DataBind(); }
        protected void btnCancel_Click(object sender, EventArgs e) { pnlQuizEditor.Visible = pnlQuestionEditor.Visible = pnlDelete.Visible = false; }
        protected void btnEditQuiz_Click(object sender, EventArgs e)
        {
            Run(() =>
            {
                EnsureEditable();
                var rows = GetQuiz();
                DataRow quiz = rows.Rows.Count == 0 ? null : rows.Rows[0];
                QuizId = quiz == null ? 0 : Convert.ToInt32(quiz["QuizID"]);
                txtQuizTitle.Text = quiz == null ? "" : Convert.ToString(quiz["Title"]);
                txtQuizDescription.Text = quiz == null ? "" : Convert.ToString(quiz["Description"]);
                txtPassingScore.Text = quiz == null ? "60" : Convert.ToString(quiz["PassingScore"]);
                litQuizEditorTitle.Text = QuizId == 0 ? "Add Quiz" : "Edit Quiz";
                pnlQuizEditor.Visible = true;
            });
        }
        protected void btnSaveQuiz_Click(object sender, EventArgs e)
        {
            if (!pnlQuizEditor.Visible) return;
            Run(() =>
            {
                EnsureEditable();
                string title = Required(txtQuizTitle.Text, "a quiz title", 150);
                string description = Optional(txtQuizDescription.Text, "the quiz description", 500);
                int score;
                if (!int.TryParse(txtPassingScore.Text, out score) || score < 0 || score > 100) throw new InvalidOperationException("Enter a passing score between 0 and 100.");
                if (CanEdit)
                {
                    var draft = GetReviewDraft(CourseId, true);
                    DataRow quiz = QuizId == 0 ? null : CourseReviewDraft.FindRow(draft.Quizzes, "QuizID", QuizId);
                    if (QuizId != 0 && quiz == null) throw new InvalidOperationException("This quiz no longer exists. Reload the page to edit it.");
                    if (quiz == null)
                    {
                        if (draft.Quizzes.Rows.Count > 0)
                            throw new InvalidOperationException("A quiz already exists. Reload the page to edit it.");
                        quiz = draft.Quizzes.NewRow(); quiz["QuizID"] = CourseReviewDraft.NewId(draft.Quizzes, "QuizID"); quiz["CourseID"] = CourseId; draft.Quizzes.Rows.Add(quiz);
                    }
                    quiz["Title"] = title; quiz["Description"] = description; quiz["PassingScore"] = score;
                    Saved("Quiz changes saved.", CurrentPage);
                    return;
                }
                EditCourse(CourseId, (connection, transaction) =>
                {
                    if (QuizId == 0 && Execute(connection, transaction, "SELECT TOP 1 QuizID FROM Quizzes WHERE CourseID=@course", Parameter("@course", CourseId)) != null)
                        throw new InvalidOperationException("A quiz already exists. Reload the page to edit it.");
                    if (QuizId > 0) EnsureQuiz(connection, transaction);
                    string sql = QuizId == 0
                        ? "INSERT INTO Quizzes (CourseID,Title,Description,PassingScore) VALUES (@course,@title,@description,@score)"
                        : "UPDATE Quizzes SET Title=@title,Description=@description,PassingScore=@score WHERE QuizID=@quiz AND CourseID=@course";
                    Execute(connection, transaction, sql, Parameter("@course", CourseId), Parameter("@quiz", QuizId), Parameter("@title", title), Parameter("@description", description), Parameter("@score", score));
                });
                Saved("Quiz saved successfully.", CurrentPage);
            });
        }
        private void EnsureQuiz(System.Data.SqlClient.SqlConnection connection, System.Data.SqlClient.SqlTransaction transaction)
        {
            if (Execute(connection, transaction, "SELECT QuizID FROM Quizzes WHERE QuizID=@quiz AND CourseID=@course", Parameter("@quiz", QuizId), Parameter("@course", CourseId)) == null)
                throw new InvalidOperationException("This quiz no longer exists. Reload the page.");
        }
        protected void btnAddQuestion_Click(object sender, EventArgs e)
        {
            Run(() =>
            {
                EnsureEditable();
                var quiz = GetQuiz();
                if (quiz.Rows.Count == 0) throw new InvalidOperationException("Add a quiz before adding questions.");
                QuizId = Convert.ToInt32(quiz.Rows[0]["QuizID"]);
                QuestionId = 0;
                txtQuestionText.Text = "";
                Options = new DataTable();
                Options.Columns.Add("OptionID", typeof(int));
                Options.Columns.Add("OptionText", typeof(string));
                Options.Columns.Add("IsCorrect", typeof(bool));
                AddBlankOption();
                AddBlankOption();
                litQuestionEditorTitle.Text = "Add Question";
                pnlQuestionEditor.Visible = true;
                BindOptions();
            });
        }
        private void AddBlankOption()
        {
            int key = Options.Rows.Count == 0 ? -1 : Math.Min(-1, Options.AsEnumerable().Min(row => (int)row["OptionID"]) - 1);
            Options.Rows.Add(key, "", false);
            Options = Options;
        }
        protected void rptQuestions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            Run(() =>
            {
                EnsureEditable();
                int questionId = Convert.ToInt32(e.CommandArgument);
                DataTable rows;
                if (HasChangesAwaitingReview(CourseId))
                {
                    rows = GetDraftQuestions(QuizId == 0 ? Convert.ToInt32(GetQuiz().Rows[0]["QuizID"]) : QuizId);
                    var selected = CourseReviewDraft.FindRow(rows, "QuestionID", questionId);
                    var one = rows.Clone(); if (selected != null) one.ImportRow(selected); rows = one;
                }
                else rows = Read("SELECT qq.* FROM QuizQuestions qq JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE qq.QuestionID=@question AND q.CourseID=@course", Parameter("@question", questionId), Parameter("@course", CourseId));
                if (rows.Rows.Count == 0) throw new InvalidOperationException("This question no longer exists.");
                QuestionId = questionId;
                QuizId = Convert.ToInt32(rows.Rows[0]["QuizID"]);
                if (e.CommandName == "EditQuestion")
                {
                    txtQuestionText.Text = Convert.ToString(rows.Rows[0]["QuestionText"]);
                    if (HasChangesAwaitingReview(CourseId))
                    {
                        var allOptions = ReadDraftSection(CourseId, "QuizOptions", "", new System.Data.SqlClient.SqlParameter[0]);
                        Options = allOptions.Clone();
                        foreach (DataRow option in allOptions.Rows) if (Convert.ToInt32(option["QuestionID"]) == questionId) Options.ImportRow(option);
                    }
                    else Options = Read("SELECT OptionID,OptionText,IsCorrect FROM QuizOptions WHERE QuestionID=@question ORDER BY OptionID", Parameter("@question", questionId));
                    litQuestionEditorTitle.Text = "Edit Question";
                    pnlQuestionEditor.Visible = true;
                    BindOptions();
                }
                else if (e.CommandName == "DeleteQuestion")
                {
                    DeleteWholeQuiz = false;
                    litDeleteTitle.Text = "Delete Question?";
                    litDeleteDescription.Text = "Delete this question and all its answer options? " + Convert.ToString(rows.Rows[0]["QuestionText"]);
                    pnlDelete.Visible = true;
                }
            });
        }
        private void CaptureOptions()
        {
            string correct = Request.Form[AnswerGroup];
            foreach (RepeaterItem item in rptEditOptions.Items)
            {
                DataRow option = Options.Rows[item.ItemIndex];
                option["OptionText"] = ((TextBox)item.FindControl("txtOptionText")).Text.Trim();
                option["IsCorrect"] = correct == Convert.ToString(option["OptionID"]);
            }
            Options = Options;
        }
        protected void btnAddOption_Click(object sender, EventArgs e)
        {
            if (!pnlQuestionEditor.Visible) return;
            Run(() => { EnsureEditable(); CaptureOptions(); AddBlankOption(); BindOptions(); });
        }
        protected void rptEditOptions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!pnlQuestionEditor.Visible) return;
            Run(() =>
            {
                EnsureEditable();
                CaptureOptions();
                int key = Convert.ToInt32(e.CommandArgument);
                var row = Options.AsEnumerable().FirstOrDefault(option => (int)option["OptionID"] == key);
                if (row != null) Options.Rows.Remove(row);
                Options = Options;
                BindOptions();
            });
        }
        protected void btnSaveQuestion_Click(object sender, EventArgs e)
        {
            if (!pnlQuestionEditor.Visible) return;
            Run(() =>
            {
                EnsureEditable();
                CaptureOptions();
                string question = Required(txtQuestionText.Text, "the question text");
                if (Options.Rows.Count < 2) throw new InvalidOperationException("A question must contain at least two answer options.");
                if (Options.AsEnumerable().Count(row => (bool)row["IsCorrect"]) != 1) throw new InvalidOperationException("Please select one correct answer.");
                foreach (DataRow option in Options.Rows) Required(Convert.ToString(option["OptionText"]), "each answer option", 500);
                if (CanEdit)
                {
                    var draft = GetReviewDraft(CourseId, true);
                    var quiz = CourseReviewDraft.FindRow(draft.Quizzes, "QuizID", QuizId);
                    if (quiz == null) throw new InvalidOperationException("This quiz no longer exists.");
                    DataRow questionRow = QuestionId == 0 ? null : CourseReviewDraft.FindRow(draft.Questions, "QuestionID", QuestionId);
                    if (QuestionId != 0 && questionRow == null) throw new InvalidOperationException("This question no longer exists. Reload the page.");
                    if (questionRow == null)
                    {
                        questionRow = draft.Questions.NewRow();
                        questionRow["QuestionID"] = CourseReviewDraft.NewId(draft.Questions, "QuestionID");
                        questionRow["QuizID"] = QuizId;
                        questionRow["QuestionOrder"] = draft.Questions.Select("QuizID=" + QuizId).Length + 1;
                        draft.Questions.Rows.Add(questionRow);
                    }
                    questionRow["QuestionText"] = question;
                    var previousOptions = new System.Collections.Generic.List<DataRow>();
                    foreach (DataRow option in draft.Options.Rows)
                        if (Convert.ToInt32(option["QuestionID"]) == Convert.ToInt32(questionRow["QuestionID"])) previousOptions.Add(option);
                    foreach (DataRow previous in previousOptions) draft.Options.Rows.Remove(previous);
                    foreach (DataRow option in Options.Rows)
                    {
                        var stagedOption = draft.Options.NewRow();
                        stagedOption["OptionID"] = CourseReviewDraft.NewId(draft.Options, "OptionID");
                        stagedOption["QuestionID"] = questionRow["QuestionID"];
                        stagedOption["OptionText"] = option["OptionText"];
                        stagedOption["IsCorrect"] = option["IsCorrect"];
                        draft.Options.Rows.Add(stagedOption);
                    }
                    Saved("Question changes saved.", CurrentPage);
                    return;
                }
                EditCourse(CourseId, (connection, transaction) =>
                {
                    EnsureQuiz(connection, transaction);
                    int questionId = QuestionId;
                    if (questionId == 0)
                    {
                        questionId = Convert.ToInt32(Execute(connection, transaction,
                            "INSERT INTO QuizQuestions (QuizID,QuestionText,QuestionOrder) OUTPUT INSERTED.QuestionID SELECT @quiz,@text,ISNULL(MAX(QuestionOrder),0)+1 FROM QuizQuestions WHERE QuizID=@quiz",
                            Parameter("@quiz", QuizId), Parameter("@text", question)));
                    }
                    else
                    {
                        if (Execute(connection, transaction, "SELECT QuestionID FROM QuizQuestions WHERE QuestionID=@id AND QuizID=@quiz", Parameter("@id", questionId), Parameter("@quiz", QuizId)) == null)
                            throw new InvalidOperationException("This question no longer exists.");
                        Execute(connection, transaction, "UPDATE QuizQuestions SET QuestionText=@text WHERE QuestionID=@id AND QuizID=@quiz", Parameter("@text", question), Parameter("@id", questionId), Parameter("@quiz", QuizId));
                    }
                    // Keep existing question and option IDs; editing must not remove quiz attempts.
                    var existingIds = new System.Collections.Generic.List<int>();
                    using (var command = new System.Data.SqlClient.SqlCommand("SELECT OptionID FROM QuizOptions WHERE QuestionID=@id", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@id", questionId);
                        using (var reader = command.ExecuteReader()) while (reader.Read()) existingIds.Add(reader.GetInt32(0));
                    }
                    foreach (int optionId in existingIds)
                        if (!Options.AsEnumerable().Any(row => (int)row["OptionID"] == optionId))
                            Execute(connection, transaction, "DELETE FROM QuizOptions WHERE OptionID=@id AND QuestionID=@question", Parameter("@id", optionId), Parameter("@question", questionId));
                    foreach (DataRow option in Options.Rows)
                    {
                        int optionId = (int)option["OptionID"];
                        if (optionId > 0 && !existingIds.Contains(optionId)) throw new InvalidOperationException("These answer options have changed. Reload the page.");
                        string sql = optionId > 0
                            ? "UPDATE QuizOptions SET OptionText=@text,IsCorrect=@correct WHERE OptionID=@id AND QuestionID=@question"
                            : "INSERT INTO QuizOptions (QuestionID,OptionText,IsCorrect) VALUES (@question,@text,@correct)";
                        Execute(connection, transaction, sql, Parameter("@question", questionId), Parameter("@id", optionId), Parameter("@text", option["OptionText"]), Parameter("@correct", option["IsCorrect"]));
                    }
                });
                Saved("Question saved successfully.", CurrentPage);
            });
        }
        protected void btnDeleteQuiz_Click(object sender, EventArgs e)
        {
            Run(() =>
            {
                EnsureEditable();
                var rows = GetQuiz();
                if (rows.Rows.Count == 0) throw new InvalidOperationException("This quiz no longer exists.");
                QuizId = Convert.ToInt32(rows.Rows[0]["QuizID"]);
                DeleteWholeQuiz = true;
                litDeleteTitle.Text = "Delete Quiz?";
                litDeleteDescription.Text = "This permanently deletes the quiz information, all questions, answer options and learner quiz attempts for " + Convert.ToString(rows.Rows[0]["Title"]) + ".";
                pnlDelete.Visible = true;
            });
        }
        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            if (!pnlDelete.Visible) return;
            Run(() =>
            {
                if (CanEdit)
                {
                    var draft = GetReviewDraft(CourseId, true);
                    var quiz = CourseReviewDraft.FindRow(draft.Quizzes, "QuizID", QuizId);
                    if (quiz == null) throw new InvalidOperationException("This quiz no longer exists.");
                    if (DeleteWholeQuiz)
                    {
                        var questionIds = new System.Collections.Generic.List<int>();
                        foreach (DataRow question in draft.Questions.Rows)
                            if (Convert.ToInt32(question["QuizID"]) == QuizId) questionIds.Add(Convert.ToInt32(question["QuestionID"]));
                        foreach (int questionId in questionIds) RemoveDraftQuestion(draft, questionId);
                        draft.Quizzes.Rows.Remove(quiz);
                    }
                    else
                    {
                        RemoveDraftQuestion(draft, QuestionId);
                        var remaining = draft.Questions.Select("QuizID=" + QuizId, "QuestionOrder ASC,QuestionID ASC");
                        for (int i = 0; i < remaining.Length; i++) remaining[i]["QuestionOrder"] = i + 1;
                    }
                    Saved(DeleteWholeQuiz ? "Quiz changes saved." : "Question changes saved.", CurrentPage);
                    return;
                }
                EditCourse(CourseId, (connection, transaction) =>
                {
                    EnsureQuiz(connection, transaction);
                    if (DeleteWholeQuiz)
                        Execute(connection, transaction, "DELETE FROM Quizzes WHERE QuizID=@quiz AND CourseID=@course", Parameter("@quiz", QuizId), Parameter("@course", CourseId));
                    else
                    {
                        Execute(connection, transaction, "DELETE FROM QuizQuestions WHERE QuestionID=@question AND QuizID=@quiz", Parameter("@question", QuestionId), Parameter("@quiz", QuizId));
                        NormalizeQuestions(connection, transaction, QuizId);
                    }
                });
                Saved(DeleteWholeQuiz ? "Quiz deleted successfully." : "Question deleted successfully.", CurrentPage);
            });
        }
        private static void RemoveDraftQuestion(CourseReviewDraft draft, int questionId)
        {
            var options = new System.Collections.Generic.List<DataRow>();
            foreach (DataRow option in draft.Options.Rows)
                if (Convert.ToInt32(option["QuestionID"]) == questionId) options.Add(option);
            foreach (DataRow option in options) draft.Options.Rows.Remove(option);
            var question = CourseReviewDraft.FindRow(draft.Questions, "QuestionID", questionId);
            if (question != null) draft.Questions.Rows.Remove(question);
        }
        protected void btnPrevious_Click(object sender, EventArgs e) { CurrentPage = Math.Max(0, CurrentPage - 1); }
        protected void btnNext_Click(object sender, EventArgs e) { CurrentPage++; }
    }
}
