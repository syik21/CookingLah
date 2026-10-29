<%@ Page Title="Admin Dashboard | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="CookingLah.Pages.Admin.AdminDashboard" %>
<asp:Content ID="DashboardHead" ContentPlaceHolderID="AdminHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Admin/AdminDashboard.css") %>?v=<%= System.IO.File.GetLastWriteTimeUtc(Server.MapPath("~/Pages/Admin/AdminDashboard.css")).Ticks %>" rel="stylesheet" />
</asp:Content>
<asp:Content ID="DashboardContent" ContentPlaceHolderID="AdminContent" runat="server">
<div class="admin-dashboard">
    <header class="dashboard-header">
        <p class="dashboard-kicker">The admin notebook</p><h1>Admin Dashboard</h1>
        <p class="dashboard-welcome">Welcome back, <asp:Label ID="lblAdminName" runat="server" />.</p>
        <p class="dashboard-intro">Monitor users, courses and platform activity.</p>
    </header>
    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="dashboard-message" role="alert"><asp:Literal ID="litError" runat="server" Mode="Encode" /></asp:Panel>

    <section class="summary-grid" aria-label="Platform summary">
        <a class="summary-card users" href="<%= ResolveUrl("~/Pages/Admin/ManageUser/ManageUserPage.aspx") %>"><span>Total Users</span><strong><asp:Literal ID="litTotalUsers" runat="server" /></strong><small><asp:Literal ID="litNewUsers" runat="server" /> this month</small></a>
        <a class="summary-card courses" href="<%= ResolveUrl("~/Pages/Admin/CourseContentReview/ReviewCoursesPage.aspx") %>"><span>Total Courses</span><strong><asp:Literal ID="litTotalCourses" runat="server" /></strong><small><asp:Literal ID="litPublishedCourses" runat="server" /> published</small></a>
        <a class="summary-card active" href="<%= ResolveUrl("~/Pages/Admin/UserReport/UserReportPage.aspx") %>"><span>Active Users</span><strong><asp:Literal ID="litActiveUsers" runat="server" /></strong><small>Last 7 days</small></a>
        <a id="pendingSummaryCard" runat="server" class="summary-card pending" href="~/Pages/Admin/CourseContentReview/ReviewCoursesPage.aspx?status=Pending"><span>Pending Courses</span><strong><asp:Literal ID="litPendingCourses" runat="server" /></strong><small>Awaiting review</small></a>
    </section>

    <div class="dashboard-top-grid">
        <section class="dashboard-panel overview-panel" aria-labelledby="overviewTitle">
            <div class="panel-heading"><div><p>System mix</p><h2 id="overviewTitle">Platform Overview</h2></div></div>
            <div class="overview-columns">
                <div><h3>Users</h3>
                    <div class="breakdown-row"><div><span>Learners</span><strong><asp:Literal ID="litLearners" runat="server" /></strong></div><span class="breakdown-track"><span style='<%= BarWidth(LearnerCount, TotalUserCount) %>'></span></span></div>
                    <div class="breakdown-row"><div><span>Instructors</span><strong><asp:Literal ID="litInstructors" runat="server" /></strong></div><span class="breakdown-track"><span style='<%= BarWidth(InstructorCount, TotalUserCount) %>'></span></span></div>
                    <div class="breakdown-row"><div><span>Admins</span><strong><asp:Literal ID="litAdmins" runat="server" /></strong></div><span class="breakdown-track"><span style='<%= BarWidth(AdminCount, TotalUserCount) %>'></span></span></div>
                </div>
                <div><h3>Courses</h3>
                    <div class="breakdown-row published"><div><span>Published</span><strong><asp:Literal ID="litOverviewPublished" runat="server" /></strong></div><span class="breakdown-track"><span style='<%= BarWidth(PublishedCount, TotalCourseCount) %>'></span></span></div>
                    <div class="breakdown-row draft"><div><span>Draft</span><strong><asp:Literal ID="litDraft" runat="server" /></strong></div><span class="breakdown-track"><span style='<%= BarWidth(DraftCount, TotalCourseCount) %>'></span></span></div>
                    <div class="breakdown-row pending"><div><span>Pending</span><strong><asp:Literal ID="litOverviewPending" runat="server" /></strong></div><span class="breakdown-track"><span style='<%= BarWidth(PendingCount, TotalCourseCount) %>'></span></span></div>
                    <div class="breakdown-row rejected"><div><span>Rejected</span><strong><asp:Literal ID="litRejected" runat="server" /></strong></div><span class="breakdown-track"><span style='<%= BarWidth(RejectedCount, TotalCourseCount) %>'></span></span></div>
                </div>
            </div>
        </section>
        <aside class="dashboard-panel quick-actions" aria-labelledby="actionsTitle">
            <div class="panel-heading"><div><p>Shortcuts</p><h2 id="actionsTitle">Quick Actions</h2></div></div>
            <nav aria-label="Admin quick actions">
                <a href="<%= ResolveUrl("~/Pages/Admin/ManageUser/ManageUserPage.aspx") %>"><span>Manage Users</span><small>Accounts and access</small></a>
                <a href="<%= ResolveUrl("~/Pages/Admin/CourseContentReview/ReviewCoursesPage.aspx?status=Pending") %>"><span>Review Courses</span><small>Pending submissions</small></a>
                <a href="<%= ResolveUrl("~/Pages/Admin/ManageCategories/ManagerCategoriesPage.aspx") %>"><span>Manage Categories</span><small>Course organisation</small></a>
                <a href="<%= ResolveUrl("~/Pages/Admin/UserReport/UserReportPage.aspx") %>"><span>User Report</span><small>Activity insights</small></a>
            </nav>
        </aside>
    </div>

    <section class="dashboard-panel pending-section" aria-labelledby="pendingTitle">
        <div class="panel-heading pending-heading"><div><p>Needs attention</p><h2 id="pendingTitle">Courses Waiting for Review</h2></div><asp:Label ID="lblPendingSummary" runat="server" CssClass="count-note" /></div>
        <asp:Repeater ID="rptPendingCourses" runat="server"><ItemTemplate>
            <article class="pending-course-row">
                <div class="pending-course-main"><h3><%#: Eval("Title") %></h3><p><%#: Eval("CategoryName") %> <span aria-hidden="true">&bull;</span> <%#: Eval("DifficultyLevel") %></p></div>
                <dl><div><dt>Instructor</dt><dd><%#: DisplayName(Eval("InstructorName"), Eval("Username")) %></dd></div><div><dt>Submitted</dt><dd><%#: Eval("SubmittedAt", "{0:dd MMM yyyy}") %></dd></div></dl>
                <a class="btn btn-secondary" href='<%# ReviewCourseUrl(Eval("CourseID")) %>'>Review Course</a>
            </article>
        </ItemTemplate></asp:Repeater>
        <asp:Panel ID="pnlNoPending" runat="server" Visible="false" CssClass="dashboard-empty"><strong><asp:Literal ID="litPendingEmptyTitle" runat="server" /></strong><span><asp:Literal ID="litPendingEmptyText" runat="server" /></span></asp:Panel>
        <asp:HyperLink ID="lnkAllPending" runat="server" CssClass="btn btn-secondary section-link" NavigateUrl="~/Pages/Admin/CourseContentReview/ReviewCoursesPage.aspx?status=Pending">View All Pending Courses</asp:HyperLink>
    </section>

    <div class="dashboard-bottom-grid">
        <section class="dashboard-panel activity-panel" aria-labelledby="activityTitle">
            <div class="panel-heading"><div><p>Latest logins</p><h2 id="activityTitle">Recent User Activity</h2></div></div>
            <div class="activity-list"><asp:Repeater ID="rptRecentUsers" runat="server"><ItemTemplate>
                <div class="activity-row"><div><strong><%#: DisplayName(Eval("FullName"), Eval("Username")) %></strong><span><%#: Eval("Role") %></span></div><time datetime='<%# DateTimeIso(Eval("LastLogin")) %>'><%#: RelativeLogin(Eval("LastLogin")) %></time></div>
            </ItemTemplate></asp:Repeater></div>
            <asp:Panel ID="pnlNoActivity" runat="server" Visible="false" CssClass="dashboard-empty compact"><strong>No recent user activity.</strong></asp:Panel>
            <a class="btn btn-secondary section-link" href="<%= ResolveUrl("~/Pages/Admin/UserReport/UserReportPage.aspx") %>">View User Report</a>
        </section>
        <section class="dashboard-panel performance-panel" aria-labelledby="performanceTitle">
            <div class="panel-heading"><div><p>Learning health</p><h2 id="performanceTitle">Platform Performance</h2></div></div>
            <dl class="performance-grid"><div><dt>Total Enrolments</dt><dd><asp:Literal ID="litEnrolments" runat="server" /></dd></div><div><dt>Completed Enrolments</dt><dd><asp:Literal ID="litCompleted" runat="server" /></dd></div><div><dt>Average Course Rating</dt><dd><asp:Literal ID="litAverageRating" runat="server" /></dd></div><div><dt>Total Reviews</dt><dd><asp:Literal ID="litReviews" runat="server" /></dd></div></dl>
            <asp:Label ID="lblPerformanceEmpty" runat="server" CssClass="performance-note" />
        </section>
    </div>
</div>
</asp:Content>
<asp:Content ID="DashboardScripts" ContentPlaceHolderID="AdminScripts" runat="server"><script src="<%= ResolveUrl("~/Pages/Admin/AdminDashboard.js") %>" defer></script></asp:Content>
