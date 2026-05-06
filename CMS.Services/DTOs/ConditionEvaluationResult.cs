using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Result DTO for condition evaluation
    /// </summary>
    public class ConditionEvaluationResult
    {
        /// <summary>
        /// Whether condition evaluated to true
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Condition expression that was evaluated
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// Context data used for evaluation
        /// </summary>
        public string ContextData { get; set; }

        /// <summary>
        /// Error message if evaluation failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Whether evaluation was successful
        /// </summary>
        public bool IsSuccessful { get; set; }
    }
}
