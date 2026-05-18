using System;

namespace CMS.Domain.Models
{
    /// <summary>ข้อมูลคณะอนุกรรมการ จาก core.m_SubCommittee</summary>
    public class SubCommittee
    {
        public int      SubCommitteeID   { get; set; }
        public int      OrgUnitID        { get; set; }
        public string   SubCommitteeCode { get; set; }
        public string   SubCommitteeNameTH { get; set; }
        public string   SubCommitteeNameEN { get; set; }
        public bool     IsActive         { get; set; }
        public bool     IsDeleted        { get; set; }
        public DateTime CreatedDate      { get; set; }
    }
}
