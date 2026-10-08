-- One-off cleanup: divisions created by importing the "รายชื่อพนักงานตามสิทธิ์" file into Org Master
-- (DivisionCode = row "No" 1..7 / "No", DivisionName = employee code / "รหัสพนักงาน").
-- Rename with the next free script number before committing (the Scripts folder is not in this checkout).
--
-- Safe by default: runs in a transaction, shows what it will delete, and ROLLS BACK unless @Commit = 1.
-- Review the SELECT output first, then set @Commit = 1 and run again.

SET XACT_ABORT ON;
DECLARE @Commit BIT = 0;

BEGIN TRAN;

DECLARE @BadDivisions TABLE (DivisionID INT PRIMARY KEY);
INSERT INTO @BadDivisions (DivisionID)
SELECT DivisionID
FROM EMS.m_Division
WHERE DivisionCode IN ('1','2','3','4','5','6','7','No')
  AND DivisionName IN ('01942','00955','00269','01245','01462','00493','01923',N'รหัสพนักงาน');

-- Preview
SELECT 'Division' AS Kind, d.DivisionID, d.DivisionCode, d.DivisionName
FROM EMS.m_Division d JOIN @BadDivisions b ON b.DivisionID = d.DivisionID;
SELECT 'Department' AS Kind, d.DepartmentID, d.DepartmentCode, d.DepartmentName, d.DivisionID
FROM EMS.m_Department d JOIN @BadDivisions b ON b.DivisionID = d.DivisionID;
SELECT 'PositionMapping' AS Kind, COUNT(*) AS Cnt
FROM EMS.m_PositionMapping m
WHERE m.DivisionID IN (SELECT DivisionID FROM @BadDivisions)
   OR m.DepartmentID IN (SELECT DepartmentID FROM EMS.m_Department WHERE DivisionID IN (SELECT DivisionID FROM @BadDivisions));

-- Delete children first (FK_mDept_Division blocks deleting a division that still has departments)
DELETE FROM EMS.m_PositionMapping
WHERE DivisionID IN (SELECT DivisionID FROM @BadDivisions)
   OR DepartmentID IN (SELECT DepartmentID FROM EMS.m_Department WHERE DivisionID IN (SELECT DivisionID FROM @BadDivisions));

DELETE FROM EMS.m_Department
WHERE DivisionID IN (SELECT DivisionID FROM @BadDivisions);

DELETE FROM EMS.m_Division
WHERE DivisionID IN (SELECT DivisionID FROM @BadDivisions);

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
