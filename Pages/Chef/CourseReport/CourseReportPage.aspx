<%@ Page Title="Course Report | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="CourseReportPage.aspx.cs" Inherits="CookingLah.Pages.Chef.CourseReport.CourseReportPage" %>
<asp:Content
    ID="CourseReportHead"
    ContentPlaceHolderID="ChefHead"
    runat="server">
    <link href="<%= ResolveUrl("~/Pages/Chef/CourseReport/CourseReportPage.css") %>?v=<%= System.IO.File.GetLastWriteTimeUtc(Server.MapPath("~/Pages/Chef/CourseReport/CourseReportPage.css")).Ticks %>" rel="stylesheet" />
</asp:Content>
<asp:Content
    ID="CourseReportContent"
    ContentPlaceHolderID="ChefContent"
    runat="server">
    <div class="course-report">
        <header class="cr-page-heading">
            <p class="cr-eyebrow">LEARNING / ANALYTICS</p>
            <h1>Course report</h1>
            <p>Analyse course creation, enrolment and learning performance.</p>
        </header>

        <asp:Panel
            ID="pnlError"
            runat="server"
            CssClass="cr-message"
            role="alert"
            Visible="false">
            <asp:Literal ID="litError" runat="server" />
        </asp:Panel>

        <asp:Panel ID="pnlReport" runat="server">
            <section class="cr-filter-panel" aria-labelledby="courseFilterHeading">
                <div>
                    <p class="cr-eyebrow" id="courseFilterHeading">REPORT FILTERS</p>
                    <p class="cr-filter-help">Narrow the courses and period shown below.</p>
                </div>
                <div class="cr-filters">
                    <div class="cr-field">
                        <label for="<%= ddlDateRange.ClientID %>">Date range</label>
                        <asp:DropDownList
                            ID="ddlDateRange"
                            runat="server"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="DateRange_Changed">
                            <asp:ListItem Value="30">Last 30 Days</asp:ListItem>
                            <asp:ListItem Value="3m">Last 3 Months</asp:ListItem>
                            <asp:ListItem Value="6m">Last 6 Months</asp:ListItem>
                            <asp:ListItem Value="year">This Year</asp:ListItem>
                            <asp:ListItem Value="all" Selected="True">All Time</asp:ListItem>
                            <asp:ListItem Value="custom">Custom Year / Month</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="cr-field">
                        <label for="<%= ddlYear.ClientID %>">Year</label>
                        <asp:DropDownList
                            ID="ddlYear"
                            runat="server"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="CalendarFilter_Changed" />
                    </div>
                    <div class="cr-field">
                        <label for="<%= ddlMonth.ClientID %>">Month</label>
                        <asp:DropDownList
                            ID="ddlMonth"
                            runat="server"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="CalendarFilter_Changed">
                            <asp:ListItem Value="">All Months</asp:ListItem>
                            <asp:ListItem Value="1">January</asp:ListItem>
                            <asp:ListItem Value="2">February</asp:ListItem>
                            <asp:ListItem Value="3">March</asp:ListItem>
                            <asp:ListItem Value="4">April</asp:ListItem>
                            <asp:ListItem Value="5">May</asp:ListItem>
                            <asp:ListItem Value="6">June</asp:ListItem>
                            <asp:ListItem Value="7">July</asp:ListItem>
                            <asp:ListItem Value="8">August</asp:ListItem>
                            <asp:ListItem Value="9">September</asp:ListItem>
                            <asp:ListItem Value="10">October</asp:ListItem>
                            <asp:ListItem Value="11">November</asp:ListItem>
                            <asp:ListItem Value="12">December</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="cr-field">
                        <label for="<%= ddlCategory.ClientID %>">Category</label>
                        <asp:DropDownList
                            ID="ddlCategory"
                            runat="server"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="Filter_Changed" />
                    </div>
                    <div class="cr-field">
                        <label for="<%= ddlStatus.ClientID %>">Course status</label>
                        <asp:DropDownList
                            ID="ddlStatus"
                            runat="server"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="Filter_Changed">
                            <asp:ListItem Value="">All Status</asp:ListItem>
                            <asp:ListItem Value="Draft">Draft</asp:ListItem>
                            <asp:ListItem Value="Pending">Pending</asp:ListItem>
                            <asp:ListItem Value="Published">Published</asp:ListItem>
                            <asp:ListItem Value="Rejected">Rejected</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <asp:LinkButton
                        ID="btnClearTop"
                        runat="server"
                        CssClass="cr-clear-link"
                        OnClick="ClearFilters_Click"
                        CausesValidation="false">Clear filters</asp:LinkButton>
                </div>
            </section>

            <section class="cr-section" aria-labelledby="courseSummaryHeading">
                <div class="cr-section-heading">
                    <div>
                        <p class="cr-eyebrow">AT A GLANCE</p>
                        <h2 id="courseSummaryHeading">Summary</h2>
                    </div>
                    <span class="cr-period"><asp:Literal ID="litPeriodLabel" runat="server" /></span>
                </div>
                <div class="cr-summary-grid">
                    <article class="cr-summary-card cr-summary-total"><span>Total courses</span><strong><asp:Literal ID="litTotalCourses" runat="server" /></strong><small>Matching report filters</small></article>
                    <article class="cr-summary-card cr-summary-published"><span>Published courses</span><strong><asp:Literal ID="litPublishedCourses" runat="server" /></strong><small>Ready for learners</small></article>
                    <article class="cr-summary-card cr-summary-enrolments"><span>Total enrolments</span><strong><asp:Literal ID="litTotalEnrolments" runat="server" /></strong><small>Course progress records</small></article>
                    <article class="cr-summary-card cr-summary-rating"><span>Average rating</span><strong><asp:Literal ID="litAverageRating" runat="server" /></strong><small><asp:Literal ID="litReviewSummary" runat="server" /></small></article>
                </div>
            </section>

            <section class="cr-panel cr-trend-panel" aria-labelledby="creationTrendHeading">
                <div class="cr-section-heading">
                    <div>
                        <p class="cr-eyebrow">COURSE GROWTH</p>
                        <h2 id="creationTrendHeading">Course creation trend</h2>
                    </div>
                    <span class="cr-chart-legend"><i aria-hidden="true"></i> Courses created</span>
                </div>
                <asp:HiddenField ID="hfTrendData" runat="server" />
                <div class="cr-chart-scroll">
                    <div
                        id="courseCreationChart"
                        class="cr-chart"
                        role="img"
                        aria-label="Courses created over time">
                    </div>
                </div>
                <noscript><p class="cr-chart-note">Enable JavaScript to display the course creation chart.</p></noscript>
            </section>

            <div class="cr-two-column">
                <section class="cr-panel" aria-labelledby="courseStatusHeading">
                    <div class="cr-section-heading">
                        <div>
                            <p class="cr-eyebrow">WORKFLOW</p>
                            <h2 id="courseStatusHeading">Course status</h2>
                        </div>
                    </div>
                    <div class="cr-bar-list">
                        <asp:Repeater ID="rptCourseStatus" runat="server">
                            <ItemTemplate>
                                <div class="cr-bar-row">
                                    <div class="cr-bar-label"><span><%#: Eval("Status") %></span><strong><%#: Eval("CourseCount") %></strong></div>
                                    <div class="cr-bar-track" aria-hidden="true"><span style='<%# "width:" + Eval("BarPercent") + "%" %>'></span></div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </section>

                <section class="cr-panel" aria-labelledby="enrolmentHeading">
                    <div class="cr-section-heading">
                        <div>
                            <p class="cr-eyebrow">LEARNER PROGRESS</p>
                            <h2 id="enrolmentHeading">Enrolment &amp; completion</h2>
                        </div>
                    </div>
                    <dl class="cr-metric-list">
                        <div>
                            <dt>Total enrolments</dt>
                            <dd><asp:Literal ID="litEnrolmentTotal" runat="server" /></dd>
                        </div>
                        <div>
                            <dt>Completed</dt>
                            <dd><asp:Literal ID="litCompleted" runat="server" /></dd>
                        </div>
                        <div>
                            <dt>In progress</dt>
                            <dd><asp:Literal ID="litInProgress" runat="server" /></dd>
                        </div>
                        <div>
                            <dt>Submitted / waiting approval</dt>
                            <dd><asp:Literal ID="litSubmitted" runat="server" /></dd>
                        </div>
                        <div class="cr-rate">
                            <dt>Completion rate</dt>
                            <dd><asp:Literal ID="litCompletionRate" runat="server" /></dd>
                        </div>
                    </dl>
                </section>
            </div>

            <div class="cr-two-column">
                <section class="cr-panel" aria-labelledby="mostEnrolledHeading">
                    <div class="cr-section-heading">
                        <div>
                            <p class="cr-eyebrow">POPULAR COURSES</p>
                            <h2 id="mostEnrolledHeading">Most enrolled courses</h2>
                        </div><span class="cr-period">Top 5</span></div>
                    <asp:Panel
                        ID="pnlMostEnrolled"
                        runat="server"
                        CssClass="cr-ranking-list">
                        <div class="cr-ranking-header" aria-hidden="true"><span>Course</span><span>Enrolments</span></div>
                        <asp:Repeater ID="rptMostEnrolled" runat="server">
                            <ItemTemplate>
                                <div class="cr-ranking-row"><strong><%#: Eval("Title") %></strong><span><%#: Eval("EnrolmentCount") %></span></div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </asp:Panel>
                    <asp:Panel
                        ID="pnlNoEnrolments"
                        runat="server"
                        CssClass="cr-inline-empty"
                        Visible="false">No course enrolments for this selection.</asp:Panel>
                </section>

                <section class="cr-panel" aria-labelledby="topRatedHeading">
                    <div class="cr-section-heading">
                        <div>
                            <p class="cr-eyebrow">LEARNER FEEDBACK</p>
                            <h2 id="topRatedHeading">Top rated courses</h2>
                        </div><span class="cr-period">Top 5</span></div>
                    <asp:Panel
                        ID="pnlTopRated"
                        runat="server"
                        CssClass="cr-ranking-list">
                        <div class="cr-rating-header" aria-hidden="true"><span>Course</span><span>Rating</span><span>Reviews</span></div>
                        <asp:Repeater ID="rptTopRated" runat="server">
                            <ItemTemplate>
                                <div class="cr-rating-row"><strong><%#: Eval("Title") %></strong><span>&#9733; <%#: FormatRating(Eval("AverageRating")) %></span><span><%#: Eval("ReviewCount") %></span></div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </asp:Panel>
                    <asp:Panel
                        ID="pnlNoRatings"
                        runat="server"
                        CssClass="cr-inline-empty"
                        Visible="false">No course reviews for this selection.</asp:Panel>
                </section>
            </div>

            <section class="cr-panel cr-performance-panel" aria-labelledby="performanceHeading">
                <div class="cr-section-heading cr-performance-heading">
                    <div>
                        <p class="cr-eyebrow">DETAILED REPORT</p>
                        <h2 id="performanceHeading">Course performance</h2>
                    </div>
                    <asp:Label
                        ID="lblResultCount"
                        runat="server"
                        CssClass="cr-count"
                        aria-live="polite" />
                </div>
                <asp:Panel
                    ID="pnlSearch"
                    runat="server"
                    CssClass="cr-search"
                    DefaultButton="btnSearch">
                    <div class="cr-field">
                        <label for="<%= txtSearch.ClientID %>">Search course</label>
                        <div class="cr-search-row">
                            <div class="sk-search">
                                <asp:TextBox
                                    ID="txtSearch"
                                    runat="server"
                                    MaxLength="100"
                                    placeholder="Search course..." />
                            </div>
                            <asp:Button
                                ID="btnSearch"
                                runat="server"
                                Text="Search"
                                CssClass="btn btn-secondary"
                                OnClick="Search_Click" />
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel
                    ID="pnlCourseTable"
                    runat="server"
                    CssClass="cr-table-scroll">
                    <table class="cr-table">
                        <thead>
                            <tr>
                                <th scope="col">Course</th>
                                <th scope="col">Status</th>
                                <th scope="col">Enrolments</th>
                                <th scope="col">Completed</th>
                                <th scope="col">Completion rate</th>
                                <th scope="col">Rating</th>
                                <th scope="col">Reviews</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptCoursePerformance" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td><strong><%#: Eval("Title") %></strong><small><%#: Eval("CategoryName") %></small></td>
                                        <td><span class='<%# GetStatusCss(Eval("Status")) %>'><%#: Eval("Status") %></span></td>
                                        <td><%#: Eval("EnrolmentCount") %></td>
                                        <td><%#: Eval("CompletedCount") %></td>
                                        <td><%#: FormatCompletionRate(Eval("EnrolmentCount"), Eval("CompletedCount")) %></td>
                                        <td><%#: FormatCourseRating(Eval("ReviewCount"), Eval("AverageRating")) %></td>
                                        <td><%#: Eval("ReviewCount") %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </asp:Panel>

                <asp:Panel
                    ID="pnlEmpty"
                    runat="server"
                    CssClass="cr-empty"
                    Visible="false">
                    <h3>No courses found.</h3>
                    <p>Try changing the search or report filters.</p>
                    <asp:Button
                        ID="btnClearEmpty"
                        runat="server"
                        Text="Clear Filters"
                        CssClass="btn btn-secondary"
                        OnClick="ClearFilters_Click"
                        CausesValidation="false" />
                </asp:Panel>

                <asp:Panel
                    ID="pnlPagination"
                    runat="server"
                    CssClass="cr-pagination">
                    <asp:Button
                        ID="btnPrevious"
                        runat="server"
                        Text="< Previous"
                        CssClass="btn btn-secondary"
                        OnClick="Previous_Click"
                        CausesValidation="false" />
                    <asp:Label ID="lblPage" runat="server" />
                    <asp:Button
                        ID="btnNext"
                        runat="server"
                        Text="Next >"
                        CssClass="btn btn-secondary"
                        OnClick="Next_Click"
                        CausesValidation="false" />
                </asp:Panel>
            </section>
        </asp:Panel>
    </div>
</asp:Content>
<asp:Content
    ID="CourseReportScripts"
    ContentPlaceHolderID="ChefScripts"
    runat="server">
    <script src="<%= ResolveUrl("~/Pages/Chef/CourseReport/CourseReportPage.js") %>?v=<%= System.IO.File.GetLastWriteTimeUtc(Server.MapPath("~/Pages/Chef/CourseReport/CourseReportPage.js")).Ticks %>" defer></script>
</asp:Content>
