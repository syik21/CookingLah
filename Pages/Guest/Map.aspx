<%@ Page Title="Food Map" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Map.aspx.cs" Inherits="CookingLah.Pages.Guest.Map" %>
<%@ Register Src="~/Pages/Shared/FoodMap/FoodMapView.ascx" TagPrefix="shared" TagName="FoodMapView" %>

<asp:Content
    ID="MainContent"
    ContentPlaceHolderID="MainContent"
    runat="server">
    <shared:FoodMapView ID="FoodMapView1" runat="server" />
</asp:Content>
