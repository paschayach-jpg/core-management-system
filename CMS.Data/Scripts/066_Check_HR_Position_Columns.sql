-- ============================================================
-- Script 066: ตรวจสอบและเพิ่ม column ที่ขาดใน HR_Position
-- ============================================================

-- ตรวจสอบ columns ที่มีอยู่
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'core'
  AND TABLE_NAME   = 'HR_Position'
ORDER BY ORDINAL_POSITION;
GO

-- เพิ่ม LevelCode ถ้ายังไม่มี
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('core.HR_Position') AND name = 'LevelCode'
)
BEGIN
    ALTER TABLE core.HR_Position ADD LevelCode NVARCHAR(20) NULL;
    -- Copy จาก Level ถ้ามี
    UPDATE core.HR_Position SET LevelCode = Level WHERE LevelCode IS NULL AND Level IS NOT NULL;
    PRINT '✅ เพิ่ม LevelCode ใน HR_Position';
END
ELSE
    PRINT '✅ LevelCode มีอยู่แล้ว';
GO

-- เพิ่ม PositionLevel ถ้ายังไม่มี
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('core.HR_Position') AND name = 'PositionLevel'
)
BEGIN
    ALTER TABLE core.HR_Position ADD PositionLevel NVARCHAR(20) NULL;
    PRINT '✅ เพิ่ม PositionLevel ใน HR_Position';
END
ELSE
    PRINT '✅ PositionLevel มีอยู่แล้ว';
GO

-- แสดงข้อมูลตัวอย่าง
SELECT TOP 10 PositionID, PositionCode, PositionName, 
              PositionLevel, LevelCode, Level
FROM core.HR_Position
WHERE IsActive = 1
ORDER BY PositionName;
GO
