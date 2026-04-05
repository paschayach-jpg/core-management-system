-- ============================================================
-- Script 059: Seed FolderAuthorityConfig แยกตาม Division → Department
-- โครงสร้าง: ทุก Division และ Department ใน HR ได้รับ config
-- Default สิทธิ์: 1HC=ไม่มี, 2SC=ไม่มี, 3NC=View, 4PC=View+Add
-- *** ปรับสิทธิ์ตามความเหมาะสมหลัง insert ***
-- ============================================================

-- ── เพิ่ม column Level ถ้ายังไม่มี ───────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('core.HR_Division')   AND name = 'Level')
    ALTER TABLE core.HR_Division   ADD Level NVARCHAR(20) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('core.HR_Department') AND name = 'Level')
    ALTER TABLE core.HR_Department ADD Level NVARCHAR(20) NULL;
GO

-- ── Seed FolderAuthorityConfig แยกตาม Division ───────────────
PRINT '=== DIVISION CONFIGS ===';

DECLARE @divCode NVARCHAR(50), @divName NVARCHAR(200);
DECLARE cur_div CURSOR FOR
    SELECT DivisionCode, DivisionName FROM core.HR_Division WHERE IsActive = 1 ORDER BY DivisionCode;

OPEN cur_div;
FETCH NEXT FROM cur_div INTO @divCode, @divName;
WHILE @@FETCH_STATUS = 0
BEGIN
    DELETE FROM core.FolderAuthorityConfig WHERE ConfigLabel = @divCode AND IsActive = 1;

    INSERT INTO core.FolderAuthorityConfig
        (ConfigLabel, DivisionCode, LevelCode, ConfidentialityLevel,
         CanView, CanAdd, CanEdit, CanApprove, IsActive, CreatedDate)
    VALUES
        (@divCode, @divCode, '1HC', 1,  0,0,0,0,  1, GETDATE()),
        (@divCode, @divCode, '2SC', 2,  0,0,0,0,  1, GETDATE()),
        (@divCode, @divCode, '3NC', 3,  1,0,0,0,  1, GETDATE()),
        (@divCode, @divCode, '4PC', 4,  1,1,0,0,  1, GETDATE());

    PRINT '  DIV: ' + @divCode + ' — ' + ISNULL(@divName, '');
    FETCH NEXT FROM cur_div INTO @divCode, @divName;
END
CLOSE cur_div; DEALLOCATE cur_div;
GO

-- ── Seed FolderAuthorityConfig แยกตาม Department (ภายใต้ Division) ──
PRINT '';
PRINT '=== DEPARTMENT CONFIGS (แยกตาม Division) ===';

DECLARE @deptCode NVARCHAR(50), @deptName NVARCHAR(200);
DECLARE @parentDivCode NVARCHAR(50), @parentDivName NVARCHAR(200);

DECLARE cur_dept CURSOR FOR
    SELECT
        dep.DepartmentCode,
        dep.DepartmentName,
        div.DivisionCode,
        div.DivisionName
    FROM   core.HR_Department dep
    JOIN   core.HR_Division   div ON dep.DivisionID = div.DivisionID
    WHERE  dep.IsActive = 1
    ORDER  BY div.DivisionCode, dep.DepartmentCode;

OPEN cur_dept;
FETCH NEXT FROM cur_dept INTO @deptCode, @deptName, @parentDivCode, @parentDivName;
WHILE @@FETCH_STATUS = 0
BEGIN
    DELETE FROM core.FolderAuthorityConfig WHERE ConfigLabel = @deptCode AND IsActive = 1;

    INSERT INTO core.FolderAuthorityConfig
        (ConfigLabel, DepartmentCode, DivisionCode, LevelCode, ConfidentialityLevel,
         CanView, CanAdd, CanEdit, CanApprove, IsActive, CreatedDate)
    VALUES
        (@deptCode, @deptCode, @parentDivCode, '1HC', 1,  0,0,0,0,  1, GETDATE()),
        (@deptCode, @deptCode, @parentDivCode, '2SC', 2,  0,0,0,0,  1, GETDATE()),
        (@deptCode, @deptCode, @parentDivCode, '3NC', 3,  1,0,0,0,  1, GETDATE()),
        (@deptCode, @deptCode, @parentDivCode, '4PC', 4,  1,1,0,0,  1, GETDATE());

    PRINT '    DEP: ' + @deptCode + ' (' + ISNULL(@deptName,'') + ') → DIV: ' + @parentDivCode;
    FETCH NEXT FROM cur_dept INTO @deptCode, @deptName, @parentDivCode, @parentDivName;
END
CLOSE cur_dept; DEALLOCATE cur_dept;
GO

-- ── แสดงผลลัพธ์แยกตาม Division → Department ─────────────────
PRINT '';
PRINT '=== ผลลัพธ์ทั้งหมด ===';

SELECT
    div.DivisionCode,
    div.DivisionName,
    dep.DepartmentCode,
    dep.DepartmentName,
    c.LevelCode,
    c.CanView, c.CanAdd, c.CanEdit, c.CanApprove
FROM   core.HR_Division   div
JOIN   core.HR_Department dep ON dep.DivisionID = div.DivisionID
JOIN   core.FolderAuthorityConfig c
           ON c.ConfigLabel = dep.DepartmentCode AND c.IsActive = 1
WHERE  div.IsActive = 1 AND dep.IsActive = 1
ORDER  BY div.DivisionCode, dep.DepartmentCode, c.ConfidentialityLevel;
GO

PRINT '✅ Script 059 เสร็จสมบูรณ์';
GO
