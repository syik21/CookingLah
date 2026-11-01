using System;
using System.Data;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;
using static CookingLah.Pages.Chef.ManageCourse.CourseManagementData;

namespace CookingLah.Pages.Chef.ManageCourse.Overview
{
    public partial class CourseOverview : CourseManagementControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Visible && CanEdit && !IsPostBack && Request.QueryString["edit"] == "1") Run(OpenEditor);
        }
        public void LoadCourse(int courseId)
        {
            rptOverview.DataSource = new[] { Host.Course }.CopyToDataTable();
            rptOverview.DataBind();
            btnEdit.Visible = CanEdit;
        }
        protected override void OnPreRender(EventArgs e)
        {
            if (Visible) Run(() => LoadCourse(CourseId));
            base.OnPreRender(e);
        }
        private void OpenEditor()
        {
            EnsureEditable();
            var course = Host.Course;
            txtTitle.Text = Convert.ToString(course["Title"]);
            txtDescription.Text = Convert.ToString(course["Description"]);
            LoadFoodCultures(ddlFoodCulture);
            ddlCategory.DataSource = Read("SELECT CategoryID,CategoryName FROM Categories ORDER BY CategoryName");
            ddlCategory.DataTextField = "CategoryName";
            ddlCategory.DataValueField = "CategoryID";
            ddlCategory.DataBind();
            ddlCategory.SelectedValue = Convert.ToString(course["CategoryID"]);
            ddlDifficulty.SelectedValue = Convert.ToString(course["DifficultyLevel"]);

            CourseReviewDraft draft = GetReviewDraft(CourseId, false);
            DataRow stagedCourse = draft == null ? course : draft.Courses.Rows[0];
            string stagedDishName = stagedCourse.Table.Columns.Contains("NewFoodDishName")
                ? Convert.ToString(stagedCourse["NewFoodDishName"])
                : "";

            if (!string.IsNullOrWhiteSpace(stagedDishName))
            {
                rblFoodCultureMode.SelectedValue = "New";
                txtNewDishName.Text = stagedDishName;

                foodCountryPicker.SelectCountry(Convert.ToString(stagedCourse["NewFoodCountry"]));
            }
            else if (stagedCourse["FoodCultureID"] != DBNull.Value)
            {
                rblFoodCultureMode.SelectedValue = "Existing";
                ddlFoodCulture.SelectedValue = Convert.ToString(stagedCourse["FoodCultureID"]);
            }

            pnlEditor.Visible = true;
        }
        protected void btnEdit_Click(object sender, EventArgs e) { Run(OpenEditor); }
        protected void btnCancel_Click(object sender, EventArgs e) { pnlEditor.Visible = false; }
        protected void btnSaveCourse_Click(object sender, EventArgs e)
        {
            if (!pnlEditor.Visible) return;
            Run(() =>
            {
                EnsureEditable();
                string title = Required(txtTitle.Text, "a course title", 150);
                string description = Required(txtDescription.Text, "a course description");
                int categoryId;
                if (!int.TryParse(ddlCategory.SelectedValue, out categoryId)) throw new InvalidOperationException("Please select a category.");
                string difficulty = ddlDifficulty.SelectedValue;
                if (Array.IndexOf(new[] { "Beginner", "Intermediate", "Advanced" }, difficulty) < 0) throw new InvalidOperationException("Please select a difficulty level.");
                if (CanEdit)
                {
                    var draft = GetReviewDraft(CourseId, true);
                    if (Read("SELECT CategoryID FROM Categories WHERE CategoryID=@id", Parameter("@id", categoryId)).Rows.Count == 0)
                        throw new InvalidOperationException("This category is no longer available. Reload the page and choose another.");
                    string image = SaveUpload(fuThumbnail, Convert.ToString(draft.Courses.Rows[0]["Thumbnail"]), "Thumbnail");
                    draft.TrackMediaUpload(image);
                    DataRow staged = draft.Courses.Rows[0];

                    int? foodCultureId = null;
                    string newDishName = "";
                    object newCountry = DBNull.Value;
                    if (IsTraditionalDishesCategory(categoryId))
                    {
                        if (rblFoodCultureMode.SelectedValue == "Existing")
                        {
                            foodCultureId = ValidateExistingFoodCultureId(ddlFoodCulture.SelectedValue);
                        }
                        else if (rblFoodCultureMode.SelectedValue == "New")
                        {
                            newDishName = ValidateDishName(txtNewDishName.Text);
                            string country = ValidateCountry(foodCountryPicker.SelectedCountryName);
                            ValidateNewFoodCulture(newDishName, country);
                            newCountry = country;
                        }
                        else
                        {
                            throw new InvalidOperationException("Please choose how to link a Traditional Food.");
                        }
                    }

                    staged["Title"] = title; staged["Description"] = description; staged["CategoryID"] = categoryId;
                    staged["DifficultyLevel"] = difficulty; staged["Thumbnail"] = image;
                    staged["FoodCultureID"] = foodCultureId.HasValue ? (object)foodCultureId.Value : DBNull.Value;
                    staged["NewFoodDishName"] = newDishName;
                    staged["NewFoodCountry"] = newCountry;
                    Saved("Course changes saved.");
                    return;
                }
                EditCourse(CourseId, (connection, transaction) =>
                {
                    if (Execute(connection, transaction, "SELECT CategoryID FROM Categories WHERE CategoryID=@id", Parameter("@id", categoryId)) == null)
                        throw new InvalidOperationException("This category is no longer available. Reload the page and choose another.");
                    string image = SaveUpload(fuThumbnail, Convert.ToString(Host.Course["Thumbnail"]), "Thumbnail");
                    int? foodCultureId = ResolveFoodCultureId(
                        connection,
                        transaction,
                        categoryId,
                        rblFoodCultureMode.SelectedValue,
                        ddlFoodCulture.SelectedValue,
                        txtNewDishName.Text,
                        foodCountryPicker.SelectedCountryName);

                    Execute(connection, transaction, "UPDATE Courses SET Title=@title,Description=@description,CategoryID=@category,FoodCultureID=@foodCultureId,DifficultyLevel=@difficulty,Thumbnail=@image WHERE CourseID=@id",
                        Parameter("@title", title), Parameter("@description", description), Parameter("@category", categoryId), Parameter("@foodCultureId", foodCultureId), Parameter("@difficulty", difficulty), Parameter("@image", image), Parameter("@id", CourseId));
                });
                Saved("Course updated successfully.");
            });
        }
    }
}
