-- ============================================================
-- Download & Share approver config: who may approve file-download requests or internal file-share requests, per HR department / division.
--   ApprovalType 'D' = Download Approver (default)
--   ApprovalType 'S' = Internal Share Approver
--   ScopeType 'D'    = Department (OrgCode = EMS.m_Department.DepartmentCode)
--   ScopeType 'V'    = Division   (OrgCode = EMS.m_Division.DivisionCode)
-- A file's folder is matched by EMS.OrgUnit.OrgUnitCode (department) and its parent division code.
-- Removing an approver sets IsActive = 0 (history kept).
-- Rename with the next schema script number before committing. Safe to re-run.
-- ============================================================

IF OBJECT_ID(N'workflow.DownloadApproverConfig', N'U') IS NULL
BEGIN
    CREATE TABLE workflow.DownloadApproverConfig
    (
        ConfigID       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DownloadApproverConfig PRIMARY KEY,
        ApprovalType   CHAR(1)       NOT NULL CONSTRAINT DF_DownloadApprover_ApprovalType DEFAULT ('D'),
        ScopeType      CHAR(1)       NOT NULL,
        OrgCode        NVARCHAR(50)  NOT NULL,
        ApproverUserID INT           NOT NULL,
        SortOrder      INT           NOT NULL CONSTRAINT DF_DownloadApprover_SortOrder DEFAULT (0),
        IsActive       BIT           NOT NULL CONSTRAINT DF_DownloadApprover_IsActive  DEFAULT (1),
        CreatedBy      INT           NULL,
        CreatedDate    DATETIME      NOT NULL CONSTRAINT DF_DownloadApprover_CreatedDate DEFAULT (GETDATE()),
        UpdatedBy      INT           NULL,
        UpdatedDate    DATETIME      NULL,
        CONSTRAINT CK_DownloadApprover_ApprovalType CHECK (ApprovalType IN ('D', 'S')),
        CONSTRAINT CK_DownloadApprover_ScopeType CHECK (ScopeType IN ('D', 'V')),
        CONSTRAINT FK_DownloadApprover_User FOREIGN KEY (ApproverUserID) REFERENCES EMS.UserAccount (UserID),
        CONSTRAINT UQ_DownloadApprover UNIQUE (ApprovalType, ScopeType, OrgCode, ApproverUserID)
    );

    CREATE INDEX IX_DownloadApprover_Org ON workflow.DownloadApproverConfig (ApprovalType, ScopeType, OrgCode, IsActive);
    PRINT 'Created workflow.DownloadApproverConfig';
END
ELSE
BEGIN
    -- Upgrade existing table if ApprovalType column is missing
    IF COL_LENGTH('workflow.DownloadApproverConfig', 'ApprovalType') IS NULL
    BEGIN
        -- Drop old unique constraint and index if exists
        IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_DownloadApprover' AND parent_object_id = OBJECT_ID('workflow.DownloadApproverConfig'))
            ALTER TABLE workflow.DownloadApproverConfig DROP CONSTRAINT UQ_DownloadApprover;

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DownloadApprover_Org' AND object_id = OBJECT_ID('workflow.DownloadApproverConfig'))
            DROP INDEX IX_DownloadApprover_Org ON workflow.DownloadApproverConfig;

        ALTER TABLE workflow.DownloadApproverConfig
            ADD ApprovalType CHAR(1) NOT NULL CONSTRAINT DF_DownloadApprover_ApprovalType DEFAULT ('D');

        ALTER TABLE workflow.DownloadApproverConfig
            ADD CONSTRAINT CK_DownloadApprover_ApprovalType CHECK (ApprovalType IN ('D', 'S'));

        ALTER TABLE workflow.DownloadApproverConfig
            ADD CONSTRAINT UQ_DownloadApprover UNIQUE (ApprovalType, ScopeType, OrgCode, ApproverUserID);

        CREATE INDEX IX_DownloadApprover_Org ON workflow.DownloadApproverConfig (ApprovalType, ScopeType, OrgCode, IsActive);

        PRINT 'Added ApprovalType column and updated constraints on workflow.DownloadApproverConfig';
    END
    ELSE
    BEGIN
        PRINT 'workflow.DownloadApproverConfig is up to date';
    END
END
