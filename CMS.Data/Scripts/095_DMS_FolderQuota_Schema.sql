-- =============================================================================
-- Script : 095_DMS_FolderQuota_Schema.sql
-- Module : DMS — Folder Quota Management
-- Tables : dms.FolderQuotaConfig
--          dms.FolderQuotaChangeRequest
--          dms.FolderQuotaUsageLog
-- =============================================================================

-- ── 1. FolderQuotaConfig ──────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'dms' AND TABLE_NAME = 'FolderQuotaConfig')
BEGIN
    CREATE TABLE dms.FolderQuotaConfig (
        QuotaConfigID       INT           NOT NULL IDENTITY(1,1),
        OrgUnitID           INT           NOT NULL,  -- FK -> EMS.OrgUnit
        ParentOrgUnitID     INT           NULL,       -- NULL = ทั่วไป, NOT NULL = เฉพาะ Sub นั้น
        QuotaBytes          BIGINT        NOT NULL,   -- 40GB default = 42949672960
        WarnThresholdPct    TINYINT       NOT NULL DEFAULT 80,
        CriticalThresholdPct TINYINT      NOT NULL DEFAULT 95,
        IsActive            BIT           NOT NULL DEFAULT 1,
        CreatedBy           INT           NOT NULL,
        CreatedDate         DATETIME      NOT NULL DEFAULT GETDATE(),
        UpdatedBy           INT           NULL,
        UpdatedDate         DATETIME      NULL,

        CONSTRAINT PK_FolderQuotaConfig PRIMARY KEY (QuotaConfigID),
        CONSTRAINT FK_FolderQuotaConfig_OrgUnit
            FOREIGN KEY (OrgUnitID) REFERENCES EMS.OrgUnit(OrgUnitID),
        CONSTRAINT FK_FolderQuotaConfig_ParentOrgUnit
            FOREIGN KEY (ParentOrgUnitID) REFERENCES EMS.OrgUnit(OrgUnitID),
        CONSTRAINT CHK_FolderQuotaConfig_WarnPct
            CHECK (WarnThresholdPct BETWEEN 1 AND 99),
        CONSTRAINT CHK_FolderQuotaConfig_CritPct
            CHECK (CriticalThresholdPct BETWEEN 2 AND 100
                   AND CriticalThresholdPct > WarnThresholdPct)
    );

    CREATE UNIQUE INDEX UX_FolderQuotaConfig_OrgUnit
        ON dms.FolderQuotaConfig (OrgUnitID, ParentOrgUnitID)
        WHERE IsActive = 1;

    PRINT 'Created table: dms.FolderQuotaConfig';
END
ELSE
    PRINT 'Already exists: dms.FolderQuotaConfig';
GO

-- ── 2. FolderQuotaChangeRequest ───────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'dms' AND TABLE_NAME = 'FolderQuotaChangeRequest')
BEGIN
    CREATE TABLE dms.FolderQuotaChangeRequest (
        RequestID           INT           NOT NULL IDENTITY(1,1),
        OrgUnitID           INT           NOT NULL,
        CurrentQuotaBytes   BIGINT        NOT NULL DEFAULT 0,
        RequestedQuotaBytes BIGINT        NOT NULL,
        Reason              NVARCHAR(500) NULL,
        -- Status: 1=Pending, 2=Approved, 3=Rejected, 4=Cancelled
        Status              TINYINT       NOT NULL DEFAULT 1,
        RequestedBy         INT           NOT NULL,
        RequestedDate       DATETIME      NOT NULL DEFAULT GETDATE(),
        ReviewedBy          INT           NULL,
        ReviewedDate        DATETIME      NULL,
        ReviewRemark        NVARCHAR(500) NULL,

        CONSTRAINT PK_FolderQuotaChangeRequest PRIMARY KEY (RequestID),
        CONSTRAINT FK_FolderQuotaChangeRequest_OrgUnit
            FOREIGN KEY (OrgUnitID) REFERENCES EMS.OrgUnit(OrgUnitID),
        CONSTRAINT FK_FolderQuotaChangeRequest_RequestedBy
            FOREIGN KEY (RequestedBy) REFERENCES EMS.UserAccount(UserID),
        CONSTRAINT FK_FolderQuotaChangeRequest_ReviewedBy
            FOREIGN KEY (ReviewedBy) REFERENCES EMS.UserAccount(UserID),
        CONSTRAINT CHK_FolderQuotaChangeRequest_Status
            CHECK (Status IN (1, 2, 3, 4))
    );

    CREATE INDEX IX_FolderQuotaChangeRequest_OrgUnit
        ON dms.FolderQuotaChangeRequest (OrgUnitID, Status);

    CREATE INDEX IX_FolderQuotaChangeRequest_RequestedBy
        ON dms.FolderQuotaChangeRequest (RequestedBy, RequestedDate DESC);

    PRINT 'Created table: dms.FolderQuotaChangeRequest';
END
ELSE
    PRINT 'Already exists: dms.FolderQuotaChangeRequest';
GO

-- ── 3. FolderQuotaUsageLog (optional: track usage trend) ─────────────────────
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'dms' AND TABLE_NAME = 'FolderQuotaUsageLog')
BEGIN
    CREATE TABLE dms.FolderQuotaUsageLog (
        LogID       BIGINT   NOT NULL IDENTITY(1,1),
        OrgUnitID   INT      NOT NULL,
        UsedBytes   BIGINT   NOT NULL,
        LogDate     DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT PK_FolderQuotaUsageLog PRIMARY KEY (LogID),
        CONSTRAINT FK_FolderQuotaUsageLog_OrgUnit
            FOREIGN KEY (OrgUnitID) REFERENCES EMS.OrgUnit(OrgUnitID)
    );

    CREATE INDEX IX_FolderQuotaUsageLog_OrgUnit_Date
        ON dms.FolderQuotaUsageLog (OrgUnitID, LogDate DESC);

    PRINT 'Created table: dms.FolderQuotaUsageLog';
END
ELSE
    PRINT 'Already exists: dms.FolderQuotaUsageLog';
GO

-- ── 4. Seed default quota config (40 GB per OrgUnit tier) ────────────────────
-- ปรับ OrgUnitID ให้ตรงกับข้อมูลจริงในระบบ
-- ตัวอย่างนี้เป็น comment — ให้ Admin กำหนดผ่าน FolderQuotaConfigForm แทน
/*
INSERT INTO dms.FolderQuotaConfig (OrgUnitID, QuotaBytes, WarnThresholdPct, CriticalThresholdPct, IsActive, CreatedBy)
SELECT OrgUnitID,
       42949672960,   -- 40 GB
       80, 95, 1,
       1              -- Admin UserID
FROM   EMS.OrgUnit
WHERE  IsActive = 1 AND IsDeleted = 0;
*/

PRINT '=== 095_DMS_FolderQuota_Schema.sql completed ===';
GO

