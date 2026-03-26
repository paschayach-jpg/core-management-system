namespace CMS.Domain.Models
{
    /// <summary>
    /// A single folder entry within a FolderTemplate hierarchy.
    /// Maps to storage.FolderTemplateItem in SCS database.
    /// ConfidentialityLevel: 1=Top Secret, 2=Secret, 3=Confidential, 4=Public
    /// </summary>
    public class FolderTemplateItem
    {
        public int ItemID { get; set; }
        public int TemplateID { get; set; }
        public int? ParentItemID { get; set; }
        public string FolderName { get; set; }
        public int ConfidentialityLevel { get; set; }
        public int SortOrder { get; set; }
        public bool InheritPermission { get; set; }
    }
}
