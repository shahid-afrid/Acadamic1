-- SQL Script to Add MeetingInvitations Table
-- Run this in SQL Server Management Studio or Azure Data Studio
-- Make sure you're connected to your TeamPro1 database

-- Create MeetingInvitations Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MeetingInvitations')
BEGIN
    CREATE TABLE [dbo].[MeetingInvitations] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [TeamId] INT NOT NULL,
        [FacultyId] INT NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [MeetingDateTime] DATETIME2 NOT NULL,
        [Location] NVARCHAR(200) NULL,
        [DurationMinutes] INT NOT NULL DEFAULT 60,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [Student1ResponseId] INT NULL,
        [Student1Response] NVARCHAR(50) NULL,
        [Student2ResponseId] INT NULL,
        [Student2Response] NVARCHAR(50) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NULL,
        
        -- Foreign Keys
        CONSTRAINT [FK_MeetingInvitations_Teams_TeamId] 
            FOREIGN KEY ([TeamId]) REFERENCES [Teams]([Id]) ON DELETE CASCADE,
        
        CONSTRAINT [FK_MeetingInvitations_Faculties_FacultyId] 
            FOREIGN KEY ([FacultyId]) REFERENCES [Faculties]([Id]) ON DELETE NO ACTION,
        
        -- Constraints
        CONSTRAINT [CHK_MeetingInvitations_DurationMinutes] 
            CHECK ([DurationMinutes] >= 15 AND [DurationMinutes] <= 480),
        
        CONSTRAINT [CHK_MeetingInvitations_Status] 
            CHECK ([Status] IN ('Pending', 'Accepted', 'Rejected', 'Cancelled', 'Completed'))
    );
    
    -- Create Indexes for better query performance
    CREATE INDEX [IX_MeetingInvitations_TeamId] ON [MeetingInvitations]([TeamId]);
    CREATE INDEX [IX_MeetingInvitations_FacultyId] ON [MeetingInvitations]([FacultyId]);
    CREATE INDEX [IX_MeetingInvitations_MeetingDateTime] ON [MeetingInvitations]([MeetingDateTime]);
    CREATE INDEX [IX_MeetingInvitations_Status] ON [MeetingInvitations]([Status]);
    
    PRINT 'MeetingInvitations table created successfully';
END
ELSE
BEGIN
    PRINT 'MeetingInvitations table already exists';
END
GO

-- Verify table was created
SELECT 
    'MeetingInvitations' AS TableName, 
    COUNT(*) AS RecordCount 
FROM MeetingInvitations;
GO

PRINT 'MeetingInvitations table ready! Faculty can now send meeting invites to students.';
GO
