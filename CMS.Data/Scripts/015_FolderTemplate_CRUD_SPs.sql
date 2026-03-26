-- ============================================================
-- 015_FolderTemplate_CRUD_SPs.sql
-- Adds missing CRUD stored procedures for FolderTemplate
-- ============================================================

-- ── sp_GetFolderTemplateById ─────────────────────────────────────────────────
IF OBJECT_ID('storage.sp_GetFolderTemplateById', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_GetFolderTemplateById;
GO
CREATE PROCEDURE storage.sp_GetFolderTemplateById
    @TemplateID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM storage.FolderTemplate WHERE TemplateID = @TemplateID;
END
GO

-- ── sp_UpdateFolderTemplate ──────────────────────────────────────────────────
IF OBJECT_ID('storage.sp_UpdateFolderTemplate', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_UpdateFolderTemplate;
GO
CREATE PROCEDURE storage.sp_UpdateFolderTemplate
    @TemplateID   INT,
    @TemplateCode NVARCHAR(50),
    @TemplateName NVARCHAR(200),
    @TargetType   NVARCHAR(50),
    @Description  NVARCHAR(500) = NULL,
    @IsActive     BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.FolderTemplate
    SET TemplateCode = @TemplateCode,
        TemplateName = @TemplateName,
        TargetType   = @TargetType,
        Description  = @Description,
        IsActive     = @IsActive
    WHERE TemplateID = @TemplateID;
END
GO

-- ── sp_DeleteFolderTemplate ──────────────────────────────────────────────────
IF OBJECT_ID('storage.sp_DeleteFolderTemplate', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_DeleteFolderTemplate;
GO
CREATE PROCEDURE storage.sp_DeleteFolderTemplate
    @TemplateID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM storage.FolderTemplateItem WHERE TemplateID = @TemplateID;
    DELETE FROM storage.FolderTemplate     WHERE TemplateID = @TemplateID;
END
GO

-- ── sp_UpdateFolderTemplateItem ──────────────────────────────────────────────
IF OBJECT_ID('storage.sp_UpdateFolderTemplateItem', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_UpdateFolderTemplateItem;
GO
CREATE PROCEDURE storage.sp_UpdateFolderTemplateItem
    @ItemID              INT,
    @FolderName          NVARCHAR(200),
    @ConfidentialityLevel TINYINT,
    @SortOrder           INT,
    @InheritPermission   BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.FolderTemplateItem
    SET FolderName           = @FolderName,
        ConfidentialityLevel = @ConfidentialityLevel,
        SortOrder            = @SortOrder,
        InheritPermission    = @InheritPermission
    WHERE ItemID = @ItemID;
END
GO

-- ── sp_DeleteFolderTemplateItem ──────────────────────────────────────────────
IF OBJECT_ID('storage.sp_DeleteFolderTemplateItem', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_DeleteFolderTemplateItem;
GO
CREATE PROCEDURE storage.sp_DeleteFolderTemplateItem
    @ItemID INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Delete children recursively (simple 2-level; extend if deeper nesting needed)
    DELETE FROM storage.FolderTemplateItem
    WHERE ParentItemID IN (
        SELECT ItemID FROM storage.FolderTemplateItem WHERE ParentItemID = @ItemID
    );
    DELETE FROM storage.FolderTemplateItem WHERE ParentItemID = @ItemID;
    DELETE FROM storage.FolderTemplateItem WHERE ItemID = @ItemID;
END
GO
