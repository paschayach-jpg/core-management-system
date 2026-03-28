using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Stores the state of an in-progress file upload for resume support.
    /// Persisted to IsolatedStorage (per-user, per-machine).
    /// </summary>
    public class UploadCheckpoint
    {
        public string FileName         { get; set; }
        public long FileSizeBytes      { get; set; }
        public int FolderID            { get; set; }
        public long BytesUploaded      { get; set; }
        public DateTime CheckpointDate { get; set; }
    }
}
