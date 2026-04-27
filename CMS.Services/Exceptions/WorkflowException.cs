using System;

namespace CMS.Services.Exceptions
{
    /// <summary>
    /// Base exception class for all workflow-related errors.
    /// Provides a consistent error code and message structure for workflow operations.
    /// </summary>
    public class WorkflowException : Exception
    {
        /// <summary>
        /// Error code identifying the type of workflow error.
        /// Used for debugging and error handling in UI/API layers.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Detailed error message describing what went wrong.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Inner exception that caused this workflow exception, if any.
        /// </summary>
        public new Exception InnerException { get; set; }

        /// <summary>
        /// Initializes a new instance of the WorkflowException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errorCode">The error code identifying the error type.</param>
        /// <param name="innerException">The inner exception that caused this error.</param>
        public WorkflowException(string message, string errorCode, Exception innerException = null)
            : base(message, innerException)
        {
            ErrorMessage = message;
            ErrorCode = errorCode;
            InnerException = innerException;
        }
    }
}
