<%@ Page Title="Manage users | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ManageUserPage.aspx.cs" Inherits="CookingLah.Pages.Admin.ManageUser.ManageUserPage" %>
<asp:Content ID="Head" ContentPlaceHolderID="AdminHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Admin/ManageUser/ManageUser.css") %>?v=<%= System.IO.File.GetLastWriteTimeUtc(Server.MapPath("~/Pages/Admin/ManageUser/ManageUser.css")).Ticks %>" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="AdminContent" runat="server">
<%-- Keep the first output expression outside conditionals so IntelliSense's generated __o stays in scope. --%>
<%= string.Empty %>
<div class="manage-users">
    <header class="mu-heading">
        <div><p class="mu-eyebrow">COMMUNITY / ADMINISTRATION</p><h1>Manage users</h1><p>A little care for your cooking community.</p></div>
        <a href="<%= H(PageUrl(PageNumber)) %>&amp;mode=create" class="btn btn-primary" data-popup="dialog">+ Create user</a>
    </header>
    <% if (Feedback != "") { %><div class="mu-feedback" role="status"><%= H(Feedback) %></div><% } %>
    <% if (ErrorMessage != "" && Mode == "") { %><div class="mu-feedback mu-error" role="alert" tabindex="-1" id="userError"><%= H(ErrorMessage) %></div><% } %>

    <% if (Mode == "create" || Mode == "edit") { %>
    <section class="mu-panel mu-modal mu-editor" id="userEditor" aria-labelledby="editorHeading">
        <div class="mu-section-heading"><div><p class="mu-eyebrow"><%= Mode == "create" ? "A NEW FACE IN THE KITCHEN" : "ACCOUNT DETAILS" %></p><h2 id="editorHeading"><%= Mode == "create" ? "Create user" : "Edit profile" %></h2></div><a href="<%= H(PageUrl(PageNumber)) %>" class="mu-action mu-close" data-close-modal="" aria-label="Close user editor">&times;</a></div>
        <asp:Label ID="lblError" runat="server" ClientIDMode="Static" CssClass="mu-feedback mu-error" role="alert" tabindex="-1" />
        <p>All fields are required unless marked optional.<% if (Mode == "edit") { %> Role: <strong><%= H(RoleLabel(RoleValue)) %></strong>. Roles stay fixed to preserve linked records.<% } %></p>
        <input type="hidden" name="userCsrf" value="<%= H(CsrfToken) %>" />
        <input type="hidden" name="userId" value="<%= TargetId %>" />
        <fieldset id="userFields" class="mu-fields">
            <legend class="mu-sr-only">User profile details</legend>
            <div class="mu-field"><label for="fullName">Full name</label><input id="fullName" name="fullName" value="<%= H(NameValue) %>" maxlength="50" required="required" autocomplete="off" /></div>
            <div class="mu-field"><label for="username">Username</label><input id="username" name="username" value="<%= H(UsernameValue) %>" maxlength="50" required="required" autocomplete="off" /></div>
            <div class="mu-field"><label for="email">Email</label><input id="email" name="email" type="email" value="<%= H(EmailValue) %>" maxlength="100" required="required" autocomplete="off" /></div>
            <div class="mu-field"><label for="country">Country</label><select id="country" name="country" required="required"><option value="">Select country</option><% foreach (string country in Countries) { %><option value="<%= H(country) %>" <%= country == CountryValue ? "selected" : "" %>><%= H(country) %></option><% } %></select></div>
            <% if (Mode == "create") { %>
            <div class="mu-field"><label for="newRole">Role</label><select id="newRole" name="newRole"><% foreach (string role in Roles) { %><option value="<%= role %>" <%= role == RoleValue ? "selected" : "" %>><%= RoleLabel(role) %></option><% } %></select></div>
            <% } %>
            <div class="mu-field"><label for="accountStatus">Account status</label><select id="accountStatus" name="accountStatus" aria-describedby="statusHint"><% foreach (string status in Statuses) { %><option value="<%= status %>" <%= status == StatusValue ? "selected" : "" %>><%= status %></option><% } %></select><small id="statusHint">Only Active accounts can sign in.</small></div>
            <% if (Mode == "create" || RoleValue == "Instructor") { %>
            <div class="mu-field" id="professionField"><label for="profession">Profession <span>(required for Instructors)</span></label><input id="profession" name="profession" maxlength="100" value="<%= H(ProfessionValue) %>" <%= RoleValue == "Instructor" ? "required" : "" %> placeholder="e.g. Baking instructor" /></div>
            <% } %>
            <% if (Mode == "create") { %>
            <div class="mu-field"><label for="password">Password</label><input id="password" name="password" type="password" required="required" minlength="8" maxlength="72" data-autocomplete="new-password" aria-describedby="passwordHint" /><small id="passwordHint">At least 8 characters; up to 72 UTF-8 bytes.</small></div>
            <div class="mu-field"><label for="confirmPassword">Confirm password</label><input id="confirmPassword" name="confirmPassword" type="password" required="required" minlength="8" maxlength="72" data-autocomplete="new-password" /></div>
            <% } %>
        </fieldset>
        <div class="mu-form-actions"><button type="submit" name="userAction" value="<%= Mode == "create" ? "create" : "save" %>" class="btn btn-primary" data-save-user=""><%= Mode == "create" ? "Create user" : "Save changes" %></button><a class="mu-link" href="<%= H(PageUrl(PageNumber)) %>">Cancel</a></div>
    </section>
    <% } %>

    <% if (Mode == "remove") { %>
    <section class="mu-panel mu-modal mu-removal" id="userRemove" aria-labelledby="removeHeading">
        <div class="mu-section-heading"><div><p class="mu-eyebrow">PLEASE CONFIRM</p><h2 id="removeHeading"><%= TargetRole == "Learner" ? "Permanently delete learner?" : "Deactivate account?" %></h2></div><a href="<%= H(PageUrl(PageNumber)) %>" class="mu-action mu-close" data-close-modal="" aria-label="Close removal confirmation">&times;</a></div>
        <% if (ErrorMessage != "") { %><div class="mu-feedback mu-error" role="alert" tabindex="-1" id="removeError"><%= H(ErrorMessage) %></div><% } %>
        <p><strong><%= H(TargetName) %></strong> &middot; <%= H(UsernameValue) %> &middot; <%= H(RoleLabel(TargetRole)) %></p>
        <p><%= TargetRole == "Learner" ? "This permanently deletes the learner and their course progress, quiz attempts, activity submissions, reviews and reports. This cannot be undone." : "This sets the account to Inactive and prevents future sign-ins. Their profile and linked records are kept. You can reactivate the account by editing its status." %></p>
        <input type="hidden" name="userCsrf" value="<%= H(CsrfToken) %>" /><input type="hidden" name="userId" value="<%= TargetId %>" />
        <div class="mu-form-actions"><a href="<%= H(PageUrl(PageNumber)) %>" class="btn" data-initial-focus="">Keep user</a><button type="submit" name="userAction" value="remove" class="btn mu-danger" data-save-user=""><%= TargetRole == "Learner" ? "Permanently delete" : "Set to Inactive" %></button></div>
    </section>
    <% } %>

    <section class="mu-panel" aria-labelledby="directoryHeading">
        <div class="mu-section-heading"><h2 id="directoryHeading">User directory</h2><span class="mu-count"><%= Total %> <%= Total == 1 ? "user" : "users" %></span></div>
        <div class="mu-filters" role="search" aria-label="Filter users">
            <div class="mu-field mu-search"><label for="userSearch">Search users</label><input id="userSearch" value="<%= H(Search) %>" type="search" maxlength="100" placeholder="Username or email" /></div>
            <div class="mu-field"><label for="roleFilter">Role</label><select id="roleFilter"><option value="">All roles</option><% foreach (string role in Roles) { %><option value="<%= role %>" <%= role == RoleFilter ? "selected" : "" %>><%= RoleLabel(role) %></option><% } %></select></div>
            <div class="mu-field"><label for="statusFilter">Status</label><select id="statusFilter"><option value="">All statuses</option><% foreach (string status in Statuses) { %><option value="<%= status %>" <%= status == StatusFilter ? "selected" : "" %>><%= status %></option><% } %></select></div>
            <button type="button" class="btn" id="applyFilters">Search</button><a href="ManageUserPage.aspx" class="mu-link">Reset</a>
        </div>
        <noscript><p>Search filters need JavaScript. You can still browse, create, edit and remove users.</p></noscript>
        <div class="mu-table-wrap">
            <table class="mu-table"><caption class="mu-sr-only">Registered users and account actions</caption><thead><tr><th scope="col">User</th><th scope="col">Role</th><th scope="col">Status</th><th scope="col">Actions</th></tr></thead><tbody>
                <asp:Repeater ID="rptUsers" runat="server" EnableViewState="false">
                    <ItemTemplate>
                        <tr>
                            <td data-label="User">
                                <strong>
                                    @<%#: Eval("Username") %>
                                    <asp:Label runat="server" CssClass="mu-you" Text="You"
                                        Visible='<%# Convert.ToInt32(Eval("UserID")) == ActorId %>' />
                                </strong>
                                <span class="mu-user-detail">Email: <%#: Eval("Email") %></span>
                            </td>
                            <td data-label="Role"><%#: RoleLabel(Convert.ToString(Eval("Role"))) %></td>
                            <td data-label="Status">
                                <span class='<%# GetStatusCss(Eval("AccountStatus")) %>'><%#: Eval("AccountStatus") %></span>
                            </td>
                            <td data-label="Actions">
                                <div class="mu-row-actions">
                                    <asp:HyperLink runat="server" CssClass="mu-action" Text="Edit"
                                        NavigateUrl='<%# PageUrl(PageNumber) + "&mode=edit&id=" + Eval("UserID") %>' data-popup="dialog"
                                        aria-label='<%# "Edit " + Eval("Username") %>' />
                                    <asp:HyperLink runat="server" CssClass="mu-action mu-remove-link"
                                        Text='<%# GetRemoveLabel(Eval("Role")) %>'
                                        NavigateUrl='<%# PageUrl(PageNumber) + "&mode=remove&id=" + Eval("UserID") %>' data-popup="dialog"
                                        aria-label='<%# GetRemoveLabel(Eval("Role")) + " " + Eval("Username") %>'
                                        Visible='<%# CanRemoveUser(Eval("UserID"), Eval("Role"), Eval("AccountStatus")) %>' />
                                </div>
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody></table>
        </div>
        <% if (Total == 0) { %><div class="mu-empty"><h3>No users found</h3><p>Try a different name or clear the filters.</p><a href="ManageUserPage.aspx" class="btn">Clear filters</a></div><% } %>
        <footer class="mu-directory-footer"><p>Learners are permanently deleted. Instructors and Admins are set to Inactive.</p><nav class="mu-pagination" aria-label="User pages"><% if (PageNumber > 1) { %><a class="mu-action" href="<%= H(PageUrl(PageNumber - 1)) %>">Previous</a><% } %><span>Page <%= PageNumber %> of <%= Math.Max(1, (Total + PageSize - 1) / PageSize) %></span><% if (PageNumber * PageSize < Total) { %><a class="mu-action" href="<%= H(PageUrl(PageNumber + 1)) %>">Next</a><% } %></nav></footer>
    </section>
</div>
</asp:Content>
<asp:Content ID="Scripts" ContentPlaceHolderID="AdminScripts" runat="server"><script src="<%= ResolveUrl("~/Pages/Admin/ManageUser/ManageUserjs.js") %>?v=<%= System.IO.File.GetLastWriteTimeUtc(Server.MapPath("~/Pages/Admin/ManageUser/ManageUserjs.js")).Ticks %>" defer="defer"></script></asp:Content>
