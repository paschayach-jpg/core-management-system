using System;
using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Reusable folder structure template.
    /// Maps to storage.FolderTemplate in SCS database.
    /// TargetType: SubCommittee | Division | Department | Any
    /// </summary>
    public class FolderTemplate
    {
        public int TemplateID { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string TargetType { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        /// <summary>
        /// หน่วยงานที่ Template นี้ใช้สำหรับ (null = Generic ใช้ได้ทุกหน่วยงาน)
        /// </summary>
        public int? OrgUnitID { get; set; }
        /// <summary>Denormalized display name — ไม่ได้เก็บใน DB, โหลดจาก JOIN</summary>
        public string OrgUnitName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public IList<FolderTemplateItem> Items { get; set; }
    }
}
