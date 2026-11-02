<%@ Page Title="Account closed | Cooking LAH" Language="C#" MasterPageFile="~/Site.Master" %>

<asp:Content
    ID="ClosureHead"
    ContentPlaceHolderID="HeadContent"
    runat="server">
    <link href="<%= ResolveUrl("~/Pages/Shared/Profile/Profile.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content
    ID="ClosureContent"
    ContentPlaceHolderID="MainContent"
    runat="server">
    <div class="profile-page">
        <div class="container profile-main">
            <section class="scrap-paper profile-closure" aria-labelledby="accountClosedTitle">
                <p class="profile-breadcrumb">Account closed</p>
                <% if (Request.QueryString["deactivated"] == "1") { %>
                <h1 id="accountClosedTitle">Your account has been deactivated.</h1>
                <p>You have been signed out. Your profile and courses have been retained. Contact an administrator if you would like to reactivate your account.</p>
                <% } else { %>
                <h1 id="accountClosedTitle">Your account has been deleted.</h1>
                <p>Your account and associated learning records have been permanently deleted. You have been signed out.</p>
                <% } %>
                <a class="btn btn-primary" href="<%= ResolveUrl("~/Main.aspx") %>">Back to main page</a>
            </section>
        </div>
    </div>
</asp:Content>
