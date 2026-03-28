-- ============================================================
-- Script 026: Cleanup Duplicate OrgUnit Codes
-- Identifies and fixes duplicate OrgUnitCode entries
-- ============================================================

-- First, let's see what duplicates exist
PRINT 'Checking for duplicate OrgUnitCode entries...'

SELECT OrgUnitCode, COUNT(*) as DuplicateCount
FROM core.OrgUnit 
WHERE IsDeleted = 0 
  AND OrgUnitCode IS NOT NULL
GROUP BY OrgUnitCode
HAVING COUNT(*) > 1
ORDER BY OrgUnitCode;

-- If there are duplicates, we need to fix them
-- Keep the first one (lowest OrgUnitID) and rename the others

DECLARE @sql NVARCHAR(MAX) = '';
DECLARE @code NVARCHAR(20);
DECLARE @counter INT;

DECLARE duplicate_cursor CURSOR FOR
    SELECT OrgUnitCode
    FROM core.OrgUnit 
    WHERE IsDeleted = 0 
      AND OrgUnitCode IS NOT NULL
    GROUP BY OrgUnitCode
    HAVING COUNT(*) > 1;

OPEN duplicate_cursor;
FETCH NEXT FROM duplicate_cursor INTO @code;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @counter = 1;
    
    -- Update duplicate entries (keep the first one unchanged)
    UPDATE core.OrgUnit 
    SET OrgUnitCode = @code + '-DUP' + CAST(@counter AS NVARCHAR(3))
    FROM (
        SELECT OrgUnitID, 
               ROW_NUMBER() OVER (ORDER BY OrgUnitID) as rn
        FROM core.OrgUnit 
        WHERE OrgUnitCode = @code 
          AND IsDeleted = 0
    ) ranked
    WHERE core.OrgUnit.OrgUnitID = ranked.OrgUnitID 
      AND ranked.rn > 1;
    
    SET @counter = @counter + 1;
    FETCH NEXT FROM duplicate_cursor INTO @code;
END

CLOSE duplicate_cursor;
DEALLOCATE duplicate_cursor;

-- Verify no more duplicates exist
PRINT 'Verification - Remaining duplicates (should be empty):'
SELECT OrgUnitCode, COUNT(*) as DuplicateCount
FROM core.OrgUnit 
WHERE IsDeleted = 0 
  AND OrgUnitCode IS NOT NULL
GROUP BY OrgUnitCode
HAVING COUNT(*) > 1
ORDER BY OrgUnitCode;

PRINT 'Script 026: Duplicate OrgUnitCode cleanup completed';