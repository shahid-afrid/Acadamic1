-- =====================================================
-- Add FacultyId Column to Faculties Table
-- This script adds unique Faculty IDs based on department
-- Format: DeptCode + Sequential Number (e.g., 3201, 3202)
-- =====================================================

USE TeamPro1;
GO

-- Step 1: Add FacultyId column (nullable initially)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Faculties') AND name = 'FacultyId')
BEGIN
    ALTER TABLE Faculties
    ADD FacultyId NVARCHAR(10) NULL;
    PRINT '? FacultyId column added';
END
ELSE
BEGIN
    PRINT '! FacultyId column already exists';
END
GO

-- Step 2: Generate FacultyIds for existing faculty based on department
-- Department mapping matches Admin structure:
-- CSE(DS) = 32, CSE(AI&ML) = 33, Computer Science = 31, etc.

UPDATE Faculties
SET FacultyId = CASE Department
    WHEN 'CSE(DS)' THEN '32' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY Department ORDER BY Id) AS VARCHAR), 2)
    WHEN 'CSE(AI&ML)' THEN '33' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY Department ORDER BY Id) AS VARCHAR), 2)
    WHEN 'Computer Science' THEN '31' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY Department ORDER BY Id) AS VARCHAR), 2)
    WHEN 'CSE' THEN '31' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY Department ORDER BY Id) AS VARCHAR), 2)
    WHEN 'ECE' THEN '41' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY Department ORDER BY Id) AS VARCHAR), 2)
    WHEN 'EEE' THEN '42' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY Department ORDER BY Id) AS VARCHAR), 2)
    WHEN 'Mechanical' THEN '51' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY Department ORDER BY Id) AS VARCHAR), 2)
    WHEN 'Civil' THEN '61' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY Department ORDER BY Id) AS VARCHAR), 2)
    WHEN 'IT' THEN '34' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY Department ORDER BY Id) AS VARCHAR), 2)
    ELSE '99' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY Department ORDER BY Id) AS VARCHAR), 2)
END
WHERE FacultyId IS NULL;
GO

PRINT '? FacultyId values generated for existing faculty';
GO

-- Step 3: Make FacultyId NOT NULL
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Faculties') AND name = 'FacultyId' AND is_nullable = 1)
BEGIN
    ALTER TABLE Faculties
    ALTER COLUMN FacultyId NVARCHAR(10) NOT NULL;
    PRINT '? FacultyId column set to NOT NULL';
END
GO

-- Step 4: Add unique constraint on FacultyId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Faculties') AND name = 'IX_Faculties_FacultyId')
BEGIN
    CREATE UNIQUE INDEX IX_Faculties_FacultyId ON Faculties(FacultyId);
    PRINT '? Unique index created on FacultyId';
END
ELSE
BEGIN
    PRINT '! Unique index already exists on FacultyId';
END
GO

-- Step 5: Verify the changes
PRINT '';
PRINT '========================================';
PRINT 'Faculty ID Assignments:';
PRINT '========================================';

SELECT 
    FacultyId,
    FullName,
    Department,
    Email,
    Id as InternalId
FROM Faculties
ORDER BY FacultyId;
GO

PRINT '';
PRINT '========================================';
PRINT 'Department Summary:';
PRINT '========================================';

SELECT 
    Department,
    COUNT(*) as FacultyCount,
    MIN(FacultyId) as FirstId,
    MAX(FacultyId) as LastId
FROM Faculties
GROUP BY Department
ORDER BY Department;
GO

PRINT '';
PRINT '??? FacultyId column setup complete! ???';
PRINT '';
GO
