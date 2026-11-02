<%@ Page Title="Food Map" Language="C#" MasterPageFile="~/Pages/Learner/Learner.Master" AutoEventWireup="true" CodeBehind="Map.aspx.cs" Inherits="CookingLah.Pages.Learner.Map.Map" %>
<%@ Register Src="~/Pages/Shared/FoodMap/FoodMapView.ascx" TagPrefix="shared" TagName="FoodMapView" %>

<asp:Content
    ID="MainContent"
    ContentPlaceHolderID="LearnerContent"
    runat="server">
    <shared:FoodMapView ID="FoodMapView1" runat="server" />
</asp:Content>
