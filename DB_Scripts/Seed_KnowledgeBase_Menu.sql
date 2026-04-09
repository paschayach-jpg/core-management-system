-- ============================================================================
-- Seed menu item for Knowledge Base (KnowledgeBaseForm)
-- Safe to run multiple times (idempotent)
-- ============================================================================

SET NOCOUNT ON;

IF OBJECT_ID('core.MenuItem', 'U') IS NULL
BEGIN
    RAISERROR('core.MenuItem table not found. Please run Menu_Schema_Migration.sql first.', 16, 1);
    RETURN;
END;

DECLARE @CreatedByUserID INT;

SELECT TOP (1) @CreatedByUserID = ua.UserID
FROM core.UserAccount ua
ORDER BY ua.UserID ASC;

IF @CreatedByUserID IS NULL
BEGIN
    RAISERROR('No user found in core.UserAccount for CreatedByUserID.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM core.MenuItem WHERE MenuKey = 'knowledge_base')
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
        Url,
        CreatedByUserID,
        CreatedDate
    )
    VALUES
    (
        NULL,
        N'Knowledge Base',
        'knowledge_base',
        'KnowledgeBaseForm',
        NULL,
        100,
        1,
        1,
        N'เปิดหน้าห้องสมุดความรู้',
        'KMS',
        NULL,
        @CreatedByUserID,
        GETDATE()
    );

    PRINT 'Inserted menu: knowledge_base -> KnowledgeBaseForm';
END
ELSE
BEGIN
    UPDATE core.MenuItem
    SET
        FormClassName = 'KnowledgeBaseForm',
        IsActive = 1,
        ModifiedDate = GETDATE(),
        ModifiedByUserID = @CreatedByUserID
    WHERE MenuKey = 'knowledge_base';

    PRINT 'Updated existing menu: knowledge_base -> KnowledgeBaseForm';
END;

SELECT MenuItemID, ParentMenuItemID, MenuName, MenuKey, FormClassName, IsActive, RequiredAppRole, DisplayOrder
FROM core.MenuItem
WHERE MenuKey = 'knowledge_base';
