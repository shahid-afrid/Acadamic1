-- =====================================================
-- Faculty ID Verification Script
-- Run this AFTER AddFacultyIdColumn.sql to verify setup
-- =====================================================

USE TeamPro1;
GO

PRINT '';
PRINT '=========================================';
PRINT '   FACULTY ID VERIFICATION REPORT';
PRINT '=========================================';
PRINT '';

-- 1. Check if FacultyId column exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Faculties') AND name = 'FacultyId')
BEGIN
    PRINT '? FacultyId column exists';
END
ELSE
BEGIN
    PRINT '? ERROR: FacultyId column NOT found!';
    PRINT '  ? Run AddFacultyIdColumn.sql first';
    RETURN;
END
GO

-- 2. Check for NULL FacultyIds
DECLARE @NullCount INT;
SELECT @NullCount = COUNT(*) FROM Faculties WHERE FacultyId IS NULL;

IF @NullCount = 0
BEGIN
    PRINT '? All faculty have FacultyId assigned';
END
ELSE
BEGIN
    PRINT '? WARNING: ' + CAST(@NullCount AS VARCHAR) + ' faculty have NULL FacultyId';
    SELECT Id, FullName, Department, FacultyId FROM Faculties WHERE FacultyId IS NULL;
END
GO

-- 3. Check for duplicate FacultyIds
DECLARE @DuplicateCount INT;
SELECT @DuplicateCount = COUNT(*) FROM (
    SELECT FacultyId FROM Faculties GROUP BY FacultyId HAVING COUNT(*) > 1
) AS Duplicates;

IF @DuplicateCount = 0
BEGIN
    PRINT '? No duplicate FacultyIds found';
END
ELSE
BEGIN
    PRINT '? ERROR: Duplicate FacultyIds found!';
    SELECT FacultyId, COUNT(*) as Count 
    FROM Faculties 
    GROUP BY FacultyId 
    HAVING COUNT(*) > 1;
END
GO

-- 4. Check if unique index exists
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Faculties') AND name = 'IX_Faculties_FacultyId')
BEGIN
    PRINT '? Unique index on FacultyId exists';
END
ELSE
BEGIN
    PRINT '? WARNING: Unique index on FacultyId NOT found';
    PRINT '  ? This may cause duplicate IDs in future';
END
GO

-- 5. Display all Faculty IDs grouped by department
PRINT '';
PRINT '=========================================';
PRINT '   FACULTY IDs BY DEPARTMENT';
PRINT '=========================================';
PRINT '';

SELECT 
    Department,
    FacultyId,
    FullName,
    Email
FROM Faculties
ORDER BY Department, FacultyId;
GO

-- 6. Summary statistics
PRINT '';
PRINT '=========================================';
PRINT '   SUMMARY STATISTICS';
PRINT '=========================================';
PRINT '';

SELECT 
    Department,
    COUNT(*) as TotalFaculty,
    MIN(FacultyId) as FirstId,
    MAX(FacultyId) as LastId,
    CASE 
        WHEN MIN(FacultyId) = MAX(FacultyId) THEN 'Single Faculty'
        WHEN COUNT(*) = 1 THEN 'Single Faculty'
        ELSE 'Range: ' + MIN(FacultyId) + ' to ' + MAX(FacultyId)
    END as IdRange
FROM Faculties
WHERE FacultyId IS NOT NULL
GROUP BY Department
ORDER BY Department;
GO

-- 7. Expected vs Actual Department Codes
PRINT '';
PRINT '=========================================';
PRINT '   DEPARTMENT CODE VALIDATION';
PRINT '=========================================';
PRINT '';

SELECT 
    Department,
    LEFT(MIN(FacultyId), 2) as ActualCode,
    CASE Department
        WHEN 'CSE(DS)' THEN '32'
        WHEN 'CSE(AI&ML)' THEN '33'
        WHEN 'Computer Science' THEN '31'
        WHEN 'CSE' THEN '31'
        WHEN 'ECE' THEN '41'
        WHEN 'EEE' THEN '42'
        WHEN 'Mechanical' THEN '51'
        WHEN 'Civil' THEN '61'
        WHEN 'IT' THEN '34'
        ELSE '99'
    END as ExpectedCode,
    CASE 
        WHEN LEFT(MIN(FacultyId), 2) = 
            CASE Department
                WHEN 'CSE(DS)' THEN '32'
                WHEN 'CSE(AI&ML)' THEN '33'
                WHEN 'Computer Science' THEN '31'
                WHEN 'CSE' THEN '31'
                WHEN 'ECE' THEN '41'
                WHEN 'EEE' THEN '42'
                WHEN 'Mechanical' THEN '51'
                WHEN 'Civil' THEN '61'
                WHEN 'IT' THEN '34'
                ELSE '99'
            END
        THEN '? Correct'
        ELSE '? MISMATCH!'
    END as Status
FROM Faculties
WHERE FacultyId IS NOT NULL
GROUP BY Department
ORDER BY Department;
GO

-- 8. Check for gaps in sequential numbering (within each department)
PRINT '';
PRINT '=========================================';
PRINT '   SEQUENTIAL NUMBERING CHECK';
PRINT '=========================================';
PRINT '';

WITH FacultySequence AS (
    SELECT 
        Department,
        FacultyId,
        CAST(RIGHT(FacultyId, 2) AS INT) as SequenceNum,
        ROW_NUMBER() OVER (PARTITION BY Department ORDER BY FacultyId) as ExpectedSeq
    FROM Faculties
    WHERE FacultyId IS NOT NULL
)
SELECT 
    Department,
    FacultyId,
    SequenceNum as ActualSeq,
    ExpectedSeq,
    CASE 
        WHEN SequenceNum = ExpectedSeq THEN '? Sequential'
        ELSE '? Gap detected (not critical)'
    END as Status
FROM FacultySequence
ORDER BY Department, FacultyId;
GO

-- Final status
PRINT '';
PRINT '=========================================';
PRINT '   FINAL STATUS';
PRINT '=========================================';
PRINT '';

DECLARE @TotalFaculty INT, @WithId INT, @WithoutId INT;

SELECT @TotalFaculty = COUNT(*) FROM Faculties;
SELECT @WithId = COUNT(*) FROM Faculties WHERE FacultyId IS NOT NULL;
SELECT @WithoutId = COUNT(*) FROM Faculties WHERE FacultyId IS NULL;

PRINT 'Total Faculty: ' + CAST(@TotalFaculty AS VARCHAR);
PRINT 'With FacultyId: ' + CAST(@WithId AS VARCHAR);
PRINT 'Without FacultyId: ' + CAST(@WithoutId AS VARCHAR);
PRINT '';

IF @TotalFaculty = @WithId AND @WithId > 0
BEGIN
    PRINT '??? ALL CHECKS PASSED! ???';
    PRINT 'Faculty ID system is properly configured.';
    PRINT 'You can now run the application.';
END
ELSE IF @WithoutId > 0
BEGIN
    PRINT '? WARNING: Some faculty do not have FacultyId';
    PRINT '? Re-run AddFacultyIdColumn.sql to fix this.';
END
ELSE
BEGIN
    PRINT '? ERROR: No faculty found in database';
    PRINT '? Add faculty through the admin interface.';
END

PRINT '';
PRINT '=========================================';
GO
