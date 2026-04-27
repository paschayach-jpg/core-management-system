using System;

namespace CMS.Services.Exceptions
{
    /// <summary>
    /// Exception thrown when SLA calculation fails for a workflow step.
    /// Contains details about the step instance and calculation error for debugging.
    /// </summary>
    public class SLACalculationException : WorkflowException
    {
        /// <summary>
        /// The ID of the workflow step instance where the calculation failed.
        /// </summary>
        public int StepInstanceId { get; set; }

        /// <summary>
        /// Detailed information about the calculation error.
        /// </summary>
        public string CalculationDetails { get; set; }

        /// <summary>
        /// Initializes a new instance of the SLACalculationException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="stepInstanceId">The ID of the workflow step instance.</param>
        /// <param name="calculationDetails">Detailed information about the calculation error.</param>
        /// <param name="errorCode">The error code identifying the error type.</param>
        public SLACalculationException(string message, int stepInstanceId, string calculationDetails, string errorCode = "WF_SLA_CALCULATION_ERROR")
            : base(message, errorCode)
        {
            StepInstanceId = stepInstanceId;
            CalculationDetails = calculationDetails;
        }
    }
}
