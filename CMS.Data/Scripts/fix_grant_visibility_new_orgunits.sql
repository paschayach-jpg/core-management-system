-- ============================================================
-- fix_grant_visibility_new_orgunits.sql
--
-- Backfills security.UserOrgVisibility for OrgUnits created or linked before the app started
-- auto-granting visibility (OrgStructureService → UserOrgVisibilityService.PropagateToNewOrgUnit).
--
-- Problem: users with explicit visibility grants only see granted OrgUnits in Folder Explorer.
-- A new OrgUnit had no grants, so it was invisible even to the Admin who created it
-- (e.g. 13/10/2026: 242 ฝ่ายบริหาร / 243 แผนกงานเลขานุการ under 76 คณะกรรมการลงทุน).
--
-- Rule (same as the app):
--   For every active OrgUnit created on/after @Since, and every Division linked to a SubCommittee
--   via EMS.OrgUnitRelationship on/after @Since, grant visibility to
--     (a) every user already granted its parent (SubCommittee for a linked Division), and
--     (b) its creator — only if the creator already has at least one explicit grant.
--   Users with no explicit grants are never given a row (that would switch them off the
--   role-based fallback). Parents are processed before children so grants flow down
--   SubCommittee → Division → Department in one run.
--
-- Previews and ROLLS BACK unless @Commit = 1.
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Since         DATETIME      = '2026-10-07';   -- first day to backfill
DECLARE @ActorUserID   INT           = 1;
DECLARE @ActorUserName NVARCHAR(100) = N'data-fix: auto-grant visibility for new OrgUnits';
DECLARE @Commit        BIT           = 0;              -- 0 = preview + rollback, 1 = apply
DECLARE @Now           DATETIME      = GETDATE();

-- Targets: (OrgUnitID, parent to inherit grants from, creator), ordered top-down by OrgUnitType.
DECLARE @Targets TABLE (Seq INT IDENTITY(1,1) PRIMARY KEY, OrgUnitID INT, ParentID INT NULL,
                        CreatedBy INT NULL, Reason NVARCHAR(40));

INSERT INTO @Targets (OrgUnitID, ParentID, CreatedBy, Reason)
SELECT OrgUnitID, ParentID, CreatedBy, Reason
FROM (
    SELECT o.OrgUnitID, o.ParentOrgUnitID AS ParentID, o.CreatedBy, N'created' AS Reason, o.OrgUnitType AS SortType
    FROM   EMS.OrgUnit o
    WHERE  o.CreatedDate >= @Since AND o.IsActive = 1 AND o.IsDeleted = 0
    UNION ALL
    SELECT r.DivisionID, r.SubCommitteeID, r.CreatedBy, N'linked', 2
    FROM   EMS.OrgUnitRelationship r
    JOIN   EMS.OrgUnit d ON d.OrgUnitID = r.DivisionID AND d.IsActive = 1 AND d.IsDeleted = 0
    WHERE  r.IsActive = 1 AND r.CreatedDate >= @Since
    UNION ALL
    -- departments of a newly linked Division inherit from that Division
    SELECT dep.OrgUnitID, dep.ParentOrgUnitID, r.CreatedBy, N'linked dept', 3
    FROM   EMS.OrgUnitRelationship r
    JOIN   EMS.OrgUnit dep ON dep.ParentOrgUnitID = r.DivisionID AND dep.IsActive = 1 AND dep.IsDeleted = 0
    WHERE  r.IsActive = 1 AND r.CreatedDate >= @Since
) t
ORDER BY SortType, OrgUnitID;

PRINT '== TARGETS ==';
SELECT t.Seq, t.OrgUnitID, o.OrgUnitCode, o.OrgUnitNameTH, o.OrgUnitType, t.ParentID,
       p.OrgUnitNameTH AS ParentName, t.CreatedBy, t.Reason
FROM   @Targets t
JOIN   EMS.OrgUnit o ON o.OrgUnitID = t.OrgUnitID
LEFT JOIN EMS.OrgUnit p ON p.OrgUnitID = t.ParentID
ORDER BY t.Seq;

DECLARE @Granted TABLE (UserID INT, OrgUnitID INT, ParentID INT NULL, PRIMARY KEY (UserID, OrgUnitID));

BEGIN TRANSACTION;

    DECLARE @Seq INT = 1, @MaxSeq INT = (SELECT ISNULL(MAX(Seq), 0) FROM @Targets);
    DECLARE @OrgUnitID INT, @ParentID INT, @CreatedBy INT;

    WHILE @Seq <= @MaxSeq
    BEGIN
        SELECT @OrgUnitID = OrgUnitID, @ParentID = ParentID, @CreatedBy = CreatedBy
        FROM   @Targets WHERE Seq = @Seq;

        INSERT INTO security.UserOrgVisibility (UserID, OrgUnitID, GrantedBy, GrantedDate)
        OUTPUT inserted.UserID, inserted.OrgUnitID, @ParentID INTO @Granted (UserID, OrgUnitID, ParentID)
        SELECT r.UserID, @OrgUnitID, @ActorUserID, @Now
        FROM (
            SELECT v.UserID FROM security.UserOrgVisibility v WHERE @ParentID IS NOT NULL AND v.OrgUnitID = @ParentID
            UNION
            SELECT @CreatedBy WHERE @CreatedBy IS NOT NULL
                AND EXISTS (SELECT 1 FROM security.UserOrgVisibility v WHERE v.UserID = @CreatedBy)
        ) r
        WHERE NOT EXISTS (SELECT 1 FROM security.UserOrgVisibility x
                          WHERE x.UserID = r.UserID AND x.OrgUnitID = @OrgUnitID);

        SET @Seq += 1;
    END

    INSERT INTO audit.AuditLog (AuditDate, UserID, UserName, Module, Action, TableName, RecordID, NewValue, Result)
    SELECT @Now, @ActorUserID, @ActorUserName, 'UserOrgVisibility', 'GRANT', 'security.UserOrgVisibility', g.UserID,
           CONCAT(N'Data fix: auto-granted OrgUnit ', g.OrgUnitID, N' visibility to User ', g.UserID,
                  N' (new/linked under OrgUnit ', ISNULL(CAST(g.ParentID AS NVARCHAR(20)), N'-'), N').'), 1
    FROM   @Granted g;

    PRINT '== GRANTS ADDED ==';
    SELECT g.UserID, u.ADUserName, g.OrgUnitID, o.OrgUnitNameTH, g.ParentID
    FROM   @Granted g
    JOIN   EMS.OrgUnit o ON o.OrgUnitID = g.OrgUnitID
    LEFT JOIN EMS.UserAccount u ON u.UserID = g.UserID
    ORDER BY g.OrgUnitID, g.UserID;

IF @Commit = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT 'COMMITTED.';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT 'ROLLED BACK (preview only). Set @Commit = 1 to apply.';
END
