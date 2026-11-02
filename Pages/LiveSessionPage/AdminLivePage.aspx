<%@ Page Title="Monitor Livestream | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="AdminLivePage.aspx.cs" Inherits="CookingLah.Pages.LiveSessionPage.AdminLivePage" %>

<asp:Content
    ID="AdminLiveHead"
    ContentPlaceHolderID="AdminHead"
    runat="server">
    <link href="<%= ResolveUrl("~/Pages/LiveSessionPage/CSS/ChefLivePage.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content
    ID="AdminLiveContent"
    ContentPlaceHolderID="AdminContent"
    runat="server">
    <main class="chef-live-page admin-live-page">
        <section class="live-header">
            <div>
                <span class="live-badge">
                    ● LIVE
                </span>
            </div>
            <span class="live-participant-count">
                <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><circle cx="9" cy="8" r="3" /><path d="M3 19v-2a6 6 0 0 1 12 0v2" /><circle cx="17" cy="9" r="2.5" /><path d="M16 14a5 5 0 0 1 5 5" /></svg>
                <span id="liveParticipantCount" role="status" aria-live="polite">Connecting...</span>
            </span>
        </section>

        <div
            id="adminModerationStatus"
            class="admin-moderation-status"
            role="status"
            aria-live="polite"
            hidden>
        </div>

        <section class="live-layout">
            <div class="live-video-section">
                <div id="jitsi-container" class="jitsi-container">
                </div>
                <div class="live-details">
                    <div class="live-details__heading">
                        <h1><asp:Literal ID="litLiveTitle" runat="server" /></h1>
                        <asp:Button
                            ID="btnExitLive"
                            runat="server"
                            Text="Exit This Live"
                            CssClass="btn-end-live"
                            UseSubmitBehavior="false"
                            OnClick="btnExitLive_Click"
                            OnClientClick="return exitAdminLive(this);" />
                    </div>
                    <p class="live-details__description"><asp:Literal ID="litLiveDescription" runat="server" /></p>
                    <div class="live-chef-profile">
                        <asp:Image
                            ID="imgChefProfile"
                            runat="server"
                            CssClass="live-chef-profile__photo"
                            AlternateText="Chef profile photo" />
                        <div class="live-chef-profile__details">
                            <h2><asp:Literal ID="litChefName" runat="server" /></h2>
                            <p><asp:Literal ID="litChefProfession" runat="server" /></p>
                            <span class="live-chef-profile__country"><asp:Literal ID="litChefCountry" runat="server" /></span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="live-right-panel">
                <aside class="live-chat" aria-labelledby="liveChatTitle">
                    <div class="chat-header">
                        <h2 id="liveChatTitle">Live Chat</h2>
                    </div>
                    <div
                        id="messageList"
                        class="chat-messages"
                        role="log"
                        aria-live="polite">
                        <asp:Repeater ID="rptMessages" runat="server">
                            <ItemTemplate>
                                <div class="chat-message">
                                    <div class="chat-message-info">
                                        <strong><%#: Eval("Username") %></strong>
                                        <asp:Label
                                            runat="server"
                                            CssClass="chat-role-badge"
                                            Text="ADMIN"
                                            Visible='<%# string.Equals(Convert.ToString(Eval("Role")), "Admin", StringComparison.OrdinalIgnoreCase) %>' />
                                        <span><%#: Eval("CreatedAt", "{0:h:mm tt}") %></span>
                                    </div>
                                    <p><%#: Eval("Message") %></p>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <div class="chat-input-area">
                        <asp:TextBox
                            ID="txtChatMessage"
                            runat="server"
                            ClientIDMode="Static"
                            MaxLength="500"
                            CssClass="chat-input"
                            aria-label="Chat message"
                            placeholder="Send a message as Admin..." />
                        <button type="button" id="btnSendMessage" class="btn-send-message">Send</button>
                    </div>
                </aside>

                <div class="admin-warning-count" aria-live="polite">
                    <span>Warnings this live</span>
                    <strong><asp:Label
                            ID="lblWarningCount"
                            runat="server"
                            ClientIDMode="Static" /> / 3</strong>
                </div>
                <div class="admin-moderation-actions" aria-label="Livestream moderation">
                    <button type="button" id="btnOpenWarning" class="admin-action admin-action--warning">Issue Warning</button>
                    <button type="button" id="btnOpenSuspension" class="admin-action admin-action--danger">Suspend Live</button>
                </div>
            </div>
        </section>

        <asp:HiddenField
            ID="hfLiveSessionID"
            runat="server"
            ClientIDMode="Static" />
        <asp:HiddenField
            ID="hfJitsiRoomName"
            runat="server"
            ClientIDMode="Static" />
        <asp:HiddenField
            ID="hfChefDisplayName"
            runat="server"
            ClientIDMode="Static" />
        <asp:HiddenField
            ID="hfAdminDisplayName"
            runat="server"
            ClientIDMode="Static" />
        <asp:HiddenField
            ID="hfJitsiDomain"
            runat="server"
            ClientIDMode="Static" />

        <asp:Literal
            runat="server"
            Mode="PassThrough"
            Text='<dialog id="warningDialog" class="admin-moderation-dialog" aria-labelledby="warningDialogTitle">' />
        <div class="admin-moderation-dialog__body">
            <h2 id="warningDialogTitle">Issue warning</h2>
            <p>The livestream will continue. The Chef and viewers will see the warning.</p>
            <label for="warningReason">Warning reason</label>
            <textarea id="warningReason" maxlength="500" rows="5" required></textarea>
            <p
                id="warningError"
                class="admin-dialog-error"
                role="alert"
                hidden>
            </p>
            <div class="admin-moderation-dialog__actions">
                <button type="button" class="btn btn-secondary" data-close-dialog="warningDialog">Cancel</button>
                <button type="button" id="btnSubmitWarning" class="btn admin-action--warning">Issue Warning</button>
            </div>
        </div>
        <asp:Literal
            runat="server"
            Mode="PassThrough"
            Text="&lt;/dialog&gt;" />

        <asp:Literal
            runat="server"
            Mode="PassThrough"
            Text='<dialog id="suspensionDialog" class="admin-moderation-dialog" aria-labelledby="suspensionDialogTitle">' />
        <div class="admin-moderation-dialog__body">
            <h2 id="suspensionDialogTitle">Suspend livestream</h2>
            <p>This immediately ends the broadcast and blocks the Chef from streaming for two hours.</p>
            <label for="suspensionReason">Suspension reason</label>
            <textarea id="suspensionReason" maxlength="500" rows="5" required></textarea>
            <p
                id="suspensionError"
                class="admin-dialog-error"
                role="alert"
                hidden>
            </p>
            <div class="admin-moderation-dialog__actions">
                <button type="button" class="btn btn-secondary" data-close-dialog="suspensionDialog">Cancel</button>
                <button type="button" id="btnSubmitSuspension" class="btn admin-action--danger">Suspend Live</button>
            </div>
        </div>
        <asp:Literal
            runat="server"
            Mode="PassThrough"
            Text="&lt;/dialog&gt;" />
    </main>
</asp:Content>

<asp:Content
    ID="AdminLiveScripts"
    ContentPlaceHolderID="AdminScripts"
    runat="server">
    <script src='<%= ResolveUrl("~/Scripts/jquery-3.7.1.min.js") %>'></script>
    <script src='<%= ResolveUrl("~/Scripts/jquery.signalR-2.4.3.min.js") %>'></script>
    <script src='<%= ResolveUrl("~/signalr/hubs") %>'></script>
    <script src='<%= "https://" + System.Configuration.ConfigurationManager.AppSettings["JitsiDomain"] + "/external_api.js" %>'></script>
    <script src='<%= ResolveUrl("~/Pages/LiveSessionPage/JS/live-signalr.js") %>'></script>
    <script src='<%= ResolveUrl("~/Pages/LiveSessionPage/JS/admin-live.js") %>'></script>
</asp:Content>
