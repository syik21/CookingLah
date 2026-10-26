<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ViewCourseOverivew.ascx.cs" Inherits="CookingLah.Pages.Shared.CourseSearch.ViewCourseOverivew" %>

<asp:Panel ID="pnlGuestLogin" runat="server" CssClass="course-login-backdrop" Visible="false" role="dialog" aria-modal="true" aria-labelledby="guestCourseTitle">
    <section class="course-login-dialog">
        <asp:Button ID="btnCloseGuestPrompt" runat="server" Text="Close" CssClass="course-dialog-close" OnClick="btnCloseGuestPrompt_Click" CausesValidation="false" aria-label="Close sign-in message" />
        <p class="course-overview-kicker">A place at the table</p>
        <h2 id="guestCourseTitle">Sign in to start learning</h2>
        <p>You need a Cooking LAH account to start this course and track your learning progress.</p>
        <div class="course-login-actions">
            <a class="btn btn-primary" href="<%= ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login") %>">Log in</a>
            <a class="btn btn-secondary" href="<%= ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=signup") %>">Create account</a>
        </div>
    </section>
</asp:Panel>

<asp:LinkButton ID="btnBackToSearch" runat="server" CssClass="course-back-link" OnClick="btnBackToSearch_Click" CausesValidation="false">
    <svg class="course-back-icon" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
        <path d="M19 12H5M12 19l-7-7 7-7" />
    </svg>
    <span>Back to courses</span>
</asp:LinkButton>

<asp:Panel ID="pnlCourseDetails" runat="server" CssClass="course-overview">
    <section class="course-overview-hero" aria-labelledby="courseTitle">
        <div class="overview-hero-copy">
            <p class="course-overview-kicker">Course notebook</p>
            <h1 id="courseTitle"><asp:Literal ID="litCourseTitle" runat="server" Mode="Encode" /></h1>
            <p class="course-overview-meta"><asp:Literal ID="litCategory" runat="server" Mode="Encode" /> <span aria-hidden="true">/</span> <asp:Literal ID="litDifficulty" runat="server" Mode="Encode" /></p>
            <p class="course-overview-chef">Created by <strong><asp:Literal ID="litChefName" runat="server" Mode="Encode" /></strong></p>
            <p class="course-overview-rating"><asp:Literal ID="litRating" runat="server" Mode="Encode" /></p>
            <p class="course-overview-description"><asp:Literal ID="litDescription" runat="server" Mode="Encode" /></p>
        </div>
        <div class="course-overview-image-wrap">
            <asp:Image ID="imgCourseThumbnail" runat="server" CssClass="course-overview-image" AlternateText="Course thumbnail" />
            <asp:Panel ID="pnlCourseImagePlaceholder" runat="server" CssClass="course-overview-placeholder" Visible="false" aria-hidden="true">Ready to cook</asp:Panel>
        </div>
    </section>

    <section class="course-overview-stats" aria-label="Course content summary">
        <article><strong><asp:Literal ID="litLessonCount" runat="server" /></strong><span>Lessons</span></article>
        <article><strong><asp:Literal ID="litQuizCount" runat="server" /></strong><span>Quizzes</span></article>
        <article><strong><asp:Literal ID="litActivityCount" runat="server" /></strong><span>Activities</span></article>
    </section>

    <div class="course-overview-sections">
        <section class="overview-section overview-recipe" aria-labelledby="recipeTitle">
            <div class="overview-section-heading"><p class="course-overview-kicker">First, the ingredients</p><h2 id="recipeTitle">Recipe overview</h2></div>
            <asp:Panel ID="pnlRecipeEmpty" runat="server" Visible="false" CssClass="overview-empty">Recipe details are not available yet.</asp:Panel>
            <asp:Repeater ID="rptRecipe" runat="server">
                <ItemTemplate>
                    <div class="recipe-preview">
                        <asp:Image ID="imgRecipe" runat="server" CssClass="recipe-preview-image"
                            ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("RecipeImage"))) %>'
                            Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("RecipeImage")))) %>'
                            AlternateText="Recipe photo" loading="lazy" />
                        <div class="recipe-preview-details">
                            <div class="recipe-preview-facts">
                                <p><span>Cooking time</span><strong><%#: Eval("CookingTime") == DBNull.Value ? "Not specified" : Eval("CookingTime") + " minutes" %></strong></p>
                                <p><span>Serving size</span><strong><%#: Eval("ServingSize") == DBNull.Value ? "Not specified" : Eval("ServingSize") %></strong></p>
                            </div>
                            <h3>Ingredients</h3>
                            <pre class="recipe-ingredients"><%#: CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.FormatIngredients(Eval("Ingredients")) %></pre>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </section>

        <section class="overview-section" aria-labelledby="lessonsTitle">
            <div class="overview-section-heading"><p class="course-overview-kicker">A few steps at a time</p><h2 id="lessonsTitle">What you&rsquo;ll learn</h2></div>
            <asp:Panel ID="pnlLessonsEmpty" runat="server" Visible="false" CssClass="overview-empty">Lessons will appear here when they are ready.</asp:Panel>
            <ol class="lesson-preview-list">
                <asp:Repeater ID="rptLessonOverview" runat="server">
                    <ItemTemplate>
                        <li><span class="lesson-preview-order"><%#: Eval("LessonOrder") %></span><span><%#: Eval("Title") %></span></li>
                    </ItemTemplate>
                </asp:Repeater>
            </ol>
        </section>

        <section class="overview-section overview-quiz" aria-labelledby="quizTitle">
            <div class="overview-section-heading"><p class="course-overview-kicker">Check what you know</p><h2 id="quizTitle">Quiz</h2></div>
            <asp:Panel ID="pnlQuizEmpty" runat="server" Visible="false" CssClass="overview-empty">This course does not have a quiz yet.</asp:Panel>
            <asp:Repeater ID="rptQuizOverview" runat="server">
                <ItemTemplate>
                    <article class="overview-quiz-card">
                        <h3><%#: Eval("Title") %></h3>
                        <p><%#: Eval("Description") %></p>
                        <div><span><%#: Eval("QuestionCount") %> questions</span><span>Pass with <%#: Eval("PassingScore") %>%</span></div>
                    </article>
                </ItemTemplate>
            </asp:Repeater>
        </section>

        <section class="overview-section overview-activity" aria-labelledby="activityTitle">
            <div class="overview-section-heading"><p class="course-overview-kicker">Put it into practice</p><h2 id="activityTitle">Practical activity</h2></div>
            <asp:Panel ID="pnlActivityEmpty" runat="server" Visible="false" CssClass="overview-empty">No practical activity has been added yet.</asp:Panel>
            <asp:Repeater ID="rptActivityOverview" runat="server">
                <ItemTemplate>
                    <article class="overview-activity-card"><h3><%#: Eval("Title") %></h3><p><%#: Eval("ShortInstructions") %></p></article>
                </ItemTemplate>
            </asp:Repeater>
        </section>
    </div>

    <footer class="course-start-footer">
        <div><p class="course-overview-kicker">Ready when you are</p><p class="course-start-note">Your next kitchen skill starts with one lesson.</p></div>
        <asp:Button ID="btnStartCourse" runat="server" Text="Start course" CssClass="btn btn-primary course-start-button" OnClick="btnStartCourse_Click" CausesValidation="false" />
    </footer>
</asp:Panel>

