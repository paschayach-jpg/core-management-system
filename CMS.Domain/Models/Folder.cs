using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    public class Folder
    {
        public int FolderID { get; set; }
        public Guid FolderGUID { get; set; }
        public string FolderName { get; set; }
        public int? ParentFolderID { get; set; }
        public int OrgUnitID { get; set; }
        public int? TemplateID { get; set; }
        public int ConfidentialityLevel { get; set; }
        public int FolderDepth { get; set; }
        public string PhysicalPath { get; set; }
        public long FolderSizeBytes { get; set; }
        public int FileCount { get; set; }
        public long? QuotaBytes { get; set; }
        public bool IsArchived { get; set; }
        public int? ArchivedBy { get; set; }
        public DateTime? ArchivedDate { get; set; }
        public bool IsDeleted { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public DateTime? RecycleBinExpiry { get; set; }
        public bool InheritPermission { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Aliases
        public int FolderId { get { return FolderID; } set { FolderID = value; } }
        public int OrgUnitId { get { return OrgUnitID; } set { OrgUnitID = value; } }
        public int? ParentFolderId { get { return ParentFolderID; } set { ParentFolderID = value; } }
        public string Name { get { return FolderName; } set { FolderName = value; } }
        public DateTime CreatedAt { get { return CreatedDate; } set { CreatedDate = value; } }
        public DateTime? DeletedAt { get { return DeletedDate; } set { DeletedDate = value; } }

        /// <summary>Transient: OrgUnit display name, populated by service/presentation layer.</summary>
        public string OrgUnitName { get; set; }

        /// <summary>Transient: immediate parent folder name, populated by presentation layer for list display.</summary>
        public string ParentFolderName { get; set; }

        /// <summary>Transient: display name of the creator, populated by repository layer.</summary>
        public string CreatedByName { get; set; }

        /// <summary>ConfidentialityLevel as enum (maps int to ConfidentialityLevel enum).</summary>
        public ConfidentialityLevel ConfidentialityLevelEnum
        {
            get { return (ConfidentialityLevel)ConfidentialityLevel; }
            set { ConfidentialityLevel = (int)value; }
        }
    }
}