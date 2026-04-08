-- ============================================================================
-- MENU SYSTEM DATABASE SCHEMA
-- ============================================================================
-- Tables and stored procedures for menu management system
-- Requirements: Dynamic menu storage with role-based visibility
-- ============================================================================

-- ────────────────────────────────────────────────────────────────────────────
-- 1. CREATE MenuItem TABLE
-- ────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MenuItem' AND schema_id = SCHEMA_ID('core'))
BEGIN
    CREATE TABLE core.MenuItem
    (
        MenuItemID            INT PRIMARY KEY IDENTITY(1,1),
        ParentMenuItemID      INT NULL,
        MenuName              NVARCHAR(255) NOT NULL,
        MenuKey               NVARCHAR(100) UNIQUE NOT NULL,
        FormClassName         NVARCHAR(500) NULL,
        IconKey               NVARCHAR(100) NULL,
        DisplayOrder          INT NOT NULL DEFAULT 0,
        IsActive              BIT NOT NULL DEFAULT 1,
        RequiredAppRole       INT NULL,  -- 1=User, 2=Manager, 3=Approver, 4=Admin. NULL = all roles
        Description           NVARCHAR(500) NULL,
        ModuleCode            NVARCHAR(50) NULL,
        Url                   NVARCHAR(500) NULL,
        CreatedDate           DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedByUserID       INT NOT NULL,
        ModifiedDate          DATETIME NULL,
        ModifiedByUserID      INT NULL,
        CONSTRAINT FK_MenuItem_Parent FOREIGN KEY (ParentMenuItemID) 
            REFERENCES core.MenuItem(MenuItemID) ON DELETE CASCADE,
        CONSTRAINT FK_MenuItem_CreatedBy FOREIGN KEY (CreatedByUserID) 
            REFERENCES core.UserAccount(UserID)
    );

    CREATE INDEX IX_MenuItem_ParentID ON core.MenuItem(ParentMenuItemID);
    CREATE INDEX IX_MenuItem_MenuKey ON core.MenuItem(MenuKey);
    CREATE INDEX IX_MenuItem_IsActive ON core.MenuItem(IsActive);
    CREATE INDEX IX_MenuItem_DisplayOrder ON core.MenuItem(DisplayOrder);
    CREATE INDEX IX_MenuItem_RequiredRole ON core.MenuItem(RequiredAppRole);
    
    PRINT 'MenuItem table created successfully.';
END

-- ────────────────────────────────────────────────────────────────────────────
-- 2. CREATE STORED PROCEDURES FOR CRUD OPERATIONS
-- ────────────────────────────────────────────────────────────────────────────

-- Create MenuItem
IF OBJECT_ID('core.sp_CreateMenuItem', 'P') IS NOT NULL
    DROP PROCEDURE core.sp_CreateMenuItem;
GO

CREATE PROCEDURE core.sp_CreateMenuItem
    @ParentMenuItemID     INT = NULL,
    @MenuName             NVARCHAR(255),
    @MenuKey              NVARCHAR(100),
    @FormClassName        NVARCHAR(500) = NULL,
    @IconKey              NVARCHAR(100) = NULL,
    @DisplayOrder         INT = 0,
    @IsActive             BIT = 1,
    @RequiredAppRole      INT = NULL,
    @Description          NVARCHAR(500) = NULL,
    @ModuleCode           NVARCHAR(50) = NULL,
    @Url                  NVARCHAR(500) = NULL,
    @CreatedByUserID      INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate inputs
    IF @MenuName IS NULL OR LTRIM(RTRIM(@MenuName)) = ''
        THROW 50001, 'MenuName is required.', 1;
    
    IF @MenuKey IS NULL OR LTRIM(RTRIM(@MenuKey)) = ''
        THROW 50001, 'MenuKey is required.', 1;

    -- Check key uniqueness
    IF EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = @MenuKey)
        THROW 50001, 'MenuKey already exists.', 1;

    -- Insert new menu item
    INSERT INTO core.MenuItem
        (ParentMenuItemID, MenuName, MenuKey, FormClassName, IconKey, DisplayOrder, 
         IsActive, RequiredAppRole, Description, ModuleCode, Url, CreatedByUserID, CreatedDate)
    VALUES
        (@ParentMenuItemID, @MenuName, @MenuKey, @FormClassName, @IconKey, @DisplayOrder,
         @IsActive, @RequiredAppRole, @Description, @ModuleCode, @Url, @CreatedByUserID, GETDATE());

    -- Return the new ID
    SELECT @@IDENTITY AS MenuItemID;
END
GO

PRINT 'sp_CreateMenuItem created.';

-- Update MenuItem
IF OBJECT_ID('core.sp_UpdateMenuItem', 'P') IS NOT NULL
    DROP PROCEDURE core.sp_UpdateMenuItem;
GO

CREATE PROCEDURE core.sp_UpdateMenuItem
    @MenuItemID           INT,
    @ParentMenuItemID     INT = NULL,
    @MenuName             NVARCHAR(255),
    @MenuKey              NVARCHAR(100),
    @FormClassName        NVARCHAR(500) = NULL,
    @IconKey              NVARCHAR(100) = NULL,
    @DisplayOrder         INT = 0,
    @IsActive             BIT = 1,
    @RequiredAppRole      INT = NULL,
    @Description          NVARCHAR(500) = NULL,
    @ModuleCode           NVARCHAR(50) = NULL,
    @Url                  NVARCHAR(500) = NULL,
    @ModifiedByUserID     INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate inputs
    IF @MenuName IS NULL OR LTRIM(RTRIM(@MenuName)) = ''
        THROW 50001, 'MenuName is required.', 1;

    -- Check if menu exists
    IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuItemID = @MenuItemID)
        THROW 50001, 'MenuItem not found.', 1;

    -- Check key uniqueness (excluding current item)
    IF EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = @MenuKey AND MenuItemID <> @MenuItemID)
        THROW 50001, 'MenuKey already exists.', 1;

    -- Update menu item
    UPDATE core.MenuItem
    SET
        ParentMenuItemID = @ParentMenuItemID,
        MenuName = @MenuName,
        MenuKey = @MenuKey,
        FormClassName = @FormClassName,
        IconKey = @IconKey,
        DisplayOrder = @DisplayOrder,
        IsActive = @IsActive,
        RequiredAppRole = @RequiredAppRole,
        Description = @Description,
        ModuleCode = @ModuleCode,
        Url = @Url,
        ModifiedDate = GETDATE(),
        ModifiedByUserID = @ModifiedByUserID
    WHERE MenuItemID = @MenuItemID;
END
GO

PRINT 'sp_UpdateMenuItem created.';

-- Delete MenuItem (cascades to children)
IF OBJECT_ID('core.sp_DeleteMenuItem', 'P') IS NOT NULL
    DROP PROCEDURE core.sp_DeleteMenuItem;
GO

CREATE PROCEDURE core.sp_DeleteMenuItem
    @MenuItemID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if menu exists
    IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuItemID = @MenuItemID)
        THROW 50001, 'MenuItem not found.', 1;

    -- Delete will cascade due to foreign key constraint
    DELETE FROM core.MenuItem
    WHERE MenuItemID = @MenuItemID;
END
GO

PRINT 'sp_DeleteMenuItem created.';

-- ────────────────────────────────────────────────────────────────────────────
-- 3. INSERT SAMPLE DATA
-- ────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = 'HOME')
BEGIN
    DECLARE @AdminUserID INT = 1; -- Adjust to your admin user ID

    -- Home Menu (root)
    INSERT INTO core.MenuItem (ParentMenuItemID, MenuName, MenuKey, DisplayOrder, IsActive, RequiredAppRole, Description, ModuleCode, CreatedByUserID)
    VALUES (NULL, 'หน้าแรก', 'HOME', 1, 1, 1, 'หน้าแรกของระบบ', 'Home', @AdminUserID);

    SELECT @AdminUserID = @@IDENTITY;

    -- File Management Menu
    INSERT INTO core.MenuItem (ParentMenuItemID, MenuName, MenuKey, FormClassName, DisplayOrder, IsActive, RequiredAppRole, Description, ModuleCode, CreatedByUserID)
    VALUES (NULL, 'จัดการไฟล์', 'FILE_MANAGEMENT', 'CMS.Presentation.Modules.ImportFile.FileManagementForm', 2, 1, 1, 'จัดการไฟล์และโฟลเดอร์', 'FileManagement', @AdminUserID);

    -- User Management Menu (Admin only)
    INSERT INTO core.MenuItem (ParentMenuItemID, MenuName, MenuKey, FormClassName, DisplayOrder, IsActive, RequiredAppRole, Description, ModuleCode, CreatedByUserID)
    VALUES (NULL, 'จัดการผู้ใช้', 'USER_MANAGEMENT', 'CMS.Presentation.Modules.ImportFile.UserManagementForm', 3, 1, 4, 'จัดการบัญชีผู้ใช้ระบบ', 'UserManagement', @AdminUserID);

    -- Audit Log Menu
    INSERT INTO core.MenuItem (ParentMenuItemID, MenuName, MenuKey, FormClassName, DisplayOrder, IsActive, RequiredAppRole, Description, ModuleCode, CreatedByUserID)
    VALUES (NULL, 'บันทึกการตรวจสอบ', 'AUDIT_LOG', 'CMS.Presentation.Modules.ImportFile.AuditLogForm', 4, 1, 4, 'ดูบันทึกการทำงานของระบบ', 'AuditLog', @AdminUserID);

    PRINT 'Sample menu items inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Sample data already exists.';
END

-- ────────────────────────────────────────────────────────────────────────────
-- 4. VERIFICATION QUERIES
-- ────────────────────────────────────────────────────────────────────────────

-- View all menu items
SELECT 'Menu Items:' AS Status;
SELECT MenuItemID, ParentMenuItemID, MenuName, MenuKey, DisplayOrder, IsActive, RequiredAppRole 
FROM core.MenuItem 
ORDER BY DisplayOrder;

-- View menu hierarchy
SELECT 'Menu Hierarchy:' AS Status;
SELECT 
    REPLICATE('  ', CASE WHEN ParentMenuItemID IS NULL THEN 0 ELSE 1 END) + MenuName AS 'Menu Name',
    MenuKey,
    DisplayOrder,
    CASE IsActive WHEN 1 THEN 'Active' ELSE 'Inactive' END AS 'Status'
FROM core.MenuItem 
ORDER BY ParentMenuItemID, DisplayOrder;
