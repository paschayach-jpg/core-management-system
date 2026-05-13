-- ============================================================
-- 079_Create_SystemConfig_Table_And_SPs.sql
-- สร้างตาราง SystemConfig และ Stored Procedures ที่ใช้โดย ConfigManager
-- ต้องรันก่อนเปิดใช้งาน ConfigAdminForm (Admin → System Configuration)
-- ============================================================

-- ── 1. สร้างตาราง SystemConfig (ถ้ายังไม่มี) ────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'SystemConfig' AND TABLE_SCHEMA = 'dbo'
)
BEGIN
    CREATE TABLE dbo.SystemConfig (
        ConfigKey       NVARCHAR(200)   NOT NULL,
        ConfigValue     NVARCHAR(2000)  NOT NULL,
        UpdatedByUserId INT             NULL,
        UpdatedDate     DATETIME        NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_SystemConfig PRIMARY KEY CLUSTERED (ConfigKey)
    );
END
GO

-- ── 2. Seed ค่า Default (INSERT เฉพาะ key ที่ยังไม่มี) ───────────────────────

MERGE dbo.SystemConfig AS target
USING (VALUES
    ('NetworkDrivePath',        '\\150.1.8.23\AppForUser\Cmd_ImportFile'),
    ('MaxFileSizeBytes',        '104857600'),
    ('MaxFilesPerUpload',       '10'),
    ('AllowedExtensions',       'pdf,doc,docx,xls,xlsx,ppt,pptx,jpg,png,gif,zip'),
    ('DefaultQuotaBytes',       '0'),
    ('ApprovalTimeoutMinutes',  '1440'),
    ('AuditRetentionDays',      '1825'),
    ('VirusScanTimeoutSeconds', '30')
) AS source (ConfigKey, ConfigValue)
ON target.ConfigKey = source.ConfigKey
WHEN NOT MATCHED THEN
    INSERT (ConfigKey, ConfigValue, UpdatedDate)
    VALUES (source.ConfigKey, source.ConfigValue, GETDATE());
GO

-- ── 3. sp_GetSystemConfig ─────────────────────────────────────────────────────

IF OBJECT_ID('dbo.sp_GetSystemConfig', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetSystemConfig;
GO

CREATE PROCEDURE dbo.sp_GetSystemConfig
    @Key NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ConfigValue
    FROM   dbo.SystemConfig
    WHERE  ConfigKey = @Key;
END
GO

-- ── 4. sp_UpsertSystemConfig ──────────────────────────────────────────────────

IF OBJECT_ID('dbo.sp_UpsertSystemConfig', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UpsertSystemConfig;
GO

CREATE PROCEDURE dbo.sp_UpsertSystemConfig
    @Key              NVARCHAR(200),
    @Value            NVARCHAR(2000),
    @UpdatedByUserId  INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.SystemConfig WHERE ConfigKey = @Key)
    BEGIN
        UPDATE dbo.SystemConfig
        SET    ConfigValue     = @Value,
               UpdatedByUserId = @UpdatedByUserId,
               UpdatedDate     = GETDATE()
        WHERE  ConfigKey = @Key;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.SystemConfig (ConfigKey, ConfigValue, UpdatedByUserId, UpdatedDate)
        VALUES (@Key, @Value, @UpdatedByUserId, GETDATE());
    END
END
GO

-- ── 5. ตรวจสอบผลลัพธ์ ─────────────────────────────────────────────────────────

SELECT ConfigKey, ConfigValue, UpdatedDate
FROM   dbo.SystemConfig
ORDER BY ConfigKey;
GO
