using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.ManageCourse
{
    // Session-only snapshot for edits to a published course. No content reaches SQL
    // until the instructor submits this snapshot for review.
    [Serializable]
    public sealed class CourseReviewDraft
    {
        public DataTable Courses { get; private set; }
        public DataTable Recipes { get; private set; }
        public DataTable Lessons { get; private set; }
        public DataTable Quizzes { get; private set; }
        public DataTable Questions { get; private set; }
        public DataTable Options { get; private set; }
        public DataTable Activities { get; private set; }
        private HashSet<string> OriginalMediaPaths { get; set; }
        private HashSet<string> StagedUploadPaths { get; set; }

        private CourseReviewDraft() { }

        internal static CourseReviewDraft Load(int courseId)
        {
            var draft = new CourseReviewDraft();
            draft.Courses = Read("SELECT * FROM Courses WHERE CourseID=@id", Parameter("@id", courseId));
            draft.Courses.Columns.Add("NewFoodDishName", typeof(string));
            draft.Courses.Columns.Add("NewFoodCountry", typeof(string));
            draft.Recipes = Read("SELECT * FROM CourseRecipes WHERE CourseID=@id", Parameter("@id", courseId));
            draft.Lessons = Read("SELECT * FROM Lessons WHERE CourseID=@id ORDER BY LessonOrder,LessonID", Parameter("@id", courseId));
            draft.Quizzes = Read("SELECT * FROM Quizzes WHERE CourseID=@id ORDER BY QuizID", Parameter("@id", courseId));
            draft.Questions = Read("SELECT qq.* FROM QuizQuestions qq JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE q.CourseID=@id ORDER BY qq.QuestionOrder,qq.QuestionID", Parameter("@id", courseId));
            draft.Options = Read("SELECT o.* FROM QuizOptions o JOIN QuizQuestions qq ON qq.QuestionID=o.QuestionID JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE q.CourseID=@id ORDER BY o.OptionID", Parameter("@id", courseId));
            draft.Activities = Read("SELECT * FROM PracticalActivities WHERE CourseID=@id ORDER BY ActivityID", Parameter("@id", courseId));
            draft.OriginalMediaPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            draft.StagedUploadPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddMediaColumn(draft.Courses, "Thumbnail", draft.OriginalMediaPaths);
            AddMediaColumn(draft.Recipes, "RecipeImage", draft.OriginalMediaPaths);
            AddMediaColumn(draft.Lessons, "LessonImage", draft.OriginalMediaPaths);
            AddMediaColumn(draft.Lessons, "LessonVideo", draft.OriginalMediaPaths);
            return draft;
        }

        private static void AddMediaColumn(DataTable table, string column, HashSet<string> paths)
        {
            if (!table.Columns.Contains(column)) return;
            foreach (DataRow row in table.Rows)
            {
                string path = Convert.ToString(row[column]);
                if (!string.IsNullOrWhiteSpace(path)) paths.Add(path);
            }
        }

        public void TrackMediaUpload(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !OriginalMediaPaths.Contains(path)) StagedUploadPaths.Add(path);
        }

        public IEnumerable<string> MediaPathsToClean()
        {
            var paths = new HashSet<string>(OriginalMediaPaths, StringComparer.OrdinalIgnoreCase);
            paths.UnionWith(StagedUploadPaths);
            return paths;
        }

        public static int NewId(DataTable table, string idColumn)
        {
            int smallest = 0;
            foreach (DataRow row in table.Rows)
                if (row.RowState != DataRowState.Deleted && Convert.ToInt32(row[idColumn]) < smallest)
                    smallest = Convert.ToInt32(row[idColumn]);
            return smallest - 1;
        }

        public static DataRow FindRow(DataTable table, string idColumn, int id)
        {
            foreach (DataRow row in table.Rows)
                if (row.RowState != DataRowState.Deleted && Convert.ToInt32(row[idColumn]) == id) return row;
            return null;
        }

        public void NormalizeLessons()
        {
            var ordered = Lessons.Select("", "LessonOrder ASC,LessonID ASC");
            for (int index = 0; index < ordered.Length; index++) ordered[index]["LessonOrder"] = index + 1;
        }

        public void Apply(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            DataRow course = Courses.Rows[0];

            int categoryId = Convert.ToInt32(course["CategoryID"]);
            object categoryName = Execute(connection, transaction,
                "SELECT CategoryName FROM Categories WHERE CategoryID=@categoryId",
                Parameter("@categoryId", categoryId));

            if (categoryName == null)
                throw new InvalidOperationException("Please select a valid course category.");

            bool needsFoodCulture = string.Equals(
                Convert.ToString(categoryName),
                "Traditional Dishes",
                StringComparison.OrdinalIgnoreCase);

            object foodCultureId = needsFoodCulture ? course["FoodCultureID"] : DBNull.Value;
            string newDishName = Convert.ToString(course["NewFoodDishName"]);
            if (!needsFoodCulture)
            {
                newDishName = "";
            }
            else if (!string.IsNullOrWhiteSpace(newDishName))
            {
                string country = ValidateCountry(Convert.ToString(course["NewFoodCountry"]));
                foodCultureId = CreateFoodCulture(connection, transaction, newDishName, country);
            }
            else if (foodCultureId == DBNull.Value)
            {
                throw new InvalidOperationException("Please select a Traditional Food for this category.");
            }
            else if (foodCultureId != DBNull.Value)
            {
                object existingFoodCulture = Execute(connection, transaction, @"
                    SELECT FoodCultureID
                    FROM FoodCultures WITH (UPDLOCK,HOLDLOCK)
                    WHERE FoodCultureID=@foodCultureId",
                    Parameter("@foodCultureId", foodCultureId));

                if (existingFoodCulture == null)
                    throw new InvalidOperationException("That Traditional Food is no longer available. Please select another.");
            }

            Execute(connection, transaction, @"UPDATE Courses SET Title=@title,Description=@description,
                CategoryID=@category,FoodCultureID=@foodCultureId,DifficultyLevel=@difficulty,Thumbnail=@thumbnail WHERE CourseID=@id",
                Parameter("@title", course["Title"]), Parameter("@description", course["Description"]),
                Parameter("@category", course["CategoryID"]), Parameter("@difficulty", course["DifficultyLevel"]),
                Parameter("@foodCultureId", foodCultureId), Parameter("@thumbnail", course["Thumbnail"]), Parameter("@id", courseId));

            ApplyRecipe(connection, transaction, courseId);
            ApplyLessons(connection, transaction, courseId);
            ApplyQuizzes(connection, transaction, courseId);
            ApplyActivities(connection, transaction, courseId);
        }

        private void ApplyRecipe(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            DataRow row = Recipes.Rows.Count == 0 ? null : Recipes.Rows[0];
            if (row == null)
            {
                Execute(connection, transaction, "DELETE FROM CourseRecipes WHERE CourseID=@course", Parameter("@course", courseId));
                return;
            }
            if (Convert.ToInt32(row["RecipeID"]) > 0)
                Execute(connection, transaction, @"UPDATE CourseRecipes SET Ingredients=@ingredients,CookingTime=@time,
                    ServingSize=@servings,RecipeImage=@image WHERE RecipeID=@id AND CourseID=@course",
                    Parameter("@ingredients", row["Ingredients"]), Parameter("@time", row["CookingTime"]),
                    Parameter("@servings", row["ServingSize"]), Parameter("@image", row["RecipeImage"]),
                    Parameter("@id", row["RecipeID"]), Parameter("@course", courseId));
            else
                Execute(connection, transaction, @"INSERT INTO CourseRecipes (CourseID,Ingredients,CookingTime,ServingSize,RecipeImage)
                    VALUES (@course,@ingredients,@time,@servings,@image)", Parameter("@course", courseId),
                    Parameter("@ingredients", row["Ingredients"]), Parameter("@time", row["CookingTime"]),
                    Parameter("@servings", row["ServingSize"]), Parameter("@image", row["RecipeImage"]));
        }

        private void ApplyLessons(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            var savedIds = new HashSet<int>();
            foreach (DataRow row in Lessons.Rows)
                if (row.RowState != DataRowState.Deleted && Convert.ToInt32(row["LessonID"]) > 0)
                    savedIds.Add(Convert.ToInt32(row["LessonID"]));

            var current = Read("SELECT LessonID FROM Lessons WHERE CourseID=@id", Parameter("@id", courseId));
            foreach (DataRow row in current.Rows)
            {
                int id = Convert.ToInt32(row["LessonID"]);
                if (!savedIds.Contains(id)) Execute(connection, transaction, "DELETE FROM Lessons WHERE LessonID=@id AND CourseID=@course", Parameter("@id", id), Parameter("@course", courseId));
            }

            int offset = Convert.ToInt32(Execute(connection, transaction,
                "SELECT ISNULL(MAX(LessonOrder),0)+1000 FROM Lessons WHERE CourseID=@id", Parameter("@id", courseId)));
            Execute(connection, transaction, "UPDATE Lessons SET LessonOrder=LessonOrder+@offset WHERE CourseID=@id", Parameter("@offset", offset), Parameter("@id", courseId));
            foreach (DataRow row in Lessons.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;
                int id = Convert.ToInt32(row["LessonID"]);
                if (id > 0)
                    Execute(connection, transaction, @"UPDATE Lessons SET Title=@title,Description=@description,Instructions=@instructions,
                        LessonImage=@image,LessonVideo=@video,LessonOrder=@order WHERE LessonID=@id AND CourseID=@course",
                        Parameter("@title", row["Title"]), Parameter("@description", row["Description"]), Parameter("@instructions", row["Instructions"]),
                        Parameter("@image", row["LessonImage"]), Parameter("@video", row["LessonVideo"]), Parameter("@order", row["LessonOrder"]),
                        Parameter("@id", id), Parameter("@course", courseId));
                else
                    Execute(connection, transaction, @"INSERT INTO Lessons (CourseID,Title,Description,Instructions,LessonImage,LessonVideo,LessonOrder)
                        VALUES (@course,@title,@description,@instructions,@image,@video,@order)", Parameter("@course", courseId),
                        Parameter("@title", row["Title"]), Parameter("@description", row["Description"]), Parameter("@instructions", row["Instructions"]),
                        Parameter("@image", row["LessonImage"]), Parameter("@video", row["LessonVideo"]), Parameter("@order", row["LessonOrder"]));
            }
        }

        private void ApplyQuizzes(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            var original = Read("SELECT QuizID FROM Quizzes WHERE CourseID=@id", Parameter("@id", courseId));
            var draftIds = new HashSet<int>();
            foreach (DataRow row in Quizzes.Rows)
                if (row.RowState != DataRowState.Deleted && Convert.ToInt32(row["QuizID"]) > 0)
                    draftIds.Add(Convert.ToInt32(row["QuizID"]));
            foreach (DataRow row in original.Rows)
            {
                int id = Convert.ToInt32(row["QuizID"]);
                if (!draftIds.Contains(id)) Execute(connection, transaction, "DELETE FROM Quizzes WHERE QuizID=@id AND CourseID=@course", Parameter("@id", id), Parameter("@course", courseId));
            }

            var quizIds = new Dictionary<int, int>();
            foreach (DataRow quiz in Quizzes.Rows)
            {
                if (quiz.RowState == DataRowState.Deleted) continue;
                int oldId = Convert.ToInt32(quiz["QuizID"]);
                int realId;
                if (oldId > 0)
                {
                    realId = oldId;
                    Execute(connection, transaction, "UPDATE Quizzes SET Title=@title,Description=@description,PassingScore=@score WHERE QuizID=@quiz AND CourseID=@course",
                        Parameter("@title", quiz["Title"]), Parameter("@description", quiz["Description"]), Parameter("@score", quiz["PassingScore"]),
                        Parameter("@quiz", realId), Parameter("@course", courseId));
                }
                else
                    realId = Convert.ToInt32(Execute(connection, transaction,
                        "INSERT INTO Quizzes (CourseID,Title,Description,PassingScore) OUTPUT INSERTED.QuizID VALUES (@course,@title,@description,@score)",
                        Parameter("@course", courseId), Parameter("@title", quiz["Title"]), Parameter("@description", quiz["Description"]), Parameter("@score", quiz["PassingScore"])));
                quizIds[oldId] = realId;
            }

            Execute(connection, transaction, @"UPDATE qq SET QuestionOrder=QuestionOrder+1000000
                FROM QuizQuestions qq JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE q.CourseID=@course", Parameter("@course", courseId));
            foreach (DataRow question in Questions.Rows)
            {
                if (question.RowState == DataRowState.Deleted) continue;
                int oldQuizId = Convert.ToInt32(question["QuizID"]);
                int realQuizId = quizIds.ContainsKey(oldQuizId) ? quizIds[oldQuizId] : oldQuizId;
                int oldQuestionId = Convert.ToInt32(question["QuestionID"]);
                int realQuestionId;
                if (oldQuestionId > 0)
                {
                    realQuestionId = oldQuestionId;
                    Execute(connection, transaction, "UPDATE QuizQuestions SET QuestionText=@text,QuestionOrder=@order WHERE QuestionID=@id AND QuizID=@quiz",
                        Parameter("@text", question["QuestionText"]), Parameter("@order", question["QuestionOrder"]), Parameter("@id", realQuestionId), Parameter("@quiz", realQuizId));
                }
                else
                    realQuestionId = Convert.ToInt32(Execute(connection, transaction,
                        "INSERT INTO QuizQuestions (QuizID,QuestionText,QuestionOrder) OUTPUT INSERTED.QuestionID VALUES (@quiz,@text,@order)",
                        Parameter("@quiz", realQuizId), Parameter("@text", question["QuestionText"]), Parameter("@order", question["QuestionOrder"])));
                var existingOptions = ReadTransaction(connection, transaction, "SELECT OptionID FROM QuizOptions WHERE QuestionID=@question", Parameter("@question", realQuestionId));
                var stagedOptionIds = new HashSet<int>();
                foreach (DataRow option in Options.Rows)
                {
                    if (option.RowState == DataRowState.Deleted || Convert.ToInt32(option["QuestionID"]) != Convert.ToInt32(question["QuestionID"])) continue;
                    int oldOptionId = Convert.ToInt32(option["OptionID"]);
                    if (oldOptionId > 0)
                    {
                        stagedOptionIds.Add(oldOptionId);
                        Execute(connection, transaction, "UPDATE QuizOptions SET OptionText=@text,IsCorrect=@correct WHERE OptionID=@id AND QuestionID=@question",
                            Parameter("@text", option["OptionText"]), Parameter("@correct", option["IsCorrect"]), Parameter("@id", oldOptionId), Parameter("@question", realQuestionId));
                    }
                    else Execute(connection, transaction, "INSERT INTO QuizOptions (QuestionID,OptionText,IsCorrect) VALUES (@question,@text,@correct)",
                        Parameter("@question", realQuestionId), Parameter("@text", option["OptionText"]), Parameter("@correct", option["IsCorrect"]));
                }
                foreach (DataRow oldOption in existingOptions.Rows)
                    if (!stagedOptionIds.Contains(Convert.ToInt32(oldOption["OptionID"])))
                        Execute(connection, transaction, "DELETE FROM QuizOptions WHERE OptionID=@id AND QuestionID=@question", Parameter("@id", oldOption["OptionID"]), Parameter("@question", realQuestionId));
            }

            var stagedQuestionIds = new HashSet<int>();
            foreach (DataRow question in Questions.Rows)
                if (question.RowState != DataRowState.Deleted && Convert.ToInt32(question["QuestionID"]) > 0)
                    stagedQuestionIds.Add(Convert.ToInt32(question["QuestionID"]));
            var oldQuestions = ReadTransaction(connection, transaction, "SELECT qq.QuestionID FROM QuizQuestions qq JOIN Quizzes q ON q.QuizID=qq.QuizID WHERE q.CourseID=@id", Parameter("@id", courseId));
            foreach (DataRow question in oldQuestions.Rows)
                if (!stagedQuestionIds.Contains(Convert.ToInt32(question["QuestionID"])))
                    Execute(connection, transaction, "DELETE FROM QuizQuestions WHERE QuestionID=@id", Parameter("@id", question["QuestionID"]));
        }

        private static DataTable ReadTransaction(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                using (var adapter = new SqlDataAdapter(command))
                {
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }

        private void ApplyActivities(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            var savedIds = new HashSet<int>();
            foreach (DataRow row in Activities.Rows)
                if (row.RowState != DataRowState.Deleted && Convert.ToInt32(row["ActivityID"]) > 0)
                    savedIds.Add(Convert.ToInt32(row["ActivityID"]));
            var current = Read("SELECT ActivityID FROM PracticalActivities WHERE CourseID=@id", Parameter("@id", courseId));
            foreach (DataRow row in current.Rows)
            {
                int id = Convert.ToInt32(row["ActivityID"]);
                if (!savedIds.Contains(id)) Execute(connection, transaction, "DELETE FROM PracticalActivities WHERE ActivityID=@id AND CourseID=@course", Parameter("@id", id), Parameter("@course", courseId));
            }
            foreach (DataRow row in Activities.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;
                if (Convert.ToInt32(row["ActivityID"]) > 0)
                    Execute(connection, transaction, "UPDATE PracticalActivities SET Title=@title,Instructions=@instructions WHERE ActivityID=@id AND CourseID=@course",
                        Parameter("@title", row["Title"]), Parameter("@instructions", row["Instructions"]), Parameter("@id", row["ActivityID"]), Parameter("@course", courseId));
                else
                    Execute(connection, transaction, "INSERT INTO PracticalActivities (CourseID,Title,Instructions) VALUES (@course,@title,@instructions)",
                        Parameter("@course", courseId), Parameter("@title", row["Title"]), Parameter("@instructions", row["Instructions"]));
            }
        }
    }
}
