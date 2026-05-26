using System;
using System.Collections.Generic;
using CMS.Domain.Models.Workflow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CMS.Services.Workflow.Definition
{
    public interface IDefinitionParser
    {
        StepGraph Parse(string definitionCode, string definitionJson);
    }

    // Parse DefinitionJSON (Newtonsoft.Json) → StepGraph
    // Format ตาม SRS §7.1
    public class DefinitionParser : IDefinitionParser
    {
        public StepGraph Parse(string definitionCode, string definitionJson)
        {
            if (string.IsNullOrWhiteSpace(definitionJson))
                throw new ArgumentException("DefinitionJSON is empty", "definitionJson");

            JObject root;
            try
            {
                root = JObject.Parse(definitionJson);
            }
            catch (JsonException ex)
            {
                throw new FormatException("DefinitionJSON is not valid JSON: " + ex.Message, ex);
            }

            var graph = new StepGraph
            {
                DefinitionCode = definitionCode,
                DefinitionName = root.Value<string>("name") ?? definitionCode
            };

            var stepsArray = root["steps"] as JArray;
            if (stepsArray == null || stepsArray.Count == 0)
                throw new FormatException("DefinitionJSON must contain a 'steps' array");

            foreach (JObject stepObj in stepsArray)
            {
                var node = ParseStepNode(stepObj);
                graph.Nodes[node.StepCode] = node;

                if (node.StepType == WfeStepTypes.Start)
                    graph.StartStepCode = node.StepCode;
            }

            if (string.IsNullOrEmpty(graph.StartStepCode))
            {
                // fallback: หา step แรกในลิสต์ที่ไม่มี stepType หรือ type=START
                foreach (var n in graph.Nodes.Values)
                {
                    if (n.StepType == WfeStepTypes.Start)
                    {
                        graph.StartStepCode = n.StepCode;
                        break;
                    }
                }
            }

            return graph;
        }

        private StepNode ParseStepNode(JObject obj)
        {
            var node = new StepNode
            {
                StepCode  = obj.Value<string>("stepCode"),
                StepType  = obj.Value<string>("stepType") ?? WfeStepTypes.HumanTask,
                PluginCode = obj.Value<string>("plugin"),
                AssigneeType  = obj.Value<string>("assigneeType") ?? WfeAssigneeTypes.Role,
                AssigneeValue = obj.Value<string>("assigneeRole")
                             ?? obj.Value<string>("assigneeUser")
                             ?? obj.Value<string>("assigneeValue"),
                SlaHours     = obj["slaHours"] != null ? (int?)obj.Value<int>("slaHours") : null,
                ParallelRule = obj.Value<string>("completionRule")
            };

            // Plugin params
            var paramsObj = obj["pluginParams"] as JObject;
            if (paramsObj != null)
                foreach (var kv in paramsObj)
                    node.PluginParams[kv.Key] = kv.Value.ToString();

            // Escalation
            var escObj = obj["escalation"] as JObject;
            if (escObj != null)
            {
                node.Escalation = new EscalationRule
                {
                    AfterHours  = escObj.Value<int>("afterHours"),
                    EscalateTo  = escObj.Value<string>("escalateTo")
                };
            }

            // Transitions
            var transitions = obj["transitions"] as JArray;
            if (transitions != null)
            {
                foreach (JObject t in transitions)
                {
                    node.Transitions.Add(new TransitionRule
                    {
                        Condition = t.Value<string>("condition") ?? "true",
                        NextStep  = t.Value<string>("nextStep")
                    });
                }
            }

            return node;
        }
    }
}
