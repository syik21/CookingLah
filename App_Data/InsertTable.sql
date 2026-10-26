/* ============================================================
   COOKING LAH DATABASE
   ASP.NET Web Forms + SQL Server
   ============================================================ */


/* ============================================================
   1. USERS
   ------------------------------------------------------------
   Roles:
   - Learner
   - Instructor
   - Admin

   Learners may be permanently deleted.
   Instructor/Admin should normally be deactivated instead.
   ============================================================ */

CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,

    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,

    Role NVARCHAR(20) NOT NULL,

    ProfileImage NVARCHAR(500) NULL,

    AccountStatus NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Users_AccountStatus
        DEFAULT 'Active',

    CreatedAt DATETIME NOT NULL
        CONSTRAINT DF_Users_CreatedAt
        DEFAULT GETDATE(),

    UpdatedAt DATETIME NULL,

    CONSTRAINT CK_Users_Role
        CHECK (
            Role IN (
                'Learner',
                'Instructor',
                'Admin'
            )
        ),

    CONSTRAINT CK_Users_AccountStatus
        CHECK (
            AccountStatus IN (
                'Active',
                'Inactive',
                'Suspended'
            )
        )
);


/* ============================================================
   2. CATEGORIES
   ============================================================ */

CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,

    CategoryName NVARCHAR(100) NOT NULL UNIQUE,

    Description NVARCHAR(500) NULL,

    CreatedAt DATETIME NOT NULL
        CONSTRAINT DF_Categories_CreatedAt
        DEFAULT GETDATE()
);


/* ============================================================
   3. COURSES
   ------------------------------------------------------------
   Instructor owns the course.

   DO NOT CASCADE from Instructor -> Course.
   Instructor should be deactivated instead of deleted.
   ============================================================ */

CREATE TABLE Courses
(
    CourseID INT IDENTITY(1,1) PRIMARY KEY,

    InstructorID INT NOT NULL,
    CategoryID INT NOT NULL,
    FoodCultureID INT NULL,

    Title NVARCHAR(150) NOT NULL,

    Description NVARCHAR(MAX) NULL,

    DifficultyLevel NVARCHAR(20) NOT NULL,

    Thumbnail NVARCHAR(500) NULL,

    RejectionReason NVARCHAR(500) NULL,

    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Courses_Status
        DEFAULT 'Draft',

    CreatedAt DATETIME NOT NULL
        CONSTRAINT DF_Courses_CreatedAt
        DEFAULT GETDATE(),

    UpdatedAt DATETIME NULL,

    CONSTRAINT FK_Courses_Instructor
        FOREIGN KEY (InstructorID)
        REFERENCES Users(UserID)
        ON DELETE NO ACTION,

    CONSTRAINT FK_Courses_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID)
        ON DELETE NO ACTION,

    CONSTRAINT CK_Courses_Difficulty
        CHECK (
            DifficultyLevel IN (
                'Beginner',
                'Intermediate',
                'Advanced'
            )
        ),

    CONSTRAINT CK_Courses_Status
        CHECK (
            Status IN (
                'Draft',
                'Pending',
                'Published',
                'Rejected'
            )
        )
);


/* ============================================================
   4. COURSE RECIPES
   ------------------------------------------------------------
   One Course has one main Recipe.

   Actual learning material is stored in LessonContent blocks.
   ============================================================ */

CREATE TABLE CourseRecipes
(
    RecipeID INT IDENTITY(1,1) PRIMARY KEY,

    CourseID INT NOT NULL UNIQUE,

    Ingredients NVARCHAR(MAX) NOT NULL,

    CookingTime INT NULL,

    ServingSize INT NULL,

    RecipeImage NVARCHAR(500) NULL,

    CONSTRAINT FK_CourseRecipes_Course
        FOREIGN KEY (CourseID)
        REFERENCES Courses(CourseID)
        ON DELETE CASCADE,

    CONSTRAINT CK_CourseRecipes_CookingTime
        CHECK (
            CookingTime IS NULL
            OR CookingTime > 0
        ),

    CONSTRAINT CK_CourseRecipes_ServingSize
        CHECK (
            ServingSize IS NULL
            OR ServingSize > 0
        )
);


/* ============================================================
   5. LESSONS
   ------------------------------------------------------------
   Lessons = step-by-step cooking learning process.

   Example:
   1. Prepare Fish
   2. Prepare Marinade
   3. Grill Fish
   ============================================================ */

CREATE TABLE Lessons
(
    LessonID INT IDENTITY(1,1) PRIMARY KEY,

    CourseID INT NOT NULL,

    Title NVARCHAR(150) NOT NULL,

    Description NVARCHAR(500) NULL,

    LessonOrder INT NOT NULL,

    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CreatedAt DATETIME NOT NULL
        CONSTRAINT DF_Lessons_CreatedAt
        DEFAULT GETDATE(),

    CONSTRAINT FK_Lessons_Course
        FOREIGN KEY (CourseID)
        REFERENCES Courses(CourseID)
        ON DELETE CASCADE,

    CONSTRAINT CK_Lessons_Order
        CHECK (LessonOrder > 0),

    CONSTRAINT UQ_Lessons_CourseOrder
        UNIQUE (
            CourseID,
            LessonOrder
        )
);


/* ============================================================
   6. QUIZZES
   ============================================================ */

    CREATE TABLE dbo.LessonContent
    (
        LessonContentID INT IDENTITY(1,1) PRIMARY KEY,
        LessonID INT NOT NULL REFERENCES dbo.Lessons(LessonID) ON DELETE CASCADE,
        ContentType NVARCHAR(10) NOT NULL CHECK (ContentType IN ('Article','Image','Video')),
        ContentTitle NVARCHAR(150) NULL,
        ContentText NVARCHAR(MAX) NULL,
        MediaPath NVARCHAR(500) NULL,
        Caption NVARCHAR(500) NULL,
        ContentOrder INT NOT NULL CHECK (ContentOrder > 0),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_LessonContent_Order UNIQUE (LessonID, ContentOrder)
    );


CREATE TABLE Quizzes
(
    QuizID INT IDENTITY(1,1) PRIMARY KEY,

    CourseID INT NOT NULL,

    Title NVARCHAR(150) NOT NULL,

    Description NVARCHAR(500) NULL,

    PassingScore INT NOT NULL
        CONSTRAINT DF_Quizzes_PassingScore
        DEFAULT 50,

    CreatedAt DATETIME NOT NULL
        CONSTRAINT DF_Quizzes_CreatedAt
        DEFAULT GETDATE(),

    CONSTRAINT FK_Quizzes_Course
        FOREIGN KEY (CourseID)
        REFERENCES Courses(CourseID)
        ON DELETE CASCADE,

    CONSTRAINT CK_Quizzes_PassingScore
        CHECK (
            PassingScore BETWEEN 0 AND 100
        )
);


/* ============================================================
   7. QUIZ QUESTIONS
   ============================================================ */

CREATE TABLE QuizQuestions
(
    QuestionID INT IDENTITY(1,1) PRIMARY KEY,

    QuizID INT NOT NULL,

    QuestionText NVARCHAR(MAX) NOT NULL,

    QuestionOrder INT NOT NULL,

    CONSTRAINT FK_QuizQuestions_Quiz
        FOREIGN KEY (QuizID)
        REFERENCES Quizzes(QuizID)
        ON DELETE CASCADE,

    CONSTRAINT CK_QuizQuestions_Order
        CHECK (QuestionOrder > 0),

    CONSTRAINT UQ_QuizQuestions_QuizOrder
        UNIQUE (
            QuizID,
            QuestionOrder
        )
);


/* ============================================================
   8. QUIZ OPTIONS
   ------------------------------------------------------------
   IsCorrect:
   0 = Incorrect
   1 = Correct
   ============================================================ */

CREATE TABLE QuizOptions
(
    OptionID INT IDENTITY(1,1) PRIMARY KEY,

    QuestionID INT NOT NULL,

    OptionText NVARCHAR(500) NOT NULL,

    IsCorrect BIT NOT NULL
        CONSTRAINT DF_QuizOptions_IsCorrect
        DEFAULT 0,

    CONSTRAINT FK_QuizOptions_Question
        FOREIGN KEY (QuestionID)
        REFERENCES QuizQuestions(QuestionID)
        ON DELETE CASCADE
);


/* ============================================================
   9. QUIZ ATTEMPTS
   ------------------------------------------------------------
   Learner-owned record.

   Delete Course/Quiz:
   -> attempt deleted.

   Delete Learner:
   -> learner's attempts deleted.
   ============================================================ */

CREATE TABLE QuizAttempts
(
    AttemptID INT IDENTITY(1,1) PRIMARY KEY,

    QuizID INT NOT NULL,

    LearnerID INT NOT NULL,

    Score INT NOT NULL,

    TotalQuestions INT NOT NULL,

    Passed BIT NOT NULL,

    AttemptDate DATETIME NOT NULL
        CONSTRAINT DF_QuizAttempts_AttemptDate
        DEFAULT GETDATE(),

    CONSTRAINT FK_QuizAttempts_Quiz
        FOREIGN KEY (QuizID)
        REFERENCES Quizzes(QuizID)
        ON DELETE CASCADE,

    CONSTRAINT FK_QuizAttempts_Learner
        FOREIGN KEY (LearnerID)
        REFERENCES Users(UserID)
        ON DELETE CASCADE,

    CONSTRAINT CK_QuizAttempts_Score
        CHECK (
            Score BETWEEN 0 AND 100
        ),

    CONSTRAINT CK_QuizAttempts_TotalQuestions
        CHECK (
            TotalQuestions > 0
        )
);


/* ============================================================
   10. COURSE PROGRESS
   ------------------------------------------------------------
   One learner has one progress record per course.

   Delete Learner:
   -> progress deleted.

   Delete Course:
   -> progress deleted.
   ============================================================ */

CREATE TABLE CourseProgress
(
    ProgressID INT IDENTITY(1,1) PRIMARY KEY,

    LearnerID INT NOT NULL,

    CourseID INT NOT NULL,

    ProgressPercentage INT NOT NULL
        CONSTRAINT DF_CourseProgress_Percentage
        DEFAULT 0,

    LastCompletedLessonOrder INT NOT NULL
        CONSTRAINT DF_CourseProgress_LastCompletedLessonOrder
        DEFAULT 0,

    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_CourseProgress_Status
        DEFAULT 'Not Started',

    StartedAt DATETIME NULL,

    CompletedAt DATETIME NULL,

    LastAccessedAt DATETIME NULL,

    CONSTRAINT FK_CourseProgress_Learner
        FOREIGN KEY (LearnerID)
        REFERENCES Users(UserID)
        ON DELETE CASCADE,

    CONSTRAINT FK_CourseProgress_Course
        FOREIGN KEY (CourseID)
        REFERENCES Courses(CourseID)
        ON DELETE CASCADE,

    CONSTRAINT UQ_CourseProgress_LearnerCourse
        UNIQUE (
            LearnerID,
            CourseID
        ),

    CONSTRAINT CK_CourseProgress_Percentage
        CHECK (
            ProgressPercentage
            BETWEEN 0 AND 100
        ),

    CONSTRAINT CK_CourseProgress_Status
        CHECK (
            Status IN (
                'Not Started',
                'In Progress',
                'Completed'
            )
        )
);


/* ============================================================
   11. PRACTICAL ACTIVITIES
   ============================================================ */

CREATE TABLE PracticalActivities
(
    ActivityID INT IDENTITY(1,1) PRIMARY KEY,

    CourseID INT NOT NULL,

    Title NVARCHAR(150) NOT NULL,

    Instructions NVARCHAR(MAX) NOT NULL,

    CreatedAt DATETIME NOT NULL
        CONSTRAINT DF_PracticalActivities_CreatedAt
        DEFAULT GETDATE(),

    CONSTRAINT FK_PracticalActivities_Course
        FOREIGN KEY (CourseID)
        REFERENCES Courses(CourseID)
        ON DELETE CASCADE
);


/* ============================================================
   12. ACTIVITY SUBMISSIONS
   ------------------------------------------------------------
   Learner submits image for a practical activity.

   Delete Learner:
   -> submission deleted.

   Delete Activity/Course:
   -> submission deleted.

   ReviewedBy does NOT cascade.
   ============================================================ */

CREATE TABLE ActivitySubmissions
(
    SubmissionID INT IDENTITY(1,1) PRIMARY KEY,

    ActivityID INT NOT NULL,

    LearnerID INT NOT NULL,

    ImagePath NVARCHAR(500) NOT NULL,

    Description NVARCHAR(1000) NULL,

    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_ActivitySubmissions_Status
        DEFAULT 'Submitted',

    InstructorFeedback NVARCHAR(MAX) NULL,

    ReviewedBy INT NULL,

    SubmittedAt DATETIME NOT NULL
        CONSTRAINT DF_ActivitySubmissions_SubmittedAt
        DEFAULT GETDATE(),

    ReviewedAt DATETIME NULL,

    CONSTRAINT FK_ActivitySubmissions_Activity
        FOREIGN KEY (ActivityID)
        REFERENCES PracticalActivities(ActivityID)
        ON DELETE CASCADE,

    CONSTRAINT FK_ActivitySubmissions_Learner
        FOREIGN KEY (LearnerID)
        REFERENCES Users(UserID)
        ON DELETE CASCADE,

    CONSTRAINT FK_ActivitySubmissions_Reviewer
        FOREIGN KEY (ReviewedBy)
        REFERENCES Users(UserID)
        ON DELETE NO ACTION,

    CONSTRAINT CK_ActivitySubmissions_Status
        CHECK (
            Status IN (
                'Submitted',
                'Reviewed'
            )
        )
);


/* ============================================================
   13. REVIEWS
   ------------------------------------------------------------
   One learner can review a course once.

   Delete Learner:
   -> review deleted.

   Delete Course:
   -> review deleted.
   ============================================================ */

CREATE TABLE Reviews
(
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,

    CourseID INT NOT NULL,

    LearnerID INT NOT NULL,

    Rating INT NOT NULL,

    Comment NVARCHAR(MAX) NULL,

    CreatedAt DATETIME NOT NULL
        CONSTRAINT DF_Reviews_CreatedAt
        DEFAULT GETDATE(),

    CONSTRAINT FK_Reviews_Course
        FOREIGN KEY (CourseID)
        REFERENCES Courses(CourseID)
        ON DELETE CASCADE,

    CONSTRAINT FK_Reviews_Learner
        FOREIGN KEY (LearnerID)
        REFERENCES Users(UserID)
        ON DELETE CASCADE,

    CONSTRAINT CK_Reviews_Rating
        CHECK (
            Rating BETWEEN 1 AND 5
        ),

    CONSTRAINT UQ_Reviews_LearnerCourse
        UNIQUE (
            CourseID,
            LearnerID
        )
);


/* ============================================================
   14. COURSE REPORTS
   ------------------------------------------------------------
   Users can report Courses.

   Delete Learner/Reporter:
   -> report deleted.

   Delete Course:
   -> report deleted.

   Admin reviewer should normally be deactivated rather than
   physically deleted.
   ============================================================ */

CREATE TABLE CourseReports
(
    ReportID INT IDENTITY(1,1) PRIMARY KEY,

    CourseID INT NOT NULL,

    ReportedBy INT NOT NULL,

    Reason NVARCHAR(1000) NOT NULL,

    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_CourseReports_Status
        DEFAULT 'Pending',

    ReviewedBy INT NULL,

    CreatedAt DATETIME NOT NULL
        CONSTRAINT DF_CourseReports_CreatedAt
        DEFAULT GETDATE(),

    ReviewedAt DATETIME NULL,

    CONSTRAINT FK_CourseReports_Course
        FOREIGN KEY (CourseID)
        REFERENCES Courses(CourseID)
        ON DELETE CASCADE,

    CONSTRAINT FK_CourseReports_Reporter
        FOREIGN KEY (ReportedBy)
        REFERENCES Users(UserID)
        ON DELETE CASCADE,

    CONSTRAINT FK_CourseReports_Reviewer
        FOREIGN KEY (ReviewedBy)
        REFERENCES Users(UserID)
        ON DELETE NO ACTION,

    CONSTRAINT CK_CourseReports_Status
        CHECK (
            Status IN (
                'Pending',
                'Reviewed',
                'Removed',
                'Dismissed'
            )
        )
);


/* ============================================================
   16. FOOD CULTURES
   ------------------------------------------------------------
   Country is stored as text on the FoodCulture record.
   ============================================================ */

CREATE TABLE FoodCultures
(
    FoodCultureID INT IDENTITY(1,1) PRIMARY KEY,

    DishName NVARCHAR(150) NOT NULL,

    Country NVARCHAR(150) NOT NULL,

    Description NVARCHAR(MAX) NULL,

    Ingredients NVARCHAR(MAX) NULL,

    CookingMethod NVARCHAR(MAX) NULL,

    DishImage NVARCHAR(500) NULL
);

ALTER TABLE Courses
ADD CONSTRAINT FK_Courses_FoodCulture
    FOREIGN KEY (FoodCultureID)
    REFERENCES FoodCultures(FoodCultureID)
    ON DELETE NO ACTION;
