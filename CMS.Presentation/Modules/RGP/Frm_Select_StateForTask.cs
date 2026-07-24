using CMS.Controllers.RGP;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.RGP
{
    /// <summary>Dialog เลือก State เมื่อเปิด Task จาก Project-level view</summary>
    public class Frm_Select_StateForTask : Form
    {
        private readonly int                 _projectId;
        private readonly ProjectStateController _stateSvc;

        public int    SelectedStateId   { get; private set; }
        public string SelectedStateName { get; private set; }

        private DataGridView dgv;
        private Button btnOk, btnCancel;

        public Frm_Select_StateForTask(int projectId, ProjectStateController stateSvc)
        {
            _projectId = projectId;
            _stateSvc  = stateSvc;
            BuildUI();
            LoadStates();
        }

        private void BuildUI()
        {
            this.Text            = "เลือกขั้นตอน";
            this.ClientSize      = new Size(460, 360);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Font            = new Font("Segoe UI", 9F);
            this.BackColor       = Color.FromArgb(242, 246, 252);

            dgv = new DataGridView
            {
                Location = new Point(8, 8), Size = new Size(440, 290),
                AutoGenerateColumns = false, ReadOnly = true, AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                RowHeadersVisible = false, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowTemplate = { Height = 28 }
            };
            var colId   = new DataGridViewTextBoxColumn { DataPropertyName = "i_proj_state_id", Visible = false };
            var colSeq  = new DataGridViewTextBoxColumn { HeaderText = "#",         DataPropertyName = "i_seq_no",    Width = 40 };
            var colName = new DataGridViewTextBoxColumn { HeaderText = "ขั้นตอน",  DataPropertyName = "n_state_name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            var colStat = new DataGridViewTextBoxColumn { HeaderText = "สถานะ",    DataPropertyName = "c_status",    Width = 100 };
            dgv.Columns.AddRange(colId, colSeq, colName, colStat);
            var h = dgv.ColumnHeadersDefaultCellStyle;
            h.BackColor = Color.FromArgb(30, 58, 95); h.ForeColor = Color.White;
            h.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 28;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.EnableHeadersVisualStyles = false;
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Accept(); };

            btnOk = new Button
            {
                Text = "เลือก", Location = new Point(286, 314), Size = new Size(80, 30),
                BackColor = Color.FromArgb(43, 87, 154), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) => Accept();

            btnCancel = new Button
            {
                Text = "ยกเลิก", Location = new Point(374, 314), Size = new Size(80, 30),
                BackColor = Color.FromArgb(100, 130, 168), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { dgv, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void LoadStates()
        {
            dgv.DataSource = _stateSvc.GetStatesForCombo(_projectId);
        }

        private void Accept()
        {
            if (dgv.CurrentRow == null) return;
            DataRowView row = dgv.CurrentRow.DataBoundItem as DataRowView;
            if (row == null) return;
            SelectedStateId   = Convert.ToInt32(row["i_proj_state_id"]);
            SelectedStateName = row["n_state_name"].ToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
