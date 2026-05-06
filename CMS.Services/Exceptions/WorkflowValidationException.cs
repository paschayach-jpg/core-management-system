using System;
using System.Collections.Generic;

namespace CMS.Services.Exceptions
{
    /// <summary>
    /// Exception thrown when workflow request validation fails.
    /// Contains a list of validation errors for detailed error reporting.
    /// </summary>
    public class WorkflowValidationException : WorkflowException
    {
        /// <summary>
        /// List of validation errors that caused this exception.
        /// </summary>
        public List<string> ValidationErrors { get; set; }

        /// <summary>
        /// Initializes a new instance of the WorkflowValidationException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="validationErrors">List of validation errors.</param>
        /// <param name="errorCode">The error code identifying the error type.</param>
        public WorkflowValidationException(string message, List<string> validationErrors, string errorCode = "WF_INVALID_REQUEST")
            : base(message, errorCode)
        {
            ValidationErrors = validationErrors ?? new List<string>();
        }
    }
}
