<%@ Page Title="My Courses | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Learner/Learner.Master" AutoEventWireup="true" CodeBehind="LearnerMyCourse.aspx.cs" Inherits="CookingLah.Pages.Learner.MyCourse.LearnerMyCourse" %>

<asp:Content ID="MyCoursesHead" ContentPlaceHolderID="LearnerHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Learner/MyCourse/LearnerMyCourse.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="MyCoursesContent" ContentPlaceHolderID="LearnerContent" runat="server">
    <div class="my-courses-page" aria-labelledby="myCoursesTitle">
        <header class="my-courses-heading">
            <div>
                <p class="my-courses-kicker">Your learning notebook</p>
                <h1 id="myCoursesTitle">My courses</h1>
                <p>Pick up where you left off and celebrate what you have completed.</p>
            </div>
            <span class="my-courses-stamp" aria-hidden="true">One lesson<br />at a time</span>
        </header>

        <asp:Panel ID="pnlCourseFilters" runat="server" CssClass="my-courses-filters" DefaultButton="btnSearch">
            <div class="my-courses-search-field">
                <label for="<%= txtSearchCourses.ClientID %>">Search your courses</label>
                <div class="my-courses-search-row">
                    <div class="my-courses-search-input">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><circle cx="10.5" cy="10.5" r="6.5" /><path d="m15.5 15.5 5 5" /></svg>
                        <asp:TextBox ID="txtSearchCourses" runat="server" TextMode="Search" MaxLength="100"
                            placeholder="Search by course name or cuisine" />
                    </div>
                    <asp:Button ID="btnSearch" runat="server" Text="Find courses" CssClass="btn btn-primary"
                        OnClick="btnSearch_Click" CausesValidation="false" />
                </div>
            </div>
            <div class="my-courses-category-field">
                <label for="<%= ddlCategory.ClientID %>">Category</label>
                <asp:DropDownList ID="ddlCategory" runat="server" AutoPostBack="true"
                    OnSelectedIndexChanged="FiltersChanged" />
            </div>
            <asp:LinkButton ID="btnClearFilters" runat="server" Text="Clear filters"
                CssClass="my-courses-clear" OnClick="btnClearFilters_Click" CausesValidation="false" />
        </asp:Panel>

        <nav class="my-course-status-filters" aria-label="Filter by learning status">
            <asp:Panel ID="pnlAllCoursesFilter" runat="server" CssClass="my-course-status-filter">
                <asp:LinkButton ID="btnAllCourses" runat="server" Text="All courses"
                    OnClick="btnAllCourses_Click" CausesValidation="false" />
                <asp:Label ID="lblAllCoursesCount" runat="server" CssClass="my-course-status-count" />
            </asp:Panel>
            <asp:Panel ID="pnlInProgressFilter" runat="server" CssClass="my-course-status-filter">
                <asp:LinkButton ID="btnInProgress" runat="server" Text="In progress"
                    OnClick="btnInProgress_Click" CausesValidation="false" />
                <asp:Label ID="lblInProgressCount" runat="server" CssClass="my-course-status-count" />
            </asp:Panel>
            <asp:Panel ID="pnlCompletedFilter" runat="server" CssClass="my-course-status-filter">
                <asp:LinkButton ID="btnCompleted" runat="server" Text="Completed"
                    OnClick="btnCompleted_Click" CausesValidation="false" />
                <asp:Label ID="lblCompletedCount" runat="server" CssClass="my-course-status-count" />
            </asp:Panel>
        </nav>

        <asp:Panel ID="pnlMyLearning" runat="server" Visible="false">
            <div class="my-learning-heading">
                <div>
                    <h2>My learning</h2>
                    <asp:Label ID="lblCourseSummary" runat="server" CssClass="my-course-summary" />
                </div>
                <div class="course-view-toggle" role="group" aria-label="Course list layout">
                    <button id="btnTwoColumnView" class="course-view-toggle-button is-active" type="button"
                        aria-label="2 card view" title="2 card view" aria-pressed="true" data-view-mode="two-column-view">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><rect x="3" y="4" width="8" height="7" rx="1" /><rect x="13" y="4" width="8" height="7" rx="1" /><rect x="3" y="13" width="8" height="7" rx="1" /><rect x="13" y="13" width="8" height="7" rx="1" /></svg>
                    </button>
                    <button id="btnSingleColumnView" class="course-view-toggle-button" type="button"
                        aria-label="1 card view" title="1 card view" aria-pressed="false" data-view-mode="single-column-view">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><rect x="3" y="4" width="18" height="5" rx="1" /><rect x="3" y="11" width="18" height="5" rx="1" /><rect x="3" y="18" width="18" height="3" rx="1" /></svg>
                    </button>
                </div>
            </div>

            <div id="myCourseList" class="my-course-list two-column-view">
                <asp:Repeater ID="rptMyCourses" runat="server" OnItemCommand="rptMyCourses_ItemCommand">
                    <ItemTemplate>
                        <article class="my-course-card">
                            <div class="my-course-thumbnail-wrap">
                                <asp:Image ID="imgCourseThumbnail" runat="server" CssClass="my-course-thumbnail"
                                    ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail"))) %>'
                                    Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>'
                                    AlternateText='<%# Convert.ToString(Eval("Title")) + " course image" %>' loading="lazy" />
                                <asp:Panel runat="server" CssClass="my-course-thumbnail-empty course-image-placeholder"
                                    Visible='<%# string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>'>
                                    <img runat="server" src="~/F_Assets/mascot.png" alt="" />
                                    <span>Food Image Coming Soon</span>
                                </asp:Panel>
                            </div>
                            <div class="my-course-content">
                                <div class="my-course-title-row">
                                    <div>
                                        <p class="my-course-category"><%#: Eval("CategoryName") %></p>
                                        <h3><%#: Eval("Title") %></h3>
                                    </div>
                                    <span class='<%# StatusBadgeClass(Eval("ProgressStatus")) %>'><%#: Eval("ProgressStatus") %></span>
                                </div>
                                <p class="my-course-meta"><%#: Eval("DifficultyLevel") %> <span aria-hidden="true">·</span> By <%#: Eval("ChefName") %></p>
                                <div class="my-course-progress">
                                    <div class="my-course-progress-label">
                                        <span>Progress</span>
                                        <strong><%#: Eval("ProgressPercentage") %>%</strong>
                                    </div>
                                    <div class="my-course-progress-track" role="progressbar" aria-label='<%# Convert.ToString(Eval("Title")) + " progress" %>'
                                        aria-valuemin="0" aria-valuemax="100" aria-valuenow='<%# Eval("ProgressPercentage") %>'>
                                        <span class="my-course-progress-fill" style='<%# "width:" + Eval("ProgressPercentage") + "%" %>'></span>
                                    </div>
                                </div>
                                <div class="my-course-facts">
                                    <span><%#: Eval("LessonCount") %> lessons</span>
                                    <span><%#: Eval("QuizCount") %> quizzes</span>
                                    <span><%#: Eval("ActivityCount") %> activities</span>
                                </div>
                                <div class="my-course-actions">
                                    <asp:LinkButton ID="btnGoToCourse" runat="server"
                                        Text='<%# CourseActionText(Eval("ProgressStatus")) %>'
                                        CssClass="btn btn-primary" CommandName="GoToCourse"
                                        CommandArgument='<%# Eval("CourseID") %>' CausesValidation="false" />
                                </div>
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <asp:Panel ID="pnlPagination" runat="server" CssClass="my-course-pagination" Visible="false">
                <asp:Button ID="btnPrevious" runat="server" Text="Previous" CssClass="btn btn-secondary"
                    OnClick="btnPrevious_Click" CausesValidation="false" />
                <asp:Label ID="lblPageStatus" runat="server" CssClass="my-course-page-status" />
                <asp:Button ID="btnNext" runat="server" Text="Next" CssClass="btn btn-secondary"
                    OnClick="btnNext_Click" CausesValidation="false" />
            </asp:Panel>
        </asp:Panel>

        <asp:Panel ID="pnlNoStartedCourses" runat="server" Visible="false" CssClass="my-course-empty-state">
            <span class="my-course-empty-art" aria-hidden="true">
                <svg viewBox="0 0 80 80" focusable="false"><path d="M17 44c2 17 11 25 23 25s21-8 23-25H17Zm47 7h5c5 0 7 3 7 6s-2 6-7 6h-5M13 42c13-5 47-5 60 0M30 33c-4-6 4-8 1-13m14 13c-4-6 4-8 1-13" /><path d="m20 16 3 6 6 3-6 2-3 6-2-6-6-2 6-3 2-6Z" /></svg>
            </span>
            <h2>You haven't started any courses yet.</h2>
            <p>Explore the menu of courses and start your learning journey.</p>
            <a class="btn btn-primary" href="<%= ResolveUrl("~/Pages/Learner/SearchCourse/SearchCoursePage.aspx") %>">Explore courses</a>
        </asp:Panel>

        <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="my-course-empty-state my-course-empty-filter">
            <h2>No courses match these filters.</h2>
            <p>Try another course name or clear your filters.</p>
            <asp:LinkButton ID="btnShowAllCourses" runat="server" Text="Show all courses"
                CssClass="btn btn-secondary" OnClick="btnClearFilters_Click" CausesValidation="false" />
        </asp:Panel>
    </div>
</asp:Content>

<asp:Content ID="MyCoursesScripts" ContentPlaceHolderID="LearnerScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Learner/MyCourse/LearnerMyCourse.js") %>" defer></script>
</asp:Content>
