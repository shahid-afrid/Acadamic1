-- =============================================
-- Script: Create TeamActivityLogs table
-- Purpose: Track all activity/changes made to teams by Students, Faculty, and Admins
-- Date: 2026
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'TeamActivityLogs') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[TeamActivityLogs] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [TeamId] INT NOT NULL,
        [Action] NVARCHAR(200) NOT NULL,
        [Details] NVARCHAR(MAX) NULL,
        [PerformedByRole] NVARCHAR(50) NOT NULL,
        [PerformedByName] NVARCHAR(150) NOT NULL,
        [Timestamp] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_TeamActivityLogs] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TeamActivityLogs_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_TeamActivityLogs_TeamId] ON [dbo].[TeamActivityLogs] ([TeamId]);

    PRINT 'TeamActivityLogs table created successfully.';
END
ELSE
BEGIN
    PRINT 'TeamActivityLogs table already exists.';
END
GO
