using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class WorkflowCreateTemplateStep2Form : Form
    {
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;
        private readonly WorkflowWizardState _wizardState;

        public WorkflowCreateTemplateStep2Form(
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

            this.Load += WorkflowCreateTemplateStep2Form_Load;
            this.Closing += (s, e) =>
            {
                if (this.DialogResult == DialogResult.None)
                    this.DialogResult = DialogResult.Cancel;
            };
        }

        private void WorkflowCreateTemplateStep2Form_Load(object sender, EventArgs e)
        {
            btnPrev.Click += (s, ev) => { DialogResult = DialogResult.Retry; Close(); };
            btnNext.Click += (s, ev) => OnNextClick();
            btnAddStep.Click += (s, ev) => AddEmptyRow();
            btnDeleteStep.Click += (s, ev) => DeleteSelectedRow();
            btnMoveUp.Click += (s, ev) => MoveSelectedRow(-1);
            btnMoveDown.Click += (s, ev) => MoveSelectedRow(1);
            btnAutoPath.Click += (s, ev) => AutoGeneratePath();

            InitializeDataGridView();
            LoadFormData();
        }

        private void InitializeDataGridView()
        {
            dgvSteps.AllowUserToAddRows = false;
            dgvSteps.AllowUserToDeleteRows = false;
            dgvSteps.MultiSelect = false;
            dgvSteps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSteps.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvSteps.RowHeadersVisible = false;

            if (colAction is DataGridViewComboBoxColumn)
            {
                var actionColumn = colAction as DataGridViewComboBoxColumn;
                actionColumn.Items.Clear();
                actionColumn.Items.AddRange(new object[]
                {
                    "Approve",
                    "Review",
                    "Notify",
                    "System Task",
                    "End"
                });
            }

            dgvSteps.CellValueChanged += (s, e) => { RefreshStepNumbers(); RefreshNodeTypes(); RefreshPreview(); };
            dgvSteps.RowsRemoved += (s, e) => { RefreshStepNumbers(); RefreshNodeTypes(); RefreshPreview(); };
        }

        private void LoadFormData()
        {
            dgvSteps.Rows.Clear();

            foreach (var step in _wizardState.Steps.OrderBy(s => s.StepNumber))
            {
                var rowIndex = dgvSteps.Rows.Add();
                var row = dgvSteps.Rows[rowIndex];
                row.Cells[colSeq.Index].Value = step.StepNumber;
                row.Cells[colStepName.Index].Value = step.StepName;
                row.Cells[colAction.Index].Value = string.IsNullOrWhiteSpace(step.Action) ? "Approve" : step.Action;
                row.Cells[colNextStep.Index].Value = step.NextStep;
            }

            if (dgvSteps.Rows.Count == 0)
            {
                AddEmptyRow();
            }

            RefreshStepNumbers();
            RefreshNodeTypes();
            RefreshPreview();
        }

        private void AddEmptyRow()
        {
            var rowIndex = dgvSteps.Rows.Add();
            var row = dgvSteps.Rows[rowIndex];
            row.Cells[colSeq.Index].Value = rowIndex + 1;
            row.Cells[colAction.Index].Value = "Approve";
            row.Cells[colNodeType.Index].Value = "-";
            dgvSteps.CurrentCell = row.Cells[colStepName.Index];
            dgvSteps.BeginEdit(true);
            RefreshNodeTypes();
            RefreshPreview();
        }

        private void DeleteSelectedRow()
        {
            if (dgvSteps.CurrentRow == null)
                return;

            if (dgvSteps.Rows.Count <= 1)
            {
                MessageBox.Show("ต้องมีอย่างน้อย 1 ขั้นตอน", "ไม่สามารถลบได้",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvSteps.Rows.Remove(dgvSteps.CurrentRow);
            RefreshStepNumbers();
            RefreshNodeTypes();
            RefreshPreview();
        }

        private void MoveSelectedRow(int direction)
        {
            if (dgvSteps.CurrentRow == null)
                return;

            var currentIndex = dgvSteps.CurrentRow.Index;
            var targetIndex = currentIndex + direction;

            if (targetIndex < 0 || targetIndex >= dgvSteps.Rows.Count)
                return;

            var values = new object[dgvSteps.Columns.Count];
            for (var i = 0; i < dgvSteps.Columns.Count; i++)
                values[i] = dgvSteps.Rows[currentIndex].Cells[i].Value;

            for (var i = 0; i < dgvSteps.Columns.Count; i++)
            {
                dgvSteps.Rows[currentIndex].Cells[i].Value = dgvSteps.Rows[targetIndex].Cells[i].Value;
                dgvSteps.Rows[targetIndex].Cells[i].Value = values[i];
            }

            dgvSteps.CurrentCell = dgvSteps.Rows[targetIndex].Cells[colStepName.Index];
            RefreshStepNumbers();
            RefreshNodeTypes();
            RefreshPreview();
        }

        private void RefreshStepNumbers()
        {
            for (var i = 0; i < dgvSteps.Rows.Count; i++)
            {
                dgvSteps.Rows[i].Cells[colSeq.Index].Value = i + 1;
            }
        }

        private void RefreshNodeTypes()
        {
            if (dgvSteps.Rows.Count == 0)
                return;

            var stepNames = new List<string>();
            foreach (DataGridViewRow row in dgvSteps.Rows)
            {
                stepNames.Add((row.Cells[colStepName.Index].Value ?? string.Empty).ToString().Trim());
            }

            var incoming = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataGridViewRow row in dgvSteps.Rows)
            {
                var nextStep = (row.Cells[colNextStep.Index].Value ?? string.Empty).ToString().Trim();
                if (!string.IsNullOrWhiteSpace(nextStep) && !nextStep.Equals("End", StringComparison.OrdinalIgnoreCase))
                    incoming.Add(nextStep);
            }

            var startIndex = -1;
            for (var i = 0; i < dgvSteps.Rows.Count; i++)
            {
                var stepName = stepNames[i];
                if (!string.IsNullOrWhiteSpace(stepName) && !incoming.Contains(stepName))
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex < 0 && dgvSteps.Rows.Count > 0)
                startIndex = 0;

            for (var i = 0; i < dgvSteps.Rows.Count; i++)
            {
                var row = dgvSteps.Rows[i];
                var action = (row.Cells[colAction.Index].Value ?? string.Empty).ToString().Trim();
                var nextStep = (row.Cells[colNextStep.Index].Value ?? string.Empty).ToString().Trim();

                var isStart = i == startIndex;
                var isEnd = action.Equals("End", StringComparison.OrdinalIgnoreCase)
                    || nextStep.Equals("End", StringComparison.OrdinalIgnoreCase);

                if (isStart && isEnd)
                    row.Cells[colNodeType.Index].Value = "Start/End";
                else if (isStart)
                    row.Cells[colNodeType.Index].Value = "Start";
                else if (isEnd)
                    row.Cells[colNodeType.Index].Value = "End";
                else
                    row.Cells[colNodeType.Index].Value = "Middle";
            }
        }

        private void OnNextClick()
        {
            if (!ValidateSteps())
                return;

            _wizardState.Steps.Clear();

            for (var i = 0; i < dgvSteps.Rows.Count; i++)
            {
                var row = dgvSteps.Rows[i];
                var stepName = (row.Cells[colStepName.Index].Value ?? string.Empty).ToString().Trim();
                var action = (row.Cells[colAction.Index].Value ?? string.Empty).ToString().Trim();
                var nextStep = (row.Cells[colNextStep.Index].Value ?? string.Empty).ToString().Trim();

                _wizardState.Steps.Add(new WorkflowStepData
                {
                    StepNumber = i + 1,
                    StepName = stepName,
                    Action = action,
                    NextStep = nextStep
                });
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateSteps()
        {
            var names = new List<string>();

            foreach (DataGridViewRow row in dgvSteps.Rows)
            {
                var stepName = (row.Cells[colStepName.Index].Value ?? string.Empty).ToString().Trim();
                if (string.IsNullOrWhiteSpace(stepName))
                {
                    MessageBox.Show("กรุณากรอกชื่อขั้นตอนให้ครบทุกแถว", "ข้อมูลไม่สมบูรณ์",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                names.Add(stepName);
            }

            if (names.Count == 0)
            {
                MessageBox.Show("กรุณาเพิ่มอย่างน้อย 1 ขั้นตอน", "ข้อมูลไม่สมบูรณ์",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var duplicate = names.GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null)
            {
                MessageBox.Show("ชื่อขั้นตอนซ้ำ: " + duplicate.Key, "ข้อมูลไม่ถูกต้อง",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Validate NextStep references must match an existing step name or "End"
            var validTargets = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
            validTargets.Add("End");

            for (var i = 0; i < dgvSteps.Rows.Count; i++)
            {
                var nextStep = (dgvSteps.Rows[i].Cells[colNextStep.Index].Value ?? string.Empty).ToString().Trim();
                if (!string.IsNullOrWhiteSpace(nextStep) && !validTargets.Contains(nextStep))
                {
                    MessageBox.Show(
                        $"แถวที่ {i + 1}: ขั้นตอนถัดไป \"{nextStep}\" ไม่ตรงกับชื่อขั้นตอนที่มีอยู่\n"
                        + "กรุณาใส่ชื่อ Step ที่ถูกต้องหรือ \"End\"",
                        "ข้อมูลไม่ถูกต้อง",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private void AutoGeneratePath()
        {
            if (dgvSteps.Rows.Count == 0)
                return;

            dgvSteps.EndEdit();

            for (var i = 0; i < dgvSteps.Rows.Count; i++)
            {
                if (i < dgvSteps.Rows.Count - 1)
                {
                    var nextName = (dgvSteps.Rows[i + 1].Cells[colStepName.Index].Value ?? string.Empty).ToString().Trim();
                    dgvSteps.Rows[i].Cells[colNextStep.Index].Value = string.IsNullOrWhiteSpace(nextName)
                        ? $"(ขั้นตอน {i + 2})"
                        : nextName;
                }
                else
                {
                    dgvSteps.Rows[i].Cells[colNextStep.Index].Value = "End";
                }
            }

            RefreshNodeTypes();
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (dgvSteps.Rows.Count == 0)
            {
                rtbPreview.Text = "(ยังไม่มีขั้นตอน)";
                return;
            }

            var parts = new List<string>();
            var starts = new List<string>();
            var ends = new List<string>();

            for (var i = 0; i < dgvSteps.Rows.Count; i++)
            {
                var stepName = (dgvSteps.Rows[i].Cells[colStepName.Index].Value ?? string.Empty).ToString().Trim();
                var action = (dgvSteps.Rows[i].Cells[colAction.Index].Value ?? string.Empty).ToString().Trim();
                var nodeType = (dgvSteps.Rows[i].Cells[colNodeType.Index].Value ?? string.Empty).ToString().Trim();

                if (string.IsNullOrWhiteSpace(stepName))
                    stepName = $"(ขั้นตอน {i + 1})";

                parts.Add(string.IsNullOrWhiteSpace(action) ? stepName : $"{stepName} [{action}]");

                if (nodeType.IndexOf("Start", StringComparison.OrdinalIgnoreCase) >= 0)
                    starts.Add(stepName);
                if (nodeType.IndexOf("End", StringComparison.OrdinalIgnoreCase) >= 0)
                    ends.Add(stepName);
            }

            var startText = starts.Count > 0 ? string.Join(", ", starts.Distinct(StringComparer.OrdinalIgnoreCase)) : "-";
            var endText = ends.Count > 0 ? string.Join(", ", ends.Distinct(StringComparer.OrdinalIgnoreCase)) : "-";
            var flowText = string.Join(" -> ", parts);

            rtbPreview.Text = "Start: " + startText + Environment.NewLine
                + "End: " + endText + Environment.NewLine
                + "Flow: " + flowText;
        }
    }
}
