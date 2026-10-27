<%@ Page Title="Manage Course | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="ManageCoursePage.aspx.cs" Inherits="CookingLah.Pages.Chef.ManageCourse.ManageCoursePage" %>
<%@ Register Src="Overview/CourseOverview.ascx" TagPrefix="course" TagName="Overview" %>
<%@ Register Src="Recipe/CourseRecipe.ascx" TagPrefix="course" TagName="Recipe" %>
<%@ Register Src="Lessons/CourseLessons.ascx" TagPrefix="course" TagName="Lessons" %>
<%@ Register Src="Quiz/CourseQuiz.ascx" TagPrefix="course" TagName="Quiz" %>
<%@ Register Src="Activity/CourseActivity.ascx" TagPrefix="course" TagName="Activity" %>
<%@ Register Src="Review/CourseReview.ascx" TagPrefix="course" TagName="Review" %>
<asp:Content ID="Head" ContentPlaceHolderID="ChefHead" runat="server">
    <link href="ManageCoursePage.css" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Shared/CountryPicker/CountryPicker.css") %>" rel="stylesheet" />
    <link href="Overview/CourseOverview.css" rel="stylesheet" />
    <link href="Recipe/CourseRecipe.css" rel="stylesheet" />
    <link href="Lessons/CourseLessons.css" rel="stylesheet" />
    <link href="Quiz/CourseQuiz.css" rel="stylesheet" />
    <link href="Activity/CourseActivity.css" rel="stylesheet" />
    <link href="Review/CourseReview.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="ChefContent" runat="server">
<div class="manage-course course-builder" data-builder="">
    <div class="mc-backFeedback-row">
        <a href="../MyCourse/ChefMyCourse.aspx" class="mc-back">&larr; Back to My Courses</a>
        <p id="builderFeedback" role="status" aria-live="polite"><asp:Literal ID="litSaveState" runat="server" Mode="Encode" /></p>
    </div>
    <asp:Panel ID="pnlMessage" runat="server" Visible="false" ClientIDMode="Static" tabindex="-1"><asp:Literal ID="litMessage" runat="server" Mode="Encode" /></asp:Panel>
    <div class="course-header-card">
        <header class="manage-course-header">
            <div class="course-heading"><h1><asp:Literal ID="litCourseTitle" runat="server" Mode="Encode" /></h1>
                <p class="course-meta"><asp:Literal ID="litCourseMeta" runat="server" Mode="Encode" /></p>
                <p class="course-updated">Last updated: <asp:Literal ID="litUpdated" runat="server" Mode="Encode" /></p>
            </div>
            <div class="course-header-right"><asp:Label ID="lblCourseStatus" runat="server" />
                <div class="course-header-buttons">
                    <asp:Button ID="btnEditCourse" runat="server" Text="Edit Course" CssClass="btn btn-primary" OnClick="btnEditCourse_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" />
                    <asp:Button ID="btnSaveChanges" runat="server" Text="Save Changes" CssClass="btn btn-primary" OnClick="btnSaveChanges_Click" />
                    <button type="button" class="btn" id="btnPreviewLearner">Learner View</button>
                </div>
            </div>
        </header>
        <nav class="course-section-tabs" role="tablist" aria-label="Course sections">
            <button type="button" id="tab-overview" class="course-section-tab" role="tab" data-course-tab="overview" aria-controls="section-overview" aria-selected='<%= CurrentTab == "overview" ? "true" : "false" %>' tabindex='<%= CurrentTab == "overview" ? "0" : "-1" %>'>Overview</button>
            <button type="button" id="tab-recipe" class="course-section-tab" role="tab" data-course-tab="recipe" aria-controls="section-recipe" aria-selected='<%= CurrentTab == "recipe" ? "true" : "false" %>' tabindex='<%= CurrentTab == "recipe" ? "0" : "-1" %>'>Recipe</button>
            <button type="button" id="tab-lessons" class="course-section-tab" role="tab" data-course-tab="lessons" aria-controls="section-lessons" aria-selected='<%= CurrentTab == "lessons" ? "true" : "false" %>' tabindex='<%= CurrentTab == "lessons" ? "0" : "-1" %>'>Lessons</button>
            <button type="button" id="tab-quiz" class="course-section-tab" role="tab" data-course-tab="quiz" aria-controls="section-quiz" aria-selected='<%= CurrentTab == "quiz" ? "true" : "false" %>' tabindex='<%= CurrentTab == "quiz" ? "0" : "-1" %>'>Quiz</button>
            <button type="button" id="tab-activity" class="course-section-tab" role="tab" data-course-tab="activity" aria-controls="section-activity" aria-selected='<%= CurrentTab == "activity" ? "true" : "false" %>' tabindex='<%= CurrentTab == "activity" ? "0" : "-1" %>'>Activity</button>
            <button type="button" id="tab-review" class="course-section-tab" role="tab" data-course-tab="review" aria-controls="section-review" aria-selected='<%= CurrentTab == "review" ? "true" : "false" %>' tabindex='<%= CurrentTab == "review" ? "0" : "-1" %>'>Review</button>
        </nav>
    </div>
    <asp:HiddenField ID="hfActiveTab" runat="server" ClientIDMode="Static" />
    <asp:Panel ID="pnlPending" runat="server" CssClass="mc-message mc-warning"><strong>Pending Review</strong><p>This course is currently being reviewed. Editing is temporarily disabled.</p></asp:Panel>
    <asp:Panel ID="pnlRejected" runat="server" CssClass="mc-message mc-error"><h2>Admin Feedback</h2><asp:Literal ID="litRejection" runat="server" Mode="Encode" /></asp:Panel>
    <div class="manage-course-content" id="courseContent">
        <section id="section-overview" class="course-section" data-course-section="overview" role="tabpanel" aria-labelledby="tab-overview" tabindex="0" <%= CurrentTab == "overview" ? "" : "hidden" %>>
            <details class="builder-setup"><summary>Course setup <span><%= SetupPercent %>% complete</span></summary>
                <progress max="100" value="<%= SetupPercent %>" aria-label="Course setup completeness"><%= SetupPercent %>%</progress>
                <div class="builder-checklist"><asp:Repeater ID="rptSetup" runat="server"><ItemTemplate><span><%#: Eval("Marker") %> <%#: Eval("Name") %></span></ItemTemplate></asp:Repeater></div>
            </details>
            <course:Overview ID="Overview" runat="server" />
        </section>
        <section id="section-recipe" class="course-section" data-course-section="recipe" role="tabpanel" aria-labelledby="tab-recipe" tabindex="0" <%= CurrentTab == "recipe" ? "" : "hidden" %>>
            <course:Recipe ID="Recipe" runat="server" />
        </section>
        <section id="section-lessons" class="course-section" data-course-section="lessons" role="tabpanel" aria-labelledby="tab-lessons" tabindex="0" <%= CurrentTab == "lessons" ? "" : "hidden" %>>
            <course:Lessons ID="Lessons" runat="server" />
        </section>
        <section id="section-quiz" class="course-section" data-course-section="quiz" role="tabpanel" aria-labelledby="tab-quiz" tabindex="0" <%= CurrentTab == "quiz" ? "" : "hidden" %>>
            <course:Quiz ID="Quiz" runat="server" />
        </section>
        <section id="section-activity" class="course-section" data-course-section="activity" role="tabpanel" aria-labelledby="tab-activity" tabindex="0" <%= CurrentTab == "activity" ? "" : "hidden" %>>
            <course:Activity ID="Activity" runat="server" />
        </section>
        <section id="section-review" class="course-section" data-course-section="review" role="tabpanel" aria-labelledby="tab-review" tabindex="0" <%= CurrentTab == "review" ? "" : "hidden" %>>
            <course:Review ID="Review" runat="server" />
        </section>
    </div>
    <asp:Panel ID="pnlReadiness" runat="server" CssClass="card builder-readiness"><h2>Ready to Submit?</h2>
        <p><asp:Literal ID="litReadiness" runat="server" Mode="Encode" /></p>
        <asp:Button ID="btnSubmitReview" runat="server" Text="Submit for Review" CssClass="btn btn-primary" OnClick="btnSubmitReview_Click" />
    </asp:Panel>
    <asp:HiddenField ID="hfOrderKind" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfOrderIds" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfOrderParent" runat="server" ClientIDMode="Static" />
    <asp:Button ID="btnReorder" runat="server" ClientIDMode="Static" Text="Apply order" OnClick="btnReorder_Click" CssClass="builder-order-submit" />
</div>
</asp:Content>
<asp:Content ID="Scripts" ContentPlaceHolderID="ChefScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Shared/CountryPicker/CountryPicker.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/F_JavaScripts/UploadValidation.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/F_JavaScripts/IngredientEditor.js") %>" defer></script>
    <script src="ManageCoursePage.js" defer></script>
    <script src="Overview/CourseOverview.js" defer></script>
    <script src="Recipe/CourseRecipe.js" defer></script>
    <script src="Lessons/CourseLessons.js" defer></script>
    <script src="Quiz/CourseQuiz.js" defer></script>
    <script src="Activity/CourseActivity.js" defer></script>
    <script src="<%= ResolveUrl("~/F_JavaScripts/LessonVideoDialog.js") %>" defer></script>
</asp:Content>
