using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CMS.Services.Workflow.Plugins.Builtin
{
    // FR-008-23: CALC_PREMIUM plugin — คำนวณ Premium
    public class CalcPremiumPlugin : IWorkflowPlugin
    {
        public string PluginCode { get { return "CALC_PREMIUM"; } }

        public Task<PluginResult> ExecuteAsync(PluginContext context, CancellationToken cancellationToken)
        {
            try
            {
                // params: policyType, sumInsured, riskFactor
                var policyType  = context.GetParam("policyType");
                var sumInsured  = context.GetParam("sumInsured", "0");

                // TODO: implement premium calculation logic
                var output = new Dictionary<string, object>
                {
                    { "calculatedPremium", 0.0m },
                    { "policyType", policyType }
                };

                return Task.FromResult(PluginResult.Ok(output));
            }
            catch (System.Exception ex)
            {
                return Task.FromResult(PluginResult.Fail(ex.Message));
            }
        }
    }
}
