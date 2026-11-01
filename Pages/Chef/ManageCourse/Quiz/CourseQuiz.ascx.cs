using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.ManageCourse.CourseManagementData;

namespace CookingLah.Pages.Chef.ManageCourse.Quiz
{
    public partial class CourseQuiz : CourseManagementControl
    {
        public int SelectionId { get; private set; }
        protected int QuestionCount { get; private set; }
        protected int OptionCount { get; private set; }
        protected int QuizId { get { return quizId; } }
        protected string AnswerGroup { get { return UniqueID + "_correct"; } }
        private CourseReviewDraft draft;
        private int quizId;
        private string structure;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.InitComplete += delegate {
                Run(() => {
                    draft = (CanEdit ? GetReviewDraft(CourseId, false) : null) ?? CourseReviewDraft.Load(CourseId);
                    int selected;
                    int.TryParse(Request.Form[hfQuestion.UniqueID] ?? Request.QueryString["questionId"], out selected);
                    SelectionId = selected;
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
                if (!ownAction && !courseAction) Run(() => CaptureQuiz(false));
            };
        }

        protected string QuestionTitle(object value)
        {
            string text = Convert.ToString(value);
            return string.IsNullOrWhiteSpace(text) ? "New question" : text;
        }

        private void BindBuilder()
        {
            var quiz = draft.Quizzes.Rows.Count == 0 ? null : draft.Quizzes.Rows[0];
            quizId = quiz == null ? 0 : (int)quiz["QuizID"];
            var questions = draft.Questions.Select("QuizID=" + quizId, "QuestionOrder ASC");
            var question = questions.FirstOrDefault(row => (int)row["QuestionID"] == SelectionId);
            if (question == null) SelectionId = 0;
            var options = SelectedOptions();
            structure = quizId + "|" + string.Join(",", questions.Select(row => row["QuestionID"].ToString())) + "|" +
                string.Join(",", options.Select(row => row["OptionID"].ToString()));
            hfStructure.Value = structure;
            hfQuestion.Value = SelectionId.ToString();
            QuestionCount = questions.Length;
            rptQuestions.DataSource = new DataView(draft.Questions) { RowFilter = "QuizID=" + quizId, Sort = "QuestionOrder ASC" };
            rptQuestions.DataBind();
            pnlEmpty.Visible = quiz == null;
            btnAddQuiz.Visible = CanEdit && quiz == null;
            btnQuizInfo.Visible = quiz != null;
            btnQuizInfo.CssClass = "qb-info" + (SelectionId == 0 ? " is-selected" : "");
            btnAddQuestion.Visible = CanEdit && quiz != null;
            pnlEmptyQuestions.Visible = quiz != null && questions.Length == 0;
            pnlQuizInfo.Visible = quiz != null && question == null;
            pnlQuestion.Visible = question != null;
            pnlQuizFields.Visible = pnlQuestionFields.Visible = CanEdit && quiz != null;
            pnlQuizView.Visible = pnlQuestionView.Visible = !CanEdit;
            btnDeleteQuiz.Visible = CanEdit;
            if (quiz != null)
            {
                txtQuizTitle.Text = litQuizTitle.Text = Convert.ToString(quiz["Title"]);
                txtQuizDescription.Text = litQuizDescription.Text = Convert.ToString(quiz["Description"]);
                txtPassingScore.Text = litPassingScore.Text = Convert.ToString(quiz["PassingScore"]);
            }
            if (question == null) return;
            litQuestionNumber.Text = "Question " + question["QuestionOrder"];
            txtQuestionText.Text = litQuestionText.Text = Convert.ToString(question["QuestionText"]);
            var view = draft.Options.Clone();
            foreach (DataRow option in options) view.ImportRow(option);
            OptionCount = options.Length;
            rptEditOptions.DataSource = view;
            rptEditOptions.DataBind();
            rptViewOptions.DataSource = view;
            rptViewOptions.DataBind();
        }

        private DataRow[] SelectedOptions()
        {
            // Keep existing answer order; append new temporary options at the end.
            return draft.Options.Select("QuestionID=" + SelectionId)
                .OrderBy(row => (int)row["OptionID"] < 0 ? 1 : 0)
                .ThenBy(row => Math.Abs((int)row["OptionID"]))
                .ToArray();
        }

        private void CheckStructure()
        {
            if (IsPostBack && Request.Form[hfStructure.UniqueID] != structure)
                throw new InvalidOperationException("The quiz changed in another tab. Reload before editing.");
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

        private void FieldError(Label label, string message)
        {
            label.Text = message;
            pnlValidation.Visible = true;
        }

        public bool CaptureQuiz(bool validate)
        {
            if (!CanEdit || quizId == 0) return true;
            EnsureEditable();
            CheckStructure();
            draft = GetReviewDraft(CourseId, true);
            var quiz = CourseReviewDraft.FindRow(draft.Quizzes, "QuizID", quizId);
            if (quiz == null) throw new InvalidOperationException("Quiz not found in this course.");
            pnlValidation.Visible = false;
            errQuizTitle.Text = errQuizDescription.Text = errScore.Text = errQuestion.Text = errOptions.Text = "";
            if (SelectionId == 0)
            {
                string title = txtQuizTitle.Text.Trim(), description = txtQuizDescription.Text.Trim();
                int score;
                if ((validate && title.Length == 0) || title.Length > 150) FieldError(errQuizTitle, "Enter a quiz title within 150 characters.");
                if (description.Length > 500) FieldError(errQuizDescription, "Keep the description within 500 characters.");
                if (!int.TryParse(txtPassingScore.Text, out score) || score < 0 || score > 100) FieldError(errScore, "Enter a passing score between 0 and 100.");
                if (pnlValidation.Visible) return false;
                quiz["Title"] = title;
                quiz["Description"] = description;
                quiz["PassingScore"] = score;
            }
            else
            {
                var question = CourseReviewDraft.FindRow(draft.Questions, "QuestionID", SelectionId);
                if (question == null || (int)question["QuizID"] != quizId) throw new InvalidOperationException("Question not found in this quiz.");
                string text = txtQuestionText.Text.Trim();
                var options = SelectedOptions();
                string correct = Request.Form[AnswerGroup];
                if (validate && text.Length == 0) FieldError(errQuestion, "Question text is required.");
                if (validate && (options.Length < 2 || options.Count(row => row["OptionID"].ToString() == correct) != 1))
                    FieldError(errOptions, "Provide at least two options and select one correct answer.");
                var texts = new string[options.Length];
                foreach (RepeaterItem item in rptEditOptions.Items)
                {
                    texts[item.ItemIndex] = ((TextBox)item.FindControl("txtOptionText")).Text.Trim();
                    var error = (Label)item.FindControl("errOption");
                    error.Text = "";
                    if ((validate && texts[item.ItemIndex].Length == 0) || texts[item.ItemIndex].Length > 500)
                        FieldError(error, "Enter an answer within 500 characters.");
                }
                if (pnlValidation.Visible) return false;
                question["QuestionText"] = text;
                for (int i = 0; i < options.Length; i++)
                {
                    options[i]["OptionText"] = texts[i];
                    options[i]["IsCorrect"] = options[i]["OptionID"].ToString() == correct;
                }
            }
            return true;
        }

        protected void btnAddQuiz_Click(object sender, EventArgs e)
        {
            Edit(() => {
                if (draft.Quizzes.Rows.Count != 0) throw new InvalidOperationException("A quiz already exists.");
                var row = draft.Quizzes.NewRow();
                row["QuizID"] = CourseReviewDraft.NewId(draft.Quizzes, "QuizID");
                row["CourseID"] = CourseId;
                row["Title"] = "";
                row["Description"] = "";
                row["PassingScore"] = 60;
                draft.Quizzes.Rows.Add(row);
                SelectionId = 0;
                BindBuilder();
            });
        }

        protected void btnQuizInfo_Click(object sender, EventArgs e)
        {
            Run(() => {
                CheckStructure();
                if (CanEdit && !CaptureQuiz(false)) return;
                SelectionId = 0;
                BindBuilder();
            });
        }

        protected void btnAddQuestion_Click(object sender, EventArgs e)
        {
            Edit(() => {
                if (quizId == 0 || !CaptureQuiz(false)) return;
                var row = draft.Questions.NewRow();
                SelectionId = CourseReviewDraft.NewId(draft.Questions, "QuestionID");
                row["QuestionID"] = SelectionId;
                row["QuizID"] = quizId;
                row["QuestionText"] = "";
                row["QuestionOrder"] = draft.Questions.Select("QuizID=" + quizId).Length + 1;
                draft.Questions.Rows.Add(row);
                AddOption();
                AddOption();
                BindBuilder();
                litState.Text = "New question - unsaved changes";
            });
        }

        private void AddOption()
        {
            var row = draft.Options.NewRow();
            row["OptionID"] = CourseReviewDraft.NewId(draft.Options, "OptionID");
            row["QuestionID"] = SelectionId;
            row["OptionText"] = "";
            row["IsCorrect"] = false;
            draft.Options.Rows.Add(row);
        }

        protected void rptQuestions_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            Run(() => {
                CheckStructure();
                if (CanEdit && !CaptureQuiz(false)) return;
                int id = Convert.ToInt32(e.CommandArgument);
                var question = CourseReviewDraft.FindRow(draft.Questions, "QuestionID", id);
                if (question == null || (int)question["QuizID"] != quizId) throw new InvalidOperationException("Question not found in this quiz.");
                if (e.CommandName == "SelectQuestion") SelectionId = id;
                else if (e.CommandName == "DeleteQuestion")
                {
                    EnsureEditable();
                    draft = GetReviewDraft(CourseId, true);
                    RemoveQuestion(id);
                    var remaining = draft.Questions.Select("QuizID=" + quizId, "QuestionOrder ASC");
                    for (int i = 0; i < remaining.Length; i++) remaining[i]["QuestionOrder"] = i + 1;
                    if (SelectionId == id) SelectionId = 0;
                    litState.Text = "Question removed from working copy.";
                }
                BindBuilder();
            });
        }

        private void RemoveQuestion(int id)
        {
            foreach (DataRow option in draft.Options.Select("QuestionID=" + id)) draft.Options.Rows.Remove(option);
            draft.Questions.Rows.Remove(CourseReviewDraft.FindRow(draft.Questions, "QuestionID", id));
        }

        protected void btnAddOption_Click(object sender, EventArgs e)
        {
            Edit(() => { if (SelectionId == 0 || !CaptureQuiz(false)) return; AddOption(); BindBuilder(); });
        }

        protected void rptEditOptions_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            Edit(() => {
                if (!CaptureQuiz(false)) return;
                var option = CourseReviewDraft.FindRow(draft.Options, "OptionID", Convert.ToInt32(e.CommandArgument));
                if (option == null || (int)option["QuestionID"] != SelectionId) throw new InvalidOperationException("Option not found in this question.");
                if (draft.Options.Select("QuestionID=" + SelectionId).Length <= 2) throw new InvalidOperationException("Keep at least two answer options.");
                draft.Options.Rows.Remove(option);
                BindBuilder();
            });
        }

        protected void btnDeleteQuiz_Click(object sender, EventArgs e)
        {
            Edit(() => {
                var quiz = CourseReviewDraft.FindRow(draft.Quizzes, "QuizID", quizId);
                if (quiz == null) throw new InvalidOperationException("Quiz not found in this course.");
                foreach (DataRow question in draft.Questions.Select("QuizID=" + quizId)) RemoveQuestion((int)question["QuestionID"]);
                draft.Quizzes.Rows.Remove(quiz);
                SelectionId = 0;
                BindBuilder();
                litState.Text = "Quiz removed from working copy. Save Changes confirms deletion.";
            });
        }

        protected void btnSaveOrder_Click(object sender, EventArgs e)
        {
            Edit(() => {
                if (!CaptureQuiz(false)) return;
                draft.Reorder("questions", quizId, hfOrderIds.Value);
                BindBuilder();
                litState.Text = "Question order saved to working copy.";
            });
        }
    }
}
