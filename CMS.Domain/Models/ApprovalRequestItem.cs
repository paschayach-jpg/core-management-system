using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Item in a batch internal share approval request.
    /// Maps to workflow.ApprovalRequestItem in SCS database.
    /// </summary>
    public class ApprovalRequestItem
    {
        public int ItemID { get; set; }
        public int RequestID { get; set; }
        public int? FileID { get; set; }
        public int? FolderID { get; set; }
        public int PrincipalType { get; set; }
        public int PrincipalID { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanDownload { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? AppliedPermissionID { get; set; }

        // Presentation helpers / labels
        public string PrincipalName { get; set; }
        public string TargetName { get; set; }
    }
}
