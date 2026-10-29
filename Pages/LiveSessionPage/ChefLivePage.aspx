<%@ Page Title="Cooking LAH | My Live Stream" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="ChefLivePage.aspx.cs" Inherits="CookingLah.Pages.LiveSessionPage.ChefLivePage" %>

<asp:Content ID="MapNavigation" ContentPlaceHolderID="LiveScripts" runat="server">
<script src='<%= ResolveUrl("~/Scripts/jquery-3.7.0.min.js") %>'></script>

    <script src='<%= ResolveUrl("~/Scripts/jquery.signalR-2.4.3.min.js") %>'></script>

    <script src='<%= ResolveUrl("~/signalr/hubs") %>'></script>

    <script src='<%= ResolveUrl("~/Scripts/LiveStreaming/live-signalr.js") %>'></script>

</script>
</asp:Content>

