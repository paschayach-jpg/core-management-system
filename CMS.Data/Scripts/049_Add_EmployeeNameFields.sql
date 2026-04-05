-- ============================================================
-- Script 049: Add Employee Name fields to UserAccount
-- เพิ่มข้อมูลชื่อพนักงาน (TH/EN) จาก AD API EmployeeInfo
-- ============================================================

-- เพิ่ม columns ชื่อพนักงาน
ALTER TABLE core.UserAccount
    ADD EmployeeTitleNameEn  NVARCHAR(50)  NULL,   -- คำนำหน้า EN  (Mr., Miss, Mrs.)
        EmployeeFirstNameEn  NVARCHAR(100) NULL,   -- ชื่อ EN
        EmployeeLastNameEn   NVARCHAR(100) NULL,   -- นามสกุล EN
        EmployeeTitleNameTh  NVARCHAR(50)  NULL,   -- คำนำหน้า TH  (นาย, นางสาว)
        EmployeeFirstNameTh  NVARCHAR(100) NULL,   -- ชื่อ TH
        EmployeeLastNameTh   NVARCHAR(100) NULL,   -- นามสกุล TH
        DepartmentNameEn     NVARCHAR(200) NULL,   -- ชื่อแผนก EN
        DivisionNameEn       NVARCHAR(200) NULL;   -- ชื่อฝ่าย EN
GO

PRINT 'Script 049 completed: EmployeeNameFields added to core.UserAccount';
GO
