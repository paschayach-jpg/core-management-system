using System.Collections.Generic;

namespace CMS.Domain.Models.Workflow
{
    public class StepGraph
    {
        public string DefinitionCode { get; set; }
        public string DefinitionName { get; set; }
        public string StartStepCode { get; set; }
        public Dictionary<string, StepNode> Nodes { get; set; }

        public StepGraph()
        {
            Nodes = new Dictionary<string, StepNode>(System.StringComparer.OrdinalIgnoreCase);
        }

        public StepNode GetNode(string stepCode)
        {
            StepNode node;
            Nodes.TryGetValue(stepCode, out node);
            return node;
        }
    }

    public class StepNode
    {
        public string StepCode { get; set; }
        public string StepType { get; set; }   // START | HumanTask | AutoTask | DECISION | PARALLEL | MERGE | END
        public string PluginCode { get; set; } // AutoTask only
        public Dictionary<string, string> PluginParams { get; set; }

        public string AssigneeType { get; set; }  // USER | ROLE | ORGUNIT | INITIATOR | DYNAMIC
        public string AssigneeValue { get; set; }

        public int? SlaHours { get; set; }
        public EscalationRule Escalation { get; set; }
        public string ParallelRule { get; set; }   // AllMustComplete | MajorityComplete

        public List<TransitionRule> Transitions { get; set; }

        public StepNode()
        {
            Transitions = new List<TransitionRule>();
            PluginParams = new Dictionary<string, string>();
        }

        public bool IsTerminal { get { return StepType == WfeStepTypes.End; } }
        public bool IsHuman    { get { return StepType == WfeStepTypes.HumanTask; } }
        public bool IsAuto     { get { return StepType == WfeStepTypes.AutoTask; } }
    }

    public class TransitionRule
    {
        public string Condition { get; set; }  // NCalc expression or "true"
        public string NextStep  { get; set; }
    }

    public class EscalationRule
    {
        public int    AfterHours { get; set; }
        public string EscalateTo { get; set; }
    }

    public static class WfeStepTypes
    {
        public const string Start    = "START";
        public const string HumanTask = "HumanTask";
        public const string AutoTask  = "AutoTask";
        public const string Decision  = "DECISION";
        public const string Parallel  = "PARALLEL";
        public const string Merge     = "MERGE";
        public const string End       = "END";
    }

    public static class WfeAssigneeTypes
    {
        public const string User      = "USER";
        public const string Role      = "ROLE";
        public const string OrgUnit   = "ORGUNIT";
        public const string Initiator = "INITIATOR";
        public const string Dynamic   = "DYNAMIC";
    }
}
