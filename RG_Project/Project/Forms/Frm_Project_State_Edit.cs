using RG_Project.Project.Models;
using RG_Project.Project.Services;
using RG_Project.Project.Controllers;
using RG_Project.Login;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace RG_Project.Project.Forms
{
    public partial class Frm_Project_State_Edit : Form
    {
        private readonly int                _i_project_id;
        private readonly ProjectStateModel  _editModel;
        private readonly ProjectStateService _service;
        private readonly ProjectStateMemberController _memberCtrl = new ProjectStateMemberController();
        private readonly string             _userName;
        private readonly DateTime?          _projectStart;
        private readonly DateTime?          _projectDue;

        // ref project ที่เลือกอยู่
        private int?   _refProjectId   = null;
        private string _refProjectNo   = null;
        private string _refProjectName = null;

        public ProjectStateModel Result { get; private set; }

        public Frm_Project_State_Edit(int i_project_id, ProjectStateModel model = null,
            DateTime? projectStart = null, DateTime? projectDue = null)
        {
            InitializeComponent();
            _i_project_id = i_project_id;
            _editModel    = model;
            _service      = new ProjectStateService();
            _userName     = LoginManager.Instance.GetUserName;
            _projectStart = projectStart;
            _projectDue   = projectDue;
        }

        private void Frm_Project_State_Edit_Load(object sender, EventArgs e)
        {
            // คำนวณวันรวมของโครงการ แล้วตั้งเป็นค่า default
            if (_projectStart.HasValue && _projectDue.HasValue && _projectDue > _projectStart)
            {
                int totalDays = (_projectDue.Value - _projectStart.Value).Days;
                nudEstDays.Maximum = totalDays;
                lblEstDays.Text    = $"จำนวนวัน (คาดการณ์) [รวม {totalDays} วัน]:";
            }

            if (_editModel != null)
            {
                this.Text           = "แก้ไขขั้นตอน";
                lblTitle.Text       = "แก้ไขขั้นตอน";
                txtStateName.Text   = _editModel.n_state_name;
                nudSeqNo.Value      = _editModel.i_seq_no;
                txtDescription.Text = _editModel.c_description ?? "";
                chkRequired.Checked = (_editModel.f_required ?? "Y") == "Y";
                nudEstDays.Value    = _editModel.i_estimated_days.HasValue
                    ? Math.Min(_editModel.i_estimated_days.Value, (int)nudEstDays.Maximum)
                    : 0;

                // โหลด ref project ที่บันทึกไว้
                if (_editModel.i_ref_project_id.HasValue)
                {
                    _refProjectId   = _editModel.i_ref_project_id;
                    _refProjectNo   = _editModel.c_ref_project_no;
                    _refProjectName = _editModel.n_ref_project_name;
                }
                RefreshRefProjectDisplay();

                LoadStateMembers();
                pnlMembers.Visible = true;
            }
            else
            {
                this.Text     = "เพิ่มขั้นตอน";
                lblTitle.Text = "เพิ่มขั้นตอน";
                pnlMembers.Visible = false;
                RefreshRefProjectDisplay();
            }
        }

        // ─── Ref Project UI ───────────────────────────────────────────────────

        private void RefreshRefProjectDisplay()
        {
            if (_refProjectId.HasValue)
            {
                txtRefProject.Text      = $"[{_refProjectNo}] {_refProjectName}";
                txtRefProject.ForeColor = Color.FromArgb(30, 58, 95);
                btnClearRef.Visible     = true;
            }
            else
            {
                txtRefProject.Text      = "(ไม่ได้ระบุ)";
                txtRefProject.ForeColor = Color.Gray;
                btnClearRef.Visible     = false;
            }
        }

        private void btnSelectRef_Click(object sender, EventArgs e)
        {
            using (var dlg = new Frm_Select_RefProject(_i_project_id, _service))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _refProjectId   = dlg.SelectedProjectId;
                    _refProjectNo   = dlg.SelectedProjectNo;
                    _refProjectName = dlg.SelectedProjectName;
                    RefreshRefProjectDisplay();
                }
            }
        }

        private void btnClearRef_Click(object sender, EventArgs e)
        {
            _refProjectId   = null;
            _refProjectNo   = null;
            _refProjectName = null;
            RefreshRefProjectDisplay();
        }

        private void btnNewRefProject_Click(object sender, EventArgs e)
        {
            var frm = new Frm_Project_Detail(0);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                // หลังสร้างโครงการใหม่ เปิด dialog เลือกอีกครั้งเพื่อให้ user เลือกโครงการที่เพิ่งสร้าง
                using (var dlg = new Frm_Select_RefProject(_i_project_id, _service))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        _refProjectId   = dlg.SelectedProjectId;
                        _refProjectNo   = dlg.SelectedProjectNo;
                        _refProjectName = dlg.SelectedProjectName;
                        RefreshRefProjectDisplay();
                    }
                }
            }
        }

        // ─── Save ─────────────────────────────────────────────────────────────

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStateName.Text))
            {
                MessageBox.Show("กรุณาระบุชื่อขั้นตอน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStateName.Focus();
                return;
            }

            var model = new ProjectStateModel
            {
                i_project_id     = _i_project_id,
                n_state_name     = txtStateName.Text.Trim(),
                i_seq_no         = (int)nudSeqNo.Value,
                c_description    = txtDescription.Text.Trim(),
                f_required       = chkRequired.Checked ? "Y" : "N",
                i_estimated_days = nudEstDays.Value > 0 ? (int?)nudEstDays.Value : null,
                i_ref_project_id = _refProjectId,
                i_create_user    = _userName,
                i_update_user    = _userName
            };

            // ตรวจสอบผลรวมวันไม่เกินช่วงโครงการ
            if (_projectStart.HasValue && _projectDue.HasValue && _projectDue > _projectStart
                && nudEstDays.Value > 0)
            {
                int totalDays    = (_projectDue.Value - _projectStart.Value).Days;
                int excludeId    = _editModel?.i_proj_state_id ?? 0;
                int usedDays     = _service.GetTotalEstimatedDays(_i_project_id, excludeId);
                int remaining    = totalDays - usedDays;

                if ((int)nudEstDays.Value > remaining)
                {
                    MessageBox.Show(
                        $"จำนวนวันที่กำหนด ({nudEstDays.Value} วัน) เกินวันที่เหลือ ({remaining} วัน)\n" +
                        $"รวมทั้งหมด: {totalDays} วัน  ใช้ไปแล้ว: {usedDays} วัน",
                        "เกินจำนวนวันโครงการ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    nudEstDays.Focus();
                    return;
                }
            }

            bool success;
            if (_editModel != null)
            {
                model.i_proj_state_id = _editModel.i_proj_state_id;
                success = _service.UpdateState(model);
            }
            else
            {
                int newId = _service.AddState(model);
                success = newId > 0;
                if (success) model.i_proj_state_id = newId;
            }

            if (success)
            {
                Result = model;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("บันทึกข้อมูลไม่สำเร็จ กรุณาลองใหม่อีกครั้ง", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnViewTasks_Click(object sender, EventArgs e)
        {
            // ถ้าเป็น Add mode (ยังไม่มี State) → ไม่สามารถดู Task ได้
            if (_editModel == null)
            {
                MessageBox.Show("กรุณาบันทึกขั้นตอนก่อนเพิ่ม Task", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var frm = new Frm_Project_Task_List(
                _i_project_id,
                _editModel.i_proj_state_id,
                _editModel.n_state_name);
            frm.ShowDialog();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // ─── State Members ────────────────────────────────────────────────────

        private void LoadStateMembers()
        {
            if (_editModel == null) return;
            dgvStateMembers.DataSource = _memberCtrl.GetByState(_editModel.i_proj_state_id);
            SetupDgvMembers();
        }

        private void SetupDgvMembers()
        {
            dgvStateMembers.Columns.Clear();
            dgvStateMembers.AutoGenerateColumns = false;

            var colId   = new DataGridViewTextBoxColumn { Name = "i_state_member_id", DataPropertyName = "i_state_member_id", Visible = false };
            var colName = new DataGridViewTextBoxColumn { HeaderText = "ชื่อ",    DataPropertyName = "n_member_name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            var colRole = new DataGridViewTextBoxColumn { HeaderText = "บทบาท",   DataPropertyName = "c_role",        Width = 150 };
            var colBy   = new DataGridViewTextBoxColumn { HeaderText = "เพิ่มโดย", DataPropertyName = "i_create_user", Width = 100 };

            colRole.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvStateMembers.Columns.AddRange(colId, colName, colRole, colBy);

            var hStyle = dgvStateMembers.ColumnHeadersDefaultCellStyle;
            hStyle.BackColor = Color.FromArgb(30, 58, 95);
            hStyle.ForeColor = Color.White;
            hStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvStateMembers.ColumnHeadersHeight         = 28;
            dgvStateMembers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvStateMembers.EnableHeadersVisualStyles   = false;
            dgvStateMembers.DefaultCellStyle.Font       = new Font("Segoe UI", 9F);
            dgvStateMembers.RowTemplate.Height          = 26;
            dgvStateMembers.SelectionMode               = DataGridViewSelectionMode.FullRowSelect;
            dgvStateMembers.ReadOnly                    = true;
            dgvStateMembers.AllowUserToAddRows          = false;
            dgvStateMembers.RowHeadersVisible           = false;
        }

        private void btnAddStateMember_Click(object sender, EventArgs e)
        {
            if (_editModel == null) return;

            // รวบรวมชื่อที่เลือกไปแล้ว
            var alreadySelected = new System.Collections.Generic.List<string>();
            if (dgvStateMembers.DataSource is DataTable dt)
                foreach (DataRow r in dt.Rows)
                    alreadySelected.Add(r["n_member_name"]?.ToString() ?? "");

            using (var dlg = new Frm_Select_StateMember(_i_project_id, _userName, alreadySelected))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var m = new ProjectStateMemberModel
                    {
                        i_proj_state_id = _editModel.i_proj_state_id,
                        i_project_id    = _i_project_id,
                        n_member_name   = dlg.SelectedName,
                        c_role          = dlg.SelectedRole,
                        i_create_user   = _userName
                    };
                    if (_memberCtrl.Insert(m))
                        LoadStateMembers();
                    else
                        MessageBox.Show("บันทึกไม่สำเร็จ", "ข้อผิดพลาด",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDeleteStateMember_Click(object sender, EventArgs e)
        {
            if (dgvStateMembers.CurrentRow == null) return;
            DataRowView row = dgvStateMembers.CurrentRow.DataBoundItem as DataRowView;
            if (row == null) return;
            int    id = Convert.ToInt32(row["i_state_member_id"]);
            string n  = row["n_member_name"]?.ToString() ?? "";

            if (MessageBox.Show($"ต้องการลบ '{n}'?", "ยืนยัน",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_memberCtrl.Delete(id))
                    LoadStateMembers();
            }
        }
    }
}
