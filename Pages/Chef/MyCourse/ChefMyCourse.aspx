<%@ Page Title="My Courses | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="ChefMyCourse.aspx.cs" Inherits="CookingLah.Pages.Chef.MyCourse.ChefMyCourse" %>
<asp:Content ID="Head" ContentPlaceHolderID="ChefHead" runat="server"><link href="../ManageCourse/ManageCoursePage.css" rel="stylesheet" /><link href="ChefMyCourse.css" rel="stylesheet" /></asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="ChefContent" runat="server"><div class="manage-course">
    <asp:Panel ID="pnlMyCourses" runat="server">
        <header class="manage-course-header"><h1>My Courses</h1><a runat="server" href="~/Pages/Chef/CreateCourse/CreateCoursePage.aspx" class="btn btn-primary">+ Create Course</a></header>
        <p class="text-muted">Manage and update your cooking courses.</p>
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
        <div class="mc-list-summary" role="status"><h2><asp:Literal ID="litListHeading" runat="server" Mode="Encode" /></h2><p class="text-muted"><asp:Literal ID="litListDescription" runat="server" Mode="Encode" /></p></div>
        <asp:Panel ID="pnlNoCourseMatches" runat="server" CssClass="card mc-empty" Visible="false" role="status"><h2>No matching courses</h2><p>Try a different course name, clear the search, or select another status.</p></asp:Panel>
        <div class="mc-course-list"><asp:Repeater ID="rptCourses" runat="server"><ItemTemplate>
            <article class="card mc-course-card">
            <asp:Image runat="server" CssClass="mc-course-thumbnail" ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail"))) %>' Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>' AlternateText='<%# Convert.ToString(Eval("Title")) + " course thumbnail" %>' loading="lazy" />
            <asp:Panel runat="server" CssClass="mc-course-thumbnail mc-course-thumbnail-empty" Visible='<%# string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>'>No course image yet</asp:Panel>
            <h2><%#: Eval("Title") %></h2><p><%#: Eval("CategoryName") %> · <%#: Eval("DifficultyLevel") %></p><p><%#: Eval("LessonCount") %> Lessons · <%#: Eval("QuizCount") %> Quiz · <%#: Eval("ActivityCount") %> Practical Activity</p><p>Last updated: <%#: Eval("UpdatedAt", "{0:dd MMM yyyy}") %></p><span class='mc-status mc-status-<%# Convert.ToString(Eval("Status")).ToLowerInvariant() %>'><%#: Eval("Status") %></span>
            <asp:HyperLink runat="server" CssClass="btn btn-primary" Text="Manage Course" NavigateUrl='<%# "~/Pages/Chef/ManageCourse/ManageCoursePage.aspx?courseId=" + Convert.ToInt32(Eval("CourseID")) %>' /></article>
        </ItemTemplate></asp:Repeater></div>
        <asp:Panel ID="pnlCoursePager" runat="server" CssClass="mc-pager">
        <asp:Button ID="btnPreviousCourses" runat="server" CssClass="btn" Text="Previous" OnClick="btnPreviousCourses_Click" />
        <asp:Label ID="lblCoursesPage" runat="server" />
        <asp:Button ID="btnNextCourses" runat="server" CssClass="btn" Text="Next" OnClick="btnNextCourses_Click" /></asp:Panel>
    </asp:Panel>
</div></asp:Content>
<asp:Content ID="Scripts" ContentPlaceHolderID="ChefScripts" runat="server"><script src="ChefMyCourse.js" defer></script></asp:Content>