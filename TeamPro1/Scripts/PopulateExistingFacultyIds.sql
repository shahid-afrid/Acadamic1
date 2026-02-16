-- =====================================================
-- Script to Populate FacultyId for Existing Faculty Records
-- Run this ONCE to fix existing faculty records that have empty FacultyId
-- =====================================================

USE TeamFormationDb;  -- Change to your database name if different
GO

PRINT '===== POPULATING FACULTY IDs FOR EXISTING RECORDS =====';
PRINT '';

-- Check current state
PRINT 'Current faculty records WITHOUT FacultyId:';
SELECT 
    Id,
    FullName,
    Email,
    Department,
    FacultyId,
    CASE 
        WHEN FacultyId IS NULL OR FacultyId = '' THEN 'NEEDS FacultyId'
        ELSE 'Has FacultyId'
    END AS Status
FROM Faculties
WHERE FacultyId IS NULL OR FacultyId = '';

PRINT '';
PRINT '===== GENERATING FACULTY IDs =====';

-- Temporary table to store department codes
DECLARE @DeptCodeMapping TABLE (
    Department NVARCHAR(100),
    Code NVARCHAR(4)
);

-- Map departments to codes (based on your FacultyIdGenerator logic)
INSERT INTO @DeptCodeMapping VALUES 
    ('Computer Science', '32'),
    ('CSE(DS)', '32'),
    ('Data Science', '32'),
    ('Information Technology', '31'),
    ('IT', '31'),
    ('Electronics and Communication Engineering', '33'),
    ('ECE', '33'),
    ('Electrical and Electronics Engineering', '34'),
    ('EEE', '34'),
    ('Mechanical Engineering', '35'),
    ('MECH', '35'),
    ('Civil Engineering', '36'),
    ('CIVIL', '36');

-- Cursor to iterate through faculties without FacultyId
DECLARE @Id INT;
DECLARE @Department NVARCHAR(100);
DECLARE @DeptCode NVARCHAR(4);
DECLARE @NextNumber INT;
DECLARE @NewFacultyId NVARCHAR(10);

DECLARE faculty_cursor CURSOR FOR
SELECT Id, Department
FROM Faculties
WHERE FacultyId IS NULL OR FacultyId = ''
ORDER BY Department, Id;

OPEN faculty_cursor;
FETCH NEXT FROM faculty_cursor INTO @Id, @Department;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Get department code
    SELECT @DeptCode = Code 
    FROM @DeptCodeMapping 
    WHERE Department = @Department;
    
    -- If department not found, use default '32'
    IF @DeptCode IS NULL
        SET @DeptCode = '32';
    
    -- Find the next available number for this department
    -- Look at existing FacultyIds starting with this department code
    SELECT @NextNumber = ISNULL(MAX(CAST(SUBSTRING(FacultyId, 3, 10) AS INT)), 0) + 1
    FROM Faculties
    WHERE Department = @Department 
        AND FacultyId IS NOT NULL 
        AND FacultyId != ''
        AND LEN(FacultyId) >= 4
        AND SUBSTRING(FacultyId, 1, 2) = @DeptCode;
    
    -- If no existing FacultyIds for this department, start from 01
    IF @NextNumber IS NULL OR @NextNumber = 1
        SET @NextNumber = 1;
    
    -- Generate FacultyId (format: DeptCode + 2-digit number, e.g., 3201, 3202)
    SET @NewFacultyId = @DeptCode + RIGHT('0' + CAST(@NextNumber AS NVARCHAR(2)), 2);
    
    -- Check if this FacultyId already exists (prevent duplicates)
    WHILE EXISTS (SELECT 1 FROM Faculties WHERE FacultyId = @NewFacultyId)
    BEGIN
        SET @NextNumber = @NextNumber + 1;
        SET @NewFacultyId = @DeptCode + RIGHT('0' + CAST(@NextNumber AS NVARCHAR(2)), 2);
    END
    
    -- Update the faculty record
    UPDATE Faculties
    SET FacultyId = @NewFacultyId
    WHERE Id = @Id;
    
    PRINT 'Updated Faculty Id=' + CAST(@Id AS NVARCHAR(10)) + 
          ', Department=' + @Department + 
          ', New FacultyId=' + @NewFacultyId;
    
    FETCH NEXT FROM faculty_cursor INTO @Id, @Department;
END

CLOSE faculty_cursor;
DEALLOCATE faculty_cursor;

PRINT '';
PRINT '===== VERIFICATION =====';

-- Show all faculty records with their FacultyIds
SELECT 
    Id,
    FacultyId,
    FullName,
    Email,
    Department,
    CASE 
        WHEN FacultyId IS NULL OR FacultyId = '' THEN '? MISSING'
        ELSE '? ASSIGNED'
    END AS Status
FROM Faculties
ORDER BY Department, FacultyId;

PRINT '';

-- Count records by status
SELECT 
    CASE 
        WHEN FacultyId IS NULL OR FacultyId = '' THEN 'Missing FacultyId'
        ELSE 'Has FacultyId'
    END AS Status,
    COUNT(*) AS Count
FROM Faculties
GROUP BY 
    CASE 
        WHEN FacultyId IS NULL OR FacultyId = '' THEN 'Missing FacultyId'
        ELSE 'Has FacultyId'
    END;

PRINT '';
PRINT '===== COMPLETE =====';
PRINT 'All existing faculty records should now have FacultyId assigned.';
PRINT 'New faculty added through the application will automatically get FacultyId.';

GO
