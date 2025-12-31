/*
 LoanApp Database Script (SQL Server)
 - Re-runnable / idempotent
 - Creates required tables for this application

 How to use:
1) Option A (recommended): set @DatabaseName below and run the whole script.
2) Option B: comment out the DB creation section and run against an existing database.

 Notes:
 - Uses UTC timestamps via SYSUTCDATETIME().

 - Author: Atul Kharecha
 - Date:31-DEC-2025
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

/*
----------------------------------
Database creation / selection
----------------------------------
- This section is optional and re-runnable.
- It creates the database if it does not exist, then switches execution context to it.
*/
DECLARE @DatabaseName sysname = N'LoanApp';

IF DB_ID(@DatabaseName) IS NULL
	BEGIN
		DECLARE @CreateDbSql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@DatabaseName) + N';';
		EXEC(@CreateDbSql);
END;

DECLARE @UseDbSql nvarchar(max) = N'USE ' + QUOTENAME(@DatabaseName) + N';';
EXEC(@UseDbSql);

BEGIN TRY
BEGIN TRAN;

 /*
 -----------------------------
 LoanType
 -----------------------------
 */
	IF OBJECT_ID(N'dbo.LoanType', N'U') IS NULL
	BEGIN
		CREATE TABLE dbo.LoanType
		 (
			 LoanTypeId INT IDENTITY(1,1) NOT NULL,
			 LoanTypeName NVARCHAR(150) NOT NULL,
			 InterestRatePct DECIMAL(5,2) NULL,
			 IsActive BIT NOT NULL CONSTRAINT DF_LoanType_IsActive DEFAULT (1),
			 CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_LoanType_CreatedAt DEFAULT (SYSUTCDATETIME()),

			CONSTRAINT PK_LoanType PRIMARY KEY CLUSTERED (LoanTypeId)
		 );
	END
	ELSE
	BEGIN
		 /* Ensure defaults exist if table was created manually */
		 IF OBJECT_ID(N'dbo.DF_LoanType_IsActive', N'D') IS NULL
		 ALTER TABLE dbo.LoanType ADD CONSTRAINT DF_LoanType_IsActive DEFAULT (1) FOR IsActive;

		 IF OBJECT_ID(N'dbo.DF_LoanType_CreatedAt', N'D') IS NULL
		 ALTER TABLE dbo.LoanType ADD CONSTRAINT DF_LoanType_CreatedAt DEFAULT (SYSUTCDATETIME()) FOR CreatedAt;
	END;

	 /*
	 -----------------------------
	 LoanEligibilityRule
	 -----------------------------
	 */
	IF OBJECT_ID(N'dbo.LoanEligibilityRule', N'U') IS NULL
		BEGIN
			CREATE TABLE dbo.LoanEligibilityRule
			 (
			 RuleId INT IDENTITY(1,1) NOT NULL,
			 LoanTypeId INT NOT NULL,
			 MinAge INT NOT NULL,
			 MaxAge INT NOT NULL,
			 MinMonthlyIncome DECIMAL(18,2) NOT NULL,
			 MinCreditScore INT NULL,
			 MaxEmiToIncomePct DECIMAL(5,2) NULL,
			 IsActive BIT NOT NULL CONSTRAINT DF_LoanEligibilityRule_IsActive DEFAULT (1),
			 CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_LoanEligibilityRule_CreatedAt DEFAULT (SYSUTCDATETIME()),

			 CONSTRAINT PK_LoanEligibilityRule PRIMARY KEY CLUSTERED (RuleId)
			 );
		END
	ELSE
		BEGIN
			 /* Ensure defaults exist if table was created manually */
			 IF OBJECT_ID(N'dbo.DF_LoanEligibilityRule_IsActive', N'D') IS NULL
			 ALTER TABLE dbo.LoanEligibilityRule ADD CONSTRAINT DF_LoanEligibilityRule_IsActive DEFAULT (1) FOR IsActive;

			 IF OBJECT_ID(N'dbo.DF_LoanEligibilityRule_CreatedAt', N'D') IS NULL
			 ALTER TABLE dbo.LoanEligibilityRule ADD CONSTRAINT DF_LoanEligibilityRule_CreatedAt DEFAULT (SYSUTCDATETIME()) FOR CreatedAt;
		END;

		/* Foreign key (added separately so the script is re-runnable even if objects already exist) */
		IF OBJECT_ID(N'dbo.FK_LoanEligibilityRule_LoanType', N'F') IS NULL
		BEGIN
			ALTER TABLE dbo.LoanEligibilityRule
			WITH CHECK
			ADD CONSTRAINT FK_LoanEligibilityRule_LoanType
			FOREIGN KEY (LoanTypeId)
			REFERENCES dbo.LoanType (LoanTypeId);

			ALTER TABLE dbo.LoanEligibilityRule
			CHECK CONSTRAINT FK_LoanEligibilityRule_LoanType;
		END;

		/*
		Helpful index for join/filter patterns
		*/
		IF NOT EXISTS (
			SELECT 1
			FROM sys.indexes
			WHERE name = N'IX_LoanEligibilityRule_LoanTypeId'
			AND object_id = OBJECT_ID(N'dbo.LoanEligibilityRule', N'U')
		)
		BEGIN
			CREATE INDEX IX_LoanEligibilityRule_LoanTypeId
			ON dbo.LoanEligibilityRule (LoanTypeId);
		END;

	COMMIT;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT >0
		ROLLBACK;

		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();

		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;