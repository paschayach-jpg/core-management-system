using System;

namespace CMS.Domain.Models
{
    /// <summary>ข้อมูลแผนก จาก core.HR_Department</summary>
    public class HrDepartment
    {
        public int      DepartmentID     { get; set; }
        public int      DivisionID       { get; set; }
        public string   DepartmentCode   { get; set; }
        /// <summary>ชื่อแผนก (legacy — ใช้ DepartmentNameTh แทน)</summary>
        public string   DepartmentName   { get; set; }
        public bool     IsActive         { get; set; }
        public DateTime CreatedDate      { get; set; }

        /// <summary>ระดับของแผนก เช่น 1A, 2B (จาก HR_Department.Level)</summary>
        public string   Level            { get; set; }

        /// <summary>ชื่อแผนกภาษาไทย</summary>
        public string   DepartmentNameTh { get; set; }

        /// <summary>ชื่อแผนกภาษาอังกฤษ</summary>
        public string   DepartmentNameEn { get; set; }

        /// <summary>Display name — ใช้ DepartmentNameTh ถ้ามี ไม่งั้น fallback DepartmentName</summary>
        public string   DisplayName
        {
            get { return !string.IsNullOrEmpty(DepartmentNameTh) ? DepartmentNameTh
                       : !string.IsNullOrEmpty(DepartmentName)   ? DepartmentName
                       : DepartmentCode; }
        }
    }
}
