using System.Collections.Generic;
using CMS.Domain.Models.Workflow;

namespace CMS.Services.Workflow.Definition
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors   { get; set; }
        public List<string> Warnings { get; set; }

        public ValidationResult()
        {
            Errors   = new List<string>();
            Warnings = new List<string>();
        }
    }

    public interface IDefinitionValidator
    {
        ValidationResult Validate(StepGraph graph, IEnumerable<string> registeredPluginCodes);
    }

    // BR-008-14: Validator rules ก่อน Activate
    public class DefinitionValidator : IDefinitionValidator
    {
        public ValidationResult Validate(StepGraph graph, IEnumerable<string> registeredPluginCodes)
        {
            var result = new ValidationResult { IsValid = true };
            var pluginSet = new HashSet<string>(registeredPluginCodes ?? new string[0],
                                                System.StringComparer.OrdinalIgnoreCase);

            // BR-008-14: ต้องมี Start Node 1 อัน
            int startCount = 0;
            int endCount   = 0;

            foreach (var node in graph.Nodes.Values)
            {
                if (node.StepType == WfeStepTypes.Start) startCount++;
                if (node.StepType == WfeStepTypes.End)   endCount++;
            }

            if (startCount != 1)
            {
                result.IsValid = false;
                result.Errors.Add(string.Format("WF-E001: ต้องมี Start Node 1 อัน (พบ {0} อัน)", startCount));
            }

            if (endCount < 1)
            {
                result.IsValid = false;
                result.Errors.Add("WF-E002: ต้องมี End Node อย่างน้อย 1 อัน");
            }

            // BR-008-14: Circular transition → Block
            if (HasCycle(graph))
            {
                result.IsValid = false;
                result.Errors.Add("WF-E003: พบ Circular transition — ตรวจสอบ Routing ใน Designer");
            }

            // BR-008-14: Orphan step → Warning (ยังให้ Activate ได้)
            var reachable = FindReachableNodes(graph);
            foreach (var nodeCode in graph.Nodes.Keys)
            {
                if (!reachable.Contains(nodeCode) && graph.Nodes[nodeCode].StepType != WfeStepTypes.Start)
                    result.Warnings.Add(string.Format("Step '{0}' ไม่มี transition เข้าถึง (Orphan step)", nodeCode));
            }

            // FR-008-07 / BR-008-11: Plugin ต้องอยู่ใน s_WorkflowPlugin ก่อน Activate
            foreach (var node in graph.Nodes.Values)
            {
                if (node.StepType == WfeStepTypes.AutoTask &&
                    !string.IsNullOrEmpty(node.PluginCode) &&
                    !pluginSet.Contains(node.PluginCode))
                {
                    result.IsValid = false;
                    result.Errors.Add(string.Format("WF-E008: Plugin '{0}' ไม่ได้ลงทะเบียนในระบบ", node.PluginCode));
                }
            }

            return result;
        }

        // DFS cycle detection — Kahn's / color marking
        private bool HasCycle(StepGraph graph)
        {
            var color = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var code in graph.Nodes.Keys)
                color[code] = 0; // 0=white

            foreach (var code in graph.Nodes.Keys)
                if (color[code] == 0 && DfsHasCycle(graph, code, color))
                    return true;

            return false;
        }

        private bool DfsHasCycle(StepGraph graph, string current, Dictionary<string, int> color)
        {
            color[current] = 1; // gray = in stack
            StepNode node;
            if (!graph.Nodes.TryGetValue(current, out node)) return false;

            foreach (var t in node.Transitions)
            {
                if (string.IsNullOrEmpty(t.NextStep)) continue;

                int c;
                if (!color.TryGetValue(t.NextStep, out c)) continue;

                if (c == 1) return true;
                if (c == 0 && DfsHasCycle(graph, t.NextStep, color)) return true;
            }

            color[current] = 2; // black = done
            return false;
        }

        private HashSet<string> FindReachableNodes(StepGraph graph)
        {
            var visited = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(graph.StartStepCode)) return visited;

            var queue = new Queue<string>();
            queue.Enqueue(graph.StartStepCode);
            visited.Add(graph.StartStepCode);

            while (queue.Count > 0)
            {
                var code = queue.Dequeue();
                StepNode node;
                if (!graph.Nodes.TryGetValue(code, out node)) continue;

                foreach (var t in node.Transitions)
                {
                    if (!string.IsNullOrEmpty(t.NextStep) && !visited.Contains(t.NextStep))
                    {
                        visited.Add(t.NextStep);
                        queue.Enqueue(t.NextStep);
                    }
                }
            }

            return visited;
        }
    }
}
