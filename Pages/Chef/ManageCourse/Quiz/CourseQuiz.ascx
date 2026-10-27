<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" CodeBehind="CourseQuiz.ascx.cs" Inherits="CookingLah.Pages.Chef.ManageCourse.Quiz.CourseQuiz" %>
<section class="lesson-builder quiz-builder" data-quiz-builder data-editing="<%= CanEdit ? "true" : "false" %>">
    <asp:HiddenField ID="hfQuestion" runat="server" />
    <asp:HiddenField ID="hfStructure" runat="server" />
    <aside class="lb-sidebar" aria-label="Quiz questions">
        <div class="lb-sidebar-heading"><strong>Quiz structure</strong><span class="lb-count"><%= QuestionCount %> questions</span></div>
        <div class="lb-sidebar-body">
            <asp:LinkButton ID="btnQuizInfo" runat="server" Text="Quiz information" OnClick="btnQuizInfo_Click" CausesValidation="false" />
            <asp:Button ID="btnAddQuestion" runat="server" Text="+ Add Question" CssClass="btn lb-add-lesson" OnClick="btnAddQuestion_Click" CausesValidation="false" />
            <asp:Panel ID="pnlEmptyQuestions" runat="server" CssClass="lb-empty"><p>No questions yet.</p><p><%= CanEdit ? "Add a question to begin building your quiz." : "This quiz has no questions yet." %></p></asp:Panel>
            <div class="qb-question-list" data-question-order>
                <asp:Repeater ID="rptQuestions" runat="server" OnItemCommand="rptQuestions_ItemCommand"><ItemTemplate>
                    <div class='qb-question <%# Convert.ToInt32(Eval("QuestionID")) == SelectionId ? "is-selected" : "" %>' data-question-id='<%# Eval("QuestionID") %>'>
                        <div class="qb-question-label"><asp:PlaceHolder runat="server" Visible='<%# CanEdit %>'><button type="button" class="qb-drag" draggable="true" aria-label="Drag question" title="Drag to reorder">&#9776;</button></asp:PlaceHolder>
                        <asp:LinkButton runat="server" CssClass="qb-select" CommandName="SelectQuestion" CommandArgument='<%# Eval("QuestionID") %>' CausesValidation="false" aria-current='<%# Convert.ToInt32(Eval("QuestionID")) == SelectionId ? "true" : "false" %>'><span>Question <%#: Eval("QuestionOrder") %></span><strong><%#: QuestionTitle(Eval("QuestionText")) %></strong></asp:LinkButton></div>
                        <asp:PlaceHolder runat="server" Visible='<%# CanEdit %>'><div class="qb-list-actions"><button type="button" data-qb-move="-1" aria-label="Move question up" <%# Container.ItemIndex == 0 ? "disabled" : "" %>>&#8593;</button><button type="button" data-qb-move="1" aria-label="Move question down" <%# Container.ItemIndex == QuestionCount - 1 ? "disabled" : "" %>>&#8595;</button><asp:LinkButton runat="server" Text="Delete" CssClass="lb-delete" CommandName="DeleteQuestion" CommandArgument='<%# Eval("QuestionID") %>' CausesValidation="false" OnClientClick="return confirm('Delete this question and all its answer options? Save Changes confirms the deletion.');" /></div></asp:PlaceHolder>
                    </div>
                </ItemTemplate></asp:Repeater>
            </div>
        </div>
    </aside>
    <div class="lb-middle qb-main">
        <asp:Panel ID="pnlEmpty" runat="server" CssClass="lb-empty qb-empty"><h2>Build your course quiz</h2><p>No quiz has been added to this course yet.</p><asp:Button ID="btnAddQuiz" runat="server" Text="+ Add Quiz" CssClass="btn btn-primary" OnClick="btnAddQuiz_Click" CausesValidation="false" /></asp:Panel>
        <asp:Panel ID="pnlQuizInfo" runat="server">
            <header class="qb-heading"><h2>Quiz information</h2><p>Set the title, instructions and passing score.</p></header>
            <asp:Panel ID="pnlQuizFields" runat="server">
                <div class="lb-field"><asp:Label runat="server" AssociatedControlID="txtQuizTitle" Text="Quiz title *" /><asp:TextBox ID="txtQuizTitle" runat="server" MaxLength="150" data-qb-required="Quiz title is required." placeholder="e.g. Ikan Bakar knowledge check" /><asp:Label ID="errQuizTitle" runat="server" CssClass="lb-error" /></div>
                <div class="lb-field"><asp:Label runat="server" AssociatedControlID="txtQuizDescription" Text="Description (optional)" /><asp:TextBox ID="txtQuizDescription" runat="server" TextMode="MultiLine" Rows="4" data-qb-maxlength="500" placeholder="Tell learners what this quiz covers." /><asp:Label ID="errQuizDescription" runat="server" CssClass="lb-error" /></div>
                <div class="lb-field qb-score"><asp:Label runat="server" AssociatedControlID="txtPassingScore" Text="Passing score (%) *" /><asp:TextBox ID="txtPassingScore" runat="server" MaxLength="3" inputmode="numeric" data-qb-score="" /><small>A percentage from 0 to 100.</small><asp:Label ID="errScore" runat="server" CssClass="lb-error" /></div>
            </asp:Panel>
            <asp:Panel ID="pnlQuizView" runat="server" CssClass="qb-view"><h3><asp:Literal ID="litQuizTitle" runat="server" Mode="Encode" /></h3><p class="lb-copy"><asp:Literal ID="litQuizDescription" runat="server" Mode="Encode" /></p><div class="qb-facts"><span>Passing score <strong><asp:Literal ID="litPassingScore" runat="server" Mode="Encode" />%</strong></span><span><strong><%= QuestionCount %></strong> questions</span></div><p class="qb-hint">Choose a question from the list to view its answers.</p></asp:Panel>
            <asp:Button ID="btnDeleteQuiz" runat="server" Text="Delete Quiz" CssClass="qb-delete-quiz" OnClick="btnDeleteQuiz_Click" CausesValidation="false" OnClientClick="return confirm('Delete this quiz? All questions, answer options and learner quiz attempts will be removed when you Save Changes.');" />
        </asp:Panel>
        <asp:Panel ID="pnlQuestion" runat="server">
            <header class="qb-heading"><h2><asp:Literal ID="litQuestionNumber" runat="server" Mode="Encode" /></h2><p><%= CanEdit ? "Write a clear question and select one correct answer." : "Question and answer options" %></p></header>
            <asp:Panel ID="pnlQuestionFields" runat="server">
                <div class="lb-field"><asp:Label runat="server" AssociatedControlID="txtQuestionText" Text="Question *" /><asp:TextBox ID="txtQuestionText" runat="server" TextMode="MultiLine" Rows="4" data-qb-required="Question text is required." placeholder="What should learners know?" /><asp:Label ID="errQuestion" runat="server" CssClass="lb-error" /></div>
                <fieldset class="qb-options"><legend>Answer options</legend><p class="qb-hint">Provide at least two answers. Select the circle beside the correct answer.</p>
                    <asp:Repeater ID="rptEditOptions" runat="server" OnItemCommand="rptEditOptions_ItemCommand"><ItemTemplate>
                        <div class="qb-option">
                            <label class="qb-correct-choice"><input type="radio" name="<%# AnswerGroup %>" value='<%# Eval("OptionID") %>' <%# Convert.ToBoolean(Eval("IsCorrect")) ? "checked" : "" %> /><span>Correct<span class="qb-sr-only"> answer: option <%# Container.ItemIndex + 1 %></span></span></label>
                            <div class="lb-field"><asp:Label runat="server" AssociatedControlID="txtOptionText" Text='<%# "Option " + (Container.ItemIndex + 1) + " *" %>' /><asp:TextBox ID="txtOptionText" runat="server" Text='<%# Eval("OptionText") %>' MaxLength="500" data-qb-required="Answer text is required." /><asp:Label ID="errOption" runat="server" CssClass="lb-error" /></div>
                            <asp:LinkButton runat="server" Text="Remove" CssClass="lb-delete" CommandName="RemoveOption" CommandArgument='<%# Eval("OptionID") %>' Visible='<%# OptionCount > 2 %>' CausesValidation="false" />
                        </div>
                    </ItemTemplate></asp:Repeater>
                    <asp:Label ID="errOptions" runat="server" CssClass="lb-error" />
                    <asp:Button ID="btnAddOption" runat="server" Text="+ Add Option" CssClass="btn qb-add-option" OnClick="btnAddOption_Click" CausesValidation="false" />
                </fieldset>
            </asp:Panel>
            <asp:Panel ID="pnlQuestionView" runat="server" CssClass="qb-view"><h3 class="lb-copy"><asp:Literal ID="litQuestionText" runat="server" Mode="Encode" /></h3><ol class="qb-read-options"><asp:Repeater ID="rptViewOptions" runat="server"><ItemTemplate><li class='<%# Convert.ToBoolean(Eval("IsCorrect")) ? "is-correct" : "" %>'><span><%#: Eval("OptionText") %></span><asp:PlaceHolder runat="server" Visible='<%# Convert.ToBoolean(Eval("IsCorrect")) %>'><strong class="qb-answer-badge">&#10003; Correct answer</strong></asp:PlaceHolder></li></ItemTemplate></asp:Repeater></ol></asp:Panel>
        </asp:Panel>
        <asp:Panel ID="pnlValidation" runat="server" Visible="false" CssClass="lb-validation" role="alert" tabindex="-1">Unable to save. Check the marked fields.</asp:Panel>
        <div class="lb-footer"><asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click" CausesValidation="false" data-save-quiz="" /></div>
        <p class="lb-state" role="status" aria-live="polite"><asp:Literal ID="litState" runat="server" Mode="Encode" /></p>
        <% if (CanEdit) { %><p class="lb-save-note">Edits stay in your working copy. Use Save Changes above to save the course.</p><% } %>
    </div>
    <asp:HiddenField ID="hfOrderIds" runat="server" />
    <asp:Button ID="btnSaveOrder" runat="server" Text="Save order" OnClick="btnSaveOrder_Click" CssClass="lb-order-submit" CausesValidation="false" />
</section>
