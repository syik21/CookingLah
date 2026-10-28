<%@ Page Title="Category Management | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ManagerCategoriesPage.aspx.cs" Inherits="CookingLah.Pages.Admin.ManageCategories.ManagerCategoriesPage" %>

<%-- Page stylesheet --%>
<asp:Content ID="Head" ContentPlaceHolderID="AdminHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Admin/ManageCategories/ManagerCategoriesPage.css") %>" rel="stylesheet" />
</asp:Content>

<%-- Category management content --%>
<asp:Content ID="Content" ContentPlaceHolderID="AdminContent" runat="server">
    <div class="categories-page">

        <%-- Page heading and add button --%>
        <header class="categories-page-header">
            <div>
                <p class="categories-eyebrow">The admin notebook</p>
                <h1>Category Management</h1>
                <p>Keep course categories clear and useful for Chefs and learners.</p>
            </div>

            <asp:Button ID="btnOpenAdd" runat="server"
                Text="Add Category"
                CssClass="btn btn-primary"
                OnClick="btnOpenAdd_Click"
                CausesValidation="false" />
        </header>

        <%-- Success and error messages --%>
        <asp:Panel ID="pnlMessage" runat="server" Visible="false"
            role="status" tabindex="-1" ClientIDMode="Static">
            <asp:Literal ID="litMessage" runat="server" Mode="Encode" />
        </asp:Panel>

        <%-- Search categories --%>
        <div class="categories-toolbar" aria-labelledby="categorySearchHeading">
            <div class="categories-toolbar-copy">
                <h2 id="categorySearchHeading">Find a category</h2>
                <p>Search by category name or description.</p>
            </div>
            <asp:Panel ID="pnlSearch" runat="server"
                CssClass="categories-search"
                DefaultButton="btnSearchCategory"
                role="search">

                <div class="category-field">
                    <asp:Label runat="server"
                        AssociatedControlID="txtSearchCategory"
                        Text="Search categories" />

                    <asp:TextBox ID="txtSearchCategory" runat="server"
                        MaxLength="100"
                        placeholder="Search categories..." />
                </div>

                <asp:Button ID="btnSearchCategory" runat="server"
                    Text="Search"
                    CssClass="btn"
                    OnClick="btnSearchCategory_Click"
                    CausesValidation="false" />

                <asp:Button ID="btnClearSearch" runat="server"
                    Text="Clear"
                    CssClass="category-action"
                    OnClick="btnClearSearch_Click"
                    CausesValidation="false" />
            </asp:Panel>
        </div>

        <%-- Add or edit category form --%>
        <asp:Panel ID="pnlCategoryForm" runat="server"
            Visible="false"
            CssClass="card category-form"
            ClientIDMode="Static"
            DefaultButton="btnSaveCategory">

            <h2 id="categoryFormHeading">
                <asp:Literal ID="litFormTitle" runat="server" Mode="Encode" />
            </h2>
            <p class="category-form-intro">Category names appear in course search and course creation.</p>

            <div class="category-field">
                <asp:Label runat="server"
                    AssociatedControlID="txtCategoryName"
                    Text="Category Name *" />

                <asp:TextBox ID="txtCategoryName" runat="server"
                    ClientIDMode="Static"
                    MaxLength="100"
                    aria-required="true"
                    aria-describedby="categoryNameHint" />

                <small id="categoryNameHint">
                    Required. Choose a unique name, up to 100 characters.
                </small>
            </div>

            <div class="category-field">
                <asp:Label runat="server"
                    AssociatedControlID="txtCategoryDescription"
                    Text="Description" />

                <asp:TextBox ID="txtCategoryDescription" runat="server"
                    ClientIDMode="Static"
                    TextMode="MultiLine"
                    Rows="4"
                    MaxLength="500"
                    aria-describedby="categoryDescriptionHint" />

                <small id="categoryDescriptionHint">
                    Optional. Up to 500 characters.
                    <span id="categoryDescriptionCount"></span>
                </small>
            </div>

            <div class="category-form-actions">
                <asp:Button ID="btnCancelCategory" runat="server"
                    Text="Cancel"
                    CssClass="btn"
                    OnClick="btnCancelCategory_Click"
                    CausesValidation="false" />

                <asp:Button ID="btnSaveCategory" runat="server"
                    Text="Add Category"
                    CssClass="btn btn-primary"
                    OnClick="btnSaveCategory_Click" />
            </div>
        </asp:Panel>

        <%-- Delete confirmation --%>
        <asp:Panel ID="pnlDeleteCategory" runat="server"
            Visible="false"
            CssClass="card delete-warning"
            ClientIDMode="Static">

            <h2 id="deleteHeading">
                <asp:Literal ID="litDeleteTitle" runat="server" Mode="Encode" />
            </h2>

            <p>
                <asp:Literal ID="litDeleteMessage" runat="server" Mode="Encode" />
            </p>

            <div class="category-form-actions">
                <asp:Button ID="btnCancelDelete" runat="server"
                    Text="Cancel"
                    CssClass="btn"
                    OnClick="btnCancelCategory_Click"
                    CausesValidation="false" />

                <asp:Button ID="btnConfirmDelete" runat="server"
                    Text="Delete Category"
                    CssClass="btn category-danger"
                    OnClick="btnConfirmDelete_Click"
                    CausesValidation="false" />
            </div>
        </asp:Panel>

        <%-- Category directory --%>
        <section class="categories-directory" aria-labelledby="categoriesHeading">
            <div class="categories-list-heading">
                <div>
                    <p class="categories-section-label">Course organisation</p>
                    <h2 id="categoriesHeading">Category Directory</h2>
                    <p>Review category usage before editing or deleting.</p>
                </div>

                <div class="categories-summary">
                    <span>Total</span>
                    <asp:Label ID="lblTotalCategories" runat="server" Text="Not available" />
                </div>
            </div>

            <%-- Categories table --%>
            <asp:Panel ID="pnlCategoryList" runat="server" CssClass="category-list">
                <table>
                    <caption class="visually-hidden">
                        Categories and the courses using them
                    </caption>
                    <thead>
                        <tr>
                            <th scope="col">Category Name</th>
                            <th scope="col">Description</th>
                            <th scope="col">Courses</th>
                            <th scope="col">Created Date</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptCategories" runat="server"
                            OnItemCommand="rptCategories_ItemCommand">
                            <ItemTemplate>
                                <tr class="category-row">
                                    <td data-label="Category">
                                        <strong class="category-name">
                                            <%#: Eval("CategoryName") %>
                                        </strong>
                                    </td>

                                    <td data-label="Description" class="category-description">
                                        <%#: string.IsNullOrWhiteSpace(Convert.ToString(Eval("Description"))) ? "No description added." : Eval("Description") %>
                                    </td>

                                    <td data-label="Courses">
                                        <span class="category-course-count">
                                            <%#: Eval("CourseCount") %>
                                            <%# Convert.ToInt32(Eval("CourseCount")) == 1 ? "course" : "courses" %>
                                        </span>
                                    </td>

                                    <td data-label="Created">
                                        <%#: Eval("CreatedAt", "{0:dd MMM yyyy}") %>
                                    </td>

                                    <td data-label="Actions">
                                        <div class="category-actions">
                                            <asp:LinkButton runat="server"
                                                Text="Edit"
                                                CssClass="category-action"
                                                CommandName="EditCategory"
                                                CommandArgument='<%# Eval("CategoryID") %>'
                                                CausesValidation="false"
                                                aria-label='<%# "Edit " + Eval("CategoryName") %>' />

                                            <asp:LinkButton runat="server"
                                                Text="Delete"
                                                CssClass="category-action category-delete"
                                                CommandName="DeleteCategory"
                                                CommandArgument='<%# Eval("CategoryID") %>'
                                                CausesValidation="false"
                                                aria-label='<%# "Delete " + Eval("CategoryName") %>' />
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </asp:Panel>

            <%-- Empty category list --%>
            <asp:Panel ID="pnlEmpty" runat="server"
                Visible="false" CssClass="card category-empty-state">

                <h3>
                    <asp:Literal ID="litEmptyTitle" runat="server" Mode="Encode" />
                </h3>

                <asp:Panel ID="pnlFirstCategory" runat="server">
                    <p>Create your first category so Chefs can organize their courses.</p>

                    <asp:Button runat="server"
                        Text="Add Category"
                        CssClass="btn btn-primary"
                        OnClick="btnOpenAdd_Click"
                        CausesValidation="false" />
                </asp:Panel>

                <asp:Button ID="btnEmptyClear" runat="server"
                    Text="Clear Search"
                    CssClass="btn"
                    OnClick="btnClearSearch_Click"
                    CausesValidation="false" />
            </asp:Panel>

            <%-- Category pagination --%>
            <asp:Panel ID="pnlPagination" runat="server"
                CssClass="categories-pagination"
                Visible="false"
                role="navigation"
                aria-label="Category pages">

                <asp:Label ID="lblPageSummary" runat="server" />

                <div class="category-actions">
                    <asp:Button ID="btnPrevious" runat="server"
                        Text="Previous"
                        CssClass="btn"
                        OnClick="btnPrevious_Click"
                        CausesValidation="false" />

                    <asp:Label ID="lblPageNumber" runat="server" />

                    <asp:Button ID="btnNext" runat="server"
                        Text="Next"
                        CssClass="btn"
                        OnClick="btnNext_Click"
                        CausesValidation="false" />
                </div>
            </asp:Panel>
        </section>
    </div>
</asp:Content>

<%-- Page JavaScript --%>
<asp:Content ID="Scripts" ContentPlaceHolderID="AdminScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Admin/ManageCategories/ManagerCategoriesPage.js") %>" defer></script>
</asp:Content>
