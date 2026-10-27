<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CourseActivity.ascx.cs" Inherits="CookingLah.Pages.Chef.ManageCourse.Activity.CourseActivity" %>
<section class="course-activity">
    <div class="mc-section-heading activity-heading">
        <div><h2>Practical Activity</h2><p>Define the hands-on task learners must submit for Chef review.</p></div>
        <asp:Button ID="btnEdit" runat="server" Text="Edit Practical Activity" CssClass="btn" OnClick="btnEdit_Click" />
    </div>
    <asp:Panel ID="pnlEmpty" runat="server" CssClass="card mc-empty activity-empty">
        <div class="activity-empty-mark" aria-hidden="true">+</div><h3>No practical activity added yet</h3><p>Add one clear task so learners know what to prepare and submit.</p>
        <asp:Button ID="btnAdd" runat="server" Text="+ Add Practical Activity" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
    </asp:Panel>
    <asp:Repeater ID="rptDetails" runat="server"><ItemTemplate><article class="card activity-card"><header class="activity-card-heading"><span class="activity-label">Learner task</span><h3><%#: Eval("Title") %></h3></header><section class="activity-instructions" aria-labelledby="activityInstructionsTitle"><h4 id="activityInstructionsTitle">Instructions</h4><p class="mc-copy"><%#: Eval("Instructions") %></p></section></article>
    </ItemTemplate></asp:Repeater>
    <asp:Button ID="btnDelete" runat="server" Text="Delete Practical Activity" CssClass="mc-link mc-danger-link" OnClick="btnDelete_Click" />
    <asp:Panel ID="pnlEditor" runat="server" Visible="false" CssClass="card mc-modal" data-title="Practical Activity">
        <h2><asp:Literal ID="litEditorTitle" runat="server" Mode="Encode" /></h2>
        <p class="activity-editor-intro">Use a specific title and step-by-step instructions so learners understand what evidence to submit.</p>
        <div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtTitle" Text="Activity Title *" /><asp:TextBox ID="txtTitle" runat="server" MaxLength="150" /></div>
<div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtInstructions" Text="Instructions *" /><asp:TextBox ID="txtInstructions" runat="server" TextMode="MultiLine" Rows="4" /></div>
        <div class="mc-actions">
        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" data-cancel="" />
        <asp:Button ID="btnSave" runat="server" Text="Apply Changes" CssClass="btn btn-primary" OnClick="btnSave_Click" /></div>
    </asp:Panel>
    <asp:Panel ID="pnlDelete" runat="server" Visible="false" CssClass="card mc-modal mc-delete" data-title="Delete Practical Activity?">
        <h2>Delete Practical Activity?</h2><p>Are you sure you want to delete <strong><asp:Literal ID="litDeleteName" runat="server" Mode="Encode" /></strong>?</p><p>Learner submissions related to this activity will also be removed. This action cannot be undone.</p>
        <div class="mc-actions">
        <asp:Button ID="btnCancelDelete" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" data-cancel="" />
        <asp:Button ID="btnConfirmDelete" runat="server" Text="Delete Practical Activity Permanently" CssClass="btn mc-danger" OnClick="btnConfirmDelete_Click" /></div>
    </asp:Panel>
</section>
