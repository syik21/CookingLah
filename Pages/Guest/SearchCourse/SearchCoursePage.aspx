<%@ Page Title="Explore Courses | Cooking LAH" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SearchCoursePage.aspx.cs" Inherits="CookingLah.Pages.Guest.SearchCourse.SearchCoursePage" %>
<%@ Register Src="~/Pages/Shared/CourseSearch/SearchCourse.ascx" TagPrefix="course" TagName="SearchCourse" %>

<asp:Content ID="SearchHead" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Shared/CourseSearch/SearchCourse.css?v=20261027b") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Shared/CourseSearch/ViewCourseOverivew.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="SearchContent" ContentPlaceHolderID="MainContent" runat="server">
    <course:SearchCourse ID="SearchCourse1" runat="server" IsLearner="false" />
</asp:Content>

<asp:Content ID="SearchScripts" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Shared/CourseSearch/SearchCourse.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Shared/CourseSearch/ViewCourseOverivew.js") %>" defer></script>
</asp:Content>

