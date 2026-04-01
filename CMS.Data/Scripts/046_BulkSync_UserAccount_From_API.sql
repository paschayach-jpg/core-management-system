-- ============================================================
-- Script 046: Stored Procedure สำหรับ Bulk Sync UserAccount
-- จาก Employee API (เรียกผ่าน Application หรือ Admin tool)
-- ============================================================

-- SP สำหรับ update employee info ของ user คนเดียว
CREATE OR ALTER PROCEDURE core.sp_UpdateUserEmployeeInfo
    @UserID        INT,
    @EmployeeCode  NVARCHAR(20),
    @DepartmentId  INT,
    @DepartmentCode NVARCHAR(20),
    @DepartmentName NVARCHAR(200),
    @DivisionId    INT,
    @DivisionCode  NVARCHAR(20),
    @DivisionName  NVARCHAR(200),
    @PositionName  NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.UserAccount SET
        EmployeeCode   = @EmployeeCode,
        DepartmentId   = @DepartmentId,
        DepartmentCode = @DepartmentCode,
        DepartmentName = @DepartmentName,
        DivisionId     = @DivisionId,
        DivisionCode   = @DivisionCode,
        DivisionName   = @DivisionName,
        PositionName   = @PositionName,
        UpdatedAt      = GETUTCDATE()
    WHERE UserID = @UserID;
END
GO

-- Query ดู users ที่ยังไม่มีข้อมูล Employee
SELECT UserID, ADUserName, DisplayName, DepartmentId, DivisionId
FROM core.UserAccount
WHERE EmployeeCode IS NULL AND IsActive = 1
ORDER BY ADUserName;
GO

PRINT 'Script 046 completed';
GO
