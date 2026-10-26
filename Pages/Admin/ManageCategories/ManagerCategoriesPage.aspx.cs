using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace CookingLah.Pages.Admin.ManageCategories
{
    public partial class ManagerCategoriesPage : System.Web.UI.Page
    {
        // Number of categories displayed per page.
        private const int PageSize = 10;
        // Store page, edit, delete and search values across postbacks.
        private int CurrentPage
        {
            get { return (int)(ViewState["CurrentPage"] ?? 1); }
            set { ViewState["CurrentPage"] = value; }
        }
        private int EditCategoryId
        {
            get { return (int)(ViewState["EditCategoryId"] ?? 0); }
            set { ViewState["EditCategoryId"] = value; }
        }
        private int DeleteCategoryId
        {
            get { return (int)(ViewState["DeleteCategoryId"] ?? 0); }
            set { ViewState["DeleteCategoryId"] = value; }
        }
        private string SearchTerm
        {
            get { return Convert.ToString(ViewState["SearchTerm"]); }
            set { ViewState["SearchTerm"] = value; }
        }

        // Verify that the current user is an active administrator.
        protected override void OnInit(EventArgs e)
        {
            if (Session["CategoriesCsrf"] == null)
            {
                Session["CategoriesCsrf"] = Guid.NewGuid().ToString("N");
            }
            ViewStateUserKey = Convert.ToString(Session["CategoriesCsrf"]);
            base.OnInit(e);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            // Check the database role on every request, before any postback event can run.
            int userId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out userId))
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"));
                return;
            }
            try
            {
                using (var connection = DatabaseManager.ConnectDB())
                using (var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Users WHERE UserID=@id AND Role='Admin' AND AccountStatus='Active'", connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                    connection.Open();
                    if ((int)command.ExecuteScalar() != 1)
                        throw new HttpException(403, "An active admin account is required.");
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Category authorization: {0}", ex);
                throw new HttpException(503, "Category management is temporarily unavailable. Please try again.");
            }
        }

        // Restore search and pagination values on the first page load.
        protected void Page_Load(object sender, EventArgs e)
        {
            pnlMessage.Visible = false;
            if (!IsPostBack)
            {
                SearchTerm = (Request.QueryString["q"] ?? "").Trim();
                if (SearchTerm.Length > 100)
                {
                    SearchTerm = SearchTerm.Substring(0, 100);
                }
                txtSearchCategory.Text = SearchTerm;
                int page;
                if (int.TryParse(Request.QueryString["page"], out page))
                {
                    CurrentPage = Math.Max(1, page);
                }
                string feedback = Convert.ToString(Session["CategoriesFeedback"]);
                Session.Remove("CategoriesFeedback");
                if (feedback.Length > 0)
                {
                    ShowMessage(feedback, false);
                }
            }
        }

        // Refresh the category summary and list before rendering.
        protected override void OnPreRender(EventArgs e)
        {
            RunDatabaseAction(connection =>
            {
                LoadCategorySummary(connection);
                LoadCategories(connection);
            });
            base.OnPreRender(e);
        }

        // Execute database work and show friendly SQL error messages.
        private void RunDatabaseAction(Action<SqlConnection> action)
        {
            try
            {
                using (var connection = DatabaseManager.ConnectDB())
                {
                    connection.Open();
                    action(connection);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Category management: {0}", ex);
                string message = ex.Number == 2601 || ex.Number == 2627
                    ? "A category with this name already exists."
                    : ex.Number == 547
                    ? "This category cannot be deleted because courses are currently using it."
                    : "We could not complete the request. Please try again.";
                ShowMessage(message, true);
            }
        }

        // Display a success or error notification.
        private void ShowMessage(string message, bool isError)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = "categories-message " + (isError ? "categories-error" : "categories-success");
            pnlMessage.Attributes["role"] = isError ? "alert" : "status";
            litMessage.Text = message;
        }

        // Count all categories for the summary label.
        private void LoadCategorySummary(SqlConnection connection)
        {
            using (var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Categories", connection))
                lblTotalCategories.Text = Convert.ToString(command.ExecuteScalar());
        }

        // Fetch filtered categories and configure pagination.
        private void LoadCategories(SqlConnection connection)
        {
            const string filter = " WHERE (@search='' OR CHARINDEX(@search,c.CategoryName)>0 OR CHARINDEX(@search,c.Description)>0)";
            using (var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Categories c" + filter, connection))
            {
                command.Parameters.Add("@search", SqlDbType.NVarChar, 100).Value = SearchTerm;
                int total = (int)command.ExecuteScalar();
                int pageCount = Math.Max(1, (total + PageSize - 1) / PageSize);
                CurrentPage = Math.Min(CurrentPage, pageCount);
                command.CommandText = @"SELECT c.CategoryID,c.CategoryName,c.Description,c.CreatedAt,
                    (SELECT COUNT(*) FROM dbo.Courses course WHERE course.CategoryID=c.CategoryID) AS CourseCount
                    FROM dbo.Categories c" + filter + " ORDER BY c.CategoryName,c.CategoryID OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY";
                command.Parameters.Add("@offset", SqlDbType.Int).Value = (CurrentPage - 1) * PageSize;
                command.Parameters.Add("@size", SqlDbType.Int).Value = PageSize;
                var categories = new DataTable();
                using (var reader = command.ExecuteReader())
                {
                    categories.Load(reader);
                }
                rptCategories.DataSource = categories;
                rptCategories.DataBind();
                pnlCategoryList.Visible = total > 0;
                pnlEmpty.Visible = total == 0;
                bool searching = SearchTerm.Length > 0;
                litEmptyTitle.Text = searching ? "No categories match your search." : "No categories found.";
                pnlFirstCategory.Visible = !searching;
                btnEmptyClear.Visible = searching;
                btnClearSearch.Visible = searching;
                pnlPagination.Visible = total > PageSize;
                btnPrevious.Enabled = CurrentPage > 1;
                btnNext.Enabled = CurrentPage < pageCount;
                lblPageNumber.Text = "Page " + CurrentPage + " of " + pageCount;
                lblPageSummary.Text = string.Format("Showing {0}–{1} of {2} categories", (CurrentPage - 1) * PageSize + 1, Math.Min(CurrentPage * PageSize, total), total);
            }
        }

        // Apply the category search and return to page one.
        protected void btnSearchCategory_Click(object sender, EventArgs e)
        {
            SearchTerm = txtSearchCategory.Text.Trim();
            if (SearchTerm.Length > 100)
            {
                SearchTerm = SearchTerm.Substring(0, 100);
            }
            CurrentPage = 1;
            CloseForms();
        }

        // Remove the search filter.
        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearchCategory.Text = SearchTerm = "";
            CurrentPage = 1;
            CloseForms();
        }

        // Move to the previous results page.
        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            CurrentPage = Math.Max(1, CurrentPage - 1);
            CloseForms();
        }
        // Move to the next results page.
        protected void btnNext_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            CloseForms();
        }

        // Hide add/edit and delete panels and clear selected IDs.
        private void CloseForms()
        {
            pnlCategoryForm.Visible = pnlDeleteCategory.Visible = false;
            EditCategoryId = DeleteCategoryId = 0;
        }

        // Cancel the current form.
        protected void btnCancelCategory_Click(object sender, EventArgs e)
        {
            CloseForms();
        }

        // Open an empty form for a new category.
        protected void btnOpenAdd_Click(object sender, EventArgs e)
        {
            CloseForms();
            txtCategoryName.Text = txtCategoryDescription.Text = "";
            litFormTitle.Text = btnSaveCategory.Text = "Add Category";
            pnlCategoryForm.Visible = true;
        }

        // Handle the Edit and Delete actions in the category table.
        protected void rptCategories_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int categoryId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out categoryId) || categoryId <= 0)
            {
                return;
            }
            CloseForms();
            if (e.CommandName == "EditCategory")
                RunDatabaseAction(connection => LoadCategoryForEdit(connection, categoryId));
            else if (e.CommandName == "DeleteCategory")
                RunDatabaseAction(connection => PrepareDelete(connection, categoryId));
        }

        // Load the selected category into the edit form.
        private void LoadCategoryForEdit(SqlConnection connection, int categoryId)
        {
            using (var command = new SqlCommand("SELECT CategoryName,Description FROM dbo.Categories WHERE CategoryID=@id", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = categoryId;
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        ShowMessage("This category no longer exists.", true);
                        return;
                    }
                    txtCategoryName.Text = Convert.ToString(reader["CategoryName"]);
                    txtCategoryDescription.Text = Convert.ToString(reader["Description"]);
                }
            }
            EditCategoryId = categoryId;
            litFormTitle.Text = "Edit Category";
            btnSaveCategory.Text = "Save Changes";
            pnlCategoryForm.Visible = true;
        }

        // Check for duplicate category names inside a transaction.
        private bool CategoryNameExists(SqlConnection connection, SqlTransaction transaction, string categoryName, int categoryId)
        {
            using (var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Categories WITH (UPDLOCK,HOLDLOCK) WHERE CategoryName=@name AND CategoryID<>@id", connection, transaction))
            {
                command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = categoryName;
                command.Parameters.Add("@id", SqlDbType.Int).Value = categoryId;
                return (int)command.ExecuteScalar() > 0;
            }
        }

        // Validate the submitted category name and description.
        protected void btnSaveCategory_Click(object sender, EventArgs e)
        {
            if (!pnlCategoryForm.Visible)
            {
                return;
            }
            string categoryName = txtCategoryName.Text.Trim();
            string description = txtCategoryDescription.Text.Trim();
            if (categoryName.Length == 0)
            {
                ShowMessage("Please enter a category name.", true);
                return;
            }
            if (categoryName.Length > 100)
            {
                ShowMessage("Category names must be 100 characters or fewer.", true);
                return;
            }
            if (description.Length > 500)
            {
                ShowMessage("Descriptions must be 500 characters or fewer.", true);
                return;
            }
            RunDatabaseAction(connection => SaveCategory(connection, categoryName, description));
        }

        // Insert or update a category using a transaction.
        private void SaveCategory(SqlConnection connection, string categoryName, string description)
        {
            int categoryId = EditCategoryId;
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                if (CategoryNameExists(connection, transaction, categoryName, categoryId))
                {
                    ShowMessage("A category with this name already exists.", true);
                    return; // Disposing an uncommitted transaction rolls it back.
                }
                string sql = categoryId == 0
                    ? "INSERT INTO dbo.Categories (CategoryName,Description) VALUES (@name,@description)"
                    : "UPDATE dbo.Categories SET CategoryName=@name,Description=@description WHERE CategoryID=@id";
                using (var command = new SqlCommand(sql, connection, transaction))
                {
                    command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = categoryName;
                    command.Parameters.Add("@description", SqlDbType.NVarChar, 500).Value = description.Length == 0 ? (object)DBNull.Value : description;
                    if (categoryId != 0)
                    {
                        command.Parameters.Add("@id", SqlDbType.Int).Value = categoryId;
                    }
                    if (command.ExecuteNonQuery() == 0)
                    {
                        CloseForms();
                        ShowMessage("This category no longer exists.", true);
                        return;
                    }
                }
                transaction.Commit();
            }
            CompleteOperation(categoryId == 0 ? "Category added successfully." : "Category updated successfully.");
        }

        // Count courses currently assigned to a category.
        private int GetCourseCountByCategory(SqlConnection connection, int categoryId, SqlTransaction transaction = null)
        {
            using (var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Courses WHERE CategoryID=@id", connection, transaction))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = categoryId;
                return (int)command.ExecuteScalar();
            }
        }

        // Show delete confirmation only when no courses use the category.
        private void PrepareDelete(SqlConnection connection, int categoryId)
        {
            string categoryName;
            using (var command = new SqlCommand("SELECT CategoryName FROM dbo.Categories WHERE CategoryID=@id", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = categoryId;
                object result = command.ExecuteScalar();
                if (result == null)
                {
                    ShowMessage("This category no longer exists.", true);
                    return;
                }
                categoryName = Convert.ToString(result);
            }
            int courseCount = GetCourseCountByCategory(connection, categoryId);
            DeleteCategoryId = categoryId;
            pnlDeleteCategory.Visible = true;
            btnConfirmDelete.Visible = courseCount == 0;
            litDeleteTitle.Text = courseCount == 0 ? "Delete Category?" : "Cannot delete this category.";
            litDeleteMessage.Text = courseCount == 0
                ? "Are you sure you want to delete \"" + categoryName + "\"? This action cannot be undone."
                : string.Format("{0} {1} currently using \"{2}\". Move or delete those courses before deleting this category.",
                    courseCount, courseCount == 1 ? "course is" : "courses are", categoryName);
        }

        // Confirm deletion of the selected category.
        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            if (!pnlDeleteCategory.Visible || !btnConfirmDelete.Visible || DeleteCategoryId <= 0)
            {
                return;
            }
            RunDatabaseAction(connection => DeleteCategory(connection, DeleteCategoryId));
        }

        // Safely remove a category after checking its course references.
        private void DeleteCategory(SqlConnection connection, int categoryId)
        {
            // The lock and serializable usage check keep new course references from
            // slipping between checking usage and deleting the category.
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            using (var command = new SqlCommand("SELECT CategoryID FROM dbo.Categories WITH (XLOCK,HOLDLOCK) WHERE CategoryID=@id", connection, transaction))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = categoryId;
                if (command.ExecuteScalar() == null)
                {
                    CloseForms();
                    ShowMessage("This category no longer exists.", true);
                    return;
                }
                int courseCount = GetCourseCountByCategory(connection, categoryId, transaction);
                if (courseCount > 0)
                {
                    btnConfirmDelete.Visible = false;
                    ShowMessage(string.Format("This category cannot be deleted because {0} {1} currently using it.",
                        courseCount, courseCount == 1 ? "course is" : "courses are"), true);
                    return;
                }
                command.CommandText = "DELETE FROM dbo.Categories WHERE CategoryID=@id";
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            CompleteOperation("Category deleted successfully.");
        }

        // Save success feedback and redirect after changes.
        private void CompleteOperation(string message)
        {
            // Redirect after writes so refreshing the browser cannot repeat the operation.
            Session["CategoriesFeedback"] = message;
            Response.Redirect("ManagerCategoriesPage.aspx?q=" + HttpUtility.UrlEncode(SearchTerm) + "&page=" + CurrentPage);
        }
    }
}

