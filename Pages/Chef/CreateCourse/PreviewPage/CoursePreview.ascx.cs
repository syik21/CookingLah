using System;
using System.Data;
using System.Web.UI.WebControls;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.CreateCourse.PreviewPage
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
            litTitle.Text = Convert.ToString(course["Title"]);
            litDescription.Text = Convert.ToString(course["Description"]);
            litCategory.Text = Convert.ToString(course["CategoryName"]);
            litDifficulty.Text = Convert.ToString(course["DifficultyLevel"]);
            litFoodCulture.Text = Convert.ToString(course["FoodDisplayName"]);
            imgCourse.ImageUrl = MediaUrl(Convert.ToString(course["Thumbnail"]));
            imgCourse.Visible = imgCourse.ImageUrl.Length > 0;
            BindSection(rptRecipe, litRecipeEmpty, Read("SELECT * FROM CourseRecipes WHERE CourseID=@id", Parameter("@id", courseId)));
            lessonBlocks = Read("SELECT lc.* FROM LessonContent lc JOIN Lessons l ON l.LessonID=lc.LessonID WHERE l.CourseID=@id", Parameter("@id", courseId));
            BindSection(rptPreviewLessons, litLessonsEmpty, Read("SELECT * FROM Lessons WHERE CourseID=@id ORDER BY LessonOrder", Parameter("@id", courseId)));
            BindSection(rptPreviewQuiz, litQuizEmpty, Read("SELECT q.*, (SELECT COUNT(*) FROM QuizQuestions WHERE QuizID=q.QuizID) AS QuestionCount FROM Quizzes q WHERE CourseID=@id", Parameter("@id", courseId)));
            BindSection(rptPreviewActivity, litActivityEmpty, Read("SELECT * FROM PracticalActivities WHERE CourseID=@id ORDER BY ActivityID", Parameter("@id", courseId)));
            previewOptions = Read("SELECT o.* FROM QuizOptions o JOIN QuizQuestions qq ON qq.QuestionID=o.QuestionID JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE q.CourseID=@id ORDER BY o.OptionID", Parameter("@id", courseId));
            rptPreviewQuestions.DataSource = Read("SELECT qq.* FROM QuizQuestions qq JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE q.CourseID=@id ORDER BY qq.QuestionOrder", Parameter("@id", courseId));
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
