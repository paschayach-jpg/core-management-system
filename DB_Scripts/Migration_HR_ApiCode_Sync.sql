-- ============================================================
-- Migration: เพิ่ม column สำหรับ sync ข้อมูลจาก Employee API
-- Database  : SCS
-- Schema    : core
-- Tables    : HR_Division, HR_Department
-- วัตถุประสงค์: เก็บ DivisionCode / DepartmentCode (และ NameTh/NameEn)
--             ที่มาจาก API โดยไม่แก้ไขโครงสร้าง column เดิม
-- ============================================================
USE [SCS];
GO

-- ============================================================
-- PART 1 : เพิ่ม column ใหม่ใน core.HR_Division
-- ============================================================

-- 1-A  DivisionCode จาก API  (ถ้า column เดิมชื่อ DivisionCode ใช้อยู่แล้ว
--      ให้เปลี่ยน column ใหม่เป็น ApiDivisionCode เพื่อไม่ชน)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'core.HR_Division')
      AND name      = N'ApiDivisionCode'
)
BEGIN
    ALTER TABLE [core].[HR_Division]
        ADD [ApiDivisionCode] NVARCHAR(50) NULL;
    PRINT 'Added core.HR_Division.ApiDivisionCode';
END
ELSE
    PRINT 'core.HR_Division.ApiDivisionCode already exists — skipped';
GO

-- 1-B  DivisionId ของ API (int key ที่ API ส่งมา — ต่างจาก DivisionID ที่เป็น PK ในDB)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'core.HR_Division')
      AND name      = N'ApiDivisionId'
)
BEGIN
    ALTER TABLE [core].[HR_Division]
        ADD [ApiDivisionId] INT NULL;
    PRINT 'Added core.HR_Division.ApiDivisionId';
END
ELSE
    PRINT 'core.HR_Division.ApiDivisionId already exists — skipped';
GO

-- 1-C  ชื่อภาษาไทย (API ส่ง divisionNameTh)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'core.HR_Division')
      AND name      = N'DivisionNameTh'
)
BEGIN
    ALTER TABLE [core].[HR_Division]
        ADD [DivisionNameTh] NVARCHAR(200) NULL;
    PRINT 'Added core.HR_Division.DivisionNameTh';
END
ELSE
    PRINT 'core.HR_Division.DivisionNameTh already exists — skipped';
GO

-- 1-D  ชื่อภาษาอังกฤษ (API ส่ง divisionNameEn)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'core.HR_Division')
      AND name      = N'DivisionNameEn'
)
BEGIN
    ALTER TABLE [core].[HR_Division]
        ADD [DivisionNameEn] NVARCHAR(200) NULL;
    PRINT 'Added core.HR_Division.DivisionNameEn';
END
ELSE
    PRINT 'core.HR_Division.DivisionNameEn already exists — skipped';
GO

-- 1-E  วันที่ sync ล่าสุดจาก API
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'core.HR_Division')
      AND name      = N'ApiSyncedDate'
)
BEGIN
    ALTER TABLE [core].[HR_Division]
        ADD [ApiSyncedDate] DATETIME NULL;
    PRINT 'Added core.HR_Division.ApiSyncedDate';
END
ELSE
    PRINT 'core.HR_Division.ApiSyncedDate already exists — skipped';
GO


-- ============================================================
-- PART 2 : เพิ่ม column ใหม่ใน core.HR_Department
-- ============================================================

-- 2-A  DepartmentCode จาก API
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'core.HR_Department')
      AND name      = N'ApiDepartmentCode'
)
BEGIN
    ALTER TABLE [core].[HR_Department]
        ADD [ApiDepartmentCode] NVARCHAR(50) NULL;
    PRINT 'Added core.HR_Department.ApiDepartmentCode';
END
ELSE
    PRINT 'core.HR_Department.ApiDepartmentCode already exists — skipped';
GO

-- 2-B  DepartmentId ของ API
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'core.HR_Department')
      AND name      = N'ApiDepartmentId'
)
BEGIN
    ALTER TABLE [core].[HR_Department]
        ADD [ApiDepartmentId] INT NULL;
    PRINT 'Added core.HR_Department.ApiDepartmentId';
END
ELSE
    PRINT 'core.HR_Department.ApiDepartmentId already exists — skipped';
GO

-- 2-C  ชื่อภาษาไทย (API ส่ง departmentNameTh)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'core.HR_Department')
      AND name      = N'DepartmentNameTh'
)
BEGIN
    ALTER TABLE [core].[HR_Department]
        ADD [DepartmentNameTh] NVARCHAR(200) NULL;
    PRINT 'Added core.HR_Department.DepartmentNameTh';
END
ELSE
    PRINT 'core.HR_Department.DepartmentNameTh already exists — skipped';
GO

-- 2-D  ชื่อภาษาอังกฤษ (API ส่ง departmentNameEn)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'core.HR_Department')
      AND name      = N'DepartmentNameEn'
)
BEGIN
    ALTER TABLE [core].[HR_Department]
        ADD [DepartmentNameEn] NVARCHAR(200) NULL;
    PRINT 'Added core.HR_Department.DepartmentNameEn';
END
ELSE
    PRINT 'core.HR_Department.DepartmentNameEn already exists — skipped';
GO

-- 2-E  วันที่ sync ล่าสุดจาก API
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'core.HR_Department')
      AND name      = N'ApiSyncedDate'
)
BEGIN
    ALTER TABLE [core].[HR_Department]
        ADD [ApiSyncedDate] DATETIME NULL;
    PRINT 'Added core.HR_Department.ApiSyncedDate';
END
ELSE
    PRINT 'core.HR_Department.ApiSyncedDate already exists — skipped';
GO


-- ============================================================
-- PART 3 : Stored Procedure สำหรับ Upsert ข้อมูลจาก API
--          รับข้อมูลผ่าน Table-Valued Parameter
-- ============================================================

-- 3-A  TVP type สำหรับ HR_Division
IF TYPE_ID(N'core.TVP_ApiDivision') IS NULL
BEGIN
    CREATE TYPE [core].[TVP_ApiDivision] AS TABLE
    (
        ApiDivisionId   INT            NOT NULL,
        ApiDivisionCode NVARCHAR(50)   NOT NULL,
        DivisionNameTh  NVARCHAR(200)  NULL,
        DivisionNameEn  NVARCHAR(200)  NULL
    );
    PRINT 'Created type core.TVP_ApiDivision';
END
GO

-- 3-B  TVP type สำหรับ HR_Department
IF TYPE_ID(N'core.TVP_ApiDepartment') IS NULL
BEGIN
    CREATE TYPE [core].[TVP_ApiDepartment] AS TABLE
    (
        ApiDepartmentId   INT            NOT NULL,
        ApiDepartmentCode NVARCHAR(50)   NOT NULL,
        ApiDivisionId     INT            NOT NULL,    -- ใช้ join กับ HR_Division
        DepartmentNameTh  NVARCHAR(200)  NULL,
        DepartmentNameEn  NVARCHAR(200)  NULL
    );
    PRINT 'Created type core.TVP_ApiDepartment';
END
GO

-- ============================================================
-- 3-C  SP : upsert HR_Division จาก API payload
-- ============================================================
IF OBJECT_ID(N'core.sp_SyncApiDivisions', 'P') IS NOT NULL
    DROP PROCEDURE [core].[sp_SyncApiDivisions];
GO

CREATE PROCEDURE [core].[sp_SyncApiDivisions]
    @Divisions [core].[TVP_ApiDivision] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- UPDATE rows ที่เคย sync มาก่อน (match ด้วย ApiDivisionId)
    UPDATE d
    SET
        d.ApiDivisionCode = src.ApiDivisionCode,
        d.DivisionNameTh  = src.DivisionNameTh,
        d.DivisionNameEn  = src.DivisionNameEn,
        d.ApiSyncedDate   = GETDATE()
    FROM [core].[HR_Division] d
    JOIN @Divisions src ON d.ApiDivisionId = src.ApiDivisionId;

    -- INSERT rows ใหม่ที่ยังไม่มีใน DB
    INSERT INTO [core].[HR_Division]
        (DivisionCode, DivisionName, ApiDivisionId, ApiDivisionCode, DivisionNameTh, DivisionNameEn, IsActive, CreatedDate, ApiSyncedDate)
    SELECT
        src.ApiDivisionCode,                              -- ใช้เป็น DivisionCode ด้วย
        COALESCE(src.DivisionNameTh, src.DivisionNameEn, src.ApiDivisionCode),
        src.ApiDivisionId,
        src.ApiDivisionCode,
        src.DivisionNameTh,
        src.DivisionNameEn,
        1,
        GETDATE(),
        GETDATE()
    FROM @Divisions src
    WHERE NOT EXISTS (
        SELECT 1 FROM [core].[HR_Division] d
        WHERE d.ApiDivisionId = src.ApiDivisionId
    );

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO


-- ============================================================
-- 3-D  SP : upsert HR_Department จาก API payload
-- ============================================================
IF OBJECT_ID(N'core.sp_SyncApiDepartments', 'P') IS NOT NULL
    DROP PROCEDURE [core].[sp_SyncApiDepartments];
GO

CREATE PROCEDURE [core].[sp_SyncApiDepartments]
    @Departments [core].[TVP_ApiDepartment] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- UPDATE rows ที่เคย sync มาก่อน
    UPDATE d
    SET
        d.ApiDepartmentCode = src.ApiDepartmentCode,
        d.DepartmentNameTh  = src.DepartmentNameTh,
        d.DepartmentNameEn  = src.DepartmentNameEn,
        d.ApiSyncedDate     = GETDATE()
    FROM [core].[HR_Department] d
    JOIN @Departments src ON d.ApiDepartmentId = src.ApiDepartmentId;

    -- INSERT rows ใหม่ โดย resolve DivisionID จาก ApiDivisionId
    INSERT INTO [core].[HR_Department]
        (DivisionID, DepartmentCode, DepartmentName, ApiDepartmentId, ApiDepartmentCode,
         DepartmentNameTh, DepartmentNameEn, IsActive, CreatedDate, ApiSyncedDate)
    SELECT
        div.DivisionID,
        src.ApiDepartmentCode,
        COALESCE(src.DepartmentNameTh, src.DepartmentNameEn, src.ApiDepartmentCode),
        src.ApiDepartmentId,
        src.ApiDepartmentCode,
        src.DepartmentNameTh,
        src.DepartmentNameEn,
        1,
        GETDATE(),
        GETDATE()
    FROM @Departments src
    -- หา DivisionID ใน DB โดย match กับ ApiDivisionId ที่ sync มาแล้ว
    LEFT JOIN [core].[HR_Division] div ON div.ApiDivisionId = src.ApiDivisionId
    WHERE NOT EXISTS (
        SELECT 1 FROM [core].[HR_Department] d
        WHERE d.ApiDepartmentId = src.ApiDepartmentId
    );

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO


-- ============================================================
-- PART 4 : UPDATE แบบ Manual (ใส่ข้อมูลจาก API ตรง ๆ)
--          ใช้เมื่อต้องการ sync ครั้งแรกหรือแก้ไขรายการ
-- ============================================================

/*
  ตัวอย่างข้อมูลจาก API (EmployeePosition):
  {
    "divisionId":     10,
    "divisionCode":   "DIV-01",
    "divisionNameTh": "ฝ่ายบริหาร",
    "divisionNameEn": "Management Division",
    "departmentId":   101,
    "departmentCode": "DEPT-001",
    "departmentNameTh": "แผนกการเงิน",
    "departmentNameEn": "Finance Department"
  }

  STEP 1 : sync HR_Division ก่อนเสมอ (เพราะ HR_Department FK → DivisionID)
*/

-- ── DIVISION SYNC (ตัวอย่าง — แทนที่ด้วยข้อมูลจริงจาก API) ──────────────
DECLARE @ApiDivisions [core].[TVP_ApiDivision];

INSERT INTO @ApiDivisions (ApiDivisionId, ApiDivisionCode, DivisionNameTh, DivisionNameEn)
VALUES
    (10, 'DIV-01', 'ฝ่ายบริหาร',            'Management Division'),
    (11, 'DIV-02', 'ฝ่ายการเงินและบัญชี',    'Finance & Accounting Division'),
    (12, 'DIV-03', 'ฝ่ายทรัพยากรบุคคล',     'Human Resources Division'),
    (13, 'DIV-04', 'ฝ่ายเทคโนโลยีสารสนเทศ', 'Information Technology Division');
    -- เพิ่มแถวตามข้อมูลจาก API

EXEC [core].[sp_SyncApiDivisions] @Divisions = @ApiDivisions;
GO

-- ── DEPARTMENT SYNC ───────────────────────────────────────────────────────
DECLARE @ApiDepartments [core].[TVP_ApiDepartment];

INSERT INTO @ApiDepartments (ApiDepartmentId, ApiDepartmentCode, ApiDivisionId, DepartmentNameTh, DepartmentNameEn)
VALUES
    (101, 'DEPT-001', 10, 'แผนกงานเลขานุการ',      'Secretariat Department'),
    (102, 'DEPT-002', 11, 'แผนกการเงิน',           'Finance Department'),
    (103, 'DEPT-003', 11, 'แผนกบัญชี',             'Accounting Department'),
    (104, 'DEPT-004', 12, 'แผนกสรรหา',             'Recruitment Department'),
    (105, 'DEPT-005', 13, 'แผนกพัฒนาระบบ',         'System Development Department');
    -- เพิ่มแถวตามข้อมูลจาก API

EXEC [core].[sp_SyncApiDepartments] @Departments = @ApiDepartments;
GO


-- ============================================================
-- PART 5 : ตรวจสอบผลลัพธ์
-- ============================================================

SELECT
    d.DivisionID,
    d.DivisionCode,
    d.DivisionName,
    d.ApiDivisionId,
    d.ApiDivisionCode,
    d.DivisionNameTh,
    d.DivisionNameEn,
    d.IsActive,
    d.ApiSyncedDate
FROM [core].[HR_Division] d
ORDER BY d.DivisionCode;

SELECT
    dep.DepartmentID,
    dep.DivisionID,
    div.DivisionCode,
    dep.DepartmentCode,
    dep.DepartmentName,
    dep.ApiDepartmentId,
    dep.ApiDepartmentCode,
    dep.DepartmentNameTh,
    dep.DepartmentNameEn,
    dep.IsActive,
    dep.ApiSyncedDate
FROM [core].[HR_Department] dep
JOIN [core].[HR_Division]   div ON div.DivisionID = dep.DivisionID
ORDER BY div.DivisionCode, dep.DepartmentCode;

-- ============================================================
-- Column ที่เพิ่มสรุป
-- ============================================================
/*
  core.HR_Division (column ใหม่):
  ├─ ApiDivisionId    INT           — DivisionId จาก Employee API
  ├─ ApiDivisionCode  NVARCHAR(50)  — DivisionCode จาก Employee API
  ├─ DivisionNameTh   NVARCHAR(200) — divisionNameTh จาก API
  ├─ DivisionNameEn   NVARCHAR(200) — divisionNameEn จาก API
  └─ ApiSyncedDate    DATETIME      — วันที่ sync ล่าสุด

  core.HR_Department (column ใหม่):
  ├─ ApiDepartmentId    INT           — DepartmentId จาก Employee API
  ├─ ApiDepartmentCode  NVARCHAR(50)  — DepartmentCode จาก Employee API
  ├─ DepartmentNameTh   NVARCHAR(200) — departmentNameTh จาก API
  ├─ DepartmentNameEn   NVARCHAR(200) — departmentNameEn จาก API
  └─ ApiSyncedDate      DATETIME      — วันที่ sync ล่าสุด
*/
