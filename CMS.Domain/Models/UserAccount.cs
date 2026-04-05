using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// CMS user account linked to Active Directory and Employee API.
    /// Maps to core.UserAccount in SCS database.
    /// </summary>
    public class UserAccount
    {
        public int UserID { get; set; }
        public string ADUserName { get; set; }
        public string DisplayName { get; set; }

        /// <summary>PDPA: stored only with consent.</summary>
        public string Email { get; set; }

        /// <summary>1=Officer, 2=SrOfficer, 3=Manager, 4=Admin</summary>
        public int AppRole { get; set; }

        /// <summary>OrgUnitID จาก DB (legacy)</summary>
        public int OrgUnitID { get; set; }

        public bool ConsentGiven { get; set; }
        public DateTime? ConsentDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        // ── Employee API fields ──────────────────────────────────────────────
        /// <summary>รหัสพนักงาน (employeeCode จาก AD API)</summary>
        public string EmployeeCode { get; set; }

        /// <summary>คำนำหน้าชื่อ EN (Mr., Miss, Mrs.)</summary>
        public string EmployeeTitleNameEn { get; set; }

        /// <summary>ชื่อ EN</summary>
        public string EmployeeFirstNameEn { get; set; }

        /// <summary>นามสกุล EN</summary>
        public string EmployeeLastNameEn { get; set; }

        /// <summary>คำนำหน้าชื่อ TH (นาย, นางสาว)</summary>
        public string EmployeeTitleNameTh { get; set; }

        /// <summary>ชื่อ TH</summary>
        public string EmployeeFirstNameTh { get; set; }

        /// <summary>นามสกุล TH</summary>
        public string EmployeeLastNameTh { get; set; }

        /// <summary>ID แผนก (departmentId จาก AD API)</summary>
        public int? DepartmentId { get; set; }

        /// <summary>รหัสแผนก</summary>
        public string DepartmentCode { get; set; }

        /// <summary>ชื่อแผนก TH</summary>
        public string DepartmentName { get; set; }

        /// <summary>ชื่อแผนก EN</summary>
        public string DepartmentNameEn { get; set; }

        /// <summary>ID ฝ่าย (divisionId จาก AD API)</summary>
        public int? DivisionId { get; set; }

        /// <summary>รหัสฝ่าย</summary>
        public string DivisionCode { get; set; }

        /// <summary>ชื่อฝ่าย TH</summary>
        public string DivisionName { get; set; }

        /// <summary>ชื่อฝ่าย EN</summary>
        public string DivisionNameEn { get; set; }

        /// <summary>ตำแหน่งงาน</summary>
        public string PositionName { get; set; }

        /// <summary>รหัสระดับตำแหน่ง เช่น 2B, 3A — ใช้ match กับ FolderAuthorityConfig</summary>
        public string LevelCode { get; set; }

        /// <summary>IP Address ล่าสุดที่ login หรือ register</summary>
        public string LastLoginIP { get; set; }

        /// <summary>วันที่อัพเดทข้อมูลล่าสุด</summary>
        public DateTime? UpdatedAt { get; set; }

        // ── Aliases ──────────────────────────────────────────────────────────
        public string AdIdentity { get { return ADUserName; } set { ADUserName = value; } }

        public int UserId
        {
            get { return UserID; }
            set { UserID = value; }
        }

        public UserRole Role
        {
            get { return (UserRole)AppRole; }
            set { AppRole = (int)value; }
        }

        /// <summary>ชื่อเต็ม TH (คำนำหน้า + ชื่อ + นามสกุล)</summary>
        public string FullNameTh
        {
            get
            {
                return string.Format("{0}{1} {2}",
                    EmployeeTitleNameTh ?? "",
                    EmployeeFirstNameTh ?? "",
                    EmployeeLastNameTh  ?? "").Trim();
            }
        }

        /// <summary>ชื่อเต็ม EN (Title + First + Last)</summary>
        public string FullNameEn
        {
            get
            {
                return string.Format("{0}{1} {2}",
                    EmployeeTitleNameEn ?? "",
                    EmployeeFirstNameEn ?? "",
                    EmployeeLastNameEn  ?? "").Trim();
            }
        }

        /// <summary>ชื่อแผนก/ฝ่ายสำหรับแสดงผล</summary>
        public string OrgDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(DepartmentName))
                    return string.Format("{0} / {1}", DivisionName ?? DivisionCode, DepartmentName);
                if (!string.IsNullOrEmpty(DivisionName))
                    return DivisionName;
                return string.Empty;
            }
        }
    }
}