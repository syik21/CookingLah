<%@ Page Title="Live Cooking | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="ChefStartLive.aspx.cs" Inherits="CookingLah.Pages.LiveSessionPage.ChefStartLive" %>

<%-- Page stylesheet --%>
<asp:Content ID="ChefStartLiveHead" ContentPlaceHolderID="ChefHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/LiveSessionPage/ChefStartLive.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="ChefStartLiveContent" ContentPlaceHolderID="ChefContent" runat="server">
    <main class="chef-start-live" aria-labelledby="chefStartLiveTitle">

        <%-- Page heading --%>
        <header class="chef-start-live__header">
            <p class="chef-start-live__eyebrow">Your live kitchen</p>
            <h1 id="chefStartLiveTitle">Live Cooking</h1>
            <p>Start a cooking livestream now or schedule one for later.</p>
        </header>

        <%-- Feedback and streaming restrictions --%>
        <asp:Panel ID="pnlMessage" runat="server" CssClass="live-message" Visible="false" role="status" aria-live="polite">
            <asp:Label ID="lblMessage" runat="server" />
        </asp:Panel>

        <asp:Panel ID="pnlStreamingPenalty" runat="server" CssClass="live-penalty-panel" Visible="false" role="alert">
            <div class="live-panel-mark" aria-hidden="true">!</div>
            <div>
                <h2>Livestream temporarily unavailable</h2>
                <p>
                    You cannot start a livestream until
                    <strong><asp:Label ID="lblPenaltyEndTime" runat="server" /></strong>.
                </p>
                <p class="live-panel-note">
                    Reason: <asp:Label ID="lblPenaltyReason" runat="server" />
                </p>
            </div>
        </asp:Panel>

        <%-- Link to an active livestream --%>
        <asp:Panel ID="pnlActiveLive" runat="server" CssClass="live-active-panel" Visible="false">
            <div>
                <span class="live-status live-status--live">Live</span>
                <p>You currently have an active livestream.</p>
                <h2><asp:Label ID="lblActiveLiveTitle" runat="server" /></h2>
            </div>
            <asp:Button ID="btnReturnToLive" runat="server" Text="Return to Live"
                CssClass="btn btn-primary" CausesValidation="false" OnClick="btnReturnToLive_Click" />
        </asp:Panel>

        <%-- Choose between starting now and scheduling --%>
        <section class="live-mode-selector" aria-label="Choose livestream mode">
            <button type="button" id="btnModeNow" class="live-mode-option live-mode-option--active" aria-pressed="true">
                <span>Go Live Now</span>
                <small>Start your cooking livestream immediately.</small>
            </button>
            <button type="button" id="btnModeSchedule" class="live-mode-option" aria-pressed="false">
                <span>Schedule Live</span>
                <small>Plan a livestream for a future date and time.</small>
            </button>
        </section>
        <asp:HiddenField ID="hfLiveMode" runat="server" ClientIDMode="Static" Value="Now" />

        <%-- Livestream details form --%>
        <section class="live-form" aria-labelledby="streamDetailsTitle">
            <header class="live-form__header">
                <span aria-hidden="true">01</span>
                <div>
                    <h2 id="streamDetailsTitle">Stream Details</h2>
                    <p>Fields marked * are required.</p>
                </div>
            </header>

            <div class="live-form__layout">
                <div class="live-form__fields">
                    <div class="live-form__group">
                        <asp:Label ID="lblLiveTitle" runat="server" AssociatedControlID="txtLiveTitle"
                            CssClass="live-form__label" Text="Stream Title *" />
                        <asp:TextBox ID="txtLiveTitle" runat="server" MaxLength="150"
                            placeholder="e.g. Cooking Traditional Nasi Lemak" aria-required="true" autocomplete="off" />
                    </div>

                    <div class="live-form__group">
                        <div class="live-form__label-row">
                            <asp:Label ID="lblLiveDescription" runat="server" AssociatedControlID="txtLiveDescription"
                                CssClass="live-form__label" Text="Description" />
                            <small>Optional</small>
                        </div>
                        <asp:TextBox ID="txtLiveDescription" runat="server" TextMode="MultiLine" Rows="5"
                            MaxLength="1000" placeholder="Tell learners what you will be cooking during this livestream..." />
                    </div>
                </div>

                <%-- Thumbnail upload and preview --%>
                <div class="live-form__group live-thumbnail">
                    <asp:Label ID="lblThumbnail" runat="server" AssociatedControlID="fuThumbnail"
                        CssClass="live-form__label" Text="Thumbnail" />
                    <div id="thumbnailPreview" class="live-thumbnail__preview">
                        <img src="" id="thumbnailPreviewImage" alt="Selected livestream thumbnail preview" hidden="hidden" />
                        <span id="thumbnailPreviewPlaceholder">Thumbnail Preview</span>
                    </div>
                    <asp:FileUpload ID="fuThumbnail" runat="server" ClientIDMode="Static"
                        CssClass="live-thumbnail__input" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" />
                    <small>JPG, JPEG, PNG or WEBP</small>
                </div>
            </div>

            <%-- Date and time inputs for scheduled streams --%>
            <div id="liveSchedule" class="live-schedule" hidden="hidden">
                <h3>Schedule</h3>
                <p>Choose a future start time. Scheduled sessions must be at least 2 hours apart.</p>
                <div class="live-schedule__grid">
                    <div class="live-form__group">
                        <asp:Label ID="lblScheduledDate" runat="server" AssociatedControlID="txtScheduledDate"
                            CssClass="live-form__label" Text="Scheduled Date *" />
                        <asp:TextBox ID="txtScheduledDate" runat="server" ClientIDMode="Static" TextMode="Date" />
                    </div>
                    <div class="live-form__group">
                        <asp:Label ID="lblScheduledTime" runat="server" AssociatedControlID="txtScheduledTime"
                            CssClass="live-form__label" Text="Scheduled Time *" />
                        <asp:TextBox ID="txtScheduledTime" runat="server" ClientIDMode="Static" TextMode="Time" />
                    </div>
                </div>
            </div>

            <%-- Start or schedule action buttons --%>
            <div class="live-action">
                <div id="actionNow">
                    <asp:Button ID="btnStartLive" runat="server" ClientIDMode="Static" Text="Start Live"
                        CssClass="btn btn-primary" OnClick="btnStartLive_Click" />
                </div>
                <div id="actionSchedule" hidden="hidden">
                    <asp:Button ID="btnScheduleLive" runat="server" ClientIDMode="Static" Text="Schedule Session"
                        CssClass="btn btn-primary" OnClick="btnScheduleLive_Click" />
                </div>
            </div>
        </section>

        <%-- Upcoming scheduled livestreams --%>
        <section class="scheduled-live" aria-labelledby="upcomingLiveTitle">
            <header class="scheduled-live__header">
                <h2 id="upcomingLiveTitle">Upcoming Live Sessions</h2>
                <p>Manage livestreams you have scheduled for later.</p>
            </header>

            <div class="scheduled-live__list">
                <asp:Repeater ID="rptScheduledSessions" runat="server" OnItemCommand="rptScheduledSessions_ItemCommand">
                    <ItemTemplate>
                        <article class="scheduled-live-card">
                            <div class="scheduled-live-card__thumbnail">
                                <asp:Image runat="server" ImageUrl='<%# Eval("ThumbnailPath") %>'
                                    AlternateText='<%# "Thumbnail for " + Eval("Title") %>' />
                            </div>

                            <div class="scheduled-live-card__content">
                                <span class="live-status">Scheduled</span>
                                <h3><%#: Eval("Title") %></h3>
                                <p class="scheduled-live-card__meta">
                                    <time datetime='<%# Eval("ScheduledStartAt", "{0:yyyy-MM-ddTHH:mm:ss}") %>'>
                                        <%#: Eval("ScheduledStartAt", "{0:dd MMM yyyy}") %>
                                        <span aria-hidden="true">&bull;</span>
                                        <%#: Eval("ScheduledStartAt", "{0:h:mm tt}") %>
                                    </time>
                                </p>
                                <div class="scheduled-live-card__actions">
                                    <asp:LinkButton ID="btnCancelScheduledLive" runat="server" CssClass="btn"
                                        Text="Cancel" CommandName="CancelLive" CommandArgument='<%# Eval("LiveSessionID") %>'
                                        CausesValidation="false" />
                                    <asp:LinkButton ID="btnStartScheduledLive" runat="server" CssClass="btn btn-primary"
                                        Text="Start Live" CommandName="StartLive" CommandArgument='<%# Eval("LiveSessionID") %>'
                                        CausesValidation="false" />
                                </div>
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <%-- Message when there are no scheduled sessions --%>
            <asp:Panel ID="pnlNoScheduledSessions" runat="server" CssClass="live-empty-state">
                <span aria-hidden="true">○</span>
                <h3>No upcoming livestreams</h3>
                <p>You have not scheduled any livestreams yet.</p>
            </asp:Panel>
        </section>
    </main>
</asp:Content>

<%-- Page JavaScript --%>
<asp:Content ID="ChefStartLiveScripts" ContentPlaceHolderID="ChefScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/LiveSessionPage/ChefStartLive.js") %>" defer></script>
</asp:Content>
