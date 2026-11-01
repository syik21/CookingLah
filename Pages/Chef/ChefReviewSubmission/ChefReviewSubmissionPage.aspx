<%@ Page Title="Activity Submissions | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="ChefReviewSubmissionPage.aspx.cs" Inherits="CookingLah.Pages.Chef.ChefReviewSubmission.ChefReviewSubmissionPage" %>
<%@ Import Namespace="System" %>

<asp:Content ID="SubmissionHead" ContentPlaceHolderID="ChefHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Chef/ChefReviewSubmission/ChefReviewSubmissionPage.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="SubmissionContent" ContentPlaceHolderID="ChefContent" runat="server">
    <main class="submission-review-page" aria-labelledby="submissionPageTitle">
        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="submission-message" role="status">
            <asp:Literal ID="litMessage" runat="server" Mode="Encode" />
        </asp:Panel>

        <asp:Panel ID="pnlSubmissionList" runat="server">
            <header class="submission-page-heading">
                <h1 id="submissionPageTitle">Activity Submissions</h1>
                <p>Review practical activities submitted by your learners.</p>
            </header>

            <section class="submission-statistics" aria-label="Submission statistics">
                <article class="submission-stat-card is-submitted">
                    <span>Submitted</span>
                    <strong><asp:Literal ID="litSubmittedCount" runat="server" Mode="Encode" /></strong>
                    <small>Waiting for review</small>
                </article>
                <article class="submission-stat-card is-approved">
                    <span>Approved</span>
                    <strong><asp:Literal ID="litApprovedCount" runat="server" Mode="Encode" /></strong>
                </article>
                <article class="submission-stat-card is-resubmit">
                    <span>Resubmit Required</span>
                    <strong><asp:Literal ID="litResubmitCount" runat="server" Mode="Encode" /></strong>
                </article>
            </section>

            <asp:Panel ID="pnlFilters" runat="server" CssClass="submission-filters" DefaultButton="btnSearch">
                <div class="submission-search-field">
                    <asp:Label runat="server" AssociatedControlID="txtSearch" Text="Search learner or course" />
                    <div class="submission-search-row">
                        <div class="sk-search">
                            <asp:TextBox ID="txtSearch" runat="server" MaxLength="150" placeholder="Search learner or course" />
                        </div>
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" CausesValidation="false" />
                    </div>
                </div>
                <div class="submission-filter-field">
                    <asp:Label runat="server" AssociatedControlID="ddlCourse" Text="Course" />
                    <asp:DropDownList ID="ddlCourse" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FiltersChanged" />
                </div>
                <div class="submission-filter-field">
                    <asp:Label runat="server" AssociatedControlID="ddlStatus" Text="Status" />
                    <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FiltersChanged">
                        <asp:ListItem Text="All Status" Value="" />
                        <asp:ListItem Text="Submitted" Value="Submitted" />
                        <asp:ListItem Text="Approved" Value="Approved" />
                        <asp:ListItem Text="Resubmit Required" Value="Resubmit Required" />
                    </asp:DropDownList>
                </div>
                <div class="submission-filter-field">
                    <asp:Label runat="server" AssociatedControlID="ddlSort" Text="Sort" />
                    <asp:DropDownList ID="ddlSort" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FiltersChanged">
                        <asp:ListItem Text="Newest First" Value="newest" />
                        <asp:ListItem Text="Oldest First" Value="oldest" />
                    </asp:DropDownList>
                </div>
                <asp:LinkButton ID="btnClearFilters" runat="server" Text="Clear Filters" CssClass="submission-clear" OnClick="btnClearFilters_Click" CausesValidation="false" />
            </asp:Panel>

            <asp:Panel ID="pnlSubmissionResults" runat="server" Visible="false">
                <h2 class="submission-result-count"><asp:Literal ID="litResultCount" runat="server" Mode="Encode" /></h2>
                <div class="submission-list">
                    <asp:Repeater ID="rptSubmissions" runat="server" OnItemCommand="rptSubmissions_ItemCommand">
                        <ItemTemplate>
                            <article class="submission-list-card">
                                <asp:Image runat="server" CssClass="submission-card-image" ImageUrl='<%# SubmissionImageUrl(Eval("ImagePath")) %>' AlternateText="Learner activity submission" Visible='<%# !string.IsNullOrEmpty(SubmissionImageUrl(Eval("ImagePath"))) %>' />
                                <div class="submission-list-main">
                                    <h3><%#: Eval("LearnerName") %></h3>
                                    <p class="submission-course"><%#: Eval("CourseTitle") %></p>
                                    <p><%#: Eval("ActivityTitle") %></p>
                                </div>
                                <div class="submission-list-meta">
                                    <time datetime='<%# DateValue(Eval("SubmittedAt")) %>'>Submitted <%#: DisplayDate(Eval("SubmittedAt")) %></time>
                                    <span class='<%# StatusClass(Eval("Status")) %>'><%#: Eval("Status") %></span>
                                </div>
                                <asp:LinkButton runat="server" CssClass="btn btn-secondary submission-open" Text='<%# ActionLabel(Eval("Status")) %>' CommandName="OpenSubmission" CommandArgument='<%# Eval("SubmissionID") %>' CausesValidation="false" />
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <asp:Panel ID="pnlPagination" runat="server" Visible="false" CssClass="submission-pagination">
                    <asp:Button ID="btnPrevious" runat="server" Text="Previous" CssClass="btn btn-secondary" OnClick="btnPrevious_Click" CausesValidation="false" />
                    <asp:Label ID="lblPageStatus" runat="server" />
                    <asp:Button ID="btnNext" runat="server" Text="Next" CssClass="btn btn-secondary" OnClick="btnNext_Click" CausesValidation="false" />
                </asp:Panel>
            </asp:Panel>

            <asp:Panel ID="pnlNoSubmissions" runat="server" Visible="false" CssClass="submission-empty">
                <h2>No activity submissions yet.</h2>
                <p>Learner submissions from your courses will appear here.</p>
            </asp:Panel>
            <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="submission-empty">
                <h2>No submissions found.</h2>
                <p>Try changing your search or filters.</p>
                <asp:Button ID="btnClearEmpty" runat="server" Text="Clear Filters" CssClass="btn btn-secondary" OnClick="btnClearFilters_Click" CausesValidation="false" />
            </asp:Panel>
        </asp:Panel>

        <asp:Panel ID="pnlReviewWorkspace" runat="server" Visible="false" CssClass="submission-workspace">
            <asp:LinkButton ID="btnBackToSubmissions" runat="server" CssClass="submission-back" OnClick="btnBackToSubmissions_Click" CausesValidation="false"><span aria-hidden="true">&#8592;</span> Back to Activity Submissions</asp:LinkButton>
            <header class="submission-workspace-heading">
                <p>Practical activity</p>
                <h1>Review Activity Submission</h1>
            </header>

            <div class="submission-detail-grid">
                <section class="submission-info-card" aria-labelledby="submissionInfoTitle">
                    <h2 id="submissionInfoTitle">Submission Information</h2>
                    <dl>
                        <div><dt>Learner</dt><dd><asp:Literal ID="litLearnerName" runat="server" Mode="Encode" /></dd></div>
                        <div><dt>Course</dt><dd><asp:Literal ID="litCourseTitle" runat="server" Mode="Encode" /></dd></div>
                        <div><dt>Activity</dt><dd><asp:Literal ID="litActivityTitle" runat="server" Mode="Encode" /></dd></div>
                        <div><dt>Submitted</dt><dd><asp:Literal ID="litSubmittedDate" runat="server" Mode="Encode" /></dd></div>
                        <div><dt>Status</dt><dd><asp:Label ID="lblDetailStatus" runat="server" /></dd></div>
                    </dl>
                </section>
                <section class="submission-learner-work" aria-labelledby="learnerWorkTitle">
                    <h2 id="learnerWorkTitle">Learner Submission</h2>
                    <asp:Image ID="imgSubmission" runat="server" CssClass="submission-image" Visible="false" />
                    <asp:Panel ID="pnlNoImage" runat="server" Visible="false" CssClass="submission-no-image">No submitted image is available.</asp:Panel>
                    <h3>Description</h3>
                    <p><asp:Literal ID="litDescription" runat="server" Mode="Encode" /></p>
                </section>
            </div>

            <section class="submission-requirements" aria-labelledby="requirementsTitle">
                <p>Practical activity requirements</p>
                <h2 id="requirementsTitle"><asp:Literal ID="litRequirementTitle" runat="server" Mode="Encode" /></h2>
                <div><asp:Literal ID="litInstructions" runat="server" Mode="Encode" /></div>
            </section>

            <asp:Panel ID="pnlActiveReview" runat="server" Visible="false" CssClass="submission-chef-review">
                <h2>Chef Review</h2>
                <asp:Label runat="server" AssociatedControlID="txtFeedback" Text="Feedback" />
                <asp:TextBox ID="txtFeedback" runat="server" TextMode="MultiLine" Rows="6" MaxLength="2000" placeholder="Write clear, helpful feedback for the learner" />
                <small>Required when requesting resubmission. Maximum 2,000 characters.</small>
                <div class="submission-review-actions">
                    <asp:Button ID="btnRequestResubmission" runat="server" Text="Request Resubmission" CssClass="btn btn-secondary submission-resubmit-action" OnClick="btnRequestResubmission_Click" CausesValidation="false" />
                    <asp:Button ID="btnApprove" runat="server" Text="Approve Activity" CssClass="btn btn-primary" OnClick="btnApprove_Click" CausesValidation="false" OnClientClick="return confirm('Approve this activity?\n\nThe learner activity will be approved and their course will be marked as Completed.');" />
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlReadOnlyReview" runat="server" Visible="false" CssClass="submission-review-state">
                <h2><asp:Literal ID="litReviewStateTitle" runat="server" Mode="Encode" /></h2>
                <p class="submission-state-lead"><asp:Literal ID="litReviewStateMessage" runat="server" Mode="Encode" /></p>
                <div class="submission-saved-feedback">
                    <strong>Chef Feedback</strong>
                    <p><asp:Literal ID="litSavedFeedback" runat="server" Mode="Encode" /></p>
                </div>
                <asp:Panel ID="pnlCompletedProgress" runat="server" Visible="false" CssClass="submission-completed-progress">
                    <strong>Course Progress</strong>
                    <span>100% &bull; Completed</span>
                </asp:Panel>
            </asp:Panel>
        </asp:Panel>
    </main>
</asp:Content>

<asp:Content ID="SubmissionScripts" ContentPlaceHolderID="ChefScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Chef/ChefReviewSubmission/ChefReviewSubmissionPage.js") %>" defer></script>
</asp:Content>
