using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// File metadata record stored in the DMS.
    /// Maps to storage.FileDocument in SCS database.
    /// VirusScanStatus: 0=Pending, 1=Clean, 2=Infected, 3=Error
    /// FileStatus: 1=Active, 2=Archived, 3=Deleted
    /// </summary>
    public class FileDocument
    {
        public int FileID { get; set; }
        public Guid FileGUID { get; set; }
        public int FolderID { get; set; }
        public string OriginalFileName { get; set; }
        public string StoredFileName { get; set; }
        public string FileExtension { get; set; }
        public string MimeType { get; set; }
        public long FileSizeBytes { get; set; }
        public string PhysicalPath { get; set; }
        public string Checksum { get; set; }
        public int VirusScanStatus { get; set; }
        public DateTime? VirusScanDate { get; set; }
        public int FileStatus { get; set; }
        public int UploadedBy { get; set; }
        public DateTime UploadedDate { get; set; }
        public int? ArchivedBy { get; set; }
        public DateTime? ArchivedDate { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int Version { get; set; }
        public bool IsHidden { get; set; }
        public string HiddenBy { get; set; }
        public DateTime? HiddenDate { get; set; }

        // -----------------------------------------------------------------------
        // Aliases used by presentation / report layers
        // -----------------------------------------------------------------------

        /// <summary>Alias for FileID.</summary>
        public int FileDocumentId
        {
            get { return FileID; }
            set { FileID = value; }
        }

        /// <summary>Alias for FolderID.</summary>
        public int FolderId
        {
            get { return FolderID; }
            set { FolderID = value; }
        }

        /// <summary>Alias for UploadedDate.</summary>
        public DateTime UploadedAt
        {
            get { return UploadedDate; }
            set { UploadedDate = value; }
        }

        /// <summary>True when FileStatus == 3 (Deleted).</summary>
        public bool IsDeleted
        {
            get { return FileStatus == 3; }
        }

        /// <summary>True when file is flagged for retention review (Archived or Deleted).</summary>
        public bool RetentionFlagged
        {
            get { return FileStatus == 2 || FileStatus == 3; }
        }

        /// <summary>Transient: populated when loaded via sp_GetAllFileDocumentsGroupedByFolder.</summary>
        public string FolderName { get; set; }

        /// <summary>Transient: display name of the uploader, populated by service/repository layer.</summary>
        public string UploadedByName { get; set; }

        /// <summary>Transient: confidentiality level inherited from parent folder, populated by service layer.</summary>
        public int ConfidentialityLevel { get; set; }

        /// <summary>Transient: OrgUnit display name, populated by service/presentation layer.</summary>
        public string OrgUnitName { get; set; }
    }
}