-- ============================================================
-- Script 045: Extend UserAccount with Employee API fields
-- เพิ่มข้อมูลพนักงานจาก Employee API เข้า UserAccount
-- เพื่อรองรับ:
--   1. Role-based access control
--   2. Department/Division-based folder visibility
--   3. Link กับ Employee API
-- ============================================================

-- เพิ่ม columns ใหม่ใน UserAccount
ALTER TABLE core.UserAccount
    ADD EmployeeCode    NVARCHAR(20)  NULL,   -- รหัสพนักงาน (จาก API)
        DepartmentId    INT           NULL,   -- ID แผนก (จาก Employee API)
        DepartmentCode  NVARCHAR(20)  NULL,   -- รหัสแผนก
        DepartmentName  NVARCHAR(200) NULL,   -- ชื่อแผนก
        DivisionId      INT           NULL,   -- ID ฝ่าย (จาก Employee API)
        DivisionCode    NVARCHAR(20)  NULL,   -- รหัสฝ่าย
        DivisionName    NVARCHAR(200) NULL,   -- ชื่อฝ่าย
        PositionName    NVARCHAR(200) NULL,   -- ตำแหน่ง
        UpdatedAt       DATETIME2     NULL;   -- วันที่อัพเดทล่าสุด
GO

-- Index สำหรับค้นหาตาม Department/Division
CREATE INDEX IX_UserAccount_DepartmentId
    ON core.UserAccount(DepartmentId)
    WHERE DepartmentId IS NOT NULL;
GO

CREATE INDEX IX_UserAccount_DivisionId
    ON core.UserAccount(DivisionId)
    WHERE DivisionId IS NOT NULL;
GO

CREATE INDEX IX_UserAccount_EmployeeCode
    ON core.UserAccount(EmployeeCode)
    WHERE EmployeeCode IS NOT NULL;
GO

PRINT 'Script 045 completed: UserAccount extended with Employee API fields';
GO
