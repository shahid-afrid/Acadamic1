-- Migration Script: Add Notifications Table
-- Run this script against your database to create the Notifications table

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE [Notifications] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [StudentId] INT NOT NULL,
        [Message] NVARCHAR(500) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL DEFAULT 'info',
        [IsRead] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Students_StudentId] FOREIGN KEY ([StudentId]) 
            REFERENCES [Students]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_Notifications_StudentId] ON [Notifications] ([StudentId]);

    PRINT 'Notifications table created successfully!';
END
ELSE
BEGIN
    PRINT 'Notifications table already exists.';
END
