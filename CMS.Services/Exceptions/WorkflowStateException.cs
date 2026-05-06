using System;

namespace CMS.Services.Exceptions
{
    /// <summary>
    /// Exception thrown when a workflow operation is attempted in an invalid state.
    /// Contains details about the current state and expected state for debugging.
    /// </summary>
    public class WorkflowStateException : WorkflowException
    {
        /// <summary>
        /// The current state of the workflow or step.
        /// </summary>
        public string CurrentState { get; set; }

        /// <summary>
        /// The expected state for the operation to succeed.
        /// </summary>
        public string ExpectedState { get; set; }

        /// <summary>
        /// Initializes a new instance of the WorkflowStateException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="currentState">The current state of the workflow or step.</param>
        /// <param name="expectedState">The expected state for the operation.</param>
        /// <param name="errorCode">The error code identifying the error type.</param>
        public WorkflowStateException(string message, string currentState, string expectedState, string errorCode = "WF_INVALID_STATE")
            : base(message, errorCode)
        {
            CurrentState = currentState;
            ExpectedState = expectedState;
        }
    }
}
