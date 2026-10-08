-- Folder move across OrgUnits: sp_UpdateFolder never wrote OrgUnitID, so a folder moved under
-- another unit's conf-root kept its old OrgUnitID and vanished from both unit trees.
-- Rename with the next free script number before committing.
--
-- PART 1 (required by the app): stored procedure used by FolderRepository.UpdateOrgUnit.
-- Apply BEFORE deploying the new build, otherwise cross-OrgUnit moves fail with
-- "Could not find stored procedure 'storage.sp_UpdateFolderOrgUnit'" (same-unit moves are unaffected).

IF OBJECT_ID('storage.sp_UpdateFolderOrgUnit', 'P') IS NOT NULL
    DROP PROCEDURE storage.sp_UpdateFolderOrgUnit;
GO

CREATE PROCEDURE storage.sp_UpdateFolderOrgUnit
    @FolderID  INT,
    @OrgUnitID INT,
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE storage.Folder
    SET    OrgUnitID   = @OrgUnitID,
           UpdatedBy   = @UpdatedBy,
           UpdatedDate = GETDATE()
    WHERE  FolderID = @FolderID;
END
GO

-- PART 2 (one-off data repair): folder 464 "News" was moved to FolderID 436
-- (3NC root of OrgUnit 219 "ส่วนกลาง") but kept OrgUnitID 226, so it is visible nowhere.
-- This puts it back under FolderID 444 (3NC root of OrgUnit 226 "เทคโนโลยีสารสนเทศ").
-- Preview first; it ROLLS BACK unless @Commit = 1.

SET XACT_ABORT ON;
DECLARE @Commit BIT = 0;

BEGIN TRAN;

SELECT 'before' AS State, FolderID, FolderName, ParentFolderID, OrgUnitID, ConfidentialityLevel, FolderDepth, PhysicalPath
FROM storage.Folder WHERE FolderID = 464;

UPDATE storage.Folder
SET    ParentFolderID       = 444,
       OrgUnitID            = 226,
       ConfidentialityLevel = 3,
       FolderDepth          = 2,
       PhysicalPath         = N'\\150.1.8.23\AppForUser\Cmd_LmportFile\226\464\News',
       UpdatedDate          = GETDATE()
WHERE  FolderID = 464
  AND  FolderName = N'News'
  AND  ParentFolderID = 436;   -- guard: only if still in the broken state

SELECT 'after' AS State, FolderID, FolderName, ParentFolderID, OrgUnitID, ConfidentialityLevel, FolderDepth, PhysicalPath
FROM storage.Folder WHERE FolderID = 464;

IF @Commit = 1
BEGIN
    COMMIT TRAN;
    PRINT 'Committed.';
END
ELSE
BEGIN
    ROLLBACK TRAN;
    PRINT 'Rolled back (preview only). Set @Commit = 1 to apply.';
END
