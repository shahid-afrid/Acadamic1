-- SQL Script to View Faculties Table WITHOUT showing the internal Id column
-- This shows only FacultyId (not the internal database Id)
-- Run this in SQL Server Management Studio or Azure Data Studio

USE TeamPro1;
GO

-- ===== VIEW FACULTIES - SHOWING ONLY FACULTYID (NOT INTERNAL ID) =====
PRINT '===== FACULTY LIST (FacultyId shown, internal Id hidden) =====';
PRINT '';

SELECT 
    FacultyId AS 'Faculty ID',      -- This is the human-readable ID (3201, 3202, etc.)
    FullName AS 'Full Name',
    Email,
    Department,
    CreatedAt AS 'Joined Date'
FROM Faculties
ORDER BY FacultyId;

PRINT '';
PRINT '===== NOTES =====';
PRINT '? FacultyId column shown (e.g., 3201, 3202) - This is what users should see';
PRINT '? Id column hidden (internal database ID) - This is for system use only';
PRINT '';
PRINT 'If you want to see the internal Id for debugging:';
PRINT 'SELECT Id, FacultyId, FullName, Email FROM Faculties;';
GO
