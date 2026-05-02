using System;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// Workflow Template Creation Wizard Controller
    /// Manages 4-step wizard: Basic Info → Flow Design → Approval Config → Publish
    /// </summary>
    public class WorkflowCreateTemplateWizard
    {
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;
        private readonly Form _parentForm;
        private WorkflowWizardState _wizardState;

        private WorkflowCreateTemplateForm _step1Form;
        private WorkflowCreateTemplateStep2Form _step2Form;
        private WorkflowCreateTemplateStep3Form _step3Form;
        private WorkflowCreateTemplateStep4Form _step4Form;

        public WorkflowCreateTemplateWizard(
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext,
            Form parentForm)
        {
            _templateService = templateService;
            _loggingService = loggingService;
            _userContext = userContext;
            _parentForm = parentForm;
            _wizardState = new WorkflowWizardState();
        }

        public void ShowWizard()
        {
            ShowStep1();
        }

        private void ShowStep1()
        {
            _step1Form = new WorkflowCreateTemplateForm(_templateService, _loggingService, _userContext, _wizardState);

            if (_step1Form.ShowDialog(_parentForm) == DialogResult.OK)
            {
                ShowStep2();
            }
        }

        private void ShowStep2()
        {
            _step2Form = new WorkflowCreateTemplateStep2Form(_templateService, _loggingService, _userContext, _wizardState);

            var result = _step2Form.ShowDialog(_parentForm);
            if (result == DialogResult.OK)
            {
                ShowStep3();
            }
            else if (result == DialogResult.Retry)
            {
                ShowStep1();
            }
        }

        private void ShowStep3()
        {
            _step3Form = new WorkflowCreateTemplateStep3Form(_templateService, _loggingService, _userContext, _wizardState);

            var result = _step3Form.ShowDialog(_parentForm);
            if (result == DialogResult.OK)
            {
                ShowStep4();
            }
            else if (result == DialogResult.Retry)
            {
                ShowStep2();
            }
        }

        private void ShowStep4()
        {
            _step4Form = new WorkflowCreateTemplateStep4Form(_templateService, _loggingService, _userContext, _wizardState);

            var result = _step4Form.ShowDialog(_parentForm);
            if (result == DialogResult.OK)
            {
                OnWizardComplete();
            }
            else if (result == DialogResult.Retry)
            {
                ShowStep3();
            }
        }

        private void OnWizardComplete()
        {
            try
            {
                var request = new CreateTemplateRequest
                {
                    Name = _wizardState.TemplateName,
                    Description = _wizardState.Description,
                    IsActive = _wizardState.IsPublished,
                    TemplateDefinition = SerializeWizardState(),
                    ChangeDescription = "Created via Workflow Template Wizard"
                };

                _templateService.CreateTemplate(request, _userContext);

                MessageBox.Show(
                    $"✓ Workflow Template '{_wizardState.TemplateName}' สร้างเสร็จแล้ว!\n\nสถานะ: {(_wizardState.IsPublished ? "เผยแพร่" : "Draft")}\n\nสามารถใช้งาน workflow template นี้ได้ทันที",
                    "สร้างสำเร็จ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error creating workflow template: {ex.Message}", ex);
                MessageBox.Show(
                    $"เกิดข้อผิดพลาด: {ex.Message}",
                    "ข้อผิดพลาด",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string SerializeWizardState()
        {
            var sb = new System.Text.StringBuilder();

            // Starting Mode
            if (_wizardState.StartingMode == "copy")
            {
                sb.AppendLine($"StartingMode: copy");
                sb.AppendLine($"CopiedFromTemplate: {_wizardState.CopyFromTemplateId} ({_wizardState.CopyFromTemplateName})");
                sb.AppendLine();
            }
            else if (_wizardState.StartingMode == "import")
            {
                sb.AppendLine($"StartingMode: import");
                sb.AppendLine($"ImportedFrom: {_wizardState.ImportedFilePath}");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"StartingMode: fresh");
                sb.AppendLine();
            }

            sb.AppendLine("Steps:");
            foreach (var step in _wizardState.Steps)
            {
                sb.AppendLine($"  - {step.StepNumber}: {step.StepName} ({step.Action}) -> {step.NextStep}");
            }
            sb.AppendLine("\nApprovers:");
            foreach (var approver in _wizardState.Approvers)
            {
                sb.AppendLine($"  - {approver.StepName}: {approver.ApproverGroup} (Level: {approver.Level})");
            }
            return sb.ToString();
        }
    }
}
