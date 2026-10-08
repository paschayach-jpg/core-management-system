using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// สิทธิ์ดาวน์โหลดได้ทันที (ไม่ต้องขออนุมัติ) ของผู้ใช้ 1 คน แยกตามระดับชั้นความลับ
    /// </summary>
    public class DownloadUserPermission
    {
        public int      PermissionID { get; set; }
        public int      UserID       { get; set; }
        public bool     CanHC        { get; set; }
        public bool     CanSC        { get; set; }
        public bool     CanNC        { get; set; }
        public bool     CanPC        { get; set; }
        public bool     IsActive     { get; set; }
        public int?     CreatedBy    { get; set; }
        public DateTime CreatedDate  { get; set; }
        public int?     UpdatedBy    { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // ── Display (transient — filled from EMS.UserAccount) ────────────────
        public string ADUserName     { get; set; }
        public string DisplayName    { get; set; }
        public string PositionName   { get; set; }
        public string LevelCode      { get; set; }
        public string DepartmentName { get; set; }

        /// <summary>1=HC, 2=SC, 3=NC, 4=PC</summary>
        public bool AllowsLevel(int confidentialityLevel)
        {
            switch (confidentialityLevel)
            {
                case 1: return CanHC;
                case 2: return CanSC;
                case 3: return CanNC;
                case 4: return CanPC;
                default: return false;
            }
        }
    }
}
