<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="QuizSection.ascx.cs" Inherits="CookingLah.Pages.Learner.CourseLearning.QuizSection.QuizSection" %>

<asp:Panel ID="pnlNoQuiz" runat="server" Visible="false" CssClass="quiz-empty-state">
    <h2>No quiz is available</h2>
    <p>This course does not have a quiz yet. You can continue to its practical activity.</p>
    <asp:Button ID="btnNoQuizActivity" runat="server" Text="Open practical activity"
        CssClass="btn btn-primary" OnClick="btnContinueActivity_Click" CausesValidation="false" />
</asp:Panel>

<asp:Panel ID="pnlQuiz" runat="server" Visible="false" CssClass="quiz-section">
    <header class="quiz-heading">
        <p class="learning-eyebrow">Course assessment</p>
        <h2><asp:Literal ID="litQuizTitle" runat="server" Mode="Encode" /></h2>
        <p><asp:Literal ID="litQuizDescription" runat="server" Mode="Encode" /></p>
    </header>

    <asp:Panel ID="pnlQuizIntro" runat="server" CssClass="quiz-intro">
        <div class="quiz-intro-facts">
            <span><strong><asp:Literal ID="litQuizQuestionCount" runat="server" Mode="Encode" /></strong> questions</span>
            <span>Passing score <strong><asp:Literal ID="litPassingScore" runat="server" Mode="Encode" />%</strong></span>
        </div>
        <p>Take your time and choose the best answer for each question. Your result will be saved to your course.</p>
        <asp:Button ID="btnStartQuiz" runat="server" Text="Start quiz" CssClass="btn btn-primary"
            OnClick="btnStartQuiz_Click" CausesValidation="false" />
    </asp:Panel>

    <asp:Panel ID="pnlQuizQuestions" runat="server" Visible="false" CssClass="quiz-question-area">
        <div class="quiz-question-progress">
            <span id="quizQuestionProgressText">Question 1 of 1</span>
            <div id="quizQuestionProgressBar" class="quiz-question-progress-track" role="progressbar"
                aria-label="Quiz question progress" aria-valuemin="1" aria-valuemax="1" aria-valuenow="1">
                <span></span>
            </div>
        </div>

        <asp:Panel ID="pnlQuizFeedback" runat="server" Visible="false" CssClass="quiz-feedback" role="alert">
            <asp:Literal ID="litQuizFeedback" runat="server" Mode="Encode" />
        </asp:Panel>

        <asp:Repeater ID="rptQuizQuestions" runat="server" OnItemDataBound="rptQuizQuestions_ItemDataBound">
            <ItemTemplate>
                <article class="quiz-question-card" data-question-index="<%# Container.ItemIndex %>">
                    <p class="quiz-question-label">Question <%# Container.ItemIndex + 1 %></p>
                    <h3><%#: Eval("QuestionText") %></h3>
                    <div class="quiz-answer-list">
                        <asp:Repeater ID="rptQuestionOptions" runat="server">
                            <ItemTemplate>
                                <label class="quiz-answer-option">
                                    <input type="radio" name='<%# "answer_" + Eval("QuestionID") %>'
                                        value='<%# Eval("OptionID") %>'
                                        <%# AnswerInputAttributes(Eval("QuestionID"), Eval("OptionID")) %> />
                                    <span><%#: Eval("OptionText") %></span>
                                </label>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </article>
            </ItemTemplate>
        </asp:Repeater>

        <div class="quiz-question-navigation">
            <button id="btnPreviousQuestion" type="button" class="btn btn-secondary">Previous</button>
            <asp:Button ID="btnBackToQuizResult" runat="server" ClientIDMode="Static" Text="Back to result"
                CssClass="btn btn-secondary" Visible="false" OnClick="btnBackToQuizResult_Click" CausesValidation="false" />
            <button id="btnPreviewNextQuestion" runat="server" ClientIDMode="Static" type="button" class="btn btn-primary" visible="false">Next question</button>
            <asp:Button ID="btnSubmitQuiz" runat="server" ClientIDMode="Static" Text="Next question" CssClass="btn btn-primary"
                OnClick="btnSubmitQuiz_Click" OnClientClick="return window.CookingLahQuiz.handleAction();"
                CausesValidation="false" />
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlQuizResult" runat="server" Visible="false" CssClass="quiz-result">
        <p class="learning-eyebrow">Your result</p>
        <h3><asp:Literal ID="litQuizResultHeading" runat="server" Mode="Encode" /></h3>
        <p class="quiz-result-score"><strong><asp:Literal ID="litQuizScore" runat="server" Mode="Encode" />%</strong>
            <span><asp:Literal ID="litQuizCorrectCount" runat="server" Mode="Encode" /> correct answers</span></p>
        <p>Passing score: <strong><asp:Literal ID="litQuizPassingScoreResult" runat="server" Mode="Encode" />%</strong></p>
        <asp:Panel ID="pnlQuizPassedMessage" runat="server" Visible="false" CssClass="quiz-result-message is-passed">
            You passed the course quiz. Well done!
        </asp:Panel>
        <asp:Panel ID="pnlQuizFailedMessage" runat="server" Visible="false" CssClass="quiz-result-message is-failed">
            You have not passed yet. Review the lessons and try again when you are ready.
        </asp:Panel>
        <div class="quiz-result-actions">
            <asp:Button ID="btnReviewAnswers" runat="server" Text="Review correct answers" CssClass="btn btn-secondary"
                OnClick="btnReviewAnswers_Click" CausesValidation="false" />
            <asp:Button ID="btnReviewLessons" runat="server" Text="Review lessons" CssClass="btn btn-secondary"
                OnClick="btnReviewLessons_Click" CausesValidation="false" />
            <asp:Button ID="btnTryAgain" runat="server" Text="Try again" CssClass="btn btn-secondary"
                OnClick="btnTryAgain_Click" CausesValidation="false" />
            <asp:Button ID="btnContinueActivity" runat="server" Text="Continue to activity" CssClass="btn btn-primary"
                OnClick="btnContinueActivity_Click" CausesValidation="false" />
        </div>
    </asp:Panel>
</asp:Panel>
