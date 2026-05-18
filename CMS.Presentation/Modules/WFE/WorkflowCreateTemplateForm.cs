using System;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class WorkflowCreateTemplateForm : Form
    {
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;
        private readonly WorkflowWizardState _wizardState;
        private int _currentStep = 1;

        public WorkflowCreateTemplateForm(
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

            this.Load += WorkflowCreateTemplateForm_Load;
            this.Closing += (s, e) =>
            {
                if (this.DialogResult == DialogResult.None)
                    this.DialogResult = DialogResult.Cancel;
            };
        }

        private void WorkflowCreateTemplateForm_Load(object sender, EventArgs e)
        {
            try
            {
                _loggingService?.Info("WorkflowCreateTemplateForm_Load: Starting initialization");

                // Wire up button events
                var btnNext = FindButton("btnNext");
                var btnSaveDraft = FindButton("btnSaveDraft");
                var btnCancel = FindButton("btnCancel");
                var btnBrowse = FindButton("btnBrowse");

                _loggingService?.Info($"WorkflowCreateTemplateForm_Load: Buttons found - Next:{btnNext != null}, SaveDraft:{btnSaveDraft != null}, Cancel:{btnCancel != null}, Browse:{btnBrowse != null}");

                if (btnNext != null)
                    btnNext.Click += (s, ev) => OnNextClick();

                if (btnSaveDraft != null)
                    btnSaveDraft.Click += (s, ev) => OnSaveDraftClick();

                if (btnCancel != null)
                    btnCancel.Click += (s, ev) => this.DialogResult = DialogResult.Cancel;

                if (btnBrowse != null)
                    btnBrowse.Click += (s, ev) => OnBrowseClick();

                if (txtTemplateCode != null)
                    txtTemplateCode.ReadOnly = true;

                // Wire up radio button change events
                if (rdoFresh != null)
                    rdoFresh.CheckedChanged += (s, ev) => OnModeChanged();

                if (rdoCopy != null)
                    rdoCopy.CheckedChanged += (s, ev) => OnModeChanged();

                if (rdoImport != null)
                    rdoImport.CheckedChanged += (s, ev) => OnModeChanged();

                // Load data and template list
                _loggingService?.Info("WorkflowCreateTemplateForm_Load: Calling InitializeDropdownData");
                InitializeDropdownData();

                if (_templateService == null && rdoCopy != null)
                {
                    rdoCopy.Checked = false;
                    rdoCopy.Enabled = false;
                }

                _loggingService?.Info("WorkflowCreateTemplateForm_Load: Calling LoadTemplateList");
                LoadTemplateList();

                _loggingService?.Info("WorkflowCreateTemplateForm_Load: Calling LoadFormData");
                LoadFormData();

                _loggingService?.Info("WorkflowCreateTemplateForm_Load: Initialization completed successfully");
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"WorkflowCreateTemplateForm_Load Exception: {ex.GetType().Name} - {ex.Message}\nStackTrace: {ex.StackTrace}", ex);
                MessageBox.Show(
                    $"เกิดข้อผิดพลาดในการเริ่มต้น Form:\n\n{ex.GetType().Name}\n{ex.Message}",
                    "ข้อผิดพลาด",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private void InitializeDropdownData()
        {
            if (cmbCategory != null)
            {
                cmbCategory.Items.Clear();
                cmbCategory.Items.AddRange(new object[]
                {
                    "การอนุมัติเอกสาร",
                    "Knowledge Management",
                    "งานบุคคล (HR)",
                    "IT Infrastructure",
                    "การเงิน/จัดซื้อ",
                    "อื่น ๆ"
                });
            }

            if (cmbOwner != null)
            {
                cmbOwner.Items.Clear();
                cmbOwner.Items.AddRange(new object[]
                {
                    "AD Group: CMS-Knowledge-Owners",
                    "AD Group: CMS-Legal-Managers",
                    "AD Group: HR-Process-Owners",
                    "AD Group: IT-Workflow-Admins",
                    "ฉันเอง (อรรพพล ส.)"
                });
            }

            if (cmbPriority != null)
            {
                cmbPriority.Items.Clear();
                cmbPriority.Items.AddRange(new object[] { "ต่ำ", "ปกติ", "สูง", "เร่งด่วน" });
            }

            if (cmbInitiator != null)
            {
                cmbInitiator.Items.Clear();
                cmbInitiator.Items.AddRange(new object[]
                {
                    "พนักงานทุกคน",
                    "หัวหน้างานขึ้นไป",
                    "เฉพาะเจ้าของข้อมูล"
                });
            }
        }

        private Button FindButton(string name)
        {
            return FindControl(this.Controls, name) as Button;
        }

        private void LoadTemplateList()
        {
            try
            {
                if (_templateService == null || _userContext == null)
                {
                    if (cmbCopyFrom != null)
                    {
                        cmbCopyFrom.DataSource = null;
                        cmbCopyFrom.Items.Clear();
                    }
                    return;
                }

                var filter = new WorkflowTemplateFilter { IsActive = true };
                var result = _templateService.GetTemplates(filter, _userContext);

                if (result?.Items != null && result.Items.Any() && cmbCopyFrom != null)
                {
                    cmbCopyFrom.DataSource = result.Items;
                    cmbCopyFrom.DisplayMember = "Name";
                    cmbCopyFrom.ValueMember = "TemplateId";
                }
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error loading template list: {ex.Message}", ex);
            }
        }

        private void LoadFormData()
        {
            // Restore from wizard state if returning from Step 2
            if (_wizardState != null)
            {
                txtTemplateCode.Text = string.IsNullOrWhiteSpace(_wizardState.TemplateCode)
                    ? "WFD-008"
                    : _wizardState.TemplateCode;

                if (_wizardState.StartingMode == "copy")
                {
                    rdoCopy.Checked = true;
                    if (_wizardState.CopyFromTemplateId.HasValue)
                        cmbCopyFrom.SelectedValue = _wizardState.CopyFromTemplateId.Value;
                }
                else if (_wizardState.StartingMode == "import")
                {
                    rdoImport.Checked = true;
                    txtImportPath.Text = _wizardState.ImportedFilePath ?? "";
                }
                else
                {
                    rdoFresh.Checked = true;
                }

                if (!string.IsNullOrEmpty(_wizardState.TemplateName))
                    txtTemplateName.Text = _wizardState.TemplateName;

                if (!string.IsNullOrEmpty(_wizardState.Description))
                    txtDescription.Text = _wizardState.Description;

                SetComboValue(cmbCategory, _wizardState.Category, "งานบุคคล (HR)");
                SetComboValue(cmbOwner, _wizardState.Owner, "AD Group: HR-Process-Owners");
                SetComboValue(cmbPriority, _wizardState.Priority, "ปกติ");
                SetComboValue(cmbInitiator, _wizardState.Initiator, "พนักงานทุกคน");

                if (txtSlaDays != null)
                {
                    var slaDays = _wizardState.SlaDays <= 0 ? 5 : _wizardState.SlaDays;
                    txtSlaDays.Text = slaDays.ToString();
                }

                if (txtTags != null)
                    txtTags.Text = _wizardState.TagsText ?? "HR, PDPA, พนักงาน";

                if (chkFormBuilder != null)
                    chkFormBuilder.Checked = _wizardState.UseFormBuilder;

                if (chkOwnerCommittee != null)
                    chkOwnerCommittee.Checked = _wizardState.RequireOwnerCommitteeApproval;

                if (chkAllowCancel != null)
                    chkAllowCancel.Checked = _wizardState.AllowInitiatorCancel;

                if (chkAutoStoreToDms != null)
                    chkAutoStoreToDms.Checked = _wizardState.AutoStoreStepFilesToDms;
            }
            else
            {
                rdoFresh.Checked = true;
                txtTemplateCode.Text = "WFD-008";
                SetComboValue(cmbCategory, null, "งานบุคคล (HR)");
                SetComboValue(cmbOwner, null, "AD Group: HR-Process-Owners");
                SetComboValue(cmbPriority, null, "ปกติ");
                SetComboValue(cmbInitiator, null, "พนักงานทุกคน");
                txtSlaDays.Text = "5";
                txtTags.Text = "HR, PDPA, พนักงาน";
                chkFormBuilder.Checked = true;
                chkOwnerCommittee.Checked = true;
            }

            OnModeChanged();
        }

        private void SetComboValue(ComboBox comboBox, string value, string fallback)
        {
            if (comboBox == null || comboBox.Items.Count == 0)
                return;

            var target = string.IsNullOrWhiteSpace(value) ? fallback : value;
            var selectedIndex = comboBox.FindStringExact(target);
            comboBox.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
        }

        private void OnModeChanged()
        {
            if (_templateService == null && rdoCopy != null && rdoCopy.Checked)
            {
                rdoFresh.Checked = true;
            }

            UpdateCardStyles();
            ShowConditionalPanels();
        }

        private void UpdateCardStyles()
        {
            // Update Fresh card
            if (rdoFresh.Checked)
            {
                pnlRadioFresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255))))); // #EFF6FF
                pnlRadioFresh.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235))))), 2)) // #2563EB
                {
                    pnlRadioFresh.Invalidate();
                }
            }
            else
            {
                pnlRadioFresh.BackColor = System.Drawing.Color.White;
            }

            // Update Copy card
            if (rdoCopy.Checked)
            {
                pnlRadioCopy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
                pnlRadioCopy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235))))), 2))
                {
                    pnlRadioCopy.Invalidate();
                }
            }
            else
            {
                pnlRadioCopy.BackColor = System.Drawing.Color.White;
            }

            // Update Import card
            if (rdoImport.Checked)
            {
                pnlRadioImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
                pnlRadioImport.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235))))), 2))
                {
                    pnlRadioImport.Invalidate();
                }
            }
            else
            {
                pnlRadioImport.BackColor = System.Drawing.Color.White;
            }
        }

        private void ShowConditionalPanels()
        {
            pnlCopyOptions.Visible = rdoCopy.Checked;
            pnlImportOptions.Visible = rdoImport.Checked;
        }

        private void OnBrowseClick()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Workflow Files (*.wfd;*.bpmn)|*.wfd;*.bpmn|All Files (*.*)|*.*";
                dialog.Title = "เลือกไฟล์ workflow";

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtImportPath.Text = dialog.FileName;
                }
            }
        }

        private void OnNextClick()
        {
            var templateData = GetTemplateData();
            if (templateData != null)
            {
                // Save mode to wizard state
                _wizardState.StartingMode = rdoFresh.Checked ? "fresh" : rdoCopy.Checked ? "copy" : "import";

                if (rdoCopy.Checked)
                {
                    _wizardState.CopyFromTemplateId = (int?)cmbCopyFrom.SelectedValue;
                    _wizardState.CopyFromTemplateName = cmbCopyFrom.Text;
                }
                else if (rdoImport.Checked)
                {
                    _wizardState.ImportedFilePath = txtImportPath.Text.Trim();
                }

                _wizardState.TemplateName = templateData.Name;
                _wizardState.Description = templateData.Description;
                _wizardState.TemplateCode = txtTemplateCode.Text.Trim();
                _wizardState.Category = cmbCategory.Text;
                _wizardState.Owner = cmbOwner.Text;
                _wizardState.SlaDays = int.Parse(txtSlaDays.Text.Trim());
                _wizardState.Priority = cmbPriority.Text;
                _wizardState.Initiator = cmbInitiator.Text;
                _wizardState.TagsText = txtTags.Text.Trim();
                _wizardState.UseFormBuilder = chkFormBuilder.Checked;
                _wizardState.RequireOwnerCommitteeApproval = chkOwnerCommittee.Checked;
                _wizardState.AllowInitiatorCancel = chkAllowCancel.Checked;
                _wizardState.AutoStoreStepFilesToDms = chkAutoStoreToDms.Checked;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void OnSaveDraftClick()
        {
            var templateData = GetTemplateData();
            if (templateData != null)
            {
                MessageBox.Show("Template บันทึกเป็น Draft เรียบร้อยแล้ว", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        public CreateTemplateRequest GetTemplateData()
        {
            // Validate template code
            if (string.IsNullOrWhiteSpace(txtTemplateCode?.Text))
            {
                MessageBox.Show("กรุณาระบุรหัส Template", "ข้อมูลไม่สมบูรณ์",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            // Validate template name
            if (string.IsNullOrWhiteSpace(txtTemplateName?.Text))
            {
                MessageBox.Show("กรุณาระบุชื่อ Template", "ข้อมูลไม่สมบูรณ์",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            if (cmbCategory == null || cmbCategory.SelectedIndex < 0)
            {
                MessageBox.Show("กรุณาเลือกหมวดหมู่", "ข้อมูลไม่สมบูรณ์",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            if (cmbOwner == null || cmbOwner.SelectedIndex < 0)
            {
                MessageBox.Show("กรุณาเลือกเจ้าของ (Owner)", "ข้อมูลไม่สมบูรณ์",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            int slaDays;
            if (!int.TryParse(txtSlaDays.Text.Trim(), out slaDays) || slaDays <= 0)
            {
                MessageBox.Show("SLA รวมเป้าหมายต้องเป็นตัวเลขมากกว่า 0", "ข้อมูลไม่สมบูรณ์",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            // Validate Copy mode
            if (rdoCopy.Checked && cmbCopyFrom.SelectedIndex < 0)
            {
                MessageBox.Show("กรุณาเลือก Template ต้นแบบ", "ข้อมูลไม่สมบูรณ์",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            // Validate Import mode
            if (rdoImport.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtImportPath?.Text))
                {
                    MessageBox.Show("กรุณาเลือกไฟล์ที่จะนำเข้า", "ข้อมูลไม่สมบูรณ์",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                if (!System.IO.File.Exists(txtImportPath.Text.Trim()))
                {
                    MessageBox.Show("ไฟล์ที่ระบุไม่พบ", "ข้อผิดพลาด",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            return new CreateTemplateRequest
            {
                Name = txtTemplateName.Text.Trim(),
                Description = txtDescription?.Text?.Trim() ?? "",
                IsActive = true,
                TemplateDefinition = "{}",
                ChangeDescription = string.Format(
                    "Initial template creation (Code={0}, Category={1}, Owner={2}, SLA={3}d)",
                    txtTemplateCode.Text.Trim(),
                    cmbCategory.Text,
                    cmbOwner.Text,
                    slaDays)
            };
        }

        private Control FindControl(Control.ControlCollection controls, string name)
        {
            foreach (Control ctrl in controls)
            {
                if (ctrl.Name == name)
                    return ctrl;

                // Recursively search in containers (Panel, GroupBox, TabPage, and any control with child controls)
                if (ctrl.HasChildren)
                {
                    var found = FindControl(ctrl.Controls, name);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        public int CurrentStep => _currentStep;
    }
}
