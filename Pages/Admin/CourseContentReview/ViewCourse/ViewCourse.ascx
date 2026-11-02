<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ViewCourse.ascx.cs" Inherits="CookingLah.Pages.Admin.CourseContentReview.ViewCourse.ViewCourse" %>
<%@ Register Src="~/Pages/Shared/LessonBlocks/LessonBlocks.ascx" TagPrefix="shared" TagName="LessonBlocks" %>

<div class="course-review">

    <%-- Back button, course status, and error message --%>
    <div class="review-header">
        <asp:LinkButton
            ID="btnBackToCourses"
            runat="server"
            CssClass="review-back"
            Text="← Back to Course Review"
            OnClick="Back_Click"
            CausesValidation="false" />
        <asp:Label ID="lblCourseStatus" runat="server" />
    </div>

    <asp:Panel
        ID="pnlError"
        runat="server"
        Visible="false"
        CssClass="review-message"
        role="alert">
        <asp:Literal
            ID="litError"
            runat="server"
            Mode="Encode" />
    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server">
        <asp:HiddenField
            ID="hfActiveTab"
            runat="server"
            Value="overview" />

        <%-- Published status and rejection reason --%>
        <asp:Panel
            ID="pnlPublished"
            runat="server"
            Visible="false"
            CssClass="review-message">
            This course has already been approved and published.
        </asp:Panel>

        <asp:Panel
            ID="pnlRejection"
            runat="server"
            Visible="false"
            CssClass="review-rejection">
            <h2>Reason for Rejection</h2>
            <p class="review-copy">
                <asp:Literal
                    ID="litRejection"
                    runat="server"
                    Mode="Encode" />
            </p>
        </asp:Panel>

        <%-- Course content tabs --%>
        <div class="review-tabs" role="tablist" aria-label="Course content">
            <button type="button" role="tab" data-tab="overview">Overview</button>
            <button type="button" role="tab" data-tab="recipe">Recipe</button>
            <button type="button" role="tab" data-tab="lessons">Lessons</button>
            <button type="button" role="tab" data-tab="quiz">Quiz</button>
            <button type="button" role="tab" data-tab="activity">Practical Activity</button>
        </div>

        <%-- Overview tab --%>
        <asp:Panel
            ID="tabOverview"
            runat="server"
            CssClass="review-tab-panel"
            data-section="overview"
            role="tabpanel">
            <h2>Course Overview</h2>
            <asp:Image
                ID="imgCourse"
                runat="server"
                CssClass="review-photo"
                AlternateText="Course thumbnail" />
            <asp:Panel
                ID="pnlCourseImagePlaceholder"
                runat="server"
                CssClass="review-photo course-image-placeholder"
                Visible="false"><img runat="server" src="~/F_Assets/mascot.png" alt="" /><span>Food Image Coming Soon</span></asp:Panel>

            <dl class="review-facts">
                <dt>Course title</dt>
                <dd><asp:Literal
                        ID="litOverviewTitle"
                        runat="server"
                        Mode="Encode" /></dd>
                <dt>Chef</dt>
                <dd><asp:Literal
                        ID="litChef"
                        runat="server"
                        Mode="Encode" /></dd>
                <dt>Category</dt>
                <dd><asp:Literal
                        ID="litCategory"
                        runat="server"
                        Mode="Encode" /></dd>
                <dt>Difficulty</dt>
                <dd><asp:Literal
                        ID="litDifficulty"
                        runat="server"
                        Mode="Encode" /></dd>
                <dt>Status</dt>
                <dd><asp:Literal
                        ID="litOverviewStatus"
                        runat="server"
                        Mode="Encode" /></dd>
                <dt>Created</dt>
                <dd><asp:Literal
                        ID="litCreated"
                        runat="server"
                        Mode="Encode" /></dd>
                <dt>Updated</dt>
                <dd><asp:Literal
                        ID="litUpdated"
                        runat="server"
                        Mode="Encode" /></dd>
            </dl>

            <h3>Description</h3>
            <p class="review-copy">
                <asp:Literal
                    ID="litDescription"
                    runat="server"
                    Mode="Encode" />
            </p>
            <div class="review-summary">
                <h3>Course Content</h3>
                <asp:Literal ID="litSummary" runat="server" />
            </div>
        </asp:Panel>

        <%-- Recipe tab --%>
        <asp:Panel
            ID="tabRecipe"
            runat="server"
            CssClass="review-tab-panel"
            data-section="recipe"
            role="tabpanel">
            <h2>Recipe</h2>
            <asp:Literal
                ID="litRecipeEmpty"
                runat="server"
                Text="No recipe has been added." />

            <asp:Repeater ID="rptRecipe" runat="server">
                <ItemTemplate>
                    <h3><%#: Eval("Title") %></h3>
                    <asp:Image
                        runat="server"
                        CssClass="review-photo"
                        ImageUrl='<%# MediaUrl(Eval("RecipeImage")) %>'
                        Visible='<%# MediaUrl(Eval("RecipeImage")).Length > 0 %>'
                        AlternateText="Recipe photo" />
                    <dl class="review-facts">
                        <dt>Cooking Time</dt>
                        <dd><%#: OptionalNumber(Eval("CookingTime"), " minutes") %></dd>
                        <dt>Serving Size</dt>
                        <dd><%#: OptionalNumber(Eval("ServingSize"), " people") %></dd>
                    </dl>
                    <h3>Description</h3>
                    <p class="review-copy"><%#: Eval("Description") %></p>
                    <h3>Ingredients</h3>
                    <p class="review-copy"><%#: FormatIngredients(Eval("Ingredients")) %></p>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <%-- Lessons tab --%>
        <asp:Panel
            ID="tabLessons"
            runat="server"
            CssClass="review-tab-panel"
            data-section="lessons"
            role="tabpanel">
            <h2>Lessons</h2>
            <asp:Literal
                ID="litLessonsEmpty"
                runat="server"
                Text="No lessons have been added." />

            <div class="review-lessons-layout">
                <aside class="review-lesson-selector" aria-label="Select a lesson">
                    <h3>Course lessons</h3>
                    <asp:Repeater
                        ID="rptLessonSelector"
                        runat="server"
                        OnItemCommand="LessonSelector_ItemCommand">
                        <ItemTemplate>
                            <asp:LinkButton
                                runat="server"
                                CssClass='<%# LessonSelectorClass(Eval("LessonID")) %>'
                                CommandName="SelectLesson"
                                CommandArgument='<%# Eval("LessonID") %>'
                                CausesValidation="false">
                                <span>Lesson <%#: Eval("LessonOrder") %></span>
                                <strong><%#: Eval("Title") %></strong>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:Repeater>
                </aside>

                <section class="review-lesson-content" aria-live="polite">
                    <asp:Repeater
                        ID="rptLessons"
                        runat="server"
                        OnItemDataBound="Lesson_ItemDataBound">
                        <ItemTemplate>
                            <article class="review-content-item">
                                <p class="review-eyebrow">Lesson <%#: Eval("LessonOrder") %></p>
                                <h3><%#: Eval("Title") %></h3>
                                <p class="review-copy"><%#: Eval("Description") %></p>
                                <shared:LessonBlocks ID="LessonBlocks" runat="server" />
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </section>
            </div>
        </asp:Panel>

        <%-- Quiz tab --%>
        <asp:Panel
            ID="tabQuiz"
            runat="server"
            CssClass="review-tab-panel"
            data-section="quiz"
            role="tabpanel">
            <h2>Quiz</h2>
            <asp:Literal
                ID="litQuizEmpty"
                runat="server"
                Text="No quiz has been added." />

            <%-- Quiz information --%>
            <asp:Repeater ID="rptQuizzes" runat="server">
                <ItemTemplate>
                    <article class="review-quiz-summary">
                        <h3><%#: Eval("Title") %></h3>
                        <p class="review-copy"><%#: Eval("Description") %></p>
                        <p>Passing Score: <%#: Eval("PassingScore") %>% &middot; <%#: Eval("QuestionCount") %> Questions</p>
                    </article>
                </ItemTemplate>
            </asp:Repeater>

            <%-- Questions and correct answers --%>
            <asp:Literal
                ID="litQuestionsEmpty"
                runat="server"
                Text="No quiz questions have been added." />
            <asp:Repeater
                ID="rptQuestions"
                runat="server"
                OnItemDataBound="Questions_ItemDataBound">
                <ItemTemplate>
                    <article class="review-content-item">
                        <p class="review-eyebrow">
                            <%#: Eval("QuizTitle") %> · Question <%#: Eval("QuestionOrder") %>
                        </p>
                        <h3 class="review-copy"><%#: Eval("QuestionText") %></h3>
                        <ul class="review-options">
                            <asp:Repeater ID="rptQuestionOptions" runat="server">
                                <ItemTemplate>
                                    <li class='<%# Convert.ToBoolean(Eval("IsCorrect")) ? "correct" : "" %>'>
                                        <span><%#: Eval("OptionText") %></span>
                                        <strong><%# Convert.ToBoolean(Eval("IsCorrect")) ? "Correct Answer" : "" %></strong>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </article>
                </ItemTemplate>
            </asp:Repeater>

            <%-- Question pagination --%>
            <asp:Panel
                ID="pnlQuestionPager"
                runat="server"
                CssClass="review-pagination">
                <asp:LinkButton
                    ID="btnQuestionPrevious"
                    runat="server"
                    CssClass="btn"
                    Text="‹ Previous"
                    OnClick="QuestionPrevious_Click" />
                <asp:Literal ID="litQuestionPage" runat="server" />
                <asp:LinkButton
                    ID="btnQuestionNext"
                    runat="server"
                    CssClass="btn"
                    Text="Next ›"
                    OnClick="QuestionNext_Click" />
            </asp:Panel>
        </asp:Panel>

        <%-- Practical activity tab --%>
        <asp:Panel
            ID="tabActivity"
            runat="server"
            CssClass="review-tab-panel"
            data-section="activity"
            role="tabpanel">
            <h2>Practical Activity</h2>
            <asp:Literal
                ID="litActivityEmpty"
                runat="server"
                Text="No practical activity has been added." />
            <asp:Repeater ID="rptActivities" runat="server">
                <ItemTemplate>
                    <article class="review-content-item">
                        <h3><%#: Eval("Title") %></h3>
                        <h4>Instructions</h4>
                        <p class="review-copy"><%#: Eval("Instructions") %></p>
                    </article>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <%-- Approve or reject course --%>
        <asp:Panel
            ID="pnlDecision"
            runat="server"
            CssClass="review-decision"
            Visible="false">
            <h2>Review Decision</h2>
            <p>Review all course content before making a decision.</p>
            <div class="review-decision-actions">
                <button type="button" class="btn review-reject" data-open-dialog="reject">Reject Course</button>
                <asp:PlaceHolder ID="phApproveAction" runat="server">
                    <button type="button" class="btn review-approve" data-open-dialog="approve">Approve Course</button>
                </asp:PlaceHolder>
            </div>
            <asp:HiddenField ID="hfDialog" runat="server" />

            <%-- Approval confirmation dialog --%>
            <dialog class="review-dialog" data-dialog="approve" aria-label="Approve Course?">
                <h2>Approve Course?</h2>
                <p>&quot;<asp:Literal
                        ID="litApproveTitle"
                        runat="server"
                        Mode="Encode" />&quot; will be published and available to learners.</p>
                <p>Pending → Approved</p>
                <div class="review-dialog-actions">
                    <button
                        type="button"
                        class="btn"
                        data-close-dialog
                        autofocus>Cancel</button>
                    <asp:Button
                        ID="btnApproveCourse"
                        runat="server"
                        CssClass="btn review-approve"
                        Text="Approve &amp; Publish"
                        OnClick="Approve_Click" />
                </div>
            </dialog>

            <%-- Rejection reason dialog --%>
            <dialog class="review-dialog" data-dialog="reject" aria-label="Reject Course">
                <h2>Reject Course</h2>
                <p>Please explain what the Chef needs to fix.</p>
                <asp:Label
                    runat="server"
                    AssociatedControlID="txtRejectionReason"
                    Text="Reason for Rejection *" />
                <asp:TextBox
                    ID="txtRejectionReason"
                    runat="server"
                    TextMode="MultiLine"
                    Rows="5"
                    MaxLength="500"
                    aria-required="true" />
                <p class="review-counter"><span data-character-count>0</span> / 500</p>
                <p class="review-validation" role="alert">
                    <asp:Literal
                        ID="litRejectError"
                        runat="server"
                        Mode="Encode" />
                </p>
                <div class="review-dialog-actions">
                    <button type="button" class="btn" data-close-dialog>Cancel</button>
                    <asp:Button
                        ID="btnRejectCourse"
                        runat="server"
                        CssClass="btn review-reject"
                        Text="Reject Course"
                        OnClick="Reject_Click" />
                </div>
            </dialog>
        </asp:Panel>
    </asp:Panel>

    <dialog
        class="lesson-video-dialog"
        data-lesson-video-dialog=""
        aria-labelledby="reviewVideoTitle"
        aria-describedby="reviewVideoStatus">
        <div class="lesson-video-heading">
            <h2 id="reviewVideoTitle">Lesson video</h2>
            <button
                type="button"
                class="btn"
                data-video-close=""
                autofocus>Close video</button>
        </div>
        <video
            controls
            playsinline
            preload="none"
            aria-label="Lesson video player"></video>
        <p id="reviewVideoStatus" class="lesson-video-status" role="status">
        </p>
    </dialog>
</div>
