namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request DTO for creating a new Folder.
    /// ConfidentialityLevel: 1=Top Secret, 2=Secret, 3=Confidential, 4=Public
    /// </summary>
    public class CreateFolderRequest
    {
        /// <summary>Parent folder ID. Null means root folder directly under OrgUnit.</summary>
        public int? ParentFolderId { get; set; }

        public int OrgUnitId { get; set; }

        public string Name { get; set; }

        /// <summary>Null means inherit from parent (or default to 4=Public for root folders).</summary>
        public int? ConfidentialityLevel { get; set; }
    }
}
