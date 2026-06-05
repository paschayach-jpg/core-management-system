using System.Threading;
using System.Threading.Tasks;

namespace CMS.Services.Workflow.Plugins.Builtin
{
    // FR-008-23: SEND_OIC_REPORT plugin — ส่งรายงาน OIC ตาม Circular 2566
    public class SendOicReportPlugin : IWorkflowPlugin
    {
        public string PluginCode { get { return "SEND_OIC_REPORT"; } }

        public Task<PluginResult> ExecuteAsync(PluginContext context, CancellationToken cancellationToken)
        {
            try
            {
                // params: reportType, submissionDate, policyNumber
                var reportType = context.GetParam("reportType");

                // TODO: inject IOicReportService และส่งรายงานจริง
                return Task.FromResult(PluginResult.Ok());
            }
            catch (System.Exception ex)
            {
                return Task.FromResult(PluginResult.Fail(ex.Message));
            }
        }
    }
}
