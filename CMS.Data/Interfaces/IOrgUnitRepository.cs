using System.Collections.Generic;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IOrgUnitRepository
    {
        // ── Many-to-Many: SubCommittee ↔ Division ─────────────────────────────

        /// <summary>เพิ่ม/เปิดใช้งาน relationship ระหว่าง Division กับ SubCommittee</summary>
        void LinkDivisionToSubCommittee(int divisionId, int subCommitteeId, int createdBy);

        /// <summary>ตรวจสอบว่า Division นี้เชื่อมกับ SubCommittee นี้แล้วหรือยัง (FK หรือ Relationship)</summary>
        bool IsDivisionLinkedToSubCommittee(int divisionId, int subCommitteeId);

        /// <summary>ดึง relationship ที่ active ทั้งหมด (สำหรับ build tree)</summary>
        IEnumerable<OrgUnitRelationship> GetAllActiveRelationships();

        /// <summary>ปิดใช้งาน relationship ระหว่าง Division กับ SubCommittee (IsActive = 0) — คืน true ถ้ามีแถวถูกปิด</summary>
        bool UnlinkDivisionFromSubCommittee(int divisionId, int subCommitteeId);

        /// <summary>
        /// ทำให้ Division เป็นฝ่ายอิสระ: ParentOrgUnitID = NULL และปิด relationship ทั้งหมด (transaction เดียว)
        /// — คืนจำนวน relationship ที่ถูกปิด
        /// </summary>
        int DetachDivisionToStandalone(int divisionId);


        int Insert(OrgUnit orgUnit);
        void Update(OrgUnit orgUnit);
        OrgUnit GetById(int orgUnitId);
        OrgUnit GetByCode(string orgUnitCode);
        bool ExistsByCode(string orgUnitCode);

        /// <summary>
        /// ตรวจสอบว่ามีชื่อ OrgUnitNameTH ซ้ำกันภายใต้ Parent เดียวกันหรือไม่
        /// excludeOrgUnitId ใช้สำหรับกรณี Update เพื่อยกเว้นตัวเองออกจากการตรวจ
        /// </summary>
        bool ExistsByNameUnderParent(string nameTH, int? parentId, int? excludeOrgUnitId = null);

        IEnumerable<OrgUnit> GetHierarchy();
        IEnumerable<OrgUnit> GetByConfidentialityLevel(ConfidentialityLevel level);
        bool HasActiveDependents(int orgUnitId);
        void SoftDelete(int orgUnitId, int deletedBy);

        /// <summary>Alias for SoftDelete — deactivates the OrgUnit (no deletedBy required).</summary>
        void Deactivate(int orgUnitId);

        /// <summary>
        /// Cascade soft-delete: soft-deletes all Folders, detaches UserAccounts,
        /// then soft-deletes the OrgUnit itself — bypasses the active-dependents guard.
        /// </summary>
        void ForceDeleteWithDependents(int orgUnitId, int deletedBy);

        // ── m_SubCommittee ─────────────────────────────────────────────────────

        /// <summary>
        /// Insert ข้อมูล SubCommittee เข้า m_SubCommittee (เรียกหลัง Insert EMS.OrgUnit สำเร็จ)
        /// createdByADUser ใช้ ADUserName (string snapshot) ตาม Audit Stamp rule
        /// </summary>
        void InsertSubCommitteeRecord(int orgUnitId, string code, string nameTH, string nameEN, string createdByADUser);

        /// <summary>Sync soft-delete ของ m_SubCommittee ให้ตรงกับ EMS.OrgUnit</summary>
        void SoftDeleteSubCommitteeRecord(int orgUnitId, string deletedByADUser);
    }
}

