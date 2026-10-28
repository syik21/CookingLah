<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPasswordPage.aspx.cs" Inherits="CookingLah.Pages.Guest.ResetPassword.ResetPasswordPage" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover" />
    <title>Reset Password | Cooking LAH</title>
    <link href="<%= ResolveUrl("~/F_CSS/Style.css") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Guest/ResetPassword/ResetPasswordPage.css") %>" rel="stylesheet" />
</head>
<body class="reset-page">
    <form id="form1" runat="server">
        <main class="reset-shell">
            <section class="reset-card">
                <img class="reset-logo" src="<%= ResolveUrl("~/F_Assets/CookingLahLogo.png") %>" alt="Cooking LAH" />
                <asp:Panel ID="pnlResetForm" runat="server" Visible="false" DefaultButton="btnResetPassword">
                    <p class="reset-eyebrow">Account recovery</p>
                    <h1 id="resetTitle">Reset Your Password</h1>
                    <p>Enter a new password for your account.</p>
                    <asp:Label ID="lblResetError" runat="server" CssClass="reset-message reset-error" role="alert" Visible="false" />
                    <div class="reset-field">
                        <label for="txtNewPassword">New Password</label>
                        <div class="reset-password-wrap">
                            <asp:TextBox ID="txtNewPassword" runat="server" ClientIDMode="Static" TextMode="Password" autocomplete="new-password" aria-describedby="passwordHelp" required="required" />
                            <button type="button" class="reset-toggle" data-target="txtNewPassword" aria-label="Show new password">Show</button>
                        </div>
                        <small id="passwordHelp">Use at least 8 characters and at most 72 UTF-8 bytes.</small>
                    </div>
                    <div class="reset-field">
                        <label for="txtConfirmPassword">Confirm Password</label>
                        <div class="reset-password-wrap">
                            <asp:TextBox ID="txtConfirmPassword" runat="server" ClientIDMode="Static" TextMode="Password" autocomplete="new-password" required="required" />
                            <button type="button" class="reset-toggle" data-target="txtConfirmPassword" aria-label="Show confirmed password">Show</button>
                        </div>
                    </div>
                    <asp:Button ID="btnResetPassword" runat="server" ClientIDMode="Static" CssClass="btn reset-primary" Text="Reset Password" OnClick="btnResetPassword_Click" />
                </asp:Panel>
                <asp:Panel ID="pnlInvalid" runat="server" Visible="false">
                    <p class="reset-eyebrow">Account recovery</p>
                    <h1>Reset Link Invalid</h1>
                    <p>This password reset link is invalid or has expired.</p>
                    <a class="btn reset-primary" href="<%= ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx") %>">Back to Cooking LAH</a>
                </asp:Panel>
                <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
                    <svg class="reset-success-icon" viewBox="0 0 48 48" aria-hidden="true"><circle cx="24" cy="24" r="21" /><path d="m14 24 7 7 14-15" /></svg>
                    <h1>Password Reset Successfully</h1>
                    <p>Your Cooking LAH password has been updated.</p>
                    <p>You can now return to Cooking LAH and sign in using your new password.</p>
                    <a class="btn reset-primary" href="<%= ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx") %>">Back to Cooking LAH</a>
                </asp:Panel>
            </section>
        </main>
    </form>
    <script src="<%= ResolveUrl("~/Pages/Guest/ResetPassword/ResetPasswordPage.js") %>" defer></script>
</body>
</html>
