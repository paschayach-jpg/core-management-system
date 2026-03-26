using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    public class StorageUsageReport
    {
        public int OrgUnitID { get; set; }
        public string OrgUnitName { get; set; }
        public IList<FolderStorageEntry> Folders { get; set; }
        public StorageUsageReport() { Folders = new List<FolderStorageEntry>(); }
    }

    public class FolderStorageEntry
    {
        public int FolderID { get; set; }
        public string FolderName { get; set; }
        public long CurrentUsageBytes { get; set; }
        public long? QuotaLimitBytes { get; set; }

        // Aliases
        public int FolderId { get { return FolderID; } set { FolderID = value; } }

        /// <summary>Alias for QuotaLimitBytes.</summary>
        public long? QuotaBytes { get { return QuotaLimitBytes; } set { QuotaLimitBytes = value; } }
    }
}