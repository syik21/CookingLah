<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CourseRecipe.ascx.cs" Inherits="CookingLah.Pages.Chef.ManageCourse.Recipe.CourseRecipe" %>
<section class="course-recipe">
    <div class="mc-section-heading recipe-heading">
        <div><h2>Recipe</h2><p>Give learners the ingredients, timing, and serving details they need before cooking.</p></div>
        <asp:Button ID="btnEdit" runat="server" Text="Edit Recipe" CssClass="btn" OnClick="btnEdit_Click" />
    </div>
    <asp:Panel ID="pnlEmpty" runat="server" CssClass="card mc-empty recipe-empty">
        <div class="recipe-empty-mark" aria-hidden="true">+</div><h3>No recipe added yet</h3><p>Add ingredients and preparation details to complete this part of the course.</p>
        <asp:Button ID="btnAdd" runat="server" Text="+ Add Recipe" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
    </asp:Panel>
    <asp:Repeater ID="rptDetails" runat="server"><ItemTemplate><article class='<%# "card recipe-card" + (string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("RecipeImage")))) ? " has-no-image" : "") %>'>
        <div class="recipe-visual"><asp:Image runat="server" CssClass="recipe-image" ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("RecipeImage"))) %>' Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("RecipeImage")))) %>' AlternateText="Recipe photo" /></div>
        <div class="recipe-details"><div class="recipe-measures"><div><span>Cooking time</span><strong><%#: Eval("CookingTime") == DBNull.Value ? "Not specified" : Eval("CookingTime") + " minutes" %></strong></div><div><span>Serving size</span><strong><%#: Eval("ServingSize") == DBNull.Value ? "Not specified" : Eval("ServingSize") %></strong></div></div>
        <section class="recipe-ingredients" aria-labelledby="recipeIngredientsTitle"><h3 id="recipeIngredientsTitle">Ingredients</h3><p class="mc-copy"><%#: CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.FormatIngredients(Eval("Ingredients")) %></p></section></div>
    </article>
    </ItemTemplate></asp:Repeater>
    <asp:Button ID="btnDelete" runat="server" Text="Delete Recipe" CssClass="mc-link mc-danger-link" OnClick="btnDelete_Click" />
    <asp:Panel ID="pnlEditor" runat="server" Visible="false" CssClass="card mc-modal" data-title="Recipe">
        <h2><asp:Literal ID="litEditorTitle" runat="server" Mode="Encode" /></h2>
        <p class="recipe-editor-intro">Add the essential recipe details learners will reference during the course.</p>
    <fieldset class="ingredient-editor" data-ingredient-editor=""><legend>Ingredients *</legend>
        <p class="text-muted">Add at least one ingredient. Include units in the amount, for example 200 g or 2 tbsp.</p>
        <asp:TextBox ID="txtIngredients" runat="server" TextMode="MultiLine" data-ingredient-json="" aria-label="Ingredients JSON" />
        <div data-ingredient-rows=""></div>
        <button type="button" class="btn ingredient-add" data-ingredient-add="">+ Add ingredient</button>
        <noscript>Enable JavaScript to use ingredient rows.</noscript>
    </fieldset>
<div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtCookingTime" Text="Cooking Time (minutes) *" /><asp:TextBox ID="txtCookingTime" runat="server" TextMode="Number" min="1" required="required" MaxLength="10" /></div>
<div class="mc-field"><asp:Label runat="server" AssociatedControlID="txtServingSize" Text="Serving Size" /><asp:TextBox ID="txtServingSize" runat="server" MaxLength="10" /></div>
<div class="mc-field">
<asp:Label runat="server" AssociatedControlID="fuRecipeImage" Text="Recipe Image" />
<asp:FileUpload ID="fuRecipeImage" runat="server" accept="image/png,image/jpeg" data-image-input="" data-max-file-mb="3" /></div><p class="text-muted">Images: JPG or PNG, up to 3 MB.</p><img data-image-preview="" class="mc-image-preview" alt="Selected recipe photo" hidden />
        <div class="mc-actions">
        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" data-cancel="" />
        <asp:Button ID="btnSave" runat="server" Text="Apply Changes" CssClass="btn btn-primary" OnClick="btnSave_Click" /></div>
    </asp:Panel>
    <asp:Panel ID="pnlDelete" runat="server" Visible="false" CssClass="card mc-modal mc-delete" data-title="Delete Recipe?">
        <h2>Delete Recipe?</h2><p>Are you sure you want to delete this course&rsquo;s recipe?</p><p>The course will remain, but its recipe will be removed.</p>
        <div class="mc-actions">
        <asp:Button ID="btnCancelDelete" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" data-cancel="" />
        <asp:Button ID="btnConfirmDelete" runat="server" Text="Delete Recipe Permanently" CssClass="btn mc-danger" OnClick="btnConfirmDelete_Click" /></div>
    </asp:Panel>
</section>
