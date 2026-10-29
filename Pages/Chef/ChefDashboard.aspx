<%@ Page Title="Chef Dashboard | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="ChefDashboard.aspx.cs" Inherits="CookingLah.Pages.Chef.ChefDashboard" %>
<%@ Import Namespace="System" %>
<asp:Content ID="DashboardHead" ContentPlaceHolderID="ChefHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Chef/ChefDashboard.css") %>" rel="stylesheet" />
</asp:Content>
<asp:Content ID="DashboardContent" ContentPlaceHolderID="ChefContent" runat="server">
    <style>
        .chef-status-track {
            width: 100%;
            height: 10px;
            background: var(--color-tan-light);
            border-radius: 999px;
            overflow: hidden;
        }

        .chef-status-fill {
            display: block;
            height: 100%;
            background: var(--color-primary);
            border-radius: inherit;
        }
    </style>
<main class="chef-dashboard" aria-labelledby="chefDashboardTitle">
    <header class="chef-dashboard-welcome">
        <h1 id="chefDashboardTitle">Chef Dashboard</h1>
        <p class="chef-dashboard-greeting">Welcome back, Chef <asp:Literal ID="litChefName" runat="server" Mode="Encode" />.</p>
        <p>Manage your courses and keep up with learner activity.</p>
    </header>

    <section class="chef-summary-grid" aria-label="Teaching summary">
        <a class="chef-summary-card is-courses" href="<%= ResolveUrl("~/Pages/Chef/MyCourse/ChefMyCourse.aspx") %>"><span>Total Courses</span><strong><asp:Literal ID="litTotalCourses" runat="server" Mode="Encode" /></strong><small>View your courses</small></a>
        <a class="chef-summary-card is-published" href="<%= ResolveUrl("~/Pages/Chef/MyCourse/ChefMyCourse.aspx") %>"><span>Published Courses</span><strong><asp:Literal ID="litPublishedCourses" runat="server" Mode="Encode" /></strong><small>Available to learners</small></a>
        <a class="chef-summary-card is-submissions" href="<%= ResolveUrl("~/Pages/Chef/ChefReviewSubmission/ChefReviewSubmissionPage.aspx") %>"><span>Submitted Activities</span><strong><asp:Literal ID="litSubmittedActivities" runat="server" Mode="Encode" /></strong><small>Waiting for review</small></a>
        <a class="chef-summary-card is-rating" href="<%= ResolveUrl("~/Pages/Chef/ChefViewReview/ChefViewReviewPage.aspx") %>"><span>Average Rating</span><strong><asp:Literal ID="litAverageRating" runat="server" Mode="Encode" /></strong><small><asp:Literal ID="litRatingReviewCount" runat="server" Mode="Encode" /></small></a>
    </section>

    <asp:Panel ID="pnlNoCourses" runat="server" Visible="false" CssClass="chef-dashboard-empty chef-dashboard-start">
        <div><h2>You haven&rsquo;t created any courses yet.</h2><p>Create your first course and start sharing your cooking skills.</p></div>
        <a class="btn btn-primary" href="<%= ResolveUrl("~/Pages/Chef/CreateCourse/CreateCoursePage.aspx") %>">Create Course</a>
    </asp:Panel>

    <div class="chef-dashboard-overview">
        <section class="chef-dashboard-panel" aria-labelledby="courseStatusTitle">
            <div class="chef-section-heading"><div><h2 id="courseStatusTitle">Course Status</h2><p>Your courses at a glance</p></div></div>
            <div class="chef-status-list"><asp:Repeater ID="rptCourseStatus" runat="server"><ItemTemplate>
                <div class='<%# StatusRowClass(Eval("Status"), Eval("Count")) %>'>
                    <div class="chef-status-label"><span><%#: Eval("Status") %></span><small runat="server" visible='<%# Convert.ToString(Eval("Status")) == "Rejected" && Convert.ToInt32(Eval("Count")) > 0 %>'>Needs attention</small></div>
                    <div class="chef-status-track" aria-hidden="true">
                        <asp:Panel
                            runat="server"
                            CssClass="chef-status-fill"
                            Width='<%# System.Web.UI.WebControls.Unit.Percentage(Convert.ToDouble(Eval("Percent"))) %>' />
                    </div>

                    <strong><%#: Eval("Count") %></strong>
                </div>
            </ItemTemplate></asp:Repeater></div>
        </section>
        <section class="chef-dashboard-panel chef-quick-actions" aria-labelledby="quickActionsTitle">
            <div class="chef-section-heading"><div><h2 id="quickActionsTitle">Quick Actions</h2><p>Common teaching tasks</p></div></div>
            <nav aria-label="Dashboard quick actions">
                <a class="btn btn-primary" href="<%= ResolveUrl("~/Pages/Chef/CreateCourse/CreateCoursePage.aspx") %>">+ Create Course</a>
                <a class="btn" href="<%= ResolveUrl("~/Pages/Chef/MyCourse/ChefMyCourse.aspx") %>">View My Courses</a>
                <a class="btn" href="<%= ResolveUrl("~/Pages/Chef/ChefReviewSubmission/ChefReviewSubmissionPage.aspx") %>">Review Submissions</a>
            </nav>
        </section>
    </div>

    <section class="chef-dashboard-panel chef-dashboard-list-section" aria-labelledby="activitySubmissionsTitle">
        <div class="chef-section-heading"><div><h2 id="activitySubmissionsTitle">Activity Submissions</h2><p><asp:Literal ID="litSubmissionSummary" runat="server" Mode="Encode" /></p></div><a href="<%= ResolveUrl("~/Pages/Chef/ChefReviewSubmission/ChefReviewSubmissionPage.aspx") %>">View all submissions</a></div>
        <asp:Panel ID="pnlSubmissions" runat="server" CssClass="chef-dashboard-rows"><asp:Repeater ID="rptSubmissions" runat="server"><ItemTemplate>
            <article class="chef-dashboard-row submission-row"><div><h3><%#: Eval("LearnerName") %></h3><p class="chef-row-course"><%#: Eval("CourseTitle") %></p><p><%#: Eval("ActivityTitle") %></p></div><div class="chef-row-action"><time datetime='<%# DateValue(Eval("SubmittedAt")) %>'>Submitted <%#: DisplayDate(Eval("SubmittedAt")) %></time><asp:HyperLink runat="server" CssClass="btn btn-secondary" Text="Review" NavigateUrl='<%# SubmissionUrl(Eval("SubmissionID")) %>' /></div></article>
        </ItemTemplate></asp:Repeater></asp:Panel>
        <asp:Panel ID="pnlNoSubmissions" runat="server" Visible="false" CssClass="chef-dashboard-empty"><p>No activity submissions are waiting for review.</p></asp:Panel>
    </section>

    <section class="chef-dashboard-panel chef-dashboard-list-section" aria-labelledby="recentReviewsTitle">
        <div class="chef-section-heading"><div><h2 id="recentReviewsTitle">Recent Course Reviews</h2><p>Latest learner feedback</p></div><a href="<%= ResolveUrl("~/Pages/Chef/ChefViewReview/ChefViewReviewPage.aspx") %>">View all course reviews</a></div>
        <asp:Panel ID="pnlReviews" runat="server" CssClass="chef-dashboard-rows"><asp:Repeater ID="rptReviews" runat="server"><ItemTemplate>
            <article class="chef-dashboard-row review-row"><div class="review-row-heading"><div><h3><%#: Eval("CourseTitle") %></h3><p>Reviewed by <%#: Eval("LearnerName") %></p></div><p class="dashboard-rating" aria-label='<%# Eval("Rating") + " out of 5 stars" %>'><span aria-hidden="true"><%# StarText(Eval("Rating")) %></span> <%#: Eval("Rating") %> / 5</p></div><p class="dashboard-review-comment">&ldquo;<%#: Eval("Comment") %>&rdquo;</p><time datetime='<%# DateValue(Eval("CreatedAt")) %>'>Reviewed <%#: DisplayDate(Eval("CreatedAt")) %></time></article>
        </ItemTemplate></asp:Repeater></asp:Panel>
        <asp:Panel ID="pnlNoReviews" runat="server" Visible="false" CssClass="chef-dashboard-empty"><p>No course reviews yet.</p></asp:Panel>
    </section>
</main>
</asp:Content>
<asp:Content ID="DashboardScripts" ContentPlaceHolderID="ChefScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Chef/ChefDashboard.js") %>" defer></script>
</asp:Content>
