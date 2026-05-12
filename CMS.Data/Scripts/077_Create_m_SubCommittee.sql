-- ============================================================
-- Script  : 077_Create_m_SubCommittee.sql
-- Purpose : สร้าง table m_SubCommittee สำหรับเก็บข้อมูล SubCommittee
--           แยกออกจาก core.OrgUnit เพื่อรองรับการขยาย field เฉพาะ SubCommittee
--           OrgUnitID เชื่อมกลับ core.OrgUnit (FK)
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = 'dbo' AND t.name = 'm_SubCommittee'
)
BEGIN
    CREATE TABLE m_SubCommittee (
        SubCommitteeID      INT IDENTITY(1,1)   NOT NULL,
        OrgUnitID           INT                 NOT NULL,
        SubCommitteeCode    NVARCHAR(20)        NOT NULL,
        SubCommitteeNameTH  NVARCHAR(200)       NOT NULL COLLATE Thai_CI_AS,
        SubCommitteeNameEN  NVARCHAR(200)       NULL,
        IsActive            BIT                 NOT NULL CONSTRAINT DF_mSubCommittee_IsActive    DEFAULT 1,
        IsDeleted           BIT                 NOT NULL CONSTRAINT DF_mSubCommittee_IsDeleted   DEFAULT 0,
        DeletedBy           NVARCHAR(100)       NULL,
        DeletedDate         DATETIME            NULL,
        CreatedBy           NVARCHAR(100)       NOT NULL,
        CreatedDate         DATETIME            NOT NULL CONSTRAINT DF_mSubCommittee_CreatedDate DEFAULT GETDATE(),
        UpdatedBy           NVARCHAR(100)       NULL,
        UpdatedDate         DATETIME            NULL,

        CONSTRAINT PK_mSubCommittee
            PRIMARY KEY CLUSTERED (SubCommitteeID),

        CONSTRAINT FK_mSubCommittee_OrgUnit
            FOREIGN KEY (OrgUnitID) REFERENCES core.OrgUnit(OrgUnitID),

        CONSTRAINT UQ_mSubCommittee_OrgUnitID
            UNIQUE (OrgUnitID)
    );

    PRINT 'Table m_SubCommittee created.';
END
ELSE
BEGIN
    PRINT 'Table m_SubCommittee already exists — skipped.';
END
GO

-- ── Stored Procedure: Insert ──────────────────────────────────────────────────
IF OBJECT_ID('dbo.sp_InsertSubCommittee', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertSubCommittee;
GO

CREATE PROCEDURE dbo.sp_InsertSubCommittee
    @OrgUnitID          INT,
    @SubCommitteeCode   NVARCHAR(20),
    @SubCommitteeNameTH NVARCHAR(200),
    @SubCommitteeNameEN NVARCHAR(200) = NULL,
    @CreatedBy          NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM m_SubCommittee WHERE OrgUnitID = @OrgUnitID)
    BEGIN
        RAISERROR(N'SubCommitteeID %d มีข้อมูลอยู่ใน m_SubCommittee แล้ว', 16, 1, @OrgUnitID);
        RETURN;
    END

    INSERT INTO m_SubCommittee
        (OrgUnitID, SubCommitteeCode, SubCommitteeNameTH, SubCommitteeNameEN, CreatedBy)
    VALUES
        (@OrgUnitID, @SubCommitteeCode, @SubCommitteeNameTH, @SubCommitteeNameEN, @CreatedBy);

    SELECT SCOPE_IDENTITY() AS SubCommitteeID;
END
GO

-- ── Stored Procedure: Soft Delete (sync เมื่อ OrgUnit ถูก soft-delete) ────────
IF OBJECT_ID('dbo.sp_SoftDeleteSubCommittee', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_SoftDeleteSubCommittee;
GO

CREATE PROCEDURE dbo.sp_SoftDeleteSubCommittee
    @OrgUnitID  INT,
    @DeletedBy  NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE m_SubCommittee
    SET    IsDeleted  = 1,
           IsActive   = 0,
           DeletedBy  = @DeletedBy,
           DeletedDate = GETDATE()
    WHERE  OrgUnitID = @OrgUnitID
      AND  IsDeleted = 0;
END
GO

PRINT 'Script 077 completed.';
