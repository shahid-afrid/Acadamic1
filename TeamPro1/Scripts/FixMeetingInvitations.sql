-- SQL Script to Fix Meeting Invitations with NULL Faculty Data
-- Run this in SQL Server Management Studio
-- Database: TeamPro1

USE TeamPro1;
GO

-- ===== STEP 1: CHECK CURRENT MEETING INVITATIONS =====
PRINT '===== CHECKING MEETING INVITATIONS =====';
SELECT 
    MI.Id,
    MI.TeamId,
    MI.FacultyId,
    MI.Title,
    MI.Description,
    MI.MeetingDateTime,
    MI.Location,
    MI.Status,
    F.FullName AS FacultyName,
    F.Department AS FacultyDepartment
FROM MeetingInvitations MI
LEFT JOIN Faculties F ON MI.FacultyId = F.Id
ORDER BY MI.Id;

PRINT '';
PRINT '===== CHECKING FOR INVITATIONS WITH NULL/MISSING FACULTY =====';
SELECT 
    MI.Id AS InvitationId,
    MI.TeamId,
    MI.FacultyId,
    MI.Title,
    CASE 
        WHEN MI.FacultyId IS NULL THEN 'FacultyId is NULL'
        WHEN F.Id IS NULL THEN 'Faculty record does not exist'
        ELSE 'Faculty OK'
    END AS IssueType
FROM MeetingInvitations MI
LEFT JOIN Faculties F ON MI.FacultyId = F.Id
WHERE F.Id IS NULL;

-- ===== STEP 2: FIX OPTIONS =====
PRINT '';
PRINT '===== FIX OPTIONS =====';
PRINT 'Option 1: Delete all invalid meeting invitations with missing faculty';
PRINT 'Option 2: Assign a default faculty to invitations with NULL FacultyId';
PRINT 'Option 3: Manually fix each invitation';
PRINT '';

-- ===== OPTION 1: DELETE INVALID INVITATIONS =====
PRINT '===== OPTION 1: DELETE INVALID INVITATIONS =====';
DECLARE @DeleteCount INT;

SELECT @DeleteCount = COUNT(*)
FROM MeetingInvitations MI
LEFT JOIN Faculties F ON MI.FacultyId = F.Id
WHERE F.Id IS NULL;

PRINT 'Found ' + CAST(@DeleteCount AS NVARCHAR(10)) + ' invalid invitation(s)';

IF @DeleteCount > 0
BEGIN
    PRINT 'Deleting invalid invitations...';
    
    DELETE FROM MeetingInvitations
    WHERE Id IN (
        SELECT MI.Id
        FROM MeetingInvitations MI
        LEFT JOIN Faculties F ON MI.FacultyId = F.Id
        WHERE F.Id IS NULL
    );
    
    PRINT 'Deleted ' + CAST(@DeleteCount AS NVARCHAR(10)) + ' invalid invitation(s)!';
END
ELSE
BEGIN
    PRINT 'No invalid invitations to delete.';
END

-- ===== STEP 3: VERIFY FIX =====
PRINT '';
PRINT '===== VERIFICATION AFTER FIX =====';
SELECT 
    MI.Id,
    MI.TeamId,
    MI.FacultyId,
    MI.Title,
    MI.MeetingDateTime,
    F.FullName AS FacultyName,
    CASE 
        WHEN F.Id IS NULL THEN 'STILL BROKEN'
        ELSE 'FIXED'
    END AS Status
FROM MeetingInvitations MI
LEFT JOIN Faculties F ON MI.FacultyId = F.Id;

-- ===== STEP 4: CHECK AVAILABLE FACULTIES =====
PRINT '';
PRINT '===== AVAILABLE FACULTIES FOR FUTURE INVITATIONS =====';
SELECT 
    Id AS FacultyId,
    FullName,
    Email,
    Department
FROM Faculties
ORDER BY Department, FullName;

PRINT '';
PRINT '===== FIX COMPLETE =====';
PRINT 'If you still see "undefined" in the UI, restart your application (Stop + Start in Visual Studio)';
PRINT 'Make sure Faculty is logged in when sending meeting invitations.';
GO
