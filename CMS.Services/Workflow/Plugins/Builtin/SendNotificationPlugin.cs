using System.Threading;
using System.Threading.Tasks;

namespace CMS.Services.Workflow.Plugins.Builtin
{
    // FR-008-23: SEND_NOTIFICATION plugin — ส่งแจ้งเตือนผ่าน MOD-007
    public class SendNotificationPlugin : IWorkflowPlugin
    {
        public string PluginCode { get { return "SEND_NOTIFICATION"; } }

        public Task<PluginResult> ExecuteAsync(PluginContext context, CancellationToken cancellationToken)
        {
            try
            {
                // params: recipientType (USER/ROLE), recipient, templateCode, subject
                var recipient     = context.GetParam("recipient");
                var templateCode  = context.GetParam("templateCode", "WF_DEFAULT");
                var subject       = context.GetParam("subject", "Workflow Notification");

                // TODO: inject INotificationService (MOD-007) และเรียก SendAsync จริง
                // ตอนนี้ log เพื่อให้ engine ทำงานต่อได้ก่อน
                return Task.FromResult(PluginResult.Ok());
            }
            catch (System.Exception ex)
            {
                return Task.FromResult(PluginResult.Fail(ex.Message));
            }
        }
    }
}
