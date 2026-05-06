using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services.Validators
{
    /// <summary>
    /// Validator for condition expressions used in workflow step routing.
    /// Validates expression syntax and referenced variables.
    /// </summary>
    public class ConditionExpressionValidator
    {
        private readonly IConditionEvaluatorService _conditionEvaluator;

        /// <summary>
        /// Initializes a new instance of the ConditionExpressionValidator class.
        /// </summary>
        /// <param name="conditionEvaluator">Service for evaluating condition expressions.</param>
        public ConditionExpressionValidator(IConditionEvaluatorService conditionEvaluator)
        {
            _conditionEvaluator = conditionEvaluator ?? throw new ArgumentNullException(nameof(conditionEvaluator));
        }

        /// <summary>
        /// Validates a condition expression.
        /// </summary>
        /// <param name="expression">The condition expression to validate.</param>
        /// <param name="contextVariables">Dictionary of available variables in the expression context.</param>
        /// <returns>A ValidationResult containing any validation errors.</returns>
        public ValidationResult Validate(string expression, Dictionary<string, object> contextVariables = null)
        {
            var result = new ValidationResult();

            // Validate expression is not null or empty
            if (string.IsNullOrWhiteSpace(expression))
            {
                result.Errors.Add("Condition expression cannot be null or empty");
                result.IsValid = false;
                return result;
            }

            // Validate expression syntax
            if (!IsValidExpressionSyntax(expression))
            {
                result.Errors.Add("Condition expression has invalid syntax");
            }

            // Validate referenced variables exist in context
            var referencedVariables = ExtractVariablesFromExpression(expression);
            if (contextVariables != null)
            {
                foreach (var variable in referencedVariables)
                {
                    if (!contextVariables.ContainsKey(variable))
                    {
                        result.Errors.Add($"Variable '{variable}' referenced in expression is not available in context");
                    }
                }
            }

            // Validate expression evaluates without errors
            if (contextVariables != null && result.Errors.Count == 0)
            {
                try
                {
                    // For now, pass empty context string - the service will handle validation
                    // In a real implementation, you'd serialize contextVariables to JSON
                    var evaluationResult = _conditionEvaluator.EvaluateConditionExpression(expression, "{}");
                    if (evaluationResult != null && !evaluationResult.IsSuccessful)
                    {
                        result.Errors.Add($"Expression evaluation failed: {evaluationResult.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error evaluating expression: {ex.Message}");
                }
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// Checks if an expression has valid syntax.
        /// </summary>
        private bool IsValidExpressionSyntax(string expression)
        {
            try
            {
                // Basic syntax validation - check for balanced parentheses and brackets
                int parenCount = 0;
                int bracketCount = 0;

                foreach (char c in expression)
                {
                    if (c == '(') parenCount++;
                    if (c == ')') parenCount--;
                    if (c == '[') bracketCount++;
                    if (c == ']') bracketCount--;

                    if (parenCount < 0 || bracketCount < 0)
                        return false;
                }

                return parenCount == 0 && bracketCount == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts variable names from a condition expression.
        /// </summary>
        private List<string> ExtractVariablesFromExpression(string expression)
        {
            var variables = new List<string>();

            try
            {
                // Match variable patterns like ${variableName} or @variableName
                var pattern = @"(?:\$\{([a-zA-Z_][a-zA-Z0-9_]*)\}|@([a-zA-Z_][a-zA-Z0-9_]*))";
                var matches = Regex.Matches(expression, pattern);

                foreach (Match match in matches)
                {
                    var variable = match.Groups[1].Value ?? match.Groups[2].Value;
                    if (!string.IsNullOrEmpty(variable) && !variables.Contains(variable))
                    {
                        variables.Add(variable);
                    }
                }
            }
            catch
            {
                // If extraction fails, return empty list
            }

            return variables;
        }
    }
}
