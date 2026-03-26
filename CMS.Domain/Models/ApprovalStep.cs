using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// A single step in a multi-step approval workflow.
    /// Maps to workflow.ApprovalStep in SCS database.
    /// StepStatus: 1=Pending, 2=Approved, 3=Rejected, 4=Delegated
    /// </summary>
    public class ApprovalStep
    {
        public int StepID { get; set; }
        public int RequestID { get; set; }
        public int StepOrder { get; set; }
        public int ApproverID { get; set; }

        /// <summary>1=Pending, 2=Approved, 3=Rejected, 4=Delegated</summary>
        public int StepStatus { get; set; }

        public DateTime? ActionDate { get; set; }
        public string ActionRemark { get; set; }
        public int? DelegatedTo { get; set; }
        public DateTime? NotifiedDate { get; set; }
        public DateTime? EscalatedDate { get; set; }
    }
}
