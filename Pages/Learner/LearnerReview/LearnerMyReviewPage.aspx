<%@ Page Title="My Reviews | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Learner/Learner.Master" AutoEventWireup="true" CodeBehind="LearnerMyReviewPage.aspx.cs" Inherits="CookingLah.Pages.Learner.LearnerReview.LearnerMyReviewPage" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Web.UI.WebControls" %>

<asp:Content ID="MyReviewsHead" ContentPlaceHolderID="LearnerHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Learner/LearnerReview/LearnerMyReviewPage.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="MyReviewsContent" ContentPlaceHolderID="LearnerContent" runat="server">
    <main class="my-reviews-page" aria-labelledby="myReviewsTitle">
        <header class="my-reviews-heading">
            <h1 id="myReviewsTitle">My Reviews</h1>
            <p>Manage the reviews you&rsquo;ve shared for completed courses.</p>
        </header>

        <section class="review-statistics" aria-label="Review statistics">
            <article class="review-stat-card">
                <span>Reviews Written</span>
                <strong><asp:Literal ID="litReviewsWritten" runat="server" Mode="Encode" /></strong>
            </article>
            <article class="review-stat-card">
                <span>Average Rating Given</span>
                <strong><asp:Literal ID="litAverageRating" runat="server" Mode="Encode" /> <span aria-hidden="true">&#9733;</span></strong>
            </article>
        </section>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="review-page-message" role="status">
            <asp:Literal ID="litMessage" runat="server" Mode="Encode" />
        </asp:Panel>

        <asp:Panel ID="pnlFilters" runat="server" CssClass="review-filters" DefaultButton="btnSearch">
            <div class="review-search-field">
                <label for="<%= txtSearch.ClientID %>">Search reviewed courses</label>
                <div class="review-search-row">
                    <div class="review-search-input">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><circle cx="10.5" cy="10.5" r="6.5" /><path d="m15.5 15.5 5 5" /></svg>
                        <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" MaxLength="100" placeholder="Search reviewed courses" />
                    </div>
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" CausesValidation="false" />
                </div>
            </div>
            <div class="review-filter-field">
                <label for="<%= ddlRating.ClientID %>">Rating</label>
                <asp:DropDownList ID="ddlRating" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FiltersChanged">
                    <asp:ListItem Text="All Ratings" Value="0" />
                    <asp:ListItem Text="5 Stars" Value="5" />
                    <asp:ListItem Text="4 Stars" Value="4" />
                    <asp:ListItem Text="3 Stars" Value="3" />
                    <asp:ListItem Text="2 Stars" Value="2" />
                    <asp:ListItem Text="1 Star" Value="1" />
                </asp:DropDownList>
            </div>
            <div class="review-filter-field">
                <label for="<%= ddlSort.ClientID %>">Sort</label>
                <asp:DropDownList ID="ddlSort" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FiltersChanged">
                    <asp:ListItem Text="Newest First" Value="newest" />
                    <asp:ListItem Text="Oldest First" Value="oldest" />
                    <asp:ListItem Text="Highest Rating" Value="highest" />
                    <asp:ListItem Text="Lowest Rating" Value="lowest" />
                </asp:DropDownList>
            </div>
            <asp:LinkButton ID="btnClearFilters" runat="server" Text="Clear Filters" CssClass="review-clear-filters" OnClick="btnClearFilters_Click" CausesValidation="false" />
        </asp:Panel>

        <asp:Panel ID="pnlReviewResults" runat="server" Visible="false">
            <div class="review-results-heading">
                <asp:Label ID="lblResultCount" runat="server" CssClass="review-result-count" aria-live="polite" />
                <div class="review-view-toggle" role="group" aria-label="Review card layout">
                    <span>View:</span>
                    <button type="button" class="review-view-button" data-review-view="one" aria-label="One-column view" aria-pressed="false">
                        <svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="4" width="18" height="5" rx="1" /><rect x="3" y="11" width="18" height="5" rx="1" /><rect x="3" y="18" width="18" height="2" rx="1" /></svg>
                    </button>
                    <button type="button" class="review-view-button is-active" data-review-view="two" aria-label="Two-column view" aria-pressed="true">
                        <svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="4" width="8" height="7" rx="1" /><rect x="13" y="4" width="8" height="7" rx="1" /><rect x="3" y="13" width="8" height="7" rx="1" /><rect x="13" y="13" width="8" height="7" rx="1" /></svg>
                    </button>
                </div>
            </div>

            <div id="reviewCardGrid" class="review-card-grid view-two">
                <asp:Repeater ID="rptReviews" runat="server" OnItemCommand="rptReviews_ItemCommand">
                    <ItemTemplate>
                        <article class='<%# ReviewCardClass(Eval("ReviewID")) %>'>
                            <div class="review-course-image-wrap">
                                <asp:Image runat="server" CssClass="review-course-image" ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail"))) %>' Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>' AlternateText='<%# Convert.ToString(Eval("Title")) + " course thumbnail" %>' loading="lazy" />
                                <asp:Panel runat="server" CssClass="review-course-image-empty" Visible='<%# string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>'><span>Ready to cook</span></asp:Panel>
                            </div>
                            <div class="review-card-content">
                                <div class="review-course-heading">
                                    <h2><%#: Eval("Title") %></h2>
                                    <p><%#: Eval("CategoryName") %> <span aria-hidden="true">&bull;</span> <%#: Eval("DifficultyLevel") %></p>
                                </div>

                                <asp:Panel runat="server" Visible='<%# !IsEditing(Eval("ReviewID")) %>' CssClass="review-card-display">
                                    <div class="review-rating" aria-label='<%# Eval("Rating") + " out of 5 stars" %>'><span aria-hidden="true"><%# StarText(Eval("Rating")) %></span><strong><%#: Eval("Rating") %> / 5</strong></div>
                                    <p class="review-comment"><%#: Eval("Comment") %></p>
                                    <time datetime='<%# ReviewDateValue(Eval("CreatedAt")) %>'>Reviewed on <%#: ReviewDate(Eval("CreatedAt")) %></time>
                                    <div class="review-card-actions">
                                        <asp:HyperLink runat="server" CssClass="btn btn-secondary" NavigateUrl='<%# CourseOverviewUrl(Eval("CourseID")) %>'>View Course</asp:HyperLink>
                                        <asp:LinkButton runat="server" Text="Edit" CssClass="review-text-action" CommandName="EditReview" CommandArgument='<%# Eval("ReviewID") %>' CausesValidation="false" />
                                        <asp:LinkButton runat="server" Text="Delete" CssClass="review-text-action is-delete" CommandName="DeleteReview" CommandArgument='<%# Eval("ReviewID") %>' CausesValidation="false" OnClientClick='<%# DeleteConfirmation(Eval("Title")) %>' />
                                    </div>
                                </asp:Panel>

                                <asp:Panel runat="server" Visible='<%# IsEditing(Eval("ReviewID")) %>' CssClass="review-edit-form">
                                    <h3>Edit your review</h3>
                                    <fieldset class="review-edit-rating">
                                        <legend>Rating <span aria-hidden="true">*</span></legend>
                                        <asp:RadioButtonList ID="rblEditRating" runat="server" RepeatDirection="Horizontal" CssClass="review-star-options" SelectedValue='<%# Eval("Rating") %>'>
                                            <asp:ListItem Text="1" Value="1" /><asp:ListItem Text="2" Value="2" /><asp:ListItem Text="3" Value="3" /><asp:ListItem Text="4" Value="4" /><asp:ListItem Text="5" Value="5" />
                                        </asp:RadioButtonList>
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="rblEditRating" ValidationGroup='<%# "EditReview" + Eval("ReviewID") %>' ErrorMessage="Choose a rating." CssClass="review-field-error" Display="Dynamic" />
                                    </fieldset>
                                    <div class="review-edit-comment">
                                        <label for='<%# ((TextBox)Container.FindControl("txtEditComment")).ClientID %>'>Review <span aria-hidden="true">*</span></label>
                                        <asp:TextBox ID="txtEditComment" runat="server" TextMode="MultiLine" Rows="5" MaxLength="2000" Text='<%# Eval("Comment") %>' />
                                        <small>Maximum 2,000 characters.</small>
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEditComment" ValidationGroup='<%# "EditReview" + Eval("ReviewID") %>' ErrorMessage="Enter your review." CssClass="review-field-error" Display="Dynamic" />
                                    </div>
                                    <div class="review-edit-actions">
                                        <asp:LinkButton runat="server" Text="Cancel" CssClass="btn btn-secondary" CommandName="CancelEdit" CommandArgument='<%# Eval("ReviewID") %>' CausesValidation="false" />
                                        <asp:Button runat="server" Text="Save Changes" CssClass="btn btn-primary" CommandName="SaveReview" CommandArgument='<%# Eval("ReviewID") %>' ValidationGroup='<%# "EditReview" + Eval("ReviewID") %>' />
                                    </div>
                                </asp:Panel>
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <asp:Panel ID="pnlPagination" runat="server" Visible="false" CssClass="review-pagination">
                <asp:Button ID="btnPrevious" runat="server" Text="Previous" CssClass="btn btn-secondary" OnClick="btnPrevious_Click" CausesValidation="false" />
                <asp:Label ID="lblPageStatus" runat="server" CssClass="review-page-status" />
                <asp:Button ID="btnNext" runat="server" Text="Next" CssClass="btn btn-secondary" OnClick="btnNext_Click" CausesValidation="false" />
            </asp:Panel>
        </asp:Panel>

        <asp:Panel ID="pnlNoReviews" runat="server" Visible="false" CssClass="review-empty-state">
            <h2>You haven&rsquo;t written any reviews yet.</h2>
            <p>Complete a course and share your experience to help other learners choose what to learn.</p>
            <a class="btn btn-primary" href="<%= ResolveUrl("~/Pages/Learner/SearchCourse/SearchCoursePage.aspx") %>">Explore Courses</a>
        </asp:Panel>

        <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="review-empty-state">
            <h2>No reviews found.</h2>
            <p>Try changing your search or rating filter.</p>
            <asp:LinkButton ID="btnClearEmpty" runat="server" Text="Clear Filters" CssClass="btn btn-secondary" OnClick="btnClearFilters_Click" CausesValidation="false" />
        </asp:Panel>
    </main>
</asp:Content>

<asp:Content ID="MyReviewsScripts" ContentPlaceHolderID="LearnerScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Learner/LearnerReview/LearnerMyReviewPage.js") %>" defer></script>
</asp:Content>
