using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// การตั้งค่า Excel Import Template สำหรับนำเข้าบทความ Knowledge Base
    /// Maps to KMS.m_KBImportTemplate
    /// </summary>
    public class KBImportTemplate
    {
        public int      TemplateID   { get; set; }
        public string   TemplateName { get; set; }
        public string   Description  { get; set; }
        public int      SortOrder    { get; set; }
        public bool     IsActive     { get; set; }
        public string   CreatedBy    { get; set; }
        public DateTime CreatedDate  { get; set; }
        public string   UpdatedBy    { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
