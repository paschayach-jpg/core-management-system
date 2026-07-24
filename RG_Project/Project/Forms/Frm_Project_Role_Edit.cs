using RG_Project.Project.Controllers;
using System;
using System.Windows.Forms;

namespace RG_Project.Project.Forms
{
    public partial class Frm_Project_Role_Edit : Form
    {
        private readonly int    _roleId;
        private readonly string _userName;
        private readonly ProjectMemberRoleController _ctrl = new ProjectMemberRoleController();

        public Frm_Project_Role_Edit(int roleId, string roleName, string fActive, string userName)
        {
            _roleId   = roleId;
            _userName = userName;
            InitializeComponent();
            txtName.Text      = roleName;
            chkActive.Checked = fActive == "Y";
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("กรุณาระบุชื่อ Role", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }
            string active = chkActive.Checked ? "Y" : "N";
            if (_ctrl.Update(_roleId, txtName.Text, active, _userName))
                this.DialogResult = DialogResult.OK;
            else
                MessageBox.Show("บันทึกไม่สำเร็จ", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
