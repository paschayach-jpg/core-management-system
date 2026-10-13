-- ============================================================
-- fix_duplicate_division_to_relationship.sql
--
-- Converts a duplicate Division row into a SubCommittee link (EMS.OrgUnitRelationship).
--
-- Background: before the relationship fix, adding an HR Division that already existed under
-- another SubCommittee inserted a second EMS.OrgUnit row with a generated code (DIV-xxx),
-- because OrgUnitCode must be unique. Example (13/10/2026):
--   OrgUnitID 123  10006    ทรัพยากรบุคคล  under SC 86 (คณะกรรมการฝ่ายบริหาร)   ← keep (HR code, has folders)
--   OrgUnitID 240  DIV-034  ทรัพยากรบุคคล  under SC 76 (คณะกรรมการลงทุน)         ← duplicate
--   OrgUnitID 241  DEPT-097 บุคคล          under 240                            ← duplicate of 124 under 123
--
-- What it does (one transaction):
--   1. Links @KeepDivisionID to the duplicate's SubCommittee in EMS.OrgUnitRelationship.
--   2. Departments under the duplicate: soft-deletes the ones the kept Division already has
--      (same code or name); re-parents the rest to the kept Division.
--   3. Soft-deletes the duplicate Division (IsDeleted = 1, IsActive = 0).
--   4. Writes audit.AuditLog rows for every change.
--
-- Safety:
--   * Refuses to run if the duplicate (or its departments) is referenced by folders, quota
--     config/requests/usage or user visibility grants — those need a manual decision.
--   * Previews before/after state and ROLLS BACK unless @Commit = 1.
--
-- Usage: review the parameters, run once with @Commit = 0, check the output, then set @Commit = 1.
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @DuplicateDivisionID INT           = 240;
DECLARE @KeepDivisionID      INT           = 123;
DECLARE @ActorUserID         INT           = 1;
DECLARE @ActorUserName       NVARCHAR(100) = N'data-fix: duplicate division → relationship';
DECLARE @Commit              BIT           = 0;   -- 0 = preview + rollback, 1 = apply

DECLARE @SubCommitteeID INT;
DECLARE @Now DATETIME = GETDATE();

-- ── Validation ────────────────────────────────────────────────────────────
SELECT @SubCommitteeID = ParentOrgUnitID
FROM   EMS.OrgUnit
WHERE  OrgUnitID = @DuplicateDivisionID AND OrgUnitType = 2 AND IsDeleted = 0;

IF @SubCommitteeID IS NULL
    THROW 50001, 'Duplicate must be a non-deleted Division (OrgUnitType = 2) that has a SubCommittee parent.', 1;

IF NOT EXISTS (SELECT 1 FROM EMS.OrgUnit WHERE OrgUnitID = @SubCommitteeID AND OrgUnitType = 1 AND IsDeleted = 0)
    THROW 50002, 'Parent of the duplicate is not an active SubCommittee.', 1;

IF NOT EXISTS (SELECT 1 FROM EMS.OrgUnit
               WHERE OrgUnitID = @KeepDivisionID AND OrgUnitType = 2 AND IsDeleted = 0 AND IsActive = 1)
    THROW 50003, 'Kept Division must be an active, non-deleted Division.', 1;

IF @KeepDivisionID = @DuplicateDivisionID
    THROW 50004, 'Duplicate and kept Division must be different rows.', 1;

IF EXISTS (SELECT 1 FROM EMS.OrgUnit WHERE OrgUnitID = @KeepDivisionID AND ParentOrgUnitID = @SubCommitteeID)
    THROW 50005, 'Kept Division is already the direct child of that SubCommittee; nothing to link.', 1;

IF NOT EXISTS (SELECT 1
               FROM EMS.OrgUnit d JOIN EMS.OrgUnit k ON k.OrgUnitID = @KeepDivisionID
               WHERE d.OrgUnitID = @DuplicateDivisionID
                 AND LTRIM(RTRIM(d.OrgUnitNameTH)) = LTRIM(RTRIM(k.OrgUnitNameTH)))
    THROW 50006, 'Duplicate and kept Division have different names (OrgUnitNameTH); check the IDs.', 1;

DECLARE @Subtree TABLE (OrgUnitID INT PRIMARY KEY);
INSERT INTO @Subtree (OrgUnitID)
SELECT OrgUnitID FROM EMS.OrgUnit
WHERE  OrgUnitID = @DuplicateDivisionID
   OR (ParentOrgUnitID = @DuplicateDivisionID AND IsDeleted = 0);

IF EXISTS (SELECT 1 FROM storage.Folder              WHERE OrgUnitID IN (SELECT OrgUnitID FROM @Subtree))
OR EXISTS (SELECT 1 FROM security.UserOrgVisibility  WHERE OrgUnitID IN (SELECT OrgUnitID FROM @Subtree))
OR EXISTS (SELECT 1 FROM dms.FolderQuotaConfig       WHERE OrgUnitID IN (SELECT OrgUnitID FROM @Subtree)
                                                        OR ParentOrgUnitID IN (SELECT OrgUnitID FROM @Subtree))
OR EXISTS (SELECT 1 FROM dms.FolderQuotaChangeRequest WHERE OrgUnitID IN (SELECT OrgUnitID FROM @Subtree))
OR EXISTS (SELECT 1 FROM dms.FolderQuotaUsageLog     WHERE OrgUnitID IN (SELECT OrgUnitID FROM @Subtree))
    THROW 50007, 'Duplicate Division or its departments are referenced by folders, quota or visibility rows; resolve those manually first.', 1;

-- ── Plan for departments under the duplicate ─────────────────────────────
DECLARE @DeptPlan TABLE (OrgUnitID INT PRIMARY KEY, OrgUnitCode NVARCHAR(50), OrgUnitNameTH NVARCHAR(200),
                         PlanAction VARCHAR(20), MatchesOrgUnitID INT NULL);

INSERT INTO @DeptPlan (OrgUnitID, OrgUnitCode, OrgUnitNameTH, PlanAction, MatchesOrgUnitID)
SELECT d.OrgUnitID, d.OrgUnitCode, d.OrgUnitNameTH,
       CASE WHEN m.OrgUnitID IS NULL THEN 'REPARENT' ELSE 'SOFT_DELETE' END,
       m.OrgUnitID
FROM   EMS.OrgUnit d
OUTER APPLY (SELECT TOP 1 k.OrgUnitID
             FROM   EMS.OrgUnit k
             WHERE  k.ParentOrgUnitID = @KeepDivisionID AND k.IsDeleted = 0
               AND (k.OrgUnitCode = d.OrgUnitCode
                    OR LTRIM(RTRIM(k.OrgUnitNameTH)) = LTRIM(RTRIM(d.OrgUnitNameTH)))) m
WHERE  d.ParentOrgUnitID = @DuplicateDivisionID AND d.IsDeleted = 0;

-- ── Preview (before) ─────────────────────────────────────────────────────
PRINT '== BEFORE ==';
SELECT 'before' AS [state], o.OrgUnitID, o.OrgUnitCode, o.OrgUnitNameTH, o.OrgUnitType,
       o.ParentOrgUnitID, p.OrgUnitNameTH AS ParentName, o.IsActive, o.IsDeleted
FROM   EMS.OrgUnit o LEFT JOIN EMS.OrgUnit p ON p.OrgUnitID = o.ParentOrgUnitID
WHERE  o.OrgUnitID IN (@DuplicateDivisionID, @KeepDivisionID, @SubCommitteeID)
   OR  o.ParentOrgUnitID IN (@DuplicateDivisionID, @KeepDivisionID)
ORDER BY o.OrgUnitType, o.ParentOrgUnitID, o.OrgUnitID;

SELECT 'before' AS [state], * FROM EMS.OrgUnitRelationship
WHERE  DivisionID IN (@DuplicateDivisionID, @KeepDivisionID);

SELECT 'department plan' AS [state], * FROM @DeptPlan;

-- ── Apply ────────────────────────────────────────────────────────────────
BEGIN TRANSACTION;

    -- 1. Link the kept Division to the SubCommittee
    IF EXISTS (SELECT 1 FROM EMS.OrgUnitRelationship
               WHERE SubCommitteeID = @SubCommitteeID AND DivisionID = @KeepDivisionID)
        UPDATE EMS.OrgUnitRelationship
        SET    IsActive = 1
        WHERE  SubCommitteeID = @SubCommitteeID AND DivisionID = @KeepDivisionID;
    ELSE
        INSERT INTO EMS.OrgUnitRelationship (SubCommitteeID, DivisionID, IsActive, CreatedBy, CreatedDate)
        VALUES (@SubCommitteeID, @KeepDivisionID, 1, @ActorUserID, @Now);

    INSERT INTO audit.AuditLog (AuditDate, UserID, UserName, Module, Action, TableName, RecordID, NewValue, Result)
    VALUES (@Now, @ActorUserID, @ActorUserName, 'OrgStructure', 'LINK', 'EMS.OrgUnitRelationship', @KeepDivisionID,
            CONCAT(N'Data fix: linked Division ', @KeepDivisionID, N' to SubCommittee ', @SubCommitteeID,
                   N' (replaces duplicate Division ', @DuplicateDivisionID, N').'), 1);

    -- 2a. Departments the kept Division already has → soft-delete the duplicate copy
    UPDATE o
    SET    IsDeleted = 1, IsActive = 0, DeletedDate = @Now, DeletedBy = @ActorUserID
    FROM   EMS.OrgUnit o JOIN @DeptPlan dp ON dp.OrgUnitID = o.OrgUnitID
    WHERE  dp.PlanAction = 'SOFT_DELETE';

    INSERT INTO audit.AuditLog (AuditDate, UserID, UserName, Module, Action, TableName, RecordID, OldValue, NewValue, Result)
    SELECT @Now, @ActorUserID, @ActorUserName, 'OrgStructure', 'DELETE', 'EMS.OrgUnit', dp.OrgUnitID,
           CONCAT(N'ParentOrgUnitID=', @DuplicateDivisionID),
           CONCAT(N'Data fix: soft-deleted duplicate department ''', dp.OrgUnitNameTH, N''' (', dp.OrgUnitCode,
                  N'); same as OrgUnitID ', dp.MatchesOrgUnitID, N' under Division ', @KeepDivisionID, N'.'), 1
    FROM   @DeptPlan dp WHERE dp.PlanAction = 'SOFT_DELETE';

    -- 2b. Departments the kept Division does not have → move them under it
    UPDATE o
    SET    ParentOrgUnitID = @KeepDivisionID
    FROM   EMS.OrgUnit o JOIN @DeptPlan dp ON dp.OrgUnitID = o.OrgUnitID
    WHERE  dp.PlanAction = 'REPARENT';

    INSERT INTO audit.AuditLog (AuditDate, UserID, UserName, Module, Action, TableName, RecordID, OldValue, NewValue, Result)
    SELECT @Now, @ActorUserID, @ActorUserName, 'OrgStructure', 'MOVE', 'EMS.OrgUnit', dp.OrgUnitID,
           CONCAT(N'ParentOrgUnitID=', @DuplicateDivisionID),
           CONCAT(N'Data fix: moved department ''', dp.OrgUnitNameTH, N''' to Division ', @KeepDivisionID, N'.'), 1
    FROM   @DeptPlan dp WHERE dp.PlanAction = 'REPARENT';

    -- 3. Soft-delete the duplicate Division
    UPDATE EMS.OrgUnit
    SET    IsDeleted = 1, IsActive = 0, DeletedDate = @Now, DeletedBy = @ActorUserID
    WHERE  OrgUnitID = @DuplicateDivisionID;

    INSERT INTO audit.AuditLog (AuditDate, UserID, UserName, Module, Action, TableName, RecordID, OldValue, NewValue, Result)
    VALUES (@Now, @ActorUserID, @ActorUserName, 'OrgStructure', 'DELETE', 'EMS.OrgUnit', @DuplicateDivisionID,
            CONCAT(N'ParentOrgUnitID=', @SubCommitteeID),
            CONCAT(N'Data fix: soft-deleted duplicate Division; now linked as Division ', @KeepDivisionID,
                   N' via EMS.OrgUnitRelationship.'), 1);

    -- ── Preview (after, inside the transaction) ─────────────────────────────
    PRINT '== AFTER ==';
    SELECT 'after' AS [state], o.OrgUnitID, o.OrgUnitCode, o.OrgUnitNameTH, o.OrgUnitType,
           o.ParentOrgUnitID, p.OrgUnitNameTH AS ParentName, o.IsActive, o.IsDeleted
    FROM   EMS.OrgUnit o LEFT JOIN EMS.OrgUnit p ON p.OrgUnitID = o.ParentOrgUnitID
    WHERE  o.OrgUnitID IN (@DuplicateDivisionID, @KeepDivisionID, @SubCommitteeID)
       OR  o.ParentOrgUnitID IN (@DuplicateDivisionID, @KeepDivisionID)
       OR  o.OrgUnitID IN (SELECT OrgUnitID FROM @DeptPlan)
    ORDER BY o.OrgUnitType, o.ParentOrgUnitID, o.OrgUnitID;

    SELECT 'after' AS [state], * FROM EMS.OrgUnitRelationship
    WHERE  DivisionID IN (@DuplicateDivisionID, @KeepDivisionID);

    SELECT 'after' AS [state], AuditID, Action, TableName, RecordID, NewValue
    FROM   audit.AuditLog WHERE AuditDate = @Now AND UserName = @ActorUserName;

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
