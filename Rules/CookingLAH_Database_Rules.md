# Cooking LAH Database Rules

## 1. General Database Rules

- Database system: **SQL Server**
- All primary keys use `INT IDENTITY(1,1)`.
- Foreign keys should be used to maintain relationships between tables.
- Use `NVARCHAR` for text fields.
- Use `DATETIME` for timestamps.
- Use `BIT` for true/false values.
- Images and videos should store the **file path**, not the actual binary file.
- `GETDATE()` should be used as the default value for creation dates where appropriate.
- Use constraints such as `CHECK`, `UNIQUE`, and `NOT NULL` to prevent invalid data.

---

# 2. User Rules

## User Roles

The `Users` table supports three registered user roles:

- `Learner`
- `Instructor`
- `Admin`

Guest users are **not stored** in the database because they do not have an account.

## Account Status

Allowed account statuses:

- `Active`
- `Inactive`
- `Suspended`

## Learner Deletion

Learners may be **permanently deleted**.

When a learner is deleted, learner-owned records should also be deleted automatically using `ON DELETE CASCADE`.

The following learner data should be deleted:

- Course progress
- Quiz attempts
- Practical activity submissions
- Reviews
- Course reports

Example:

```sql
DELETE FROM Users
WHERE UserID = @UserID
  AND Role = 'Learner';
```

The backend must verify that the user is a Learner before performing a permanent delete.

## Instructor Deletion

Instructor accounts should **not be permanently deleted**.

Instead, deactivate the account:

```sql
UPDATE Users
SET
    AccountStatus = 'Inactive',
    UpdatedAt = GETDATE()
WHERE UserID = @UserID
  AND Role = 'Instructor';
```

Reason:

- Instructor courses must remain available.
- Learner progress must remain available.
- Quiz attempts and reviews linked to the instructor's courses must remain available.

## Admin Deletion

Admin accounts should also normally be **deactivated instead of permanently deleted**.

```sql
UPDATE Users
SET
    AccountStatus = 'Inactive',
    UpdatedAt = GETDATE()
WHERE UserID = @UserID
  AND Role = 'Admin';
```

---

# 3. Course Rules

A Course is the main learning content created by an Instructor.

A course can contain:

- One main recipe
- Multiple lessons
- Multiple quizzes
- Practical activities
- Learner progress
- Reviews
- Course reports

A course belongs to:

- One Instructor
- One Category

## Course Status

Allowed course statuses:

- `Draft`
- `Pending`
- `Published`
- `Rejected`

Only the Course requires approval.

Individual lessons and quizzes do not need separate approval statuses unless this is added later.

---

# 4. Course Deletion Rules

A Course may be permanently deleted.

When a Course is deleted, all data that only exists because of that Course should also be removed.

Use `ON DELETE CASCADE` for:

- Course recipe
- Lessons
- Quizzes
- Quiz questions
- Quiz options
- Quiz attempts
- Course progress
- Practical activities
- Activity submissions
- Reviews
- Course reports

Example:

```sql
DELETE FROM Courses
WHERE CourseID = @CourseID;
```

Expected cascade:

```text
Course
├── CourseRecipe
├── Lessons
├── Quizzes
│   ├── QuizQuestions
│   │   └── QuizOptions
│   └── QuizAttempts
├── CourseProgress
├── PracticalActivities
│   └── ActivitySubmissions
├── Reviews
└── CourseReports
```

---

# 5. Recipe Rules

Each Course contains **one main Recipe**.

The `CourseRecipes.CourseID` field should be `UNIQUE`.

Recipe stores general recipe information such as:

- Recipe name
- Description
- Ingredients
- Cooking time
- Serving size
- Recipe image

Cooking steps are **not stored inside the Recipe table**.

Cooking steps are stored as Lessons.

---

# 6. Lesson Rules

Lessons represent the **step-by-step cooking learning process**.

Example:

```text
Course: How to Cook Ikan Bakar

Lesson 1: Prepare the Fish
Lesson 2: Prepare the Marinade
Lesson 3: Marinate the Fish
Lesson 4: Grill the Fish
```

A Lesson may contain:

- Title
- Description
- Instructions
- Image
- Video
- Lesson order

`LessonOrder` must be unique within the same Course.

Example:

```text
CourseID = 5
LessonOrder = 1
LessonOrder = 2
LessonOrder = 3
```

Two lessons inside the same Course should not have the same `LessonOrder`.

When a Course is deleted, its Lessons are deleted automatically.

---

# 7. Quiz Rules

A Course may contain multiple Quizzes.

Each Quiz may contain multiple Questions.

Each Question may contain multiple Options.

Relationship:

```text
Course
└── Quiz
    └── QuizQuestion
        └── QuizOption
```

## Quiz Options

`IsCorrect` uses SQL Server `BIT`.

```text
0 = Incorrect
1 = Correct
```

## Quiz Attempts

Quiz attempts belong to both:

- A Quiz
- A Learner

Delete behaviour:

- Delete Quiz -> delete Quiz Attempts
- Delete Learner -> delete that Learner's Quiz Attempts

Scores should be between `0` and `100`.

---

# 8. Course Progress Rules

A Learner can have only **one progress record per Course**.

Use a unique constraint on:

```text
LearnerID + CourseID
```

Allowed progress status:

- `Not Started`
- `In Progress`
- `Completed`

`ProgressPercentage` must be between:

```text
0 - 100
```

Delete behaviour:

- Delete Learner -> delete Learner's progress
- Delete Course -> delete progress for that Course

---

# 9. Practical Activity Rules

Practical Activities belong to a Course.

A Practical Activity may contain:

- Title
- Instructions

Learners can submit cooking images for the activity.

An Activity Submission contains:

- Learner
- Image path
- Description
- Submission status
- Instructor feedback
- Reviewer
- Submission date
- Review date

Allowed submission statuses:

- `Submitted`
- `Reviewed`

Delete behaviour:

```text
Delete Course
    ↓
Delete Practical Activity
    ↓
Delete Activity Submissions
```

Deleting a Learner also deletes that Learner's submissions.

---

# 10. Review Rules

Learners can review Courses.

A Review contains:

- Course
- Learner
- Rating
- Comment
- Created date

Rating must be between:

```text
1 - 5
```

A Learner can review the same Course only once.

Use a unique constraint on:

```text
CourseID + LearnerID
```

Delete behaviour:

- Delete Learner -> delete Learner's reviews
- Delete Course -> delete Course reviews

---

# 11. Course Report Rules

Users can report **Courses only**.

A Course Report contains:

- Course
- Reporter
- Reason
- Status
- Admin reviewer
- Created date
- Reviewed date

Allowed report statuses:

- `Pending`
- `Reviewed`
- `Removed`
- `Dismissed`

Delete behaviour:

- Delete Learner/reporting user -> delete their reports
- Delete Course -> delete reports for that Course

Admin reviewer references should not cascade-delete reports.

---

# 12. Category Rules

Categories are managed by Admin.

A Course belongs to one Category.

Categories should **not use `ON DELETE CASCADE`**.

If a Category is still being used by Courses, SQL Server should block deletion.

Before deleting a Category, Admin must:

1. Move the related Courses to another Category, or
2. Delete the related Courses first.

This prevents accidental deletion of many Courses.

---

# 13. Food Culture Map Rules

The Food Culture Map uses:

- `Countries`
- `FoodCultures`

Relationship:

```text
Country
└── FoodCultures
```

A Country may have multiple Food Culture records.

Deleting a Country should delete its Food Culture records using:

```sql
ON DELETE CASCADE
```

Country map coordinates may be stored using:

- `MapPositionX`
- `MapPositionY`

These values are optional if the marker positions are handled directly in CSS or JavaScript.

---

# 14. Foreign Key Delete Strategy

Use the following general rule:

## `ON DELETE CASCADE`

Use when the child data has no meaning without its parent.

Examples:

```text
Course -> Lessons
Course -> Recipe
Course -> Quizzes
Quiz -> Questions
Question -> Options
Course -> Practical Activities
Activity -> Submissions
Country -> Food Cultures
```

## `ON DELETE NO ACTION`

Use when deleting the referenced record should not automatically destroy important data.

Examples:

```text
Instructor -> Courses
Category -> Courses
Admin Reviewer -> Course Reports
Instructor Reviewer -> Activity Submissions
```

## User Role Specific Behaviour

```text
Learner
    -> Hard delete allowed
    -> Learner-owned data cascades

Instructor
    -> Soft delete / deactivate
    -> Courses remain

Admin
    -> Soft delete / deactivate
    -> Historical records remain
```

---

# 15. File Storage Rules

Do not store large image/video files directly inside SQL Server for this project.

Store their file paths instead.

Examples:

```text
~/F_Assets/Courses/ikan-bakar.jpg
~/Uploads/Activities/submission-001.jpg
~/Uploads/Lessons/grill-fish.mp4
```

Database columns such as:

- `Thumbnail`
- `RecipeImage`
- `LessonImage`
- `LessonVideo`
- `ImagePath`
- `FlagImage`
- `DishImage`

should store these paths.

---

# 16. Password Rules

Do not store passwords as plain text.

Use:

```text
PasswordHash
```

instead of:

```text
Password
```

The application should hash the password before saving it to the database.

Login should compare the entered password against the stored password hash.

---

# 17. Current Core Tables

The current Cooking LAH database contains:

```text
1. Users
2. Categories
3. Courses
4. CourseRecipes
5. Lessons
6. Quizzes
7. QuizQuestions
8. QuizOptions
9. QuizAttempts
10. CourseProgress
11. PracticalActivities
12. ActivitySubmissions
13. Reviews
14. CourseReports
15. Countries
16. FoodCultures
```

---

# 18. Features Not Included Yet

The following features are currently considered optional/additional and are **not part of the core database yet**:

- Live Cooking Session
- AI Cooking Assistant

These features can be added later without changing the main Course database structure.

---

# 19. Core Relationship Summary

```text
Users
│
├── Instructor -> Courses
│
├── Learner -> CourseProgress
├── Learner -> QuizAttempts
├── Learner -> ActivitySubmissions
├── Learner -> Reviews
└── Learner -> CourseReports


Categories
└── Courses


Courses
├── CourseRecipes
├── Lessons
├── Quizzes
│   ├── QuizQuestions
│   │   └── QuizOptions
│   └── QuizAttempts
├── CourseProgress
├── PracticalActivities
│   └── ActivitySubmissions
├── Reviews
└── CourseReports


Countries
└── FoodCultures
```

---

# 20. Important Development Rule

Before modifying or deleting important records from ASP.NET `.aspx.cs`, always verify:

1. The logged-in user is authenticated.
2. The logged-in user has the correct role.
3. The requested record exists.
4. The user has permission to modify that record.
5. The database operation uses parameterized SQL queries.

For example, only an Instructor who owns a Course or an authorized Admin should be allowed to modify that Course.

The database protects relationships, while the C# backend should protect application permissions.

## Current signup schema (verified against the local database)

Registration writes `Users.Username` (50), `Email` (100), `Password` (255), `Role` (20), and `Country` (100). The existing column named `Password` stores a BCrypt hash, never plaintext; `PasswordHash` above describes the preferred naming, not the current physical column. Country is the selected region's English display name, validated against the server-generated country list. Email remains unique.

The form's Chef choice maps to `Instructor`. Only chefs have a record in the existing `ChefProfession` table: `ChefID` references `Users.UserID`, and `Profession` is NVARCHAR(100). User and profession creation run in one transaction. Learners do not insert any profession record. No `Users.Profession` column or migration is required. Passwords are at least eight characters and limited to 72 UTF-8 bytes to avoid BCrypt truncation.
