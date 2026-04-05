-- ============================================================
-- Script 061: แก้ sp_ResolveFolderAuthority ให้ใช้ LevelCode
--             จาก UserAccount.LevelCode match กับ
--             FolderAuthorityConfig.LevelCode โดยตรง
--
-- Priority:
--   0. LevelCode  (UserAccount.LevelCode = FolderAuthorityConfig.LevelCode)
--   1. PositionName
--   2. DepartmentCode
--   3. DivisionCode
-- ============================================================

CREATE OR ALTER PROCEDURE core.sp_ResolveFolderAuthority
    @LevelCode      NVARCHAR(20)  = NULL,
    @PositionName   NVARCHAR(200) = NULL,
    @DepartmentCode NVARCHAR(50)  = NULL,
    @DivisionCode   NVARCHAR(50)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- ── Priority 0: LevelCode ────────────────────────────────────────────────
    -- match UserAccount.LevelCode กับ FolderAuthorityConfig.LevelCode โดยตรง
    -- คืน 4 rows (1HC/2SC/3NC/4PC) ของ config group นั้น
    IF @LevelCode IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE  IsActive = 1
          AND  LevelCode = @LevelCode
    )
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   core.FolderAuthorityConfig
        WHERE  IsActive = 1
          AND  LevelCode = @LevelCode
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 1: PositionName ─────────────────────────────────────────────
    IF @PositionName IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE  IsActive = 1 AND PositionName = @PositionName
    )
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   core.FolderAuthorityConfig
        WHERE  IsActive = 1 AND PositionName = @PositionName
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 2: DepartmentCode ───────────────────────────────────────────
    IF @DepartmentCode IS NOT NULL AND EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE  IsActive = 1
          AND  DepartmentCode = @DepartmentCode
          AND  PositionName   IS NULL
    )
    BEGIN
        SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
               LevelCode, ConfidentialityLevel,
               CanView, CanAdd, CanEdit, CanApprove,
               IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
        FROM   core.FolderAuthorityConfig
        WHERE  IsActive = 1
          AND  DepartmentCode = @DepartmentCode
          AND  PositionName   IS NULL
        ORDER  BY ConfidentialityLevel;
        RETURN;
    END

    -- ── Priority 3: DivisionCode ─────────────────────────────────────────────
    SELECT ConfigID, ConfigLabel, PositionName, DepartmentCode, DivisionCode,
           LevelCode, ConfidentialityLevel,
           CanView, CanAdd, CanEdit, CanApprove,
           IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
    FROM   core.FolderAuthorityConfig
    WHERE  IsActive = 1
      AND  DivisionCode   = @DivisionCode
      AND  PositionName   IS NULL
      AND  DepartmentCode IS NULL
    ORDER  BY ConfidentialityLevel;
END
GO

PRINT '✅ sp_ResolveFolderAuthority updated — Priority 0 = LevelCode field match';
GO
