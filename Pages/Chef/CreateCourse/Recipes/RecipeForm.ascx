<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RecipeForm.ascx.cs" Inherits="CookingLah.Pages.Chef.CreateCourse.Recipes.RecipeForm" %>
<div class="recipe-form">
    <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtRecipeName" Text="Recipe name *" /><asp:TextBox ID="txtRecipeName" runat="server" MaxLength="150" /></div>
    <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtRecipeDescription" Text="Recipe description" /><asp:TextBox ID="txtRecipeDescription" runat="server" TextMode="MultiLine" Rows="3" /></div>
    <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtIngredients" Text="Ingredients *" /><span class="builder-hint">Include quantities. Use one line for each ingredient.</span><asp:TextBox ID="txtIngredients" runat="server" TextMode="MultiLine" Rows="6" /></div>
    <div class="recipe-measures">
        <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtCookingTime" Text="Cooking time (minutes)" /><asp:TextBox ID="txtCookingTime" runat="server" TextMode="Number" min="1" /></div>
        <div class="builder-field"><asp:Label runat="server" AssociatedControlID="txtServingSize" Text="Serving size" /><asp:TextBox ID="txtServingSize" runat="server" TextMode="Number" min="1" /></div>
    </div>
    <div class="builder-field"><asp:Label runat="server" AssociatedControlID="fuRecipeImage" Text="Recipe image" /><span class="builder-hint">JPG or PNG, up to 3 MB.</span><asp:FileUpload ID="fuRecipeImage" runat="server" accept="image/jpeg,image/png" /><asp:Image ID="imgRecipe" runat="server" CssClass="recipe-image" AlternateText="Recipe image preview" /></div>
    <p class="builder-hint">The cooking instructions come next, in Lessons.</p>
</div>
