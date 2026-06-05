using System.Collections.Generic;

namespace CMS.Services.Workflow.Rules
{
    // FR-008-16,17,18 / BR-008-09
    public interface IConditionEvaluator
    {
        // evaluate condition expression against context variables + action string
        // returns true/false — must be pure function (no I/O)
        bool Evaluate(string condition, Dictionary<string, object> variables, string action);
    }
}
