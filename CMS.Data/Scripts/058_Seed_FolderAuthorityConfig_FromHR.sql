-- ============================================================
-- Script 058: Add Level column to HR tables + Seed FolderAuthorityConfig
-- เพิ่ม Level ใน HR_Position, HR_Division, HR_Department
-- และ auto-generate FolderAuthorityConfig ครบทุกตำแหน่ง
-- ============================================================

-- ── 1. เพิ่ม column Level ใน HR_Position ─────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('core.HR_Position') AND name = 'Level'
)
BEGIN
    ALTER TABLE core.HR_Position ADD Level NVARCHAR(20) NULL;
    PRINT '✅ เพิ่ม Level ใน HR_Position';
END
GO

-- ── 2. เพิ่ม column Level ใน HR_Division ─────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('core.HR_Division') AND name = 'Level'
)
BEGIN
    ALTER TABLE core.HR_Division ADD Level NVARCHAR(20) NULL;
    PRINT '✅ เพิ่ม Level ใน HR_Division';
END
GO

-- ── 3. เพิ่ม column Level ใน HR_Department ───────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('core.HR_Department') AND name = 'Level'
)
BEGIN
    ALTER TABLE core.HR_Department ADD Level NVARCHAR(20) NULL;
    PRINT '✅ เพิ่ม Level ใน HR_Department';
END
GO

-- ── 4. Seed FolderAuthorityConfig จากทุก Position ────────────
-- สร้าง 4 rows (1HC/2SC/3NC/4PC) ต่อ 1 Position
-- Default: CanView=1 สำหรับ 3NC, 4PC / ไม่มีสิทธิ์ 1HC, 2SC
-- *** ปรับสิทธิ์ตามความเหมาะสมหลัง insert ***

DECLARE @pos_cursor CURSOR;
DECLARE @posCode   NVARCHAR(50);
DECLARE @posName   NVARCHAR(200);
DECLARE @posLevel  NVARCHAR(20);

SET @pos_cursor = CURSOR FOR
    SELECT PositionCode, PositionName, Level
    FROM   core.HR_Position
    WHERE  IsActive = 1;

OPEN @pos_cursor;
FETCH NEXT FROM @pos_cursor INTO @posCode, @posName, @posLevel;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- ลบของเดิมถ้ามี
    DELETE FROM core.FolderAuthorityConfig
    WHERE ConfigLabel = @posCode AND IsActive = 1;

    -- Insert 4 rows
    INSERT INTO core.FolderAuthorityConfig
        (ConfigLabel, PositionName, LevelCode, ConfidentialityLevel,
         CanView, CanAdd, CanEdit, CanApprove, IsActive, CreatedDate)
    VALUES
        (@posCode, @posName, '1HC', 1,  0,0,0,0,  1, GETDATE()),
        (@posCode, @posName, '2SC', 2,  0,0,0,0,  1, GETDATE()),
        (@posCode, @posName, '3NC', 3,  1,0,0,0,  1, GETDATE()),
        (@posCode, @posName, '4PC', 4,  1,1,0,0,  1, GETDATE());

    PRINT '✅ Insert config สำหรับ: ' + @posCode + ' (' + ISNULL(@posName,'') + ')';

    FETCH NEXT FROM @pos_cursor INTO @posCode, @posName, @posLevel;
END

CLOSE @pos_cursor;
DEALLOCATE @pos_cursor;
GO

-- ── 5. แสดงผลลัพธ์ ─────────────────────────────────────────
SELECT
    c.ConfigLabel,
    c.PositionName,
    c.LevelCode,
    c.CanView, c.CanAdd, c.CanEdit, c.CanApprove
FROM   core.FolderAuthorityConfig c
WHERE  c.IsActive = 1
ORDER  BY c.ConfigLabel, c.ConfidentialityLevel;
GO

PRINT '✅ Script 058 เสร็จสมบูรณ์';
GO
