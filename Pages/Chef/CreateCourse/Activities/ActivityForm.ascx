<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ActivityForm.ascx.cs" Inherits="CookingLah.Pages.Chef.CreateCourse.Activities.ActivityForm" %>
<div class="activity-form">
    <p class="builder-hint">Optional. Give learners a practical task to try after the course. Leave both fields blank to skip.</p>
    <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtActivityTitle" Text="Activity title" /><asp:TextBox ID="txtActivityTitle" runat="server" MaxLength="150" /></div>
    <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtActivityInstructions" Text="Instructions" /><asp:TextBox ID="txtActivityInstructions" runat="server" TextMode="MultiLine" Rows="8" /></div>
    <p class="activity-tip">Explain what to cook, what to practise, and what learners should include in their submission.</p>
</div>
