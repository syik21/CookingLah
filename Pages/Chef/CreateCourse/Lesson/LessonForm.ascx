<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LessonForm.ascx.cs" Inherits="CookingLah.Pages.Chef.CreateCourse.Lesson.LessonForm" %>
<div class="lesson-form">
    <p class="builder-hint">One lesson per cooking step. Save draft to keep your edits, including new lessons and their order.</p>
    <asp:Panel
        ID="pnlEmptyLessons"
        runat="server"
        CssClass="builder-empty">
        <h3>No lessons added yet.</h3>
        <p>Add the first cooking step to begin building your course.</p>
    </asp:Panel>
    <asp:Repeater
        ID="rptLessons"
        runat="server"
        OnItemCommand="rptLessons_ItemCommand">
        <ItemTemplate>
            <article class="lesson-card card">
                <h3>Lesson <%# Eval("LessonOrder") %></h3>
                <div class="builder-field"><asp:Label
                        runat="server"
                        AssociatedControlID="txtLessonTitle"
                        Text="Title *" /><asp:TextBox
                        ID="txtLessonTitle"
                        runat="server"
                        Text='<%# Eval("Title") %>'
                        MaxLength="150" /></div>
                <div class="builder-field"><asp:Label
                        runat="server"
                        AssociatedControlID="txtLessonDescription"
                        Text="Description" /><asp:TextBox
                        ID="txtLessonDescription"
                        runat="server"
                        Text='<%# Eval("Description") %>'
                        TextMode="MultiLine"
                        Rows="2" /></div>
                <div class="builder-field"><asp:Label
                        runat="server"
                        AssociatedControlID="txtLessonInstructions"
                        Text="Instructions *" /><asp:TextBox
                        ID="txtLessonInstructions"
                        runat="server"
                        Text='<%# Eval("ArticleText") %>'
                        TextMode="MultiLine"
                        Rows="4" /></div>
                <div class="lesson-media">
                    <div class="builder-field"><asp:Label
                            runat="server"
                            AssociatedControlID="fuLessonImage"
                            Text="Lesson image" /><span class="builder-hint">JPG or PNG, up to 3 MB.</span><asp:FileUpload
                            ID="fuLessonImage"
                            runat="server"
                            accept="image/jpeg,image/png"
                            data-max-file-mb="3" /><asp:HyperLink
                            runat="server"
                            NavigateUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("ImagePath"))) %>'
                            Visible='<%# !string.IsNullOrEmpty(Convert.ToString(Eval("ImagePath"))) %>'
                            Text="View saved image"
                            Target="_blank"
                            rel="noopener" /></div>
                    <div class="builder-field"><asp:Label
                            runat="server"
                            AssociatedControlID="fuLessonVideo"
                            Text="Lesson video" /><span class="builder-hint">MP4, up to 20 MB. Upload one video per save.</span><asp:FileUpload
                            ID="fuLessonVideo"
                            runat="server"
                            accept="video/mp4"
                            data-max-file-mb="20" /><asp:HyperLink
                            runat="server"
                            NavigateUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("VideoPath"))) %>'
                            Visible='<%# !string.IsNullOrEmpty(Convert.ToString(Eval("VideoPath"))) %>'
                            Text="View saved video"
                            Target="_blank"
                            rel="noopener" /></div>
                </div>
                <div class="lesson-actions">
                    <asp:Button
                        runat="server"
                        Text="Move up"
                        CommandName="MoveUp"
                        Enabled='<%# Convert.ToInt32(Eval("LessonOrder")) > 1 %>'
                        CssClass="btn"
                        CausesValidation="false" />
                    <asp:Button
                        runat="server"
                        Text="Move down"
                        CommandName="MoveDown"
                        Enabled='<%# Convert.ToInt32(Eval("LessonOrder")) < LessonCount %>'
                        CssClass="btn"
                        CausesValidation="false" />
                    <asp:Button
                        runat="server"
                        Text="Delete lesson"
                        CommandName="DeleteLesson"
                        CssClass="btn lesson-delete"
                        CausesValidation="false"
                        data-order='<%# Eval("LessonOrder") %>'
                        OnClientClick="return CookingLessons.confirmDelete(this);" />
                </div>
            </article>
        </ItemTemplate>
    </asp:Repeater>
    <asp:Panel
        ID="pnlLessonPager"
        runat="server"
        CssClass="builder-pager"
        aria-label="Lesson pages">
        <asp:Button
            ID="btnPreviousLessons"
            runat="server"
            Text="Previous"
            CssClass="btn"
            OnClick="btnPreviousLessons_Click"
            CausesValidation="false" />
        <asp:Label ID="lblLessonPage" runat="server" />
        <asp:Button
            ID="btnNextLessons"
            runat="server"
            Text="Next"
            CssClass="btn"
            OnClick="btnNextLessons_Click"
            CausesValidation="false" />
    </asp:Panel>
    <asp:Button
        ID="btnAddLesson"
        runat="server"
        Text="+ Add first lesson"
        CssClass="btn"
        OnClick="btnAddLesson_Click"
        CausesValidation="false" />
</div>
