using System;
using System.Collections.Generic;
using System.Data;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.CreateCourse.Recipes
{
    public partial class RecipeForm : System.Web.UI.UserControl
    {
        private string ImagePath { get { return (string)ViewState["ImagePath"] ?? ""; } set { ViewState["ImagePath"] = value; } }

        public void LoadRecipe(int courseId)
        {
            var rows = Read("SELECT * FROM CourseRecipes WHERE CourseID=@id", Parameter("@id", courseId));
            if (rows.Rows.Count == 0) return;
            var recipe = rows.Rows[0];
            txtRecipeName.Text = Convert.ToString(recipe["RecipeName"]);
            txtRecipeDescription.Text = Convert.ToString(recipe["Description"]);
            txtIngredients.Text = Convert.ToString(recipe["Ingredients"]);
            txtCookingTime.Text = Convert.ToString(recipe["CookingTime"]);
            txtServingSize.Text = Convert.ToString(recipe["ServingSize"]);
            ImagePath = Convert.ToString(recipe["RecipeImage"]);
        }

        public List<string> ValidateRecipe()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(txtRecipeName.Text)) errors.Add("Please enter the recipe name.");
            if (txtRecipeName.Text.Length > 150) errors.Add("Keep the recipe name within 150 characters.");
            if (string.IsNullOrWhiteSpace(txtIngredients.Text)) errors.Add("Please enter the recipe ingredients.");
            CheckNumber(txtCookingTime.Text, "Cooking time", errors);
            CheckNumber(txtServingSize.Text, "Serving size", errors);
            return errors;
        }

        private void CheckNumber(string text, string label, List<string> errors)
        {
            int number;
            if (text.Length > 0 && (!int.TryParse(text, out number) || number < 1))
                errors.Add(label + " must be a positive whole number.");
        }

        public void SaveRecipe(int courseId)
        {
            var errors = new List<string>();
            CheckNumber(txtCookingTime.Text, "Cooking time", errors);
            CheckNumber(txtServingSize.Text, "Serving size", errors);
            if (errors.Count > 0) throw new InvalidOperationException(string.Join(" ", errors));
            ImagePath = SaveUpload(fuRecipeImage, ImagePath);
            EditDraft(courseId, (connection, transaction) => {
                Execute(connection, transaction,
                    "IF EXISTS (SELECT 1 FROM CourseRecipes WHERE CourseID=@id) " +
                    "UPDATE CourseRecipes SET RecipeName=@name, Description=@description, Ingredients=@ingredients, " +
                    "CookingTime=@time, ServingSize=@servings, RecipeImage=@image WHERE CourseID=@id " +
                    "ELSE INSERT INTO CourseRecipes (CourseID,RecipeName,Description,Ingredients,CookingTime,ServingSize,RecipeImage) " +
                    "VALUES (@id,@name,@description,@ingredients,@time,@servings,@image)",
                    Parameter("@id", courseId), Parameter("@name", txtRecipeName.Text.Trim()),
                    Parameter("@description", txtRecipeDescription.Text.Trim()), Parameter("@ingredients", txtIngredients.Text.Trim()),
                    Parameter("@time", txtCookingTime.Text.Length == 0 ? (object)DBNull.Value : int.Parse(txtCookingTime.Text)),
                    Parameter("@servings", txtServingSize.Text.Length == 0 ? (object)DBNull.Value : int.Parse(txtServingSize.Text)),
                    Parameter("@image", ImagePath));
            });
        }

        protected override void OnPreRender(EventArgs e)
        {
            imgRecipe.ImageUrl = MediaUrl(ImagePath);
            imgRecipe.Style["display"] = imgRecipe.ImageUrl.Length == 0 ? "none" : "block";
            base.OnPreRender(e);
        }
    }
}
