<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchCourse.ascx.cs" Inherits="CookingLah.Pages.Shared.CourseSearch.SearchCourse" %>
<%@ Register Src="~/Pages/Shared/CourseSearch/ViewCourseOverivew.ascx" TagPrefix="course" TagName="ViewCourseOverivew" %>

<asp:Panel ID="pnlCourseSearch" runat="server" CssClass="course-search-panel">
    <header class="course-search-heading">
        <p class="search-kicker">Find your next kitchen skill</p>
        <h1>Explore Courses</h1>
        <p>Find a cooking course that matches your interests.</p>
    </header>

    <asp:Panel ID="pnlSearchForm" runat="server" CssClass="course-search-toolbar" DefaultButton="btnSearch">
        <div class="search-field search-field-main">
            <label for="<%= txtSearch.ClientID %>">Search Courses</label>
            <div class="search-input-row">
                <div class="search-input-wrap">
                    <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><circle cx="10.8" cy="10.8" r="6.3" /><path d="m15.5 15.5 5 5" /></svg>
                    <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" MaxLength="100" placeholder="Search by course name, cuisine or skill" />
                </div>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary search-submit" OnClick="btnSearch_Click" CausesValidation="false" />
            </div>
        </div>
        <div class="search-filter-row">
            <div class="search-field">
                <label for="<%= ddlCategory.ClientID %>">Category</label>
                <asp:DropDownList ID="ddlCategory" runat="server" CssClass="search-select" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged" />
            </div>
            <div class="search-field">
                <label for="<%= ddlDifficulty.ClientID %>">Difficulty</label>
                <asp:DropDownList ID="ddlDifficulty" runat="server" CssClass="search-select" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                    <asp:ListItem Text="All Levels" Value="" />
                    <asp:ListItem Text="Beginner" Value="Beginner" />
                    <asp:ListItem Text="Intermediate" Value="Intermediate" />
                    <asp:ListItem Text="Advanced" Value="Advanced" />
                </asp:DropDownList>
            </div>
            <div class="search-field">
                <label for="<%= ddlRating.ClientID %>">Rating</label>
                <asp:DropDownList ID="ddlRating" runat="server" CssClass="search-select" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                    <asp:ListItem Text="All Ratings" Value="0" />
                    <asp:ListItem Text="4★ &amp; above" Value="4" />
                    <asp:ListItem Text="3★ &amp; above" Value="3" />
                    <asp:ListItem Text="2★ &amp; above" Value="2" />
                    <asp:ListItem Text="1★ &amp; above" Value="1" />
                </asp:DropDownList>
            </div>
            <div class="search-filter-actions"><asp:LinkButton ID="btnClearSearch" runat="server" Text="Clear Filters" CssClass="search-clear" OnClick="btnClearSearch_Click" CausesValidation="false" /></div>
        </div>
    </asp:Panel>

    <section class="course-results" aria-labelledby="courseResultsTitle">
        <div class="results-heading">
            <div>
                <h2 id="courseResultsTitle">Available Courses</h2>
                <% if (!string.IsNullOrEmpty(SelectedDishName)) { %><p>Traditional dish: <strong><%= Server.HtmlEncode(SelectedDishName) %></strong></p><% } %>
            </div>
            <asp:Label ID="lblResultCount" runat="server" CssClass="results-count" aria-live="polite" />
        </div>

        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="course-search-empty">
            <span class="empty-mark" aria-hidden="true"><svg viewBox="0 0 48 48" fill="none"><path d="M8 16h32l-3 20H11L8 16ZM6 13h36M16 12c-3-4 3-5 1-9m13 9c-3-4 3-5 1-9M17 23l6 6 9-9" /></svg></span>
            <h3>No courses found</h3><p>Try changing your search or filters.</p>
            <asp:LinkButton ID="btnClearEmpty" runat="server" Text="Clear Filters" CssClass="btn btn-secondary" OnClick="btnClearSearch_Click" CausesValidation="false" />
        </asp:Panel>

        <asp:Repeater ID="rptCourses" runat="server" OnItemCommand="rptCourses_ItemCommand">
            <HeaderTemplate><div class="course-card-grid"></HeaderTemplate>
            <ItemTemplate>
                <article class="course-search-card">
                    <div class="course-card-image-wrap">
                        <asp:Image runat="server" CssClass="course-card-image" ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail"))) %>' Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>' AlternateText='<%# Convert.ToString(Eval("Title")) + " course thumbnail" %>' loading="lazy" />
                        <asp:Panel runat="server" CssClass="course-card-image-empty" Visible='<%# string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>'><svg viewBox="0 0 120 90" fill="none" aria-hidden="true"><path d="M17 45c2 23 20 36 43 36s41-13 43-36H17Zm86 8h11c8 0 11 4 11 9s-3 9-11 9H99M14 43c18-6 67-6 91 0M39 34c-6-9 6-11 1-19m21 19c-6-9 6-11 1-19m20 19c-6-9 6-11 1-19" /><circle cx="60" cy="46" r="9" /></svg><span>Ready to cook</span></asp:Panel>
                    </div>
                    <div class="course-card-body">
                        <div class="course-card-labels"><span class="course-level"><%#: Eval("DifficultyLevel") %></span><span class="course-category"><%#: Eval("CategoryName") %></span></div>
                        <h3 title='<%#: Eval("Title") %>'><%#: Eval("Title") %></h3>
                        <p class="course-card-description"><%#: Eval("ShortDescription") %></p>
                        <p class="course-chef">By <strong><%#: Eval("ChefName") %></strong></p>
                        <p class="course-rating"><%#: FormatCourseRating(Eval("AverageRating"), Eval("ReviewCount")) %></p>
                        <asp:Panel runat="server" CssClass="course-traditional-dish" Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("DishName"))) %>'>Traditional Dish: <strong><%#: Eval("DishName") %></strong></asp:Panel>
                        <div class="course-card-actions"><asp:LinkButton runat="server" CssClass="btn btn-secondary" CommandName="ViewCourse" CommandArgument='<%# Eval("CourseID") %>' CausesValidation="false">View Course <span aria-hidden="true">&rarr;</span></asp:LinkButton></div>
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

<asp:Panel ID="pnlCourseOverview" runat="server" Visible="false" CssClass="course-overview-panel"><course:ViewCourseOverivew ID="ViewCourseOverivew1" runat="server" /></asp:Panel>
