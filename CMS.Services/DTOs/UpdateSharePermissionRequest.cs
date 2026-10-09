using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// DTO for updating permissions and expiry of an existing internal FilePermission.
    /// </summary>
    public class UpdateSharePermissionRequest
    {
        public int FilePermissionID { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanDownload { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Remark { get; set; }
    }
}
