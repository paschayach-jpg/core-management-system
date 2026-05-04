-- ============================================================
-- Migration: เพิ่ม OrgUnitID ใน storage.FolderTemplate
-- วันที่: 2026-05-05  |  โครงการ 2511AL01
-- ============================================================

-- 1. เพิ่ม Column
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('storage.FolderTemplate')
      AND name = 'OrgUnitID'
)
BEGIN
    ALTER TABLE storage.FolderTemplate
        ADD OrgUnitID INT NULL
            CONSTRAINT FK_FolderTemplate_OrgUnit
            FOREIGN KEY REFERENCES core.OrgUnit(OrgUnitID);

    PRINT 'Added OrgUnitID column to storage.FolderTemplate';
END
ELSE
    PRINT 'OrgUnitID already exists — skipped';
GO

-- ============================================================
-- 2. อัปเดต Stored Procedure: sp_InsertFolderTemplate
-- ============================================================
CREATE OR ALTER PROCEDURE storage.sp_InsertFolderTemplate
    @TemplateCode   NVARCHAR(50),
    @TemplateName   NVARCHAR(200),
    @TargetType     NVARCHAR(50),
    @Description    NVARCHAR(500) = NULL,
    @OrgUnitID      INT           = NULL,
    @CreatedBy      INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO storage.FolderTemplate
        (TemplateCode, TemplateName, TargetType, Description, OrgUnitID, IsActive, CreatedBy, CreatedDate)
    VALUES
        (@TemplateCode, @TemplateName, @TargetType, @Description, @OrgUnitID, 1, @CreatedBy, GETDATE());

    SELECT SCOPE_IDENTITY() AS NewID;
END
GO

-- ============================================================
-- 3. อัปเดต Stored Procedure: sp_UpdateFolderTemplate
-- ============================================================
CREATE OR ALTER PROCEDURE storage.sp_UpdateFolderTemplate
    @TemplateID     INT,
    @TemplateCode   NVARCHAR(50),
    @TemplateName   NVARCHAR(200),
    @TargetType     NVARCHAR(50),
    @Description    NVARCHAR(500) = NULL,
    @OrgUnitID      INT           = NULL,
    @IsActive       BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.FolderTemplate
    SET
        TemplateCode  = @TemplateCode,
        TemplateName  = @TemplateName,
        TargetType    = @TargetType,
        Description   = @Description,
        OrgUnitID     = @OrgUnitID,
        IsActive      = @IsActive
    WHERE TemplateID = @TemplateID;
END
GO

-- ============================================================
-- 4. อัปเดต Stored Procedure: sp_GetAllFolderTemplates
--    JOIN กับ m_OrgUnit เพื่อดึง OrgUnitName
-- ============================================================
CREATE OR ALTER PROCEDURE storage.sp_GetAllFolderTemplates
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        ft.TemplateID,
        ft.TemplateCode,
        ft.TemplateName,
        ft.TargetType,
        ft.Description,
        ft.OrgUnitID,
        ou.OrgUnitNameTH AS OrgUnitName,
        ft.IsActive,
        ft.CreatedBy,
        ft.CreatedDate
    FROM storage.FolderTemplate ft
    LEFT JOIN core.OrgUnit ou ON ft.OrgUnitID = ou.OrgUnitID
    WHERE ft.IsActive = 1
    ORDER BY ft.TemplateName;
END
GO

-- ============================================================
-- 5. อัปเดต Stored Procedure: sp_GetFolderTemplateById
-- ============================================================
CREATE OR ALTER PROCEDURE storage.sp_GetFolderTemplateById
    @TemplateID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        ft.TemplateID,
        ft.TemplateCode,
        ft.TemplateName,
        ft.TargetType,
        ft.Description,
        ft.OrgUnitID,
        ou.OrgUnitNameTH AS OrgUnitName,
        ft.IsActive,
        ft.CreatedBy,
        ft.CreatedDate
    FROM storage.FolderTemplate ft
    LEFT JOIN core.OrgUnit ou ON ft.OrgUnitID = ou.OrgUnitID
    WHERE ft.TemplateID = @TemplateID;
END
GO
