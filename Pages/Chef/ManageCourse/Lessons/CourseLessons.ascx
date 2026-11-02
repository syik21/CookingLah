<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" CodeBehind="CourseLessons.ascx.cs" Inherits="CookingLah.Pages.Chef.ManageCourse.Lessons.CourseLessons" %>
<section
    class="lesson-builder <%= CanEdit ? "is-editing" : "is-viewing" %>"
    data-lesson-builder
    data-editing="<%= CanEdit ? "true" : "false" %>">
    <asp:HiddenField ID="hfSelectedLesson" runat="server" />
    <asp:HiddenField ID="hfStructure" runat="server" />
    <aside class="lb-sidebar" aria-label="Course lessons">
        <div class="lb-sidebar-heading"><strong>Lessons</strong><span class="lb-count"><%= LessonCount %></span></div>
        <div class="lb-sidebar-body">
            <asp:Button
                ID="btnAddLesson"
                runat="server"
                Text="+ Add Lesson"
                OnClick="btnAddLesson_Click"
                CssClass="btn lb-add-lesson"
                CausesValidation="false" />
            <asp:Panel
                ID="pnlNoLessons"
                runat="server"
                CssClass="lb-empty">
                <p>No lessons yet.</p>
                <p><%= CanEdit ? "Add your first Lesson to begin building the Course." : "No Lessons have been added to this Course yet." %></p>
            </asp:Panel>
            <div data-lesson-order="lessons" class="lb-lesson-list">
                <asp:Repeater
                    ID="rptLessons"
                    runat="server"
                    OnItemCommand="rptLessons_ItemCommand">
                    <ItemTemplate>
                        <div class='lb-lesson <%# Convert.ToInt32(Eval("LessonID")) == SelectedLessonId ? "is-selected" : "" %>' data-lesson-id='<%# Eval("LessonID") %>'>
                            <asp:PlaceHolder runat="server" Visible='<%# CanEdit %>'><button
                                    type="button"
                                    class="lb-drag"
                                    draggable="true"
                                    aria-label="Drag lesson"
                                    title="Drag to reorder">&#9776;</button></asp:PlaceHolder>
                            <asp:LinkButton
                                ID="btnSelectLesson"
                                runat="server"
                                CommandName="SelectLesson"
                                CommandArgument='<%# Eval("LessonID") %>'
                                CssClass="lb-select"
                                CausesValidation="false"
                                aria-current='<%# Convert.ToInt32(Eval("LessonID")) == SelectedLessonId ? "true" : "false" %>'><span><%#: Eval("LessonOrder") %>.</span> <%#: LessonTitle(Eval("Title")) %></asp:LinkButton>
                            <span class="lb-readiness" title='<%# Convert.ToBoolean(Eval("Ready")) ? "Lesson complete" : "Needs required information or content" %>' aria-label='<%# Convert.ToBoolean(Eval("Ready")) ? "Lesson complete" : "Lesson incomplete" %>'><%# Convert.ToBoolean(Eval("Ready")) ? "&#10003;" : "!" %></span>
                            <asp:PlaceHolder runat="server" Visible='<%# CanEdit %>'>
                                <div class="lb-list-actions">
                                    <button
                                        type="button"
                                        data-lb-move="-1"
                                        aria-label="Move lesson up"
                                        <%# Container.ItemIndex == 0 ? "disabled" : "" %>>&#8593;</button>
                                    <button
                                        type="button"
                                        data-lb-move="1"
                                        aria-label="Move lesson down"
                                        <%# Container.ItemIndex == LessonCount - 1 ? "disabled" : "" %>>&#8595;</button>
                                    <asp:LinkButton
                                        ID="btnDeleteLesson"
                                        runat="server"
                                        Text="Delete"
                                        CommandName="DeleteLesson"
                                        CommandArgument='<%# Eval("LessonID") %>'
                                        CssClass="lb-delete"
                                        CausesValidation="false"
                                        OnClientClick="return confirm('Delete this Lesson? All Article, Image and Video content inside this Lesson will also be removed when you Save Changes.');" />
                                </div>
                            </asp:PlaceHolder>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </aside>
    <main class="lb-middle">
        <asp:Panel
            ID="pnlNoSelection"
            runat="server"
            CssClass="lb-empty lb-select-empty">
            <h2>Select a Lesson</h2>
            <p>Choose a Lesson from the left to view its content.</p>
        </asp:Panel>
        <asp:Panel ID="pnlSelectedLesson" runat="server">
            <header class="lb-heading">
                <h2><asp:Literal
                        ID="litLessonNumber"
                        runat="server"
                        Mode="Encode" /></h2>
            </header>
            <asp:Panel
                ID="pnlLessonView"
                runat="server"
                CssClass="lb-information">
                <h3><asp:Literal
                        ID="litTitle"
                        runat="server"
                        Mode="Encode" /></h3>
                <p class="lb-copy"><asp:Literal
                        ID="litDescription"
                        runat="server"
                        Mode="Encode" /></p>
            </asp:Panel>
            <asp:Panel
                ID="pnlLessonFields"
                runat="server"
                CssClass="lb-information">
                <div class="lb-field"><asp:Label
                        runat="server"
                        AssociatedControlID="txtTitle"
                        Text="Lesson Title *" /><asp:TextBox
                        ID="txtTitle"
                        runat="server"
                        MaxLength="150"
                        data-required="Lesson Title is required." /><asp:Label
                        ID="errTitle"
                        runat="server"
                        CssClass="lb-error" /></div>
                <div class="lb-field"><asp:Label
                        runat="server"
                        AssociatedControlID="txtDescription"
                        Text="Description *" /><asp:TextBox
                        ID="txtDescription"
                        runat="server"
                        TextMode="MultiLine"
                        Rows="3"
                        data-maxlength="500"
                        data-required="Lesson Description is required." /><asp:Label
                        ID="errDescription"
                        runat="server"
                        CssClass="lb-error" /></div>
            </asp:Panel>
            <div class="lb-content-heading">
                <h3>Lesson content</h3><span><%= BlockCount %> blocks</span></div>
            <asp:Panel
                ID="pnlEmptyContent"
                runat="server"
                CssClass="lb-empty">
                <p>No content yet.</p>
                <p><%= CanEdit ? "Use the content tools to build this Lesson." : "This Lesson has no learning content yet." %></p>
            </asp:Panel>
            <asp:Label
                ID="errContent"
                runat="server"
                CssClass="lb-error" />
            <div data-lesson-order="content" data-order-parent="<%= SelectedLessonId %>" class="lb-block-list">
                <asp:Repeater
                    ID="rptLessonContent"
                    runat="server"
                    OnItemCommand="rptLessonContent_ItemCommand">
                    <ItemTemplate>
                        <article
                            class="lb-block"
                            data-lesson-id='<%# Eval("LessonContentID") %>'
                            data-content-type='<%# Eval("ContentType") %>'>
                            <header class="lb-block-header">
                                <asp:PlaceHolder runat="server" Visible='<%# CanEdit %>'><button
                                        type="button"
                                        class="lb-drag"
                                        draggable="true"
                                        aria-label="Drag content block"
                                        title="Drag to reorder">&#9776;</button></asp:PlaceHolder>
                                <strong><%#: Eval("ContentType") %></strong><span class="lb-position"><%#: Eval("ContentOrder") %></span>
                                <asp:PlaceHolder runat="server" Visible='<%# CanEdit %>'>
                                    <div class="lb-block-actions">
                                        <button
                                            type="button"
                                            data-lb-move="-1"
                                            aria-label="Move content up"
                                            <%# Container.ItemIndex == 0 ? "disabled" : "" %>>&#8593;</button>
                                        <button
                                            type="button"
                                            data-lb-move="1"
                                            aria-label="Move content down"
                                            <%# Container.ItemIndex == BlockCount - 1 ? "disabled" : "" %>>&#8595;</button>
                                        <asp:LinkButton
                                            ID="btnDeleteBlock"
                                            runat="server"
                                            Text="Delete"
                                            CommandName="DeleteBlock"
                                            CommandArgument='<%# Eval("LessonContentID") %>'
                                            CssClass="lb-delete"
                                            CausesValidation="false" />
                                    </div>
                                </asp:PlaceHolder>
                            </header>
                            <div class="lb-block-body">
                                <asp:Panel runat="server" Visible='<%# CanEdit %>'>
                                    <div class="lb-field"><asp:Label
                                            runat="server"
                                            AssociatedControlID="txtContentTitle"
                                            Text="Title (optional)" /><asp:TextBox
                                            ID="txtContentTitle"
                                            runat="server"
                                            MaxLength="150"
                                            Text='<%# Eval("ContentTitle") %>' /></div>
                                    <asp:Panel
                                        runat="server"
                                        Visible='<%# Convert.ToString(Eval("ContentType")) == "Article" %>'
                                        CssClass="lb-field"><asp:Label
                                            runat="server"
                                            AssociatedControlID="txtContentText"
                                            Text="Article Content *" /><asp:TextBox
                                            ID="txtContentText"
                                            runat="server"
                                            TextMode="MultiLine"
                                            Rows="7"
                                            Text='<%# Eval("ContentText") %>'
                                            data-required="Article content is required." /></asp:Panel>
                                    <asp:Panel runat="server" Visible='<%# Convert.ToString(Eval("ContentType")) != "Article" %>'>
                                        <div class="lb-field"><asp:Label
                                                runat="server"
                                                AssociatedControlID="fuMedia"
                                                Text='<%# Eval("ContentType") + " *" %>' /><asp:FileUpload
                                                ID="fuMedia"
                                                runat="server"
                                                data-lb-upload=""
                                                accept='<%# Convert.ToString(Eval("ContentType")) == "Image" ? ".jpg,.jpeg,.png" : ".mp4" %>'
                                                data-max-file-mb='<%# Convert.ToString(Eval("ContentType")) == "Image" ? "3" : "20" %>'
                                                data-existing='<%# Media(Eval("MediaPath")) %>' /><small><%# Convert.ToString(Eval("ContentType")) == "Image" ? "JPG or PNG, up to 3 MB." : "MP4, up to 20 MB." %> Choose a file only to add or replace media.</small></div>
                                        <div class="lb-field"><asp:Label
                                                runat="server"
                                                AssociatedControlID="txtCaption"
                                                Text="Caption (optional)" /><asp:TextBox
                                                ID="txtCaption"
                                                runat="server"
                                                MaxLength="500"
                                                Text='<%# Eval("Caption") %>' /></div>
                                    </asp:Panel>
                                    <asp:Label
                                        ID="errBlock"
                                        runat="server"
                                        CssClass="lb-error" />
                                </asp:Panel>
                                <asp:PlaceHolder runat="server" Visible='<%# !CanEdit %>'>
                                    <h4><%#: Eval("ContentTitle") %></h4>
                                    <asp:Panel
                                        runat="server"
                                        Visible='<%# Convert.ToString(Eval("ContentType")) == "Article" %>'
                                        CssClass="lb-copy"><%#: Eval("ContentText") %></asp:Panel>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder runat="server" Visible='<%# Convert.ToString(Eval("ContentType")) == "Image" %>'><img
                                        class="lb-media"
                                        data-lb-preview
                                        src='<%#: Media(Eval("MediaPath")) %>'
                                        alt='<%#: string.IsNullOrWhiteSpace(Convert.ToString(Eval("Caption"))) ? "Lesson image" : Eval("Caption") %>'
                                        <%# string.IsNullOrEmpty(Media(Eval("MediaPath"))) ? "hidden" : "" %> /></asp:PlaceHolder>
                                <asp:PlaceHolder runat="server" Visible='<%# Convert.ToString(Eval("ContentType")) == "Video" %>'><video
                                        class="lb-media"
                                        data-lb-preview
                                        controls
                                        preload="metadata"
                                        src='<%#: Media(Eval("MediaPath")) %>'
                                        aria-label="Lesson video"
                                        <%# string.IsNullOrEmpty(Media(Eval("MediaPath"))) ? "hidden" : "" %>></video></asp:PlaceHolder>
                                <asp:PlaceHolder runat="server" Visible='<%# !CanEdit %>'>
                                    <p class="lb-caption"><%#: Eval("Caption") %></p>
                                </asp:PlaceHolder>
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <asp:Panel
                ID="pnlValidation"
                runat="server"
                Visible="false"
                CssClass="lb-validation"
                role="alert"
                tabindex="-1"><asp:Literal
                    ID="litValidation"
                    runat="server"
                    Mode="Encode" /></asp:Panel>
        </asp:Panel>
        <p class="lb-state" role="status" aria-live="polite"><asp:Literal
                ID="litState"
                runat="server"
                Mode="Encode" /></p>
        <% if (CanEdit) { %><p class="lb-save-note">Adding or switching lessons keeps valid inputs and selected files in your working copy. Use Save Changes above to save the course to the database.</p><% } %>
    </main>
    <asp:Panel
        ID="pnlContentToolbox"
        runat="server"
        CssClass="lb-toolbox">
        <h3>Add content</h3>
        <p>Build this Lesson using content blocks.</p>
        <div class="lb-tool"><asp:Button
                ID="btnAddArticle"
                runat="server"
                Text="+ Article"
                CommandArgument="Article"
                OnCommand="AddContent_Command"
                CssClass="btn"
                CausesValidation="false" /><small>Text and explanations.</small></div>
        <div class="lb-tool"><asp:Button
                ID="btnAddImage"
                runat="server"
                Text="+ Image"
                CommandArgument="Image"
                OnCommand="AddContent_Command"
                CssClass="btn"
                CausesValidation="false" /><small>Image with optional caption.</small></div>
        <div class="lb-tool"><asp:Button
                ID="btnAddVideo"
                runat="server"
                Text="+ Video"
                CommandArgument="Video"
                OnCommand="AddContent_Command"
                CssClass="btn"
                CausesValidation="false" /><small>Learning video with optional caption.</small></div>
    </asp:Panel>
    <asp:HiddenField ID="hfOrderKind" runat="server" />
    <asp:HiddenField ID="hfOrderIds" runat="server" />
    <asp:Button
        ID="btnSaveOrder"
        runat="server"
        Text="Save order"
        OnClick="btnSaveOrder_Click"
        CssClass="lb-order-submit"
        CausesValidation="false" />
</section>
