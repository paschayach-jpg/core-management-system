-- =============================================================================
-- Script  : 097_Fix_sp_GetFileDocumentsByOrgUnit.sql
-- Purpose : Replace sp_GetFileDocumentsByOrgUnit
--           @OrgUnitID may be SubCommittee OrgUnitID, DivisionID, or DepartmentID.
--           For Department: aggregates files across ALL OrgUnit instances
--           sharing the same DepartmentCode (e.g. บุคคล in multiple sub-committees).
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
    WHERE s.name = 'storage' AND p.name = 'sp_GetFileDocumentsByOrgUnit'
)
    DROP PROCEDURE storage.sp_GetFileDocumentsByOrgUnit;
GO

CREATE PROCEDURE storage.sp_GetFileDocumentsByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FolderOrgUnits TABLE (OrgUnitID INT PRIMARY KEY);

    -- Sub Committee: sc.OrgUnitID is direct EMS.OrgUnit.OrgUnitID
    IF EXISTS (SELECT 1 FROM EMS.m_SubCommittee WHERE OrgUnitID = @OrgUnitID AND IsActive = 1 AND IsDeleted = 0)
        INSERT INTO @FolderOrgUnits SELECT @OrgUnitID;

    -- Division: DivisionID → OrgUnits at Division level AND all Department OrgUnits under this Division
    IF EXISTS (SELECT 1 FROM EMS.m_Division WHERE DivisionID = @OrgUnitID AND IsActive = 1)
    BEGIN
        -- Division-level OrgUnits (OrgUnitType = 2)
        INSERT INTO @FolderOrgUnits
        SELECT ou.OrgUnitID
        FROM   EMS.m_Division d
        JOIN   EMS.OrgUnit ou ON ou.OrgUnitCode = d.DivisionCode AND ou.OrgUnitType = 2 AND ou.IsDeleted = 0
        WHERE  d.DivisionID = @OrgUnitID;

        -- Department-level OrgUnits under this Division (OrgUnitType = 3)
        INSERT INTO @FolderOrgUnits
        SELECT ou.OrgUnitID
        FROM   EMS.m_Department dep
        JOIN   EMS.OrgUnit ou ON ou.OrgUnitCode = dep.DepartmentCode AND ou.OrgUnitType = 3 AND ou.IsDeleted = 0
        WHERE  dep.DivisionID = @OrgUnitID AND dep.IsActive = 1
          AND  ou.OrgUnitID NOT IN (SELECT OrgUnitID FROM @FolderOrgUnits);
    END

    -- Department: DepartmentID → ALL OrgUnit instances with same DepartmentCode
    IF EXISTS (SELECT 1 FROM EMS.m_Department WHERE DepartmentID = @OrgUnitID AND IsActive = 1)
        INSERT INTO @FolderOrgUnits
        SELECT ou.OrgUnitID
        FROM   EMS.m_Department dep
        JOIN   EMS.OrgUnit ou ON ou.OrgUnitCode = dep.DepartmentCode AND ou.OrgUnitType = 3 AND ou.IsDeleted = 0
        WHERE  dep.DepartmentID = @OrgUnitID;

    -- Fallback: treat as direct OrgUnitID
    IF NOT EXISTS (SELECT 1 FROM @FolderOrgUnits)
        INSERT INTO @FolderOrgUnits SELECT @OrgUnitID;

    SELECT
        fd.FileID, fd.FileGUID, fd.FolderID,
        COALESCE(f.FolderName, N'Folder ' + CAST(fd.FolderID AS NVARCHAR(20))) AS FolderName,
        fd.OriginalFileName, fd.StoredFileName, fd.FileExtension,
        fd.MimeType, fd.FileSizeBytes, fd.PhysicalPath, fd.Checksum,
        fd.VirusScanStatus, fd.VirusScanDate,
        fd.FileStatus, fd.UploadedBy, fd.UploadedDate,
        fd.ArchivedBy, fd.ArchivedDate,
        fd.DeletedBy, fd.DeletedDate, fd.Version
    FROM   storage.FileDocument fd
    INNER  JOIN storage.Folder f ON f.FolderID = fd.FolderID
    INNER  JOIN @FolderOrgUnits ids ON ids.OrgUnitID = f.OrgUnitID
    WHERE  fd.FileStatus != 3
      AND  f.IsDeleted   = 0
    ORDER  BY f.FolderName, fd.OriginalFileName;
END
GO

PRINT '=== storage.sp_GetFileDocumentsByOrgUnit updated (entity-aware, multi-OrgUnit) ===';
GO
