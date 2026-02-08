-- =============================================
-- Script: Add ProofImageData and ProofContentType columns to TeamMeetings table
-- Purpose: Store proof images directly in the database instead of the file system
-- Date: 2026
-- =============================================

-- Add ProofImageData column (stores the image bytes)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'TeamMeetings') AND name = 'ProofImageData')
BEGIN
    ALTER TABLE [dbo].[TeamMeetings]
    ADD [ProofImageData] VARBINARY(MAX) NULL;
    PRINT 'Added ProofImageData column to TeamMeetings table.';
END
ELSE
BEGIN
    PRINT 'ProofImageData column already exists.';
END
GO

-- Add ProofContentType column (stores the MIME type, e.g., "image/jpeg")
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'TeamMeetings') AND name = 'ProofContentType')
BEGIN
    ALTER TABLE [dbo].[TeamMeetings]
    ADD [ProofContentType] NVARCHAR(100) NULL;
    PRINT 'Added ProofContentType column to TeamMeetings table.';
END
ELSE
BEGIN
    PRINT 'ProofContentType column already exists.';
END
GO

PRINT 'Migration complete. Proof images will now be stored in the database.';
