using CMS.Controllers.RGP;
using CMS.Domain.Models;
using System;
using System.Data;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.RGP
{
    public partial class Frm_Project_Member : Form
    {
        private readonly int    _projectId;
        private readonly string _userName;
        private readonly bool   _returnOnly;
        private readonly ProjectMemberController     _ctrl     = new ProjectMemberController();
        private readonly ProjectMemberRoleController _roleCtrl = new ProjectMemberRoleController();

        public string MemberName     { get; private set; }
        public string MemberRole     { get; private set; }
        public string MemberType     { get; private set; }
        public string MemberDeptCode { get; private set; }

        // mode ปกติ — insert project member
        public Frm_Project_Member(int projectId, string userName)
        {
            _projectId  = projectId;
            _userName   = userName;
            _returnOnly = false;
            InitializeComponent();
            LoadTypeCombo();
            LoadRoles();
        }

        // mode state member — แค่ return ชื่อ+role ไม่ insert เอง
        public Frm_Project_Member(int stateId, string userName, string[] roles, bool returnOnly)
        {
            _projectId  = stateId;
            _userName   = userName;
            _returnOnly = returnOnly;
            InitializeComponent();
            LoadTypeCombo();
            cbbRole.DataSource    = null;
            cbbRole.Items.Clear();
            cbbRole.Items.AddRange(roles);
            if (cbbRole.Items.Count > 0) cbbRole.SelectedIndex = 0;
        }

        private void LoadTypeCombo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("code", typeof(string));
            dt.Columns.Add("name", typeof(string));
            dt.Rows.Add("0", "=== ประเภทให้เลือก ===");
            dt.Rows.Add("1", "คณะกรรมการ");
            dt.Rows.Add("2", "ฝ่าย");
            dt.Rows.Add("3", "แผนก");
            dt.Rows.Add("4", "พนักงาน");
            cbbType.DataSource    = dt;
            cbbType.DisplayMember = "name";
            cbbType.ValueMember   = "code";
            cbbType.SelectedIndex = 0;
        }

        private void LoadRoles()
        {
            DataTable dt = _roleCtrl.GetActive();
            cbbRole.DataSource    = dt;
            cbbRole.DisplayMember = "n_role_name";
            cbbRole.ValueMember   = "i_role_id";
            if (cbbRole.Items.Count > 0) cbbRole.SelectedIndex = 0;
        }

        private void cbbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string code = cbbType.SelectedValue?.ToString() ?? "0";

            cbbName.DataSource = null;
            cbbName.Items.Clear();
            txtName.Clear();

            switch (code)
            {
                case "1": LoadNameCombo(_roleCtrl.GetSubCommittees(),  "SubCommitteeNameTH" , "SubCommitteeID"); break;
                case "2": LoadNameCombo(_roleCtrl.GetDivisions(),  "DivisionName" , "DivisionID");      break;
                case "3": LoadNameCombo(_roleCtrl.GetDepartments(), "DepartmentName", "DepartmentID");    break;
                case "4": // พนักงาน — พิมพ์เอง
                    cbbName.Visible = false;
                    txtName.Visible = true;
                    txtName.Focus();
                    return;
                default:
                    cbbName.Visible = false;
                    txtName.Visible = false;
                    return;
            }

            cbbName.Visible = true;
            txtName.Visible = false;
        }

        private void LoadNameCombo(DataTable dt, string displayMember, string valueMember)
        {
            try
            {
                cbbName.DataSource    = dt;
                cbbName.DisplayMember = displayMember;
                cbbName.ValueMember   = valueMember;
                if (cbbName.Items.Count > 0) cbbName.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnManageRole_Click(object sender, EventArgs e)
        {
            using (var frm = new Frm_Project_Role())
                frm.ShowDialog();
            LoadRoles();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            string typeCode = cbbType.SelectedValue?.ToString() ?? "0";

            if (typeCode == "0")
            {
                MessageBox.Show("กรุณาเลือกประเภท", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbbType.Focus();
                return;
            }

            string memberName = typeCode == "4" ? txtName.Text.Trim() : cbbName.Text.Trim();
            if (string.IsNullOrWhiteSpace(memberName))
            {
                MessageBox.Show("กรุณาระบุชื่อ", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                (typeCode == "4" ? (Control)txtName : cbbName).Focus();
                return;
            }

            string typeName = ((DataRowView)cbbType.SelectedItem)?["name"]?.ToString() ?? "";

            // ดึง DepartmentCode / DivisionCode จาก selected row
            string deptCode = null;
            if (typeCode == "3" && cbbName.SelectedItem is DataRowView drv3)
                deptCode = drv3.Row.Table.Columns.Contains("DepartmentCode") ? drv3.Row["DepartmentCode"]?.ToString() : null;
            else if (typeCode == "2" && cbbName.SelectedItem is DataRowView drv2)
                deptCode = drv2.Row.Table.Columns.Contains("DivisionCode") ? drv2.Row["DivisionCode"]?.ToString() : null;

            if (_returnOnly)
            {
                MemberType     = typeName;
                MemberName     = memberName;
                MemberRole     = cbbRole.Text;
                MemberDeptCode = deptCode;
                this.DialogResult = DialogResult.OK;
                return;
            }

            var m = new ProjectMemberModel
            {
                i_project_id     = _projectId,
                n_member_name    = memberName,
                c_role           = cbbRole.Text,
                c_department_code = deptCode,
                i_create_user    = _userName
            };

            if (_ctrl.Insert(m))
                this.DialogResult = DialogResult.OK;
            else
                MessageBox.Show("บันทึกไม่สำเร็จ", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
