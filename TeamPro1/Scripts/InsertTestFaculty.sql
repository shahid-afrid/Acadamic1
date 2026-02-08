-- Insert Test Faculty Members for Testing
-- Run this script in SQL Server Management Studio or through Visual Studio SQL Server Object Explorer

USE [TeamPro1Db]
GO

-- Check if test faculty already exists
IF NOT EXISTS (SELECT 1 FROM Faculties WHERE Email = 'faculty@test.com')
BEGIN
    INSERT INTO Faculties (FullName, Email, Password, Department, CreatedAt)
    VALUES 
    ('Dr. Test Faculty', 'faculty@test.com', 'test123', 'Computer Science', GETDATE())
    
    PRINT 'Test faculty added successfully!'
    PRINT 'Email: faculty@test.com'
    PRINT 'Password: test123'
END
ELSE
BEGIN
    PRINT 'Test faculty already exists!'
END

-- Optional: Add more test faculty members
IF NOT EXISTS (SELECT 1 FROM Faculties WHERE Email = 'john.doe@rgmcet.edu')
BEGIN
    INSERT INTO Faculties (FullName, Email, Password, Department, CreatedAt)
    VALUES 
    ('Dr. John Doe', 'john.doe@rgmcet.edu', 'password123', 'CSE(DS)', GETDATE())
    
    PRINT 'Dr. John Doe added successfully!'
END

IF NOT EXISTS (SELECT 1 FROM Faculties WHERE Email = 'jane.smith@rgmcet.edu')
BEGIN
    INSERT INTO Faculties (FullName, Email, Password, Department, CreatedAt)
    VALUES 
    ('Dr. Jane Smith', 'jane.smith@rgmcet.edu', 'password123', 'CSE(AI&ML)', GETDATE())
    
    PRINT 'Dr. Jane Smith added successfully!'
END

IF NOT EXISTS (SELECT 1 FROM Faculties WHERE Email = 'robert.brown@rgmcet.edu')
BEGIN
    INSERT INTO Faculties (FullName, Email, Password, Department, CreatedAt)
    VALUES 
    ('Prof. Robert Brown', 'robert.brown@rgmcet.edu', 'password123', 'Computer Science', GETDATE())
    
    PRINT 'Prof. Robert Brown added successfully!'
END

-- Display all faculty members
SELECT 
    Id,
    FullName,
    Email,
    Department,
    CreatedAt
FROM Faculties
ORDER BY Id

PRINT ''
PRINT '================================================'
PRINT 'Test Faculty Credentials:'
PRINT '================================================'
PRINT 'Email: faculty@test.com | Password: test123'
PRINT 'Email: john.doe@rgmcet.edu | Password: password123'
PRINT 'Email: jane.smith@rgmcet.edu | Password: password123'
PRINT 'Email: robert.brown@rgmcet.edu | Password: password123'
PRINT '================================================'
