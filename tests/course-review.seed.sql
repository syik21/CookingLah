-- Disposable test database only: model the user's existing RejectionReason column.
INSERT INTO Users (Username,Email,PasswordHash,FullName,Role) VALUES ('chef1','chef1@example.invalid','test','Test Chef','Instructor'),('Other Chef','chef2@example.invalid','test','Other Chef','Instructor'),('admin','admin@example.invalid','test','Admin','Admin'),('learner','learner@example.invalid','test','Learner','Learner');
INSERT INTO Categories (CategoryName) VALUES ('Malaysian Cuisine');
INSERT INTO Courses (InstructorID,CategoryID,Title,Description,DifficultyLevel,Status,RejectionReason) VALUES
(1,1,'Ikan Bakar','Learn a dish','Beginner','Pending',NULL),(1,1,'Approve Me','Test approval','Beginner','Pending','Old reason'),(1,1,'Race Course','Concurrency','Beginner','Pending',NULL),(2,1,'Other chef course','Other content','Beginner','Published',NULL),(1,1,'Private Draft','Hidden','Beginner','Draft',NULL);
DECLARE @i INT=1; WHILE @i<=7 BEGIN
INSERT INTO Courses (InstructorID,CategoryID,Title,DifficultyLevel,Status) VALUES (1,1,CONCAT('Pending Course ',@i),'Beginner','Pending');
SET @i=@i+1; END;
INSERT INTO CourseRecipes (CourseID,Ingredients,CookingTime,ServingSize) VALUES (1,'[{"name":"Fish","amount":"1 whole"}]',45,4);
INSERT INTO PracticalActivities (CourseID,Title,Instructions) VALUES (1,'Prepare Ikan Bakar','Cook it');
INSERT INTO Quizzes (CourseID,Title,PassingScore) VALUES (1,'Knowledge Quiz',60);
SET @i=1; WHILE @i<=7 BEGIN
INSERT INTO Lessons (CourseID,Title,Instructions,LessonOrder) VALUES (1,CONCAT('Lesson ',@i),'Cook carefully',@i);
INSERT INTO QuizQuestions (QuizID,QuestionText,QuestionOrder) VALUES (1,CONCAT('Question ',@i),@i);
DECLARE @question INT=SCOPE_IDENTITY();
INSERT INTO QuizOptions (QuestionID,OptionText,IsCorrect) VALUES (@question,'Medium heat',1),(@question,'High heat',0);
SET @i=@i+1; END;
