using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Master data for Knowledge Base article categories.
    /// Maps to KMS.m_KBCategory in the database.
    /// Supports self-referencing hierarchy via ParentCategoryID.
    /// </summary>
    public class KBCategory
    {
        public int     CategoryID       { get; set; }
        public int?    ParentCategoryID { get; set; }
        public string  CategoryCode     { get; set; }
        public string  CategoryName     { get; set; }
        public string  Icon             { get; set; }
        public string  Description      { get; set; }
        public int     SortOrder        { get; set; }
        public bool    IsActive         { get; set; }
        public string  CreatedBy        { get; set; }
        public DateTime CreatedDate     { get; set; }

        // Display helper — set by repository when joining parent name
        public string ParentCategoryName { get; set; }
    }
}
