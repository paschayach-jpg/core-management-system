using System;

namespace CMS.Services.Exceptions
{
    /// <summary>
    /// Exception thrown when a user lacks the required permission to perform a workflow operation.
    /// Contains details about the required permission and the user attempting the operation.
    /// </summary>
    public class WorkflowPermissionException : WorkflowException
    {
        /// <summary>
        /// The permission required to perform the operation.
        /// </summary>
        public string RequiredPermission { get; set; }

        /// <summary>
        /// The AD username of the user who attempted the operation.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Initializes a new instance of the WorkflowPermissionException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="requiredPermission">The permission required for the operation.</param>
        /// <param name="userName">The AD username of the user attempting the operation.</param>
        /// <param name="errorCode">The error code identifying the error type.</param>
        public WorkflowPermissionException(string message, string requiredPermission, string userName, string errorCode = "WF_PERMISSION_DENIED")
            : base(message, errorCode)
        {
            RequiredPermission = requiredPermission;
            UserName = userName;
        }
    }
}
