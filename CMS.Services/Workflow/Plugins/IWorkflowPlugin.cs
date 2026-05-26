using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CMS.Services.Workflow.Plugins
{
    // FR-008-22: IWorkflowPlugin interface
    public interface IWorkflowPlugin
    {
        string PluginCode { get; }
        Task<PluginResult> ExecuteAsync(PluginContext context, CancellationToken cancellationToken);
    }

    public class PluginContext
    {
        public int    InstanceId    { get; set; }
        public string InstanceCode  { get; set; }
        public string StepCode      { get; set; }
        public string RefType       { get; set; }
        public int    RefId         { get; set; }
        public int    TenantId      { get; set; }
        public Dictionary<string, object> Variables    { get; set; }
        public Dictionary<string, string> PluginParams { get; set; }

        public PluginContext()
        {
            Variables    = new Dictionary<string, object>();
            PluginParams = new Dictionary<string, string>();
        }

        public string GetParam(string key, string defaultValue = null)
        {
            string val;
            return PluginParams.TryGetValue(key, out val) ? val : defaultValue;
        }
    }

    public class PluginResult
    {
        public bool   Success      { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> OutputVariables { get; set; }

        public static PluginResult Ok(Dictionary<string, object> output = null)
        {
            return new PluginResult
            {
                Success = true,
                OutputVariables = output ?? new Dictionary<string, object>()
            };
        }

        public static PluginResult Fail(string error)
        {
            return new PluginResult
            {
                Success = false,
                ErrorMessage = error,
                OutputVariables = new Dictionary<string, object>()
            };
        }
    }
}
