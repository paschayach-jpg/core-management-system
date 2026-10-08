-- ============================================================
-- Download approver config: who may approve file-download requests, per HR department / division.
--   ScopeType 'D' = Department (OrgCode = EMS.m_Department.DepartmentCode)
--   ScopeType 'V' = Division   (OrgCode = EMS.m_Division.DivisionCode)
-- A file's folder is matched by EMS.OrgUnit.OrgUnitCode (department) and its parent division code.
-- Removing an approver sets IsActive = 0 (history kept).
-- Rename with the next schema script number before committing. Safe to re-run.
-- ============================================================

IF OBJECT_ID(N'workflow.DownloadApproverConfig', N'U') IS NULL
BEGIN
    CREATE TABLE workflow.DownloadApproverConfig
    (
        ConfigID       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DownloadApproverConfig PRIMARY KEY,
        ScopeType      CHAR(1)       NOT NULL,
        OrgCode        NVARCHAR(50)  NOT NULL,
        ApproverUserID INT           NOT NULL,
        SortOrder      INT           NOT NULL CONSTRAINT DF_DownloadApprover_SortOrder DEFAULT (0),
        IsActive       BIT           NOT NULL CONSTRAINT DF_DownloadApprover_IsActive  DEFAULT (1),
        CreatedBy      INT           NULL,
        CreatedDate    DATETIME      NOT NULL CONSTRAINT DF_DownloadApprover_CreatedDate DEFAULT (GETDATE()),
        UpdatedBy      INT           NULL,
        UpdatedDate    DATETIME      NULL,
        CONSTRAINT CK_DownloadApprover_ScopeType CHECK (ScopeType IN ('D', 'V')),
        CONSTRAINT FK_DownloadApprover_User FOREIGN KEY (ApproverUserID) REFERENCES EMS.UserAccount (UserID),
        CONSTRAINT UQ_DownloadApprover UNIQUE (ScopeType, OrgCode, ApproverUserID)
    );

    CREATE INDEX IX_DownloadApprover_Org ON workflow.DownloadApproverConfig (ScopeType, OrgCode, IsActive);
    PRINT 'Created workflow.DownloadApproverConfig';
END
ELSE
    PRINT 'workflow.DownloadApproverConfig already exists';
