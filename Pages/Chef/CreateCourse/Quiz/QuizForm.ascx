<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="QuizForm.ascx.cs" Inherits="CookingLah.Pages.Chef.CreateCourse.Quiz.QuizForm" %>
<div class="quiz-form">
    <p class="builder-hint">Optional. Leave the quiz title, description and questions empty to skip this step.</p>
    <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtQuizTitle" Text="Quiz title" /><asp:TextBox ID="txtQuizTitle" runat="server" MaxLength="150" /></div>
    <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtQuizDescription" Text="Description" /><asp:TextBox ID="txtQuizDescription" runat="server" TextMode="MultiLine" Rows="3" /></div>
    <div class="builder-field quiz-score"><asp:Label runat="server" AssociatedControlID="txtPassingScore" Text="Passing score (%)" /><asp:TextBox ID="txtPassingScore" runat="server" TextMode="Number" Text="60" min="0" max="100" /></div>
    <asp:Panel ID="pnlEmptyQuestions" runat="server" CssClass="builder-empty"><h3>No questions added yet.</h3><p>Add a question to test your learners.</p></asp:Panel>
    <asp:Repeater ID="rptQuestions" runat="server" OnItemDataBound="rptQuestions_ItemDataBound" OnItemCommand="rptQuestions_ItemCommand">
        <ItemTemplate>
            <article class="question-card card">
                <h3>Question <%# Eval("QuestionOrder") %></h3>
                <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtQuestionText" Text="Question *" /><asp:TextBox ID="txtQuestionText" runat="server" Text='<%# Eval("QuestionText") %>' TextMode="MultiLine" Rows="2" /></div>
                <fieldset class="question-options"><legend>Options: select one correct answer</legend>
                    <asp:Repeater ID="rptQuestionOptions" runat="server" OnItemCommand="rptQuestionOptions_ItemCommand">
                        <ItemTemplate>
                            <div class="question-option">
                                <label class="option-answer"><input type="radio" name='<%# AnswerGroup(Eval("QuestionKey")) %>' value='<%# Eval("OptionKey") %>' <%# Convert.ToBoolean(Eval("IsCorrect")) ? "checked" : "" %> /><span class="visually-hidden">Mark option <%# Container.ItemIndex + 1 %> as correct</span></label>
                                <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtOptionText" Text='<%# "Option " + (Container.ItemIndex + 1) %>' /><asp:TextBox ID="txtOptionText" runat="server" MaxLength="500" Text='<%# Eval("OptionText") %>' /></div>
                                <asp:Button runat="server" Text="Remove" CssClass="btn option-remove" CommandName="RemoveOption" CommandArgument='<%# Eval("OptionKey") %>' CausesValidation="false" OnClientClick="return CookingQuiz.confirmOption();" />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </fieldset>
                <div class="question-actions"><asp:Button runat="server" Text="+ Add option" CssClass="btn" CommandName="AddOption" CausesValidation="false" /><asp:Button runat="server" Text="Delete question" CssClass="btn" CommandName="DeleteQuestion" data-order='<%# Eval("QuestionOrder") %>' CausesValidation="false" OnClientClick="return CookingQuiz.confirmQuestion(this);" /></div>
            </article>
        </ItemTemplate>
    </asp:Repeater>
    <asp:Panel ID="pnlQuestionPager" runat="server" CssClass="builder-pager" aria-label="Question pages">
        <asp:Button ID="btnPreviousQuestions" runat="server" Text="Previous" CssClass="btn" OnClick="btnPreviousQuestions_Click" CausesValidation="false" />
        <asp:Label ID="lblQuestionPage" runat="server" />
        <asp:Button ID="btnNextQuestions" runat="server" Text="Next" CssClass="btn" OnClick="btnNextQuestions_Click" CausesValidation="false" />
    </asp:Panel>
    <asp:Button ID="btnAddQuestion" runat="server" Text="+ Add first question" CssClass="btn" OnClick="btnAddQuestion_Click" CausesValidation="false" />
</div>
