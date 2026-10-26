<%@ Page Title="Learner Dashboard | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Learner/Learner.Master" AutoEventWireup="true" CodeBehind="LearnerDashboard.aspx.cs" Inherits="CookingLah.Pages.Learner.LearnerDashboard" %>
<asp:Content ID="DashboardHead" ContentPlaceHolderID="LearnerHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Learner/CSS/learnerDashboard.css") %>" rel="stylesheet" />
</asp:Content>
<asp:Content ID="DashboardContent" ContentPlaceHolderID="LearnerContent" runat="server">
<svg class="dashboard-ink-filters" xmlns="http://www.w3.org/2000/svg" width="0" height="0" aria-hidden="true" focusable="false">
    <defs>
        <filter id="dashboardRoughInk" x="-10%" y="-10%" width="120%" height="120%" color-interpolation-filters="sRGB">
            <feTurbulence type="fractalNoise" baseFrequency="0.035" numOctaves="3" seed="8" result="paperNoise" />
            <feDisplacementMap in="SourceGraphic" in2="paperNoise" scale="4" xChannelSelector="R" yChannelSelector="G" />
        </filter>
    </defs>
</svg>
<div class="learner-dashboard">
    <section class="dashboard-welcome" aria-labelledby="welcomeTitle">
        <div class="dashboard-welcome-copy">
        <h1 id="welcomeTitle">Welcome back, <asp:Label ID="lblLearnerName" runat="server" Text="Learner" />!</h1>
        <p>Apron on? Let&rsquo;s add something delicious to your notebook.</p>
        <p class="dashboard-preview text-muted">Sample notebook &bull; course activities are coming soon.</p>
        </div>
        <div class="dashboard-mascot" aria-hidden="true"><img runat="server" src="~/F_Assets/mascot.png" alt="" /><span>A little practice,<br />a lot of flavour.</span></div>
    </section>
    <section class="dashboard-summary-grid" aria-label="Learning summary">
        <article class="scrap-paper summary-card"><asp:Label ID="lblCoursesInProgress" runat="server" Text="3" CssClass="summary-value" /><p>Courses in progress</p></article>
        <article class="scrap-paper summary-card"><asp:Label ID="lblCoursesCompleted" runat="server" Text="5" CssClass="summary-value" /><p>Courses completed</p></article>
        <article class="scrap-paper summary-card"><asp:Label ID="lblQuizzesTaken" runat="server" Text="12" CssClass="summary-value" /><p>Quizzes taken</p></article>
        <article class="scrap-paper summary-card"><asp:Label ID="lblAverageQuizScore" runat="server" Text="82%" CssClass="summary-value" /><p>Average quiz score</p></article>
    </section>
    <section aria-labelledby="continueTitle">
        <h2 id="continueTitle" class="ink-underline">Back to the kitchen</h2>
        <article class="notebook continue-learning-card course-book">
            <div class="notebook-spread">
                <div class="notebook-page continue-photo-page">
                    <p class="notebook-caption">On the menu today</p>
                    <asp:Image ID="imgContinueCourse" runat="server" ImageUrl="~/F_Assets/Food/nasi lemak.png" AlternateText="Nasi lemak with traditional Malaysian accompaniments" CssClass="continue-image" />
                    <span class="handwritten-note">A taste of home, made by you.</span>
                    <svg class="cooking-doodle" viewBox="0 0 100 60" fill="none" aria-hidden="true"><path d="M12 28 Q46 22 79 29 Q74 53 46 53 Q19 51 12 28Z M8 28 Q46 33 84 28 M32 18 Q25 11 34 4 M48 18 Q40 10 49 2 M63 18 Q56 11 65 5 M80 13 L92 3" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" /></svg>
                </div>
                <div class="notebook-page">
                    <div class="scrap-paper scrap-tape continue-details course-book-card">
                        <p class="scrap-label">Your bookmarked course</p>
                        <h3><asp:Label ID="lblContinueCourseTitle" runat="server" Text="Basic Malaysian Cooking" /></h3>
                        <p class="text-muted"><asp:Label ID="lblContinueDifficulty" runat="server" Text="Beginner" /> &bull; <asp:Label ID="lblContinueCategory" runat="server" Text="Malaysian Cuisine" /></p>
                        <div class="course-progress-label"><span>Course progress</span><asp:Label ID="lblContinueProgress" runat="server" Text="70%" /></div>
                        <%-- TODO: Bind value and visible percentage to the same CourseProgress value. --%>
                        <progress class="course-progress" value="70" max="100" aria-label="Basic Malaysian Cooking progress">70%</progress>
                        <p class="text-muted">Last accessed: <asp:Label ID="lblLastAccessed" runat="server" Text="21 Oct 2026" /></p>
                        <asp:Button ID="btnContinueCourse" runat="server" Text="Continue Course" CssClass="btn btn-primary" OnClick="ContinueCourse_Click" CausesValidation="false" ToolTip="Course navigation coming soon" />
                    </div>
                </div>
            </div>
        </article>
    </section>
    <section aria-labelledby="currentTitle">
        <div class="dashboard-section-heading"><h2 id="currentTitle">My Courses</h2><span class="text-muted" title="Future destination: myCourses.aspx">View all &mdash; coming soon</span></div>
        <div class="dashboard-course-grid">
            <asp:Repeater ID="rptCurrentCourses" runat="server" DataSourceID="previewCurrentCourses">
                <ItemTemplate>
                    <article class="scrap-paper scrap-tape dashboard-course-card">
                        <asp:Image runat="server" ImageUrl='<%# XPath("@image") %>' AlternateText='<%# XPath("@title") %>' CssClass="course-thumbnail" />
                        <p class="card-eyebrow"><%#: XPath("@difficulty") %></p>
                        <h3><%#: XPath("@title") %></h3>
                        <div class="course-progress-label"><span>Progress</span><span><%#: XPath("@progress") %>%</span></div>
                        <progress class="course-progress" value='<%#: XPath("@progress") %>' max="100" aria-label='<%#: XPath("@title") + " progress" %>'><%#: XPath("@progress") %>%</progress>
                        <%-- TODO: Navigate with the bound course ID when the course page exists. --%>
                        <button type="button" class="btn btn-secondary" title="Course navigation coming soon">Continue</button>
                    </article>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </section>
    <div class="dashboard-detail-grid">
        <section class="scrap-paper dashboard-notes" aria-labelledby="quizTitle">
            <h2 id="quizTitle">Recent Quiz Results</h2>
            <ul class="quiz-results">
                <asp:Repeater ID="rptRecentQuizzes" runat="server" DataSourceID="previewQuizzes">
                    <ItemTemplate>
                        <li><div><h3><%#: XPath("@title") %></h3><time class="text-muted" datetime='<%#: XPath("@dateIso") %>'><%#: XPath("@date") %></time></div>
                            <div class="quiz-result-score"><strong><%#: XPath("@score") %>%</strong><span class='tag dashboard-status <%# XPath("@status").ToString() == "Passed" ? "status-success" : "status-danger" %>'><%#: XPath("@status") %></span></div></li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </section>
        <section class="scrap-paper dashboard-notes" aria-labelledby="activitiesTitle">
            <h2 id="activitiesTitle">Practical Activities</h2>
            <ul class="activity-list">
                <asp:Repeater ID="rptPracticalActivities" runat="server" DataSourceID="previewActivities">
                    <ItemTemplate>
                        <li><h3><%#: XPath("@title") %></h3><p class="text-muted">Course: <%#: XPath("@course") %></p>
                            <div class="activity-actions"><span class='tag dashboard-status <%# XPath("@status").ToString() == "Reviewed" ? "status-success" : "status-pending" %>'><%#: XPath("@status") %></span>
                            <%-- TODO: Link to practicalActivities.aspx with the activity ID. --%>
                            <button class="btn btn-secondary" type="button" title="Activity page coming soon"><%#: XPath("@action") %></button></div></li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </section>
    </div>
    <section aria-labelledby="exploreTitle">
        <div class="dashboard-section-heading"><h2 id="exploreTitle">Explore New Courses</h2><span class="text-muted" title="Future destination: exploreCourses.aspx">View all &mdash; coming soon</span></div>
        <div class="dashboard-course-grid">
            <asp:Repeater ID="rptExploreCourses" runat="server" DataSourceID="previewExploreCourses">
                <ItemTemplate>
                    <article class="scrap-paper scrap-tape dashboard-course-card">
                        <asp:Image runat="server" ImageUrl='<%# XPath("@image") %>' AlternateText='<%# XPath("@title") %>' CssClass="course-thumbnail" />
                        <p class="card-eyebrow"><%#: XPath("@difficulty") %> &bull; <%#: XPath("@category") %></p>
                        <h3><%#: XPath("@title") %></h3>
                        <p class="text-muted">Rated <%#: XPath("@rating") %> / 5</p>
                        <button class="btn btn-secondary" type="button" title="Course details coming soon">View Course</button>
                    </article>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </section>
    <section class="scrap-paper food-map-cta" aria-labelledby="foodMapTitle">
        <div><p class="handwritten-note">For your next kitchen adventure...</p><h2 id="foodMapTitle">A world of flavour awaits</h2><p>Discover traditional dishes, ingredients, and cooking methods from different countries.</p></div>
        <asp:Button ID="btnExploreFoodMap" runat="server" Text="Explore Food Map" CssClass="btn btn-primary" OnClick="ExploreFoodMap_Click" CausesValidation="false" />
    </section>
</div>
<%-- FRONTEND PREVIEW ONLY. Inline XML keeps sample data out of backend loaders.
     Replace DataSourceID with database binding in the TODO methods when ready.
     Never persist Not Submitted: it means no ActivitySubmissions record exists. --%>
    <asp:XmlDataSource ID="previewCurrentCourses" runat="server" XPath="/preview/course" EnableCaching="false">
        <Data><preview>
            <course title="Basic Malaysian Cooking" difficulty="Beginner" progress="70" image="~/F_Assets/Food/nasi lemak.png" />
            <course title="Malaysian Grilling" difficulty="Intermediate" progress="40" image="~/F_Assets/LoginFood2.png" />
            <course title="Kitchen Essentials" difficulty="Beginner" progress="20" image="~/F_Assets/LoginFood3.png" />
        </preview></Data>
    </asp:XmlDataSource>
    <asp:XmlDataSource ID="previewQuizzes" runat="server" XPath="/preview/quiz" EnableCaching="false">
        <Data><preview>
            <quiz title="Food Safety Quiz" score="85" status="Passed" date="21 Oct 2026" dateIso="2026-10-21" />
            <quiz title="Grilling Basics Quiz" score="70" status="Passed" date="20 Oct 2026" dateIso="2026-10-20" />
            <quiz title="Knife Safety Quiz" score="55" status="Failed" date="18 Oct 2026" dateIso="2026-10-18" />
        </preview></Data>
    </asp:XmlDataSource>
    <asp:XmlDataSource ID="previewActivities" runat="server" XPath="/preview/activity" EnableCaching="false">
        <Data><preview>
            <activity title="Grill an Ikan Bakar" course="Malaysian Grilling" status="Not Submitted" action="Submit" />
            <activity title="Basic Knife Cutting" course="Knife Skills" status="Reviewed" action="View" />
            <activity title="Build a Balanced Plate" course="Kitchen Essentials" status="Submitted" action="View" />
        </preview></Data>
    </asp:XmlDataSource>
    <asp:XmlDataSource ID="previewExploreCourses" runat="server" XPath="/preview/course" EnableCaching="false">
        <Data><preview>
            <course title="Everyday Malaysian Favourites" difficulty="Beginner" category="Malaysian" rating="4.8" image="~/F_Assets/Food/nasi lemak.png" />
            <course title="Build a Better Burger" difficulty="Beginner" category="Western" rating="4.7" image="~/F_Assets/Food/burger.png" />
            <course title="A Taste of Home" difficulty="Intermediate" category="Asian" rating="4.9" image="~/F_Assets/LoginFood1.png" />
        </preview></Data>
    </asp:XmlDataSource>
</asp:Content>
<asp:Content ID="DashboardScripts" ContentPlaceHolderID="LearnerScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Learner/JS/learnerDashboard.js") %>" defer></script>
</asp:Content>
