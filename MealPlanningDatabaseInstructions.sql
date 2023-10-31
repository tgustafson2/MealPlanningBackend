USE master;
GO

IF NOT EXISTS(
    SELECT name
    FROM sys.databases
    WHERE name = N'MealPlanningDB'
)
CREATE DATABASE MealPlanningDB;
GO


USE MealPlanningDB;
GO



IF OBJECT_ID(N'MealPlanning.UserAuth', N'U') IS NULL
CREATE TABLE MealPlanning.UserAuth(
    Email NVARCHAR(50) PRIMARY KEY,
    PasswordHash VARBINARY(MAX),
    PasswordSalt VARBINARY(MAX)
);
GO

IF OBJECT_ID(N'MealPlanning.UserInfo', N'U') IS NULL
CREATE TABLE MealPlanning.UserInfo(
    Id INT IDENTITY(1,1),
    Email NVARCHAR(50),
    FirstName VARCHAR(20),
    LastName VARCHAR(20),
);
GO



IF OBJECT_ID(N'MealPlanning.MealChoice', N'U') IS NULL
CREATE TABLE MealPlanning.MealChoice(
    MealDate DATETIME,
    UserId INT,
    RecipeID INT,
    Id int IDENTITY(1,1)
);
GO

IF OBJECT_ID(N'MealPlanning.Recipe', N'U') IS NULL
CREATE TABLE MealPlanning.Recipe(
    UserId INT,
    Title NVARCHAR(100),
    Ingredients NVARCHAR(MAX),
    Directions NVARCHAR(MAX),
    Id INT IDENTITY(1,1)
);
GO


CREATE OR ALTER PROCEDURE MealPlanning.spMealChoiceGet
    @UserId INT,
    @Start DATETIME,
    @End DATETIME
AS
BEGIN
    SELECT MC.Id, MC.MealDate, MC.RecipeID, MC.UserId
    FROM MealPlanning.MealChoice MC
    WHERE UserId = @UserId
    AND MealDate BETWEEN @Start AND @End
END;
GO


CREATE OR ALTER PROCEDURE MealPlanning.spRecipeGet
    @UserId INT,
    @TitleSearch NVARCHAR(100) = NULL,
    @IngredientsSearch NVARCHAR(MAX) = NULL,
    @DirectionsSearch NVARCHAR(MAX) = NULL,
    @Id INT = NULL
AS
BEGIN
    SELECT R.UserId,
        R.Title,
        R.Ingredients,
        R.Directions,
        R.Id
    FROM MealPlanning.Recipe AS R
        WHERE R.UserId = @UserId
        AND R.Id = ISNULL(@Id, R.Id)
        AND (@TitleSearch IS NULL 
            OR R.Title LIKE '%' + @TitleSearch + '%')
        AND (@IngredientsSearch IS NULL
            OR R.Ingredients LIKE '%' + @IngredientsSearch + '%')
        AND (@DirectionsSearch IS NULL
            OR R.Directions LIKE '%' + @DirectionsSearch + '%')
END;
GO


CREATE OR ALTER PROCEDURE MealPlanning.spMealChoiceDelete
    @Id INT,
    @UserId INT
AS
BEGIN
    DELETE FROM MealPlanning.MealChoice
    WHERE Id = @Id AND UserId=@UserId
END;
GO


CREATE OR ALTER PROCEDURE MealPlanning.spMealChoiceUpsert
    @MealDate DATETIME,
    @UserId INT,
    @RecipeID INT,
    @Id INT = NULL
AS
BEGIN
    IF NOT EXISTS(SELECT * FROM MealPlanning.MealChoice WHERE Id = @Id)
        BEGIN
            INSERT INTO MealPlanning.MealChoice(
                MealDate,
                UserId,
                RecipeID
            ) VALUES (
                @MealDate,
                @UserId,
                @RecipeID
            )
        END
    ELSE
        BEGIN
            UPDATE MealPlanning.MealChoice
                SET MealDate = @MealDate,
                    UserId = @UserId,
                    RecipeID = @RecipeID
                WHERE Id = @Id
        END
END;
GO


CREATE OR ALTER PROCEDURE MealPlanning.spRecipeDelete
    @Id INT,
    @UserId INT
AS
BEGIN
    DELETE FROM MealPlanning.Recipe
    WHERE Id = @Id AND UserId = @UserId

    DELETE FROM MealPlanning.MealChoice
    WHERE RecipeID = @Id AND UserId = @UserId

END;
GO



CREATE OR ALTER PROCEDURE MealPlanning.spRecipeUpsert
    @UserId INT,
    @Title NVARCHAR(50),
    @Ingredients NVARCHAR(MAX),
    @Directions NVARCHAR(MAX),
    @Id INT = NULL
AS
BEGIN
    IF NOT EXISTS(SELECT * FROM MealPlanning.Recipe WHERE Id = @Id)
        BEGIN
            INSERT INTO MealPlanning.Recipe(
                UserId,
                Title,
                Ingredients,
                Directions
            ) VALUES (
                @UserId,
                @Title,
                @Ingredients,
                @Directions
            )
        END
    ELSE
        BEGIN
            UPDATE MealPlanning.Recipe
                SET Title = @Title,
                    Ingredients = @Ingredients,
                    Directions = @Directions
                WHERE Id = @Id AND UserId = @UserId
        END
END;
GO




CREATE OR ALTER PROCEDURE MealPlanning.spUserInfoUpsert
    @Email NVARCHAR(50),
    @FirstName VARCHAR(20),
    @LastName VARCHAR(20),
    @UserId INT = NULL
AS
BEGIN
    IF NOT EXISTS(SELECT * FROM MealPlanning.UserInfo WHERE Id = @UserId)
        BEGIN
        IF NOT EXISTS(SELECT * FROM MealPlanning.UserInfo WHERE Email = @Email)
            BEGIN
                DECLARE @OutputUserId INT
                INSERT INTO MealPlanning.UserInfo(
                    Email,
                    FirstName,
                    LastName
                ) VALUES (
                    @Email,
                    @FirstName,
                    @LastName
                )

                SET @OutputUserId = @@IDENTITY
            END
        END
    ELSE
        BEGIN
            UPDATE MealPlanning.UserInfo
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email
                WHERE Id = @UserId
        END
END;
GO

CREATE OR ALTER PROCEDURE MealPlanning.spUser_Delete
    @UserId INT
AS
BEGIN
    DECLARE @Email NVARCHAR(50);

    SELECT @Email = UserInfo.Email
    FROM MealPlanning.UserInfo
    WHERE UserInfo.Id = @UserId;

    DELETE FROM MealPlanning.MealChoice
    WHERE MealChoice.UserId = @UserId;

    DELETE FROM MealPlanning.Recipe
    WHERE Recipe.UserId = @UserId;

    DELETE FROM MealPlanning.UserInfo
    WHERE UserInfo.Id = @UserId;

    DELETE FROM MealPlanning.UserAuth
    WHERE UserAuth.Email = @Email;
END;
GO



CREATE OR ALTER PROCEDURE MealPlanning.spLoginConfirmGet
    @Email NVARCHAR(50)
AS
BEGIN
    SELECT auth.PasswordHash, auth.PasswordSalt
    FROM MealPlanning.UserAuth as auth
    WHERE auth.Email = @Email
END;
GO

CREATE OR ALTER PROCEDURE MealPlanning.spReg_Upsert
    @Email NVARCHAR(50),
    @PassHash VARBINARY(MAX),
    @PassSalt VARBINARY(MAX)
AS
BEGIN
    IF NOT EXISTS (SELECT * FROM MealPlanning.UserAuth WHERE Email = @Email)
        BEGIN
            INSERT INTO MealPlanning.UserAuth(
                Email,
                PasswordHash,
                PasswordSalt
            ) VALUES (
                @Email,
                @PassHash,
                @PassSalt
            )
        END
    ELSE
        BEGIN
            UPDATE MealPlanning.UserAuth
            SET PasswordHash = @PassHash,
                PasswordSalt = @PassSalt
            WHERE Email = @Email
        END
END;
GO

