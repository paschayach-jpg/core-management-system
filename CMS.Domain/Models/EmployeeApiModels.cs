using System;
using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// ข้อมูลพนักงานที่ได้จาก Employee API
    /// </summary>
    public class EmployeeApiResponse
    {
        public string EmployeeCode { get; set; }
        public int    PrefixId     { get; set; }
        public string FullNameEn   { get; set; }
        public string FullNameTh   { get; set; }
        public string Company      { get; set; }
        public string Remark       { get; set; }
        public List<EmployeePosition> Positions { get; set; }

        /// <summary>ชื่อแสดงผล (ไทย — อังกฤษ)</summary>
        public string DisplayName
        {
            get
            {
                string th = FullNameTh ?? string.Empty;
                string en = FullNameEn ?? string.Empty;
                if (!string.IsNullOrEmpty(th) && !string.IsNullOrEmpty(en))
                    return string.Format("{0} ({1})", th, en);
                return !string.IsNullOrEmpty(th) ? th : en;
            }
        }

        /// <summary>ตำแหน่งหลัก (position แรก)</summary>
        public EmployeePosition PrimaryPosition
        {
            get { return Positions != null && Positions.Count > 0 ? Positions[0] : null; }
        }
    }

    public class EmployeePosition
    {
        public int      Id               { get; set; }
        public int      DepartmentId     { get; set; }
        public string   DepartmentCode   { get; set; }
        public string   DepartmentNameEn { get; set; }
        public string   DepartmentNameTh { get; set; }
        public int      DivisionId       { get; set; }
        public string   DivisionCode     { get; set; }
        public string   DivisionNameEn   { get; set; }
        public string   DivisionNameTh   { get; set; }
        public string   PositionName     { get; set; }
        public string   Level            { get; set; }
        public string   Remark           { get; set; }
        public DateTime? DateBegin       { get; set; }
        public DateTime? DateEnd         { get; set; }

        public string DepartmentDisplay
        {
            get { return !string.IsNullOrEmpty(DepartmentNameTh) ? DepartmentNameTh : DepartmentNameEn ?? string.Empty; }
        }

        public string DivisionDisplay
        {
            get { return !string.IsNullOrEmpty(DivisionNameTh) ? DivisionNameTh : DivisionNameEn ?? string.Empty; }
        }
    }

    /// <summary>ข้อมูลสรุปสำหรับใช้ใน dropdown / permission selector</summary>
    public class EmployeeSummary
    {
        public string EmployeeCode     { get; set; }
        public string DisplayName      { get; set; }
        public string DepartmentCode   { get; set; }
        public string DepartmentNameTh { get; set; }
        public string DepartmentNameEn { get; set; }
        public int    DepartmentId     { get; set; }
        public string DivisionCode     { get; set; }
        public string DivisionNameTh   { get; set; }
        public string DivisionNameEn   { get; set; }
        public int    DivisionId       { get; set; }
        public string Company          { get; set; }

        /// <summary>Label สำหรับ dropdown: "ชื่อ — แผนก [รหัส]"</summary>
        public string DropdownLabel
        {
            get
            {
                string dept = !string.IsNullOrEmpty(DepartmentNameTh) ? DepartmentNameTh : DepartmentNameEn;
                return string.IsNullOrEmpty(dept)
                    ? string.Format("{0} [{1}]", DisplayName, EmployeeCode)
                    : string.Format("{0} — {1} [{2}]", DisplayName, dept, EmployeeCode);
            }
        }
    }
}
