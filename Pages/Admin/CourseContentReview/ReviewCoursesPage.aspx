<%@ Page Title="Course Content Review | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ReviewCoursesPage.aspx.cs" Inherits="CookingLah.Pages.Admin.CourseContentReview.ReviewCoursesPage" %>
<%@ Register Src="~/Pages/Admin/CourseContentReview/ViewCourse/ViewCourse.ascx" TagPrefix="review" TagName="ViewCourse" %>

<%-- Page stylesheets --%>
<asp:Content ID="ReviewHead" ContentPlaceHolderID="AdminHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Admin/CourseContentReview/ReviewCoursesPage.css") %>?v=<%= System.IO.File.GetLastWriteTimeUtc(Server.MapPath("~/Pages/Admin/CourseContentReview/ReviewCoursesPage.css")).Ticks %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Admin/CourseContentReview/ViewCourse/ViewCourse.css") %>?v=<%= System.IO.File.GetLastWriteTimeUtc(Server.MapPath("~/Pages/Admin/CourseContentReview/ViewCourse/ViewCourse.css")).Ticks %>" rel="stylesheet" />
</asp:Content>

<%-- Main course review content --%>
<asp:Content ID="ReviewContent" ContentPlaceHolderID="AdminContent" runat="server">
    <div class="review-page">

        <%-- Feedback message after an action --%>
        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="review-message" role="status">
            <asp:Literal ID="litMessage" runat="server" Mode="Encode" />
        </asp:Panel>

        <%-- Course list view --%>
        <asp:Panel ID="pnlCourseList" runat="server">

            <%-- Page heading --%>
            <header class="review-page-header">
                <p class="review-eyebrow">THE ADMIN NOTEBOOK</p>
                <h1>Course Content Review</h1>
                <p>Review submitted courses before they are published.</p>
            </header>

            <%-- Course search and chef filter --%>
            <asp:Panel ID="pnlToolbar" runat="server" CssClass="review-toolbar" DefaultButton="btnSearch">
                <div class="review-search">
                    <asp:Label runat="server" AssociatedControlID="txtSearchCourses" Text="Search courses" />

                    <div class="review-search-input">
                        <asp:TextBox ID="txtSearchCourses" runat="server" MaxLength="150" placeholder="Search courses..." />
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="Search_Click" />
                    </div>
                </div>

                <div>
                    <asp:Label runat="server" AssociatedControlID="ddlChefFilter" Text="Chef" />
                    <asp:DropDownList ID="ddlChefFilter" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="Search_Click" />
                </div>
            </asp:Panel>

            <%-- Filter courses by review status --%>
            <nav class="review-status-filters" aria-label="Filter courses by status">
                <asp:LinkButton ID="btnAllCourses" runat="server" OnClick="btnAllCourses_Click" />
                <asp:LinkButton ID="btnPending" runat="server" OnClick="btnPending_Click" />
                <asp:LinkButton ID="btnApproved" runat="server" OnClick="btnApproved_Click" />
                <asp:LinkButton ID="btnRejected" runat="server" OnClick="btnRejected_Click" />
            </nav>

            <%-- Course list heading and result count --%>
            <div class="review-list-heading">
                <h2>Course List</h2>
                <asp:Literal ID="litResults" runat="server" Mode="Encode" />
            </div>

            <%-- Display each course using a repeater --%>
            <div class="review-course-list">
                <asp:Repeater ID="rptCourses" runat="server" OnItemCommand="rptCourses_ItemCommand">
                    <ItemTemplate>
                        <article class="review-course-card">

                            <%-- Course thumbnail --%>
                            <div class="review-course-thumbnail">
                                <asp:Image
                                    runat="server"
                                    ImageUrl='<%# MediaUrl(Eval("Thumbnail")) %>'
                                    Visible='<%# MediaUrl(Eval("Thumbnail")).Length > 0 %>'
                                    AlternateText='<%# Eval("Title") %>' />

                                <asp:Panel runat="server" CssClass="course-image-placeholder" Visible='<%# MediaUrl(Eval("Thumbnail")).Length == 0 %>'><img runat="server" src="~/F_Assets/mascot.png" alt="" /><span>Food Image Coming Soon</span></asp:Panel>
                            </div>

                            <%-- Course details --%>
                            <div class="review-course-info">
                                <h3><%#: Eval("Title") %></h3>
                                <p>Chef: <%#: Eval("ChefName") %></p>
                                <p><%#: Eval("CategoryName") %> &middot; <%#: Eval("DifficultyLevel") %></p>
                                <p class="review-content-counts">
                                    <%#: Eval("LessonCount") %> Lessons &middot;
                                    <%#: Eval("QuizCount") %> Quiz &middot;
                                    <%#: Eval("ActivityCount") %> Activity
                                </p>
                                <p class="review-updated">Updated: <%#: Eval("UpdatedAt", "{0:dd MMM yyyy}") %></p>
                            </div>

                            <%-- Status and view button --%>
                            <div class="review-course-actions">
                                <span class='review-badge <%# StatusClass(Eval("Status")) %>'>
                                    <%#: StatusLabel(Eval("Status")) %>
                                </span>

                                <asp:LinkButton
                                    ID="btnViewCourse"
                                    runat="server"
                                    CssClass="btn btn-primary"
                                    CommandName="ViewCourse"
                                    CommandArgument='<%# Eval("CourseID") %>'
                                    Text="View Course" />
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <%-- Message shown when no courses match --%>
            <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="review-empty">
                <h3><asp:Literal ID="litEmpty" runat="server" Mode="Encode" /></h3>
                <asp:LinkButton ID="btnClearSearch" runat="server" CssClass="btn" OnClick="ClearSearch_Click" Text="Clear Search" />
            </asp:Panel>

            <%-- Previous and next page controls --%>
            <asp:Panel ID="pnlPagination" runat="server" CssClass="review-pagination">
                <asp:LinkButton ID="btnPrevious" runat="server" CssClass="btn" Text="‹ Previous" OnClick="Previous_Click" />
                <asp:Literal ID="litPage" runat="server" />
                <asp:LinkButton ID="btnNext" runat="server" CssClass="btn" Text="Next ›" OnClick="Next_Click" />
            </asp:Panel>
        </asp:Panel>

        <%-- Selected course detail view --%>
        <asp:Panel ID="pnlViewCourse" runat="server" Visible="false">
            <review:ViewCourse ID="ViewCourse1" runat="server" />
        </asp:Panel>
    </div>
</asp:Content>

<%-- Page JavaScript files --%>
<asp:Content ID="ReviewScripts" ContentPlaceHolderID="AdminScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Admin/CourseContentReview/ReviewCoursesPage.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Admin/CourseContentReview/ViewCourse/ViewCourse.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/F_JavaScripts/LessonVideoDialog.js") %>" defer></script>
</asp:Content>
