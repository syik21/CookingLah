using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace CookingLah.Pages.Admin.CourseContentReview.ViewCourse
{
    public partial class ViewCourse : System.Web.UI.UserControl
    {
        // Events used to notify the parent review page.
        public event EventHandler BackRequested;
        public event EventHandler CourseReviewed;
        public string ReviewMessage { get; private set; }
        // Keep the selected course and pagination positions across postbacks.
        private int CourseId
        {
            get { return (int)(ViewState["CourseId"] ?? 0); }
            set { ViewState["CourseId"] = value; }
        }

        private int LessonPage
        {
            get { return (int)(ViewState["LessonPage"] ?? 0); }
            set { ViewState["LessonPage"] = value; }
        }

        private int QuestionPage
        {
            get { return (int)(ViewState["QuestionPage"] ?? 0); }
            set { ViewState["QuestionPage"] = value; }
        }
        private DataTable questionOptions;
        private DataTable lessonBlocks;

        protected void Lesson_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            var lesson = (DataRowView)e.Item.DataItem;
            var blocks = (CookingLah.Pages.Shared.LessonBlocks.LessonBlocks)e.Item.FindControl("LessonBlocks");
            blocks.BindBlocks(lessonBlocks, Convert.ToInt32(lesson["LessonID"]));
        }


        private DataRow course;

        // Reset the selected course and review form.
        public void LoadCourse(int courseId)
        {
            CourseId = courseId;
            LessonPage = 0;
            QuestionPage = 0;
            hfActiveTab.Value = "overview";
            hfDialog.Value = "";
            txtRejectionReason.Text = "";
            litRejectError.Text = "";
            pnlError.Visible = false;
        }

        // Load course details and show only the selected tab.
        protected override void OnPreRender(EventArgs e)
        {
            if (Visible && CourseId > 0)
            {
                try
                {
                    pnlDetails.Visible = LoadOverview();
                    if (pnlDetails.Visible)
                    {
                        LoadRecipe();
                        LoadLessons();
                        LoadQuiz();
                        LoadActivity();
                        ApplyCourseStatus();
                    }
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Trace.TraceError("Course review details: {0}", ex);
                    pnlDetails.Visible = false;
                    ShowError("Course content could not be loaded. Please go back and try again.");
                }
                foreach (Panel panel in new[] { tabOverview, tabRecipe, tabLessons, tabQuiz, tabActivity })
                {
                    bool active = panel.Attributes["data-section"] == hfActiveTab.Value;
                    if (active)
                    {
                        panel.Attributes.Remove("hidden");
                    }
                    else
                    {
                        panel.Attributes["hidden"] = "hidden";
                    }
                }
            }
            base.OnPreRender(e);
        }

        // Execute a course query using its ID.
        private DataTable ReadCourse(string sql)
        {
            return ReviewData.Read(sql, ReviewData.Int("@id", CourseId));
        }

        // Load the course information and content counts.
        private bool LoadOverview()
        {
            var rows = ReadCourse(@"SELECT c.*,u.Username AS ChefName,cat.CategoryName,
                (SELECT COUNT(*) FROM CourseRecipes WHERE CourseID=c.CourseID) AS RecipeCount,
                (SELECT COUNT(*) FROM Lessons WHERE CourseID=c.CourseID) AS LessonCount,
                (SELECT COUNT(*) FROM Quizzes WHERE CourseID=c.CourseID) AS QuizCount,
                (SELECT COUNT(*) FROM QuizQuestions qq JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE q.CourseID=c.CourseID) AS QuestionCount,
                (SELECT COUNT(*) FROM PracticalActivities WHERE CourseID=c.CourseID) AS ActivityCount
                FROM Courses c JOIN Users u ON u.UserID=c.InstructorID JOIN Categories cat ON cat.CategoryID=c.CategoryID
                WHERE c.CourseID=@id AND c.Status IN ('Pending','Published','Rejected')");
            if (rows.Rows.Count == 0)
            {
                ShowError("This course is no longer available for review.");
                return false;
            }
            course = rows.Rows[0];
            litTitle.Text = litOverviewTitle.Text = litApproveTitle.Text = Convert.ToString(course["Title"]);
            litChef.Text = Convert.ToString(course["ChefName"]);
            litCategory.Text = Convert.ToString(course["CategoryName"]);
            litDifficulty.Text = Convert.ToString(course["DifficultyLevel"]);
            litMeta.Text = "Chef " + litChef.Text + " · " + litCategory.Text + " · " + litDifficulty.Text;
            litDescription.Text = Convert.ToString(course["Description"]);
            litCreated.Text = Convert.ToDateTime(course["CreatedAt"]).ToString("dd MMM yyyy");
            litUpdated.Text = course.IsNull("UpdatedAt") ? "Not updated yet" : Convert.ToDateTime(course["UpdatedAt"]).ToString("dd MMM yyyy");
            imgCourse.ImageUrl = MediaUrl(course["Thumbnail"]);
            imgCourse.Visible = imgCourse.ImageUrl.Length > 0;
            pnlCourseImagePlaceholder.Visible = !imgCourse.Visible;
            litSummary.Text = "<ul><li>" + (Convert.ToInt32(course["RecipeCount"]) > 0 ? "✓ Recipe Available" : "Recipe not added") + "</li>";
            string[] columns = { "LessonCount", "QuizCount", "QuestionCount", "ActivityCount" };
            string[] labels = { " Lessons", " Quiz", " Quiz Questions", " Practical Activity" };
            for (int index = 0; index < columns.Length; index++)
            {
                int count = Convert.ToInt32(course[columns[index]]);
                litSummary.Text += "<li>" + (count > 0 ? "✓ " : "") + count + labels[index] + "</li>";
            }
            litSummary.Text += "</ul>";
            return true;
        }

        // Show the recipe associated with the course.
        private void LoadRecipe()
        {
            // Recipe description is the course description in the existing schema.
            BindSection(rptRecipe, litRecipeEmpty, ReadCourse("SELECT r.*,c.Title,c.Description FROM CourseRecipes r JOIN Courses c ON c.CourseID=r.CourseID WHERE r.CourseID=@id"));
        }

        // Load lessons with pagination.
        private void LoadLessons()
        {
            lessonBlocks = ReadCourse("SELECT lc.* FROM LessonContent lc JOIN Lessons l ON l.LessonID=lc.LessonID WHERE l.CourseID=@id");
            var rows = ReadCourse("SELECT * FROM Lessons WHERE CourseID=@id ORDER BY LessonOrder,LessonID");
            litLessonsEmpty.Visible = rows.Rows.Count == 0;
            LessonPage = BindPage(rptLessons, rows, LessonPage, pnlLessonPager, btnLessonPrevious, btnLessonNext, litLessonPage);
        }

        // Load quizzes, questions, and answer options.
        private void LoadQuiz()
        {
            BindSection(rptQuizzes, litQuizEmpty, ReadCourse("SELECT q.*,(SELECT COUNT(*) FROM QuizQuestions WHERE QuizID=q.QuizID) AS QuestionCount FROM Quizzes q WHERE CourseID=@id ORDER BY QuizID"));
            questionOptions = ReadCourse("SELECT o.* FROM QuizOptions o JOIN QuizQuestions qq ON qq.QuestionID=o.QuestionID JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE q.CourseID=@id ORDER BY o.OptionID");
            var questions = ReadCourse("SELECT qq.*,q.Title AS QuizTitle FROM QuizQuestions qq JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE q.CourseID=@id ORDER BY q.QuizID,qq.QuestionOrder,qq.QuestionID");
            litQuestionsEmpty.Visible = questions.Rows.Count == 0 && rptQuizzes.Items.Count > 0;
            QuestionPage = BindPage(rptQuestions, questions, QuestionPage, pnlQuestionPager, btnQuestionPrevious, btnQuestionNext, litQuestionPage);
        }

        // Load practical activities.
        private void LoadActivity()
        {
            BindSection(rptActivities, litActivityEmpty, ReadCourse("SELECT * FROM PracticalActivities WHERE CourseID=@id ORDER BY ActivityID"));
        }

        // Bind a section and display its empty message when necessary.
        private void BindSection(Repeater repeater, Literal empty, DataTable rows)
        {
            empty.Visible = rows.Rows.Count == 0;
            repeater.DataSource = rows;
            repeater.DataBind();
        }

        // Display five records per page and update navigation controls.
        private int BindPage(Repeater repeater, DataTable rows, int page, Panel pager, LinkButton previous, LinkButton next, Literal label)
        {
            int pageCount = Math.Max(1, (rows.Rows.Count + 4) / 5);
            page = Math.Max(0, Math.Min(page, pageCount - 1));
            repeater.DataSource = new PagedDataSource
            {
                DataSource = rows.DefaultView,
                AllowPaging = true,
                PageSize = 5,
                CurrentPageIndex = page
            };
            repeater.DataBind();
            pager.Visible = pageCount > 1;
            previous.Enabled = page > 0;
            next.Enabled = page < pageCount - 1;
            label.Text = string.Format("Page {0} of {1}", page + 1, pageCount);
            return page;
        }

        // Bind answer options for each quiz question.
        protected void Questions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }
            int questionId = Convert.ToInt32(((DataRowView)e.Item.DataItem)["QuestionID"]);
            var options = (Repeater)e.Item.FindControl("rptQuestionOptions");
            options.DataSource = new DataView(questionOptions) { RowFilter = "QuestionID=" + questionId };
            options.DataBind();
        }

        // Display the current status and available review actions.
        private void ApplyCourseStatus()
        {
            string status = Convert.ToString(course["Status"]);
            lblStatus.Text = litOverviewStatus.Text = ReviewData.StatusLabel(status);
            lblStatus.CssClass = "review-badge " + (status == "Published" ? "approved" : status == "Rejected" ? "rejected" : "pending");
            pnlDecision.Visible = status == "Pending";
            pnlPublished.Visible = status == "Published";
            pnlRejection.Visible = status == "Rejected";
            litRejection.Text = string.IsNullOrWhiteSpace(Convert.ToString(course["RejectionReason"])) ? "No rejection reason was recorded." : Convert.ToString(course["RejectionReason"]);
        }

        // Back navigation and lesson/question pagination.
        protected void Back_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
            {
                BackRequested(this, EventArgs.Empty);
            }
        }

        protected void LessonPrevious_Click(object sender, EventArgs e)
        {
            LessonPage--;
            hfActiveTab.Value = "lessons";
        }

        protected void LessonNext_Click(object sender, EventArgs e)
        {
            LessonPage++;
            hfActiveTab.Value = "lessons";
        }

        protected void QuestionPrevious_Click(object sender, EventArgs e)
        {
            QuestionPage--;
            hfActiveTab.Value = "quiz";
        }

        protected void QuestionNext_Click(object sender, EventArgs e)
        {
            QuestionPage++;
            hfActiveTab.Value = "quiz";
        }

        // Approve or reject the selected course.
        protected void Approve_Click(object sender, EventArgs e)
        {
            SaveDecision("Published", null);
        }

        protected void Reject_Click(object sender, EventArgs e)
        {
            string reason = txtRejectionReason.Text.Trim();
            if (reason.Length == 0 || reason.Length > 500)
            {
                litRejectError.Text = reason.Length == 0
                    ? "Please provide a reason for rejecting this course."
                    : "Please keep the rejection reason to 500 characters or fewer.";
                hfDialog.Value = "reject";
                return;
            }
            SaveDecision("Rejected", reason);
        }

        // Save the admin decision only while the course is pending.
        private void SaveDecision(string status, string reason)
        {
            try
            {
                using (var connection = DatabaseManager.ConnectDB())
                using (var command = new SqlCommand(@"UPDATE Courses SET Status=@status,RejectionReason=@reason,UpdatedAt=GETDATE()
                    OUTPUT INSERTED.Title WHERE CourseID=@id AND Status='Pending'
                    AND EXISTS (SELECT 1 FROM Users WHERE UserID=@admin AND Role='Admin' AND AccountStatus='Active')", connection))
                {
                    command.Parameters.Add(ReviewData.Int("@id", CourseId));
                    command.Parameters.Add(ReviewData.Int("@admin", Convert.ToInt32(Session["UserId"])));
                    command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = status;
                    command.Parameters.Add("@reason", SqlDbType.NVarChar, 500).Value = (object)reason ?? DBNull.Value;
                    connection.Open();
                    object title = command.ExecuteScalar();
                    hfDialog.Value = "";
                    if (title == null)
                    {
                        ShowError("This course is no longer pending, or your admin access has changed. No decision was saved.");
                        return;
                    }
                    ReviewMessage = status == "Published" ? "Course approved successfully. \"" + Convert.ToString(title) + "\" is now available to learners." : "Course rejected successfully. The rejection reason will be shown to the Chef so they can make the required changes.";
                }
                if (CourseReviewed != null)
                {
                    CourseReviewed(this, EventArgs.Empty);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Course review decision: {0}", ex);
                hfDialog.Value = "";
                ShowError("The decision could not be saved. Please try again.");
            }
        }

        // Display an error message to the administrator.
        private void ShowError(string message)
        {
            pnlError.Visible = true;
            litError.Text = message;
        }

        // Helpers used in the ASPX markup.
        protected string MediaUrl(object value)
        {
            return ReviewData.MediaUrl(value);
        }

        protected string OptionalNumber(object value, string unit)
        {
            return value == DBNull.Value ? "Not provided" : Convert.ToString(value) + unit;
        }

        protected string FormatIngredients(object value)
        {
            return CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.FormatIngredients(value);
        }
    }
}
