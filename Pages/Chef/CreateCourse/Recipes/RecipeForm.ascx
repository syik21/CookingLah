<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RecipeForm.ascx.cs" Inherits="CookingLah.Pages.Chef.CreateCourse.Recipes.RecipeForm" %>
<div class="recipe-form">
    <fieldset class="ingredient-editor" data-ingredient-editor=""><legend>Ingredients *</legend>
        <p class="text-muted">Add at least one ingredient. Include units in the amount, for example 200 g or 2 tbsp.</p>
        <asp:TextBox ID="txtIngredients" runat="server" TextMode="MultiLine" data-ingredient-json="" aria-label="Ingredients JSON" />
        <div data-ingredient-rows=""></div>
        <button type="button" class="btn ingredient-add" data-ingredient-add="">+ Add ingredient</button>
        <noscript>Enable JavaScript to use ingredient rows.</noscript>
    </fieldset>
    <div class="recipe-measures">
        <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtCookingTime" Text="Cooking time (minutes)" /><asp:TextBox ID="txtCookingTime" runat="server" TextMode="Number" min="1" /></div>
        <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtServingSize" Text="Serving size" /><asp:TextBox ID="txtServingSize" runat="server" TextMode="Number" min="1" /></div>
    </div>
    <div class="builder-field"><asp:Label runat="server" AssociatedControlID="fuRecipeImage" Text="Recipe image" /><span class="builder-hint">JPG or PNG, up to 3 MB.</span><asp:FileUpload ID="fuRecipeImage" runat="server" accept="image/jpeg,image/png" data-max-file-mb="3" /><asp:Image ID="imgRecipe" runat="server" CssClass="recipe-image" AlternateText="Recipe image preview" /></div>
    <p class="builder-hint">The cooking instructions come next, in Lessons.</p>
</div>
