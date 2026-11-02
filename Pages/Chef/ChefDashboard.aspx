<%@ Page Title="Chef Dashboard | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="ChefDashboard.aspx.cs" Inherits="CookingLah.Pages.Chef.ChefDashboard" %>
<%@ Import Namespace="System" %>
<asp:Content
    ID="DashboardHead"
    ContentPlaceHolderID="ChefHead"
    runat="server">
    <link href="<%= ResolveUrl("~/Pages/Chef/ChefDashboard.css") %>" rel="stylesheet" />
</asp:Content>
<asp:Content
    ID="DashboardContent"
    ContentPlaceHolderID="ChefContent"
    runat="server">
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

        <svg
            class="chef-ink-filters"
            xmlns="http://www.w3.org/2000/svg"
            width="0"
            height="0"
            aria-hidden="true"
            focusable="false">
            <defs>
                <filter
                    id="chefRoughInk"
                    x="-10%"
                    y="-10%"
                    width="120%"
                    height="120%"
                    color-interpolation-filters="sRGB">
                    <feTurbulence
                        type="fractalNoise"
                        baseFrequency="0.035"
                        numOctaves="3"
                        seed="8"
                        result="paperNoise" />
                    <feDisplacementMap
                        in="SourceGraphic"
                        in2="paperNoise"
                        scale="4"
                        xChannelSelector="R"
                        yChannelSelector="G" />
                </filter>
            </defs>
        </svg>

        <header class="chef-dashboard-welcome">
            <h1 id="chefDashboardTitle">Chef Dashboard</h1>
            <p class="chef-dashboard-greeting">Welcome back, Chef <asp:Literal
                    ID="litChefName"
                    runat="server"
                    Mode="Encode" />.</p>
            <p>Manage your courses and keep up with learner activity.</p>
        </header>

        <section class="chef-summary-grid" aria-label="Teaching summary">
            <a class="chef-stat" href="<%= ResolveUrl("~/Pages/Chef/MyCourse/ChefMyCourse.aspx") %>">
                <span class="chef-stat-tab"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><path d="M12 5v16M12 5C8 2 4 3 2 4v15c4-1 7-1 10 2 3-3 6-3 10-2V4c-2-1-6-2-10 1Z" /></svg> Courses</span>
                <div class="chef-stat-body">
                    <div class="chef-stat-sheet">
                        <strong class="chef-stat-value"><asp:Literal
                                ID="litTotalCourses"
                                runat="server"
                                Mode="Encode" /></strong>
                        <span class="chef-stat-label">Total Courses</span>
                        <small class="chef-stat-hint">View your courses</small>
                    </div>
                </div>
            </a>
            <a class="chef-stat" href="<%= ResolveUrl("~/Pages/Chef/MyCourse/ChefMyCourse.aspx") %>">
                <span class="chef-stat-tab"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><circle cx="12" cy="12" r="9" /><path d="m8 12 3 3 5-6" /></svg> Published</span>
                <div class="chef-stat-body">
                    <div class="chef-stat-sheet">
                        <strong class="chef-stat-value"><asp:Literal
                                ID="litPublishedCourses"
                                runat="server"
                                Mode="Encode" /></strong>
                        <span class="chef-stat-label">Published Courses</span>
                        <small class="chef-stat-hint">Available to learners</small>
                    </div>
                </div>
            </a>
            <a class="chef-stat" href="<%= ResolveUrl("~/Pages/Chef/ChefReviewSubmission/ChefReviewSubmissionPage.aspx") %>">
                <span class="chef-stat-tab"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><path d="M3 6h4l2-3h6l2 3h4v15H3Z" /><circle cx="12" cy="13" r="4" /></svg> Submissions</span>
                <div class="chef-stat-body">
                    <div class="chef-stat-sheet">
                        <strong class="chef-stat-value"><asp:Literal
                                ID="litSubmittedActivities"
                                runat="server"
                                Mode="Encode" /></strong>
                        <span class="chef-stat-label">Submitted Activities</span>
                        <small class="chef-stat-hint">Waiting for review</small>
                    </div>
                </div>
            </a>
            <a class="chef-stat" href="<%= ResolveUrl("~/Pages/Chef/ChefViewReview/ChefViewReviewPage.aspx") %>">
                <span class="chef-stat-tab"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><path d="m12 3 3 6 7 1-5 5 1 7-6-3-6 3 1-7-5-5 7-1Z" /></svg> Rating</span>
                <div class="chef-stat-body">
                    <div class="chef-stat-sheet">
                        <strong class="chef-stat-value"><asp:Literal
                                ID="litAverageRating"
                                runat="server"
                                Mode="Encode" /></strong>
                        <span class="chef-stat-label">Average Rating</span>
                        <small class="chef-stat-hint"><asp:Literal
                                ID="litRatingReviewCount"
                                runat="server"
                                Mode="Encode" /></small>
                    </div>
                </div>
            </a>
        </section>

        <asp:Panel
            ID="pnlNoCourses"
            runat="server"
            Visible="false"
            CssClass="chef-dashboard-empty chef-dashboard-start">
            <div>
                <h2><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><path d="M12 5v16M12 5C8 2 4 3 2 4v15c4-1 7-1 10 2 3-3 6-3 10-2V4c-2-1-6-2-10 1Z" /></svg> You haven&rsquo;t created any courses yet.</h2>
                <p>Create your first course and start sharing your cooking skills.</p>
            </div>
            <a class="btn btn-primary" href="<%= ResolveUrl("~/Pages/Chef/CreateCourse/CreateCoursePage.aspx") %>">Create Course</a>
        </asp:Panel>

        <div class="chef-dashboard-overview">
            <section class="chef-dashboard-panel" aria-labelledby="courseStatusTitle">
                <div class="chef-section-heading">
                    <div>
                        <h2 id="courseStatusTitle"><svg
                                class="dashboard-title-icon"
                                viewBox="0 0 24 24"
                                aria-hidden="true"
                                focusable="false"><path d="M4 3v18h17M8 16v-4m5 4V8m5 8V4" /></svg> Course Status</h2>
                        <p>Your courses at a glance</p>
                    </div>
                </div>
                <div class="chef-status-list">
                    <asp:Repeater ID="rptCourseStatus" runat="server">
                        <ItemTemplate>
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
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>
            <section class="chef-dashboard-panel chef-quick-actions" aria-labelledby="quickActionsTitle">
                <div class="chef-section-heading">
                    <div>
                        <h2 id="quickActionsTitle"><svg
                                class="dashboard-title-icon"
                                viewBox="0 0 24 24"
                                aria-hidden="true"
                                focusable="false"><path d="m13 2-9 12h7l-1 8 10-13h-7Z" /></svg> Quick Actions</h2>
                        <p>Common teaching tasks</p>
                    </div>
                </div>
                <nav aria-label="Dashboard quick actions">
                    <a class="btn btn-primary" href="<%= ResolveUrl("~/Pages/Chef/CreateCourse/CreateCoursePage.aspx") %>">+ Create Course</a>
                    <a class="btn" href="<%= ResolveUrl("~/Pages/Chef/MyCourse/ChefMyCourse.aspx") %>">View My Courses</a>
                    <a class="btn" href="<%= ResolveUrl("~/Pages/Chef/ChefReviewSubmission/ChefReviewSubmissionPage.aspx") %>">Review Submissions</a>
                </nav>
            </section>
        </div>

        <section class="chef-dashboard-panel chef-dashboard-list-section" aria-labelledby="activitySubmissionsTitle">
            <div class="chef-section-heading">
                <div>
                    <h2 id="activitySubmissionsTitle"><svg
                            class="dashboard-title-icon"
                            viewBox="0 0 24 24"
                            aria-hidden="true"
                            focusable="false"><path d="M3 6h4l2-3h6l2 3h4v15H3Z" /><circle cx="12" cy="13" r="4" /></svg> Activity Submissions</h2>
                    <p><asp:Literal
                            ID="litSubmissionSummary"
                            runat="server"
                            Mode="Encode" /></p>
                </div><a href="<%= ResolveUrl("~/Pages/Chef/ChefReviewSubmission/ChefReviewSubmissionPage.aspx") %>">View all submissions</a></div>
            <asp:Panel
                ID="pnlSubmissions"
                runat="server"
                CssClass="chef-dashboard-rows">
                <asp:Repeater ID="rptSubmissions" runat="server">
                    <ItemTemplate>
                        <article class="chef-dashboard-row submission-row">
                            <div>
                                <h3><svg
                                        class="dashboard-title-icon"
                                        viewBox="0 0 24 24"
                                        aria-hidden="true"
                                        focusable="false"><circle cx="9" cy="7" r="3" /><path d="M3 21v-3a6 6 0 0 1 12 0v3M16 4a3 3 0 0 1 0 6m2 4a5 5 0 0 1 3 4v3" /></svg> <%#: Eval("LearnerName") %></h3>
                                <p class="chef-row-course"><%#: Eval("CourseTitle") %></p>
                                <p><%#: Eval("ActivityTitle") %></p>
                            </div>
                            <div class="chef-row-action"><time datetime='<%# DateValue(Eval("SubmittedAt")) %>'>Submitted <%#: DisplayDate(Eval("SubmittedAt")) %></time><asp:HyperLink
                                    runat="server"
                                    CssClass="btn btn-secondary"
                                    Text="Review"
                                    NavigateUrl='<%# SubmissionUrl(Eval("SubmissionID")) %>' /></div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </asp:Panel>
            <asp:Panel
                ID="pnlNoSubmissions"
                runat="server"
                Visible="false"
                CssClass="chef-dashboard-empty">
                <p>No activity submissions are waiting for review.</p>
            </asp:Panel>
        </section>

        <section class="chef-dashboard-panel chef-dashboard-list-section" aria-labelledby="recentReviewsTitle">
            <div class="chef-section-heading">
                <div>
                    <h2 id="recentReviewsTitle"><svg
                            class="dashboard-title-icon"
                            viewBox="0 0 24 24"
                            aria-hidden="true"
                            focusable="false"><path d="m12 3 3 6 7 1-5 5 1 7-6-3-6 3 1-7-5-5 7-1Z" /></svg> Recent Course Reviews</h2>
                    <p>Latest learner feedback</p>
                </div><a href="<%= ResolveUrl("~/Pages/Chef/ChefViewReview/ChefViewReviewPage.aspx") %>">View all course reviews</a></div>
            <asp:Panel
                ID="pnlReviews"
                runat="server"
                CssClass="chef-dashboard-rows">
                <asp:Repeater ID="rptReviews" runat="server">
                    <ItemTemplate>
                        <article class="chef-dashboard-row review-row">
                            <div class="review-row-heading">
                                <div>
                                    <h3><svg
                                            class="dashboard-title-icon"
                                            viewBox="0 0 24 24"
                                            aria-hidden="true"
                                            focusable="false"><path d="M12 5v16M12 5C8 2 4 3 2 4v15c4-1 7-1 10 2 3-3 6-3 10-2V4c-2-1-6-2-10 1Z" /></svg> <%#: Eval("CourseTitle") %></h3>
                                    <p>Reviewed by <%#: Eval("LearnerName") %></p>
                                </div>
                                <p class="dashboard-rating" aria-label='<%# Eval("Rating") + " out of 5 stars" %>'><span aria-hidden="true"><%# StarText(Eval("Rating")) %></span> <%#: Eval("Rating") %> / 5</p>
                            </div>
                            <p class="dashboard-review-comment">&ldquo;<%#: Eval("Comment") %>&rdquo;</p><time datetime='<%# DateValue(Eval("CreatedAt")) %>'>Reviewed <%#: DisplayDate(Eval("CreatedAt")) %></time></article>
                    </ItemTemplate>
                </asp:Repeater>
            </asp:Panel>
            <asp:Panel
                ID="pnlNoReviews"
                runat="server"
                Visible="false"
                CssClass="chef-dashboard-empty">
                <p>No course reviews yet.</p>
            </asp:Panel>
        </section>
    </main>
</asp:Content>
<asp:Content
    ID="DashboardScripts"
    ContentPlaceHolderID="ChefScripts"
    runat="server">
    <script src="<%= ResolveUrl("~/Pages/Chef/ChefDashboard.js") %>" defer></script>
</asp:Content>
