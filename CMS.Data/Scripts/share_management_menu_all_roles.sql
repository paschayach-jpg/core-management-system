-- ============================================================
-- Show the "จัดการการแชร์" (ShareManagement) menu to every role, so Officers can see and
-- revoke the shares they created. ShareService already limits non-admins to their own shares.
--
-- Menu visibility rule (CMS.Services/MenuService.cs): IsActive = 1 AND
--   (RequiredAppRole IS NULL OR RequiredAppRole <= user's AppRole), checked for every ancestor too.
--
-- What it does:
--   1. ShareManagement row(s) exist  → RequiredAppRole = NULL, IsActive = 1.
--      No row                        → insert one next to "แชร์กับฉัน" (SharedWithMe), or at root.
--   2. Ancestor groups that still hide it (RequiredAppRole > 1) are opened as well, but ONLY pure
--      group nodes (no FormClassName). An ancestor that opens a form is left alone and reported,
--      so this never exposes another screen to Officers.
--
-- Rename with the next schema script number before committing. Safe to re-run.
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- ── Rows that FolderForm resolves to the "ShareManagement" action ───────────
DECLARE @Targets TABLE (MenuItemID INT PRIMARY KEY);

INSERT INTO @Targets (MenuItemID)
SELECT MenuItemID
FROM   EMS.MenuItem
WHERE  LOWER(REPLACE(REPLACE(ISNULL(FormClassName, ''), '_', ''), ' ', '')) IN ('sharemanagement', 'sharemanagementform', 'manageshare')
   OR  LOWER(ISNULL(MenuKey, '')) IN ('share_management', 'sharemanagement', 'manage_share', 'manageshare')
   OR  MenuName LIKE N'%ลิงก์แชร์%'
   OR  MenuName LIKE N'%share management%'
   OR  MenuName LIKE N'%manage share%';

PRINT '--- Before ---';
SELECT m.MenuItemID, m.MenuName, m.MenuKey, m.FormClassName, m.RequiredAppRole, m.IsActive, m.ParentMenuItemID
FROM   EMS.MenuItem m
JOIN   @Targets t ON t.MenuItemID = m.MenuItemID;

-- ── 1a. Insert when missing ────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM @Targets)
BEGIN
    DECLARE @ParentID INT, @DisplayOrder INT, @ModuleCode NVARCHAR(50), @CreatedBy INT;

    -- Same group as "แชร์กับฉัน" when it exists
    SELECT TOP 1
           @ParentID     = ParentMenuItemID,
           @DisplayOrder = DisplayOrder + 1,
           @ModuleCode   = ModuleCode
    FROM   EMS.MenuItem
    WHERE  LOWER(REPLACE(REPLACE(ISNULL(FormClassName, ''), '_', ''), ' ', '')) IN ('sharedwithme', 'sharedwithmeform')
       OR  LOWER(ISNULL(MenuKey, '')) IN ('shared_with_me', 'sharedwithme')
    ORDER  BY IsActive DESC, MenuItemID;

    IF @DisplayOrder IS NULL
        SELECT @DisplayOrder = ISNULL(MAX(DisplayOrder), 0) + 1 FROM EMS.MenuItem WHERE ParentMenuItemID IS NULL;

    SELECT TOP 1 @CreatedBy = UserID FROM EMS.UserAccount WHERE AppRole = 4 ORDER BY UserID;
    IF @CreatedBy IS NULL SET @CreatedBy = 1;

    INSERT INTO EMS.MenuItem
        (ParentMenuItemID, MenuName, MenuKey, FormClassName, IconKey, DisplayOrder, IsActive,
         RequiredAppRole, Description, ModuleCode, Url, CreatedDate, CreatedByUserID)
    VALUES
        (@ParentID, N'📤 จัดการการแชร์', 'share_management', 'ShareManagementForm', NULL, @DisplayOrder, 1,
         NULL, N'ดู/ยกเลิก/ปิดชั่วคราว รายการที่ฉันแชร์ให้ผู้อื่น (Admin เห็นทั้งหมด)', @ModuleCode, NULL, GETDATE(), @CreatedBy);

    INSERT INTO @Targets (MenuItemID) VALUES (SCOPE_IDENTITY());
    PRINT 'Inserted ShareManagement menu row';
END
ELSE
BEGIN
    -- ── 1b. Open existing rows to all roles ─────────────────────────────────
    UPDATE m
    SET    m.RequiredAppRole = NULL,
           m.IsActive        = 1,
           m.ModifiedDate    = GETDATE()
    FROM   EMS.MenuItem m
    JOIN   @Targets t ON t.MenuItemID = m.MenuItemID
    WHERE  m.RequiredAppRole IS NOT NULL OR m.IsActive = 0;

    PRINT CONCAT('Opened ShareManagement rows: ', @@ROWCOUNT);
END

-- ── 2. Ancestors that still hide the menu from Officers ────────────────────
;WITH Ancestors AS
(
    SELECT p.MenuItemID, p.ParentMenuItemID, p.MenuName, p.FormClassName, p.RequiredAppRole, p.IsActive
    FROM   EMS.MenuItem c
    JOIN   @Targets t      ON t.MenuItemID = c.MenuItemID
    JOIN   EMS.MenuItem p  ON p.MenuItemID = c.ParentMenuItemID
    UNION ALL
    SELECT p.MenuItemID, p.ParentMenuItemID, p.MenuName, p.FormClassName, p.RequiredAppRole, p.IsActive
    FROM   Ancestors a
    JOIN   EMS.MenuItem p ON p.MenuItemID = a.ParentMenuItemID
)
SELECT DISTINCT MenuItemID, MenuName, FormClassName, RequiredAppRole, IsActive
INTO   #Ancestors
FROM   Ancestors;

UPDATE m
SET    m.RequiredAppRole = NULL,
       m.IsActive        = 1,
       m.ModifiedDate    = GETDATE()
FROM   EMS.MenuItem m
JOIN   #Ancestors a ON a.MenuItemID = m.MenuItemID
WHERE  (m.RequiredAppRole > 1 OR m.IsActive = 0)
  AND  NULLIF(LTRIM(RTRIM(m.FormClassName)), '') IS NULL;

PRINT CONCAT('Opened ancestor group rows: ', @@ROWCOUNT);

IF EXISTS (SELECT 1 FROM #Ancestors a JOIN EMS.MenuItem m ON m.MenuItemID = a.MenuItemID
           WHERE (m.RequiredAppRole > 1 OR m.IsActive = 0))
BEGIN
    PRINT 'WARNING: some ancestors still hide the menu (they open a form, so they were not changed).';
    PRINT '         Move the ShareManagement row under a group visible to Officers via Menu Management.';
    SELECT m.MenuItemID, m.MenuName, m.FormClassName, m.RequiredAppRole, m.IsActive
    FROM   #Ancestors a JOIN EMS.MenuItem m ON m.MenuItemID = a.MenuItemID
    WHERE  m.RequiredAppRole > 1 OR m.IsActive = 0;
END

DROP TABLE #Ancestors;

PRINT '--- After ---';
SELECT m.MenuItemID, m.MenuName, m.MenuKey, m.FormClassName, m.RequiredAppRole, m.IsActive,
       m.ParentMenuItemID, p.MenuName AS ParentName, p.RequiredAppRole AS ParentRole
FROM   EMS.MenuItem m
JOIN   @Targets t ON t.MenuItemID = m.MenuItemID
LEFT JOIN EMS.MenuItem p ON p.MenuItemID = m.ParentMenuItemID;

COMMIT TRANSACTION;
