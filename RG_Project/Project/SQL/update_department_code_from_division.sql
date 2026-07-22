-- ============================================================
-- STEP 1: รันบน CMS_DB — ดึง mapping DepartmentName -> DepartmentCode
-- ============================================================
SELECT DepartmentCode, DepartmentName
FROM EMS.m_Department
ORDER BY DepartmentName;


-- ============================================================
-- STEP 2: เชื่อมต่อ Claims DB แล้วรัน
--         (เปิด New Query Connection ใหม่ชี้ไปที่ Claims DB)
-- ============================================================
USE BIS_Claim;   -- << เปลี่ยนเป็นชื่อ DB Claims จริง ถ้าต่างกัน
GO

-- สร้าง temp mapping จากผล STEP 1
CREATE TABLE #dept (DepartmentCode NVARCHAR(20), DepartmentName NVARCHAR(200));

INSERT INTO #dept (DepartmentCode, DepartmentName) VALUES
-- วาง rows จาก STEP 1 ที่นี่ เช่น:
-- ('D001', 'แผนกบัญชี'),
-- ('D002', 'แผนกการตลาด'),
('', '');  -- placeholder — ลบออกเมื่อใส่ข้อมูลจริง

-- ตรวจสอบก่อน UPDATE
SELECT
    pm.i_member_id,
    pm.n_member_name,
    pm.department_code   AS เดิม,
    d.DepartmentCode     AS ใหม่
FROM dbo.t_project_member pm
INNER JOIN #dept d ON d.DepartmentName = LTRIM(RTRIM(pm.n_member_name));

-- UPDATE จริง (uncomment เมื่อตรวจสอบแล้ว)
-- UPDATE pm
-- SET    pm.department_code = d.DepartmentCode
-- FROM   dbo.t_project_member pm
-- INNER JOIN #dept d ON d.DepartmentName = LTRIM(RTRIM(pm.n_member_name));

DROP TABLE #dept;
