using System;

namespace CMS.Services.DTOs
{
    public class PendingPermissionChangeDto
    {
        public int RequestID { get; set; }
        public int ItemID { get; set; }
        public int TargetPermissionID { get; set; }
        public int? FileID { get; set; }
        public int? FolderID { get; set; }
        public int PrincipalType { get; set; }
        public int PrincipalID { get; set; }
        public string PrincipalName { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanDownload { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Remark { get; set; }
        public DateTime RequestedDate { get; set; }
        public int RequestedBy { get; set; }
    }
}
