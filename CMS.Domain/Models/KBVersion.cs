using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Version History ของบทความ KB เก็บ Snapshot เนื้อหาทุกครั้งที่แก้ไข
    /// Maps to KMS.t_KBVersion
    /// </summary>
    public class KBVersion
    {
        public int      VersionID         { get; set; }
        public int      ArticleID         { get; set; }
        public int      VersionNumber     { get; set; }
        public string   ContentSnapshot   { get; set; }
        public string   ChangeDescription { get; set; }
        public string   CreatedBy         { get; set; }
        public DateTime CreatedDate       { get; set; }
    }
}
