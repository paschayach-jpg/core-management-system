-- perf_001_FolderIndexes.sql
-- Performance indexes for storage.Folder table.
-- All CREATE INDEX statements are guarded with IF NOT EXISTS — safe to run multiple times.
-- DO NOT modify existing SPs or queries. Index optimizer picks these up automatically.
--
-- Query patterns covered:
--   sp_GetFoldersByOrgUnit      WHERE OrgUnitID = @x
--   sp_GetRootFoldersByOrgUnit  WHERE OrgUnitID = @x AND ParentFolderID IS NULL
--   sp_GetDeletedFoldersByOrgUnit WHERE OrgUnitID = @x AND IsDeleted = 1
--   sp_GetActiveFolders         WHERE IsDeleted = 0 AND IsArchived = 0
--   GetAllDeletedFolders        WHERE IsDeleted = 1 ORDER BY DeletedDate DESC
-- ============================================================

-- Index 1: OrgUnit-scoped queries (most frequently called — GetByOrgUnit, GetRootsByOrgUnit, GetDeletedByOrgUnit)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Folder_OrgUnitID_IsDeleted_IsArchived'
      AND object_id = OBJECT_ID('storage.Folder')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Folder_OrgUnitID_IsDeleted_IsArchived
    ON storage.Folder (OrgUnitID, IsDeleted, IsArchived)
    INCLUDE (FolderID, FolderName, ParentFolderID, ConfidentialityLevel,
             FolderDepth, PhysicalPath, InheritPermission,
             CreatedBy, CreatedDate, FolderSizeBytes, FileCount);
END
GO

-- Index 2: Deleted-folder recycle bin (GetAllDeletedFolders ORDER BY DeletedDate DESC)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Folder_IsDeleted_DeletedDate'
      AND object_id = OBJECT_ID('storage.Folder')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Folder_IsDeleted_DeletedDate
    ON storage.Folder (IsDeleted, DeletedDate DESC)
    INCLUDE (FolderID, FolderName, OrgUnitID, DeletedBy, RecycleBinExpiry);
END
GO
