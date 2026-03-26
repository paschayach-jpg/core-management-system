using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Workflow approval request.
    /// Maps to workflow.ApprovalRequest in SCS database.
    /// RequestType: CREATE_HC | DELETE_FOLDER | CHANGE_PERMISSION | UPLOAD_HC
    /// RequestStatus: 1=Pending, 2=Approved, 3=Rejected, 4=Expired
    /// </summary>
    public class ApprovalRequest
    {
        public int RequestID { get; set; }
        public string RequestType { get; set; }
        public int? FolderID { get; set; }
        public int? FileID { get; set; }
        public int? PermissionID { get; set; }
        public int RequestedBy { get; set; }
        public DateTime RequestedDate { get; set; }

        /// <summary>1=Pending, 2=Approved, 3=Rejected, 4=Expired</summary>
        public int RequestStatus { get; set; }

        public string Subject { get; set; }
        public string Remark { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? ResolvedBy { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string ResolveRemark { get; set; }
    }
}
