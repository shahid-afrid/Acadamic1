-- ================================================
-- QUICK VERIFICATION SCRIPT
-- Run this to verify everything is ready
-- ================================================

USE TeamPro1
GO

-- 1. Check if Notifications table exists
PRINT '========================================='
PRINT '1. Checking if Notifications table exists...'
PRINT '========================================='
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    PRINT '? SUCCESS: Notifications table exists!'
    PRINT ''
    
    -- Show table structure
    PRINT 'Table Structure:'
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Notifications'
    ORDER BY ORDINAL_POSITION
    
    PRINT ''
END
ELSE
BEGIN
    PRINT '? ERROR: Notifications table does NOT exist!'
    PRINT 'Please run: TeamPro1\Scripts\AddNotificationsTable.sql'
    PRINT ''
END
GO

-- 2. Check if Students table exists (required)
PRINT '========================================='
PRINT '2. Checking if Students table exists...'
PRINT '========================================='
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
    PRINT '? SUCCESS: Students table exists!'
    
    -- Count students
    DECLARE @StudentCount INT
    SELECT @StudentCount = COUNT(*) FROM Students
    PRINT 'Total Students: ' + CAST(@StudentCount AS VARCHAR(10))
    
    IF @StudentCount >= 2
    BEGIN
        PRINT '? At least 2 students exist (good for testing)'
        PRINT ''
        
        -- Show first 2 students
        PRINT 'First 2 students:'
        SELECT TOP 2 
            Id,
            FullName,
            Email,
            RegdNumber,
            Year,
            Department
        FROM Students
        ORDER BY Id
        PRINT ''
    END
    ELSE
    BEGIN
        PRINT '?? WARNING: Only ' + CAST(@StudentCount AS VARCHAR(10)) + ' student(s) found'
        PRINT 'You need at least 2 students to test the notification feature'
        PRINT ''
    END
END
ELSE
BEGIN
    PRINT '? ERROR: Students table does NOT exist!'
    PRINT ''
END
GO

-- 3. Check current notifications
PRINT '========================================='
PRINT '3. Checking existing notifications...'
PRINT '========================================='
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    DECLARE @NotificationCount INT
    SELECT @NotificationCount = COUNT(*) FROM Notifications
    
    IF @NotificationCount > 0
    BEGIN
        PRINT 'Existing notifications: ' + CAST(@NotificationCount AS VARCHAR(10))
        PRINT ''
        
        -- Show all notifications
        SELECT 
            n.Id,
            s.FullName as StudentName,
            s.RegdNumber,
            n.Message,
            n.Type,
            CASE WHEN n.IsRead = 1 THEN 'Read' ELSE 'Unread' END as Status,
            n.CreatedAt
        FROM Notifications n
        INNER JOIN Students s ON n.StudentId = s.Id
        ORDER BY n.CreatedAt DESC
        PRINT ''
    END
    ELSE
    BEGIN
        PRINT '? No notifications yet (expected for new setup)'
        PRINT ''
    END
END
GO

-- 4. Check TeamRequests table
PRINT '========================================='
PRINT '4. Checking TeamRequests table...'
PRINT '========================================='
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TeamRequests')
BEGIN
    PRINT '? SUCCESS: TeamRequests table exists!'
    
    -- Count pending requests
    DECLARE @PendingCount INT
    SELECT @PendingCount = COUNT(*) FROM TeamRequests WHERE Status = 'Pending'
    PRINT 'Pending team requests: ' + CAST(@PendingCount AS VARCHAR(10))
    
    IF @PendingCount > 0
    BEGIN
        PRINT ''
        PRINT 'Pending Requests:'
        SELECT 
            tr.Id,
            s1.FullName as Sender,
            s2.FullName as Receiver,
            tr.Status,
            tr.CreatedAt
        FROM TeamRequests tr
        INNER JOIN Students s1 ON tr.SenderId = s1.Id
        INNER JOIN Students s2 ON tr.ReceiverId = s2.Id
        WHERE tr.Status = 'Pending'
        ORDER BY tr.CreatedAt DESC
    END
    PRINT ''
END
ELSE
BEGIN
    PRINT '? ERROR: TeamRequests table does NOT exist!'
    PRINT ''
END
GO

-- 5. Check Teams table
PRINT '========================================='
PRINT '5. Checking Teams table...'
PRINT '========================================='
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Teams')
BEGIN
    PRINT '? SUCCESS: Teams table exists!'
    
    DECLARE @TeamCount INT
    SELECT @TeamCount = COUNT(*) FROM Teams
    PRINT 'Total teams: ' + CAST(@TeamCount AS VARCHAR(10))
    PRINT ''
END
ELSE
BEGIN
    PRINT '? ERROR: Teams table does NOT exist!'
    PRINT ''
END
GO

-- 6. Check TeamFormationSchedules
PRINT '========================================='
PRINT '6. Checking Team Formation Schedule...'
PRINT '========================================='
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TeamFormationSchedules')
BEGIN
    PRINT '? SUCCESS: TeamFormationSchedules table exists!'
    PRINT ''
    
    -- Show open schedules
    IF EXISTS (SELECT 1 FROM TeamFormationSchedules WHERE IsOpen = 1)
    BEGIN
        PRINT 'OPEN schedules (students CAN form teams):'
        SELECT 
            Department,
            Year,
            CASE WHEN IsOpen = 1 THEN 'OPEN' ELSE 'CLOSED' END as Status,
            OpenedAt,
            LastUpdated
        FROM TeamFormationSchedules
        WHERE IsOpen = 1
        ORDER BY Department, Year
        PRINT ''
    END
    ELSE
    BEGIN
        PRINT '?? WARNING: No schedules are currently OPEN'
        PRINT 'Students cannot form teams until admin opens the schedule'
        PRINT ''
    END
    
    -- Show all schedules
    PRINT 'All schedules:'
    SELECT 
        Department,
        Year,
        CASE WHEN IsOpen = 1 THEN 'OPEN ?' ELSE 'CLOSED ?' END as Status,
        OpenedAt,
        ClosedAt,
        LastUpdated
    FROM TeamFormationSchedules
    ORDER BY Department, Year
    PRINT ''
END
ELSE
BEGIN
    PRINT '? ERROR: TeamFormationSchedules table does NOT exist!'
    PRINT ''
END
GO

-- 7. Final Summary
PRINT '========================================='
PRINT '7. FINAL VERIFICATION SUMMARY'
PRINT '========================================='
GO

DECLARE @AllGood BIT = 1

-- Check Notifications table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    PRINT '? Notifications table missing'
    SET @AllGood = 0
END
ELSE
    PRINT '? Notifications table exists'

-- Check Students table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
    PRINT '? Students table missing'
    SET @AllGood = 0
END
ELSE
    PRINT '? Students table exists'

-- Check TeamRequests table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TeamRequests')
BEGIN
    PRINT '? TeamRequests table missing'
    SET @AllGood = 0
END
ELSE
    PRINT '? TeamRequests table exists'

-- Check Teams table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Teams')
BEGIN
    PRINT '? Teams table missing'
    SET @AllGood = 0
END
ELSE
    PRINT '? Teams table exists'

-- Check TeamFormationSchedules table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TeamFormationSchedules')
BEGIN
    PRINT '? TeamFormationSchedules table missing'
    SET @AllGood = 0
END
ELSE
    PRINT '? TeamFormationSchedules table exists'

PRINT ''
PRINT '========================================='
IF @AllGood = 1
BEGIN
    PRINT '?? ALL CHECKS PASSED! ??'
    PRINT 'Your database is ready for the notification feature!'
    PRINT ''
    PRINT 'Next steps:'
    PRINT '1. Make sure application builds (dotnet build)'
    PRINT '2. Run the application (F5 in Visual Studio)'
    PRINT '3. Test the notification feature:'
    PRINT '   - Login as Student A'
    PRINT '   - Send request to Student B'
    PRINT '   - Login as Student B'
    PRINT '   - Reject the request'
    PRINT '   - Login as Student A'
    PRINT '   - Go to Pool of Students'
    PRINT '   - POP-UP SHOULD APPEAR! ??'
END
ELSE
BEGIN
    PRINT '?? SOME CHECKS FAILED ??'
    PRINT 'Please fix the issues above before testing'
END
PRINT '========================================='
GO

-- 8. Test Query: Manually create a test notification
PRINT ''
PRINT '========================================='
PRINT '8. OPTIONAL: Create Test Notification'
PRINT '========================================='
PRINT 'Uncomment the code below to create a test notification'
PRINT ''
GO

/*
-- Uncomment this block to create a test notification for the first student
IF EXISTS (SELECT 1 FROM Students)
BEGIN
    DECLARE @TestStudentId INT
    SELECT TOP 1 @TestStudentId = Id FROM Students ORDER BY Id
    
    -- Delete any existing test notifications for this student
    DELETE FROM Notifications WHERE StudentId = @TestStudentId AND Message LIKE '%Test notification%'
    
    -- Create a test notification
    INSERT INTO Notifications (StudentId, Message, Type, IsRead, CreatedAt)
    VALUES (
        @TestStudentId,
        'Test notification - Another student has rejected your team request. This is a test message.',
        'danger',
        0,
        GETDATE()
    )
    
    PRINT '? Test notification created for Student ID: ' + CAST(@TestStudentId AS VARCHAR(10))
    PRINT ''
    PRINT 'To see the notification:'
    PRINT '1. Login as this student'
    PRINT '2. Go to Pool of Students page'
    PRINT '3. Pop-up should appear after 500ms'
    PRINT ''
    
    -- Show the created notification
    SELECT 
        n.Id,
        s.FullName as StudentName,
        s.Email,
        n.Message,
        n.Type,
        'Unread' as Status,
        n.CreatedAt
    FROM Notifications n
    INNER JOIN Students s ON n.StudentId = s.Id
    WHERE n.Id = SCOPE_IDENTITY()
END
ELSE
BEGIN
    PRINT '? No students found to create test notification'
END
GO
*/

PRINT ''
PRINT '========================================='
PRINT 'Script completed!'
PRINT 'Run time: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='
GO
