<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchCourse.ascx.cs" Inherits="CookingLah.Pages.Shared.CourseSearch.SearchCourse" %>
<%@ Register Src="~/Pages/Shared/CourseSearch/ViewCourseOverivew.ascx" TagPrefix="course" TagName="ViewCourseOverivew" %>

<asp:Panel ID="pnlCourseSearch" runat="server" CssClass="course-search-panel">
    <header class="search-hero">
        <div class="search-hero-copy">
            <p class="search-kicker">A little practice, a lot of flavour</p>
            <h1>Cook something<br />new.</h1>
            <p class="search-intro">Learn a new technique, explore a favourite cuisine, and make the next meal your own.</p>
            <span class="search-hero-caption">Pick a course. Tie on an apron. Begin.</span>
        </div>
        <div class="search-hero-art" aria-hidden="true">
            <svg class="kitchen-doodle" viewBox="0 0 420 300" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path class="doodle-ink doodle-pan" d="M76 151c5 56 45 91 107 91 63 0 104-36 108-91H76Z" />
                <path class="doodle-ink" d="M289 168h55c19 0 28 9 28 22s-9 22-28 22h-65" />
                <path class="doodle-ink" d="M67 145c38-14 163-15 231 0M94 163c44 11 132 11 184-1" />
                <path class="doodle-ink doodle-steam" d="M129 112c-13-20 13-24 1-44M184 106c-12-20 13-24 1-43M237 112c-13-20 13-24 1-44" />
                <path class="doodle-fill-egg" d="M131 145c2-30 25-49 55-49 31 0 54 19 55 49-27 14-83 15-110 0Z" />
                <circle class="doodle-yolk" cx="186" cy="126" r="16" />
                <path class="doodle-leaf" d="M91 89c-15-24-12-46 12-61 19 21 16 43-12 61ZM293 103c8-30 27-43 55-37-2 27-20 42-55 37ZM305 66l-12 35" />
                <path class="doodle-ink" d="M91 89c-15-24-12-46 12-61 19 21 16 43-12 61ZM293 103c8-30 27-43 55-37-2 27-20 42-55 37ZM305 66l-12 35" />
                <path class="doodle-ink doodle-spark" d="m62 105 5 12 12 5-12 5-5 12-5-12-12-5 12-5 5-12ZM347 116l4 9 9 4-9 4-4 9-4-9-9-4 9-4 4-9Z" />
                <path class="doodle-ink" d="M107 263c48 12 112 12 157-1" />
            </svg>
            <span class="search-hero-note">Fresh ideas<br />on the menu</span>
        </div>
    </header>

    <asp:Panel ID="pnlSearchForm" runat="server" CssClass="course-search-toolbar" DefaultButton="btnSearch">
        <div class="search-field search-field-main">
            <label for="<%= txtSearch.ClientID %>">What would you like to cook?</label>
            <div class="search-input-row">
                <div class="search-input-wrap">
                    <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><circle cx="10.8" cy="10.8" r="6.3" /><path d="m15.5 15.5 5 5" /></svg>
                    <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" MaxLength="100" placeholder="Search dishes, skills or cuisines" />
                </div>
                <asp:Button ID="btnSearch" runat="server" Text="Find courses" CssClass="btn btn-primary search-submit" OnClick="btnSearch_Click" CausesValidation="false" />
            </div>
        </div>
        <div class="search-field">
            <label for="<%= ddlCategory.ClientID %>">Course category</label>
            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="search-select" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged" />
        </div>
        <div class="search-field">
            <label for="<%= ddlDifficulty.ClientID %>">Difficulty</label>
            <asp:DropDownList ID="ddlDifficulty" runat="server" CssClass="search-select" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                <asp:ListItem Text="Any level" Value="" />
                <asp:ListItem Text="Beginner" Value="Beginner" />
                <asp:ListItem Text="Intermediate" Value="Intermediate" />
                <asp:ListItem Text="Advanced" Value="Advanced" />
            </asp:DropDownList>
        </div>
        <div class="search-filter-actions">
            <asp:LinkButton ID="btnClearSearch" runat="server" Text="Reset filters" CssClass="search-clear" OnClick="btnClearSearch_Click" CausesValidation="false" />
        </div>
    </asp:Panel>

    <section class="course-results" aria-labelledby="courseResultsTitle">
        <div class="results-heading">
            <div>
                <h2 id="courseResultsTitle">Courses to cook through</h2>
                <asp:Label ID="lblResultCount" runat="server" CssClass="results-count" aria-live="polite" />
            </div>
        </div>

        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="course-search-empty">
            <span class="empty-mark" aria-hidden="true">
                <svg viewBox="0 0 48 48" fill="none"><path d="M8 16h32l-3 20H11L8 16ZM6 13h36M16 12c-3-4 3-5 1-9m13 9c-3-4 3-5 1-9M17 23l6 6 9-9" /></svg>
            </span>
            <h3>No courses found</h3>
            <p>Try changing your search or filters.</p>
            <asp:LinkButton ID="btnClearEmpty" runat="server" Text="Clear filters" CssClass="btn btn-secondary" OnClick="btnClearSearch_Click" CausesValidation="false" />
        </asp:Panel>

        <asp:Repeater ID="rptCourses" runat="server" OnItemCommand="rptCourses_ItemCommand">
            <HeaderTemplate><div class="course-card-grid"></HeaderTemplate>
            <ItemTemplate>
                <article class="course-search-card">
                    <div class="course-card-image-wrap">
                        <asp:Image ID="imgCourse" runat="server" CssClass="course-card-image"
                            ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail"))) %>'
                            Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>'
                            AlternateText='<%# Convert.ToString(Eval("Title")) + " course" %>' loading="lazy" />
                        <asp:Panel runat="server" CssClass="course-card-image-empty"
                            Visible='<%# string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>'>
                            <svg viewBox="0 0 120 90" fill="none" aria-hidden="true"><path d="M17 45c2 23 20 36 43 36s41-13 43-36H17Zm86 8h11c8 0 11 4 11 9s-3 9-11 9H99M14 43c18-6 67-6 91 0M39 34c-6-9 6-11 1-19m21 19c-6-9 6-11 1-19m20 19c-6-9 6-11 1-19" /><circle cx="60" cy="46" r="9" /></svg>
                            <span>Ready to cook</span>
                        </asp:Panel>
                        <span class="course-level"><%#: Eval("DifficultyLevel") %></span>
                    </div>
                    <div class="course-card-body">
                        <p class="course-category"><%#: Eval("CategoryName") %></p>
                        <h3><%#: Eval("Title") %></h3>
                        <p class="course-chef">By <%#: Eval("ChefName") %></p>
                        <p class="course-card-description"><%#: Eval("ShortDescription") %></p>
                        <div class="course-card-facts">
                            <span><%#: Eval("LessonCount") %> lessons</span>
                            <span><%#: Eval("QuizCount") %> quizzes</span>
                            <span><%#: Eval("ActivityCount") %> activities</span>
                        </div>
                        <asp:Panel runat="server" CssClass="course-learning-status"
                            Visible='<%# HasStartedCourse(Eval("ProgressStatus"), Eval("ProgressPercentage")) %>'>
                            <div class="course-progress-heading">
                                <span><%#: CourseProgressLabel(Eval("ProgressStatus")) %></span>
                                <strong><%#: CourseProgressValue(Eval("ProgressStatus"), Eval("ProgressPercentage")) %>%</strong>
                            </div>
                            <div class="course-progress-track" role="progressbar" aria-label="Course progress"
                                aria-valuemin="0" aria-valuemax="100"
                                aria-valuenow='<%# CourseProgressValue(Eval("ProgressStatus"), Eval("ProgressPercentage")) %>'>
                                <span class="course-progress-fill" style='<%# "width:" + CourseProgressValue(Eval("ProgressStatus"), Eval("ProgressPercentage")) + "%" %>'></span>
                            </div>
                        </asp:Panel>
                        <asp:Panel runat="server" CssClass="course-not-started"
                            Visible='<%# !HasStartedCourse(Eval("ProgressStatus"), Eval("ProgressPercentage")) %>'>
                            <span class="course-not-started-marker" aria-hidden="true"></span>
                            <span><%#: CourseNotStartedMessage() %></span>
                        </asp:Panel>
                        <p class="course-rating"><%#: FormatRating(Eval("AverageRating"), Eval("ReviewCount")) %></p>
                        <div class="course-card-actions">
                            <asp:LinkButton runat="server" Text="View course" CssClass="btn btn-secondary"
                                CommandName="ViewCourse" CommandArgument='<%# Eval("CourseID") %>' CausesValidation="false" />
                        </div>
                    </div>
                </article>
            </ItemTemplate>
            <FooterTemplate></div></FooterTemplate>
        </asp:Repeater>

        <asp:Panel ID="pnlPager" runat="server" CssClass="course-search-pager" Visible="false">
            <asp:Button ID="btnPrevious" runat="server" Text="‹ Previous" CssClass="btn btn-secondary pager-button" OnClick="btnPrevious_Click" CausesValidation="false" />
            <asp:Label ID="lblPage" runat="server" CssClass="pager-status" />
            <asp:Button ID="btnNext" runat="server" Text="Next ›" CssClass="btn btn-secondary pager-button" OnClick="btnNext_Click" CausesValidation="false" />
        </asp:Panel>
    </section>
</asp:Panel>

<asp:Panel ID="pnlCourseOverview" runat="server" Visible="false" CssClass="course-overview-panel">
    <course:ViewCourseOverivew ID="ViewCourseOverivew1" runat="server" />
</asp:Panel>

