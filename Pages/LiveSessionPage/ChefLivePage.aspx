<%@ Page
    Title="Cooking LAH | Live Stream"
    Language="C#"
    MasterPageFile="~/Pages/Chef/Chef.Master"
    AutoEventWireup="true"
    CodeBehind="ChefLivePage.aspx.cs"
    Inherits="CookingLah.Pages.LiveSessionPage.ChefLivePage" %>


<asp:Content ID="ChefLiveHead" ContentPlaceHolderID="ChefHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/LiveSessionPage/CSS/ChefLivePage.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="ChefLiveContent" ContentPlaceHolderID="ChefContent" runat="server">
    <div class="chef-live-page">

        <!-- LIVE HEADER -->
        <section class="live-header">
            <div>
                <span class="live-badge">
                    ● LIVE
                </span>
            </div>
            <div class="live-header-actions">
                <span class="chef-warning-count" aria-live="polite">
                    Warnings <strong><asp:Label ID="lblWarningCount" runat="server" ClientIDMode="Static" /> / 3</strong>
                </span>
                <span class="live-participant-count">
                    <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><circle cx="9" cy="8" r="3" /><path d="M3 19v-2a6 6 0 0 1 12 0v2" /><circle cx="17" cy="9" r="2.5" /><path d="M16 14a5 5 0 0 1 5 5" /></svg>
                    <span id="liveParticipantCount" role="status" aria-live="polite">Connecting...</span>
                </span>
            </div>
        </section>

        <dialog id="warningDialog" class="chef-warning-dialog" aria-labelledby="warningTitle">
            <div class="chef-warning-dialog__body">
                <h2 id="warningTitle">Admin warning</h2>
                <p>An administrator has issued a warning for this livestream.</p>
                <div class="chef-warning-dialog__reason">
                    <strong>Reason</strong>
                    <p id="warningReason"></p>
                </div>
                <div class="chef-warning-dialog__actions">
                    <button type="button" id="btnAcknowledgeWarning" class="btn btn-primary">I understand</button>
                </div>
            </div>
        </dialog>


        <!-- MAIN LIVE AREA -->
        <section class="live-layout">
            <!-- JITSI -->
            <div class="live-video-section">
                <div
                    id="jitsi-container"
                    class="jitsi-container">
                </div>
                <div class="live-details">
                    <h1><asp:Literal ID="litLiveTitle" runat="server" /></h1>
                    <p><asp:Literal ID="litLiveDescription" runat="server" /></p>
                </div>
            </div>


            <!-- LIVE CHAT -->
            <div class="live-right-panel">
                <aside class="live-chat" aria-labelledby="liveChatTitle">
                    <div class="chat-header">
                        <h2 id="liveChatTitle">
                            Live Chat
                        </h2>
                    </div>


                    <!-- Existing + new messages -->
                    <div
                        id="messageList"
                        class="chat-messages"
                        role="log"
                        aria-live="polite">
                        <asp:Repeater
                            ID="rptMessages"
                            runat="server">
                            <ItemTemplate>
                                <div class="chat-message">
                                    <div class="chat-message-info">
                                        <strong>
                                            <%#: Eval("Username") %>
                                        </strong>

                                        <asp:Label runat="server" CssClass="chat-role-badge" Text="ADMIN"
                                            Visible='<%# string.Equals(Convert.ToString(Eval("Role")), "Admin", StringComparison.OrdinalIgnoreCase) %>' />

                                        <span>
                                            <%#: Eval(
                                                "CreatedAt",
                                                "{0:h:mm tt}"
                                            ) %>
                                        </span>
                                    </div>
                                    <p>
                                        <%#: Eval("Message") %>
                                    </p>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>


                    <!-- CHAT INPUT -->
                    <div class="chat-input-area">
                        <asp:TextBox
                            ID="txtChatMessage"
                            runat="server"
                            ClientIDMode="Static"
                            MaxLength="500"
                            CssClass="chat-input"
                            aria-label="Chat message"
                            placeholder="Send a message..." />

                        <button
                            type="button"
                            id="btnSendMessage"
                            class="btn-send-message">
                            Send
                        </button>
                    </div>

                </aside>

                <asp:Button
                    ID="btnEndLive"
                    runat="server"
                    Text="End Live Session"
                    CssClass="btn-end-live"
                    OnClick="btnEndLive_Click"
                    OnClientClick="return confirm('Are you sure you want to end this live session?');" />
            </div>
        </section>


        <!-- VALUES FOR JAVASCRIPT -->
        <asp:HiddenField
            ID="hfLiveSessionID"
            runat="server"
            ClientIDMode="Static" />

        <asp:HiddenField
            ID="hfJitsiRoomName"
            runat="server"
            ClientIDMode="Static" />

        <asp:HiddenField
            ID="hfChefID"
            runat="server"
            ClientIDMode="Static" />

        <asp:HiddenField
            ID="hfChefDisplayName"
            runat="server"
            ClientIDMode="Static" />

    </div>
</asp:Content>

<asp:Content ID="ChefLiveScripts" ContentPlaceHolderID="LiveScripts" runat="server">
    <!-- jQuery -->
    <script src='<%= ResolveUrl("~/Scripts/jquery-3.7.1.min.js") %>'></script>
    <!-- SignalR -->
    <script src='<%= ResolveUrl("~/Scripts/jquery.signalR-2.4.3.min.js") %>'></script>
    <script src='<%= ResolveUrl("~/signalr/hubs") %>'></script>
    <!-- Jitsi -->
    <script>window.jitsiDomain = '<%= System.Web.HttpUtility.JavaScriptStringEncode(System.Configuration.ConfigurationManager.AppSettings["JitsiDomain"]) %>';</script>
    <script>window.liveSessionId = Number(document.getElementById("hfLiveSessionID").value);</script>
    <script src="<%= "https://" + System.Configuration.ConfigurationManager.AppSettings["JitsiDomain"] + "/external_api.js" %>"></script>

    <!-- Shared SignalR logic -->
    <script src='<%= ResolveUrl("~/Pages/LiveSessionPage/JS/live-signalr.js") %>'></script>

    <!-- Chef page logic -->
    <script src='<%= ResolveUrl("~/Pages/LiveSessionPage/JS/chef-live.js") %>'></script>

</asp:Content>
