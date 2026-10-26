INSERT INTO Users (Username,Email,PasswordHash,FullName,Role) VALUES ('chef1','chef1@example.invalid','test-only','Test Chef','Instructor'),('chef2','chef2@example.invalid','test-only','Other Chef','Instructor'),('learner','learner@example.invalid','test-only','Learner','Learner');
INSERT INTO Categories (CategoryName) VALUES ('Malaysian Cuisine');
INSERT INTO Courses (InstructorID,CategoryID,Title,Description,DifficultyLevel,Status) VALUES (1,1,'Owned Course','Learn a dish','Beginner','Published'),(2,1,'Private Other Course','Other chef content','Beginner','Draft'),(1,1,'Empty Draft','Start here','Beginner','Draft');
INSERT INTO CourseRecipes (CourseID,Ingredients) VALUES (1,'[{"name":"Fish","amount":"1 whole"}]');
INSERT INTO PracticalActivities (CourseID,Title,Instructions) VALUES (1,'Original Activity','Cook it');
INSERT INTO Quizzes (CourseID,Title,PassingScore) VALUES (1,'Original Quiz',60);
DECLARE @i INT=1; WHILE @i<=7 BEGIN
INSERT INTO Lessons (CourseID,Title,Instructions,LessonOrder) VALUES (1,CONCAT('Lesson ',@i),'Cook carefully',@i);
INSERT INTO QuizQuestions (QuizID,QuestionText,QuestionOrder) VALUES (1,CONCAT('Question ',@i),@i);
DECLARE @question INT=SCOPE_IDENTITY();
INSERT INTO QuizOptions (QuestionID,OptionText,IsCorrect) VALUES (@question,'Yes',1),(@question,'No',0);
SET @i=@i+1; END;
INSERT INTO QuizAttempts (QuizID,LearnerID,Score,TotalQuestions,Passed) VALUES (1,3,80,7,1);
INSERT INTO ActivitySubmissions (ActivityID,LearnerID,ImagePath) VALUES (1,3,'test.jpg');
INSERT INTO CourseProgress (LearnerID,CourseID) VALUES (3,1);
INSERT INTO Reviews (CourseID,LearnerID,Rating) VALUES (1,3,5);
INSERT INTO CourseReports (CourseID,ReportedBy,Reason) VALUES (1,3,'Test');
