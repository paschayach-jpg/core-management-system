using System;
using System.Text;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class WorkflowCreateTemplateStep4Form : Form
    {
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;
        private readonly WorkflowWizardState _wizardState;

        public WorkflowCreateTemplateStep4Form(
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext,
            WorkflowWizardState wizardState)
        {
            InitializeComponent();
            _templateService = templateService;
            _loggingService = loggingService;
            _userContext = userContext;
            _wizardState = wizardState;

            this.Load += WorkflowCreateTemplateStep4Form_Load;
            this.Closing += (s, e) =>
            {
                if (this.DialogResult == DialogResult.None)
                    this.DialogResult = DialogResult.Cancel;
            };
        }

        private void WorkflowCreateTemplateStep4Form_Load(object sender, EventArgs e)
        {
            var btnPrev = this.Controls.Find("btnPrev", true)[0] as Button;
            var btnComplete = this.Controls.Find("btnComplete", true)[0] as Button;

            btnPrev.Click += (s, ev) => { this.DialogResult = DialogResult.Retry; this.Close(); };
            btnComplete.Click += (s, ev) => OnCompleteClick();

            LoadFormData();
        }

        private void LoadFormData()
        {
            var txtSummary = FindControl(this.Controls, "txtSummary") as TextBox;

            if (txtSummary != null)
            {
                txtSummary.Text = GenerateSummary();
            }

            var rdoPublish = FindControl(this.Controls, "rdoPublish") as RadioButton;
            if (rdoPublish != null)
            {
                rdoPublish.Checked = _wizardState.IsPublished;
            }

            var rdoDraft = FindControl(this.Controls, "rdoDraft") as RadioButton;
            if (rdoDraft != null)
            {
                rdoDraft.Checked = !_wizardState.IsPublished;
            }
        }

        private string GenerateSummary()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"✓ ชื่อ Template: {_wizardState.TemplateName}");

            if (!string.IsNullOrEmpty(_wizardState.Description))
                sb.AppendLine($"✓ คำอธิบาย: {_wizardState.Description}");

            sb.AppendLine();
            sb.AppendLine($"✓ จำนวนขั้นตอน: {_wizardState.Steps.Count}");
            foreach (var step in _wizardState.Steps)
            {
                sb.AppendLine($"  - {step.StepName}");
            }

            sb.AppendLine();
            sb.AppendLine($"✓ การกำหนด Approver: {_wizardState.Approvers.Count} ขั้นตอน");
            foreach (var approver in _wizardState.Approvers)
            {
                sb.AppendLine($"  - {approver.StepName}: {approver.ApproverGroup}");
            }

            return sb.ToString();
        }

        private Control FindControl(Control.ControlCollection controls, string name)
        {
            foreach (Control ctrl in controls)
            {
                if (ctrl.Name == name)
                    return ctrl;

                if (ctrl is Panel || ctrl is GroupBox || ctrl is TabPage)
                {
                    var found = FindControl(ctrl.Controls, name);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        private void OnCompleteClick()
        {
            var rdoPublish = FindControl(this.Controls, "rdoPublish") as RadioButton;
            _wizardState.IsPublished = rdoPublish?.Checked ?? true;

            MessageBox.Show(
                $"✓ Template {_wizardState.TemplateName} สร้างสำเร็จ!\n\nสถานะ: {(_wizardState.IsPublished ? "เผยแพร่" : "Draft")}\n\nสามารถใช้งาน workflow template นี้ได้ทันที",
                "สร้างสำเร็จ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
