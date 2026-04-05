using System;

namespace CMS.Domain.Models
{
    /// <summary>ข้อมูลฝ่าย จาก core.HR_Division</summary>
    public class HrDivision
    {
        public int      DivisionID   { get; set; }
        public string   DivisionCode { get; set; }
        public string   DivisionName { get; set; }
        public bool     IsActive     { get; set; }
        public DateTime CreatedDate  { get; set; }
    }
}
