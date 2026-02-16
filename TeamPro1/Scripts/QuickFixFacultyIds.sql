-- =====================================================
-- QUICK FIX: Populate FacultyId for ALL existing faculty
-- Run this script in SQL Server Management Studio
-- =====================================================

USE TeamFormationDb;  -- ?? CHANGE THIS TO YOUR DATABASE NAME
GO

-- Step 1: Generate FacultyIds based on department
UPDATE F
SET F.FacultyId = 
    CASE 
        -- Computer Science / CSE(DS) / Data Science -> 32XX
        WHEN F.Department IN ('Computer Science', 'CSE(DS)', 'Data Science') THEN 
            '32' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY 
                CASE WHEN F.Department IN ('Computer Science', 'CSE(DS)', 'Data Science') THEN 1 ELSE 0 END 
                ORDER BY F.Id) AS NVARCHAR(2)), 2)
        
        -- Information Technology / IT -> 31XX
        WHEN F.Department IN ('Information Technology', 'IT') THEN 
            '31' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY 
                CASE WHEN F.Department IN ('Information Technology', 'IT') THEN 1 ELSE 0 END 
                ORDER BY F.Id) AS NVARCHAR(2)), 2)
        
        -- ECE -> 33XX
        WHEN F.Department IN ('Electronics and Communication Engineering', 'ECE') THEN 
            '33' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY 
                CASE WHEN F.Department IN ('Electronics and Communication Engineering', 'ECE') THEN 1 ELSE 0 END 
                ORDER BY F.Id) AS NVARCHAR(2)), 2)
        
        -- EEE -> 34XX
        WHEN F.Department IN ('Electrical and Electronics Engineering', 'EEE') THEN 
            '34' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY 
                CASE WHEN F.Department IN ('Electrical and Electronics Engineering', 'EEE') THEN 1 ELSE 0 END 
                ORDER BY F.Id) AS NVARCHAR(2)), 2)
        
        -- Mechanical -> 35XX
        WHEN F.Department IN ('Mechanical Engineering', 'MECH') THEN 
            '35' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY 
                CASE WHEN F.Department IN ('Mechanical Engineering', 'MECH') THEN 1 ELSE 0 END 
                ORDER BY F.Id) AS NVARCHAR(2)), 2)
        
        -- Civil -> 36XX
        WHEN F.Department IN ('Civil Engineering', 'CIVIL') THEN 
            '36' + RIGHT('0' + CAST(ROW_NUMBER() OVER (PARTITION BY 
                CASE WHEN F.Department IN ('Civil Engineering', 'CIVIL') THEN 1 ELSE 0 END 
                ORDER BY F.Id) AS NVARCHAR(2)), 2)
        
        -- Default for unknown departments -> 32XX
        ELSE '32' + RIGHT('0' + CAST(ROW_NUMBER() OVER (ORDER BY F.Id) AS NVARCHAR(2)), 2)
    END
FROM Faculties F
WHERE F.FacultyId IS NULL OR F.FacultyId = '';

-- Step 2: Verify the update
SELECT 
    Id,
    FacultyId,
    FullName,
    Email,
    Department,
    CreatedAt
FROM Faculties
ORDER BY Department, FacultyId;

-- Step 3: Summary
SELECT 
    Department,
    COUNT(*) AS TotalFaculty,
    MIN(FacultyId) AS FirstId,
    MAX(FacultyId) AS LastId
FROM Faculties
GROUP BY Department
ORDER BY Department;

PRINT '? FacultyId population complete!';
GO
