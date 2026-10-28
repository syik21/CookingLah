using CookingLah.F_Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.ManageCourse
{
    // All content writes lock and recheck the parent course in the same transaction.
    public static class CourseManagementData
    {
        private static string ReviewChangesKey(int courseId) { return "ManageCourseReviewChanges_" + courseId; }

        public static bool HasChangesAwaitingReview(int courseId)
        {
            return HttpContext.Current.Session[ReviewChangesKey(courseId)] is CourseReviewDraft;
        }

        public static void DiscardChangesAwaitingReview(int courseId)
        {
            var draft = GetReviewDraft(courseId, false);
            if (draft != null) CleanupUnreferencedMedia(draft.MediaPathsToClean());
            HttpContext.Current.Session.Remove(ReviewChangesKey(courseId));
        }

        public static void CleanupUnreferencedMedia(IEnumerable<string> paths)
        {
            if (paths == null) return;
            foreach (string path in paths)
            {
                if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("~/F_Assets/CourseMedia/", StringComparison.OrdinalIgnoreCase)) continue;
                string fileName = path.Substring("~/F_Assets/CourseMedia/".Length);
                if (fileName.Length == 0 || Path.GetFileName(fileName) != fileName) continue;
                try
                {
                    int references = Convert.ToInt32(Read(@"
                        SELECT (SELECT COUNT(*) FROM Courses WHERE Thumbnail=@path)
                             + (SELECT COUNT(*) FROM CourseRecipes WHERE RecipeImage=@path)
                             + (SELECT COUNT(*) FROM LessonContent WHERE MediaPath=@path)", Parameter("@path", path)).Rows[0][0]);
                    if (references != 0) continue;
                    string mediaRoot = Path.GetFullPath(HttpContext.Current.Server.MapPath("~/F_Assets/CourseMedia/"));
                    string fullPath = Path.GetFullPath(HttpContext.Current.Server.MapPath(path));
                    if (fullPath.StartsWith(mediaRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath)) File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Unable to clean unreferenced course media: {0}", ex);
                }
            }
        }

        public static bool HasChangesAwaitingReview(int courseId, object updatedAt)
        {
            return HasChangesAwaitingReview(courseId);
        }

        public static CourseReviewDraft GetReviewDraft(int courseId, bool create)
        {
            if (create)
            {
                int instructorId;
                if (!int.TryParse(Convert.ToString(HttpContext.Current.Session["UserId"]), out instructorId) ||
                    HttpContext.Current.Session["BuilderEditing_" + courseId] == null ||
                    Read(@"SELECT c.CourseID FROM Courses c JOIN Users u ON u.UserID=c.InstructorID
                        WHERE c.CourseID=@id AND c.InstructorID=@user AND c.Status IN ('Draft','Rejected')
                        AND u.Role='Instructor' AND u.AccountStatus='Active'", Parameter("@id", courseId), Parameter("@user", instructorId)).Rows.Count == 0)
                    throw new InvalidOperationException("This course is not editable. Reload it and click Edit Course.");
            }
            string key = ReviewChangesKey(courseId);
            var draft = HttpContext.Current.Session[key] as CourseReviewDraft;
            if (draft == null && create)
            {
                draft = CourseReviewDraft.Load(courseId);
                HttpContext.Current.Session[key] = draft;
            }
            return draft;
        }

        public static DataTable ReadDraftSection(int courseId, string section, string sql, params SqlParameter[] parameters)
        {
            var draft = HttpContext.Current.Session["BuilderEditing_" + courseId] == null ? null : GetReviewDraft(courseId, false);
            if (draft != null)
            {
                switch (section)
                {
                    case "Courses": return draft.Courses.Copy();
                    case "CourseRecipes": return draft.Recipes.Copy();
                    case "Lessons": return draft.Lessons.Copy();
                    case "LessonContent": return draft.LessonContent.Copy();
                    case "Quizzes": return draft.Quizzes.Copy();
                    case "QuizQuestions": return draft.Questions.Copy();
                    case "QuizOptions": return draft.Options.Copy();
                    case "PracticalActivities": return draft.Activities.Copy();
                }
            }
            return Read(sql, parameters);
        }

        public static void ApplyDraftCourse(int courseId, DataRow target)
        {
            var draft = GetReviewDraft(courseId, false);
            if (draft == null || draft.Courses.Rows.Count == 0) return;
            DataRow staged = draft.Courses.Rows[0];
            foreach (string column in new[] { "Title", "Description", "CategoryID", "FoodCultureID", "DifficultyLevel", "Thumbnail" })
                target[column] = staged[column];

            string stagedDishName = staged.Table.Columns.Contains("NewFoodDishName")
                ? Convert.ToString(staged["NewFoodDishName"])
                : "";
            if (!string.IsNullOrWhiteSpace(stagedDishName) && staged.Table.Columns.Contains("NewFoodCountry"))
            {
                string country = Convert.ToString(staged["NewFoodCountry"]);
                target["FoodDisplayName"] = string.IsNullOrWhiteSpace(country)
                    ? stagedDishName
                    : stagedDishName + " \u2014 " + country;
            }
            else if (staged["FoodCultureID"] == DBNull.Value)
            {
                target["FoodDisplayName"] = "Not linked to a Traditional Food";
            }
            else
            {
                var food = Read(@"SELECT fc.DishName + NCHAR(8212) + fc.Country AS FoodDisplayName
                    FROM FoodCultures fc
                    WHERE fc.FoodCultureID=@id", Parameter("@id", staged["FoodCultureID"]));
                target["FoodDisplayName"] = food.Rows.Count == 0
                    ? "Not linked to a Traditional Food"
                    : Convert.ToString(food.Rows[0]["FoodDisplayName"]);
            }
            var category = Read("SELECT CategoryName FROM Categories WHERE CategoryID=@id", Parameter("@id", staged["CategoryID"]));
            if (category.Rows.Count > 0) target["CategoryName"] = category.Rows[0]["CategoryName"];
        }

        public static bool IsPublishedCourse(int courseId)
        {
            var draft = GetReviewDraft(courseId, false);
            if (draft != null && draft.Courses.Rows.Count > 0)
                return Convert.ToString(draft.Courses.Rows[0]["Status"]) == "Published";
            return Convert.ToString(Read("SELECT Status FROM Courses WHERE CourseID=@id", Parameter("@id", courseId)).Rows[0]["Status"]) == "Published";
        }

        public static void EditCourse(int courseId, Action<SqlConnection, SqlTransaction> save, bool submittingForReview = false)
        {
            if (!submittingForReview && HttpContext.Current.Session["BuilderEditing_" + courseId] == null)
                throw new InvalidOperationException("Click Edit Course before saving.");
            int instructorId = Convert.ToInt32(HttpContext.Current.Session["UserId"]);
            using (var connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    object status = Execute(connection, transaction,
                        "SELECT c.Status FROM Courses c WITH (UPDLOCK,HOLDLOCK) JOIN Users u ON u.UserID=c.InstructorID " +
                        "WHERE c.CourseID=@id AND c.InstructorID=@user AND u.Role='Instructor' AND u.AccountStatus='Active'",
                        Parameter("@id", courseId), Parameter("@user", instructorId));
                    if (status == null) throw new InvalidOperationException("You do not have permission to manage this course.");
                    if (Convert.ToString(status) != "Draft" && Convert.ToString(status) != "Rejected")
                        throw new InvalidOperationException("Only Draft or Rejected courses can be edited.");
                    save(connection, transaction);
                    Execute(connection, transaction, "UPDATE Courses SET UpdatedAt=GETDATE() WHERE CourseID=@id", Parameter("@id", courseId));
                    transaction.Commit();
                    if (submittingForReview)
                    {
                        var draft = GetReviewDraft(courseId, false);
                        if (draft != null) CleanupUnreferencedMedia(draft.MediaPathsToClean());
                        HttpContext.Current.Session.Remove(ReviewChangesKey(courseId));
                    }
                }
            }
        }

        public static string Required(string text, string label, int maxLength = int.MaxValue)
        {
            text = text.Trim();
            if (text.Length == 0) throw new InvalidOperationException("Please enter " + label + ".");
            if (text.Length > maxLength) throw new InvalidOperationException("Keep " + label + " within " + maxLength + " characters.");
            return text;
        }

        public static string Optional(string text, string label, int maxLength = int.MaxValue)
        {
            text = text.Trim();
            if (text.Length > maxLength) throw new InvalidOperationException("Keep " + label + " within " + maxLength + " characters.");
            return text;
        }

        public static object PositiveNumber(string text, string label)
        {
            if (string.IsNullOrWhiteSpace(text)) return DBNull.Value;
            int number;
            if (!int.TryParse(text, out number) || number < 1)
                throw new InvalidOperationException(label + " must be a positive whole number.");
            return number;
        }

        // Moving every order above the current maximum avoids the UNIQUE order
        // constraint while the second statement restores a consecutive sequence.
        public static void NormalizeLessons(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            Execute(connection, transaction, @"
                DECLARE @offset INT=(SELECT ISNULL(MAX(LessonOrder),0) FROM Lessons WHERE CourseID=@id);
                UPDATE Lessons SET LessonOrder=LessonOrder+@offset WHERE CourseID=@id;
                WITH ordered AS (SELECT LessonOrder,ROW_NUMBER() OVER(ORDER BY LessonOrder,LessonID) AS NewOrder
                FROM Lessons WHERE CourseID=@id) UPDATE ordered SET LessonOrder=NewOrder;", Parameter("@id", courseId));
        }

        public static void NormalizeQuestions(SqlConnection connection, SqlTransaction transaction, int quizId)
        {
            Execute(connection, transaction, @"
                DECLARE @offset INT=(SELECT ISNULL(MAX(QuestionOrder),0) FROM QuizQuestions WHERE QuizID=@id);
                UPDATE QuizQuestions SET QuestionOrder=QuestionOrder+@offset WHERE QuizID=@id;
                WITH ordered AS (SELECT QuestionOrder,ROW_NUMBER() OVER(ORDER BY QuestionOrder,QuestionID) AS NewOrder
                FROM QuizQuestions WHERE QuizID=@id) UPDATE ordered SET QuestionOrder=NewOrder;", Parameter("@id", quizId));
        }
    }

    public class CourseManagementControl : System.Web.UI.UserControl
    {
        protected ManageCoursePage Host { get { return (ManageCoursePage)Page; } }
        protected int CourseId { get { return Host.CourseId; } }
        protected bool CanEdit { get { return Host.CanEdit; } }
        protected bool IsPublishedCourse { get { return Host.Course != null && Convert.ToString(Host.Course["Status"]) == "Published"; } }
        protected void Run(Action action) { Host.RunAction(action); }
        protected void Saved(string message) { Host.Saved(message); }
        protected void Saved(string message, int currentPage) { Host.Saved(message, currentPage); }
        protected void EnsureEditable()
        {
            if (!CanEdit) throw new InvalidOperationException("This course cannot be edited while it is under review.");
        }
    }
}
