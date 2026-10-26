<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LessonContent.ascx.cs" Inherits="CookingLah.Pages.Learner.CourseLearning.Lesson.LessonContent" %>

<asp:Panel ID="pnlLessonUnavailable" runat="server" Visible="false" CssClass="lesson-empty">
    <h2>Lesson unavailable</h2>
    <p>This lesson is no longer available in this course.</p>
</asp:Panel>

<asp:Panel ID="pnlLessonContent" runat="server" Visible="false" CssClass="lesson-content">
    <header class="lesson-content-heading">
        <p class="lesson-step-label">Lesson <asp:Literal ID="litLessonOrder" runat="server" Mode="Encode" /></p>
        <h2><asp:Literal ID="litLessonTitle" runat="server" Mode="Encode" /></h2>
        <p class="lesson-description"><asp:Literal ID="litLessonDescription" runat="server" Mode="Encode" /></p>
    </header>

    <asp:Image ID="imgLesson" runat="server" CssClass="lesson-image" Visible="false" />

    <asp:Panel ID="pnlLessonVideo" runat="server" CssClass="lesson-video" Visible="false">
        <video controls preload="metadata" aria-label="Lesson video">
            <asp:Literal ID="litLessonVideoSource" runat="server" />
            Your browser does not support video playback.
        </video>
    </asp:Panel>

    <section class="lesson-instructions" aria-labelledby="lessonInstructionsTitle">
        <h3 id="lessonInstructionsTitle">What to do</h3>
        <asp:Literal ID="litInstructions" runat="server" Mode="Encode" />
    </section>

    <asp:Panel ID="pnlLessonCompletionNote" runat="server" Visible="false" CssClass="lesson-completion-note" role="status">
        This lesson is complete. Your progress is saved to your account.
    </asp:Panel>

    <div class="lesson-navigation">
        <asp:LinkButton ID="btnPreviousLesson" runat="server" Text="Previous lesson"
            CssClass="btn btn-secondary" OnClick="btnPreviousLesson_Click" CausesValidation="false" />
        <div class="lesson-navigation-next">
            <asp:Button ID="btnMarkComplete" runat="server" Text="Mark complete"
                CssClass="btn btn-primary" OnClick="btnMarkComplete_Click" CausesValidation="false" />
            <asp:LinkButton ID="btnNextLesson" runat="server" Text="Next lesson"
                CssClass="btn btn-primary" OnClick="btnNextLesson_Click" CausesValidation="false" />
        </div>
    </div>
</asp:Panel>
