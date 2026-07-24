using CMS.Controllers.RGP;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.RGP
{
    public partial class Frm_Select_StateMember : Form
    {
        private readonly int    _projectId;
        private readonly string _userName;
        private readonly ProjectMemberController _ctrl = new ProjectMemberController();
        private readonly System.Collections.Generic.HashSet<string> _excludedNames;

        public string SelectedName { get; private set; }
        public string SelectedRole { get; private set; }

        public Frm_Select_StateMember(int projectId, string userName,
            System.Collections.Generic.IEnumerable<string> excludedNames = null)
        {
            InitializeComponent();
            _projectId     = projectId;
            _userName      = userName ?? "";
            _excludedNames = excludedNames != null
                ? new System.Collections.Generic.HashSet<string>(excludedNames, StringComparer.OrdinalIgnoreCase)
                : new System.Collections.Generic.HashSet<string>();
        }

        private void Frm_Select_StateMember_Load(object sender, EventArgs e)
        {
            LoadMembers();
        }

        private void LoadMembers()
        {
            DataTable all = _ctrl.GetByProject(_projectId);

            // filter บุคคลที่เลือกไปแล้ว
            if (_excludedNames.Count > 0)
            {
                var filtered = all.AsEnumerable()
                    .Where(r => !_excludedNames.Contains(r["n_member_name"]?.ToString() ?? ""))
                    .ToList();
                all = filtered.Count > 0
                    ? filtered.CopyToDataTable()
                    : all.Clone();
            }

            dgvMembers.DataSource = all;

            if (all.Rows.Count == 0)
            {
                lblHint.Text      = _excludedNames.Count > 0
                    ? "บุคคลทุกคนในโครงการถูกเลือกไปแล้ว"
                    : "ยังไม่มีบุคคลในโครงการนี้ กรุณาเพิ่มบุคคลในแท็บ 'บุคคลที่เกี่ยวข้อง' ก่อน";
                lblHint.ForeColor = Color.FromArgb(196, 53, 53);
            }
            else
            {
                lblHint.Text      = "เลือกบุคคลจากรายชื่อในโครงการ แล้วกด ตกลง";
                lblHint.ForeColor = Color.FromArgb(42, 57, 80);
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (dgvMembers.CurrentRow == null) return;
            DataRowView row = dgvMembers.CurrentRow.DataBoundItem as DataRowView;
            if (row == null) return;
            SelectedName      = row["n_member_name"]?.ToString() ?? "";
            SelectedRole      = row["c_role"]?.ToString() ?? "";
            this.DialogResult = DialogResult.OK;
        }

        private void dgvMembers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) BtnOk_Click(sender, e);
        }

        private void btnAddMember_Click(object sender, EventArgs e)
        {
            using (var frm = new Frm_Project_Member(_projectId, _userName))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    LoadMembers(); // refresh รายการหลังเพิ่มสำเร็จ
            }
        }
    }
}
