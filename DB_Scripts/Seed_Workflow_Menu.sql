-- ============================================================================
-- WORKFLOW ENGINE (WFE) MENU ITEMS
-- ============================================================================
-- เพิ่มเมนูสำหรับระบบ Workflow Engine
-- ============================================================================

DECLARE @AdminUserID INT = 1; -- ปรับตาม UserID ของ Admin ในระบบ
DECLARE @WFEParentID INT;

-- ────────────────────────────────────────────────────────────────────────────
-- 1. สร้างเมนูหลัก WFE (Folder)
-- ────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = 'WFE')
BEGIN
    INSERT INTO core.MenuItem 
    (
        ParentMenuItemID, 
        MenuName, 
        MenuKey, 
        FormClassName,
        IconKey, 
        DisplayOrder, 
        IsActive, 
        RequiredAppRole, 
        Description, 
        ModuleCode, 
        CreatedByUserID,
        CreatedDate
    )
    VALUES 
    (
        NULL,                           -- ParentMenuItemID (root level)
        N'WFE',                         -- MenuName
        'WFE',                          -- MenuKey
        NULL,                           -- FormClassName (folder, no form)
        'folder',                       -- IconKey
        5,                              -- DisplayOrder
        1,                              -- IsActive
        1,                              -- RequiredAppRole (User and above)
        N'ระบบ Workflow Engine',       -- Description
        'WFE',                          -- ModuleCode
        @AdminUserID,                   -- CreatedByUserID
        GETDATE()                       -- CreatedDate
    );
    
    SET @WFEParentID = @@IDENTITY;
    PRINT 'WFE parent menu created with ID: ' + CAST(@WFEParentID AS NVARCHAR(10));
END
ELSE
BEGIN
    SELECT @WFEParentID = MenuItemID FROM core.MenuItem WHERE MenuKey = 'WFE';
    PRINT 'WFE parent menu already exists with ID: ' + CAST(@WFEParentID AS NVARCHAR(10));
END

-- ────────────────────────────────────────────────────────────────────────────
-- 2. สร้างเมนูย่อยภายใต้ WFE
-- ────────────────────────────────────────────────────────────────────────────

-- 2.1 รายงาน SLA
IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = 'WFE_SLA_REPORT')
BEGIN
    INSERT INTO core.MenuItem 
    (
        ParentMenuItemID, 
        MenuName, 
        MenuKey, 
        FormClassName,
        IconKey, 
        DisplayOrder, 
        IsActive, 
        RequiredAppRole, 
        Description, 
        ModuleCode, 
        CreatedByUserID,
        CreatedDate
    )
    VALUES 
    (
        @WFEParentID,
        N'รายงาน SLA',
        'WFE_SLA_REPORT',
        'CMS.Presentation.Modules.Workflow.WorkflowReportForm',
        'report',
        1,
        1,
        2,                              -- Manager and above
        N'รายงาน SLA ของ Workflow',
        'WFE',
        @AdminUserID,
        GETDATE()
    );
    PRINT 'รายงาน SLA menu created.';
END

-- 2.2 ตรวจสอบ Workflow
IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = 'WFE_DASHBOARD')
BEGIN
    INSERT INTO core.MenuItem 
    (
        ParentMenuItemID, 
        MenuName, 
        MenuKey, 
        FormClassName,
        IconKey, 
        DisplayOrder, 
        IsActive, 
        RequiredAppRole, 
        Description, 
        ModuleCode, 
        CreatedByUserID,
        CreatedDate
    )
    VALUES 
    (
        @WFEParentID,
        N'ตรวจสอบ Workflow',
        'WFE_DASHBOARD',
        'CMS.Presentation.Modules.Workflow.WorkflowDashboardForm',
        'dashboard',
        2,
        1,
        1,
        N'Dashboard สำหรับติดตาม Workflow',
        'WFE',
        @AdminUserID,
        GETDATE()
    );
    PRINT 'ตรวจสอบ Workflow menu created.';
END

-- 2.3 Delegation
IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = 'WFE_DELEGATION')
BEGIN
    INSERT INTO core.MenuItem 
    (
        ParentMenuItemID, 
        MenuName, 
        MenuKey, 
        FormClassName,
        IconKey, 
        DisplayOrder, 
        IsActive, 
        RequiredAppRole, 
        Description, 
        ModuleCode, 
        CreatedByUserID,
        CreatedDate
    )
    VALUES 
    (
        @WFEParentID,
        N'Delegation',
        'WFE_DELEGATION',
        'CMS.Presentation.Modules.Workflow.WorkflowDelegationForm',
        'delegate',
        3,
        1,
        1,
        N'มอบหมายงาน Workflow',
        'WFE',
        @AdminUserID,
        GETDATE()
    );
    PRINT 'Delegation menu created.';
END

-- 2.4 ออกแบบ Workflow
IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = 'WFE_DESIGNER')
BEGIN
    INSERT INTO core.MenuItem 
    (
        ParentMenuItemID, 
        MenuName, 
        MenuKey, 
        FormClassName,
        IconKey, 
        DisplayOrder, 
        IsActive, 
        RequiredAppRole, 
        Description, 
        ModuleCode, 
        CreatedByUserID,
        CreatedDate
    )
    VALUES 
    (
        @WFEParentID,
        N'ออกแบบ Workflow',
        'WFE_DESIGNER',
        'CMS.Presentation.Modules.Workflow.WorkflowDesignerForm',
        'design',
        4,
        1,
        4,                              -- Admin only
        N'ออกแบบ Workflow Template',
        'WFE',
        @AdminUserID,
        GETDATE()
    );
    PRINT 'ออกแบบ Workflow menu created.';
END

-- 2.5 รายละเอียด Workflow
IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = 'WFE_DETAIL')
BEGIN
    INSERT INTO core.MenuItem 
    (
        ParentMenuItemID, 
        MenuName, 
        MenuKey, 
        FormClassName,
        IconKey, 
        DisplayOrder, 
        IsActive, 
        RequiredAppRole, 
        Description, 
        ModuleCode, 
        CreatedByUserID,
        CreatedDate
    )
    VALUES 
    (
        @WFEParentID,
        N'รายละเอียด Workflow',
        'WFE_DETAIL',
        'CMS.Presentation.Modules.Workflow.WorkflowDetailForm',
        'detail',
        5,
        1,
        1,
        N'รายละเอียด Workflow Instance',
        'WFE',
        @AdminUserID,
        GETDATE()
    );
    PRINT 'รายละเอียด Workflow menu created.';
END

-- 2.6 จัดการ Template Workflow
IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = 'WFE_TEMPLATE_LIST')
BEGIN
    INSERT INTO core.MenuItem 
    (
        ParentMenuItemID, 
        MenuName, 
        MenuKey, 
        FormClassName,
        IconKey, 
        DisplayOrder, 
        IsActive, 
        RequiredAppRole, 
        Description, 
        ModuleCode, 
        CreatedByUserID,
        CreatedDate
    )
    VALUES 
    (
        @WFEParentID,
        N'จัดการ Template Workflow',
        'WFE_TEMPLATE_LIST',
        'CMS.Presentation.Modules.Workflow.WorkflowTemplateListForm',
        'list',
        6,
        1,
        2,                              -- Manager and above
        N'รายการ Workflow Template',
        'WFE',
        @AdminUserID,
        GETDATE()
    );
    PRINT 'จัดการ Template Workflow menu created.';
END

-- 2.7 Workflow Visual Flow
IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = 'WFE_VISUAL_FLOW')
BEGIN
    INSERT INTO core.MenuItem 
    (
        ParentMenuItemID, 
        MenuName, 
        MenuKey, 
        FormClassName,
        IconKey, 
        DisplayOrder, 
        IsActive, 
        RequiredAppRole, 
        Description, 
        ModuleCode, 
        CreatedByUserID,
        CreatedDate
    )
    VALUES 
    (
        @WFEParentID,
        N'Workflow Visual Flow',
        'WFE_VISUAL_FLOW',
        'CMS.Presentation.Modules.Workflow.WorkflowVisualFlowForm',
        'flow',
        7,
        1,
        1,
        N'แสดง Workflow แบบ Visual Flow',
        'WFE',
        @AdminUserID,
        GETDATE()
    );
    PRINT 'Workflow Visual Flow menu created.';
END

-- ────────────────────────────────────────────────────────────────────────────
-- 3. VERIFICATION
-- ────────────────────────────────────────────────────────────────────────────

PRINT '';
PRINT '════════════════════════════════════════════════════════════════';
PRINT 'WFE Menu Structure:';
PRINT '════════════════════════════════════════════════════════════════';

SELECT 
    CASE 
        WHEN m.ParentMenuItemID IS NULL THEN '📁 ' + m.MenuName
        ELSE '  📄 ' + m.MenuName
    END AS [Menu Structure],
    m.MenuKey,
    m.FormClassName,
    m.DisplayOrder,
    CASE m.RequiredAppRole
        WHEN 1 THEN 'User'
        WHEN 2 THEN 'Manager'
        WHEN 3 THEN 'Approver'
        WHEN 4 THEN 'Admin'
        ELSE 'All'
    END AS [Required Role],
    CASE m.IsActive WHEN 1 THEN '✓ Active' ELSE '✗ Inactive' END AS [Status]
FROM core.MenuItem m
WHERE m.MenuKey = 'WFE' OR m.ParentMenuItemID = @WFEParentID
ORDER BY m.ParentMenuItemID, m.DisplayOrder;

PRINT '';
PRINT 'WFE menu items created successfully!';
PRINT '════════════════════════════════════════════════════════════════';
