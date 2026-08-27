using CMS.Domain.Models;
using CMS.Controllers.RGP;
using CMS.Presentation.Services;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.RGP
{
    public partial class Frm_Project_State_Edit : Form
    {
        private readonly int                _i_project_id;
        private readonly ProjectStateModel  _editModel;
        private readonly ProjectStateController _service;
        private readonly ProjectStateMemberController _memberCtrl = new ProjectStateMemberController();
        private readonly ProjectNotifyLogController _notifyLogCtrl = new ProjectNotifyLogController();
        private readonly string             _userName;
        private readonly DateTime?          _projectStart;
        private readonly DateTime?          _projectDue;
        // เมื่อ _i_project_id == 0 (โครงการยังไม่ถูกบันทึก) ใช้ callback นี้แทนการ query DB
        // เพื่อรวมวันคาดการณ์ของขั้นตอนอื่นที่ staged ไว้ใน parent (รับ excludeStateId คืนผลรวมวัน)
        private readonly Func<int, int>     _stagedUsedDaysProvider;

        // ref project ที่เลือกอยู่
        private int?   _refProjectId   = null;
        private string _refProjectNo   = null;
        private string _refProjectName = null;

        public ProjectStateModel Result { get; private set; }

        public Frm_Project_State_Edit(int i_project_id, ProjectStateModel model = null,
            DateTime? projectStart = null, DateTime? projectDue = null,
            Func<int, int> stagedUsedDaysProvider = null)
        {
            InitializeComponent();
            _i_project_id = i_project_id;
            _editModel    = model;
            _service      = new ProjectStateController();
            _userName     = RgpSession.Current?.ADUserName;
            _projectStart = projectStart;
            _projectDue   = projectDue;
            _stagedUsedDaysProvider = stagedUsedDaysProvider;
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

                // สมาชิกของขั้นตอนต้องมีขั้นตอนจริงใน DB ก่อน — ถ้าโครงการยังไม่บันทึก (staging) ยังใช้ไม่ได้
                if (_i_project_id > 0)
                {
                    LoadStateMembers();
                    pnlMembers.Visible = true;
                }
                else
                {
                    pnlMembers.Visible = false;
                }
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
                int usedDays     = _i_project_id > 0
                    ? _service.GetTotalEstimatedDays(_i_project_id, excludeId)
                    : (_stagedUsedDaysProvider?.Invoke(excludeId) ?? 0);
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
            if (_i_project_id == 0)
            {
                // โครงการยังไม่ถูกบันทึก — เก็บไว้ใน memory เฉยๆ ไม่ยิง DB (parent จะเป็นคนกำหนด id ชั่วคราว/จริง)
                if (_editModel != null) model.i_proj_state_id = _editModel.i_proj_state_id;
                success = true;
            }
            else if (_editModel != null)
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
            // ถ้าเป็น Add mode (ยังไม่มี State) หรือโครงการ/ขั้นตอนยัง staging อยู่ → ไม่สามารถดู Task ได้
            if (_editModel == null || _i_project_id == 0)
            {
                MessageBox.Show("กรุณาบันทึกโครงการและขั้นตอนก่อนเพิ่ม Task", "แจ้งเตือน",
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

            var colId    = new DataGridViewTextBoxColumn { Name = "i_state_member_id", DataPropertyName = "i_state_member_id", Visible = false };
            var colName  = new DataGridViewTextBoxColumn { HeaderText = "ชื่อ",       DataPropertyName = "n_member_name",  AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            var colType  = new DataGridViewTextBoxColumn { HeaderText = "ประเภท",     DataPropertyName = "c_member_type",  Width = 90 };
            var colRole  = new DataGridViewTextBoxColumn { HeaderText = "บทบาท",      DataPropertyName = "c_role",         Width = 130 };
            var colSent  = new DataGridViewTextBoxColumn { HeaderText = "ส่งไปแล้ว (ครั้ง)", DataPropertyName = "i_notify_count", Width = 110 };
            var colBy    = new DataGridViewTextBoxColumn { HeaderText = "เพิ่มโดย",    DataPropertyName = "i_create_user",  Width = 100 };

            colType.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colRole.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colSent.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvStateMembers.Columns.AddRange(colId, colName, colType, colRole, colSent, colBy);

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
                        c_member_type   = dlg.SelectedType,
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

        // ─── ส่ง Notify ────────────────────────────────────────────────────────
        private async void btnNotifyMember_Click(object sender, EventArgs e)
        {
            if (dgvStateMembers.CurrentRow == null) return;
            DataRowView row = dgvStateMembers.CurrentRow.DataBoundItem as DataRowView;
            if (row == null) return;

            string memberType = row["c_member_type"]?.ToString() ?? "";
            if (memberType != "พนักงาน")
            {
                MessageBox.Show("ส่ง Notify ได้เฉพาะประเภท \"พนักงาน\" เท่านั้น (ต้องมี AD username)", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int    stateMemberId = Convert.ToInt32(row["i_state_member_id"]);
            string adUserName    = row["n_member_name"]?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(adUserName)) return;

            if (!NotificationManager.Instance.IsConnected)
            {
                MessageBox.Show("ระบบแจ้งเตือนไม่พร้อมใช้งานในขณะนี้ กรุณาลองใหม่อีกครั้ง", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnNotifyMember.Enabled = false;
            try
            {
                // 1) เช็คว่ามี Task ที่สร้างจาก Notify ให้คนนี้/ขั้นตอนนี้มาก่อนหรือยัง
                int? existingTaskId = _notifyLogCtrl.GetExistingTaskId(_editModel.i_proj_state_id, stateMemberId);
                int  taskId;
                bool createdNewTask = false;

                if (existingTaskId.HasValue)
                {
                    taskId = existingTaskId.Value;
                }
                else
                {
                    var taskModel = new ProjectTaskModel
                    {
                        i_proj_state_id = _editModel.i_proj_state_id,
                        i_project_id    = _i_project_id,
                        n_title         = $"ติดตาม - {_editModel.n_state_name}",
                        n_description   = _editModel.c_description,
                        n_assignee      = adUserName,
                        i_create_user   = _userName
                    };
                    taskId = new ProjectTaskController().CreateTask(taskModel);
                    createdNewTask = taskId > 0;
                }

                // 2) ส่ง Notification จริง
                string projectName = "";
                try
                {
                    DataTable projDt = new ProjectController().GetProjectById(_i_project_id);
                    if (projDt.Rows.Count > 0) projectName = projDt.Rows[0]["n_project_name"]?.ToString() ?? "";
                }
                catch { /* ไม่กระทบการส่ง notify ถ้าดึงชื่อโครงการไม่ได้ */ }

                string apiResult = "Success", apiMessage = null;
                try
                {
                    await NotificationManager.Instance.SendNotificationAsync(
                        typeCode: "ASSIGN",
                        title: "มีงานส่งมา",
                        message: $"คุณได้รับงานสำหรับขั้นตอน '{_editModel.n_state_name}' ในโครงการ '{projectName}' กรุณาดำเนินการ",
                        recipientUserAd: adUserName,
                        senderUserAd: _userName,
                        referenceType: "project_state",
                        referenceId: _editModel.i_proj_state_id.ToString(),
                        referenceCode: null,
                        priority: 2);
                }
                catch (Exception exSend)
                {
                    apiResult  = "Failed";
                    apiMessage = exSend.Message;
                }

                // 3) log การส่ง
                _notifyLogCtrl.Insert(new ProjectNotifyLogModel
                {
                    i_project_id      = _i_project_id,
                    i_proj_state_id   = _editModel.i_proj_state_id,
                    i_state_member_id = stateMemberId,
                    n_recipient_name  = adUserName,
                    i_task_id         = taskId > 0 ? (int?)taskId : null,
                    f_task_created    = createdNewTask ? "Y" : "N",
                    c_api_result      = apiResult,
                    n_message         = apiMessage,
                    i_send_user       = _userName
                });

                LoadStateMembers();

                if (apiResult == "Success")
                {
                    MessageBox.Show(createdNewTask
                        ? "ส่ง Notify และสร้าง Task ติดตามให้แล้ว"
                        : "ส่ง Notify แล้ว (มี Task ติดตามอยู่ก่อนแล้ว ไม่ได้สร้างใหม่)",
                        "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("ส่ง Notify ไม่สำเร็จ: " + apiMessage, "ข้อผิดพลาด",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnNotifyMember.Enabled = true;
            }
        }
    }
}
