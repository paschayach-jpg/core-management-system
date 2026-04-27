using CMS.Domain.Models;
using System.Collections.Generic;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Workflow step resolution service
    /// Determines next step(s) based on conditions and workflow definition
    /// </summary>
    public interface IStepResolverService
    {
        /// <summary>
        /// Resolve next step(s) after current step completion
        /// </summary>
        List<WorkflowStep> ResolveNextSteps(WorkflowStep currentStep, string outputData, WorkflowDefinition template);

        /// <summary>
        /// Evaluate condition expressions
        /// </summary>
        bool EvaluateConditions(string conditionExpression, string outputData);

        /// <summary>
        /// Check if a user is eligible to be delegated a task
        /// </summary>
        bool ResolveDelegationEligibility(WorkflowStep step, string delegateUserName);

        /// <summary>
        /// Get all possible next steps for a given step
        /// </summary>
        List<WorkflowStep> GetPossibleNextSteps(WorkflowStep currentStep, WorkflowDefinition template);
    }
}
