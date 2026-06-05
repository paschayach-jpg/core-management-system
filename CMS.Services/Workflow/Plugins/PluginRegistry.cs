using System;
using System.Collections.Generic;

namespace CMS.Services.Workflow.Plugins
{
    // FR-008-25: Registry ของ Plugin ทั้งหมดในระบบ
    public class PluginRegistry : IPluginRegistry
    {
        private readonly Dictionary<string, IWorkflowPlugin> _registry =
            new Dictionary<string, IWorkflowPlugin>(StringComparer.OrdinalIgnoreCase);

        public void Register(IWorkflowPlugin plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException("plugin");

            _registry[plugin.PluginCode] = plugin;
        }

        public IWorkflowPlugin Resolve(string pluginCode)
        {
            IWorkflowPlugin plugin;
            if (!_registry.TryGetValue(pluginCode, out plugin))
                throw new KeyNotFoundException(
                    string.Format("Plugin '{0}' not registered (WF-E008). Admin must register it in s_WorkflowPlugin.", pluginCode));

            return plugin;
        }

        public bool IsRegistered(string pluginCode)
        {
            return _registry.ContainsKey(pluginCode);
        }
    }
}
