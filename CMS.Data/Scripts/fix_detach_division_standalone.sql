-- ============================================================
-- fix_detach_division_standalone.sql
--
-- Makes a Division standalone (same level as a SubCommittee): ParentOrgUnitID = NULL and every
-- active EMS.OrgUnitRelationship link closed. Same effect as the app's
-- "แยกเป็นฝ่ายอิสระ" action (OrgStructureService.DetachDivisionToStandalone).
-- Departments, folders and visibility grants are not touched.
--
-- Example (13/10/2026): 242 ฝ่ายบริหาร (10009) — parent 76 คณะกรรมการลงทุน, linked to 95 สินไหมทดแทน.
--
-- Previews and ROLLS BACK unless @Commit = 1.
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @DivisionID    INT           = 242;
DECLARE @ActorUserID   INT           = 1;
DECLARE @ActorUserName NVARCHAR(100) = N'data-fix: detach division to standalone';
DECLARE @Commit        BIT           = 0;   -- 0 = preview + rollback, 1 = apply
DECLARE @Now           DATETIME      = GETDATE();

DECLARE @OldParentID INT, @NameTH NVARCHAR(200), @Code NVARCHAR(50);

SELECT @OldParentID = ParentOrgUnitID, @NameTH = OrgUnitNameTH, @Code = OrgUnitCode
FROM   EMS.OrgUnit
WHERE  OrgUnitID = @DivisionID AND OrgUnitType = 2 AND IsDeleted = 0 AND IsActive = 1;

IF @NameTH IS NULL
    THROW 50001, 'Target must be an active, non-deleted Division (OrgUnitType = 2).', 1;

IF EXISTS (SELECT 1 FROM EMS.OrgUnit
           WHERE OrgUnitNameTH = @NameTH AND ParentOrgUnitID IS NULL AND IsDeleted = 0 AND OrgUnitID <> @DivisionID)
    THROW 50002, 'Another top-level OrgUnit already has this name.', 1;

PRINT '== BEFORE ==';
SELECT OrgUnitID, OrgUnitCode, OrgUnitNameTH, ParentOrgUnitID FROM EMS.OrgUnit WHERE OrgUnitID = @DivisionID;
SELECT RelationshipID, SubCommitteeID, DivisionID, IsActive FROM EMS.OrgUnitRelationship WHERE DivisionID = @DivisionID;

BEGIN TRANSACTION;

    UPDATE EMS.OrgUnit SET ParentOrgUnitID = NULL WHERE OrgUnitID = @DivisionID;

    DECLARE @Closed TABLE (RelationshipID INT, SubCommitteeID INT);
    UPDATE EMS.OrgUnitRelationship
    SET    IsActive = 0
    OUTPUT inserted.RelationshipID, inserted.SubCommitteeID INTO @Closed
    WHERE  DivisionID = @DivisionID AND IsActive = 1;

    INSERT INTO audit.AuditLog (AuditDate, UserID, UserName, Module, Action, TableName, RecordID, OldValue, NewValue, Result)
    VALUES (@Now, @ActorUserID, @ActorUserName, 'OrgStructure', 'MOVE', 'EMS.OrgUnit', @DivisionID,
            CONCAT(N'ParentOrgUnitID=', ISNULL(CAST(@OldParentID AS NVARCHAR(20)), N'NULL')),
            CONCAT(N'Data fix: Division ''', @NameTH, N''' (', @Code, N') made standalone (no parent).'), 1);

    INSERT INTO audit.AuditLog (AuditDate, UserID, UserName, Module, Action, TableName, RecordID, OldValue, NewValue, Result)
    SELECT @Now, @ActorUserID, @ActorUserName, 'OrgStructure', 'UNLINK', 'EMS.OrgUnitRelationship', @DivisionID,
           CONCAT(N'RelationshipID=', c.RelationshipID, N' SubCommitteeID=', c.SubCommitteeID, N' IsActive=1'),
           CONCAT(N'Data fix: unlinked Division ', @DivisionID, N' from SubCommittee ', c.SubCommitteeID,
                  N' (made standalone).'), 1
    FROM   @Closed c;

PRINT '== AFTER ==';
SELECT OrgUnitID, OrgUnitCode, OrgUnitNameTH, ParentOrgUnitID FROM EMS.OrgUnit WHERE OrgUnitID = @DivisionID;
SELECT RelationshipID, SubCommitteeID, DivisionID, IsActive FROM EMS.OrgUnitRelationship WHERE DivisionID = @DivisionID;

IF @Commit = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT 'COMMITTED';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT 'ROLLED BACK (preview only) — set @Commit = 1 to apply';
END
