using System;
using System.Collections.Generic;
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
                    rdoFresh.CheckedChanged += (s, ev) => HandleRadioCheckedChanged(rdoFresh);

                if (rdoCopy != null)
                    rdoCopy.CheckedChanged += (s, ev) => HandleRadioCheckedChanged(rdoCopy);

                if (rdoImport != null)
                    rdoImport.CheckedChanged += (s, ev) => HandleRadioCheckedChanged(rdoImport);

                // Wire up panel card click events to select the radio buttons
                Action selectFresh = () => { if (!rdoFresh.Checked) rdoFresh.Checked = true; };
                if (pnlRadioFresh != null) pnlRadioFresh.Click += (s, ev) => selectFresh();
                if (lblFreshTitle != null) lblFreshTitle.Click += (s, ev) => selectFresh();
                if (lblFreshDesc != null) lblFreshDesc.Click += (s, ev) => selectFresh();

                Action selectCopy = () => { if (rdoCopy.Enabled && !rdoCopy.Checked) rdoCopy.Checked = true; };
                if (pnlRadioCopy != null) pnlRadioCopy.Click += (s, ev) => selectCopy();
                if (lblCopyTitle != null) lblCopyTitle.Click += (s, ev) => selectCopy();
                if (lblCopyDesc != null) lblCopyDesc.Click += (s, ev) => selectCopy();

                Action selectImport = () => { if (!rdoImport.Checked) rdoImport.Checked = true; };
                if (pnlRadioImport != null) pnlRadioImport.Click += (s, ev) => selectImport();
                if (lblImportTitle != null) lblImportTitle.Click += (s, ev) => selectImport();
                if (lblImportDesc != null) lblImportDesc.Click += (s, ev) => selectImport();

                // Wire up custom paint events for panels to draw the premium selected blue border
                if (pnlRadioFresh != null)
                {
                    pnlRadioFresh.Paint += (s, pe) => {
                        if (rdoFresh.Checked) {
                            using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(37, 99, 235), 2)) {
                                pe.Graphics.DrawRectangle(pen, 0, 0, pnlRadioFresh.Width - 1, pnlRadioFresh.Height - 1);
                            }
                        }
                    };
                }
                if (pnlRadioCopy != null)
                {
                    pnlRadioCopy.Paint += (s, pe) => {
                        if (rdoCopy.Checked) {
                            using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(37, 99, 235), 2)) {
                                pe.Graphics.DrawRectangle(pen, 0, 0, pnlRadioCopy.Width - 1, pnlRadioCopy.Height - 1);
                            }
                        }
                    };
                }
                if (pnlRadioImport != null)
                {
                    pnlRadioImport.Paint += (s, pe) => {
                        if (rdoImport.Checked) {
                            using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(37, 99, 235), 2)) {
                                pe.Graphics.DrawRectangle(pen, 0, 0, pnlRadioImport.Width - 1, pnlRadioImport.Height - 1);
                            }
                        }
                    };
                }

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
                    cmbCopyFrom.ValueMember = "WorkflowDefinitionId";
                    cmbCopyFrom.SelectedIndexChanged += cmbCopyFrom_SelectedIndexChanged;
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

            if (rdoCopy != null && rdoCopy.Checked)
            {
                cmbCopyFrom_SelectedIndexChanged(null, null);
            }
        }

        private bool _isChangingMode = false;

        private void HandleRadioCheckedChanged(RadioButton selectedRadio)
        {
            if (_isChangingMode) return;
            if (!selectedRadio.Checked) return;

            _isChangingMode = true;
            try
            {
                if (selectedRadio == rdoFresh)
                {
                    if (rdoCopy != null) rdoCopy.Checked = false;
                    if (rdoImport != null) rdoImport.Checked = false;
                }
                else if (selectedRadio == rdoCopy)
                {
                    if (rdoFresh != null) rdoFresh.Checked = false;
                    if (rdoImport != null) rdoImport.Checked = false;
                }
                else if (selectedRadio == rdoImport)
                {
                    if (rdoFresh != null) rdoFresh.Checked = false;
                    if (rdoCopy != null) rdoCopy.Checked = false;
                }
            }
            finally
            {
                _isChangingMode = false;
            }

            OnModeChanged();
        }

        private void cmbCopyFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCopyFrom == null || !rdoCopy.Checked)
                return;

            var selectedTemplate = cmbCopyFrom.SelectedItem as WorkflowTemplateResult;
            if (selectedTemplate != null)
            {
                txtTemplateName.Text = selectedTemplate.Name + " (Copy)";
                txtDescription.Text = selectedTemplate.Description ?? "";
                
                SetComboValue(cmbCategory, selectedTemplate.Category, "งานบุคคล (HR)");
                SetComboValue(cmbOwner, selectedTemplate.OwnerDepartment, "AD Group: HR-Process-Owners");
                
                // Load steps and approvers of this template from the DB into _wizardState
                LoadStepsAndApproversFromCopiedTemplate(selectedTemplate.WorkflowDefinitionId);
            }
        }

        private void LoadStepsAndApproversFromCopiedTemplate(int copyFromTemplateId)
        {
            var selectedTemplate = cmbCopyFrom.SelectedItem as WorkflowTemplateResult;
            if (selectedTemplate != null && !string.IsNullOrWhiteSpace(selectedTemplate.TemplateDefinition) && selectedTemplate.TemplateDefinition.Trim().StartsWith("{"))
            {
                try
                {
                    var copiedState = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkflowWizardState>(selectedTemplate.TemplateDefinition);
                    if (copiedState != null)
                    {
                        _wizardState.Steps.Clear();
                        if (copiedState.Steps != null)
                            _wizardState.Steps.AddRange(copiedState.Steps);

                        _wizardState.Approvers.Clear();
                        if (copiedState.Approvers != null)
                            _wizardState.Approvers.AddRange(copiedState.Approvers);

                        _wizardState.FormSections.Clear();
                        if (copiedState.FormSections != null)
                            _wizardState.FormSections.AddRange(copiedState.FormSections);

                        _wizardState.FormFields.Clear();
                        if (copiedState.FormFields != null)
                            _wizardState.FormFields.AddRange(copiedState.FormFields);

                        _wizardState.FormTitle = copiedState.FormTitle;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _loggingService?.Warn($"Failed to deserialize TemplateDefinition JSON, falling back to database query: {ex.Message}");
                }
            }

            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["CmsDb"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Fetch steps
                    var steps = new List<WorkflowStepData>();
                    var stepIds = new Dictionary<int, string>(); // WFStepId -> StepName

                    string stepsSql = "SELECT WFStepId, StepName, StepType, SequenceNumber FROM WFE.s_WFStep WHERE WFDefinitionId = @WFDefinitionId AND IsDeleted = 0 ORDER BY SequenceNumber";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(stepsSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@WFDefinitionId", copyFromTemplateId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int stepId = Convert.ToInt32(reader["WFStepId"]);
                                string stepName = Convert.ToString(reader["StepName"]);
                                int stepTypeByte = Convert.ToInt32(reader["StepType"]);
                                int seqNo = Convert.ToInt32(reader["SequenceNumber"]);

                                stepIds[stepId] = stepName;

                                string actionStr = "Approve";
                                if (stepTypeByte == 2) actionStr = "Approve";
                                else if (stepTypeByte == 3) actionStr = "Review";
                                else if (stepTypeByte == 4) actionStr = "Notify";
                                else if (stepTypeByte == 8) actionStr = "End";
                                else actionStr = "System Task";

                                steps.Add(new WorkflowStepData
                                {
                                    StepNumber = seqNo,
                                    StepName = stepName,
                                    Action = actionStr,
                                    NextStep = "End" // Will resolve below
                                });
                            }
                        }
                    }

                    // 2. Fetch connections to resolve NextStep
                    string connSql = "SELECT FromStepId, ToStepId FROM WFE.s_WFConnection WHERE WFDefinitionId = @WFDefinitionId AND IsDeleted = 0";
                    var connections = new Dictionary<int, int>(); // FromStepId -> ToStepId
                    using (var cmd = new System.Data.SqlClient.SqlCommand(connSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@WFDefinitionId", copyFromTemplateId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int fromId = Convert.ToInt32(reader["FromStepId"]);
                                int toId = Convert.ToInt32(reader["ToStepId"]);
                                connections[fromId] = toId;
                            }
                        }
                    }

                    // Resolve NextStep for each step using connections
                    for (int i = 0; i < steps.Count; i++)
                    {
                        // Find stepId matching the stepName
                        int stepId = stepIds.FirstOrDefault(x => x.Value == steps[i].StepName).Key;
                        if (stepId > 0 && connections.TryGetValue(stepId, out int nextStepId))
                        {
                            if (stepIds.TryGetValue(nextStepId, out string nextStepName))
                            {
                                steps[i].NextStep = nextStepName;
                            }
                        }
                    }

                    // 3. Clear and pre-populate steps in wizard state
                    _wizardState.Steps.Clear();
                    _wizardState.Steps.AddRange(steps);

                    // 4. Pre-populate approvers for steps that have assigned roles/users
                    var approvers = new List<WorkflowApproverData>();
                    string approversSql = "SELECT StepName, AssignedRole FROM WFE.s_WFStep WHERE WFDefinitionId = @WFDefinitionId AND IsDeleted = 0 AND AssignedRole IS NOT NULL AND AssignedRole <> '' ORDER BY SequenceNumber";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(approversSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@WFDefinitionId", copyFromTemplateId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string stepName = Convert.ToString(reader["StepName"]);
                                string assignedRole = Convert.ToString(reader["AssignedRole"]);

                                approvers.Add(new WorkflowApproverData
                                {
                                    StepName = stepName,
                                    ApproverGroup = assignedRole,
                                    Level = "1"
                                });
                            }
                        }
                    }

                    _wizardState.Approvers.Clear();
                    _wizardState.Approvers.AddRange(approvers);
                }
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"LoadStepsAndApproversFromCopiedTemplate error: {ex.Message}", ex);
            }
        }

        private void UpdateCardStyles()
        {
            // Update Fresh card
            if (rdoFresh.Checked)
            {
                pnlRadioFresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255))))); // #EFF6FF
                pnlRadioFresh.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            }
            else
            {
                pnlRadioFresh.BackColor = System.Drawing.Color.White;
            }
            pnlRadioFresh.Invalidate();

            // Update Copy card
            if (rdoCopy.Checked)
            {
                pnlRadioCopy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
                pnlRadioCopy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            }
            else
            {
                pnlRadioCopy.BackColor = System.Drawing.Color.White;
            }
            pnlRadioCopy.Invalidate();

            // Update Import card
            if (rdoImport.Checked)
            {
                pnlRadioImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
                pnlRadioImport.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            }
            else
            {
                pnlRadioImport.BackColor = System.Drawing.Color.White;
            }
            pnlRadioImport.Invalidate();
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
