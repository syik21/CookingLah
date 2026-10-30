<%@ Page Title="Cooking LAH | Live Stream" Language="C#" MasterPageFile="~/Pages/Learner/Learner.Master" AutoEventWireup="true" CodeBehind="LearnerWatchLivePage.aspx.cs" Inherits="CookingLah.Pages.LiveSessionPage.LearnerWatchLivePage" %>

<asp:Content ID="LearnerLiveHead" ContentPlaceHolderID="LearnerLiveHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/LiveSessionPage/CSS/LearnerLivePage.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="LearnerLiveContent" ContentPlaceHolderID="LearnerLiveContent" runat="server">
        <div class="learner-live-page">

        <!-- LIVE HEADER -->
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
                    Text="Exit This Live"
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
            ID="hfLearnerID"
            runat="server"
            ClientIDMode="Static" />

    </div>
</asp:Content>

<asp:Content ID="LearnerLiveScripts" ContentPlaceHolderID="LiveScripts" runat="server">
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
    <script src='<%= ResolveUrl("~/Pages/LiveSessionPage/JS/learner-live.js") %>'></script>

</asp:Content>
