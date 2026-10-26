<%@ Page Title="Manage Course | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="ManageCoursePage.aspx.cs" Inherits="CookingLah.Pages.Chef.ManageCourse.ManageCoursePage" %>
<%@ Register Src="Overview/CourseOverview.ascx" TagPrefix="course" TagName="Overview" %>
<%@ Register Src="Recipe/CourseRecipe.ascx" TagPrefix="course" TagName="Recipe" %>
<%@ Register Src="Lessons/CourseLessons.ascx" TagPrefix="course" TagName="Lessons" %>
<%@ Register Src="Quiz/CourseQuiz.ascx" TagPrefix="course" TagName="Quiz" %>
<%@ Register Src="Activity/CourseActivity.ascx" TagPrefix="course" TagName="Activity" %>
<%@ Register Src="Preview/CoursePreview.ascx" TagPrefix="course" TagName="Preview" %>
<asp:Content ID="Head" ContentPlaceHolderID="ChefHead" runat="server">
    <link href="ManageCoursePage.css" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Shared/CountryPicker/CountryPicker.css") %>" rel="stylesheet" />
    <link href="Overview/CourseOverview.css" rel="stylesheet" />
    <link href="Recipe/CourseRecipe.css" rel="stylesheet" />
    <link href="Lessons/CourseLessons.css" rel="stylesheet" />
    <link href="Quiz/CourseQuiz.css" rel="stylesheet" />
    <link href="Activity/CourseActivity.css" rel="stylesheet" />
    <link href="Preview/CoursePreview.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="ChefContent" runat="server">
<div class="manage-course">
    <dialog class="lesson-video-dialog" data-lesson-video-dialog="" aria-labelledby="chefVideoTitle" aria-describedby="chefVideoStatus">
        <div class="lesson-video-heading"><h2 id="chefVideoTitle">Lesson video</h2><button type="button" class="btn" data-video-close="" autofocus>Close video</button></div>
        <video controls playsinline preload="none" aria-label="Lesson video player"></video>
        <p id="chefVideoStatus" class="lesson-video-status" role="status"></p>
    </dialog>
    <asp:Panel ID="pnlMessage" runat="server" Visible="false" ClientIDMode="Static" tabindex="-1"><asp:Literal ID="litMessage" runat="server" Mode="Encode" /></asp:Panel>
    <asp:Panel ID="pnlMyCourses" runat="server">
        <header class="manage-course-header"><h1>My Courses</h1><a runat="server" href="~/Pages/Chef/CreateCourse/CreateCoursePage.aspx" class="btn btn-primary">+ Create Course</a></header>
        <p class="text-muted">Choose a course to manage its content.</p>
        <asp:Panel ID="pnlNoCourses" runat="server" CssClass="card mc-empty"><h2>Your teaching kitchen starts here.</h2><p>No courses yet. Create your first cooking course.</p></asp:Panel>
        <asp:Panel ID="pnlCourseSearch" runat="server" CssClass="mc-course-search" DefaultButton="btnSearchCourses" role="search" aria-label="Search your courses">
            <div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtCourseSearch" Text="Search by course name" /><asp:TextBox ID="txtCourseSearch" runat="server" MaxLength="150" placeholder="Enter a course name" /></div>
            <asp:Button ID="btnSearchCourses" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearchCourses_Click" CausesValidation="false" />
            <asp:Button ID="btnClearCourseSearch" runat="server" Text="Clear" CssClass="btn" OnClick="btnClearCourseSearch_Click" CausesValidation="false" />
        </asp:Panel>
        <div class="mc-status-filters" role="group" aria-label="Filter courses by status">
            <asp:Button ID="btnStatusAll" runat="server" Text="All courses" CssClass="mc-status-filter" CommandArgument="All" OnCommand="CourseStatus_Command" CausesValidation="false" />
            <asp:Button ID="btnStatusDraft" runat="server" Text="Draft" CssClass="mc-status-filter" CommandArgument="Draft" OnCommand="CourseStatus_Command" CausesValidation="false" />
            <asp:Button ID="btnStatusPending" runat="server" Text="Pending review" CssClass="mc-status-filter" CommandArgument="Pending" OnCommand="CourseStatus_Command" CausesValidation="false" />
            <asp:Button ID="btnStatusPublished" runat="server" Text="Published" CssClass="mc-status-filter" CommandArgument="Published" OnCommand="CourseStatus_Command" CausesValidation="false" />
            <asp:Button ID="btnStatusRejected" runat="server" Text="Rejected" CssClass="mc-status-filter" CommandArgument="Rejected" OnCommand="CourseStatus_Command" CausesValidation="false" />
        </div>
        <div class="mc-list-summary" role="status"><h2><asp:Literal ID="litListHeading" runat="server" Mode="Encode" /></h2><p class="text-muted"><asp:Literal ID="litListDescription" runat="server" Mode="Encode" /></p></div>
        <asp:Panel ID="pnlNoCourseMatches" runat="server" CssClass="card mc-empty" Visible="false" role="status"><h2>No matching courses</h2><p>Try a different course name, clear the search, or select another status.</p></asp:Panel>
        <div class="mc-course-list"><asp:Repeater ID="rptCourses" runat="server"><ItemTemplate>
            <article class="card mc-course-card">
            <asp:Image runat="server" CssClass="mc-course-thumbnail" ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail"))) %>' Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>' AlternateText='<%# Convert.ToString(Eval("Title")) + " course thumbnail" %>' loading="lazy" />
            <asp:Panel runat="server" CssClass="mc-course-thumbnail mc-course-thumbnail-empty" Visible='<%# string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>'>No course image yet</asp:Panel>
            <h2><%#: Eval("Title") %></h2><p><%#: Eval("CategoryName") %></p><span class='mc-status mc-status-<%# Convert.ToString(Eval("Status")).ToLowerInvariant() %>'><%#: Eval("Status") %></span>
            <asp:HyperLink runat="server" CssClass="btn btn-primary" Text="Manage Course" NavigateUrl='<%# "ManageCoursePage.aspx?courseId=" + Eval("CourseID") %>' /></article>
        </ItemTemplate></asp:Repeater></div>
        <asp:Panel ID="pnlCoursePager" runat="server" CssClass="mc-pager">
        <asp:Button ID="btnPreviousCourses" runat="server" CssClass="btn" Text="Previous" OnClick="btnPreviousCourses_Click" />
        <asp:Label ID="lblCoursesPage" runat="server" />
        <asp:Button ID="btnNextCourses" runat="server" CssClass="btn" Text="Next" OnClick="btnNextCourses_Click" /></asp:Panel>
    </asp:Panel>
    <asp:Panel ID="pnlWorkspace" runat="server">
        <a href="ManageCoursePage.aspx" class="mc-back">&larr; Back to My Courses</a>
        <header class="manage-course-header">
            <div><h1><asp:Literal ID="litCourseTitle" runat="server" Mode="Encode" /></h1><p><asp:Literal ID="litCourseMeta" runat="server" Mode="Encode" /></p><asp:Label ID="lblCourseStatus" runat="server" /></div>
            <div class="mc-header-actions" role="group" aria-label="Course actions">
                <div class="mc-header-main-actions">
                    <a href="<%= TabUrl("preview") %>" class="btn mc-preview-action" aria-label="Preview learner view">Preview Course</a>
                    <asp:Button ID="btnSubmitReview" runat="server" Text="Submit for Review" CssClass="btn btn-primary" OnClick="btnSubmitReview_Click" />
                </div>
                <asp:Button ID="btnDeleteCourse" runat="server" Text="Delete Course" CssClass="mc-link mc-danger-link mc-delete-action" OnClick="btnDeleteCourse_Click" />
            </div>
        </header>
        <dialog id="publishedChangesDialog" class="review-reminder-dialog"
            aria-labelledby="publishedChangesTitle" aria-describedby="publishedChangesDescription">
            <h2 id="publishedChangesTitle">Changes not submitted for review</h2>
            <p id="publishedChangesDescription">Your edits are held in this session and have not changed the course in the database. Submit them for review now, or leave without submitting.</p>
            <div class="review-reminder-actions">
                <button type="button" class="btn" data-review-stay autofocus>Continue editing</button>
                <input type="hidden" id="reviewLeaveDestination" name="reviewLeaveDestination" value="" />
                <button type="submit" class="btn" data-review-leave name="discardReview" value="1" formnovalidate>Leave without submitting</button>
                <button type="button" class="btn btn-primary" data-review-submit>Submit for Review</button>
            </div>
        </dialog>
        <asp:Panel ID="pnlPending" runat="server" CssClass="mc-message mc-warning">This course is currently under review. Editing is temporarily disabled until the review is completed.</asp:Panel>
        <nav class="course-tabs" aria-label="Course sections">
            <a class="<%= TabClass("overview") %>" href="<%= TabUrl("overview") %>" aria-current="<%= TabCurrent("overview") %>">Overview</a>
            <a class="<%= TabClass("recipe") %>" href="<%= TabUrl("recipe") %>" aria-current="<%= TabCurrent("recipe") %>">Recipe</a>
            <a class="<%= TabClass("lessons") %>" href="<%= TabUrl("lessons") %>" aria-current="<%= TabCurrent("lessons") %>">Lessons</a>
            <a class="<%= TabClass("quiz") %>" href="<%= TabUrl("quiz") %>" aria-current="<%= TabCurrent("quiz") %>">Quiz</a>
            <a class="<%= TabClass("activity") %>" href="<%= TabUrl("activity") %>" aria-current="<%= TabCurrent("activity") %>">Practical Activity</a>
        </nav>
        <asp:Panel ID="pnlDeleteCourse" runat="server" Visible="false" CssClass="card mc-modal mc-delete" data-title="Delete Course?">
            <h2>Delete Course?</h2><p>You are about to permanently delete <strong><asp:Literal ID="litDeleteCourseName" runat="server" Mode="Encode" /></strong>.</p>
            <p>The following data will also be permanently deleted:</p>
            <ul><li>Recipe and lessons</li><li>Quiz, questions, answer options and learner quiz attempts</li><li>Practical activities and learner submissions</li><li>Learner course progress</li><li>Reviews and course reports</li></ul>
            <p><strong>This action cannot be undone.</strong></p>
            <div class="mc-actions">
            <asp:Button ID="btnCancelDeleteCourse" runat="server" CssClass="btn" Text="Cancel" OnClick="btnCancelDeleteCourse_Click" data-cancel="" />
            <asp:Button ID="btnConfirmDeleteCourse" runat="server" CssClass="btn mc-danger" Text="Delete Course Permanently" OnClick="btnConfirmDeleteCourse_Click" /></div>
        </asp:Panel>
        <div class="manage-course-content">
            <course:Overview ID="Overview" runat="server" />
            <course:Recipe ID="Recipe" runat="server" />
            <course:Lessons ID="Lessons" runat="server" />
            <course:Quiz ID="Quiz" runat="server" />
            <course:Activity ID="Activity" runat="server" />
            <course:Preview ID="Preview" runat="server" />
        </div>
    </asp:Panel>
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
