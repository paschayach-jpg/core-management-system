namespace CMS.Domain.Enums
{
    /// <summary>
    /// Status values for ApprovalRequest records.
    /// Maps to workflow.ApprovalRequest.RequestStatus column.
    /// </summary>
    public enum ApprovalRequestStatus
    {
        Pending  = 1,
        Approved = 2,
        Rejected = 3,
        Expired  = 4
    }

    /// <summary>
    /// Status values for ApprovalStep records.
    /// Maps to workflow.ApprovalStep.StepStatus column.
    /// </summary>
    public enum ApprovalStepStatus
    {
        Pending   = 1,
        Approved  = 2,
        Rejected  = 3,
        Delegated = 4
    }
}
