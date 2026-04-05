-- ============================================================
-- Script 060: Fix UserAccount stored procedures to include EmployeeCode
-- แก้ไข SP ที่ SELECT/UPDATE UserAccount ให้รองรับ EmployeeCode
-- ============================================================

-- 1. sp_GetUserAccountByADUserName
CREATE OR ALTER PROCEDURE core.sp_GetUserAccountByADUserName
    @ADUserName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, ADUserName, DisplayName, Email, AppRole, OrgUnitID,
           ConsentGiven, ConsentDate, IsActive, LastLoginDate, CreatedBy, CreatedDate,
           EmployeeCode, EmployeeTitleNameEn, EmployeeFirstNameEn, EmployeeLastNameEn,
           EmployeeTitleNameTh, EmployeeFirstNameTh, EmployeeLastNameTh,
           DepartmentId, DepartmentCode, DepartmentName, DepartmentNameEn,
           DivisionId, DivisionCode, DivisionName, DivisionNameEn,
           PositionName, LevelCode, UpdatedAt
    FROM   core.UserAccount
    WHERE  ADUserName = @ADUserName;
END
GO

-- 2. sp_GetUserAccountByID
CREATE OR ALTER PROCEDURE core.sp_GetUserAccountByID
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, ADUserName, DisplayName, Email, AppRole, OrgUnitID,
           ConsentGiven, ConsentDate, IsActive, LastLoginDate, CreatedBy, CreatedDate,
           EmployeeCode, EmployeeTitleNameEn, EmployeeFirstNameEn, EmployeeLastNameEn,
           EmployeeTitleNameTh, EmployeeFirstNameTh, EmployeeLastNameTh,
           DepartmentId, DepartmentCode, DepartmentName, DepartmentNameEn,
           DivisionId, DivisionCode, DivisionName, DivisionNameEn,
           PositionName, LevelCode, UpdatedAt
    FROM   core.UserAccount
    WHERE  UserID = @UserID;
END
GO

-- 3. sp_GetAllUserAccounts
CREATE OR ALTER PROCEDURE core.sp_GetAllUserAccounts
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, ADUserName, DisplayName, Email, AppRole, OrgUnitID,
           ConsentGiven, ConsentDate, IsActive, LastLoginDate, CreatedBy, CreatedDate,
           EmployeeCode, EmployeeTitleNameEn, EmployeeFirstNameEn, EmployeeLastNameEn,
           EmployeeTitleNameTh, EmployeeFirstNameTh, EmployeeLastNameTh,
           DepartmentId, DepartmentCode, DepartmentName, DepartmentNameEn,
           DivisionId, DivisionCode, DivisionName, DivisionNameEn,
           PositionName, LevelCode, UpdatedAt
    FROM   core.UserAccount
    WHERE  IsActive = 1
    ORDER  BY ADUserName;
END
GO

-- 4. sp_GetUserAccountsByRole
CREATE OR ALTER PROCEDURE core.sp_GetUserAccountsByRole
    @AppRole TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, ADUserName, DisplayName, Email, AppRole, OrgUnitID,
           ConsentGiven, ConsentDate, IsActive, LastLoginDate, CreatedBy, CreatedDate,
           EmployeeCode, EmployeeTitleNameEn, EmployeeFirstNameEn, EmployeeLastNameEn,
           EmployeeTitleNameTh, EmployeeFirstNameTh, EmployeeLastNameTh,
           DepartmentId, DepartmentCode, DepartmentName, DepartmentNameEn,
           DivisionId, DivisionCode, DivisionName, DivisionNameEn,
           PositionName, LevelCode, UpdatedAt
    FROM   core.UserAccount
    WHERE  AppRole = @AppRole AND IsActive = 1;
END
GO

-- 5. sp_UpdateUserAccount — เพิ่ม EmployeeCode
CREATE OR ALTER PROCEDURE core.sp_UpdateUserAccount
    @UserID       INT,
    @DisplayName  NVARCHAR(200),
    @AppRole      TINYINT,
    @OrgUnitID    INT,
    @EmployeeCode NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.UserAccount
    SET    DisplayName  = @DisplayName,
           AppRole      = @AppRole,
           OrgUnitID    = @OrgUnitID,
           EmployeeCode = @EmployeeCode
    WHERE  UserID = @UserID;
END
GO

PRINT '✅ Script 060 เสร็จสมบูรณ์: UserAccount SPs รองรับ EmployeeCode แล้ว';
GO
