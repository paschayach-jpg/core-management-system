-- ============================================================
-- Script 067: Seed FolderAuthorityConfig จาก HR_PositionMapping
-- Insert 4 rows (ConfidentialityLevel 1-4) ต่อ 1 ตำแหน่ง
-- Skip ถ้า ConfigLabel นั้นมีอยู่แล้ว (idempotent)
-- ============================================================

IF OBJECT_ID('core.sp_SeedFolderAuthorityFromPositionMapping', 'P') IS NOT NULL
    DROP PROCEDURE core.sp_SeedFolderAuthorityFromPositionMapping;
GO

CREATE PROCEDURE core.sp_SeedFolderAuthorityFromPositionMapping
    @CanView    BIT = 0,
    @CanAdd     BIT = 0,
    @CanEdit    BIT = 1,
    @CanApprove BIT = 1,
    @CreatedBy  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- ── Source: HR_PositionMapping JOIN HR_Position, HR_Division, HR_Department ──
    -- ตัด duplicate โดย distinct บน (PositionName, DepartmentCode, DivisionCode, LevelCode)
    ;WITH SourceData AS
    (
        SELECT DISTINCT
            b.PositionName                          AS PositionName,
            d.DepartmentCode                        AS DepartmentCode,
            c.DivisionCode                          AS DivisionCode,
            e.LevelCode                             AS LevelCode
        FROM [SCS].[core].[HR_PositionMapping]  a
        INNER JOIN [SCS].[core].[HR_Position]   b ON a.PositionID   = b.PositionID
        INNER JOIN [SCS].[core].[HR_Division]   c ON a.DivisionID   = c.DivisionID
        INNER JOIN [SCS].[core].[HR_Department] d ON a.DepartmentID = d.DepartmentID
        INNER JOIN [SCS].[core].[HR_Position]   e ON a.PositionID   = e.PositionID
        WHERE b.PositionName IS NOT NULL
          AND b.PositionName <> ''
    ),
    -- Level codes ทั้ง 4 ระดับ
    Levels AS
    (
        SELECT 1 AS ConfidentialityLevel, '1HC' AS FolderLevelCode UNION ALL
        SELECT 2,                          '2SC'                    UNION ALL
        SELECT 3,                          '3NC'                    UNION ALL
        SELECT 4,                          '4PC'
    )

    INSERT INTO [SCS].[core].[FolderAuthorityConfig]
    (
        ConfigLabel,
        PositionName,
        DepartmentCode,
        DivisionCode,
        ConfidentialityLevel,
        LevelCode,
        CanView,
        CanAdd,
        CanEdit,
        CanApprove,
        IsActive,
        CreatedBy,
        CreatedDate
    )
    SELECT
        s.PositionName              AS ConfigLabel,
        s.PositionName              AS PositionName,
        s.DepartmentCode            AS DepartmentCode,
        s.DivisionCode              AS DivisionCode,
        lv.ConfidentialityLevel     AS ConfidentialityLevel,
        lv.FolderLevelCode          AS LevelCode,
        @CanView                    AS CanView,
        @CanAdd                     AS CanAdd,
        @CanEdit                    AS CanEdit,
        @CanApprove                 AS CanApprove,
        1                           AS IsActive,
        @CreatedBy                  AS CreatedBy,
        GETDATE()                   AS CreatedDate
    FROM SourceData s
    CROSS JOIN Levels lv
    -- Skip ถ้า ConfigLabel + ConfidentialityLevel มีอยู่แล้ว (IsActive ทุกสถานะ)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM [SCS].[core].[FolderAuthorityConfig] f
        WHERE f.ConfigLabel          = s.PositionName
          AND f.ConfidentialityLevel = lv.ConfidentialityLevel
    );

    DECLARE @Inserted INT = @@ROWCOUNT;
    PRINT CONCAT('Inserted ', @Inserted, ' rows into FolderAuthorityConfig (', @Inserted / 4, ' positions × 4 levels)');
END
GO

-- ── รัน SP ทันที (default: CanEdit=1, CanApprove=1, CanView=0, CanAdd=0) ──
EXEC core.sp_SeedFolderAuthorityFromPositionMapping
    @CanView    = 0,
    @CanAdd     = 0,
    @CanEdit    = 1,
    @CanApprove = 1,
    @CreatedBy  = NULL;
GO

-- ── ตรวจสอบผลลัพธ์ ──────────────────────────────────────────────────────────
SELECT
    ConfigLabel,
    PositionName,
    DepartmentCode,
    DivisionCode,
    LevelCode,
    ConfidentialityLevel,
    CanView, CanAdd, CanEdit, CanApprove,
    CreatedBy,
    CreatedDate
FROM [SCS].[core].[FolderAuthorityConfig]
WHERE CreatedBy IS NULL
ORDER BY ConfigLabel, ConfidentialityLevel;
GO

PRINT 'Script 067 completed.';
