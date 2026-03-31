-- ============================================================
-- Script 047: Org Unit Visibility Permission
-- สร้าง table security.UserOrgVisibility และ Stored Procedures
-- สำหรับ explicit per-user OrgUnit visibility grants
-- ============================================================

-- ============================================================
-- TABLE: security.UserOrgVisibility
-- ============================================================

CREATE TABLE security.UserOrgVisibility (
    UserID      INT      NOT NULL,
    OrgUnitID   INT      NOT NULL,
    GrantedBy   INT      NOT NULL,
    GrantedDate DATETIME NOT NULL CONSTRAINT DF_sec_UOV_GrantedDate DEFAULT GETDATE(),
    CONSTRAINT PK_sec_UserOrgVisibility     PRIMARY KEY (UserID, OrgUnitID),
    CONSTRAINT FK_sec_UOV_User              FOREIGN KEY (UserID)    REFERENCES core.UserAccount(UserID),
    CONSTRAINT FK_sec_UOV_OrgUnit           FOREIGN KEY (OrgUnitID) REFERENCES core.OrgUnit(OrgUnitID),
    CONSTRAINT FK_sec_UOV_GrantedBy         FOREIGN KEY (GrantedBy) REFERENCES core.UserAccount(UserID)
);
GO

CREATE INDEX IX_sec_UOV_OrgUnit ON security.UserOrgVisibility(OrgUnitID);
GO

-- ============================================================
-- SP: usp_UserOrgVisibility_GetByUser
-- ============================================================

CREATE OR ALTER PROCEDURE usp_UserOrgVisibility_GetByUser
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        uov.UserID,
        uov.OrgUnitID,
        uov.GrantedBy,
        uov.GrantedDate
    FROM security.UserOrgVisibility uov
    WHERE uov.UserID = @UserID;
END
GO

-- ============================================================
-- SP: usp_UserOrgVisibility_GetByOrgUnit
-- ============================================================

CREATE OR ALTER PROCEDURE usp_UserOrgVisibility_GetByOrgUnit
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        uov.UserID,
        uov.OrgUnitID,
        uov.GrantedBy,
        uov.GrantedDate
    FROM security.UserOrgVisibility uov
    WHERE uov.OrgUnitID = @OrgUnitID;
END
GO

-- ============================================================
-- SP: usp_UserOrgVisibility_Grant  (MERGE / upsert)
-- ============================================================

CREATE OR ALTER PROCEDURE usp_UserOrgVisibility_Grant
    @UserID    INT,
    @OrgUnitID INT,
    @GrantedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    MERGE security.UserOrgVisibility AS target
    USING (SELECT @UserID AS UserID, @OrgUnitID AS OrgUnitID) AS source
        ON target.UserID = source.UserID AND target.OrgUnitID = source.OrgUnitID
    WHEN MATCHED THEN
        UPDATE SET
            GrantedBy   = @GrantedBy,
            GrantedDate = GETDATE()
    WHEN NOT MATCHED THEN
        INSERT (UserID, OrgUnitID, GrantedBy, GrantedDate)
        VALUES (@UserID, @OrgUnitID, @GrantedBy, GETDATE());
END
GO

-- ============================================================
-- SP: usp_UserOrgVisibility_Revoke  (silent no-op if missing)
-- ============================================================

CREATE OR ALTER PROCEDURE usp_UserOrgVisibility_Revoke
    @UserID    INT,
    @OrgUnitID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM security.UserOrgVisibility
    WHERE UserID = @UserID AND OrgUnitID = @OrgUnitID;
END
GO

PRINT 'Script 047 completed: security.UserOrgVisibility table and stored procedures created.';
GO
