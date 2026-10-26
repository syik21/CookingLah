<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CourseActivity.ascx.cs" Inherits="CookingLah.Pages.Chef.ManageCourse.Activity.CourseActivity" %>
<section class="course-activity">
    <div class="mc-section-heading"><h2>Practical Activity</h2><asp:Button ID="btnEdit" runat="server" Text="Edit Practical Activity" CssClass="btn" OnClick="btnEdit_Click" /></div>
    <asp:Panel ID="pnlEmpty" runat="server" CssClass="card mc-empty"><p>No Practical Activity currently exists for this course.</p>
    <asp:Button ID="btnAdd" runat="server" Text="+ Add Practical Activity" CssClass="btn btn-primary" OnClick="btnEdit_Click" /></asp:Panel>
    <asp:Repeater ID="rptDetails" runat="server"><ItemTemplate><article class="card"><h3><%#: Eval("Title") %></h3><h4>Instructions</h4><p class="mc-copy"><%#: Eval("Instructions") %></p></article>
    </ItemTemplate></asp:Repeater>
    <asp:Button ID="btnDelete" runat="server" Text="Delete Practical Activity" CssClass="mc-link mc-danger-link" OnClick="btnDelete_Click" />
    <asp:Panel ID="pnlEditor" runat="server" Visible="false" CssClass="card mc-modal" data-title="Practical Activity">
        <h2><asp:Literal ID="litEditorTitle" runat="server" Mode="Encode" /></h2>
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
