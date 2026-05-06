-- Migration: Add IsHidden and audit fields to t_FileDocument
-- Purpose: Enable file hiding feature (MOD-003)
-- Created: 2026-05-06
-- Target DB: CMS_DB

USE CMS_DB;
GO

-- Add IsHidden, HiddenBy, HiddenDate, UpdatedBy, UpdatedDate columns if they don't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 't_FileDocument' AND COLUMN_NAME = 'IsHidden')
BEGIN
    ALTER TABLE t_FileDocument
    ADD IsHidden BIT NOT NULL DEFAULT 0;
    PRINT 'Column [IsHidden] added to t_FileDocument';
END
ELSE
    PRINT 'Column [IsHidden] already exists in t_FileDocument';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 't_FileDocument' AND COLUMN_NAME = 'HiddenBy')
BEGIN
    ALTER TABLE t_FileDocument
    ADD HiddenBy NVARCHAR(100) NULL;
    PRINT 'Column [HiddenBy] added to t_FileDocument';
END
ELSE
    PRINT 'Column [HiddenBy] already exists in t_FileDocument';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 't_FileDocument' AND COLUMN_NAME = 'HiddenDate')
BEGIN
    ALTER TABLE t_FileDocument
    ADD HiddenDate DATETIME NULL;
    PRINT 'Column [HiddenDate] added to t_FileDocument';
END
ELSE
    PRINT 'Column [HiddenDate] already exists in t_FileDocument';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 't_FileDocument' AND COLUMN_NAME = 'UpdatedBy')
BEGIN
    ALTER TABLE t_FileDocument
    ADD UpdatedBy NVARCHAR(100) NULL;
    PRINT 'Column [UpdatedBy] added to t_FileDocument';
END
ELSE
    PRINT 'Column [UpdatedBy] already exists in t_FileDocument';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 't_FileDocument' AND COLUMN_NAME = 'UpdatedDate')
BEGIN
    ALTER TABLE t_FileDocument
    ADD UpdatedDate DATETIME NULL;
    PRINT 'Column [UpdatedDate] added to t_FileDocument';
END
ELSE
    PRINT 'Column [UpdatedDate] already exists in t_FileDocument';

-- Create index for filtering hidden files
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileDocument_IsHidden' AND object_id = OBJECT_ID('t_FileDocument'))
BEGIN
    CREATE INDEX IX_FileDocument_IsHidden ON t_FileDocument(IsHidden, FolderID)
    INCLUDE (FileID, OriginalFileName, FileStatus);
    PRINT 'Index [IX_FileDocument_IsHidden] created';
END
ELSE
    PRINT 'Index [IX_FileDocument_IsHidden] already exists';

GO

PRINT '========================================';
PRINT 'Migration completed successfully!';
PRINT '========================================';
