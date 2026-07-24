using CMS.Domain.Models;
using RG_Project.Project.Services;
using RG_Project.Login;
using System;
using System.Windows.Forms;

namespace RG_Project.Project.Forms
{
    public partial class Frm_Project_Progress_Update : Form
    {
        private readonly int    _i_proj_state_id;
        private readonly int    _i_project_id;
        private readonly string _n_state_name;
        private readonly ProjectProgressService _service;
        private readonly string _userName;
        private bool _isSyncing = false;

        public Frm_Project_Progress_Update(int i_proj_state_id, int i_project_id,
            string n_state_name, int current_pct)
        {
            InitializeComponent();
            _i_proj_state_id = i_proj_state_id;
            _i_project_id    = i_project_id;
            _n_state_name    = n_state_name;
            _service         = new ProjectProgressService();
            _userName        = LoginManager.Instance.GetUserName;

            lblStateName.Text  = n_state_name;
            nudPct.Value       = current_pct;
            SyncStatusFromPct(current_pct);
        }

        private void nudPct_ValueChanged(object sender, EventArgs e)
        {
            if (_isSyncing) return;
            SyncStatusFromPct((int)nudPct.Value);
        }

        private void cbbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isSyncing) return;
            _isSyncing = true;
            try
            {
                switch (cbbStatus.SelectedItem?.ToString())
                {
                    case "Pending":    nudPct.Value = 0;   break;
                    case "Completed":  nudPct.Value = 100; break;
                    // InProgress: keep current pct
                }
            }
            finally { _isSyncing = false; }
        }

        private void SyncStatusFromPct(int pct)
        {
            _isSyncing = true;
            try
            {
                if (pct == 0)         cbbStatus.SelectedItem = "Pending";
                else if (pct == 100)  cbbStatus.SelectedItem = "Completed";
                else                  cbbStatus.SelectedItem = "InProgress";
            }
            finally { _isSyncing = false; }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var model = new ProjectProgressModel
            {
                i_proj_state_id = _i_proj_state_id,
                i_project_id    = _i_project_id,
                i_pct_complete  = (int)nudPct.Value,
                n_remark        = txtRemark.Text.Trim(),
                i_action_user   = _userName
            };

            string errorMsg;
            if (_service.UpdateStateProgress(model, out errorMsg))
            {
                MessageBox.Show("บันทึกความคืบหน้าเรียบร้อย", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("บันทึกไม่สำเร็จ: " + errorMsg, "ข้อผิดพลาด",
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
