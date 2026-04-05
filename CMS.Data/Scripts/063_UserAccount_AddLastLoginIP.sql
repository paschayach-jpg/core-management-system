-- ============================================================
-- Script 063: Add LastLoginIP to UserAccount
-- เก็บ IP Address ของผู้ใช้ตอน Login หรือ Register
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('core.UserAccount') AND name = 'LastLoginIP'
)
BEGIN
    ALTER TABLE core.UserAccount ADD LastLoginIP NVARCHAR(45) NULL;
    PRINT '✅ เพิ่ม LastLoginIP ใน core.UserAccount แล้ว';
END
GO

PRINT '✅ Script 063 เสร็จสมบูรณ์';
GO
