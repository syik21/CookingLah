using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.CreateCourse
{
    public partial class CreateCoursePage : System.Web.UI.Page
    {
        private static readonly string[] StepNames = { "Basic information", "Recipe", "Lessons", "Quiz", "Practical activity", "Preview & submit" };
        public int CourseID { get { return (int?)ViewState["CourseID"] ?? 0; } private set { ViewState["CourseID"] = value; } }
        public int CurrentStep { get { return (int?)ViewState["CurrentStep"] ?? 0; } private set { ViewState["CurrentStep"] = value; } }
        protected bool HasUnsavedChanges { get { return (bool?)ViewState["HasUnsavedChanges"] ?? false; } set { ViewState["HasUnsavedChanges"] = value; } }
        private int CompletedSteps { get { return (int?)ViewState["CompletedSteps"] ?? 0; } set { ViewState["CompletedSteps"] = value; } }
        private int HighestStep { get { return (int?)ViewState["HighestStep"] ?? 0; } set { ViewState["HighestStep"] = value; } }
        private int InstructorID { get { return Convert.ToInt32(Session["UserId"]); } }
        private string Thumbnail { get { return (string)ViewState["Thumbnail"] ?? ""; } set { ViewState["Thumbnail"] = value; } }
        private bool CanEdit { get; set; }

        protected override void OnInit(EventArgs e)
        {
            if (Session["CourseBuilderToken"] == null) Session["CourseBuilderToken"] = Guid.NewGuid().ToString("N");
            ViewStateUserKey = Convert.ToString(Session["CourseBuilderToken"]);
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int userId;
            string role = Convert.ToString(Session["Role"]);
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out userId) || userId <= 0 ||
                (!role.Equals("Instructor", StringComparison.OrdinalIgnoreCase) && !role.Equals("Chef", StringComparison.OrdinalIgnoreCase)))
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"));
                return;
            }
            Form.Enctype = "multipart/form-data";
            try
            {
                if (Read("SELECT UserID FROM Users WHERE UserID=@id AND Role='Instructor' AND AccountStatus='Active'", Parameter("@id", userId)).Rows.Count == 0)
                    throw new InvalidOperationException("An active chef account is required to create courses.");
                if (!IsPostBack)
                {
                    LoadCategories();
                    InitializeCourseBuilder();
                }
                if (CourseID > 0) EnsureOwnedDraft();
                CanEdit = true;
                ShowStep(CurrentStep);
            }
            catch (SqlException) { BlockBuilder("The course builder cannot reach the database. Please try again shortly."); }
            catch (InvalidOperationException error) { BlockBuilder(error.Message); }
        }

        private void LoadCategories()
        {
            ddlCategory.DataSource = Read("SELECT CategoryID,CategoryName FROM Categories ORDER BY CategoryName");
            ddlCategory.DataTextField = "CategoryName";
            ddlCategory.DataValueField = "CategoryID";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert(0, new ListItem("Select category", ""));
            if (ddlCategory.Items.Count == 1)
                SetFeedback("No course categories are available yet. Ask an administrator to add a category before saving.");
        }

        private void InitializeCourseBuilder()
        {
            if (Request.QueryString["courseId"] == null) return;
            int courseId;
            if (!int.TryParse(Request.QueryString["courseId"], out courseId) || courseId <= 0)
                throw new InvalidOperationException("This draft link is not valid.");
            CourseID = courseId;
            EnsureOwnedDraft();
            var course = Read("SELECT * FROM Courses WHERE CourseID=@id AND InstructorID=@user", Parameter("@id", CourseID), Parameter("@user", InstructorID)).Rows[0];
            txtCourseTitle.Text = Convert.ToString(course["Title"]);
            txtDescription.Text = Convert.ToString(course["Description"]);
            ddlCategory.SelectedValue = Convert.ToString(course["CategoryID"]);
            ddlDifficulty.SelectedValue = Convert.ToString(course["DifficultyLevel"]);
            Thumbnail = Convert.ToString(course["Thumbnail"]);
            RecipeForm1.LoadRecipe(CourseID);
            LessonForm1.LoadLessons(CourseID);
            QuizForm1.LoadQuiz(CourseID);
            ActivityForm1.LoadActivity(CourseID);
            HighestStep = 5;
            SetFeedback("Draft loaded. Continue editing, then save your changes.");
        }

        private void EnsureOwnedDraft()
        {
            var rows = Read("SELECT CourseID FROM Courses WHERE CourseID=@id AND InstructorID=@user AND Status='Draft'",
                Parameter("@id", CourseID), Parameter("@user", InstructorID));
            if (rows.Rows.Count == 0) throw new InvalidOperationException("This draft is unavailable or has already been submitted for review.");
        }

        private void ShowStep(int step)
        {
            CurrentStep = Math.Max(0, Math.Min(5, step));
            Panel[] panels = { pnlBasicInfo, pnlRecipe, pnlLessons, pnlQuiz, pnlActivity, pnlPreview };
            for (int index = 0; index < panels.Length; index++) panels[index].Visible = index == CurrentStep;
            litStepCount.Text = "Step " + (CurrentStep + 1) + " of 6";
            litStepTitle.Text = StepNames[CurrentStep];
            btnPrevious.Visible = CurrentStep > 0;
            btnNext.Visible = CurrentStep < 5;
            btnSubmitCourse.Visible = CurrentStep == 5;
            if (CurrentStep == 5 && CourseID > 0) CoursePreview1.LoadPreview(CourseID);
        }

        private List<string> ValidateBasicInfo()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(txtCourseTitle.Text)) errors.Add("Please enter the course title.");
            if (txtCourseTitle.Text.Length > 150) errors.Add("Keep the course title within 150 characters.");
            if (string.IsNullOrWhiteSpace(txtDescription.Text)) errors.Add("Please describe what learners will learn.");
            int categoryId;
            if (!int.TryParse(ddlCategory.SelectedValue, out categoryId)) errors.Add("Please select a course category.");
            if (ddlDifficulty.SelectedValue != "Beginner" && ddlDifficulty.SelectedValue != "Intermediate" && ddlDifficulty.SelectedValue != "Advanced")
                errors.Add("Please select a difficulty level.");
            return errors;
        }

        private List<string> StepErrors(int step)
        {
            switch (step)
            {
                case 0: return ValidateBasicInfo();
                case 1: return RecipeForm1.ValidateRecipe();
                case 2: return LessonForm1.ValidateLessons();
                case 3: return QuizForm1.ValidateQuiz();
                case 4: return ActivityForm1.ValidateActivity();
                default: return new List<string>();
            }
        }

        private bool ValidateCurrentStep()
        {
            var errors = StepErrors(CurrentStep);
            if (errors.Count == 0) return true;
            SetErrors(errors);
            return false;
        }

        private void CaptureCurrentStep()
        {
            if (CurrentStep < 5) MarkUnsaved();
            if (CurrentStep == 2) LessonForm1.CaptureLessons();
            if (CurrentStep == 3) QuizForm1.CaptureQuiz();
            CompletedSteps &= ~(1 << CurrentStep);
        }

        private void SaveBasicInfo()
        {
            var errors = ValidateBasicInfo();
            if (errors.Count > 0) throw new InvalidOperationException(string.Join(" ", errors));
            Thumbnail = SaveUpload(fuCourseThumbnail, Thumbnail);
            if (CourseID == 0)
            {
                using (var connection = DatabaseManager.ConnectDB())
                {
                    connection.Open();
                    CourseID = Convert.ToInt32(Execute(connection, null,
                        "INSERT INTO Courses (InstructorID,CategoryID,Title,Description,DifficultyLevel,Thumbnail,Status,CreatedAt,UpdatedAt) " +
                        "OUTPUT INSERTED.CourseID SELECT @user,@category,@title,@description,@difficulty,@thumbnail,'Draft',GETDATE(),GETDATE() " +
                        "WHERE EXISTS (SELECT 1 FROM Users WHERE UserID=@user AND Role='Instructor' AND AccountStatus='Active')",
                        BasicParameters()));
                    if (CourseID == 0) throw new InvalidOperationException("Your account cannot create a draft right now.");
                }
                return;
            }
            EditDraft(CourseID, (connection, transaction) => {
                var parameters = new List<SqlParameter>(BasicParameters()) { Parameter("@id", CourseID) };
                Execute(connection, transaction, "UPDATE Courses SET CategoryID=@category,Title=@title,Description=@description,DifficultyLevel=@difficulty,Thumbnail=@thumbnail WHERE CourseID=@id", parameters.ToArray());
            });
        }

        private SqlParameter[] BasicParameters()
        {
            return new[] { Parameter("@user", InstructorID), Parameter("@category", int.Parse(ddlCategory.SelectedValue)),
                Parameter("@title", txtCourseTitle.Text.Trim()), Parameter("@description", txtDescription.Text.Trim()),
                Parameter("@difficulty", ddlDifficulty.SelectedValue), Parameter("@thumbnail", Thumbnail) };
        }

        private void SaveCurrentStep()
        {
            switch (CurrentStep)
            {
                case 0: SaveBasicInfo(); break;
                case 1: RecipeForm1.SaveRecipe(CourseID); break;
                case 2: LessonForm1.SaveLessons(CourseID); break;
                case 3: QuizForm1.SaveQuiz(CourseID); break;
                case 4: ActivityForm1.SaveActivity(CourseID); break;
            }
            if (StepErrors(CurrentStep).Count == 0 && CurrentStep < 5) CompletedSteps |= 1 << CurrentStep;
            HasUnsavedChanges = false;
            litSaveNote.Text = "Draft saved at " + DateTime.Now.ToString("HH:mm") + ".";
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            RunAction(() => { CaptureCurrentStep(); SaveCurrentStep(); ShowStep(CurrentStep - 1); });
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            RunAction(() => {
                CaptureCurrentStep();
                if (!ValidateCurrentStep()) return;
                SaveCurrentStep();
                HighestStep = Math.Max(HighestStep, CurrentStep + 1);
                ShowStep(CurrentStep + 1);
            });
        }

        protected void btnSaveDraft_Click(object sender, EventArgs e)
        {
            RunAction(() => { CaptureCurrentStep(); SaveCurrentStep(); SetFeedback("Draft saved. You can return using the resume link below."); });
        }

        protected void Step_Command(object sender, CommandEventArgs e)
        {
            RunAction(() => {
                int step = Convert.ToInt32(e.CommandArgument);
                if (step > HighestStep || step == CurrentStep) return;
                CaptureCurrentStep();
                SaveCurrentStep();
                ShowStep(step);
            });
        }

        protected void btnSubmitCourse_Click(object sender, EventArgs e)
        {
            RunAction(() => {
                bool submitted = false;
                EditDraft(CourseID, (connection, transaction) => {
                    // The course lock prevents another editor changing content during validation.
                    RecipeForm1.LoadRecipe(CourseID);
                    LessonForm1.LoadLessons(CourseID);
                    QuizForm1.LoadQuiz(CourseID);
                    ActivityForm1.LoadActivity(CourseID);
                    for (int step = 0; step < 5; step++)
                    {
                        var errors = StepErrors(step);
                        if (errors.Count == 0) continue;
                        ShowStep(step);
                        SetErrors(errors);
                        return;
                    }
                    Execute(connection, transaction, "UPDATE Courses SET Status='Pending' WHERE CourseID=@id", Parameter("@id", CourseID));
                    submitted = true;
                });
                if (!submitted) return;
                CanEdit = false;
                pnlBuilder.Visible = false;
                litCourseStatus.Text = "Pending review";
                SetFeedback("Course submitted for review. An administrator will review it before it is published.");
            });
        }

        public void RunAction(Action action)
        {
            if (!CanEdit) return;
            pnlErrors.Visible = false;
            pnlFeedback.Visible = false;
            try { action(); }
            catch (SqlException error) { Trace.Warn("Course builder", "Database operation failed.", error); SetErrors(new[] { "Changes could not be saved. Check field lengths and try again. Your on-screen text is still here." }); }
            catch (InvalidOperationException error) { SetErrors(new[] { error.Message }); }
            catch (IOException) { SetErrors(new[] { "The file could not be saved. Please select it again and retry." }); }
            catch (UnauthorizedAccessException) { SetErrors(new[] { "The upload folder is not writable. Please contact the administrator." }); }
        }

        public void MarkUnsaved()
        {
            HasUnsavedChanges = true;
            CompletedSteps &= ~(1 << CurrentStep);
            litSaveNote.Text = "Unsaved changes - select Save draft to keep them.";
        }
        private void SetFeedback(string message) { pnlFeedback.Visible = true; litFeedback.Text = message; }
        private void SetErrors(IEnumerable<string> errors) { pnlErrors.Visible = true; rptErrors.DataSource = errors; rptErrors.DataBind(); }
        private void BlockBuilder(string message) { CanEdit = false; pnlBuilder.Visible = false; SetErrors(new[] { message }); }
        protected string StepClass(int step) { return step == CurrentStep ? "is-current" : ((CompletedSteps & (1 << step)) != 0 ? "is-complete" : ""); }
        protected string StepMarker(int step) { return (CompletedSteps & (1 << step)) != 0 && step != CurrentStep ? "\u2713" : (step + 1).ToString(); }
        protected string StepState(int step) { return step == CurrentStep ? " (current step)" : ((CompletedSteps & (1 << step)) != 0 ? " (complete)" : ""); }

        protected override void OnPreRender(EventArgs e)
        {
            LinkButton[] steps = { btnStep0, btnStep1, btnStep2, btnStep3, btnStep4, btnStep5 };
            for (int index = 0; index < steps.Length; index++)
            {
                steps[index].Enabled = CanEdit && index <= HighestStep;
                if (index == CurrentStep) steps[index].Attributes["aria-current"] = "step";
                else steps[index].Attributes.Remove("aria-current");
            }
            if (CanEdit)
            {
                litCourseStatus.Text = CourseID == 0 ? "New draft" : "Draft #" + CourseID;
                lnkResume.Visible = CourseID > 0;
                lnkResume.NavigateUrl = ResolveUrl("~/Pages/Chef/CreateCourse/CreateCoursePage.aspx?courseId=" + CourseID);
                imgThumbnail.ImageUrl = MediaUrl(Thumbnail);
                imgThumbnail.Style["display"] = imgThumbnail.ImageUrl.Length == 0 ? "none" : "block";
            }
            base.OnPreRender(e);
        }
    }
}
