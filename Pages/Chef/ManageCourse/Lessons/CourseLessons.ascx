<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CourseLessons.ascx.cs" Inherits="CookingLah.Pages.Chef.ManageCourse.Lessons.CourseLessons" %>
<section class="course-lessons">
    <div class="mc-section-heading"><h2>Lessons</h2><asp:Button ID="btnAddLesson" runat="server" Text="+ Add Lesson" CssClass="btn btn-primary" OnClick="btnAddLesson_Click" /></div>
    <asp:Panel ID="pnlEmpty" runat="server" CssClass="card mc-empty"><p>No lessons have been added. Add the first cooking step.</p></asp:Panel>
    <asp:Repeater ID="rptLessons" runat="server" OnItemCommand="rptLessons_ItemCommand"><ItemTemplate>
        <article class="card lesson-card">
            <span class="lesson-number">Lesson <%#: Eval("LessonOrder") %></span><h3><%#: Eval("Title") %></h3><p class="mc-copy"><%#: Eval("Description") %></p>
            <details><summary>View lesson content</summary><p class="mc-copy"><%#: Eval("Instructions") %></p>
                <asp:Image runat="server" CssClass="lesson-image" ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("LessonImage"))) %>' Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("LessonImage")))) %>' AlternateText="Lesson photo" />
                <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("LessonVideo")))) %>'>
                    <button type="button" class="btn lesson-video-trigger" data-video-open=""
                        data-video-url='<%#: CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("LessonVideo"))) %>'
                        data-video-title='<%#: Eval("Title") %>'>Watch lesson video</button>
                </asp:PlaceHolder>
            </details>
            <div class="mc-actions">
                <asp:LinkButton runat="server" Text="Edit" CssClass="btn" CommandName="EditLesson" CommandArgument='<%# Eval("LessonID") %>' Visible='<%# CanEdit %>' />
                <asp:LinkButton runat="server" Text="Move Up" CssClass="mc-link" CommandName="MoveUp" CommandArgument='<%# Eval("LessonID") %>' Visible='<%# CanEdit && Convert.ToInt32(Eval("LessonOrder")) > 1 %>' />
                <asp:LinkButton runat="server" Text="Move Down" CssClass="mc-link" CommandName="MoveDown" CommandArgument='<%# Eval("LessonID") %>' Visible='<%# CanEdit && Convert.ToInt32(Eval("LessonOrder")) < LessonCount %>' />
                <asp:LinkButton runat="server" Text="Delete" CssClass="mc-link mc-danger-link" CommandName="DeleteLesson" CommandArgument='<%# Eval("LessonID") %>' Visible='<%# CanEdit %>' />
            </div>
        </article>
    </ItemTemplate></asp:Repeater>
    <asp:Panel ID="pnlPager" runat="server" CssClass="mc-pager">
    <asp:Button ID="btnPrevious" runat="server" Text="Previous" CssClass="btn" OnClick="btnPrevious_Click" />
    <asp:Label ID="lblPage" runat="server" />
    <asp:Button ID="btnNext" runat="server" Text="Next" CssClass="btn" OnClick="btnNext_Click" /></asp:Panel>
    <asp:Panel ID="pnlEditor" runat="server" Visible="false" CssClass="card mc-modal" data-title="Lesson">
        <h2><asp:Literal ID="litEditorTitle" runat="server" Mode="Encode" /></h2>
        <div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtTitle" Text="Lesson Title *" /><asp:TextBox ID="txtTitle" runat="server" MaxLength="150" /></div>
        <div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtDescription" Text="Description" /><asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" MaxLength="500" /></div>
        <div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtInstructions" Text="Instructions *" /><asp:TextBox ID="txtInstructions" runat="server" TextMode="MultiLine" Rows="6" /></div>
        <div class="mc-field">
        <asp:Label runat="server" AssociatedControlID="fuImage" Text="Lesson Image (JPG or PNG, up to 3 MB)" />
        <asp:FileUpload ID="fuImage" runat="server" accept="image/png,image/jpeg" data-image-input="" data-max-file-mb="3" /><img class="mc-image-preview" data-image-preview="" alt="Selected lesson image" hidden /></div>
        <div class="mc-field"><asp:Label runat="server" AssociatedControlID="fuVideo" Text="Lesson Video (MP4, up to 20 MB)" /><asp:FileUpload ID="fuVideo" runat="server" accept="video/mp4" data-max-file-mb="20" /></div>
        <p class="text-muted">Leave file fields empty to keep existing media.</p>
        <div class="mc-actions">
        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" data-cancel="" />
        <asp:Button ID="btnSaveLesson" runat="server" Text="Save Lesson" CssClass="btn btn-primary" OnClick="btnSaveLesson_Click" /></div>
    </asp:Panel>
    <asp:Panel ID="pnlDelete" runat="server" Visible="false" CssClass="card mc-modal mc-delete" data-title="Delete Lesson?">
        <h2>Delete Lesson?</h2><p>Delete <strong><asp:Literal ID="litDeleteName" runat="server" Mode="Encode" /></strong>? The remaining lessons will be renumbered. This action cannot be undone.</p>
        <div class="mc-actions">
        <asp:Button ID="btnCancelDelete" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" data-cancel="" />
        <asp:Button ID="btnConfirmDelete" runat="server" Text="Delete Lesson Permanently" CssClass="btn mc-danger" OnClick="btnConfirmDelete_Click" /></div>
    </asp:Panel>
</section>
