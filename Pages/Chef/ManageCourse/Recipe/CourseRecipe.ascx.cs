using System;
using System.Data;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;
using static CookingLah.Pages.Chef.ManageCourse.CourseManagementData;

namespace CookingLah.Pages.Chef.ManageCourse.Recipe
{
    public partial class CourseRecipe : CourseManagementControl
    {
        private int RecordId { get { return (int?)ViewState["RecordId"] ?? 0; } set { ViewState["RecordId"] = value; } }
        private string ImagePath { get { return (string)ViewState["ImagePath"] ?? ""; } set { ViewState["ImagePath"] = value; } }
        private DataTable GetRows() { return ReadDraftSection(CourseId, "CourseRecipes", "SELECT TOP 1 * FROM CourseRecipes WHERE CourseID=@id ORDER BY RecipeID", Parameter("@id", CourseId)); }
        public void LoadRecipe(int courseId)
        {
            var rows = GetRows();
            rptDetails.DataSource = rows;
            rptDetails.DataBind();
            pnlEmpty.Visible = rows.Rows.Count == 0;
            btnAdd.Visible = CanEdit;
            btnEdit.Visible = btnDelete.Visible = CanEdit && rows.Rows.Count > 0;
        }
        protected override void OnPreRender(EventArgs e)
        {
            if (Visible) Run(() => LoadRecipe(CourseId));
            base.OnPreRender(e);
        }
        protected void btnEdit_Click(object sender, EventArgs e)
        {
            Run(() =>
            {
                EnsureEditable();
                var rows = GetRows();
                DataRow row = rows.Rows.Count == 0 ? null : rows.Rows[0];
                RecordId = row == null ? 0 : Convert.ToInt32(row["RecipeID"]);
                txtIngredients.Text = IngredientsForEditor(row == null ? "" : Convert.ToString(row["Ingredients"]));
                txtCookingTime.Text = row == null ? "" : Convert.ToString(row["CookingTime"]);
                txtServingSize.Text = row == null ? "" : Convert.ToString(row["ServingSize"]);
                ImagePath = row == null ? "" : Convert.ToString(row["RecipeImage"]);
                litEditorTitle.Text = RecordId == 0 ? "Add Recipe" : "Edit Recipe";
                pnlEditor.Visible = true;
            });
        }
        protected void btnCancel_Click(object sender, EventArgs e) { pnlEditor.Visible = pnlDelete.Visible = false; }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!pnlEditor.Visible) return;
            Run(() =>
            {
                EnsureEditable();
                string ingredients = ValidateIngredientsJson(txtIngredients.Text);
                object time = PositiveNumber(txtCookingTime.Text, "Cooking time");
                object servings = PositiveNumber(txtServingSize.Text, "Serving size");
                if (CanEdit)
                {
                    var draft = GetReviewDraft(CourseId, true);
                    DataRow row = RecordId == 0 ? null : CourseReviewDraft.FindRow(draft.Recipes, "RecipeID", RecordId);
                    if (row == null)
                    {
                        if (draft.Recipes.Rows.Count > 0) throw new InvalidOperationException("This recipe changed. Reload the page and try again.");
                        row = draft.Recipes.NewRow();
                        row["RecipeID"] = CourseReviewDraft.NewId(draft.Recipes, "RecipeID");
                        row["CourseID"] = CourseId;
                        draft.Recipes.Rows.Add(row);
                    }
                    row["Ingredients"] = ingredients; row["CookingTime"] = time; row["ServingSize"] = servings;
                    string image = SaveUpload(fuRecipeImage, ImagePath);
                    draft.TrackMediaUpload(image);
                    row["RecipeImage"] = image;
                    Saved("Recipe changes saved.");
                    return;
                }
                EditCourse(CourseId, (connection, transaction) =>
                {
                    if (RecordId == 0 && Execute(connection, transaction, "SELECT TOP 1 RecipeID FROM CourseRecipes WHERE CourseID=@course", Parameter("@course", CourseId)) != null)
                        throw new InvalidOperationException("This section has already been added. Reload the page to edit it.");
                    if (RecordId != 0 && Execute(connection, transaction, "SELECT RecipeID FROM CourseRecipes WHERE RecipeID=@id AND CourseID=@course", Parameter("@id", RecordId), Parameter("@course", CourseId)) == null)
                        throw new InvalidOperationException("This content no longer exists. Reload the page.");
                    string sql = RecordId == 0 ? "INSERT INTO CourseRecipes (CourseID,Ingredients,CookingTime,ServingSize,RecipeImage) VALUES (@course,@ingredients,@time,@servings,@image)" : "UPDATE CourseRecipes SET Ingredients=@ingredients,CookingTime=@time,ServingSize=@servings,RecipeImage=@image WHERE RecipeID=@id AND CourseID=@course";
                    Execute(connection, transaction, sql, Parameter("@course", CourseId), Parameter("@id", RecordId), Parameter("@ingredients", ingredients),
                        Parameter("@time", time), Parameter("@servings", servings), Parameter("@image", SaveUpload(fuRecipeImage, ImagePath)));
                });
                Saved("Recipe saved successfully.");
            });
        }
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            Run(() =>
            {
                EnsureEditable();
                var rows = GetRows();
                if (rows.Rows.Count == 0) throw new InvalidOperationException("This content no longer exists.");
                RecordId = Convert.ToInt32(rows.Rows[0]["RecipeID"]);
                pnlDelete.Visible = true;
            });
        }
        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            if (!pnlDelete.Visible || RecordId == 0) return;
            Run(() =>
            {
                EnsureEditable();
                if (CanEdit)
                {
                    var draft = GetReviewDraft(CourseId, true);
                    if (RecordId > 0)
                    {
                        var row = CourseReviewDraft.FindRow(draft.Recipes, "RecipeID", RecordId);
                        if (row != null) draft.Recipes.Rows.Remove(row);
                    }
                    else if (draft.Recipes.Rows.Count > 0) draft.Recipes.Rows.Clear();
                    Saved("Recipe changes saved.");
                    return;
                }
                EditCourse(CourseId, (connection, transaction) =>
                    Execute(connection, transaction, "DELETE FROM CourseRecipes WHERE RecipeID=@id AND CourseID=@course", Parameter("@id", RecordId), Parameter("@course", CourseId)));
                Saved("Recipe deleted successfully.");
            });
        }
    }
}
