<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminLoginPage.aspx.cs" Inherits="CookingLah.Pages.Admin.LoginPage.AdminLoginPage" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Admin login - Cooking LAH</title>
    <link runat="server" href="~/F_CSS/Style.css" rel="stylesheet" />
    <link runat="server" href="~/F_CSS/LoginSignupPage.css" rel="stylesheet" />
    <link runat="server" href="~/Pages/Admin/LoginPage/AdminLoginPage.css" rel="stylesheet" />
</head>
<body class="admin-login">
    <form id="form1" runat="server">
        <header>
            <a class="logo" href="<%= ResolveUrl("~/Main.aspx") %>" aria-label="Cooking LAH home"><img src="<%= ResolveUrl("~/F_Assets/CookingLahLogo.png") %>" alt="Cooking LAH" /></a>
            <span class="admin-badge">Admin access</span>
        </header>
        <main>
            <section class="intro" aria-labelledby="introTitle">
                <h1 id="introTitle">Behind <mark>the kitchen.</mark></h1>
                <p>A little care keeps the community cooking. Sign in to your administrator account to get started.</p>
                <div class="stickers" aria-hidden="true">
                    <div class="st a"><img src="<%= ResolveUrl("~/F_Assets/LoginFood1.png") %>" alt="" /></div>
                    <div class="st b"><img src="<%= ResolveUrl("~/F_Assets/LoginFood2.png") %>" alt="" /></div>
                    <div class="st c"><img src="<%= ResolveUrl("~/F_Assets/LoginFood3.png") %>" alt="" /></div>
                    <div class="st d"><img src="<%= ResolveUrl("~/F_Assets/LoginFood4.png") %>" alt="" /></div>
                    <div class="st e"><img src="<%= ResolveUrl("~/F_Assets/LoginFood5.png") %>" alt="" /></div>
                    <div class="st f"><img src="<%= ResolveUrl("~/F_Assets/LoginFood6.png") %>" alt="" /></div>
                    <div class="st g"><img src="<%= ResolveUrl("~/F_Assets/LoginFood7.png") %>" alt="" /></div>
                </div>
            </section>
            <section class="card admin-login-card" aria-labelledby="loginTitle">
                <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnLogin" CssClass="view on">
                    <p class="admin-eyebrow">COOKING LAH / ADMINISTRATION</p>
                    <h2 id="loginTitle">Welcome back</h2>
                    <p class="sub">Log in with your admin username or email.</p>
                    <asp:Label ID="lblLoginError" runat="server" ClientIDMode="Static" CssClass="errbox" role="alert" tabindex="-1" Visible="false" />
                    <input type="hidden" name="adminLoginCsrf" value="<%= Server.HtmlEncode(CsrfToken) %>" />
                    <div class="f">
                        <label class="l" for="txtLoginEmail">Username or email</label>
                        <asp:TextBox ID="txtLoginEmail" runat="server" ClientIDMode="Static" MaxLength="100" autocomplete="username" placeholder="Your admin username or email" required="required" />
                    </div>
                    <div class="f admin-password">
                        <label class="l" for="txtLoginPass">Password</label>
                        <asp:TextBox ID="txtLoginPass" runat="server" ClientIDMode="Static" TextMode="Password" autocomplete="current-password" placeholder="Enter your password" required="required" />
                        <button type="button" class="eye" id="togglePassword" aria-controls="txtLoginPass" aria-label="Show password" aria-pressed="false" hidden>Show</button>
                    </div>
                    <asp:Button ID="btnLogin" runat="server" ClientIDMode="Static" Text="Log in as admin" CssClass="btn full" OnClick="btnLogin_Click" />
                    <p class="admin-help">For active administrator accounts only. Need access or help signing in? Contact another administrator.</p>
                </asp:Panel>
                <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="admin-success" role="status" tabindex="-1" ClientIDMode="Static">
                    <span class="admin-badge">Signed in</span>
                    <h2 id="successTitle">You're logged in.</h2>
                    <p>Your administrator account has been verified successfully.</p>
                </asp:Panel>
            </section>
        </main>
    </form>
    <script src="<%= ResolveUrl("~/Pages/Admin/LoginPage/AdminLoginPage.js") %>" defer></script>
</body>
</html>
