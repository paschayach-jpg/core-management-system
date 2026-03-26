-- Fix: sp_GetRootFoldersByOrgUnit ใช้ FolderDepth = 0 ซึ่งไม่ถูกต้อง
-- Root folder ควร query ด้วย ParentFolderID IS NULL แทน
-- เพราะ folder เก่าๆ อาจมี FolderDepth = 1 แต่ ParentFolderID = NULL

ALTER PROCEDURE storage.sp_GetRootFoldersByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [FolderID], [FolderGUID], [FolderName], [ParentFolderID], [OrgUnitID], [TemplateID],
           [ConfidentialityLevel], [FolderDepth], [PhysicalPath], [FolderSizeBytes], [FileCount],
           [IsArchived], [ArchivedBy], [ArchivedDate], [IsDeleted], [DeletedBy], [DeletedDate],
           [RecycleBinExpiry], [InheritPermission], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]
    FROM   [storage].[Folder]
    WHERE  [OrgUnitID] = @OrgUnitID AND [ParentFolderID] IS NULL AND [IsDeleted] = 0;
END
GO

PRINT 'sp_GetRootFoldersByOrgUnit updated successfully.';
