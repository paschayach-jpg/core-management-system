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
        private readonly Form _dialogOwner;
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
            // Use top-level owner so dialogs appear correctly above MDI child forms
            _dialogOwner = parentForm.MdiParent ?? parentForm;
            _wizardState = new WorkflowWizardState();
        }

        public void ShowWizard()
        {
            try
            {
                _loggingService?.Info("ShowWizard: Starting workflow template wizard");
                ShowStep1();
                _loggingService?.Info("ShowWizard: Workflow template wizard completed");
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"ShowWizard Exception: {ex.GetType().Name} - {ex.Message}\nStackTrace: {ex.StackTrace}", ex);
                MessageBox.Show(
                    $"เกิดข้อผิดพลาดในการสร้าง Template Wizard:\n\n{ex.GetType().Name}\n{ex.Message}\n\nกรุณาดูรายละเอียดจาก Log File",
                    "ข้อผิดพลาด",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ShowStep1()
        {
            try
            {
                _loggingService?.Info("ShowStep1: Creating WorkflowCreateTemplateForm");
                _step1Form = new WorkflowCreateTemplateForm(_templateService, _loggingService, _userContext, _wizardState);
                _loggingService?.Info("ShowStep1: Form created successfully, about to show dialog");

                // Verify form controls
                if (_step1Form.Controls.Count == 0)
                {
                    _loggingService?.Warn("ShowStep1: Form has no controls! InitializeComponent may have failed.");
                }

                _loggingService?.Info($"ShowStep1: Form size = {_step1Form.Width}x{_step1Form.Height}, Controls count = {_step1Form.Controls.Count}");
                _loggingService?.Info("ShowStep1: Calling ShowDialog");

                if (_step1Form.ShowDialog(_dialogOwner) == DialogResult.OK)
                {
                    _loggingService?.Info("ShowStep1: Dialog returned OK, proceeding to Step 2");
                    ShowStep2();
                }
                else
                {
                    _loggingService?.Info("ShowStep1: Dialog was cancelled or returned non-OK result");
                }
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"ShowStep1 Exception: {ex.GetType().Name} - {ex.Message}\nStackTrace: {ex.StackTrace}", ex);
                MessageBox.Show(
                    $"เกิดข้อผิดพลาดในขั้นตอนที่ 1 (ข้อมูลพื้นฐาน):\n\n{ex.GetType().Name}\n{ex.Message}\n\nกรุณาดูรายละเอียดจาก Log File",
                    "ข้อผิดพลาด",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ShowStep2()
        {
            _step2Form = new WorkflowCreateTemplateStep2Form(_templateService, _loggingService, _userContext, _wizardState);

            var result = _step2Form.ShowDialog(_dialogOwner);
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

            var result = _step3Form.ShowDialog(_dialogOwner);
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

            var result = _step4Form.ShowDialog(_dialogOwner);
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
                if (_templateService == null)
                {
                    MessageBox.Show(
                        "เปิดหน้าสร้าง Template ได้แล้ว แต่ระบบบันทึก Template ยังไม่พร้อมใช้งาน\n\nกรุณาตรวจสอบ WorkflowTemplateService ในขั้นตอน Startup อีกครั้ง",
                        "Workflow Template Service ไม่พร้อมใช้งาน",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

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
