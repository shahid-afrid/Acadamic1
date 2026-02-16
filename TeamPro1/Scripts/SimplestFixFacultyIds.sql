-- =====================================================
-- SIMPLEST FIX: Manually assign FacultyIds
-- Copy and paste this into SQL Server Management Studio
-- =====================================================

USE TeamFormationDb;  -- ?? CHANGE THIS if your database has a different name
GO

-- For CSE(DS) / Computer Science faculty, assign 3201, 3202, 3203, etc.
DECLARE @Counter INT = 1;

UPDATE Faculties
SET FacultyId = '32' + RIGHT('0' + CAST(@Counter AS NVARCHAR(2)), 2),
    @Counter = @Counter + 1
WHERE (Department = 'CSE(DS)' OR Department = 'Computer Science')
  AND (FacultyId IS NULL OR FacultyId = '')
ORDER BY Id;

-- Verify
SELECT Id, FacultyId, FullName, Email, Department
FROM Faculties
ORDER BY FacultyId;

PRINT '? Done! All faculty now have FacultyId assigned.';
GO
