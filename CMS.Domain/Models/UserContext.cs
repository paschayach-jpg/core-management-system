using System.Collections.Generic;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Represents the authenticated user''s session context, resolved from Active Directory.
    /// </summary>
    public class UserContext
    {
        public int UserID { get; set; }
        public string ADUserName { get; set; }

        /// <summary>ชื่อเต็มสำหรับแสดงผล (จาก UserAccount.DisplayName) — ใช้โดยโมดูล RGP เป็นหลัก</summary>
        public string DisplayName { get; set; }

        /// <summary>1=Officer, 2=SrOfficer, 3=Manager, 4=Admin</summary>
        public int AppRole { get; set; }

        public int OrgUnitID { get; set; }

        public string MachineName { get; set; }
        public string IPAddress { get; set; }
        public string SessionID { get; set; }

        /// <summary>LevelCode ของตำแหน่ง เช่น 1A, 2B — ใช้ resolve FolderAuthority</summary>
        public string LevelCode { get; set; }

        /// <summary>ชื่อตำแหน่งงาน</summary>
        public string PositionName { get; set; }

        /// <summary>รหัสแผนก</summary>
        public string DepartmentCode { get; set; }

        /// <summary>ชื่อแผนก</summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// สิทธิ์ที่ Resolve แล้วจาก FolderAuthorityConfig ตาม ตำแหน่ง/แผนก/ฝ่าย
        /// Key = ConfidentialityLevel (1=HC, 2=SC, 3=NC, 4=PC)
        /// Populated ใน Program.cs หลัง login สำเร็จ
        /// </summary>
        public Dictionary<int, FolderAuthorityConfig> Authority { get; set; }

        /// <summary>รหัสแผนก (int) — ใช้ match กับ s_FolderPermission.PrincipalID</summary>
        public int? DepartmentId { get; set; }

        /// <summary>รหัสฝ่าย (int) — ใช้ match กับ s_FolderPermission.PrincipalID</summary>
        public int? DivisionId { get; set; }

        /// <summary>
        /// ACL pre-loaded ตอน login — key = FolderID, value = ACL entries ทั้งหมดของ folder นั้น
        /// ใช้ pre-warm PermissionService._aclCache ก่อน request แรก
        /// เปลี่ยนสิทธิ์ใหม่ต้อง re-login เพื่อรับ cache ใหม่
        /// </summary>
        public Dictionary<int, List<FolderPermission>> SessionAcl { get; set; }

        public FolderAuthorityConfig GetAuthority(int level)
        {
            if (Authority == null) return null;
            FolderAuthorityConfig cfg;
            Authority.TryGetValue(level, out cfg);
            return cfg;
        }

        /// <summary>
        /// ตรวจสอบว่า User มีสิทธิ์ View folder ที่มี ConfidentialityLevel นี้
        /// Level 4 (PC — ทั่วไป): สิทธิ์ดูเป็นสิทธิ์พื้นฐานของทุกคน — ถ้าไม่มี FolderAuthorityConfig
        /// row ที่ match เลย (cfg == null) ให้ผ่านโดย default; ถ้ามี row ที่ match แต่ตั้ง CanView=false
        /// ไว้ชัดเจน ให้เคารพค่านั้น (ไม่ override)
        /// </summary>
        public bool CanViewFolder(int confidentialityLevel)
        {
            var cfg = GetAuthority(confidentialityLevel);
            if (cfg == null)
                return confidentialityLevel == 4;

            return cfg.CanView;
        }

        /// <summary>ตรวจสอบว่า User มีสิทธิ์ Add ใน folder ที่มี ConfidentialityLevel นี้</summary>
        public bool CanAddToFolder(int confidentialityLevel)
        {
            var cfg = GetAuthority(confidentialityLevel);
            return cfg != null && cfg.CanAdd;
        }

        /// <summary>ตรวจสอบว่า User มีสิทธิ์ Edit ใน folder ที่มี ConfidentialityLevel นี้</summary>
        public bool CanEditFolder(int confidentialityLevel)
        {
            var cfg = GetAuthority(confidentialityLevel);
            return cfg != null && cfg.CanEdit;
        }

        /// <summary>ตรวจสอบว่า User มีสิทธิ์ Approve ใน folder ที่มี ConfidentialityLevel นี้</summary>
        public bool CanApproveFolder(int confidentialityLevel)
        {
            var cfg = GetAuthority(confidentialityLevel);
            return cfg != null && cfg.CanApprove;
        }

        /// <summary>ตรวจสอบว่า User มีสิทธิ์ Download ใน folder ที่มี ConfidentialityLevel นี้</summary>
        public bool CanDownloadFolder(int confidentialityLevel)
        {
            if (AppRole >= 3) return true;
            var cfg = GetAuthority(confidentialityLevel);
            return cfg != null && cfg.CanDownload;
        }

        // -----------------------------------------------------------------------
        // Aliases
        // -----------------------------------------------------------------------

        public int UserId
        {
            get { return UserID; }
            set { UserID = value; }
        }

        public string WorkstationId
        {
            get { return MachineName; }
            set { MachineName = value; }
        }

        public UserRole Role
        {
            get { return (UserRole)AppRole; }
            set { AppRole = (int)value; }
        }

        /// <summary>Alias for ADUserName (domain\username).</summary>
        public string AdIdentity
        {
            get { return ADUserName; }
            set { ADUserName = value; }
        }
    }
}