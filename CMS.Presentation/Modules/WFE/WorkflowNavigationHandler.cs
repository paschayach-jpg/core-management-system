using System;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;
using CMS.Presentation;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// Centralized navigation handler for Workflow module.
    /// Handles opening forms from sidebar navigation clicks.
    /// </summary>
    public static class WorkflowNavigationHandler
    {
        /// <summary>
        /// Opens the appropriate form based on navigation tag.
        /// Returns true if navigation was handled, false otherwise.
        /// </summary>
        public static bool HandleNavigation(
            string tag,
            Form currentForm,
            IWorkflowService workflowService = null,
            IWorkflowTemplateService templateService = null,
            ISLACalculator slaCalculator = null,
            ILoggingService loggingService = null,
            UserContext userContext = null,
            IAuditService auditService = null)
        {
            switch (tag)
            {
                case "mytasks":
                    OpenWorkflowDashboard(currentForm, workflowService, slaCalculator, loggingService, userContext);
                    return true;

                case "monitor":
                    OpenWorkflowMonitor(currentForm, workflowService, loggingService, userContext);
                    return true;

                case "create":
                    OpenWorkflowDesigner(currentForm, templateService, loggingService, userContext);
                    return true;

                case "history":
                    MessageBox.Show("ประวัติ Workflow: อยู่ระหว่างพัฒนา", "ข้อมูล",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;

                case "template":
                    OpenTemplateList(currentForm, templateService, loggingService, userContext);
                    return true;

                case "designer":
                    OpenWorkflowDesigner(currentForm, templateService, loggingService, userContext);
                    return true;

                case "approval":
                    OpenTemplateApproval(currentForm, templateService, auditService, loggingService, userContext);
                    return true;

                case "delegation":
                    OpenDelegation(currentForm);
                    return true;

                case "sla":
                    OpenSLAReport(currentForm, workflowService, slaCalculator, loggingService, userContext);
                    return true;

                case "settings":
                    MessageBox.Show("ตั้งค่า: อยู่ระหว่างพัฒนา", "ข้อมูล",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;

                case "visualflow":
                    OpenVisualFlow(currentForm, workflowService, loggingService, userContext);
                    return true;

                default:
                    return false;
            }
        }

        // ── Form openers ────────────────────────────────────────────────────────

        private static void OpenWorkflowDashboard(
            Form currentForm,
            IWorkflowService workflowService,
            ISLACalculator slaCalculator,
            ILoggingService loggingService,
            UserContext userContext)
        {
            if (currentForm is WorkflowDashboardFormNew)
                return; // Already on this form

            // Allow opening without services for preview/testing
            var form = new WorkflowDashboardFormNew(workflowService, slaCalculator, loggingService, userContext);
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenWorkflowMonitor(
            Form currentForm,
            IWorkflowService workflowService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            if (currentForm is WorkflowMonitorForm)
                return;

            // Allow opening without services for preview/testing
            var form = new WorkflowMonitorForm(workflowService, loggingService, userContext);
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenTemplateList(
            Form currentForm,
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            if (currentForm is WorkflowTemplateListForm)
                return;

            // Fallback: get service from MDI_Container if not passed by calling form
            if (templateService == null && currentForm.MdiParent is MDI_Container mdi)
                templateService = mdi.WorkflowTemplateService;

            var form = new WorkflowTemplateListForm(templateService, null, loggingService, userContext);
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenWorkflowDesigner(
            Form currentForm,
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            if (currentForm is WorkflowDesignerForm)
                return;

            // Allow opening without services for preview/testing
            var form = new WorkflowDesignerForm(null, templateService, loggingService, userContext);
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenTemplateApproval(
            Form currentForm,
            IWorkflowTemplateService templateService,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            if (currentForm is TemplateApprovalForm)
                return;

            // Allow opening without services for preview/testing
            var form = new TemplateApprovalForm(templateService, auditService, loggingService, userContext);
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenDelegation(Form currentForm)
        {
            if (currentForm is WorkflowDelegationForm)
                return;

            var form = new WorkflowDelegationForm();
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenSLAReport(
            Form currentForm,
            IWorkflowService workflowService,
            ISLACalculator slaCalculator,
            ILoggingService loggingService,
            UserContext userContext)
        {
            if (currentForm is SLAReportForm)
                return;

            // Allow opening without services for preview/testing
            var form = new SLAReportForm(workflowService, slaCalculator, loggingService, userContext);
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenVisualFlow(
            Form currentForm,
            IWorkflowService workflowService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            if (currentForm is WorkflowVisualFlowForm)
                return;

            // Allow opening without services for preview/testing
            var form = new WorkflowVisualFlowForm(workflowService, loggingService, userContext);
            OpenAsMdiChild(currentForm, form);
        }

        // ── Helper ──────────────────────────────────────────────────────────────

        private static void OpenAsMdiChild(Form currentForm, Form newForm)
        {
            // If current form is MDI child, use same MDI parent
            if (currentForm.MdiParent != null)
            {
                newForm.MdiParent = currentForm.MdiParent;
                newForm.Show();
                currentForm.Close();
            }
            else
            {
                // Open as dialog if no MDI parent
                newForm.ShowDialog(currentForm);
            }
        }
    }
}
