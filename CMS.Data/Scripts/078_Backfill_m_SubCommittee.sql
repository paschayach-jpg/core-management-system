-- ============================================================
-- Script  : 078_Backfill_m_SubCommittee.sql
-- Purpose : Backfill ข้อมูล SubCommittee ที่มีอยู่ก่อนหน้าใน core.OrgUnit
--           เข้า m_SubCommittee (เฉพาะที่ยังไม่มีในตาราง)
--           OrgUnitType = 1 = SubCommittee
-- ============================================================

INSERT INTO m_SubCommittee
    (OrgUnitID, SubCommitteeCode, SubCommitteeNameTH, SubCommitteeNameEN,
     IsActive, IsDeleted, CreatedBy, CreatedDate)
SELECT
    ou.OrgUnitID,
    ou.OrgUnitCode,
    ou.OrgUnitNameTH,
    ou.OrgUnitNameEN,
    ou.IsActive,
    ou.IsDeleted,
    'SYSTEM_BACKFILL',   -- ADUserName snapshot (ไม่มีข้อมูลเดิม)
    ou.CreatedDate
FROM core.OrgUnit ou
WHERE ou.OrgUnitType = 1
  AND NOT EXISTS (
        SELECT 1
        FROM   m_SubCommittee sc
        WHERE  sc.OrgUnitID = ou.OrgUnitID
  );

PRINT CAST(@@ROWCOUNT AS NVARCHAR) + ' row(s) backfilled into m_SubCommittee.';
