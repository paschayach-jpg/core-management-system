namespace CMS.Services.Workflow.Plugins
{
    // FR-008-25: Plugin Registry interface
    public interface IPluginRegistry
    {
        void Register(IWorkflowPlugin plugin);
        IWorkflowPlugin Resolve(string pluginCode);
        bool IsRegistered(string pluginCode);
    }
}
