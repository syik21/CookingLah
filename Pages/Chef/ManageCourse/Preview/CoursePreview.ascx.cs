using System;
using System.Data;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;
using static CookingLah.Pages.Chef.ManageCourse.CourseManagementData;

namespace CookingLah.Pages.Chef.ManageCourse.Preview
{
    public partial class CoursePreview : System.Web.UI.UserControl
    {
        private DataTable previewOptions;
        private DataTable lessonBlocks;

        protected void Lesson_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            var lesson = (DataRowView)e.Item.DataItem;
            var blocks = (CookingLah.Pages.Shared.LessonBlocks.LessonBlocks)e.Item.FindControl("LessonBlocks");
            blocks.BindBlocks(lessonBlocks, Convert.ToInt32(lesson["LessonID"]));
        }



        public void LoadPreview(int courseId)
        {
            var rows = Read(@"SELECT c.*,cat.CategoryName,
                CASE WHEN fc.FoodCultureID IS NULL THEN N'Not linked to a Traditional Food'
                     ELSE fc.DishName + NCHAR(8212) + fc.Country END AS FoodDisplayName
                FROM Courses c
                JOIN Categories cat ON cat.CategoryID=c.CategoryID
                LEFT JOIN FoodCultures fc ON fc.FoodCultureID=c.FoodCultureID
                WHERE c.CourseID=@id AND c.InstructorID=@user",
                Parameter("@id", courseId), Parameter("@user", Convert.ToInt32(Session["UserId"])));
            if (rows.Rows.Count == 0) throw new InvalidOperationException("This course is unavailable.");
            var course = rows.Rows[0];
            ApplyDraftCourse(courseId, course);
            litTitle.Text = Convert.ToString(course["Title"]);
            litDescription.Text = Convert.ToString(course["Description"]);
            litCategory.Text = Convert.ToString(course["CategoryName"]);
            litDifficulty.Text = Convert.ToString(course["DifficultyLevel"]);
            litFoodCulture.Text = Convert.ToString(course["FoodDisplayName"]);
            imgCourse.ImageUrl = MediaUrl(Convert.ToString(course["Thumbnail"]));
            imgCourse.Visible = imgCourse.ImageUrl.Length > 0;
            BindSection(rptRecipe, litRecipeEmpty, ReadDraftSection(courseId, "CourseRecipes", "SELECT * FROM CourseRecipes WHERE CourseID=@id", Parameter("@id", courseId)));
            lessonBlocks = ReadDraftSection(courseId, "LessonContent", "SELECT lc.* FROM LessonContent lc JOIN Lessons l ON l.LessonID=lc.LessonID WHERE l.CourseID=@id", Parameter("@id", courseId));
            BindSection(rptPreviewLessons, litLessonsEmpty, ReadDraftSection(courseId, "Lessons", "SELECT * FROM Lessons WHERE CourseID=@id ORDER BY LessonOrder", Parameter("@id", courseId)));
            var quizzes = ReadDraftSection(courseId, "Quizzes", "SELECT * FROM Quizzes WHERE CourseID=@id", Parameter("@id", courseId));
            if (!quizzes.Columns.Contains("QuestionCount")) quizzes.Columns.Add("QuestionCount", typeof(int));
            var questions = ReadDraftSection(courseId, "QuizQuestions", "SELECT qq.* FROM QuizQuestions qq JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE q.CourseID=@id ORDER BY qq.QuestionOrder", Parameter("@id", courseId));
            foreach (DataRow quiz in quizzes.Rows)
                quiz["QuestionCount"] = questions.Select("QuizID=" + Convert.ToInt32(quiz["QuizID"])).Length;
            BindSection(rptPreviewQuiz, litQuizEmpty, quizzes);
            BindSection(rptPreviewActivity, litActivityEmpty, ReadDraftSection(courseId, "PracticalActivities", "SELECT * FROM PracticalActivities WHERE CourseID=@id ORDER BY ActivityID", Parameter("@id", courseId)));
            previewOptions = ReadDraftSection(courseId, "QuizOptions", "SELECT o.* FROM QuizOptions o JOIN QuizQuestions qq ON qq.QuestionID=o.QuestionID JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE q.CourseID=@id ORDER BY o.OptionID", Parameter("@id", courseId));
            rptPreviewQuestions.DataSource = questions;
            rptPreviewQuestions.DataBind();
        }

        private void BindSection(Repeater repeater, Literal emptyState, DataTable rows)
        {
            emptyState.Visible = rows.Rows.Count == 0;
            repeater.DataSource = rows;
            repeater.DataBind();
        }

        protected void rptPreviewQuestions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            int questionId = Convert.ToInt32(((DataRowView)e.Item.DataItem)["QuestionID"]);
            var options = (Repeater)e.Item.FindControl("rptPreviewOptions");
            previewOptions.DefaultView.RowFilter = "QuestionID=" + questionId;
            options.DataSource = previewOptions.DefaultView;
            options.DataBind();
        }
    }
}
