-- ============================================================
-- Script 055: Seed FolderAuthorityConfig สำหรับตำแหน่ง 2B
-- ตำแหน่ง 2B: สิทธิ์ View+Add เฉพาะ Folder 3NC และ 4PC
-- Folder 1HC, 2SC: ไม่มีสิทธิ์
-- ============================================================

SET NOCOUNT ON;

-- ลบของเดิมถ้ามี (idempotent)
DELETE FROM core.FolderAuthorityConfig
WHERE ConfigLabel = '2B' AND IsActive = 1;

-- Insert 4 rows (1 ต่อ folder level)
INSERT INTO core.FolderAuthorityConfig
    (ConfigLabel, PositionName, DepartmentCode, DivisionCode,
     LevelCode, ConfidentialityLevel,
     CanView, CanAdd, CanEdit, CanApprove,
     IsActive, CreatedDate)
VALUES
    -- 1HC: ไม่มีสิทธิ์
    ('2B', '2B', NULL, NULL, '1HC', 1,  0, 0, 0, 0,  1, GETDATE()),
    -- 2SC: ไม่มีสิทธิ์
    ('2B', '2B', NULL, NULL, '2SC', 2,  0, 0, 0, 0,  1, GETDATE()),
    -- 3NC: View + Add
    ('2B', '2B', NULL, NULL, '3NC', 3,  1, 1, 0, 0,  1, GETDATE()),
    -- 4PC: View + Add
    ('2B', '2B', NULL, NULL, '4PC', 4,  1, 1, 0, 0,  1, GETDATE());

PRINT '✅ Insert FolderAuthorityConfig สำหรับตำแหน่ง 2B แล้ว: ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows';

-- ตรวจสอบผลลัพธ์
SELECT ConfigID, ConfigLabel, PositionName, LevelCode, ConfidentialityLevel,
       CanView, CanAdd, CanEdit, CanApprove
FROM   core.FolderAuthorityConfig
WHERE  ConfigLabel = '2B'
ORDER  BY ConfidentialityLevel;
GO
