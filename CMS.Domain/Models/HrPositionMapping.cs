using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// ผูก Position กับ Department/Division
    /// จาก core.HR_PositionMapping
    /// </summary>
    public class HrPositionMapping
    {
        public int  MappingID    { get; set; }
        public int  PositionID   { get; set; }
        public int? DepartmentID { get; set; }
        public int  DivisionID   { get; set; }
        public int  HeadCount    { get; set; }
        public bool IsActive     { get; set; }
        public DateTime CreatedDate { get; set; }

        // ── Transient (populated by join) ────────────────────────────────────
        public string PositionName   { get; set; }
        public string PositionCode   { get; set; }
        public string Level          { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string DivisionCode   { get; set; }
        public string DivisionName   { get; set; }
    }
}
