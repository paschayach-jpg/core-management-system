using System;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Dialogs;
using CMS.Services.DTOs;
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
            IAuditService auditService = null,
            int workflowInstanceId = 0)
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
                    OpenWorkflowStart(currentForm, workflowService, templateService, loggingService, userContext);
                    return true;

                case "history":
                    OpenHistorySearch(currentForm, workflowService, templateService, auditService, loggingService, userContext);
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
                    OpenDelegation(currentForm, workflowService, loggingService, auditService, userContext);
                    return true;

                case "sla":
                    OpenSLAReport(currentForm, workflowService, slaCalculator, loggingService, userContext);
                    return true;

                case "settings":
                    OpenUserProfile(currentForm, workflowService, loggingService, auditService, userContext);
                    return true;

                case "visualflow":
                    OpenVisualFlow(currentForm, workflowService, templateService, loggingService, userContext, workflowInstanceId);
                    return true;

                case "detail":
                    OpenWorkflowDetail(currentForm, workflowService, templateService, loggingService, userContext, workflowInstanceId, auditService);
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
                return;

            if (currentForm.MdiParent is MDI_Container mdi)
            {
                if (workflowService == null) workflowService = mdi.WorkflowService;
                if (slaCalculator   == null) slaCalculator   = mdi.SlaCalculator;
                if (loggingService  == null) loggingService  = mdi.Logger;
                if (userContext     == null) userContext      = mdi.CurrentUser;
            }

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

            if (currentForm.MdiParent is MDI_Container mdi)
            {
                if (workflowService == null) workflowService = mdi.WorkflowService;
                if (loggingService  == null) loggingService  = mdi.Logger;
                if (userContext     == null) userContext      = mdi.CurrentUser;
            }

            var form = new WorkflowMonitorForm(workflowService, loggingService, userContext);
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenWorkflowStart(
            Form currentForm,
            IWorkflowService workflowService,
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            if (currentForm.MdiParent is MDI_Container mdi)
            {
                if (workflowService == null) workflowService = mdi.WorkflowService;
                if (templateService == null) templateService = mdi.WorkflowTemplateService;
                if (loggingService == null) loggingService = mdi.Logger;
                if (userContext == null) userContext = mdi.CurrentUser;
            }

            if (workflowService == null || templateService == null || userContext == null)
            {
                MessageBox.Show(
                    "Workflow start service is not ready. Please open this screen from the main CMS window.",
                    "Workflow",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new WorkflowStartDialog(templateService, loggingService, userContext))
            {
                if (dialog.ShowDialog(currentForm) != DialogResult.OK)
                    return;

                var result = workflowService.CreateWorkflowInstance(new CreateWorkflowInstanceRequest
                {
                    WorkflowDefinitionId = dialog.SelectedTemplateId,
                    Description = dialog.InstanceDescription,
                    Priority = dialog.Priority,
                    InputParameters = dialog.InputParametersJson
                }, userContext);

                if (result == null || result.WorkflowInstanceId <= 0)
                {
                    MessageBox.Show(
                        "ไม่สามารถเริ่ม Workflow ได้ กรุณาตรวจสอบ Template แล้วลองใหม่",
                        "Workflow",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(
                    string.Format("เริ่ม Workflow สำเร็จ\n\nInstance ID: {0}\nสถานะ: {1}", result.WorkflowInstanceId, result.Status),
                    "Workflow",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            OpenWorkflowDashboard(currentForm, workflowService, null, loggingService, userContext);
        }

        private static void OpenHistorySearch(
            Form currentForm,
            IWorkflowService workflowService,
            IWorkflowTemplateService templateService,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            if (currentForm is HistorySearchForm)
                return;

            if (currentForm.MdiParent is MDI_Container mdi)
            {
                if (workflowService == null) workflowService = mdi.WorkflowService;
                if (templateService == null) templateService = mdi.WorkflowTemplateService;
                if (auditService == null) auditService = mdi.AuditService;
                if (loggingService == null) loggingService = mdi.Logger;
                if (userContext == null) userContext = mdi.CurrentUser;
            }

            if (workflowService == null || auditService == null || loggingService == null || userContext == null)
            {
                MessageBox.Show(
                    "Workflow history service is not ready. Please open this screen from the main CMS window.",
                    "Workflow",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var form = new HistorySearchForm(workflowService, auditService, loggingService, userContext, templateService);
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

            IAuditService auditService = null;
            if (currentForm.MdiParent is MDI_Container mdi)
            {
                if (templateService == null) templateService = mdi.WorkflowTemplateService;
                if (loggingService == null)  loggingService  = mdi.Logger;
                if (userContext == null)      userContext      = mdi.CurrentUser;
                auditService = mdi.AuditService;
            }

            var form = new WorkflowTemplateListForm(templateService, auditService, loggingService, userContext);
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

            if (currentForm.MdiParent is MDI_Container mdi)
            {
                if (templateService == null) templateService = mdi.WorkflowTemplateService;
                if (auditService == null)    auditService    = mdi.AuditService;
                if (loggingService == null)  loggingService  = mdi.Logger;
                if (userContext == null)      userContext      = mdi.CurrentUser;
            }

            var form = new TemplateApprovalForm(templateService, auditService, loggingService, userContext);
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenDelegation(
            Form currentForm,
            IWorkflowService workflowService,
            ILoggingService loggingService,
            IAuditService auditService,
            UserContext userContext)
        {
            if (currentForm is WorkflowDelegationForm)
                return;

            CMS.Services.Interfaces.IWorkflowDelegationService delegationService = null;
            CMS.Services.Interfaces.IEmployeeApiService employeeApiService = null;

            CMS.Services.Interfaces.ILdapUserSearchService ldapSearchService = null;

            if (currentForm.MdiParent is MDI_Container mdi)
            {
                if (workflowService  == null) workflowService  = mdi.WorkflowService;
                if (loggingService   == null) loggingService   = mdi.Logger;
                if (auditService     == null) auditService     = mdi.AuditService;
                if (userContext      == null) userContext       = mdi.CurrentUser;
                delegationService  = mdi.WorkflowDelegationService;
                employeeApiService = mdi.EmployeeApiService;
                ldapSearchService  = mdi.LdapUserSearchService;
            }

            if (delegationService == null || workflowService == null || employeeApiService == null)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Delegation service ไม่พร้อมใช้งาน กรุณาเปิดผ่าน MDI Container",
                    "ข้อมูล", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                return;
            }

            var form = new WorkflowDelegationForm(
                delegationService,
                workflowService,
                employeeApiService,
                loggingService,
                auditService,
                userContext,
                ldapSearchService);
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

            if (currentForm.MdiParent is MDI_Container mdi)
            {
                if (workflowService == null) workflowService = mdi.WorkflowService;
                if (slaCalculator   == null) slaCalculator   = mdi.SlaCalculator;
                if (loggingService  == null) loggingService  = mdi.Logger;
                if (userContext     == null) userContext      = mdi.CurrentUser;
            }

            var form = new SLAReportForm(workflowService, slaCalculator, loggingService, userContext);
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenUserProfile(
            Form currentForm,
            IWorkflowService workflowService,
            ILoggingService loggingService,
            IAuditService auditService,
            UserContext userContext)
        {
            if (currentForm is UserProfileForm)
                return;

            IWorkflowDelegationService delegationService = null;
            IEmployeeApiService employeeApiService = null;
            ILdapUserSearchService ldapSearchService = null;

            if (currentForm.MdiParent is MDI_Container mdi)
            {
                if (workflowService  == null) workflowService  = mdi.WorkflowService;
                if (loggingService   == null) loggingService   = mdi.Logger;
                if (auditService     == null) auditService     = mdi.AuditService;
                if (userContext      == null) userContext       = mdi.CurrentUser;
                delegationService  = mdi.WorkflowDelegationService;
                employeeApiService = mdi.EmployeeApiService;
                ldapSearchService  = mdi.LdapUserSearchService;
            }

            var form = new UserProfileForm(
                userContext,
                workflowService,
                delegationService,
                employeeApiService,
                ldapSearchService,
                loggingService,
                auditService);
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenVisualFlow(
            Form currentForm,
            IWorkflowService workflowService,
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext,
            int workflowInstanceId)
        {
            if (currentForm is WorkflowVisualFlowForm)
                return;

            if (currentForm.MdiParent is MDI_Container mdi)
            {
                if (workflowService == null) workflowService = mdi.WorkflowService;
                if (templateService == null) templateService = mdi.WorkflowTemplateService;
                if (loggingService  == null) loggingService  = mdi.Logger;
                if (userContext     == null) userContext      = mdi.CurrentUser;
            }

            var form = new WorkflowVisualFlowForm(workflowService, templateService, loggingService, userContext, workflowInstanceId);
            OpenAsMdiChild(currentForm, form);
        }

        private static void OpenWorkflowDetail(
            Form currentForm,
            IWorkflowService workflowService,
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext,
            int workflowInstanceId,
            IAuditService auditService = null)
        {
            if (currentForm is WorkflowDetailFormNew detail && detail.WorkflowInstanceId == workflowInstanceId)
                return;

            if (currentForm.MdiParent is MDI_Container mdi)
            {
                if (workflowService == null) workflowService = mdi.WorkflowService;
                if (templateService == null) templateService = mdi.WorkflowTemplateService;
                if (loggingService  == null) loggingService  = mdi.Logger;
                if (userContext     == null) userContext      = mdi.CurrentUser;
                if (auditService    == null) auditService     = mdi.AuditService;
            }

            var form = new WorkflowDetailFormNew(
                workflowInstanceId,
                workflowService,
                templateService,
                loggingService,
                userContext,
                auditService);

            if (currentForm is WorkflowDashboardFormNew)
                form.SourceNavTag = "mytasks";
            else if (currentForm is WorkflowMonitorForm)
                form.SourceNavTag = "monitor";
            else if (currentForm is HistorySearchForm)
                form.SourceNavTag = "history";

            OpenAsMdiChild(currentForm, form);
        }

        // ── Helper ──────────────────────────────────────────────────────────────

        private static void OpenAsMdiChild(Form currentForm, Form newForm)
        {
            if (currentForm.MdiParent != null)
            {
                newForm.MdiParent = currentForm.MdiParent;
                newForm.WindowState = FormWindowState.Normal;
                newForm.Show();
                newForm.Activate();
                currentForm.Close();
            }
            else
            {
                newForm.ShowDialog(currentForm);
            }
        }
    }
}
