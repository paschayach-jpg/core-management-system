-- ============================================================
-- 012_Cleanup_DuplicatePermissions.sql
-- Remove duplicate FolderPermission entries, keeping only the
-- most recently granted entry per (FolderID, PrincipalType, PrincipalID, IsDeny)
-- ============================================================

-- Preview duplicates before cleanup
SELECT FolderID, PrincipalType, PrincipalID, IsDeny, COUNT(*) AS DuplicateCount
FROM   security.FolderPermission
WHERE  RevokedDate IS NULL
GROUP  BY FolderID, PrincipalType, PrincipalID, IsDeny
HAVING COUNT(*) > 1;

-- Revoke older duplicates, keep the latest GrantedDate per group
UPDATE security.FolderPermission
SET    RevokedBy    = 1,
       RevokedDate  = GETDATE(),
       RevokeReason = 'Duplicate cleanup — superseded by newer entry'
WHERE  PermissionID NOT IN (
    -- Keep the latest entry per group
    SELECT MAX(PermissionID)
    FROM   security.FolderPermission
    WHERE  RevokedDate IS NULL
    GROUP  BY FolderID, PrincipalType, PrincipalID, IsDeny
)
AND RevokedDate IS NULL;

-- Verify result
SELECT FolderID, PrincipalType, PrincipalID, IsDeny, COUNT(*) AS Count
FROM   security.FolderPermission
WHERE  RevokedDate IS NULL
GROUP  BY FolderID, PrincipalType, PrincipalID, IsDeny
HAVING COUNT(*) > 1;
-- Should return 0 rows if cleanup was successful
