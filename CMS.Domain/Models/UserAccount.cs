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
        /// <summary>รหัสพนักงาน (employeeCode จาก Employee API)</summary>
        public string EmployeeCode { get; set; }

        /// <summary>ID แผนก (departmentId จาก Employee API)</summary>
        public int? DepartmentId { get; set; }

        /// <summary>รหัสแผนก (departmentCode)</summary>
        public string DepartmentCode { get; set; }

        /// <summary>ชื่อแผนก (departmentNameTh)</summary>
        public string DepartmentName { get; set; }

        /// <summary>ID ฝ่าย (divisionId จาก Employee API)</summary>
        public int? DivisionId { get; set; }

        /// <summary>รหัสฝ่าย (divisionCode)</summary>
        public string DivisionCode { get; set; }

        /// <summary>ชื่อฝ่าย (divisionNameTh)</summary>
        public string DivisionName { get; set; }

        /// <summary>ตำแหน่งงาน</summary>
        public string PositionName { get; set; }

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