<%@ Page Title="My Activity Submissions | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Learner/Learner.Master" AutoEventWireup="true" CodeBehind="LearnerMyActivityPage.aspx.cs" Inherits="CookingLah.Pages.Learner.LearnMyActivity.LearnerMyActivityPage" %>
<%@ Import Namespace="System" %>

<asp:Content
    ID="ActivityHead"
    ContentPlaceHolderID="LearnerHead"
    runat="server">
    <link href="<%= ResolveUrl("~/Pages/Learner/LearnMyActivity/LearnerMyActivityPage.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content
    ID="ActivityContent"
    ContentPlaceHolderID="LearnerContent"
    runat="server">
    <div class="my-activity-page">
        <asp:Panel
            ID="pnlMessage"
            runat="server"
            Visible="false"
            CssClass="activity-page-message"
            role="alert"><asp:Literal
                ID="litMessage"
                runat="server"
                Mode="Encode" /></asp:Panel>

        <asp:Panel ID="pnlSubmissionList" runat="server">
            <header class="activity-page-heading">
                <p class="activity-kicker">Your practical work</p>
                <h1>My Activity Submissions</h1>
                <p>Track your practical activities, submission status, and Chef feedback.</p>
            </header>

            <section class="activity-statistics" aria-label="Activity submission statistics">
                <article class="activity-stat-card is-submitted"><span>Submitted</span><strong><asp:Literal
                            ID="litSubmittedCount"
                            runat="server"
                            Mode="Encode" /></strong></article>
                <article class="activity-stat-card is-approved"><span>Approved</span><strong><asp:Literal
                            ID="litApprovedCount"
                            runat="server"
                            Mode="Encode" /></strong></article>
                <article class="activity-stat-card is-resubmit"><span>Resubmit Required</span><strong><asp:Literal
                            ID="litResubmitCount"
                            runat="server"
                            Mode="Encode" /></strong></article>
            </section>

            <asp:Panel
                ID="pnlFilters"
                runat="server"
                CssClass="activity-filters"
                DefaultButton="btnSearch">
                <div class="activity-search-field">
                    <label for="<%= txtSearch.ClientID %>">Search by course or activity</label>
                    <%-- In LearnerMyActivityPage.aspx, replace the whole <div class="activity-search-row"> ... </div> with this. --%>
<%-- In LearnerMyActivityPage.aspx, replace the whole <div class="activity-search-row"> ... </div> with this. --%>
                    <div class="activity-search-row">
                        <div class="sketch-search">
                            <span class="sketch-plus" aria-hidden="true">
                                <svg viewBox="0 0 24 24"><path d="M12.2 4.5 C12 9 12.3 15 11.8 19.6"/><path d="M4.4 12.3 C9 11.8 15 12.4 19.7 11.7"/></svg>
                            </span>
                            <asp:TextBox
                                ID="txtSearch"
                                runat="server"
                                TextMode="Search"
                                MaxLength="100"
                                placeholder="Search by course..." />
                        </div>
                        <asp:Button
                            ID="btnSearch"
                            runat="server"
                            Text="Search"
                            CssClass="btn btn-primary"
                            OnClick="btnSearch_Click"
                            CausesValidation="false" />
                    </div>
                </div>
                <div class="activity-filter-field">
                    <label for="<%= ddlStatus.ClientID %>">Status</label>
                    <asp:DropDownList
                        ID="ddlStatus"
                        runat="server"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="FiltersChanged">
                        <asp:ListItem Text="All Status" Value="" /><asp:ListItem Text="Submitted" Value="Submitted" /><asp:ListItem Text="Approved" Value="Approved" /><asp:ListItem Text="Resubmit Required" Value="Resubmit Required" />
                    </asp:DropDownList>
                </div>
                <div class="activity-filter-field">
                    <label for="<%= ddlSort.ClientID %>">Sort</label>
                    <asp:DropDownList
                        ID="ddlSort"
                        runat="server"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="FiltersChanged"><asp:ListItem Text="Newest First" Value="newest" /><asp:ListItem Text="Oldest First" Value="oldest" /></asp:DropDownList>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlSubmissionResults" runat="server">
                <div class="activity-results-heading">
                    <h2><asp:Literal
                            ID="litResultCount"
                            runat="server"
                            Mode="Encode" /></h2>
                </div>
                <div class="activity-submission-list">
                    <asp:Repeater ID="rptSubmissions"
                        runat="server"
                        OnItemCommand="rptSubmissions_ItemCommand">
                        <ItemTemplate>
                            <article class='<%# CardClass(Eval("Status")) %>'>
                                <div class="submission-thumbnail-wrap">
                                    <asp:Image
                                        runat="server"
                                        CssClass="submission-thumbnail"
                                        ImageUrl='<%# Media(Eval("Thumbnail")) %>'
                                        Visible='<%# !string.IsNullOrEmpty(Media(Eval("Thumbnail"))) %>'
                                        AlternateText='<%# Convert.ToString(Eval("CourseTitle")) + " course thumbnail" %>'
                                        loading="lazy" />
                                    <asp:Panel
                                        runat="server"
                                        CssClass="submission-thumbnail-empty course-image-placeholder"
                                        Visible='<%# string.IsNullOrEmpty(Media(Eval("Thumbnail"))) %>'><img runat="server" src="~/F_Assets/mascot.png" alt="" /><span>Food Image Coming Soon</span></asp:Panel>
                                </div>
                                <div class="submission-card-content">
                                    <div class="submission-card-heading">
                                        <div>
                                            <p class="submission-course"><%#: Eval("CourseTitle") %></p>
                                            <h3><%#: Eval("ActivityTitle") %></h3>
                                        </div><span class='<%# StatusClass(Eval("Status")) %>'><%#: Eval("Status") %></span></div>
                                    <p class="submission-date">Submitted on <time datetime='<%# DateValue(Eval("SubmittedAt")) %>'><%#: DisplayDate(Eval("SubmittedAt")) %></time></p>
                                    <p class="submission-status-message"><%#: StatusMessage(Eval("Status")) %></p>
                                    <div class="submission-card-action"><asp:LinkButton
                                            runat="server"
                                            CommandName="OpenSubmission"
                                            CommandArgument='<%# Eval("SubmissionID") %>'
                                            Text='<%# ActionLabel(Eval("Status")) %>'
                                            CssClass='<%# ActionClass(Eval("Status")) %>'
                                            CausesValidation="false" /></div>
                                </div>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>

            <asp:Panel
                ID="pnlNoSubmissions"
                runat="server"
                Visible="false"
                CssClass="activity-empty-state">
                <h2>No activity submissions yet</h2>
                <p>Your practical activity submissions will appear here after you submit them through a course.</p><a class="btn btn-primary" href="<%= ResolveUrl("~/Pages/Learner/MyCourse/LearnerMyCourse.aspx") %>">My Courses</a></asp:Panel>
            <asp:Panel
                ID="pnlNoResults"
                runat="server"
                Visible="false"
                CssClass="activity-empty-state">
                <h2>No submissions found</h2>
                <p>Try changing your search or status filter.</p><asp:Button
                    ID="btnClearFilters"
                    runat="server"
                    Text="Clear Filters"
                    CssClass="btn"
                    OnClick="btnClearFilters_Click"
                    CausesValidation="false" /></asp:Panel>
            <asp:Panel
                ID="pnlPagination"
                runat="server"
                Visible="false"
                CssClass="activity-pagination"><asp:Button
                    ID="btnPrevious"
                    runat="server"
                    Text="Previous"
                    CssClass="btn"
                    OnClick="btnPrevious_Click"
                    CausesValidation="false" /><asp:Label ID="lblPageStatus" runat="server" /><asp:Button
                    ID="btnNext"
                    runat="server"
                    Text="Next"
                    CssClass="btn"
                    OnClick="btnNext_Click"
                    CausesValidation="false" /></asp:Panel>
        </asp:Panel>

        <asp:Panel
            ID="pnlSubmissionDetail"
            runat="server"
            Visible="false"
            CssClass="activity-detail"
            data-activity-detail="">
            <asp:LinkButton
                ID="btnBackToSubmissions"
                runat="server"
                CssClass="activity-back-link"
                OnClick="btnBackToSubmissions_Click"
                CausesValidation="false">&larr; Back to My Activity Submissions</asp:LinkButton>
            <header class="activity-detail-heading" tabindex="-1" data-detail-heading="">
                <p class="activity-kicker">Activity submission</p>
                <h1><asp:Literal
                        ID="litDetailActivityTitle"
                        runat="server"
                        Mode="Encode" /></h1>
                <p><asp:Literal
                        ID="litDetailCourseTitle"
                        runat="server"
                        Mode="Encode" /></p>
            </header>
            <section class="activity-detail-summary" aria-label="Submission information">
                <div><span>Submitted</span><strong><asp:Literal
                            ID="litDetailDate"
                            runat="server"
                            Mode="Encode" /></strong></div>
                <div><span>Status</span><asp:Label ID="lblDetailStatus" runat="server" /></div>
                <asp:Panel
                    ID="pnlCompletedCourse"
                    runat="server"
                    Visible="false"
                    CssClass="activity-progress-confirmation">
                    <div><span>Course Status</span><strong><asp:Literal
                                ID="litCourseStatus"
                                runat="server"
                                Mode="Encode" /></strong></div>
                    <div><span>Course Progress</span><strong><asp:Literal
                                ID="litCourseProgress"
                                runat="server"
                                Mode="Encode" />%</strong></div>
                </asp:Panel>
            </section>
            <asp:Panel
                ID="pnlDetailState"
                runat="server"
                CssClass="activity-detail-state">
                <h2><asp:Literal
                        ID="litDetailStateTitle"
                        runat="server"
                        Mode="Encode" /></h2>
                <p><asp:Literal
                        ID="litDetailStateMessage"
                        runat="server"
                        Mode="Encode" /></p>
            </asp:Panel>
            <div class="activity-detail-grid">
                <section class="activity-detail-card">
                    <h2>Your Submission</h2><asp:Image
                        ID="imgSubmission"
                        runat="server"
                        CssClass="activity-submission-image" /><asp:Panel
                        ID="pnlNoImage"
                        runat="server"
                        Visible="false"
                        CssClass="activity-no-image">No submission image is available.</asp:Panel>
                    <h3>Description</h3>
                    <p class="activity-copy"><asp:Literal
                            ID="litDescription"
                            runat="server"
                            Mode="Encode" /></p>
                </section>
                <section class="activity-detail-card activity-feedback-card">
                    <h2>Chef Feedback</h2>
                    <p class="activity-copy"><asp:Literal
                            ID="litChefFeedback"
                            runat="server"
                            Mode="Encode" /></p>
                </section>
            </div>
            <div class="activity-detail-actions"><asp:HyperLink
                    ID="lnkCourseOverview"
                    runat="server"
                    CssClass="btn">View Course</asp:HyperLink><asp:HyperLink
                    ID="lnkCourseLearning"
                    runat="server"
                    CssClass="btn btn-primary">Review Course Content</asp:HyperLink><asp:HyperLink
                    ID="lnkResubmit"
                    runat="server"
                    CssClass="btn btn-primary"
                    Visible="false">Resubmit Activity</asp:HyperLink></div>
        </asp:Panel>
    </div>
</asp:Content>

<asp:Content
    ID="ActivityScripts"
    ContentPlaceHolderID="LearnerScripts"
    runat="server"><script src="<%= ResolveUrl("~/Pages/Learner/LearnMyActivity/LearnerMyActivityPage.js") %>" defer></script></asp:Content>
