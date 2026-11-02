<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="CookingLah.Pages.ErrorPage.ErrorPage" %>
<!DOCTYPE html>
<html lang="en">
    <head runat="server">
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <title><asp:Literal ID="litPageTitle" runat="server" /> | Cooking LAH</title>
        <link rel="icon" type="image/x-icon" href="<%= ResolveUrl("~/favicon.ico?v=2") %>" />
        <link href="<%= ResolveUrl("~/F_CSS/Style.css") %>" rel="stylesheet" />
        <link href="<%= ResolveUrl("~/Pages/ErrorPage/ErrorPage.css") %>" rel="stylesheet" />
    </head>
    <body class="error-page">
        <main class="error-shell">
            <a class="error-logo" href="<%= ResolveUrl("~/Main.aspx") %>" aria-label="Cooking LAH main page">
                <img src="<%= ResolveUrl("~/F_Assets/CookingLahLogo.png") %>" alt="Cooking LAH" />
            </a>

            <section class="error-card" aria-labelledby="errorTitle">
                <div class="error-visual" aria-hidden="true">
                    <asp:Literal ID="litIllustration" runat="server" />
                    <img src="<%= ResolveUrl("~/F_Assets/mascot.png") %>" alt="" />
                </div>

                <div class="error-copy">
                    <span class="error-code"><asp:Literal ID="litStatusCode" runat="server" /></span>
                    <h1 id="errorTitle"><asp:Literal ID="litHeading" runat="server" /></h1>
                    <p><asp:Literal ID="litMessage" runat="server" /></p>
                    <div class="error-actions">
                        <a class="btn btn-primary" href="<%= ResolveUrl("~/Main.aspx") %>">Back to main page</a>
                        <asp:HyperLink
                            ID="lnkReload"
                            runat="server"
                            CssClass="btn error-reload"
                            Text="Reload page" />
                    </div>
                </div>
            </section>
        </main>
    </body>
</html>
