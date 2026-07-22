using RG_Project.Project.Controllers;
using RG_Project.Login;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace RG_Project.Project.Forms
{
    public partial class Frm_Project_Role : Form
    {
        private readonly ProjectMemberRoleController _ctrl = new ProjectMemberRoleController();
        private readonly string _userName;

        public Frm_Project_Role()
        {
            InitializeComponent();
            _userName = LoginManager.Instance.GetUserName;
        }

        private void Frm_Project_Role_Load(object sender, EventArgs e)
        {
            LoadRoles();
        }

        private void LoadRoles()
        {
            DataTable dt = _ctrl.GetAll();
            SetupDgv();
            dgvRoles.DataSource = dt;
            AlternateRows();
        }

        private void SetupDgv()
        {
            dgvRoles.DataSource           = null;
            dgvRoles.AutoGenerateColumns  = false;
            dgvRoles.Columns.Clear();

            var colId     = new DataGridViewTextBoxColumn { HeaderText = "ID",           DataPropertyName = "i_role_id",     Width = 50  };
            var colName   = new DataGridViewTextBoxColumn { HeaderText = "ชื่อ Role",     DataPropertyName = "n_role_name", Width = 120 };
            var colActive = new DataGridViewTextBoxColumn { HeaderText = "ใช้งาน",       DataPropertyName = "f_active",      Width =50  };
            var colCrBy   = new DataGridViewTextBoxColumn { HeaderText = "สร้างโดย",     DataPropertyName = "i_create_user", Width = 120 };
            var colCrDt   = new DataGridViewTextBoxColumn { HeaderText = "วันที่สร้าง",  DataPropertyName = "d_create_date", Width = 135 };
            var colUpBy   = new DataGridViewTextBoxColumn { HeaderText = "แก้ไขโดย",     DataPropertyName = "i_update_user", Width = 120 };
            var colUpDt   = new DataGridViewTextBoxColumn { HeaderText = "วันที่แก้ไข",  DataPropertyName = "d_update_date", Width = 135 };

            colId.DefaultCellStyle.Alignment     = DataGridViewContentAlignment.MiddleCenter;
            colActive.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colCrDt.DefaultCellStyle.Format      = "dd/MM/yyyy HH:mm";
            colUpDt.DefaultCellStyle.Format      = "dd/MM/yyyy HH:mm";

            dgvRoles.Columns.AddRange(colId, colName, colActive, colCrBy, colCrDt, colUpBy, colUpDt);

            // ── header style ──────────────────────────────────────────────────
            var hStyle = dgvRoles.ColumnHeadersDefaultCellStyle;
            hStyle.BackColor = Color.FromArgb(30, 58, 95);
            hStyle.ForeColor = Color.White;
            hStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            hStyle.Padding   = new Padding(4, 0, 0, 0);
            dgvRoles.ColumnHeadersHeight         = 30;
            dgvRoles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvRoles.EnableHeadersVisualStyles   = false;

            // ── cell style ────────────────────────────────────────────────────
            dgvRoles.DefaultCellStyle.Font               = new Font("Segoe UI", 9F);
            dgvRoles.DefaultCellStyle.ForeColor          = Color.FromArgb(42, 57, 80);
            dgvRoles.DefaultCellStyle.BackColor          = Color.White;
            dgvRoles.DefaultCellStyle.SelectionBackColor = Color.FromArgb(190, 215, 255);
            dgvRoles.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 40, 80);
            dgvRoles.DefaultCellStyle.Padding            = new Padding(4, 0, 0, 0);

            dgvRoles.GridColor         = Color.FromArgb(208, 220, 240);
            dgvRoles.CellBorderStyle   = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvRoles.BackgroundColor   = Color.White;
            dgvRoles.BorderStyle       = BorderStyle.None;
            dgvRoles.RowHeadersVisible = false;
            dgvRoles.RowTemplate.Height = 28;
            dgvRoles.SelectionMode     = DataGridViewSelectionMode.FullRowSelect;
            dgvRoles.AllowUserToAddRows = false;
            dgvRoles.AllowUserToDeleteRows = false;
            dgvRoles.MultiSelect       = false;
            // ReadOnly ปิดไว้เพื่อไม่ให้กระทบกับ DataSource binding
        }

        private void AlternateRows()
        {
            dgvRoles.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 244, 255);
            dgvRoles.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(42, 57, 80);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtRoleName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("กรุณาระบุชื่อ Role", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoleName.Focus();
                return;
            }
            if (_ctrl.Insert(name, _userName))
            {
                txtRoleName.Text = "";
                LoadRoles();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvRoles.CurrentRow == null) return;
            DataTable dt = (DataTable)dgvRoles.DataSource;
            DataRow row  = dt.Rows[dgvRoles.CurrentRow.Index];

            int    id     = Convert.ToInt32(row["i_role_id"]);
            string name   = row["n_role_name"].ToString();
            string active = row["f_active"].ToString();

            using (var dlg = new Frm_Project_Role_Edit(id, name, active, _userName))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    LoadRoles();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvRoles.CurrentRow == null) return;
            DataTable dt  = (DataTable)dgvRoles.DataSource;
            DataRow row   = dt.Rows[dgvRoles.CurrentRow.Index];
            int    id     = Convert.ToInt32(row["i_role_id"]);
            string name   = row["n_role_name"].ToString();

            if (MessageBox.Show($"ต้องการลบ Role '{name}'?", "ยืนยัน",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_ctrl.Delete(id))
                    LoadRoles();
                else
                    MessageBox.Show("ลบไม่สำเร็จ อาจมีข้อมูลอ้างอิงอยู่", "แจ้งเตือน",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
