-- Script 048: Optimize Confidentiality View Loading
-- Purpose: Add stored procedure to fetch active folders efficiently for confidentiality tree view
-- This reduces the number of queries needed to build the "ตามความลับ" tree from N+1 to 1

-- Create or replace sp_GetActiveFolders to fetch only non-deleted, non-archived folders
IF OBJECT_ID('storage.sp_GetActiveFolders', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetActiveFolders;
GO

CREATE PROCEDURE storage.sp_GetActiveFolders
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        FolderID,
        FolderGUID,
        FolderName,
        ParentFolderID,
        OrgUnitID,
        TemplateID,
        ConfidentialityLevel,
        FolderDepth,
        PhysicalPath,
        FolderSizeBytes,
        FileCount,
        IsArchived,
        ArchivedBy,
        ArchivedDate,
        IsDeleted,
        DeletedBy,
        DeletedDate,
        RecycleBinExpiry,
        InheritPermission,
        CreatedBy,
        CreatedDate,
        UpdatedBy,
        UpdatedDate
    FROM storage.Folder
    WHERE IsDeleted = 0 
      AND IsArchived = 0
    ORDER BY OrgUnitID, ConfidentialityLevel, FolderName;
END;
GO

-- Add index to improve parent-child lookups (if not exists)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Folder_ParentFolderID_IsDeleted_IsArchived'
    AND object_id = OBJECT_ID('storage.Folder')
)
BEGIN
    CREATE INDEX IX_Folder_ParentFolderID_IsDeleted_IsArchived 
    ON storage.Folder(ParentFolderID, IsDeleted, IsArchived)
    INCLUDE (FolderID, FolderName, ConfidentialityLevel, FileCount);
END;
GO

-- Add index to improve confidentiality level lookups
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Folder_ConfidentialityLevel_IsDeleted'
    AND object_id = OBJECT_ID('storage.Folder')
)
BEGIN
    CREATE INDEX IX_Folder_ConfidentialityLevel_IsDeleted 
    ON storage.Folder(ConfidentialityLevel, IsDeleted)
    INCLUDE (FolderID, FolderName, OrgUnitID, ParentFolderID, FileCount);
END;
GO
