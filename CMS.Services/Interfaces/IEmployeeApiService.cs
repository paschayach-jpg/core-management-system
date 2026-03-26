using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service สำหรับดึงข้อมูลพนักงานจาก Employee API
    /// </summary>
    public interface IEmployeeApiService
    {
        /// <summary>ดึงพนักงานทั้งหมด</summary>
        IEnumerable<EmployeeSummary> GetAllEmployees();

        /// <summary>ดึงพนักงานตาม employeeCode</summary>
        EmployeeApiResponse GetByCode(string employeeCode);

        /// <summary>ดึงรายชื่อแผนก (department) ที่ไม่ซ้ำกัน</summary>
        IEnumerable<DepartmentSummary> GetDepartments();

        /// <summary>ดึงรายชื่อฝ่าย (division) ที่ไม่ซ้ำกัน</summary>
        IEnumerable<DivisionSummary> GetDivisions();

        /// <summary>ดึงพนักงานตาม departmentId</summary>
        IEnumerable<EmployeeSummary> GetByDepartment(int departmentId);

        /// <summary>ดึงพนักงานตาม divisionId</summary>
        IEnumerable<EmployeeSummary> GetByDivision(int divisionId);
    }

    public class DepartmentSummary
    {
        public int    DepartmentId   { get; set; }
        public string DepartmentCode { get; set; }
        public string NameTh         { get; set; }
        public string NameEn         { get; set; }
        public string DisplayName    { get { return !string.IsNullOrEmpty(NameTh) ? NameTh : NameEn; } }
    }

    public class DivisionSummary
    {
        public int    DivisionId   { get; set; }
        public string DivisionCode { get; set; }
        public string NameTh       { get; set; }
        public string NameEn       { get; set; }
        public string DisplayName  { get { return !string.IsNullOrEmpty(NameTh) ? NameTh : NameEn; } }
    }
}
