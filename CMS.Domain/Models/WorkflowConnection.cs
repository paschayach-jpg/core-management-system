namespace CMS.Domain.Models
{
    /// <summary>
    /// Represents a directed transition (connection) between two steps in a workflow template.
    /// When a step completes, the engine evaluates all outgoing connections in SequenceNumber
    /// order and follows the first one whose ConditionExpression evaluates to true (or the
    /// unconditional connection when no expression is set).
    /// Maps to workflow.WorkflowConnection in the SCS database.
    /// </summary>
    public class WorkflowConnection
    {
        /// <summary>Primary key.</summary>
        public int WorkflowConnectionId { get; set; }

        /// <summary>Foreign key to the parent WorkflowDefinition template.</summary>
        public int WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Foreign key to the WorkflowStep that is the source of this transition.
        /// </summary>
        public int FromStepId { get; set; }

        /// <summary>
        /// Foreign key to the WorkflowStep that is the target of this transition.
        /// </summary>
        public int ToStepId { get; set; }

        /// <summary>
        /// Optional IF-THEN condition expression that must evaluate to true for this
        /// connection to be followed. Null or empty means the connection is unconditional.
        /// Evaluated by IConditionEvaluatorService at runtime using the step's output data.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Evaluation order among all outgoing connections from the same FromStep.
        /// The engine follows the first matching connection in ascending order.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>Soft-delete flag. True when this connection has been logically removed.</summary>
        public bool IsDeleted { get; set; }
    }
}
