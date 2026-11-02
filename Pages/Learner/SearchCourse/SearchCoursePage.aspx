<%@ Page Title="Explore Courses | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Learner/Learner.Master" AutoEventWireup="true" CodeBehind="SearchCoursePage.aspx.cs" Inherits="CookingLah.Pages.Learner.SearchCourse.SearchCoursePage" %>
<%@ Register Src="~/Pages/Shared/CourseSearch/SearchCourse.ascx" TagPrefix="course" TagName="SearchCourse" %>

<asp:Content
    ID="SearchHead"
    ContentPlaceHolderID="LearnerHead"
    runat="server">
    <link href="<%= ResolveUrl("~/Pages/Shared/CourseSearch/SearchCourse.css?v=20261027b") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Shared/CourseSearch/ViewCourseOverivew.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content
    ID="SearchContent"
    ContentPlaceHolderID="LearnerContent"
    runat="server">
    <course:SearchCourse ID="SearchCourse1" runat="server" IsLearner="true" />
</asp:Content>

<asp:Content
    ID="SearchScripts"
    ContentPlaceHolderID="LearnerScripts"
    runat="server">
    <script src="<%= ResolveUrl("~/Pages/Shared/CourseSearch/SearchCourse.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Shared/CourseSearch/ViewCourseOverivew.js") %>" defer></script>
</asp:Content>

