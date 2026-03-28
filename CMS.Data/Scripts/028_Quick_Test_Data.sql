-- ============================================================
-- Script 028: Quick Test Data Check
-- Quick check of current OrgUnit data
-- ============================================================

PRINT 'Quick OrgUnit Data Check:'
PRINT '========================='

-- Show summary
SELECT 
    'Total Active OrgUnits' as Description,
    COUNT(*) as Count
FROM core.OrgUnit 
WHERE IsDeleted = 0 AND IsActive = 1

UNION ALL

SELECT 
    'SubCommittees (Type 1)' as Description,
    COUNT(*) as Count
FROM core.OrgUnit 
WHERE IsDeleted = 0 AND IsActive = 1 AND OrgUnitType = 1

UNION ALL

SELECT 
    'Divisions (Type 2)' as Description,
    COUNT(*) as Count
FROM core.OrgUnit 
WHERE IsDeleted = 0 AND IsActive = 1 AND OrgUnitType = 2

UNION ALL

SELECT 
    'Divisions without Parent (Many-to-Many)' as Description,
    COUNT(*) as Count
FROM core.OrgUnit 
WHERE IsDeleted = 0 AND IsActive = 1 AND OrgUnitType = 2 AND ParentOrgUnitID IS NULL

UNION ALL

SELECT 
    'Departments (Type 3)' as Description,
    COUNT(*) as Count
FROM core.OrgUnit 
WHERE IsDeleted = 0 AND IsActive = 1 AND OrgUnitType = 3;

PRINT ''
PRINT 'Recent Division Additions:'
SELECT TOP 5
    OrgUnitCode,
    OrgUnitNameTH,
    ParentOrgUnitID,
    CASE 
        WHEN ParentOrgUnitID IS NULL THEN 'Many-to-Many (No Parent)'
        ELSE 'Has Parent: ' + CAST(ParentOrgUnitID AS NVARCHAR(10))
    END as ParentStatus,
    CreatedDate
FROM core.OrgUnit 
WHERE IsDeleted = 0 AND IsActive = 1 AND OrgUnitType = 2
ORDER BY CreatedDate DESC;

PRINT 'Quick check completed.';