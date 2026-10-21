-- Run against the database configured as CookingLahDB before deploying this signup update.
IF COL_LENGTH('dbo.Users', 'Profession') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Profession NVARCHAR(100) NULL;
END;
