<%@ Page Title="Learner Dashboard | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Learner/Learner.Master" AutoEventWireup="true" CodeBehind="LearnerDashboard.aspx.cs" Inherits="CookingLah.Pages.Learner.LearnerDashboard" %>
<asp:Content
    ID="DashboardHead"
    ContentPlaceHolderID="LearnerHead"
    runat="server">
    <link href="<%= ResolveUrl("~/Pages/Learner/CSS/learnerDashboard.css") %>" rel="stylesheet" />
</asp:Content>
<asp:Content
    ID="DashboardContent"
    ContentPlaceHolderID="LearnerContent"
    runat="server">
    <svg
        class="dashboard-ink-filters"
        xmlns="http://www.w3.org/2000/svg"
        width="0"
        height="0"
        aria-hidden="true"
        focusable="false">
        <defs>
            <filter
                id="dashboardRoughInk"
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
    <div class="learner-dashboard">
        <section class="dashboard-welcome" aria-labelledby="welcomeTitle">
            <div class="dashboard-welcome-copy">
                <h1 id="welcomeTitle">Welcome back, <asp:Label
                        ID="lblLearnerName"
                        runat="server"
                        Text="Learner" />!</h1>
                <p>Apron on? Let&rsquo;s add something delicious to your notebook.</p>
                <p class="dashboard-preview text-muted">Your learning notebook, updated as you cook.</p>
            </div>
            <div class="dashboard-mascot" aria-hidden="true"><img runat="server" src="~/F_Assets/mascot.png" alt="" /><span>A little practice,<br />a lot of flavour.</span></div>
        </section>
        <section class="dashboard-summary-grid" aria-label="Learning summary">
            <article class="stat-folder">
                <span class="stat-tab"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><circle cx="12" cy="12" r="9" /><path d="M12 7v5l3 2" /></svg> In progress</span>
                <div class="stat-body">
                    <div class="stat-sheet">
                        <asp:Label
                            ID="lblCoursesInProgress"
                            runat="server"
                            Text="3"
                            CssClass="summary-value" />
                        <p class="stat-label">Courses in progress</p>
                    </div>
                </div>
            </article>
            <article class="stat-folder">
                <span class="stat-tab"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><circle cx="12" cy="12" r="9" /><path d="m8 12 3 3 5-6" /></svg> Completed</span>
                <div class="stat-body">
                    <div class="stat-sheet">
                        <asp:Label
                            ID="lblCoursesCompleted"
                            runat="server"
                            Text="5"
                            CssClass="summary-value" />
                        <p class="stat-label">Courses completed</p>
                    </div>
                </div>
            </article>
            <article class="stat-folder">
                <span class="stat-tab"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><path d="M9 5H5v16h14V5h-4M9 3h6v4H9ZM8 12h8m-8 4h5" /></svg> Quizzes</span>
                <div class="stat-body">
                    <div class="stat-sheet">
                        <asp:Label
                            ID="lblQuizzesTaken"
                            runat="server"
                            Text="12"
                            CssClass="summary-value" />
                        <p class="stat-label">Quizzes taken</p>
                    </div>
                </div>
            </article>
            <article class="stat-folder">
                <span class="stat-tab"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><path d="M4 3v18h17M8 16v-4m5 4V8m5 8V4" /></svg> Avg score</span>
                <div class="stat-body">
                    <div class="stat-sheet">
                        <asp:Label
                            ID="lblAverageQuizScore"
                            runat="server"
                            Text="82%"
                            CssClass="summary-value" />
                        <p class="stat-label">Average quiz score</p>
                    </div>
                </div>
            </article>
        </section>
        <section aria-labelledby="continueTitle">
            <h2 id="continueTitle" class="ink-underline"><svg
                    class="dashboard-title-icon"
                    viewBox="0 0 24 24"
                    aria-hidden="true"
                    focusable="false"><path d="M12 5v16M12 5C8 2 4 3 2 4v15c4-1 7-1 10 2 3-3 6-3 10-2V4c-2-1-6-2-10 1Z" /></svg> Back to the kitchen</h2>
            <article class="notebook continue-learning-card course-book">
                <div class="notebook-spread">
                    <div class="notebook-page continue-photo-page">
                        <p class="notebook-caption">On the menu today</p>
                        <asp:Image
                            ID="imgContinueCourse"
                            runat="server"
                            ImageUrl="~/F_Assets/Food/nasi lemak.png"
                            AlternateText="Nasi lemak with traditional Malaysian accompaniments"
                            CssClass="continue-image" />
                        <span class="handwritten-note">A taste of home, made by you.</span>
                        <svg
                            class="cooking-doodle"
                            viewBox="0 0 100 60"
                            fill="none"
                            aria-hidden="true"><path
                                d="M12 28 Q46 22 79 29 Q74 53 46 53 Q19 51 12 28Z M8 28 Q46 33 84 28 M32 18 Q25 11 34 4 M48 18 Q40 10 49 2 M63 18 Q56 11 65 5 M80 13 L92 3"
                                stroke="currentColor"
                                stroke-width="2.5"
                                stroke-linecap="round"
                                stroke-linejoin="round" /></svg>
                    </div>
                    <div class="notebook-page">
                        <div class="scrap-paper scrap-tape continue-details course-book-card">
                            <p class="scrap-label">Your bookmarked course</p>
                            <h3><svg
                                    class="dashboard-title-icon"
                                    viewBox="0 0 24 24"
                                    aria-hidden="true"
                                    focusable="false"><path d="M12 5v16M12 5C8 2 4 3 2 4v15c4-1 7-1 10 2 3-3 6-3 10-2V4c-2-1-6-2-10 1Z" /></svg> <asp:Label
                                    ID="lblContinueCourseTitle"
                                    runat="server"
                                    Text="Basic Malaysian Cooking" /></h3>
                            <p class="text-muted"><asp:Label
                                    ID="lblContinueDifficulty"
                                    runat="server"
                                    Text="Beginner" /> &bull; <asp:Label
                                    ID="lblContinueCategory"
                                    runat="server"
                                    Text="Malaysian Cuisine" /></p>
                            <div class="course-progress-label"><span>Course progress</span><asp:Label
                                    ID="lblContinueProgress"
                                    runat="server"
                                    Text="70%" /></div>
                            <progress
                                class="course-progress"
                                value="<%= ContinueProgress %>"
                                max="100"
                                aria-label="Course progress"><%= ContinueProgress %>%</progress>
                            <p class="text-muted">Last accessed: <asp:Label
                                    ID="lblLastAccessed"
                                    runat="server"
                                    Text="21 Oct 2026" /></p>
                            <asp:Button
                                ID="btnContinueCourse"
                                runat="server"
                                Text="Continue Course"
                                CssClass="btn btn-primary"
                                OnClick="ContinueCourse_Click"
                                CausesValidation="false" />
                        </div>
                    </div>
                </div>
            </article>
        </section>
        <section aria-labelledby="currentTitle">
            <div class="dashboard-section-heading">
                <h2 id="currentTitle"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><path d="M12 5v16M12 5C8 2 4 3 2 4v15c4-1 7-1 10 2 3-3 6-3 10-2V4c-2-1-6-2-10 1Z" /></svg> My Courses</h2><a class="text-muted" href="<%= ResolveUrl("~/Pages/Learner/MyCourse/LearnerMyCourse.aspx") %>">View all</a></div>
            <div class="dashboard-course-grid">
                <asp:Repeater ID="rptCurrentCourses" runat="server">
                    <ItemTemplate>
                        <article class="scrap-paper scrap-tape dashboard-course-card">
                            <asp:Image
                                runat="server"
                                ImageUrl='<%# CourseImage(Eval("Thumbnail")) %>'
                                Visible='<%# HasCourseImage(Eval("Thumbnail")) %>'
                                AlternateText='<%# Convert.ToString(Eval("Title")) %>'
                                CssClass="course-thumbnail" />
                            <asp:Panel
                                runat="server"
                                CssClass="course-thumbnail course-image-placeholder"
                                Visible='<%# !HasCourseImage(Eval("Thumbnail")) %>'><img runat="server" src="~/F_Assets/mascot.png" alt="" /><span>Food Image Coming Soon</span></asp:Panel>
                            <p class="card-eyebrow"><%#: Eval("DifficultyLevel") %></p>
                            <h3><svg
                                    class="dashboard-title-icon"
                                    viewBox="0 0 24 24"
                                    aria-hidden="true"
                                    focusable="false"><path d="M12 5v16M12 5C8 2 4 3 2 4v15c4-1 7-1 10 2 3-3 6-3 10-2V4c-2-1-6-2-10 1Z" /></svg> <%#: Eval("Title") %></h3>
                            <p>Cooking time: <%#: Eval("CookingTime") == DBNull.Value ? "Not specified" : Eval("CookingTime") + " min" %></p>
                            <div class="course-progress-label"><span>Progress</span><span><%#: Eval("ProgressPercentage") %>%</span></div>
                            <progress
                                class="course-progress"
                                value='<%# Eval("ProgressPercentage") %>'
                                max="100"
                                aria-label='<%# Eval("Title") + " progress" %>'><%#: Eval("ProgressPercentage") %>%</progress>
                            <a class="btn btn-secondary" href='<%# CourseLearningUrl(Eval("CourseID")) %>'>Continue</a>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </section>
        <div class="dashboard-detail-grid">
            <section class="scrap-paper dashboard-notes" aria-labelledby="quizTitle">
                <h2 id="quizTitle"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><path d="M9 5H5v16h14V5h-4M9 3h6v4H9ZM8 12h8m-8 4h5" /></svg> Recent Quiz Results</h2>
                <ul class="quiz-results">
                <% if (rptRecentQuizzes.Items.Count == 0) { %><li class="text-muted">No quiz results yet.</li><% } %>
                    <asp:Repeater ID="rptRecentQuizzes" runat="server">
                        <ItemTemplate>
                            <li>
                                <div>
                                    <h3><svg
                                            class="dashboard-title-icon"
                                            viewBox="0 0 24 24"
                                            aria-hidden="true"
                                            focusable="false"><path d="M12 5v16M12 5C8 2 4 3 2 4v15c4-1 7-1 10 2 3-3 6-3 10-2V4c-2-1-6-2-10 1Z" /></svg> <%#: Eval("Title") %></h3>
                                </div>
                                <div class="quiz-result-score"><strong><%#: Eval("Score") %>%</strong><span class='tag dashboard-status <%# Convert.ToBoolean(Eval("Passed")) ? "status-success" : "status-danger" %>'><%# Convert.ToBoolean(Eval("Passed")) ? "Passed" : "Failed" %></span></div>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </section>
            <section class="scrap-paper dashboard-notes" aria-labelledby="activitiesTitle">
                <h2 id="activitiesTitle"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><path d="M3 6h4l2-3h6l2 3h4v15H3Z" /><circle cx="12" cy="13" r="4" /></svg> Practical Activities</h2>
                <ul class="activity-list">
                <% if (rptPracticalActivities.Items.Count == 0) { %><li class="text-muted">No practical activities yet.</li><% } %>
                    <asp:Repeater ID="rptPracticalActivities" runat="server">
                        <ItemTemplate>
                            <li>
                                <h3><svg
                                        class="dashboard-title-icon"
                                        viewBox="0 0 24 24"
                                        aria-hidden="true"
                                        focusable="false"><path d="M12 5v16M12 5C8 2 4 3 2 4v15c4-1 7-1 10 2 3-3 6-3 10-2V4c-2-1-6-2-10 1Z" /></svg> <%#: Eval("Title") %></h3>
                                <p class="text-muted">Course: <%#: Eval("CourseTitle") %></p>
                                <div class="activity-actions"><span class='tag dashboard-status <%# ActivityStatusClass(Eval("SubmissionStatus")) %>'><%#: ActivityStatus(Eval("SubmissionStatus")) %></span>
                                    <a class="btn btn-secondary" href='<%# ActivityUrl(Eval("CourseID"), Eval("SubmissionID")) %>'><%# Eval("SubmissionID") == DBNull.Value ? "Submit" : "View" %></a></div>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </section>
        </div>
        <section aria-labelledby="exploreTitle">
            <div class="dashboard-section-heading">
                <h2 id="exploreTitle"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><circle cx="10" cy="10" r="7" /><path d="m15 15 6 6" /></svg> Explore New Courses</h2><a class="text-muted" href="<%= ResolveUrl("~/Pages/Learner/SearchCourse/SearchCoursePage.aspx") %>">View all</a></div>
            <div class="dashboard-course-grid">
                <asp:Repeater ID="rptExploreCourses" runat="server">
                    <ItemTemplate>
                        <article class="scrap-paper scrap-tape dashboard-course-card">
                            <asp:Image
                                runat="server"
                                ImageUrl='<%# CourseImage(Eval("Thumbnail")) %>'
                                Visible='<%# HasCourseImage(Eval("Thumbnail")) %>'
                                AlternateText='<%# Convert.ToString(Eval("Title")) %>'
                                CssClass="course-thumbnail" />
                            <asp:Panel
                                runat="server"
                                CssClass="course-thumbnail course-image-placeholder"
                                Visible='<%# !HasCourseImage(Eval("Thumbnail")) %>'><img runat="server" src="~/F_Assets/mascot.png" alt="" /><span>Food Image Coming Soon</span></asp:Panel>
                            <p class="card-eyebrow"><%#: Eval("DifficultyLevel") %> &bull; <%#: Eval("CategoryName") %></p>
                            <h3><svg
                                    class="dashboard-title-icon"
                                    viewBox="0 0 24 24"
                                    aria-hidden="true"
                                    focusable="false"><path d="M12 5v16M12 5C8 2 4 3 2 4v15c4-1 7-1 10 2 3-3 6-3 10-2V4c-2-1-6-2-10 1Z" /></svg> <%#: Eval("Title") %></h3>
                            <p>Cooking time: <%#: Eval("CookingTime") == DBNull.Value ? "Not specified" : Eval("CookingTime") + " min" %></p>
                            <p class="text-muted">Rated <%#: Eval("AverageRating") %> / 5</p>
                            <a class="btn btn-secondary" href='<%# CourseOverviewUrl(Eval("CourseID")) %>'>View Course</a>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </section>
        <section class="scrap-paper food-map-cta" aria-labelledby="foodMapTitle">
            <div>
                <p class="handwritten-note">For your next kitchen adventure...</p>
                <h2 id="foodMapTitle"><svg
                        class="dashboard-title-icon"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                        focusable="false"><circle cx="12" cy="12" r="9" /><ellipse
                            cx="12"
                            cy="12"
                            rx="4"
                            ry="9" /><path d="M3 12h18" /></svg> A world of flavour awaits</h2>
                <p>Discover traditional dishes, ingredients, and cooking methods from different countries.</p>
            </div>
            <asp:Button
                ID="btnExploreFoodMap"
                runat="server"
                Text="Explore Food Map"
                CssClass="btn btn-primary"
                OnClick="ExploreFoodMap_Click"
                CausesValidation="false" />
        </section>
    </div>
</asp:Content>
<asp:Content
    ID="DashboardScripts"
    ContentPlaceHolderID="LearnerScripts"
    runat="server">
    <script src="<%= ResolveUrl("~/Pages/Learner/JS/learnerDashboard.js") %>" defer></script>
</asp:Content>
