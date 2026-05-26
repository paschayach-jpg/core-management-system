using System.Threading;
using System.Threading.Tasks;

namespace CMS.Services.Workflow.Plugins.Builtin
{
    // FR-008-23: CALL_RISC_SERVER plugin — เรียก RISC Claim Server
    public class CallRiscServerPlugin : IWorkflowPlugin
    {
        public string PluginCode { get { return "CALL_RISC_SERVER"; } }

        public Task<PluginResult> ExecuteAsync(PluginContext context, CancellationToken cancellationToken)
        {
            try
            {
                // params: endpoint, operation, claimId
                var operation = context.GetParam("operation");
                var claimId   = context.GetParam("claimId");

                // TODO: inject IHttpClient / RiscServerClient และเรียก API จริง
                return Task.FromResult(PluginResult.Ok());
            }
            catch (System.Exception ex)
            {
                return Task.FromResult(PluginResult.Fail(ex.Message));
            }
        }
    }
}
