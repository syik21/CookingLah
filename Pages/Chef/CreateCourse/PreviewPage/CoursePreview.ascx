<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CoursePreview.ascx.cs" Inherits="CookingLah.Pages.Chef.CreateCourse.PreviewPage.CoursePreview" %>
<%@ Register Src="~/Pages/Shared/LessonBlocks/LessonBlocks.ascx" TagPrefix="shared" TagName="LessonBlocks" %>
<div class="course-preview">
    <p class="builder-hint">Review the saved version of your course. Go back to a step to make changes.</p>
    <section class="preview-section">
        <h3>Course</h3><asp:Image ID="imgCourse" runat="server" CssClass="preview-photo" AlternateText="Course thumbnail" />
        <h4><asp:Literal ID="litTitle" runat="server" Mode="Encode" /></h4>
        <p><asp:Literal ID="litCategory" runat="server" Mode="Encode" /> &middot; <asp:Literal ID="litDifficulty" runat="server" Mode="Encode" /></p>
        <p>Traditional Food: <asp:Literal ID="litFoodCulture" runat="server" Mode="Encode" /></p>
        <p class="preview-copy"><asp:Literal ID="litDescription" runat="server" Mode="Encode" /></p>
    </section>
    <section class="preview-section"><h3>Recipe</h3>
        <asp:Literal ID="litRecipeEmpty" runat="server" Text="No recipe saved yet." Mode="Encode" />
        <asp:Repeater ID="rptRecipe" runat="server"><ItemTemplate>
            <asp:Image runat="server" CssClass="preview-photo" ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("RecipeImage"))) %>' Visible='<%# !string.IsNullOrEmpty(Convert.ToString(Eval("RecipeImage"))) %>' AlternateText="Recipe photo" />
            <dl class="preview-measures"><div><dt>Cooking time</dt><dd><%#: Eval("CookingTime") == DBNull.Value ? "Not specified" : Eval("CookingTime") + " minutes" %></dd></div><div><dt>Serving size</dt><dd><%#: Eval("ServingSize") == DBNull.Value ? "Not specified" : Eval("ServingSize") %></dd></div></dl>
            <h4>Ingredients</h4><p class="preview-copy"><%#: CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.FormatIngredients(Eval("Ingredients")) %></p>
        </ItemTemplate></asp:Repeater>
    </section>
    <section class="preview-section"><div class="preview-section-heading"><h3>Lessons</h3><button type="button" class="btn preview-expand" hidden>Expand lesson details</button></div>
        <asp:Literal ID="litLessonsEmpty" runat="server" Text="No lessons saved yet." Mode="Encode" />
        <asp:Repeater ID="rptPreviewLessons" runat="server" OnItemDataBound="Lesson_ItemDataBound"><ItemTemplate>
            <details class="preview-lesson"><summary><%#: Eval("LessonOrder") %>. <%#: Eval("Title") %></summary><p class="preview-copy"><%#: Eval("Description") %></p><shared:LessonBlocks ID="LessonBlocks" runat="server" /></details>
        </ItemTemplate></asp:Repeater>
    </section>
    <section class="preview-section"><h3>Quiz</h3><asp:Literal ID="litQuizEmpty" runat="server" Text="No quiz included." Mode="Encode" />
        <asp:Repeater ID="rptPreviewQuiz" runat="server"><ItemTemplate><h4><%#: Eval("Title") %></h4><p class="preview-copy"><%#: Eval("Description") %></p><p><%#: Eval("QuestionCount") %> questions &middot; Passing score: <%#: Eval("PassingScore") %>%</p></ItemTemplate></asp:Repeater>
        <asp:Repeater ID="rptPreviewQuestions" runat="server" OnItemDataBound="rptPreviewQuestions_ItemDataBound"><ItemTemplate>
            <details class="preview-question"><summary><%#: Eval("QuestionOrder") %>. <%#: Eval("QuestionText") %></summary><ul><asp:Repeater ID="rptPreviewOptions" runat="server"><ItemTemplate><li><%#: Eval("OptionText") %><%#: Convert.ToBoolean(Eval("IsCorrect")) ? " (Correct answer)" : "" %></li></ItemTemplate></asp:Repeater></ul></details>
        </ItemTemplate></asp:Repeater>
    </section>
    <section class="preview-section"><h3>Practical activity</h3><asp:Literal ID="litActivityEmpty" runat="server" Text="No practical activity included." Mode="Encode" /><asp:Repeater ID="rptPreviewActivity" runat="server"><ItemTemplate><h4><%#: Eval("Title") %></h4><p class="preview-copy"><%#: Eval("Instructions") %></p></ItemTemplate></asp:Repeater></section>
    <p class="preview-submit-note">Submitting sends your draft to the admin team. You can edit it here until you submit.</p>
</div>
