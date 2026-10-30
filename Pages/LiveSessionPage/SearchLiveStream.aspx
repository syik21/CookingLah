<%@ Page Title="Live Cooking | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Learner/Learner.Master" AutoEventWireup="true" CodeBehind="SearchLiveStream.aspx.cs" Inherits="CookingLah.Pages.LiveSessionPage.SearchLiveStream" %>

<asp:Content ID="SearchLiveHead" ContentPlaceHolderID="SharedLiveHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/LiveSessionPage/CSS/SearchLiveStream.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="SearchLiveContent" ContentPlaceHolderID="SharedLiveContent" runat="server">
    <main class="live-search" aria-labelledby="liveSearchTitle">
        <header class="live-search__header">
            <div>
                <span class="live-search__label">Live kitchen</span>
                <h1 id="liveSearchTitle" class="live-search__title">Live Cooking</h1>
                <asp:Literal ID="litSubtitle" runat="server" />
            </div>
            <span class="live-search__mark" aria-hidden="true">LIVE</span>
        </header>

        <asp:Panel ID="pnlSearch" runat="server" CssClass="live-search-bar" DefaultButton="btnSearch">
            <asp:Label ID="lblLiveSearch" runat="server" AssociatedControlID="txtLiveSearch" CssClass="sr-only" Text="Search live streams or Chefs" />
            <asp:TextBox ID="txtLiveSearch" runat="server" CssClass="live-search-bar__input" MaxLength="150"
                placeholder="Search live streams or chefs..." autocomplete="off" />
            <div class="live-search-bar__actions">
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnClearSearch_Click" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlMessage" runat="server" CssClass="live-search-message" Visible="false" role="alert">
            <asp:Label ID="lblMessage" runat="server" />
        </asp:Panel>

        <section class="live-now" aria-labelledby="liveNowTitle">
            <header class="live-now__header">
                <h2 id="liveNowTitle" class="live-now__title"><span aria-hidden="true"></span>Live now</h2>
                <asp:Label ID="lblResultSummary" runat="server" CssClass="live-now__summary" />
            </header>

            <div class="live-now__grid">
                <asp:Repeater ID="rptLiveStreams" runat="server">
                    <ItemTemplate>
                        <article class="live-card">
                            <div class="live-card__thumbnail-wrap">
                                <asp:Image runat="server" CssClass="live-card__thumbnail" loading="lazy"
                                    ImageUrl='<%# GetThumbnailUrl(Eval("ThumbnailPath")) %>'
                                    Visible='<%# HasImage(Eval("ThumbnailPath")) %>'
                                    AlternateText='<%# Convert.ToString(Eval("Title")) + " livestream thumbnail" %>' />
                                <asp:Panel runat="server" CssClass="live-card__thumbnail-placeholder"
                                    Visible='<%# !HasImage(Eval("ThumbnailPath")) %>'>
                                    <img runat="server" src="~/F_Assets/mascot.png" alt="" />
                                    <span>Live cooking in progress</span>
                                </asp:Panel>
                                <span class="live-card__badge">Live</span>
                            </div>

                            <div class="live-card__body">
                                <h3 class="live-card__title"><%#: Eval("Title") %></h3>
                                <div class="live-card__chef">
                                    <asp:Image runat="server" CssClass="live-card__chef-avatar" loading="lazy"
                                        ImageUrl='<%# GetProfileImageUrl(Eval("ProfileImage")) %>'
                                        AlternateText='<%# Convert.ToString(Eval("ChefName")) + " profile photo" %>' />
                                    <div class="live-card__chef-info">
                                        <span class="live-card__chef-name"><%#: Eval("ChefName") %></span>
                                        <span><%#: FormatStartedAt(Eval("StartedAt")) %></span>
                                    </div>
                                </div>

                                <asp:HyperLink runat="server" CssClass="live-card__action"
                                    NavigateUrl='<%# GetLiveStreamUrl(Convert.ToInt32(Eval("LiveSessionID"))) %>'
                                    Text='<%# GetActionText() %>' />
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </section>

        <asp:Panel ID="pnlNoLiveStreams" runat="server" CssClass="live-empty" Visible="false">
            <span class="live-empty__icon" aria-hidden="true">X</span>
            <h2 class="live-empty__title">No one is live right now.</h2>
            <p class="live-empty__text">Check back later for live cooking streams.</p>
        </asp:Panel>

        <asp:Panel ID="pnlNoSearchResults" runat="server" CssClass="live-empty" Visible="false">
            <span class="live-empty__icon" aria-hidden="true">?</span>
            <h2 class="live-empty__title">No live streams matched “<asp:Literal ID="litSearchTerm" runat="server" />”.</h2>
            <p class="live-empty__text">Try another stream title or Chef name.</p>
        </asp:Panel>
    </main>
</asp:Content>

<asp:Content ID="SearchLiveScripts" ContentPlaceHolderID="SharedLiveScripts" runat="server" />
