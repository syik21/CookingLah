<%@ Page Title="My Courses | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="ChefMyCourse.aspx.cs" Inherits="CookingLah.Pages.Chef.MyCourse.ChefMyCourse" %>
<asp:Content ID="Head" ContentPlaceHolderID="ChefHead" runat="server"><link href="../ManageCourse/ManageCoursePage.css" rel="stylesheet" /><link href="ChefMyCourse.css" rel="stylesheet" /></asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="ChefContent" runat="server"><div class="manage-course">
    <asp:Panel ID="pnlMyCourses" runat="server">
        <header class="manage-course-header mc-page-header"><div><p class="mc-page-eyebrow">YOUR TEACHING KITCHEN</p><h1>My Courses</h1><p class="text-muted">Manage, update, and track every cooking course.</p></div><a runat="server" href="~/Pages/Chef/CreateCourse/CreateCoursePage.aspx" class="btn btn-primary">+ Create Course</a></header>
        <asp:Panel ID="pnlNoCourses" runat="server" CssClass="card mc-empty"><h2>Your teaching kitchen starts here.</h2><p>No courses yet. Create your first cooking course.</p></asp:Panel>
        <asp:Panel ID="pnlCourseSearch" runat="server" CssClass="mc-course-search" DefaultButton="btnSearchCourses" role="search" aria-label="Search your courses">
            <div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtCourseSearch" Text="Search by course name" /><asp:TextBox ID="txtCourseSearch" runat="server" MaxLength="150" placeholder="Enter a course name" /></div>
            <asp:DropDownList ID="ddlCategory" runat="server" aria-label="Filter by category" AutoPostBack="true" OnSelectedIndexChanged="btnSearchCourses_Click" />
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
        <div class="mc-course-toolbar">
            <div class="mc-list-summary" role="status"><h2><asp:Literal ID="litListHeading" runat="server" Mode="Encode" /></h2><p class="text-muted"><asp:Literal ID="litListDescription" runat="server" Mode="Encode" /></p></div>
            <div class="mc-view-toggle" role="group" aria-label="Course display">
                <button type="button" class="mc-view-button is-selected" data-course-view="cards" aria-pressed="true">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>
                    Cards
                </button>
                <button type="button" class="mc-view-button" data-course-view="list" aria-pressed="false">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M9 6h12M9 12h12M9 18h12"/><circle cx="4" cy="6" r="1"/><circle cx="4" cy="12" r="1"/><circle cx="4" cy="18" r="1"/></svg>
                    List
                </button>
            </div>
        </div>
        <asp:Panel ID="pnlNoCourseMatches" runat="server" CssClass="card mc-empty" Visible="false" role="status"><h2>No matching courses</h2><p>Try a different course name, clear the search, or select another status.</p></asp:Panel>
        <div class="mc-course-list" data-course-list><asp:Repeater ID="rptCourses" runat="server"><ItemTemplate>
            <article class="card mc-course-card">
            <asp:Image runat="server" CssClass="mc-course-thumbnail" ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail"))) %>' Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>' AlternateText='<%# Convert.ToString(Eval("Title")) + " course thumbnail" %>' loading="lazy" />
            <asp:Panel runat="server" CssClass="mc-course-thumbnail mc-course-thumbnail-empty course-image-placeholder" Visible='<%# string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>'><img runat="server" src="~/F_Assets/mascot.png" alt="" /><span>Food Image Coming Soon</span></asp:Panel>
            <div class="mc-course-card-body"><div class="mc-course-card-heading"><div><p class="mc-course-category"><%#: Eval("CategoryName") %> · <%#: Eval("DifficultyLevel") %></p><h2 title='<%#: Eval("Title") %>'><%#: Eval("Title") %></h2></div><span class='mc-status mc-status-<%# Convert.ToString(Eval("Status")).ToLowerInvariant() %>'><%#: Eval("Status") %></span></div><p class="mc-course-description"><%#: Eval("Description") %></p><p class="mc-course-counts"><%#: Eval("LessonCount") %> Lessons · <%#: Eval("QuizCount") %> Quiz · <%#: Eval("ActivityCount") %> Practical Activity</p><div class="mc-course-card-footer"><p>Updated <%#: Eval("UpdatedAt", "{0:dd MMM yyyy}") %></p><asp:HyperLink runat="server" CssClass="btn btn-primary" Text="Manage Course" NavigateUrl='<%# "~/Pages/Chef/ManageCourse/ManageCoursePage.aspx?courseId=" + Convert.ToInt32(Eval("CourseID")) %>' /></div></div></article>
        </ItemTemplate></asp:Repeater></div>
        <asp:Panel ID="pnlCoursePager" runat="server" CssClass="mc-pager">
        <asp:Button ID="btnPreviousCourses" runat="server" CssClass="btn" Text="Previous" OnClick="btnPreviousCourses_Click" />
        <asp:Label ID="lblCoursesPage" runat="server" />
        <asp:Button ID="btnNextCourses" runat="server" CssClass="btn" Text="Next" OnClick="btnNextCourses_Click" /></asp:Panel>
    </asp:Panel>
</div></asp:Content>
<asp:Content ID="Scripts" ContentPlaceHolderID="ChefScripts" runat="server"><script src="ChefMyCourse.js" defer></script></asp:Content>
