-- ============================================================
-- Script 027: Debug OrgUnit Data
-- Shows current state of OrgUnit data for troubleshooting
-- ============================================================

PRINT 'Current OrgUnit Data Analysis:'
PRINT '================================'

-- Show all active OrgUnits
PRINT 'All Active OrgUnits:'
SELECT 
    OrgUnitID,
    OrgUnitCode,
    OrgUnitNameTH,
    OrgUnitType,
    ParentOrgUnitID,
    IsActive,
    IsDeleted,
    CreatedDate
FROM core.OrgUnit 
WHERE IsDeleted = 0
ORDER BY OrgUnitType, OrgUnitCode;

PRINT ''
PRINT 'OrgUnit Type Summary:'
SELECT 
    OrgUnitType,
    CASE OrgUnitType 
        WHEN 1 THEN 'SubCommittee'
        WHEN 2 THEN 'Division' 
        WHEN 3 THEN 'Department'
        ELSE 'Unknown'
    END as TypeName,
    COUNT(*) as Count,
    COUNT(CASE WHEN ParentOrgUnitID IS NULL THEN 1 END) as WithoutParent,
    COUNT(CASE WHEN ParentOrgUnitID IS NOT NULL THEN 1 END) as WithParent
FROM core.OrgUnit 
WHERE IsDeleted = 0 AND IsActive = 1
GROUP BY OrgUnitType
ORDER BY OrgUnitType;

PRINT ''
PRINT 'Division Details (Type 2):'
SELECT 
    OrgUnitID,
    OrgUnitCode,
    OrgUnitNameTH,
    ParentOrgUnitID,
    CASE 
        WHEN ParentOrgUnitID IS NULL THEN 'Standalone (Many-to-Many)'
        ELSE 'Has Parent: ' + CAST(ParentOrgUnitID AS NVARCHAR(10))
    END as ParentStatus
FROM core.OrgUnit 
WHERE OrgUnitType = 2 AND IsDeleted = 0 AND IsActive = 1
ORDER BY OrgUnitCode;

PRINT ''
PRINT 'SubCommittee Details (Type 1):'
SELECT 
    OrgUnitID,
    OrgUnitCode,
    OrgUnitNameTH,
    (SELECT COUNT(*) FROM core.OrgUnit child 
     WHERE child.ParentOrgUnitID = parent.OrgUnitID 
       AND child.OrgUnitType = 2 
       AND child.IsDeleted = 0) as DirectDivisionCount
FROM core.OrgUnit parent
WHERE OrgUnitType = 1 AND IsDeleted = 0 AND IsActive = 1
ORDER BY OrgUnitCode;

PRINT ''
PRINT 'Recent OrgUnit Creations (Last 10):'
SELECT TOP 10
    OrgUnitID,
    OrgUnitCode,
    OrgUnitNameTH,
    OrgUnitType,
    ParentOrgUnitID,
    CreatedDate
FROM core.OrgUnit 
WHERE IsDeleted = 0
ORDER BY CreatedDate DESC;

PRINT 'Debug analysis completed.';