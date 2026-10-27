using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.CreateCourse
{
    public partial class CreateCoursePage : Page
    {
        private int InstructorId { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InstructorId = GetCurrentInstructorId();
            if (InstructorId <= 0)
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            ViewStateUserKey = Session.SessionID;
            Response.Cache.SetNoStore();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (InstructorId <= 0) return;
            Form.Enctype = "multipart/form-data";

            if (!IsPostBack)
            {
                RedirectExistingCourse();
                LoadCategories();
                LoadFoodCultures(ddlFoodCulture);
            }
        }

        // Verify the session belongs to an active Instructor.
        private int GetCurrentInstructorId()
        {
            int instructorId = 0;
            bool validSession = string.Equals(Convert.ToString(Session["Role"]), "Instructor", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(Convert.ToString(Session["UserId"]), out instructorId)
                && instructorId > 0;
            if (!validSession) return 0;

            DataTable instructor = Read("SELECT UserID FROM Users WHERE UserID=@id AND Role='Instructor' AND AccountStatus='Active'", Parameter("@id", instructorId));
            return instructor.Rows.Count == 1 ? instructorId : 0;
        }

        // Existing drafts are edited in Manage Course.
        private void RedirectExistingCourse()
        {
            if (Request.QueryString["courseId"] == null) return;

            int courseId;
            if (!int.TryParse(Request.QueryString["courseId"], out courseId) || courseId <= 0 ||
                Read("SELECT CourseID FROM Courses WHERE CourseID=@id AND InstructorID=@instructorId", Parameter("@id", courseId), Parameter("@instructorId", InstructorId)).Rows.Count != 1)
            {
                SetErrors(new[] { "This course link is not valid or the course is unavailable." });
                return;
            }

            RedirectToManageCourse(courseId);
        }

        private void LoadCategories()
        {
            ddlCategory.DataSource = Read("SELECT CategoryID,CategoryName FROM Categories ORDER BY CategoryName");
            ddlCategory.DataTextField = "CategoryName";
            ddlCategory.DataValueField = "CategoryID";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert(0, new ListItem("Select category", ""));

            if (ddlCategory.Items.Count == 1)
            {
                SetErrors(new[] { "No course categories are available. Ask an administrator to add a category first." });
                btnSaveNext.Enabled = false;
            }
        }

        protected void btnSaveNext_Click(object sender, EventArgs e)
        {
            pnlErrors.Visible = false;
            List<string> errors = ValidateBasicInformation();
            if (errors.Count > 0)
            {
                SetErrors(errors);
                return;
            }

            try
            {
                int courseId = CreateDraftCourse();
                RedirectToManageCourse(courseId);
            }
            catch (SqlException error)
            {
                Trace.Warn("Create course", "Database operation failed.", error);
                SetErrors(new[] { "The course could not be saved. Check your details and try again." });
            }
            catch (InvalidOperationException error)
            {
                SetErrors(new[] { error.Message });
            }
            catch (IOException)
            {
                SetErrors(new[] { "The course image could not be saved. Select it again and retry." });
            }
            catch (UnauthorizedAccessException)
            {
                SetErrors(new[] { "The upload folder is not writable. Please contact the administrator." });
            }
        }

        // Validate the course information before creating a draft.
        private List<string> ValidateBasicInformation()
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(txtCourseTitle.Text)) errors.Add("Enter a course title.");
            if (txtCourseTitle.Text.Trim().Length > 150) errors.Add("Keep the course title within 150 characters.");
            if (string.IsNullOrWhiteSpace(txtDescription.Text)) errors.Add("Describe what learners will make and learn.");
            if (txtDescription.Text.Trim().Length > 2000) errors.Add("Keep the description within 2,000 characters.");

            int categoryId;
            if (!int.TryParse(ddlCategory.SelectedValue, out categoryId)) errors.Add("Select a course category.");
            if (ddlDifficulty.SelectedValue != "Beginner" && ddlDifficulty.SelectedValue != "Intermediate" && ddlDifficulty.SelectedValue != "Advanced") errors.Add("Select a difficulty level.");
            return errors;
        }

        // Create the private draft using the submitted basic information.
        private int CreateDraftCourse()
        {
            string thumbnail = SaveUpload(fuCourseThumbnail, "");
            int courseId;

            using (SqlConnection connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    int? foodCultureId = ResolveFoodCultureId(
                        connection,
                        transaction,
                        int.Parse(ddlCategory.SelectedValue),
                        rblFoodCultureMode.SelectedValue,
                        ddlFoodCulture.SelectedValue,
                        txtNewDishName.Text,
                        foodCountryPicker.SelectedCountryName);

                    object result = Execute(connection, transaction, @"
                        INSERT INTO Courses
                            (InstructorID,CategoryID,FoodCultureID,Title,Description,DifficultyLevel,Thumbnail,Status,CreatedAt,UpdatedAt)
                        OUTPUT INSERTED.CourseID
                        SELECT @instructorId,@categoryId,@foodCultureId,@title,@description,@difficulty,@thumbnail,'Draft',GETDATE(),GETDATE()
                        WHERE EXISTS (
                            SELECT 1 FROM Users
                            WHERE UserID=@instructorId AND Role='Instructor' AND AccountStatus='Active'
                        )",
                        Parameter("@instructorId", InstructorId),
                        Parameter("@categoryId", int.Parse(ddlCategory.SelectedValue)),
                        Parameter("@foodCultureId", foodCultureId),
                        Parameter("@title", txtCourseTitle.Text.Trim()),
                        Parameter("@description", txtDescription.Text.Trim()),
                        Parameter("@difficulty", ddlDifficulty.SelectedValue),
                        Parameter("@thumbnail", thumbnail));

                    if (result == null) throw new InvalidOperationException("Your account cannot create a course right now.");
                    courseId = Convert.ToInt32(result);
                    transaction.Commit();
                }
            }

            return courseId;
        }

        private void RedirectToManageCourse(int courseId)
        {
            Response.Redirect(ResolveUrl("~/Pages/Chef/ManageCourse/ManageCoursePage.aspx?courseId=" + courseId), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        // Retained for the compiled legacy builder controls, which are no longer rendered here.
        public void RunAction(Action action)
        {
            try
            {
                action();
            }
            catch (InvalidOperationException error)
            {
                SetErrors(new[] { error.Message });
            }
        }

        // Legacy builder controls are no longer responsible for this page's save state.
        public void MarkUnsaved()
        {
        }

        private void SetErrors(IEnumerable<string> errors)
        {
            pnlErrors.Visible = true;
            rptErrors.DataSource = errors;
            rptErrors.DataBind();
        }
    }
}
