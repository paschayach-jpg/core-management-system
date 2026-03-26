using System;
using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Request DTO for submitting a permission-change approval workflow.
    /// RequestType: CREATE_HC | DELETE_FOLDER | CHANGE_PERMISSION | UPLOAD_HC
    /// </summary>
    public class SubmitApprovalRequest
    {
        public string RequestType { get; set; }

        public int? FolderID { get; set; }
        public int? FileID { get; set; }

        public string Subject { get; set; }
        public string Remark { get; set; }

        public DateTime? ExpiryDate { get; set; }

        /// <summary>Ordered list of approver steps.</summary>
        public IList<ApprovalStepDefinition> Steps { get; set; }
    }

    /// <summary>
    /// Defines a single approver step within a SubmitApprovalRequest.
    /// </summary>
    public class ApprovalStepDefinition
    {
        public int StepOrder { get; set; }
        public int ApproverID { get; set; }
    }
}
