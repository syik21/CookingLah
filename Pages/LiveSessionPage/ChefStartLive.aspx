<%@ Page Title="Start Live | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="ChefStartLive.aspx.cs" Inherits="CookingLah.Pages.LiveSessionPage.ChefStartLive" %>
<asp:Content ID="ChefStartLiveHead" ContentPlaceHolderID="ChefHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/LiveSessionPage/ChefStartLive.css") %>" rel="stylesheet" />
</asp:Content>
<asp:Content ID="ChefStartLiveContent" ContentPlaceHolderID="ChefContent" runat="server">
    <main class="chef-live-start" aria-labelledby="pageTitle">
        <asp:Panel ID="pnlMessage" runat="server" CssClass="chef-live-message" Visible="false" role="status" aria-live="polite">
            <asp:Label ID="lblMessage" runat="server" />
        </asp:Panel>
        <section class="chef-live-hero" aria-labelledby="pageTitle">
            <div class="chef-live-status-card">
                <asp:Label ID="lblStatusBadge" runat="server" CssClass="chef-live-status-badge" />
                <h1 id="pageTitle"><asp:Literal ID="litStatusTitle" runat="server" /></h1>
                <p><asp:Literal ID="litStatusText" runat="server" /></p>
                <asp:Label ID="lblCurrentTitle" runat="server" CssClass="chef-live-current-title" Visible="false" />
                <asp:Panel ID="pnlStreamingPenalty" runat="server" CssClass="chef-live-suspended" Visible="false">
                    <p>You cannot start another livestream until <strong><asp:Label ID="lblPenaltyEndTime" runat="server" /></strong>.</p>
                    <p><strong>Reason:</strong><br /><asp:Label ID="lblPenaltyReason" runat="server" /></p>
                </asp:Panel>
                <asp:Button ID="btnReturnToLive" runat="server" Text="Return to Live" CssClass="btn btn-primary chef-live-return"
                    CausesValidation="false" Visible="false" OnClick="btnReturnToLive_Click" />
            </div>
            <div class="chef-live-preview" aria-live="polite">
                <span class="chef-live-preview__status"><asp:Literal ID="litPreviewStatus" runat="server" /></span>
                <h2><asp:Literal ID="litPreviewTitle" runat="server" /></h2>
                <p><asp:Literal ID="litPreviewText" runat="server" /></p>
            </div>
        </section>
        <section class="chef-channel-header" aria-label="Chef channel">
            <asp:Image ID="imgChefAvatar" runat="server" CssClass="chef-channel-header__avatar" AlternateText="Chef profile photo" />
            <div><h2><asp:Label ID="lblChefName" runat="server" /></h2><p>Cooking Instructor</p></div>
        </section>
        <div class="chef-live-tabs" role="tablist" aria-label="Livestream information">
            <button type="button" class="chef-live-tab chef-live-tab--active" id="tabGoLive" role="tab" aria-selected="true" aria-controls="panelGoLive" data-tab="panelGoLive">Go Live</button>
            <button type="button" class="chef-live-tab" id="tabAbout" role="tab" aria-selected="false" aria-controls="panelAbout" data-tab="panelAbout">About Stream</button>
            <button type="button" class="chef-live-tab" id="tabRules" role="tab" aria-selected="false" aria-controls="panelRules" data-tab="panelRules">Rules</button>
        </div>
        <section id="panelGoLive" class="chef-live-content" role="tabpanel" aria-labelledby="tabGoLive">
            <asp:Panel ID="pnlGoLiveForm" runat="server" CssClass="chef-live-form">
                <header><h2>Go Live</h2><p>Share a cooking lesson with the Cooking LAH community.</p></header>
                <div class="chef-live-form__layout">
                    <div class="chef-live-form__fields">
                        <div class="chef-live-form-group">
                            <asp:Label ID="lblLiveTitle" runat="server" AssociatedControlID="txtLiveTitle" Text="Stream Title *" />
                            <asp:TextBox ID="txtLiveTitle" runat="server" MaxLength="150" placeholder="e.g. Cooking Traditional Nasi Lemak" aria-required="true" />
                        </div>
                        <div class="chef-live-form-group">
                            <asp:Label ID="lblLiveDescription" runat="server" AssociatedControlID="txtLiveDescription" Text="Description" />
                            <asp:TextBox ID="txtLiveDescription" runat="server" TextMode="MultiLine" Rows="6" MaxLength="1000" placeholder="Tell learners what you will be cooking during this livestream..." />
                        </div>
                    </div>
                    <div class="chef-live-form-group chef-live-thumbnail">
                        <asp:Label ID="lblThumbnail" runat="server" AssociatedControlID="fuThumbnail" Text="Thumbnail" />
                        <div class="chef-live-thumbnail-preview" id="thumbnailPreview">
                            <img id="thumbnailPreviewImage" alt="Selected livestream thumbnail preview" hidden="hidden" />
                            <span id="thumbnailPreviewPlaceholder">Upload Thumbnail / Preview</span>
                        </div>
                        <asp:FileUpload ID="fuThumbnail" runat="server" ClientIDMode="Static" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" data-max-file-mb="3" />
                        <small>Optional. JPG, JPEG, PNG or WEBP up to 3 MB.</small>
                    </div>
                </div>
                <div class="chef-live-form__action">
                    <asp:Button ID="btnStartLive" runat="server" Text="Start Live Now" CssClass="btn btn-primary" OnClick="btnStartLive_Click" />
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlUnavailable" runat="server" CssClass="chef-live-unavailable" Visible="false">
                <h2><asp:Label ID="lblUnavailableTitle" runat="server" /></h2>
                <p><asp:Label ID="lblUnavailableText" runat="server" /></p>
            </asp:Panel>
        </section>
        <section id="panelAbout" class="chef-live-content" role="tabpanel" aria-labelledby="tabAbout" hidden="hidden">
            <h2>About your stream</h2><p>Your title, description and thumbnail help learners understand what you will cook before joining.</p>
        </section>
        <section id="panelRules" class="chef-live-content" role="tabpanel" aria-labelledby="tabRules" hidden="hidden">
            <h2>Community rules</h2><p>Keep your stream respectful, cooking-focused and safe for the Cooking LAH community.</p>
        </section>
    </main>
</asp:Content>
<asp:Content ID="ChefStartLiveScripts" ContentPlaceHolderID="ChefScripts" runat="server">
    <script src="<%= ResolveUrl("~/F_JavaScripts/UploadValidation.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/LiveSessionPage/ChefStartLive.js") %>" defer></script>
</asp:Content>
