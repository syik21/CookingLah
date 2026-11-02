<%@ Page Title="Course Reviews | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="ChefViewReviewPage.aspx.cs" Inherits="CookingLah.Pages.Chef.ChefViewReview.ChefViewReviewPage" %>
<%@ Import Namespace="System" %>
<asp:Content
    ID="ReviewHead"
    ContentPlaceHolderID="ChefHead"
    runat="server">
    <link href="<%= ResolveUrl("~/Pages/Chef/ChefViewReview/ChefViewReviewPage.css") %>" rel="stylesheet" />
</asp:Content>
<asp:Content
    ID="ReviewContent"
    ContentPlaceHolderID="ChefContent"
    runat="server">
    <main class="chef-reviews" aria-labelledby="courseReviewsTitle">
        <header class="chef-reviews-heading">
            <h1 id="courseReviewsTitle">Course Reviews</h1>
            <p>View ratings and feedback from learners taking your courses.</p>
        </header>
        <section class="chef-review-stats" aria-label="Review statistics">
            <article><span>Total Reviews</span><strong><asp:Literal
                        ID="litTotalReviews"
                        runat="server"
                        Mode="Encode" /></strong></article>
            <article><span>Average Rating</span><strong><asp:Literal
                        ID="litAverageRating"
                        runat="server"
                        Mode="Encode" /></strong></article>
            <article><span>Reviewed Courses</span><strong><asp:Literal
                        ID="litReviewedCourses"
                        runat="server"
                        Mode="Encode" /></strong></article>
        </section>
        <asp:Panel
            ID="pnlFilters"
            runat="server"
            CssClass="chef-review-filters"
            DefaultButton="btnSearch">
            <div class="chef-review-search"><label for="<%= txtSearch.ClientID %>">Search course or learner</label><div>
                    <div class="sk-search"><asp:TextBox
                            ID="txtSearch"
                            runat="server"
                            TextMode="Search"
                            MaxLength="100"
                            placeholder="Search course or learner..." /></div><asp:Button
                        ID="btnSearch"
                        runat="server"
                        Text="Search"
                        CssClass="btn btn-primary"
                        OnClick="btnSearch_Click"
                        CausesValidation="false" /></div>
            </div>
            <div><label for="<%= ddlCourse.ClientID %>">Course</label><asp:DropDownList
                    ID="ddlCourse"
                    runat="server"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FiltersChanged" /></div>
            <div><label for="<%= ddlRating.ClientID %>">Rating</label><asp:DropDownList
                    ID="ddlRating"
                    runat="server"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FiltersChanged"><asp:ListItem Text="All Ratings" Value="0" /><asp:ListItem Text="5 Stars" Value="5" /><asp:ListItem Text="4 Stars" Value="4" /><asp:ListItem Text="3 Stars" Value="3" /><asp:ListItem Text="2 Stars" Value="2" /><asp:ListItem Text="1 Star" Value="1" /></asp:DropDownList></div>
            <div><label for="<%= ddlSort.ClientID %>">Sort</label><asp:DropDownList
                    ID="ddlSort"
                    runat="server"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FiltersChanged"><asp:ListItem Text="Newest First" Value="newest" /><asp:ListItem Text="Oldest First" Value="oldest" /><asp:ListItem Text="Highest Rating" Value="highest" /><asp:ListItem Text="Lowest Rating" Value="lowest" /></asp:DropDownList></div>
        </asp:Panel>
        <asp:Panel ID="pnlResults" runat="server">
            <h2 class="chef-review-result-count"><asp:Literal
                    ID="litResultCount"
                    runat="server"
                    Mode="Encode" /></h2>
            <div class="chef-review-list">
                <asp:Repeater ID="rptReviews" runat="server">
                    <ItemTemplate>
                        <article class="chef-review-row">
                            <div class="chef-review-image-wrap">
                                <asp:Image
                                    runat="server"
                                    CssClass="chef-review-image"
                                    ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail"))) %>'
                                    Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>'
                                    AlternateText='<%# Convert.ToString(Eval("CourseTitle")) + " course thumbnail" %>'
                                    loading="lazy" />
                                <asp:Panel
                                    runat="server"
                                    CssClass="chef-review-image-empty course-image-placeholder"
                                    Visible='<%# string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>'><img runat="server" src="~/F_Assets/mascot.png" alt="" /><span>Food Image Coming Soon</span></asp:Panel>
                            </div>
                            <div class="chef-review-copy">
                                <h3><%#: Eval("CourseTitle") %></h3>
                                <p class="chef-review-meta"><%#: Eval("CategoryName") %> &bull; <%#: Eval("DifficultyLevel") %></p>
                                <p class="chef-review-byline">Reviewed by <strong><%#: Eval("LearnerName") %></strong><br />Reviewed on <time datetime='<%# ReviewDateValue(Eval("CreatedAt")) %>'><%#: ReviewDate(Eval("CreatedAt")) %></time></p>
                                <p class="chef-review-comment"><%#: Eval("Comment") %></p>
                            </div>
                            <div class="chef-review-action">
                                <p class="chef-review-rating" aria-label='<%# Eval("Rating") + " out of 5 stars" %>'><span aria-hidden="true"><%# StarText(Eval("Rating")) %></span> <%#: Eval("Rating") %> / 5</p><asp:HyperLink
                                    runat="server"
                                    CssClass="btn btn-primary"
                                    Text="View Course"
                                    NavigateUrl='<%# ManageCourseUrl(Eval("CourseID")) %>' /></div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <asp:Panel
                ID="pnlPagination"
                runat="server"
                CssClass="chef-review-pagination"><asp:Button
                    ID="btnPrevious"
                    runat="server"
                    Text="&#8249; Previous"
                    CssClass="btn"
                    OnClick="btnPrevious_Click"
                    CausesValidation="false" /><asp:Label ID="lblPageStatus" runat="server" /><asp:Button
                    ID="btnNext"
                    runat="server"
                    Text="Next &#8250;"
                    CssClass="btn"
                    OnClick="btnNext_Click"
                    CausesValidation="false" /></asp:Panel>
        </asp:Panel>
        <asp:Panel
            ID="pnlNoReviews"
            runat="server"
            Visible="false"
            CssClass="chef-review-empty">
            <h2>No course reviews yet.</h2>
            <p>Learner feedback will appear here after learners complete your courses and leave a review.</p>
        </asp:Panel>
        <asp:Panel
            ID="pnlNoResults"
            runat="server"
            Visible="false"
            CssClass="chef-review-empty">
            <h2>No reviews found.</h2>
            <p>Try changing your search or filters.</p><asp:Button
                ID="btnClearFilters"
                runat="server"
                Text="Clear Filters"
                CssClass="btn btn-primary"
                OnClick="btnClearFilters_Click"
                CausesValidation="false" /></asp:Panel>
    </main>
</asp:Content>
