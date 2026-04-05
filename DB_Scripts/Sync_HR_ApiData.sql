-- ============================================================
-- Sync ข้อมูล Division / Department จาก Employee API (ข้อมูลจริง)
-- Database : SCS  |  Schema : core
-- หมายเหตุ : รัน Migration_HR_ApiCode_Sync.sql ก่อน เพื่อ ADD column ใหม่
-- ============================================================
USE [SCS];
GO

-- ============================================================
-- STEP 1 : เตรียม Temp Table สำหรับข้อมูล Division จาก API
-- ============================================================
IF OBJECT_ID('tempdb..#ApiDivision') IS NOT NULL DROP TABLE #ApiDivision;

CREATE TABLE #ApiDivision (
    ApiDivisionId   INT           NOT NULL PRIMARY KEY,
    ApiDivisionCode NVARCHAR(10)  NOT NULL,
    DivisionNameTh  NVARCHAR(200) NULL,
    DivisionNameEn  NVARCHAR(200) NULL
);

INSERT INTO #ApiDivision (ApiDivisionId, ApiDivisionCode, DivisionNameTh, DivisionNameEn)
VALUES
    ( 1, '00', N'ขึ้นตรงกรรมการผู้จัดการ',      N'Depend on Managing Director'),
    ( 2, '01', N'รับประกัน',                     N'Underwrite'),
    ( 3, '02', N'สินไหมทดแทน',                   N'Claim'),
    ( 4, '03', N'การตลาด',                        N'Marketing'),
    ( 5, '04', N'MISและคณิตศาสตร์ประกันภัย',     N'Management Information Systems And Actuary'),
    ( 6, '05', N'เทคโนโลยีสารสนเทศ',              N'Information and Technology'),
    ( 7, '06', N'บัญชีและการเงิน',               N'Finance and Accounting'),
    ( 8, '07', N'สำนักกรรมการผู้จัดการ',          N'Board of Managing Directors'),
    ( 9, '08', N'ทรัพยากรบุคคล',                 N'Human Resource'),
    (10, '09', N'สำนักกรรมการผู้จัดการ(อาคาร)',  N'Board of Managing Directors (Building)');
-- *** ถ้า API มี Division เพิ่มเติม ให้เพิ่มแถวด้านบน ***

-- ============================================================
-- STEP 2 : MERGE ข้อมูล Division เข้า core.HR_Division
--          Match ด้วย ApiDivisionId  (ไม่แตะ DivisionCode/DivisionName เดิม)
-- ============================================================
MERGE [core].[HR_Division] AS tgt
USING #ApiDivision           AS src
   ON tgt.ApiDivisionId = src.ApiDivisionId
WHEN MATCHED THEN
    UPDATE SET
        tgt.ApiDivisionCode = src.ApiDivisionCode,
        tgt.DivisionNameTh  = src.DivisionNameTh,
        tgt.DivisionNameEn  = src.DivisionNameEn,
        tgt.ApiSyncedDate   = GETDATE()
WHEN NOT MATCHED BY TARGET THEN
    -- INSERT เฉพาะเมื่อยังไม่มีแถวที่ ApiDivisionId ตรงกัน
    -- DivisionCode / DivisionName ดึงจาก API ไว้ก่อน (admin แก้ทีหลังได้)
    INSERT (DivisionCode, DivisionName, ApiDivisionId, ApiDivisionCode,
            DivisionNameTh, DivisionNameEn, IsActive, CreatedDate, ApiSyncedDate)
    VALUES (src.ApiDivisionCode,
            COALESCE(src.DivisionNameTh, src.DivisionNameEn, src.ApiDivisionCode),
            src.ApiDivisionId, src.ApiDivisionCode,
            src.DivisionNameTh, src.DivisionNameEn,
            1, GETDATE(), GETDATE());

PRINT CONCAT('HR_Division synced: ', @@ROWCOUNT, ' row(s) affected');
GO


-- ============================================================
-- STEP 3 : เตรียม Temp Table สำหรับข้อมูล Department จาก API
--          (unique by departmentId — ใช้ข้อมูล position ล่าสุด)
-- ============================================================
IF OBJECT_ID('tempdb..#ApiDepartment') IS NOT NULL DROP TABLE #ApiDepartment;

CREATE TABLE #ApiDepartment (
    ApiDepartmentId   INT           NOT NULL PRIMARY KEY,
    ApiDepartmentCode NVARCHAR(10)  NOT NULL,
    ApiDivisionId     INT           NOT NULL,
    DepartmentNameTh  NVARCHAR(200) NULL,
    DepartmentNameEn  NVARCHAR(200) NULL
);

INSERT INTO #ApiDepartment (ApiDepartmentId, ApiDepartmentCode, ApiDivisionId, DepartmentNameTh, DepartmentNameEn)
VALUES
-- divisionId 1 — ขึ้นตรงกรรมการผู้จัดการ
    ( 1, '00',  1, N'กรรมการ',                     N'Director'),
-- divisionId 2 — รับประกัน
    ( 4, '00',  2, N'รับประกัน',                    N'Underwrite'),
    ( 5, '01',  2, N'รับประกัน 1',                  N'Underwrite 1'),
    ( 6, '02',  2, N'รับประกัน 2',                  N'Underwrite 2'),
    ( 8, '04',  2, N'รับประกัน 4',                  N'Underwrite 4'),
    ( 9, '05',  2, N'รับประกัน 5',                  N'Underwrite 5'),
    (10, '11',  2, N'Risk Survey',                   N'Risk Survey'),
    (11, '06',  2, N'รับประกัน 6',                  N'Special Project 6'),
-- divisionId 3 — สินไหมทดแทน
    (13, '01',  3, N'สำรวจภัย',                     N'Explore the Dangers'),
    (14, '02',  3, N'สินไหมสาย 1',                  N'Claim Line 1'),
    (15, '04',  3, N'เรียกร้อง',                    N'Claim'),
    (16, '05',  3, N'สินไหมสาย 2',                  N'Claim Line 2'),
    (58, '03',  3, N'รับแจ้ง',                      N'Notification'),
-- divisionId 4 — การตลาด
    (18, '01',  4, N'พัฒนาธุรกิจ 1',               N'Business Development 1'),
    (20, '03',  4, N'พัฒนาธุรกิจ 3',               N'Business Development 3'),
    (22, '05',  4, N'พัฒนาธุรกิจ 5',               N'Business Development 5'),
    (25, '08',  4, N'พัฒนาธุรกิจ 8',               N'Business Development 8'),
    (27, '22',  4, N'พัฒนาตัวแทน',                  N'Agent Development'),
    (28, '31',  4, N'กิจการสาขาและศูนย์ตัวแทน',    N'Branch Management'),
    (32, '35',  4, N'สาขาชลบุรี',                   N'Chonburi Branch'),
    (33, '36',  4, N'สาขาคลองหลวง',                 N'Klongluang Branch'),
    (34, '37',  4, N'สาขาย่อยลำปาง',               N'Lampang Sub Branch'),
-- divisionId 5 — MIS
    (36, '01',  5, N'MIS',                           N'MIS'),
-- divisionId 6 — เทคโนโลยีสารสนเทศ
    (38, '01',  6, N'เทคโนโลยีสารสนเทศ',            N'IT'),
-- divisionId 7 — บัญชีและการเงิน
    (39, '00',  7, N'ฝ่ายบัญชีและการเงิน',          N'Finance and Accounting'),
    (40, '01',  7, N'บัญชี',                         N'Accounting'),
    (41, '02',  7, N'การเงิน',                       N'Finance'),
-- divisionId 8 — สำนักกรรมการผู้จัดการ
    (35, '00',  8, N'ส่วนกลางสำนัก',               N'General Office'),
    (42, '01',  8, N'งานเลขาและประชาสัมพันธ์',     N'Secretary'),
    (43, '02',  8, N'สารบรรณ',                      N'Correspondence'),
    (44, '03',  8, N'กฎหมาย',                       N'Compliance'),
    (46, '11',  8, N'แผนกจัดซื้อ ทั่วไป',          N'General Procurement'),
    (63, '10',  8, N'แผนกจัดซื้ออะไหล่',           N'Spare Parts Procurement'),
-- divisionId 9 — ทรัพยากรบุคคล
    (48, '00',  9, N'ส่วนกลางบุคคล',               N'HR General'),
-- divisionId 10 — สำนักกรรมการผู้จัดการ(อาคาร)
    (50, '00', 10, N'อาคารและทรัพย์สิน',            N'Building');
-- *** ถ้า API มี Department เพิ่มเติม (ข้อมูลถูกตัดทอน) ให้เพิ่มแถวด้านบน ***

-- ============================================================
-- STEP 4 : MERGE ข้อมูล Department เข้า core.HR_Department
--          Match ด้วย ApiDepartmentId
--          Resolve DivisionID โดย join กับ HR_Division ผ่าน ApiDivisionId
-- ============================================================
MERGE [core].[HR_Department] AS tgt
USING (
    SELECT
        d.ApiDepartmentId,
        d.ApiDepartmentCode,
        d.DepartmentNameTh,
        d.DepartmentNameEn,
        d.ApiDivisionId,
        div.DivisionID          AS ResolvedDivisionID
    FROM #ApiDepartment d
    LEFT JOIN [core].[HR_Division] div ON div.ApiDivisionId = d.ApiDivisionId
) AS src ON tgt.ApiDepartmentId = src.ApiDepartmentId
WHEN MATCHED THEN
    UPDATE SET
        tgt.ApiDepartmentCode = src.ApiDepartmentCode,
        tgt.DepartmentNameTh  = src.DepartmentNameTh,
        tgt.DepartmentNameEn  = src.DepartmentNameEn,
        tgt.ApiSyncedDate     = GETDATE()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (DivisionID, DepartmentCode, DepartmentName,
            ApiDepartmentId, ApiDepartmentCode,
            DepartmentNameTh, DepartmentNameEn,
            IsActive, CreatedDate, ApiSyncedDate)
    VALUES (src.ResolvedDivisionID,
            src.ApiDepartmentCode,
            COALESCE(src.DepartmentNameTh, src.DepartmentNameEn, src.ApiDepartmentCode),
            src.ApiDepartmentId, src.ApiDepartmentCode,
            src.DepartmentNameTh, src.DepartmentNameEn,
            1, GETDATE(), GETDATE());

PRINT CONCAT('HR_Department synced: ', @@ROWCOUNT, ' row(s) affected');
GO


-- ============================================================
-- STEP 5 : ตรวจสอบผลลัพธ์
-- ============================================================

-- Division
SELECT
    div.DivisionID,
    div.DivisionCode      AS DB_DivisionCode,
    div.DivisionName      AS DB_DivisionName,
    div.ApiDivisionId,
    div.ApiDivisionCode,
    div.DivisionNameTh,
    div.DivisionNameEn,
    div.IsActive,
    div.ApiSyncedDate
FROM [core].[HR_Division] div
ORDER BY div.ApiDivisionId;

-- Department พร้อมชื่อฝ่าย
SELECT
    dep.DepartmentID,
    div.DivisionID,
    div.DivisionNameTh     AS FlyName,
    dep.DepartmentCode     AS DB_DeptCode,
    dep.DepartmentName     AS DB_DeptName,
    dep.ApiDepartmentId,
    dep.ApiDepartmentCode,
    dep.DepartmentNameTh,
    dep.DepartmentNameEn,
    dep.IsActive,
    dep.ApiSyncedDate
FROM [core].[HR_Department] dep
JOIN [core].[HR_Division]   div ON div.DivisionID = dep.DivisionID
ORDER BY div.ApiDivisionId, dep.ApiDepartmentId;

-- รายการ Department ที่ยัง resolve DivisionID ไม่ได้ (ถ้ามี)
SELECT dep.*
FROM [core].[HR_Department] dep
WHERE dep.ApiDepartmentId IS NOT NULL
  AND dep.DivisionID IS NULL;
GO
