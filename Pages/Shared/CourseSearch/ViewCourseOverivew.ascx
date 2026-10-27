<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ViewCourseOverivew.ascx.cs" Inherits="CookingLah.Pages.Shared.CourseSearch.ViewCourseOverivew" %>
<%-- Guest sign-in prompt --%>
<asp:Panel ID="pnlGuestLogin" runat="server" CssClass="course-login-backdrop" Visible="false" role="dialog" aria-modal="true" aria-labelledby="guestCourseTitle">
    <section class="course-login-dialog">
        <asp:Button ID="btnCloseGuestPrompt" runat="server" Text="Close" CssClass="course-dialog-close" OnClick="btnCloseGuestPrompt_Click" CausesValidation="false" aria-label="Close sign-in message" />
        <p class="course-overview-kicker">
            A place at the table
        </p>
        <h2 id="guestCourseTitle">
            Sign in to start learning
        </h2>
        <p>
            You need a Cooking LAH account to start this course and track your progress.
        </p>
        <div class="course-login-actions">
            <a class="btn btn-primary" href="<%= ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login") %>">
                Log in
            </a>
            <a class="btn btn-secondary" href="<%= ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=signup") %>">
                Create account
            </a>
        </div>
    </section>
</asp:Panel>
<div class="course-overview-topbar">
    <asp:LinkButton ID="btnBackToSearch" runat="server" CssClass="course-back-link" OnClick="btnBackToSearch_Click" CausesValidation="false">
        <svg class="course-back-icon" viewBox="0 0 24 24" aria-hidden="true">
            <path d="M19 12H5M12 19l-7-7 7-7" />
        </svg>
        <span>
            Back to Search Course
        </span>
    </asp:LinkButton>
    <h1>
        Course Overview
    </h1>
</div>
<%-- Course details and overview --%>
<asp:Panel ID="pnlCourseDetails" runat="server" CssClass="course-overview-layout">
    <main class="course-overview-main">
        <section class="course-intro-card" aria-labelledby="courseTitle">
            <div class="course-overview-image-wrap">
                <asp:Image ID="imgCourseThumbnail" runat="server" CssClass="course-overview-image" AlternateText="Course thumbnail" />
                <asp:Panel ID="pnlCourseImagePlaceholder" runat="server" CssClass="course-overview-placeholder course-image-placeholder" Visible="false"><img runat="server" src="~/F_Assets/mascot.png" alt="" /><span>Food Image Coming Soon</span></asp:Panel>
            </div>
            <div class="course-intro-copy">
                <h2 id="courseTitle">
                    <asp:Literal ID="litCourseTitle" runat="server" Mode="Encode" />
                </h2>
                <p class="course-overview-chef">
                    <span>
                        Instructor
                    </span>
                    <strong>
                        <asp:Literal ID="litChefName" runat="server" Mode="Encode" />
                    </strong>
                </p>
                <p class="course-overview-meta">
                    <asp:Literal ID="litCategory" runat="server" Mode="Encode" />
                    <asp:PlaceHolder ID="phFoodCulture" runat="server">
                        <span>
                            &bull;
                        </span>
                        <asp:Literal ID="litFoodCulture" runat="server" Mode="Encode" />
                    </asp:PlaceHolder>
                </p>
                <p class="course-difficulty">
                    <asp:Literal ID="litDifficulty" runat="server" Mode="Encode" />
                </p>
                <div class="course-about">
                    <p class="course-overview-kicker">
                        About this course
                    </p>
                    <p class="course-overview-description">
                        <asp:Literal ID="litDescription" runat="server" Mode="Encode" />
                    </p>
                </div>
                <div class="course-content-summary">
                    <p class="course-overview-kicker">
                        Course content
                    </p>
                    <div class="course-overview-stats">
                        <span>
                            <strong>
                                <asp:Literal ID="litLessonCount" runat="server" />
                            </strong>
                            <asp:Literal ID="litLessonLabel" runat="server" />
                        </span>
                        <span>
                            <strong>
                                <asp:Literal ID="litQuizCount" runat="server" />
                            </strong>
                            <asp:Literal ID="litQuizLabel" runat="server" />
                        </span>
                        <span>
                            <strong>
                                <asp:Literal ID="litActivityCount" runat="server" />
                            </strong>
                            <asp:Literal ID="litActivityLabel" runat="server" />
                        </span>
                    </div>
                </div>
            </div>
        </section>
        <div class="course-overview-sections">
            <section class="overview-section" aria-labelledby="recipeTitle">
                <div class="overview-section-heading">
                    <p class="course-overview-kicker">
                        First, the ingredients
                    </p>
                    <h2 id="recipeTitle">
                        Recipe overview
                    </h2>
                </div>
                <%-- Empty recipe message --%>
                <asp:Panel ID="pnlRecipeEmpty" runat="server" Visible="false" CssClass="overview-empty">
                    Recipe details are not available yet.
                </asp:Panel>
                <%-- Recipe information --%>
                <asp:Repeater ID="rptRecipe" runat="server">
                    <ItemTemplate>
                        <div class="recipe-preview">
                            <asp:Image runat="server" CssClass="recipe-preview-image" ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("RecipeImage"))) %>' Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("RecipeImage")))) %>' AlternateText="Recipe photo" loading="lazy" />
                            <div>
                                <div class="recipe-preview-facts">
                                    <p>
                                        <span>
                                            Cooking time
                                        </span>
                                        <strong>
                                            <%#: Eval("CookingTime") == DBNull.Value ? "Not specified" : Eval("CookingTime") + " minutes" %>
                                        </strong>
                                    </p>
                                    <p>
                                        <span>
                                            Serving size
                                        </span>
                                        <strong>
                                            <%#: Eval("ServingSize") == DBNull.Value ? "Not specified" : Eval("ServingSize") %>
                                        </strong>
                                    </p>
                                </div>
                                <h3>
                                    Ingredients
                                </h3>
                                <pre class="recipe-ingredients"><%#: CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.FormatIngredients(Eval("Ingredients")) %></pre>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </section>
            <section class="overview-section" aria-labelledby="lessonsTitle">
                <div class="overview-section-heading">
                    <p class="course-overview-kicker">
                        A few steps at a time
                    </p>
                    <h2 id="lessonsTitle">
                        What you&rsquo;ll learn
                    </h2>
                </div>
                <asp:Panel ID="pnlLessonsEmpty" runat="server" Visible="false" CssClass="overview-empty">
                    Lessons will appear here when ready.
                </asp:Panel>
                <ol class="lesson-preview-list">
                    <%-- Lesson preview list --%>
                    <asp:Repeater ID="rptLessonOverview" runat="server">
                        <ItemTemplate>
                            <li>
                                <span class="lesson-preview-order">
                                    <%#: Eval("LessonOrder") %>
                                </span>
                                <span>
                                    <%#: Eval("Title") %>
                                </span>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ol>
            </section>
            <section class="overview-section" aria-labelledby="quizTitle">
                <div class="overview-section-heading">
                    <p class="course-overview-kicker">
                        Check what you know
                    </p>
                    <h2 id="quizTitle">
                        Quiz
                    </h2>
                </div>
                <asp:Panel ID="pnlQuizEmpty" runat="server" Visible="false" CssClass="overview-empty">
                    This course does not have a quiz yet.
                </asp:Panel>
                <%-- Quiz preview --%>
                <asp:Repeater ID="rptQuizOverview" runat="server">
                    <ItemTemplate>
                        <article class="overview-quiz-card">
                            <h3>
                                <%#: Eval("Title") %>
                            </h3>
                            <p>
                                <%#: Eval("Description") %>
                            </p>
                            <div>
                                <span>
                                    <%#: Eval("QuestionCount") %>
                                    questions
                                </span>
                                <span>
                                    Pass with
                                    <%#: Eval("PassingScore") %>
                                    %
                                </span>
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </section>
            <section class="overview-section" aria-labelledby="activityTitle">
                <div class="overview-section-heading">
                    <p class="course-overview-kicker">
                        Put it into practice
                    </p>
                    <h2 id="activityTitle">
                        Practical activity
                    </h2>
                </div>
                <asp:Panel ID="pnlActivityEmpty" runat="server" Visible="false" CssClass="overview-empty">
                    No practical activity has been added yet.
                </asp:Panel>
                <%-- Practical activities --%>
                <asp:Repeater ID="rptActivityOverview" runat="server">
                    <ItemTemplate>
                        <article class="overview-activity-card">
                            <h3>
                                <%#: Eval("Title") %>
                            </h3>
                            <p>
                                <%#: Eval("ShortInstructions") %>
                            </p>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </section>
        </div>
        <section id="course-reviews" class="overview-section course-reviews-section" aria-labelledby="courseReviewsTitle">
            <div class="overview-section-heading">
                <p class="course-overview-kicker">
                    From the community
                </p>
                <h2 id="courseReviewsTitle">
                    Course Reviews
                </h2>
            </div>
            <asp:Panel ID="pnlReviewsEmpty" runat="server" Visible="false" CssClass="overview-empty">
                No reviews yet.
            </asp:Panel>
            <div class="full-review-list">
                <%-- Community reviews --%>
                <asp:Repeater ID="rptAllReviews" runat="server">
                    <ItemTemplate>
                        <article class="review-item">
                            <div class="review-stars" aria-label='<%# Eval("Rating") + " out of 5 stars" %>'>
                                <%# StarText(Eval("Rating")) %>
                            </div>
                            <p>
                                <%#: Eval("Comment") %>
                            </p>
                            <footer>
                                <strong>
                                    <%#: Eval("LearnerName") %>
                                </strong>
                                <time datetime='<%# ReviewDateValue(Eval("CreatedAt")) %>'>
                                    <%#: ReviewDate(Eval("CreatedAt")) %>
                                </time>
                            </footer>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <asp:Panel ID="pnlReviewPager" runat="server" CssClass="review-pager" Visible="false">
                <asp:Button ID="btnReviewsPrevious" runat="server" Text="Previous" CssClass="btn btn-secondary" OnClick="btnReviewsPrevious_Click" CausesValidation="false" />
                <asp:Label ID="lblReviewPage" runat="server" />
                <asp:Button ID="btnReviewsNext" runat="server" Text="Next" CssClass="btn btn-secondary" OnClick="btnReviewsNext_Click" CausesValidation="false" />
            </asp:Panel>
        </section>
        <section id="your-review" class="overview-section your-review-section" aria-labelledby="yourReviewTitle" tabindex="-1">
            <div class="overview-section-heading">
                <p class="course-overview-kicker">
                    Your experience
                </p>
                <h2 id="yourReviewTitle">
                    Your Review
                </h2>
            </div>
            <asp:Panel ID="pnlReviewFeedback" runat="server" Visible="false" CssClass="review-feedback" role="status">
                <asp:Literal ID="litReviewFeedback" runat="server" Mode="Encode" />
            </asp:Panel>
            <asp:Panel ID="pnlReviewUnavailable" runat="server">
                <p>
                    <asp:Literal ID="litReviewUnavailable" runat="server" Mode="Encode" />
                </p>
            </asp:Panel>
            <%-- Submit a course review --%>
            <asp:Panel ID="pnlReviewForm" runat="server" Visible="false" CssClass="review-form">
                <p>
                    Share your experience with other learners.
                </p>
                <fieldset class="star-rating">
                    <legend>
                        Rating *
                    </legend>
                    <asp:RadioButtonList ID="rblRating" runat="server" RepeatDirection="Horizontal" CssClass="star-rating-options">
                        <asp:ListItem Value="1" Text="1" />
                        <asp:ListItem Value="2" Text="2" />
                        <asp:ListItem Value="3" Text="3" />
                        <asp:ListItem Value="4" Text="4" />
                        <asp:ListItem Value="5" Text="5" />
                    </asp:RadioButtonList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="rblRating" ValidationGroup="Review" ErrorMessage="Choose a rating from 1 to 5." CssClass="field-error" Display="Dynamic" />
                </fieldset>
                <div class="review-comment-field">
                    <label for="<%= txtReviewComment.ClientID %>">
                        Review *
                    </label>
                    <asp:TextBox ID="txtReviewComment" runat="server" TextMode="MultiLine" Rows="5" MaxLength="2000" />
                    <small>
                        Required. Maximum 2,000 characters.
                    </small>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtReviewComment" ValidationGroup="Review" ErrorMessage="Enter your review." CssClass="field-error" Display="Dynamic" />
                </div>
                <div class="review-form-actions">
                    <asp:Button ID="btnCancelReviewEdit" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancelReviewEdit_Click" CausesValidation="false" Visible="false" />
                    <asp:Button ID="btnSaveReview" runat="server" Text="Submit Review" CssClass="btn btn-primary" OnClick="btnSaveReview_Click" ValidationGroup="Review" />
                </div>
            </asp:Panel>
            <%-- Existing learner review --%>
            <asp:Panel ID="pnlMyReview" runat="server" Visible="false" CssClass="my-review-card">
                <div class="review-stars">
                    <asp:Literal ID="litMyReviewStars" runat="server" />
                </div>
                <p>
                    <asp:Literal ID="litMyReviewComment" runat="server" Mode="Encode" />
                </p>
                <p class="review-date">
                    <asp:Literal ID="litMyReviewDate" runat="server" Mode="Encode" />
                </p>
                <div class="review-form-actions">
                    <asp:Button ID="btnEditReview" runat="server" Text="Edit Review" CssClass="btn btn-secondary" OnClick="btnEditReview_Click" CausesValidation="false" />
                    <asp:Button ID="btnDeleteReview" runat="server" Text="Delete Review" CssClass="btn btn-secondary review-delete" OnClick="btnDeleteReview_Click" CausesValidation="false" OnClientClick="return confirm('Delete your review? This cannot be undone.');" />
                </div>
            </asp:Panel>
        </section>
    </main>
    <aside class="course-sidebar" aria-label="Course rating, reviews and action">
        <div class="course-sidebar-sticky">
            <section class="sidebar-card rating-summary-card">
                <%-- Rating summary and breakdown --%>
                <asp:Panel ID="pnlHasRatings" runat="server">
                    <div class="rating-summary">
                        <span class="rating-stars">
                            <asp:Literal ID="litRatingStars" runat="server" />
                        </span>
                        <strong>
                            <asp:Literal ID="litAverageRating" runat="server" />
                            / 5
                        </strong>
                    </div>
                    <p class="rating-total">
                        <asp:Literal ID="litReviewCount" runat="server" />
                        learner reviews
                    </p>
                    <button type="button" class="rating-breakdown-toggle" aria-expanded="false" aria-controls="ratingBreakdown">
                        <span>
                            View rating breakdown
                        </span>
                        <span aria-hidden="true">
                            &#9662;
                        </span>
                    </button>
                    <div id="ratingBreakdown" class="rating-breakdown" hidden>
                        <asp:Repeater ID="rptRatingBreakdown" runat="server">
                            <ItemTemplate>
                                <div class="rating-row">
                                    <span>
                                        <%#: Eval("Stars") %>
                                        &#9733;
                                    </span>
                                    <span class="rating-bar-track">
                                        <span class="rating-bar-fill" style='<%# "width:" + Eval("Percentage") + "%" %>'>
                                        </span>
                                    </span>
                                    <span>
                                        <%#: Eval("Count") %>
                                    </span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlNoRatings" runat="server" Visible="false" CssClass="no-ratings">
                    <strong>
                        No reviews yet
                    </strong>
                    <span>
                        Be the first after completing the course.
                    </span>
                </asp:Panel>
            </section>
            <section class="sidebar-card recent-reviews-card">
                <h2>
                    Recent Reviews
                </h2>
                <asp:Panel ID="pnlRecentReviewsEmpty" runat="server" Visible="false" CssClass="sidebar-empty">
                    No reviews yet.
                </asp:Panel>
                <div class="recent-review-list">
                    <%-- Recent reviews --%>
                    <asp:Repeater ID="rptRecentReviews" runat="server">
                        <ItemTemplate>
                            <article class="recent-review">
                                <div class="review-stars">
                                    <%# StarText(Eval("Rating")) %>
                                </div>
                                <p>
                                    <%#: Eval("Comment") %>
                                </p>
                                <footer>
                                    <strong>
                                        <%#: Eval("LearnerName") %>
                                    </strong>
                                    <time>
                                        <%#: ReviewDate(Eval("CreatedAt")) %>
                                    </time>
                                </footer>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <a href="#course-reviews" class="sidebar-text-link js-scroll-link">
                    View All Reviews
                </a>
            </section>
            <section class="sidebar-card course-action-card">
                <asp:Button ID="btnStartCourse" runat="server" Text="Learn this Course" CssClass="btn btn-primary course-primary-action" OnClick="btnStartCourse_Click" CausesValidation="false" />
                <asp:HyperLink ID="lnkReviewAction" runat="server" NavigateUrl="#your-review" CssClass="btn btn-primary course-primary-action js-scroll-link" Visible="false" />
                <asp:Label ID="lblCompletionMessage" runat="server" Text="You have completed this course." CssClass="course-completion-message" Visible="false" />
                <asp:HyperLink ID="lnkReviewCourseContent" runat="server" Text="Review Course Content" CssClass="course-review-content-link" Visible="false" />
            </section>
        </div>
    </aside>
</asp:Panel>
