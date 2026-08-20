using RG_Project.Project.Services;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace RG_Project.Project.Forms
{
    /// <summary>
    /// Dialog สำหรับเลือกโครงการที่ ref (เฉพาะโครงการที่ยังไม่ Completed)
    /// </summary>
    public partial class Frm_Select_RefProject : Form
    {
        private readonly ProjectStateService _service;
        private readonly int                 _excludeProjectId;

        public int?   SelectedProjectId   { get; private set; }
        public string SelectedProjectNo   { get; private set; }
        public string SelectedProjectName { get; private set; }

        private DataTable _source;

        public Frm_Select_RefProject(int excludeProjectId, ProjectStateService service)
        {
            InitializeComponent();
            _excludeProjectId = excludeProjectId;
            _service          = service;
        }

        private void Frm_Select_RefProject_Load(object sender, EventArgs e)
        {
            _source        = _service.GetActiveProjectsForRef(_excludeProjectId);
            dgv.DataSource = _source;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) => Filter();

        private void Filter()
        {
            if (_source == null) return;
            string kw = txtSearch.Text.Trim().Replace("'", "''");
            _source.DefaultView.RowFilter = string.IsNullOrEmpty(kw)
                ? ""
                : $"c_project_no LIKE '%{kw}%' OR n_project_name LIKE '%{kw}%'";
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) AcceptSelection();
        }

        private void btnOk_Click(object sender, EventArgs e) => AcceptSelection();

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void AcceptSelection()
        {
            if (dgv.CurrentRow == null) return;
            DataRowView row = dgv.CurrentRow.DataBoundItem as DataRowView;
            if (row == null) return;

            SelectedProjectId   = Convert.ToInt32(row["i_project_id"]);
            SelectedProjectNo   = row["c_project_no"].ToString();
            SelectedProjectName = row["n_project_name"].ToString();
            this.DialogResult   = DialogResult.OK;
            this.Close();
        }
    }
}
