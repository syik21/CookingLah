<%@ Page Title="User Report | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="UserReportPage.aspx.cs" Inherits="CookingLah.Pages.Admin.UserReport.UserReportPage" %>
<asp:Content ID="UserReportHead" ContentPlaceHolderID="AdminHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Admin/UserReport/UserReportPage.css") %>?v=<%= System.IO.File.GetLastWriteTimeUtc(Server.MapPath("~/Pages/Admin/UserReport/UserReportPage.css")).Ticks %>" rel="stylesheet" />
</asp:Content>
<asp:Content ID="UserReportContent" ContentPlaceHolderID="AdminContent" runat="server">
    <div class="user-report">
        <header class="ur-page-heading">
            <p class="ur-eyebrow">COMMUNITY / ANALYTICS</p>
            <h1>User report</h1>
            <p>Analyse user registration and system activity.</p>
        </header>

        <asp:Panel ID="pnlError" runat="server" CssClass="ur-message" role="alert" Visible="false">
            <asp:Literal ID="litError" runat="server" />
        </asp:Panel>

        <asp:Panel ID="pnlReport" runat="server">
            <section class="ur-filter-panel" aria-labelledby="filterHeading">
                <div>
                    <p class="ur-eyebrow" id="filterHeading">REPORT FILTERS</p>
                    <p class="ur-filter-help">Adjust the period and user group shown below.</p>
                </div>
                <div class="ur-filters">
                    <div class="ur-field">
                        <label for="<%= ddlDateRange.ClientID %>">Date range</label>
                        <asp:DropDownList ID="ddlDateRange" runat="server" AutoPostBack="true" OnSelectedIndexChanged="DateRange_Changed">
                            <asp:ListItem Value="7">Last 7 Days</asp:ListItem>
                            <asp:ListItem Value="30" Selected="True">Last 30 Days</asp:ListItem>
                            <asp:ListItem Value="3m">Last 3 Months</asp:ListItem>
                            <asp:ListItem Value="6m">Last 6 Months</asp:ListItem>
                            <asp:ListItem Value="year">This Year</asp:ListItem>
                            <asp:ListItem Value="all">All Time</asp:ListItem>
                            <asp:ListItem Value="custom">Custom Year / Month</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="ur-field">
                        <label for="<%= ddlYear.ClientID %>">Year</label>
                        <asp:DropDownList ID="ddlYear" runat="server" AutoPostBack="true" OnSelectedIndexChanged="CalendarFilter_Changed" />
                    </div>
                    <div class="ur-field">
                        <label for="<%= ddlMonth.ClientID %>">Month</label>
                        <asp:DropDownList ID="ddlMonth" runat="server" AutoPostBack="true" OnSelectedIndexChanged="CalendarFilter_Changed">
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
                    <div class="ur-field">
                        <label for="<%= ddlRole.ClientID %>">Role</label>
                        <asp:DropDownList ID="ddlRole" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                    </div>
                    <div class="ur-field">
                        <label for="<%= ddlStatus.ClientID %>">Account status</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                    </div>
                    <asp:LinkButton ID="btnClearTop" runat="server" CssClass="ur-clear-link" OnClick="ClearFilters_Click" CausesValidation="false">Clear filters</asp:LinkButton>
                </div>
            </section>

            <section class="ur-section" aria-labelledby="summaryHeading">
                <div class="ur-section-heading">
                    <div><p class="ur-eyebrow">AT A GLANCE</p><h2 id="summaryHeading">Summary</h2></div>
                    <span class="ur-period"><asp:Literal ID="litPeriodLabel" runat="server" /></span>
                </div>
                <div class="ur-summary-grid">
                    <article class="ur-summary-card ur-summary-total"><span>Total users</span><strong><asp:Literal ID="litTotalUsers" runat="server" /></strong><small>Matching role and status</small></article>
                    <article class="ur-summary-card ur-summary-new"><span>New users</span><strong><asp:Literal ID="litNewUsers" runat="server" /></strong><small><asp:Literal ID="litNewUsersPeriod" runat="server" /></small></article>
                    <article class="ur-summary-card ur-summary-active"><span>Active users</span><strong><asp:Literal ID="litActiveUsers" runat="server" /></strong><small>Logged in during period</small></article>
                    <article class="ur-summary-card ur-summary-inactive"><span>Inactive users</span><strong><asp:Literal ID="litInactiveUsers" runat="server" /></strong><small>No login during period</small></article>
                </div>
            </section>

            <section class="ur-panel ur-trend-panel" aria-labelledby="trendHeading">
                <div class="ur-section-heading">
                    <div><p class="ur-eyebrow">USER GROWTH</p><h2 id="trendHeading">User registration trend</h2></div>
                    <span class="ur-chart-legend"><i aria-hidden="true"></i> New users</span>
                </div>
                <asp:HiddenField ID="hfTrendData" runat="server" />
                <div class="ur-chart-scroll"><div id="registrationChart" class="ur-chart" role="img" aria-label="New user registrations over time"></div></div>
                <noscript><p class="ur-chart-note">Enable JavaScript to display the registration chart.</p></noscript>
            </section>

            <div class="ur-two-column">
                <section class="ur-panel" aria-labelledby="distributionHeading">
                    <div class="ur-section-heading"><div><p class="ur-eyebrow">MEMBERSHIP</p><h2 id="distributionHeading">User distribution</h2></div></div>
                    <div class="ur-distribution-list">
                        <asp:Repeater ID="rptRoleDistribution" runat="server">
                            <ItemTemplate>
                                <div class="ur-distribution-row">
                                    <div class="ur-distribution-label"><span><%#: Eval("Role") %></span><strong><%#: Eval("UserCount") %></strong></div>
                                    <div class="ur-distribution-track" aria-hidden="true"><span style='<%# "width:" + Eval("BarPercent") + "%" %>'></span></div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </section>

                <section class="ur-panel" aria-labelledby="loginHeading">
                    <div class="ur-section-heading"><div><p class="ur-eyebrow">UNIQUE USERS</p><h2 id="loginHeading">Login activity</h2></div></div>
                    <dl class="ur-login-list">
                        <div><dt>Active today</dt><dd><asp:Literal ID="litActiveToday" runat="server" /></dd></div>
                        <div><dt>Active last 7 days</dt><dd><asp:Literal ID="litActiveSevenDays" runat="server" /></dd></div>
                        <div><dt>Active last 30 days</dt><dd><asp:Literal ID="litActiveThirtyDays" runat="server" /></dd></div>
                    </dl>
                </section>
            </div>

            <section class="ur-panel" aria-labelledby="instructorHeading">
                <div class="ur-section-heading">
                    <div><p class="ur-eyebrow">CONTRIBUTION</p><h2 id="instructorHeading">Instructor activity</h2></div>
                    <span class="ur-period">Top 5</span>
                </div>
                <asp:Panel ID="pnlInstructorList" runat="server" CssClass="ur-instructor-list">
                    <div class="ur-instructor-header" aria-hidden="true"><span>Instructor</span><span>Courses created</span></div>
                    <asp:Repeater ID="rptInstructorActivity" runat="server">
                        <ItemTemplate>
                            <div class="ur-instructor-row">
                                <div><strong><%#: Eval("Fullname") %></strong><small>@<%#: Eval("Username") %></small></div>
                                <span><strong><%#: Eval("CourseCount") %></strong> <%# Convert.ToInt32(Eval("CourseCount")) == 1 ? "Course" : "Courses" %></span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>
                <asp:Panel ID="pnlNoInstructorActivity" runat="server" CssClass="ur-inline-empty" Visible="false">No instructor course activity yet.</asp:Panel>
            </section>

            <section class="ur-panel ur-activity-panel" aria-labelledby="activityHeading">
                <div class="ur-section-heading ur-activity-heading">
                    <div><p class="ur-eyebrow">DETAILED REPORT</p><h2 id="activityHeading">User activity</h2></div>
                    <asp:Label ID="lblResultCount" runat="server" CssClass="ur-count" aria-live="polite" />
                </div>
                <asp:Panel ID="pnlSearch" runat="server" CssClass="ur-search" DefaultButton="btnSearch">
                    <div class="ur-field">
                        <label for="<%= txtSearch.ClientID %>">Search user</label>
                        <div class="ur-search-row">
                            <asp:TextBox ID="txtSearch" runat="server" MaxLength="100" placeholder="Search username, email or name..." />
                            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-secondary" OnClick="Search_Click" />
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlUserTable" runat="server" CssClass="ur-table-scroll">
                    <table class="ur-table">
                        <thead><tr><th scope="col">User</th><th scope="col">Role</th><th scope="col">Joined date</th><th scope="col">Last login</th><th scope="col">Active days</th><th scope="col">Account status</th></tr></thead>
                        <tbody>
                            <asp:Repeater ID="rptUserActivity" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td><strong><%#: Eval("Fullname") %></strong><small>@<%#: Eval("Username") %><br /><%#: Eval("Email") %></small></td>
                                        <td><%#: Eval("Role") %></td>
                                        <td><%#: FormatJoinedDate(Eval("CreatedAt")) %></td>
                                        <td><%#: FormatLastLogin(Eval("LastLogin")) %></td>
                                        <td><strong><%#: Eval("ActiveDays") %></strong></td>
                                        <td><span class='<%# GetStatusCss(Eval("AccountStatus")) %>'><%#: Eval("AccountStatus") %></span></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </asp:Panel>

                <asp:Panel ID="pnlEmpty" runat="server" CssClass="ur-empty" Visible="false">
                    <h3>No users found.</h3><p>Try changing the search or filters.</p>
                    <asp:Button ID="btnClearEmpty" runat="server" Text="Clear Filters" CssClass="btn btn-secondary" OnClick="ClearFilters_Click" CausesValidation="false" />
                </asp:Panel>

                <asp:Panel ID="pnlPagination" runat="server" CssClass="ur-pagination">
                    <asp:Button ID="btnPrevious" runat="server" Text="‹ Previous" CssClass="btn btn-secondary" OnClick="Previous_Click" CausesValidation="false" />
                    <asp:Label ID="lblPage" runat="server" />
                    <asp:Button ID="btnNext" runat="server" Text="Next ›" CssClass="btn btn-secondary" OnClick="Next_Click" CausesValidation="false" />
                </asp:Panel>
            </section>
        </asp:Panel>
    </div>
</asp:Content>
<asp:Content ID="UserReportScripts" ContentPlaceHolderID="AdminScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Admin/UserReport/UserReportPage.js") %>?v=<%= System.IO.File.GetLastWriteTimeUtc(Server.MapPath("~/Pages/Admin/UserReport/UserReportPage.js")).Ticks %>" defer></script>
</asp:Content>
