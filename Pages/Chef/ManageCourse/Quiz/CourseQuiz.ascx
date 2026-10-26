<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CourseQuiz.ascx.cs" Inherits="CookingLah.Pages.Chef.ManageCourse.Quiz.CourseQuiz" %>
<section class="course-quiz">
    <div class="mc-section-heading"><h2>Quiz</h2><asp:Button ID="btnEditQuiz" runat="server" Text="Edit Quiz" CssClass="btn" OnClick="btnEditQuiz_Click" /></div>
    <asp:Panel ID="pnlEmpty" runat="server" CssClass="card mc-empty"><p>No quiz exists for this course.</p>
    <asp:Button ID="btnAddQuiz" runat="server" Text="+ Add Quiz" CssClass="btn btn-primary" OnClick="btnEditQuiz_Click" /></asp:Panel>
    <asp:Repeater ID="rptQuiz" runat="server"><ItemTemplate><article class="card quiz-summary"><h3><%#: Eval("Title") %></h3><p class="mc-copy"><%#: Eval("Description") %></p><p>Passing score: <%#: Eval("PassingScore") %>% &middot; <%#: Eval("QuestionCount") %> questions</p></article>
    </ItemTemplate></asp:Repeater>
    <asp:Button ID="btnDeleteQuiz" runat="server" Text="Delete Quiz" CssClass="mc-link mc-danger-link" OnClick="btnDeleteQuiz_Click" />
    <asp:Panel ID="pnlQuestions" runat="server">
        <div class="mc-section-heading"><h2>Questions</h2><asp:Button ID="btnAddQuestion" runat="server" Text="+ Add Question" CssClass="btn btn-primary" OnClick="btnAddQuestion_Click" /></div>
        <asp:Panel ID="pnlEmptyQuestions" runat="server" CssClass="card mc-empty"><p>No questions added yet.</p></asp:Panel>
        <asp:Repeater ID="rptQuestions" runat="server" OnItemCommand="rptQuestions_ItemCommand" OnItemDataBound="rptQuestions_ItemDataBound"><ItemTemplate>
            <article class="card question-card"><span>Question <%#: Eval("QuestionOrder") %></span><h3><%#: Eval("QuestionText") %></h3>
                <ul class="quiz-options">
                <asp:Repeater ID="rptQuestionOptions" runat="server"><ItemTemplate><li><%#: Eval("OptionText") %><span class="quiz-correct"><%#: Convert.ToBoolean(Eval("IsCorrect")) ? "Correct answer" : "" %></span></li>
                </ItemTemplate></asp:Repeater></ul>
                <div class="mc-actions">
                <asp:LinkButton runat="server" Text="Edit Question" CssClass="btn" CommandName="EditQuestion" CommandArgument='<%# Eval("QuestionID") %>' Visible='<%# CanEdit %>' />
                <asp:LinkButton runat="server" Text="Delete Question" CssClass="mc-link mc-danger-link" CommandName="DeleteQuestion" CommandArgument='<%# Eval("QuestionID") %>' Visible='<%# CanEdit %>' /></div>
            </article>
        </ItemTemplate></asp:Repeater>
        <asp:Panel ID="pnlPager" runat="server" CssClass="mc-pager">
        <asp:Button ID="btnPrevious" runat="server" Text="Previous" CssClass="btn" OnClick="btnPrevious_Click" />
        <asp:Label ID="lblPage" runat="server" />
        <asp:Button ID="btnNext" runat="server" Text="Next" CssClass="btn" OnClick="btnNext_Click" /></asp:Panel>
    </asp:Panel>
    <asp:Panel ID="pnlQuizEditor" runat="server" Visible="false" CssClass="card mc-modal" data-title="Quiz">
        <h2><asp:Literal ID="litQuizEditorTitle" runat="server" Mode="Encode" /></h2>
        <div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtQuizTitle" Text="Quiz Title *" /><asp:TextBox ID="txtQuizTitle" runat="server" MaxLength="150" /></div>
        <div class="mc-field">
        <asp:Label runat="server" AssociatedControlID="txtQuizDescription" Text="Description" />
        <asp:TextBox ID="txtQuizDescription" runat="server" TextMode="MultiLine" Rows="3" MaxLength="500" /></div>
        <div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtPassingScore" Text="Passing Score (0-100%) *" /><asp:TextBox ID="txtPassingScore" runat="server" MaxLength="3" /></div>
        <div class="mc-actions">
        <asp:Button ID="btnCancelQuiz" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" data-cancel="" />
        <asp:Button ID="btnSaveQuiz" runat="server" Text="Save Quiz" CssClass="btn btn-primary" OnClick="btnSaveQuiz_Click" /></div>
    </asp:Panel>
    <asp:Panel ID="pnlQuestionEditor" runat="server" Visible="false" CssClass="card mc-modal" data-title="Question and Answers">
        <h2><asp:Literal ID="litQuestionEditorTitle" runat="server" Mode="Encode" /></h2>
        <div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtQuestionText" Text="Question *" /><asp:TextBox ID="txtQuestionText" runat="server" TextMode="MultiLine" Rows="3" /></div>
        <fieldset class="question-options-editor"><legend>Answer Options</legend><p>Provide at least two options and select exactly one correct answer. Changes apply when you save the question.</p>
            <asp:Repeater ID="rptEditOptions" runat="server" OnItemCommand="rptEditOptions_ItemCommand"><ItemTemplate>
                <div class="quiz-option-editor">
                    <label class="quiz-answer-choice"><input type="radio" name="<%# AnswerGroup %>" value="<%# Eval("OptionID") %>" <%# Convert.ToBoolean(Eval("IsCorrect")) ? "checked" : "" %> /><span>Correct</span></label>
                    <div class="mc-field">
                    <asp:Label runat="server" AssociatedControlID="txtOptionText" Text='<%# "Option " + (Container.ItemIndex + 1) %>' />
                    <asp:TextBox ID="txtOptionText" runat="server" Text='<%# Eval("OptionText") %>' MaxLength="500" /></div>
                    <asp:LinkButton runat="server" Text="Remove Option" CssClass="mc-link mc-danger-link" CommandName="RemoveOption" CommandArgument='<%# Eval("OptionID") %>' />
                </div>
            </ItemTemplate></asp:Repeater>
            <asp:Button ID="btnAddOption" runat="server" Text="+ Add Option" CssClass="btn" OnClick="btnAddOption_Click" />
        </fieldset>
        <div class="mc-actions">
        <asp:Button ID="btnCancelQuestion" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" data-cancel="" />
        <asp:Button ID="btnSaveQuestion" runat="server" Text="Save Question" CssClass="btn btn-primary" OnClick="btnSaveQuestion_Click" /></div>
    </asp:Panel>
    <asp:Panel ID="pnlDelete" runat="server" Visible="false" CssClass="card mc-modal mc-delete" data-title="Confirm Deletion">
        <h2><asp:Literal ID="litDeleteTitle" runat="server" Mode="Encode" /></h2><p><asp:Literal ID="litDeleteDescription" runat="server" Mode="Encode" /></p><p><strong>This action cannot be undone.</strong></p>
        <div class="mc-actions">
        <asp:Button ID="btnCancelDelete" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" data-cancel="" />
        <asp:Button ID="btnConfirmDelete" runat="server" Text="Delete Permanently" CssClass="btn mc-danger" OnClick="btnConfirmDelete_Click" /></div>
    </asp:Panel>
</section>
