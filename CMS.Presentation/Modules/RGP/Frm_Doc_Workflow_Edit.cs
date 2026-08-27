using CMS.Domain.Models;
using CMS.Controllers.RGP;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.RGP
{
    public partial class Frm_Doc_Workflow_Edit : Form
    {
        private readonly int                          _i_project_id;
        private readonly ProjectDocumentWorkflowModel _editModel;
        private readonly ProjectDocumentController       _service;
        private readonly ProjectStateController          _stateService;
        private readonly string                       _userName;
        // เมื่อ _i_project_id == 0 (โครงการยังไม่ถูกบันทึก) ใช้ list ขั้นตอนที่ staged ไว้ใน parent แทนการ query DB
        private readonly List<ProjectStateModel>      _stagedStates;

        public ProjectDocumentWorkflowModel Result { get; private set; }

        public Frm_Doc_Workflow_Edit(int i_project_id, ProjectDocumentWorkflowModel model = null,
            List<ProjectStateModel> stagedStates = null)
        {
            InitializeComponent();
            _i_project_id = i_project_id;
            _editModel    = model;
            _service      = new ProjectDocumentController();
            _stateService = new ProjectStateController();
            _userName     = RgpSession.Current?.ADUserName;
            _stagedStates = stagedStates;
        }

        private void Frm_Doc_Workflow_Edit_Load(object sender, EventArgs e)
        {
            LoadStatesCombo();

            if (_editModel != null)
            {
                this.Text     = "แก้ไขเอกสาร";
                lblTitle.Text = "แก้ไขเอกสาร";
                SelectComboById(cbbState, "i_proj_state_id", _editModel.i_proj_state_id);
                txtDocName.Text         = _editModel.n_doc_name ?? "";
                txtDocType.Text         = _editModel.c_doc_type ?? "";
                chkRequired.Checked     = (_editModel.f_required ?? "Y") == "Y";
                cbbDocStatus.SelectedItem = _editModel.c_doc_status ?? "Pending";
                txtRemark.Text          = _editModel.n_remark ?? "";
                nudSeqNo.Value          = _editModel.i_seq_no > 0 ? _editModel.i_seq_no : 1;
            }
            else
            {
                this.Text     = "เพิ่มเอกสาร";
                lblTitle.Text = "เพิ่มเอกสาร";
                cbbDocStatus.SelectedItem = "Pending";
            }
        }

        private void LoadStatesCombo()
        {
            DataTable dt;
            if (_i_project_id == 0)
            {
                // โครงการยังไม่ถูกบันทึก — ใช้ขั้นตอนที่ staged ไว้แทนการ query DB
                dt = new DataTable();
                dt.Columns.Add("i_proj_state_id", typeof(int));
                dt.Columns.Add("n_state_name", typeof(string));
                if (_stagedStates != null)
                    foreach (var s in _stagedStates)
                        dt.Rows.Add(s.i_proj_state_id, s.n_state_name);
            }
            else
            {
                dt = _stateService.GetStatesForCombo(_i_project_id);
            }
            cbbState.DisplayMember = "n_state_name";
            cbbState.ValueMember   = "i_proj_state_id";
            cbbState.DataSource    = dt;
        }

        private void SelectComboById(ComboBox cbb, string valueMember, int id)
        {
            DataTable dt = cbb.DataSource as DataTable;
            if (dt == null) return;
            foreach (DataRowView row in dt.DefaultView)
            {
                if (Convert.ToInt32(row[valueMember]) == id)
                {
                    cbb.SelectedItem = row;
                    break;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDocName.Text))
            {
                MessageBox.Show("กรุณาระบุชื่อเอกสาร", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDocName.Focus();
                return;
            }
            if (cbbState.SelectedValue == null)
            {
                MessageBox.Show("กรุณาเลือกขั้นตอน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var model = new ProjectDocumentWorkflowModel
            {
                i_project_id    = _i_project_id,
                i_proj_state_id = Convert.ToInt32(cbbState.SelectedValue),
                n_doc_name      = txtDocName.Text.Trim(),
                c_doc_type      = txtDocType.Text.Trim(),
                f_required      = chkRequired.Checked ? "Y" : "N",
                c_doc_status    = cbbDocStatus.SelectedItem?.ToString() ?? "Pending",
                n_remark        = txtRemark.Text.Trim(),
                i_seq_no        = (int)nudSeqNo.Value,
                i_create_user   = _userName,
                i_update_user   = _userName
            };

            bool success;
            if (_i_project_id == 0)
            {
                // โครงการยังไม่ถูกบันทึก — เก็บไว้ใน memory เฉยๆ ไม่ยิง DB
                if (_editModel != null) model.i_doc_wf_id = _editModel.i_doc_wf_id;
                success = true;
            }
            else if (_editModel != null)
            {
                model.i_doc_wf_id = _editModel.i_doc_wf_id;
                success = _service.UpdateDocWorkflow(model);
            }
            else
            {
                int newId = _service.AddDocWorkflow(model);
                success = newId > 0;
                if (success) model.i_doc_wf_id = newId;
            }

            if (success)
            {
                Result = model;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("บันทึกข้อมูลไม่สำเร็จ", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
