namespace CMS.Domain.Enums
{
    /// <summary>
    /// Defines the type/role of a step within a workflow template.
    /// Maps to workflow.WorkflowStep.StepType column.
    /// </summary>
    public enum WorkflowStepType
    {
        Start        = 1,
        Approval     = 2,
        Review       = 3,
        Notification = 4,
        Parallel     = 5,
        Decision     = 6,
        Merge        = 7,
        End          = 8
    }

    /// <summary>
    /// Overall status of a workflow instance.
    /// Maps to workflow.WorkflowInstance.Status column.
    /// </summary>
    public enum WorkflowInstanceStatus
    {
        Created    = 1,
        InProgress = 2,
        Completed  = 3,
        Rejected   = 4,
        Suspended  = 5
    }

    /// <summary>
    /// Status of an individual step instance within a running workflow.
    /// Maps to workflow.WorkflowStepInstance.Status column.
    /// </summary>
    public enum WorkflowStepInstanceStatus
    {
        Pending    = 1,
        InProgress = 2,
        Completed  = 3,
        Rejected   = 4,
        Delegated  = 5
    }

    /// <summary>
    /// SLA compliance status for a workflow step instance.
    /// Maps to workflow.WorkflowStepInstance.SLAStatus column.
    /// </summary>
    public enum SLAStatus
    {
        OnTime  = 1,
        AtRisk  = 2,
        Overdue = 3
    }

    /// <summary>
    /// Action taken when a step instance breaches its SLA deadline.
    /// Maps to workflow.WFSLARule.EscalationAction column.
    /// </summary>
    public enum EscalationAction
    {
        NotifyManager    = 1,
        IncreasePriority = 2,
        Reassign         = 3,
        AutoReject       = 4
    }

    /// <summary>
    /// Approval lifecycle status of a workflow template definition.
    /// Stored in WorkflowDefinition.ApprovalStatus column (tinyint).
    /// Draft → PendingApproval → Active (or back to Draft on reject).
    /// </summary>
    public enum WorkflowTemplateStatus : byte
    {
        Draft           = 0,
        PendingApproval = 1,
        Active          = 2,
        Inactive        = 3,
        Deprecated      = 4
    }
}
