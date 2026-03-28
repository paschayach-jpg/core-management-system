-- ============================================================
-- Script 029: Flexible Structure Example
-- Shows how the new flexible structure works
-- ============================================================

PRINT 'Flexible Organization Structure Example:'
PRINT '======================================='

-- Example data structure that demonstrates flexibility:
-- 1 SubCommittee can have multiple Divisions
-- Divisions can also exist independently
-- Departments must belong to a Division

PRINT 'Example Structure:'
PRINT '🏢 SubCommittee A'
PRINT '├── 📁 Division 1 (belongs to SubCommittee A)'
PRINT '├── 📁 Division 2 (belongs to SubCommittee A)'
PRINT '└── 📁 Division 3 (belongs to SubCommittee A)'
PRINT ''
PRINT '🏢 SubCommittee B'
PRINT '├── 📁 Division 4 (belongs to SubCommittee B)'
PRINT '└── 📁 Division 5 (belongs to SubCommittee B)'
PRINT ''
PRINT '🔗 Independent Divisions:'
PRINT '├── 📁 Division X (independent - can work with any SubCommittee)'
PRINT '└── 📁 Division Y (independent - can work with any SubCommittee)'

PRINT ''
PRINT 'Current Database Structure:'

-- Show actual structure
WITH OrgHierarchy AS (
    SELECT 
        OrgUnitID,
        OrgUnitCode,
        OrgUnitNameTH,
        OrgUnitType,
        ParentOrgUnitID,
        0 as Level,
        CAST(OrgUnitNameTH AS NVARCHAR(500)) as Path
    FROM core.OrgUnit 
    WHERE OrgUnitType = 1 AND IsDeleted = 0 AND IsActive = 1 -- SubCommittees
    
    UNION ALL
    
    SELECT 
        child.OrgUnitID,
        child.OrgUnitCode,
        child.OrgUnitNameTH,
        child.OrgUnitType,
        child.ParentOrgUnitID,
        parent.Level + 1,
        parent.Path + ' → ' + child.OrgUnitNameTH
    FROM core.OrgUnit child
    INNER JOIN OrgHierarchy parent ON child.ParentOrgUnitID = parent.OrgUnitID
    WHERE child.IsDeleted = 0 AND child.IsActive = 1
)
SELECT 
    REPLICATE('  ', Level) + 
    CASE OrgUnitType 
        WHEN 1 THEN '🏢 ' + OrgUnitNameTH + ' [SubCommittee]'
        WHEN 2 THEN '📁 ' + OrgUnitNameTH + ' [Division]'
        WHEN 3 THEN '📋 ' + OrgUnitNameTH + ' [Department]'
    END as Structure,
    OrgUnitCode,
    CASE 
        WHEN ParentOrgUnitID IS NULL THEN 'Root/Independent'
        ELSE 'Parent: ' + CAST(ParentOrgUnitID AS NVARCHAR(10))
    END as ParentInfo
FROM OrgHierarchy
ORDER BY Level, OrgUnitNameTH;

-- Show independent divisions
PRINT ''
PRINT 'Independent Divisions (no parent):'
SELECT 
    '🔗 ' + OrgUnitNameTH as IndependentDivision,
    OrgUnitCode,
    'Can work with any SubCommittee' as Status
FROM core.OrgUnit 
WHERE OrgUnitType = 2 
  AND ParentOrgUnitID IS NULL 
  AND IsDeleted = 0 
  AND IsActive = 1
ORDER BY OrgUnitNameTH;

PRINT 'Flexible structure analysis completed.';