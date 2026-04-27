using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Condition evaluation service
    /// Evaluates workflow condition expressions
    /// </summary>
    public interface IConditionEvaluatorService
    {
        /// <summary>
        /// Evaluate a condition expression with given context data
        /// </summary>
        bool EvaluateCondition(string expression, string contextData);

        /// <summary>
        /// Validate a condition expression for syntax and correctness
        /// </summary>
        ValidationResult ValidateConditionExpression(string expression);

        /// <summary>
        /// Parse and evaluate a condition expression
        /// </summary>
        ConditionEvaluationResult EvaluateConditionExpression(string expression, string contextData);
    }
}
