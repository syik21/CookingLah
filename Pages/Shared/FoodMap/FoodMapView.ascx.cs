using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Web.UI;
using CookingLah.F_Services;

namespace CookingLah.Pages.Shared.FoodMap
{
    public partial class FoodMapView : UserControl
    {
        protected string SelectedCountry = "Malaysia";
        protected string FoodCulturesJson = "[]";
        protected string CourseSearchUrl
        {
            get
            {
                return ResolveUrl(Convert.ToString(Session["Role"]) == "Learner"
                    ? "~/Pages/Learner/SearchCourse/SearchCoursePage.aspx"
                    : "~/Pages/Guest/SearchCourse/SearchCoursePage.aspx");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            string requestedCountry = (Request.QueryString["country"] ?? "").Trim();
            if (requestedCountry.Length > 0 && requestedCountry.Length <= 150)
            {
                SelectedCountry = requestedCountry;
            }

            LoadFoodCultures();
        }

        private void LoadFoodCultures()
        {
            const string sql = @"
                SELECT FoodCultureID, Country, DishName, Description, Ingredients, DishImage
                FROM FoodCultures
                ORDER BY Country, DishName, FoodCultureID;";
            var foods = new List<object>();

            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        foods.Add(new
                        {
                            foodCultureId = Convert.ToInt32(reader["FoodCultureID"]),
                            country = Convert.ToString(reader["Country"]),
                            dishName = Convert.ToString(reader["DishName"]),
                            description = Convert.ToString(reader["Description"]),
                            ingredients = Convert.ToString(reader["Ingredients"]),
                            dishImage = Convert.ToString(reader["DishImage"])
                        });
                    }
                }
            }

            // Escape HTML delimiters so database text cannot close the JSON script element.
            FoodCulturesJson = new JavaScriptSerializer().Serialize(foods)
                .Replace("<", "\\u003c").Replace(">", "\\u003e").Replace("&", "\\u0026");
        }
    }
}
