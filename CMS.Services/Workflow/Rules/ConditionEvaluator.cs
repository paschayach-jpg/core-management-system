using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CMS.Services.Workflow.Rules
{
    // FR-008-16,17,18 / BR-008-09 / NFR-008-07
    // Implementation: lightweight expression evaluator รองรับ pattern ที่ใช้ใน DefinitionJSON
    // TODO: เมื่อ NCalc2 NuGet ถูก install แล้ว ให้ replace body ด้วย NCalc-based implementation
    //       Install: Install-Package NCalc2 -Version 2.1.0
    //
    // Pattern ที่รองรับ:
    //   "true"                         → always pass (BR-008-09)
    //   "action == 'APPROVE'"          → string compare กับ action parameter
    //   "ctx.claimAmount > 50000"      → numeric compare กับ ctx.Variables
    //   "ctx.someFlag == true"         → bool compare
    //   "ctx.status != 'PENDING'"      → string not-equal
    //
    // Whitelist operators: ==, !=, >, <, >=, <= (NFR-008-07: ห้าม arbitrary code)
    public class ConditionEvaluator : IConditionEvaluator
    {
        private static readonly Regex _ctxPattern =
            new Regex(@"ctx\.(\w+)\s*(==|!=|>|<|>=|<=)\s*(.+)", RegexOptions.Compiled);

        private static readonly Regex _actionPattern =
            new Regex(@"action\s*(==|!=)\s*'(\w+)'", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool Evaluate(string condition, Dictionary<string, object> variables, string action)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return true;

            condition = condition.Trim();

            // BR-008-09: condition = "true" → pass immediately
            if (string.Equals(condition, "true", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(condition, "false", StringComparison.OrdinalIgnoreCase))
                return false;

            // action == 'APPROVE'  /  action != 'REJECT'
            var actionMatch = _actionPattern.Match(condition);
            if (actionMatch.Success)
            {
                var op  = actionMatch.Groups[1].Value;
                var val = actionMatch.Groups[2].Value;
                return op == "==" ? string.Equals(action, val, StringComparison.OrdinalIgnoreCase)
                                  : !string.Equals(action, val, StringComparison.OrdinalIgnoreCase);
            }

            // ctx.variableName op value
            var ctxMatch = _ctxPattern.Match(condition);
            if (ctxMatch.Success)
            {
                var varName = ctxMatch.Groups[1].Value;
                var op      = ctxMatch.Groups[2].Value;
                var rhs     = ctxMatch.Groups[3].Value.Trim().Trim('\'', '"');

                object lhsObj;
                if (variables == null || !variables.TryGetValue(varName, out lhsObj))
                    throw new InvalidOperationException(
                        string.Format("Variable 'ctx.{0}' not found in context (WF-E013)", varName));

                return CompareValues(lhsObj, op, rhs);
            }

            throw new NotSupportedException(
                string.Format("Condition expression not supported: '{0}' (WF-E013). " +
                              "Install NCalc2 for full expression support.", condition));
        }

        private static bool CompareValues(object lhs, string op, string rhsStr)
        {
            // numeric comparison
            double lhsNum, rhsNum;
            if (lhs != null && double.TryParse(lhs.ToString(), out lhsNum) &&
                double.TryParse(rhsStr, out rhsNum))
            {
                switch (op)
                {
                    case "==": return lhsNum == rhsNum;
                    case "!=": return lhsNum != rhsNum;
                    case ">":  return lhsNum > rhsNum;
                    case "<":  return lhsNum < rhsNum;
                    case ">=": return lhsNum >= rhsNum;
                    case "<=": return lhsNum <= rhsNum;
                }
            }

            // bool comparison
            bool lhsBool, rhsBool;
            if (lhs != null && bool.TryParse(lhs.ToString(), out lhsBool) &&
                bool.TryParse(rhsStr, out rhsBool))
            {
                switch (op)
                {
                    case "==": return lhsBool == rhsBool;
                    case "!=": return lhsBool != rhsBool;
                }
            }

            // string comparison
            var lhsStr = lhs != null ? lhs.ToString() : string.Empty;
            switch (op)
            {
                case "==": return string.Equals(lhsStr, rhsStr, StringComparison.OrdinalIgnoreCase);
                case "!=": return !string.Equals(lhsStr, rhsStr, StringComparison.OrdinalIgnoreCase);
                default:
                    throw new NotSupportedException(
                        string.Format("Operator '{0}' not supported for string comparison (WF-E013)", op));
            }
        }
    }
}
