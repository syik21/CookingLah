using System;
using System.Collections.Generic;
using System.Data;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.CreateCourse.Recipes
{
    public partial class RecipeForm : System.Web.UI.UserControl
    {
        private string ImagePath
        {
            get
            {
                return (string)ViewState["ImagePath"] ?? "";
            }
            set
            {
                ViewState["ImagePath"] = value;
            }
        }

        public void LoadRecipe(int courseId)
        {
            var rows = Read("SELECT * FROM CourseRecipes WHERE CourseID=@id", Parameter("@id", courseId));
            if (rows.Rows.Count == 0)
                return;
            var recipe = rows.Rows[0];
            txtIngredients.Text = IngredientsForEditor(Convert.ToString(recipe["Ingredients"]));
            txtCookingTime.Text = Convert.ToString(recipe["CookingTime"]);
            txtServingSize.Text = Convert.ToString(recipe["ServingSize"]);
            ImagePath = Convert.ToString(recipe["RecipeImage"]);
        }

        public List<string> ValidateRecipe()
        {
            var errors = new List<string>();
            try
            {
                ValidateIngredientsJson(txtIngredients.Text);
            }
            catch (InvalidOperationException ex) { errors.Add(ex.Message); }
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
            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(" ", errors));
            string ingredients = ValidateIngredientsJson(txtIngredients.Text);
            ImagePath = SaveUpload(fuRecipeImage, ImagePath, "Recipe");
            EditDraft(courseId, (connection, transaction) =>
            {
                Execute(connection, transaction,
                    "IF EXISTS (SELECT 1 FROM CourseRecipes WHERE CourseID=@id) " +
                    "UPDATE CourseRecipes SET Ingredients=@ingredients, " +
                    "CookingTime=@time, ServingSize=@servings, RecipeImage=@image WHERE CourseID=@id " +
                    "ELSE INSERT INTO CourseRecipes (CourseID,Ingredients,CookingTime,ServingSize,RecipeImage) " +
                    "VALUES (@id,@ingredients,@time,@servings,@image)",
                    Parameter("@id", courseId),
                    Parameter("@ingredients", ingredients),
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
