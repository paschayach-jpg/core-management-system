using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// 1 row = ผู้อนุมัติดาวน์โหลดไฟล์ 1 คน ของแผนก (ScopeType 'D') หรือฝ่าย (ScopeType 'V')
    /// OrgCode = รหัส HR Master (DepartmentCode / DivisionCode)
    /// </summary>
    public class DownloadApproverConfig
    {
        public const string ScopeDepartment = "D";
        public const string ScopeDivision   = "V";

        public int      ConfigID       { get; set; }
        public string   ScopeType      { get; set; }
        public string   OrgCode        { get; set; }
        public int      ApproverUserID { get; set; }
        public int      SortOrder      { get; set; }
        public bool     IsActive       { get; set; }
        public int?     CreatedBy      { get; set; }
        public DateTime CreatedDate    { get; set; }
        public int?     UpdatedBy      { get; set; }
        public DateTime? UpdatedDate   { get; set; }

        // ── Display (transient — filled from EMS.UserAccount) ────────────────
        public string ApproverName   { get; set; }
        public string PositionName   { get; set; }
        public string LevelCode      { get; set; }
        public string DepartmentName { get; set; }
    }
}
