<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LessonBlocks.ascx.cs" Inherits="CookingLah.Pages.Shared.LessonBlocks.LessonBlocks" %>
<asp:Repeater ID="rptBlocks" runat="server"><ItemTemplate>
    <article class="lesson-content-block" data-content-type="<%#: Eval("ContentType") %>">
        <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("ContentTitle"))) %>'>
            <h4><%#: Eval("ContentTitle") %></h4>
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" Visible='<%# Convert.ToString(Eval("ContentType")) == "Article" %>'>
            <p class="lesson-block-text"><%#: Eval("ContentText") %></p>
        </asp:PlaceHolder>
        <asp:Image runat="server" CssClass="lesson-block-media" ImageUrl='<%# Media(Eval("MediaPath")) %>'
            AlternateText='<%# Convert.ToString(Eval("Caption")) %>'
            Visible='<%# Convert.ToString(Eval("ContentType")) == "Image" && Media(Eval("MediaPath")).Length > 0 %>' loading="lazy" />
        <asp:PlaceHolder runat="server" Visible='<%# Convert.ToString(Eval("ContentType")) == "Video" && Media(Eval("MediaPath")).Length > 0 %>'>
            <video class="lesson-block-media" controls preload="metadata" aria-label='<%#: string.IsNullOrWhiteSpace(Convert.ToString(Eval("ContentTitle"))) ? "Lesson video" : Eval("ContentTitle") %>'>
                <source src='<%#: Media(Eval("MediaPath")) %>' type="video/mp4" />
                Your browser does not support video playback.
            </video>
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" Visible='<%# Convert.ToString(Eval("ContentType")) != "Article" && !string.IsNullOrWhiteSpace(Convert.ToString(Eval("Caption"))) %>'>
            <p class="lesson-block-caption"><%#: Eval("Caption") %></p>
        </asp:PlaceHolder>
    </article>
</ItemTemplate></asp:Repeater>
