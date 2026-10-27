using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;

namespace CookingLah.Pages.Chef.CreateCourse
{
    // Shared connection, ownership and upload handling; feature SQL stays in its control.
    public static class CourseBuilderData
    {
        public sealed class Ingredient
        {
            public string name { get; set; }
            public string amount { get; set; }
        }

        // Legacy plain-text recipes remain readable until edited and saved as JSON.
        public static Ingredient[] ReadIngredients(string value)
        {
            value = (value ?? "").Trim();
            if (value.Length == 0) return new Ingredient[0];
            if (value.StartsWith("["))
            {
                try { return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Ingredient[]>(value) ?? new Ingredient[0]; }
                catch (ArgumentException) { }
                catch (InvalidOperationException) { }
            }
            return Array.ConvertAll(value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries),
                line => new Ingredient { name = line.Trim(), amount = "" });
        }

        public static string IngredientsForEditor(string value)
        {
            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(ReadIngredients(value));
        }

        public static string ValidateIngredientsJson(string value)
        {
            Ingredient[] rows;
            try { rows = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Ingredient[]>(value ?? ""); }
            catch (ArgumentException) { throw new InvalidOperationException("Please enter valid ingredient rows."); }
            catch (InvalidOperationException) { throw new InvalidOperationException("Please enter valid ingredient rows."); }
            if (rows == null || rows.Length == 0 || rows.Length > 100)
                throw new InvalidOperationException("Please include between 1 and 100 ingredients.");
            foreach (var row in rows)
            {
                if (row == null || string.IsNullOrWhiteSpace(row.name) || string.IsNullOrWhiteSpace(row.amount))
                    throw new InvalidOperationException("Enter a name and amount for every ingredient.");
                row.name = row.name.Trim();
                row.amount = row.amount.Trim();
                if (row.name.Length > 150 || row.amount.Length > 100)
                    throw new InvalidOperationException("Keep ingredient names within 150 characters and amounts within 100 characters.");
            }
            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(rows);
        }

        public static string FormatIngredients(object value)
        {
            return string.Join(Environment.NewLine, Array.ConvertAll(ReadIngredients(Convert.ToString(value)),
                row => row == null ? "" : row.name + (string.IsNullOrWhiteSpace(row.amount) ? "" : " — " + row.amount)));
        }

        public static SqlParameter Parameter(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static void LoadFoodCultures(DropDownList dropdown)
        {
            dropdown.DataSource = Read(@"
                SELECT fc.FoodCultureID,
                       fc.DishName + NCHAR(8212) + fc.Country AS FoodDisplayName
                FROM FoodCultures fc
                ORDER BY fc.Country,fc.DishName");
            dropdown.DataTextField = "FoodDisplayName";
            dropdown.DataValueField = "FoodCultureID";
            dropdown.DataBind();
            dropdown.Items.Insert(0, new ListItem("Select a traditional food", ""));
        }

        public static int? ResolveFoodCultureId(
            SqlConnection connection,
            SqlTransaction transaction,
            int categoryId,
            string mode,
            string selectedFoodCultureId,
            string newDishName,
            string newCountry)
        {
            object categoryName = Execute(connection, transaction,
                "SELECT CategoryName FROM Categories WHERE CategoryID=@categoryId",
                Parameter("@categoryId", categoryId));

            if (categoryName == null)
                throw new InvalidOperationException("Please select a valid course category.");

            if (!string.Equals(Convert.ToString(categoryName), "Traditional Dishes", StringComparison.OrdinalIgnoreCase))
                return null;

            if (mode == "Existing")
            {
                int foodCultureId;
                if (!int.TryParse(selectedFoodCultureId, out foodCultureId) || foodCultureId <= 0)
                    throw new InvalidOperationException("Please select a Traditional Food.");

                object existingId = Execute(connection, transaction, @"
                    SELECT FoodCultureID
                    FROM FoodCultures WITH (UPDLOCK,HOLDLOCK)
                    WHERE FoodCultureID=@foodCultureId",
                    Parameter("@foodCultureId", foodCultureId));

                if (existingId == null)
                    throw new InvalidOperationException("That Traditional Food is no longer available. Please select another.");

                return Convert.ToInt32(existingId);
            }

            if (mode != "New")
                throw new InvalidOperationException("Please choose how to link a Traditional Food.");

            return CreateFoodCulture(connection, transaction, newDishName, newCountry);
        }

        public static bool IsTraditionalDishesCategory(int categoryId)
        {
            DataTable category = Read(
                "SELECT CategoryName FROM Categories WHERE CategoryID=@categoryId",
                Parameter("@categoryId", categoryId));

            return category.Rows.Count > 0 && string.Equals(
                Convert.ToString(category.Rows[0]["CategoryName"]),
                "Traditional Dishes",
                StringComparison.OrdinalIgnoreCase);
        }

        public static string ValidateDishName(string dishName)
        {
            dishName = (dishName ?? "").Trim();
            if (dishName.Length == 0)
                throw new InvalidOperationException("Please enter a Dish Name.");
            if (dishName.Length > 150)
                throw new InvalidOperationException("Keep the Dish Name within 150 characters.");

            return dishName;
        }

        public static int ValidateExistingFoodCultureId(string selectedFoodCultureId)
        {
            int foodCultureId;
            if (!int.TryParse(selectedFoodCultureId, out foodCultureId) || foodCultureId <= 0)
                throw new InvalidOperationException("Please select a Traditional Food.");

            if (Read("SELECT FoodCultureID FROM FoodCultures WHERE FoodCultureID=@foodCultureId",
                Parameter("@foodCultureId", foodCultureId)).Rows.Count == 0)
                throw new InvalidOperationException("That Traditional Food is no longer available. Please select another.");

            return foodCultureId;
        }

        public static string ValidateCountry(string country)
        {
            country = (country ?? "").Trim();
            if (country.Length == 0)
                throw new InvalidOperationException("Please select the Country associated with this Traditional Food.");
            if (country.Length > 150)
                throw new InvalidOperationException("Keep the Country within 150 characters.");

            if (!CookingLah.Pages.Shared.CountryPicker.CountryPicker.IsSupportedCountryName(country))
            {
                throw new InvalidOperationException("Please select a valid Country.");
            }

            return country;
        }

        public static void ValidateNewFoodCulture(string dishName, string country)
        {
            dishName = ValidateDishName(dishName);
            country = ValidateCountry(country);

            DataTable duplicate = Read(@"
                SELECT TOP 1 FoodCultureID
                FROM FoodCultures
                WHERE UPPER(LTRIM(RTRIM(DishName)))=UPPER(@dishName)",
                Parameter("@dishName", dishName));

            if (duplicate.Rows.Count > 0)
            {
                throw new InvalidOperationException(
                    "A Traditional Food named \"" + dishName + "\" already exists. Please select it from the existing Traditional Food list.");
            }
        }

        public static int CreateFoodCulture(SqlConnection connection, SqlTransaction transaction, string dishName, string country)
        {
            dishName = ValidateDishName(dishName);
            country = ValidateCountry(country);

            object duplicateId = Execute(connection, transaction, @"
                SELECT TOP 1 FoodCultureID
                FROM FoodCultures WITH (UPDLOCK,HOLDLOCK)
                WHERE UPPER(LTRIM(RTRIM(DishName)))=UPPER(@dishName)",
                Parameter("@dishName", dishName));

            if (duplicateId != null)
            {
                throw new InvalidOperationException(
                    "A Traditional Food named \"" + dishName + "\" already exists. Please select it from the existing Traditional Food list.");
            }

            return Convert.ToInt32(Execute(connection, transaction, @"
                INSERT INTO FoodCultures (Country,DishName)
                OUTPUT INSERTED.FoodCultureID
                VALUES (@country,@dishName)",
                Parameter("@country", country),
                Parameter("@dishName", dishName)));
        }

        public static DataTable Read(string sql, params SqlParameter[] parameters)
        {
            using (var connection = DatabaseManager.ConnectDB())
            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                command.Parameters.AddRange(parameters);
                var rows = new DataTable();
                adapter.Fill(rows);
                return rows;
            }
        }

        public static object Execute(SqlConnection connection, SqlTransaction transaction,
            string sql, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteScalar();
            }
        }

        public static void EditDraft(int courseId, Action<SqlConnection, SqlTransaction> save)
        {
            int instructorId = Convert.ToInt32(HttpContext.Current.Session["UserId"]);
            using (var connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var allowed = Execute(connection, transaction,
                        "SELECT c.CourseID FROM Courses c WITH (UPDLOCK, HOLDLOCK) " +
                        "JOIN Users u ON u.UserID=c.InstructorID WHERE c.CourseID=@id " +
                        "AND c.InstructorID=@user AND c.Status='Draft' " +
                        "AND u.Role='Instructor' AND u.AccountStatus='Active'",
                        Parameter("@id", courseId), Parameter("@user", instructorId));
                    if (allowed == null)
                        throw new InvalidOperationException("This course is no longer an editable draft.");
                    save(connection, transaction);
                    Execute(connection, transaction, "UPDATE Courses SET UpdatedAt=GETDATE() WHERE CourseID=@id",
                        Parameter("@id", courseId));
                    transaction.Commit();
                }
            }
        }

        public static string SaveUpload(FileUpload upload, string existingPath, bool video = false)
        {
            if (!upload.HasFile) return existingPath;
            int limit = video ? 20 * 1024 * 1024 : 3 * 1024 * 1024;
            if (upload.PostedFile.ContentLength > limit)
                throw new InvalidOperationException(video ? "Choose a video smaller than 20 MB." : "Choose an image smaller than 3 MB.");

            string folder = "~/F_Assets/CourseMedia/";
            string path = folder + Guid.NewGuid().ToString("N") + (video ? ".mp4" : ".jpg");
            string physicalFolder = HttpContext.Current.Server.MapPath(folder);
            Directory.CreateDirectory(physicalFolder);
            if (video)
            {
                byte[] header = new byte[12];
                upload.PostedFile.InputStream.Read(header, 0, header.Length);
                upload.PostedFile.InputStream.Position = 0;
                if (Path.GetExtension(upload.FileName).ToLowerInvariant() != ".mp4" ||
                    System.Text.Encoding.ASCII.GetString(header, 4, 4) != "ftyp")
                    throw new InvalidOperationException("Choose an MP4 video.");
                upload.SaveAs(HttpContext.Current.Server.MapPath(path));
                return path;
            }
            try
            {
                using (var image = System.Drawing.Image.FromStream(upload.PostedFile.InputStream))
                {
                    if ((long)image.Width * image.Height > 24000000)
                        throw new InvalidOperationException("Choose an image smaller than 24 megapixels.");
                    using (var bitmap = new Bitmap(image))
                        bitmap.Save(HttpContext.Current.Server.MapPath(path), ImageFormat.Jpeg);
                }
            }
            catch (ArgumentException) { throw new InvalidOperationException("Choose a valid JPG or PNG image."); }
            return path;
        }

        public static string MediaUrl(string path)
        {
            // Only generated local course media is rendered by the builder.
            return !string.IsNullOrEmpty(path) && path.StartsWith("~/F_Assets/CourseMedia/", StringComparison.Ordinal)
                ? VirtualPathUtility.ToAbsolute(path) : "";
        }
    }
}
