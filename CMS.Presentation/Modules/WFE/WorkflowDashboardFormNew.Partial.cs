using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class WorkflowDashboardFormNew
    {
        public WorkflowDashboardFormNew(
            IWorkflowService workflowService,
            ISLACalculator slaCalculator,
            ILoggingService loggingService,
            UserContext userContext,
            IWorkflowTemplateService templateService,
            IWorkflowDelegationService delegationService)
            : this(workflowService, slaCalculator, loggingService, userContext, templateService)
        {
        }
    }
}
