<%@ Page Title="Create Course | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="CreateCoursePage.aspx.cs" Inherits="CookingLah.Pages.Chef.CreateCourse.CreateCoursePage" %>
<%@ Register Src="~/Pages/Chef/CreateCourse/Recipes/RecipeForm.ascx" TagPrefix="course" TagName="RecipeForm" %>
<%@ Register Src="~/Pages/Chef/CreateCourse/Lesson/LessonForm.ascx" TagPrefix="course" TagName="LessonForm" %>
<%@ Register Src="~/Pages/Chef/CreateCourse/Quiz/QuizForm.ascx" TagPrefix="course" TagName="QuizForm" %>
<%@ Register Src="~/Pages/Chef/CreateCourse/Activities/ActivityForm.ascx" TagPrefix="course" TagName="ActivityForm" %>
<%@ Register Src="~/Pages/Chef/CreateCourse/PreviewPage/CoursePreview.ascx" TagPrefix="course" TagName="CoursePreview" %>
<asp:Content ID="HeadContent" ContentPlaceHolderID="ChefHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Chef/CreateCourse/CreateCoursePage.css") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Chef/CreateCourse/Recipes/RecipeForm.css") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Chef/CreateCourse/Lesson/LessonForm.css") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Chef/CreateCourse/Quiz/QuizForm.css") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Chef/CreateCourse/Activities/ActivityForm.css") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Chef/CreateCourse/PreviewPage/CoursePreview.css") %>" rel="stylesheet" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="ChefContent" runat="server">
<div class="course-builder" data-course-id="<%= CourseID %>" data-step="<%= CurrentStep %>" data-unsaved="<%= HasUnsavedChanges ? "true" : "false" %>">
    <header class="builder-heading"><div><h1>Create course</h1><p>Build your cooking course step by step.</p></div><span class="builder-status" role="status"><span class="builder-status-label">Status</span><asp:Literal ID="litCourseStatus" runat="server" Text="New draft" Mode="Encode" /></span></header>
    <nav aria-label="Course creation progress"><ol class="builder-steps">
        <li class="<%= StepClass(0) %>"><asp:LinkButton ID="btnStep0" runat="server" CommandArgument="0" OnCommand="Step_Command" CausesValidation="false"><span class="step-number" aria-hidden="true"><%= StepMarker(0) %></span><span>Basic info</span><span class="visually-hidden"><%= StepState(0) %></span></asp:LinkButton></li>
        <li class="<%= StepClass(1) %>"><asp:LinkButton ID="btnStep1" runat="server" CommandArgument="1" OnCommand="Step_Command" CausesValidation="false"><span class="step-number" aria-hidden="true"><%= StepMarker(1) %></span><span>Recipe</span><span class="visually-hidden"><%= StepState(1) %></span></asp:LinkButton></li>
        <li class="<%= StepClass(2) %>"><asp:LinkButton ID="btnStep2" runat="server" CommandArgument="2" OnCommand="Step_Command" CausesValidation="false"><span class="step-number" aria-hidden="true"><%= StepMarker(2) %></span><span>Lessons</span><span class="visually-hidden"><%= StepState(2) %></span></asp:LinkButton></li>
        <li class="<%= StepClass(3) %>"><asp:LinkButton ID="btnStep3" runat="server" CommandArgument="3" OnCommand="Step_Command" CausesValidation="false"><span class="step-number" aria-hidden="true"><%= StepMarker(3) %></span><span>Quiz</span><span class="visually-hidden"><%= StepState(3) %></span></asp:LinkButton></li>
        <li class="<%= StepClass(4) %>"><asp:LinkButton ID="btnStep4" runat="server" CommandArgument="4" OnCommand="Step_Command" CausesValidation="false"><span class="step-number" aria-hidden="true"><%= StepMarker(4) %></span><span>Activity</span><span class="visually-hidden"><%= StepState(4) %></span></asp:LinkButton></li>
        <li class="<%= StepClass(5) %>"><asp:LinkButton ID="btnStep5" runat="server" CommandArgument="5" OnCommand="Step_Command" CausesValidation="false"><span class="step-number" aria-hidden="true"><%= StepMarker(5) %></span><span>Preview</span><span class="visually-hidden"><%= StepState(5) %></span></asp:LinkButton></li>
    </ol></nav>
    <asp:Panel ID="pnlFeedback" runat="server" CssClass="builder-feedback" role="status" Visible="false" tabindex="-1"><asp:Literal ID="litFeedback" runat="server" Mode="Encode" /></asp:Panel>
    <asp:Panel ID="pnlErrors" runat="server" CssClass="builder-errors" role="alert" Visible="false" tabindex="-1"><h2>Please check these details</h2><ul><asp:Repeater ID="rptErrors" runat="server"><ItemTemplate><li><%#: Container.DataItem %></li></ItemTemplate></asp:Repeater></ul></asp:Panel>
    <asp:Panel ID="pnlBuilder" runat="server">
        <section class="builder-sheet card" aria-labelledby="builderStepTitle">
            <p class="builder-step-count"><asp:Literal ID="litStepCount" runat="server" Mode="Encode" /></p>
            <h2 id="builderStepTitle" tabindex="-1"><asp:Literal ID="litStepTitle" runat="server" Mode="Encode" /></h2>
            <asp:Panel ID="pnlBasicInfo" runat="server">
                <p class="builder-hint">Start with the essentials. Fields marked * are required. Save a draft before adding your recipe and lessons.</p>
                <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtCourseTitle" Text="Course title *" /><asp:TextBox ID="txtCourseTitle" runat="server" MaxLength="150" placeholder="e.g. Ikan Bakar basics" /></div>
                <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtDescription" Text="Description *" /><span class="builder-hint">Tell learners what they will cook and the skills they will practise.</span><asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="5" /></div>
                <div class="builder-basic-grid">
                    <div class="builder-field"><asp:Label runat="server" AssociatedControlID="ddlCategory" Text="Category *" /><asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" /></div>
                    <div class="builder-field"><asp:Label runat="server" AssociatedControlID="ddlDifficulty" Text="Difficulty *" /><asp:DropDownList ID="ddlDifficulty" runat="server" CssClass="form-select"><asp:ListItem Text="Select difficulty" Value="" /><asp:ListItem Text="Beginner" /><asp:ListItem Text="Intermediate" /><asp:ListItem Text="Advanced" /></asp:DropDownList></div>
                </div>
                <div class="builder-field builder-thumbnail"><asp:Label runat="server" AssociatedControlID="fuCourseThumbnail" Text="Course thumbnail" /><span class="builder-hint">Choose a clear photo of the finished dish. JPG or PNG, up to 3 MB.</span><asp:FileUpload ID="fuCourseThumbnail" runat="server" accept="image/jpeg,image/png" /><asp:Image ID="imgThumbnail" runat="server" AlternateText="Course thumbnail preview" /></div>
            </asp:Panel>
            <asp:Panel ID="pnlRecipe" runat="server"><course:RecipeForm ID="RecipeForm1" runat="server" /></asp:Panel>
            <asp:Panel ID="pnlLessons" runat="server"><course:LessonForm ID="LessonForm1" runat="server" /></asp:Panel>
            <asp:Panel ID="pnlQuiz" runat="server"><course:QuizForm ID="QuizForm1" runat="server" /></asp:Panel>
            <asp:Panel ID="pnlActivity" runat="server"><course:ActivityForm ID="ActivityForm1" runat="server" /></asp:Panel>
            <asp:Panel ID="pnlPreview" runat="server"><course:CoursePreview ID="CoursePreview1" runat="server" /></asp:Panel>
        </section>
        <footer class="builder-actions">
            <div><asp:Button ID="btnSaveDraft" runat="server" Text="Save draft" CssClass="btn" OnClick="btnSaveDraft_Click" CausesValidation="false" /><p class="builder-save-note"><asp:Literal ID="litSaveNote" runat="server" Text="Your course stays private until approved." Mode="Encode" /></p></div>
            <div class="builder-navigation"><asp:Button ID="btnPrevious" runat="server" Text="Back" CssClass="btn" OnClick="btnPrevious_Click" CausesValidation="false" /><asp:Button ID="btnNext" runat="server" Text="Save &amp; next" CssClass="btn builder-primary" OnClick="btnNext_Click" CausesValidation="false" /><asp:Button ID="btnSubmitCourse" runat="server" Text="Submit for review" CssClass="btn builder-primary" OnClick="btnSubmitCourse_Click" CausesValidation="false" /></div>
        </footer>
        <asp:HyperLink ID="lnkResume" runat="server" CssClass="builder-resume" Text="Resume this draft later - bookmark this link" Visible="false" />
    </asp:Panel>
</div>
</asp:Content>
<asp:Content ID="ScriptsContent" ContentPlaceHolderID="ChefScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Chef/CreateCourse/CreateCoursePage.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Chef/CreateCourse/Recipes/RecipeForm.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Chef/CreateCourse/Lesson/LessonForm.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Chef/CreateCourse/Quiz/QuizForm.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Chef/CreateCourse/Activities/ActivityForm.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Chef/CreateCourse/PreviewPage/CoursePreview.js") %>" defer></script>
</asp:Content>
