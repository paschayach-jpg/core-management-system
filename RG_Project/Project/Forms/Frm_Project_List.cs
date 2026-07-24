using RG_Project.Project.Services;
using RG_Project.Login;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RG_Project.Project.Forms
{
    public partial class Frm_Project_List : Form
    {
        private readonly ProjectService _service;
        private readonly string         _userName;
        private readonly string         _fullname;
        private readonly string         _code = ConfigurationManager.AppSettings["code_project"] ?? "PJ001";
        private readonly List<string>   _subcodes = new List<string>();
        private DataTable               _sourceTable; // เก็บข้อมูลทั้งหมดไว้ filter ใน memory

        // ── Office 2016 Navy palette ───────────────────────────────────────────────
        private static readonly Color NavyDark    = Color.FromArgb(30,  58,  95);
        private static readonly Color NavyMedium  = Color.FromArgb(43,  87, 154);
        private static readonly Color NavyMediumH = Color.FromArgb(62, 110, 183);   // hover
        private static readonly Color NavyLight   = Color.FromArgb(70, 110, 165);
        private static readonly Color NavyLightH  = Color.FromArgb(90, 130, 185);   // hover
        private static readonly Color SlateBlue   = Color.FromArgb(100, 130, 168);
        private static readonly Color SlateBlueH  = Color.FromArgb(120, 150, 190);  // hover
        private static readonly Color DangerRed   = Color.FromArgb(196,  53,  53);
        private static readonly Color DangerRedH  = Color.FromArgb(218,  73,  73);  // hover

        public Frm_Project_List()
        {
            InitializeComponent();
            _service      = new ProjectService();
            _userName     = LoginManager.Instance.GetUserName ?? "";
            var emp       = LoginManager.Instance.employeeInfo;
            _fullname     = emp != null
                          ? (emp.EmployeeFirstNameTh + " " + emp.EmployeeLastNameTh).Trim()
                          : _userName;
            getSub();
        }

        private void getSub()
        {
            _subcodes.Clear();
            _subcodes.Add(_code + "_1");
        }

        private string SubCode => _subcodes.Count > 0 ? _subcodes[0] : "PJ001_1";

        private void Frm_Project_List_Load(object sender, EventArgs e)
        {
            lb_user.Text = _fullname;
            lb_date.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            ApplyButtonHovers();
            //SetControlActions();

            LoadTemplateCombo();
            dtpFrom.Value = DateTime.Now.AddMonths(-3);
            dtpTo.Value   = DateTime.Now;

            Search();
        }

        // ─── Hover Effects ────────────────────────────────────────────────────────

        private void ApplyButtonHovers()
        {
            ApplyHover(btnSearch, NavyMediumH, NavyMedium);
            ApplyHover(btnAdd,    NavyMediumH, NavyMedium);
            ApplyHover(btnEdit,   NavyLightH,  NavyLight);
            ApplyHover(btnDelete, DangerRedH,  DangerRed);
            ApplyHover(btnTasks,           Color.FromArgb(130, 80, 210),  Color.FromArgb(111, 66, 193));
            ApplyHover(btnManageTemplate,  Color.FromArgb(40, 150, 110),  Color.FromArgb(32, 120, 90));
        }

        private static void ApplyHover(Button btn, Color hoverColor, Color normalColor)
        {
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = normalColor;
        }

        // ─── Filter Combos ────────────────────────────────────────────────────────

        private void LoadTemplateCombo()
        {
            DataTable dt = _service.GetTemplates();
            dt.Columns["i_template_id"].AllowDBNull   = true;
            dt.Columns["c_template_code"].AllowDBNull = true;
            DataRow blank = dt.NewRow();
            blank["i_template_id"]   = DBNull.Value;
            blank["n_template_name"] = "--- ทุก Template ---";
            dt.Rows.InsertAt(blank, 0);
            cbbTemplate.DisplayMember = "n_template_name";
            cbbTemplate.ValueMember   = "i_template_id";
            cbbTemplate.DataSource    = dt;
            cbbTemplate.SelectedIndex = 0;
        }

        private void LoadFilterCombos(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return;

            // cbbStatus — distinct จากข้อมูลจริง
            var statusRows = dt.AsEnumerable()
                .GroupBy(r => r.Field<string>("c_status"))
                .Select(g => g.First())
                .Where(r => !string.IsNullOrEmpty(r.Field<string>("c_status")))
                .OrderBy(r => r.Field<string>("c_status"))
                .CopyToDataTable();
            DataRow blankStatus = statusRows.NewRow();
            blankStatus["c_status"] = "--- ทุกสถานะ ---";
            statusRows.Rows.InsertAt(blankStatus, 0);

            cbbStatus.SelectedIndexChanged -= CbbStatus_Changed;
            cbbStatus.DataSource    = statusRows;
            cbbStatus.DisplayMember = "c_status";
            cbbStatus.ValueMember   = "c_status";
            cbbStatus.SelectedIndex = 0;
            cbbStatus.SelectedIndexChanged += CbbStatus_Changed;
        }

        private void CbbStatus_Changed(object sender, EventArgs e) => Filter();

        // ─── Filter (DefaultView.RowFilter) ──────────────────────────────────────

        private void Filter()
        {
           if (_sourceTable == null) return;
            var filters = new List<string>();

            // สถานะ
            if (cbbStatus.SelectedIndex > 0 && cbbStatus.SelectedValue != null)
                filters.Add($"c_status = '{cbbStatus.SelectedValue}'");

            // Template
            if (cbbTemplate.SelectedValue != null && cbbTemplate.SelectedValue != DBNull.Value)
                filters.Add($"i_template_id = {cbbTemplate.SelectedValue}");

            // txtProjectNo — ค้นเฉพาะเลขที่โครงการ
            string keyNo = txtProjectNo.Text.Trim();
            if (!string.IsNullOrEmpty(keyNo))
                filters.Add($"c_project_no LIKE '%{keyNo}%'");

            // txtProjectName — ค้นเฉพาะชื่อโครงการ
            string keyName = txtProjectName.Text.Trim();
            if (!string.IsNullOrEmpty(keyName))
                filters.Add($"n_project_name LIKE '%{keyName}%'");

            _sourceTable.DefaultView.RowFilter = string.Join(" AND ", filters);
            lblTotal.Text = $"รายการทั้งหมด: {dgvProjects.Rows.Count:#,##0} รายการ";
            txtTotal.Text = dgvProjects.Rows.Count.ToString("#,##0");
        }

        // ─── Search ───────────────────────────────────────────────────────────────

        private void Search()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                // ตรวจสอบวันที่ — วันสิ้นสุดต้องไม่น้อยกว่าวันเริ่มต้น
                if (chkUseDateFilter.Checked && dtpTo.Value.Date < dtpFrom.Value.Date)
                {
                    MessageBox.Show("วันที่สิ้นสุดต้องไม่น้อยกว่าวันที่เริ่มต้น", "แจ้งเตือน",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtpTo.Focus();
                    return;
                }
                _sourceTable = _service.SearchProjects(
                    "",  // ไม่ filter ที่ DB แล้ว ใช้ DefaultView แทน
                    "",
                    "",
                    "",
                    chkUseDateFilter.Checked ? dtpFrom.Value.ToString("yyyy-MM-dd") : "",
                    chkUseDateFilter.Checked ? dtpTo.Value.ToString("yyyy-MM-dd")   : ""
                );

                dgvProjects.DataSource = _sourceTable;
                SetupDgv();
                LoadFilterCombos(_sourceTable);

                // wire template combo หลัง load
                cbbTemplate.SelectedIndexChanged -= CbbTemplate_Changed;
                cbbTemplate.SelectedIndexChanged += CbbTemplate_Changed;

                Filter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { this.Cursor = Cursors.Arrow; }
        }

        private void CbbTemplate_Changed(object sender, EventArgs e) => Filter();

        // ─── DataGridView Setup ───────────────────────────────────────────────────

        private void SetupDgv()
        {
            dgvProjects.Columns.Clear();
            dgvProjects.AutoGenerateColumns = false;

            {
                var colId     = new DataGridViewTextBoxColumn { Name = "i_project_id", DataPropertyName = "i_project_id", Visible = false };
                var colProjNo = new DataGridViewTextBoxColumn { HeaderText = "เลขที่โครงการ",   DataPropertyName = "c_project_no",    Width = 145 };
                var colName   = new DataGridViewTextBoxColumn { HeaderText = "ชื่อโครงการ",     DataPropertyName = "n_project_name",  Width = 255, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
                var colTmpl   = new DataGridViewTextBoxColumn { HeaderText = "Template",         DataPropertyName = "n_template_name", Width = 175 };
                var colStat   = new DataGridViewTextBoxColumn { HeaderText = "สถานะ",            DataPropertyName = "c_status",        Width = 108 };
                var colPct    = new DataGridViewProgressBarColumn { HeaderText = "% เสร็จ",      DataPropertyName = "i_pct_overall",   Width = 90  };
                var colStart  = new DataGridViewTextBoxColumn { HeaderText = "วันที่เริ่ม",      DataPropertyName = "d_start_date",    Width = 115 };
                var colCreate = new DataGridViewTextBoxColumn { HeaderText = "วันที่สร้าง",     DataPropertyName = "d_create_date",   Width = 125 };
                var colBy     = new DataGridViewTextBoxColumn { HeaderText = "สร้างโดย",        DataPropertyName = "i_create_user",   Width = 95  };
                var colUpBy   = new DataGridViewTextBoxColumn { HeaderText = "ผู้ทำงานล่าสุด",  DataPropertyName = "i_last_worker",  Width = 110 };
                var colUpDt   = new DataGridViewTextBoxColumn { HeaderText = "วันที่ทำงานล่าสุด", DataPropertyName = "d_last_update",  Width = 130 };

                colStat.DefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleCenter;
                colUpDt.DefaultCellStyle.Format     = "dd/MM/yyyy HH:mm";

                dgvProjects.Columns.AddRange(colId, colProjNo, colName, colTmpl, colStat, colPct, colStart, colCreate, colBy, colUpBy, colUpDt);
            }

            // ── Column Header style ───────────────────────────────────────────────
            var hStyle = dgvProjects.ColumnHeadersDefaultCellStyle;
            hStyle.BackColor  = NavyDark;
            hStyle.ForeColor  = Color.White;
            hStyle.Font       = new Font("Segoe UI", 9F, FontStyle.Bold);
            hStyle.Padding    = new Padding(6, 0, 0, 0);
            hStyle.Alignment  = DataGridViewContentAlignment.MiddleLeft;
            dgvProjects.ColumnHeadersHeight         = 32;
            dgvProjects.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvProjects.EnableHeadersVisualStyles   = false;

            // ── Cell style ────────────────────────────────────────────────────────
            var dStyle = dgvProjects.DefaultCellStyle;
            dStyle.Font               = new Font("Segoe UI", 9F);
            dStyle.ForeColor          = Color.FromArgb(42, 57, 80);
            dStyle.BackColor          = Color.White;
            dStyle.SelectionBackColor = Color.FromArgb(190, 215, 255);
            dStyle.SelectionForeColor = Color.FromArgb(20, 40, 80);
            dStyle.Padding            = new Padding(4, 0, 0, 0);

            var altStyle = dgvProjects.AlternatingRowsDefaultCellStyle;
            altStyle.BackColor = Color.FromArgb(236, 244, 255);
            altStyle.ForeColor = Color.FromArgb(42, 57, 80);

            dgvProjects.GridColor          = Color.FromArgb(208, 220, 240);
            dgvProjects.CellBorderStyle    = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvProjects.RowHeadersVisible  = false;
            dgvProjects.RowTemplate.Height = 28;
            dgvProjects.SelectionMode      = DataGridViewSelectionMode.FullRowSelect;
            dgvProjects.ReadOnly           = true;
            dgvProjects.AllowUserToAddRows = false;
            dgvProjects.AllowUserToDeleteRows = false;
            dgvProjects.MultiSelect        = false;

            dgvProjects.CellFormatting -= DgvProjects_CellFormatting;
            dgvProjects.CellFormatting += DgvProjects_CellFormatting;
        }

        /// <summary>
        /// แทนที่ column ที่มี DataPropertyName ตรงกับ propName ด้วย DataGridViewProgressBarColumn
        /// </summary>
        private void ReplaceWithProgressBarColumn(string propName)
        {
            var existing = dgvProjects.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(c => c.DataPropertyName == propName);
            if (existing == null) return;

            int idx = existing.Index;
            var pbCol = new DataGridViewProgressBarColumn
            {
                Name             = existing.Name,
                DataPropertyName = propName,
                HeaderText       = existing.HeaderText,
                Width            = existing.Width < 80 ? 90 : existing.Width,
                DisplayIndex     = existing.DisplayIndex,
                Visible          = existing.Visible,
            };
            dgvProjects.Columns.RemoveAt(idx);
            dgvProjects.Columns.Insert(idx, pbCol);
        }

        private void DgvProjects_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvProjects.Columns[e.ColumnIndex].DataPropertyName == "c_status" && e.Value != null)
            {
                switch (e.Value.ToString())
                {
                    case "Completed":
                        e.CellStyle.BackColor = Color.FromArgb(198, 239, 206);
                        e.CellStyle.ForeColor = Color.FromArgb(0, 97, 0);
                        e.CellStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;
                    case "InProgress":
                        e.CellStyle.BackColor = Color.FromArgb(255, 235, 156);
                        e.CellStyle.ForeColor = Color.FromArgb(99, 71, 0);
                        e.CellStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;
                    case "Cancelled":
                        e.CellStyle.BackColor = Color.FromArgb(255, 199, 206);
                        e.CellStyle.ForeColor = Color.FromArgb(156, 0, 6);
                        e.CellStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;
                    default: // Open
                        e.CellStyle.BackColor = Color.FromArgb(215, 228, 248);
                        e.CellStyle.ForeColor = Color.FromArgb(30, 58, 95);
                        e.CellStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;
                }
            }
        }

        // ─── Button Events ────────────────────────────────────────────────────────

        private void btnSearch_Click(object sender, EventArgs e) { Search(); }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtProjectNo.Text         = "";
            txtProjectName.Text       = "";
            cbbStatus.SelectedIndex   = 0;
            cbbTemplate.SelectedIndex = 0;
            chkUseDateFilter.Checked  = false;
            dtpFrom.Value = DateTime.Now.AddMonths(-3);
            dtpTo.Value   = DateTime.Now;
            Filter();
        }

        private void txtProjectNo_TextChanged(object sender, EventArgs e)   => Filter();
        private void txtProjectName_TextChanged(object sender, EventArgs e) => Filter();

        private void dtpTo_ValueChanged(object sender, EventArgs e)
        {
            if (chkUseDateFilter.Checked && dtpTo.Value.Date < dtpFrom.Value.Date)
            {
                dtpTo.Value = dtpFrom.Value;
                MessageBox.Show("วันที่สิ้นสุดต้องไม่น้อยกว่าวันที่เริ่มต้น", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dtpFrom_ValueChanged(object sender, EventArgs e)
        {
            if (chkUseDateFilter.Checked && dtpTo.Value.Date < dtpFrom.Value.Date)
                dtpTo.Value = dtpFrom.Value;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var frm = new Frm_Project_Detail(0);
            if (frm.ShowDialog() == DialogResult.OK) Search();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            int id = GetSelectedProjectId();
            if (id > 0)
            {
                var frm = new Frm_Project_Detail(id);
                frm.ShowDialog();
                Search();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProjects.CurrentRow == null)
            {
                MessageBox.Show("กรุณาเลือกโครงการที่ต้องการลบ", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = GetSelectedProjectId();
            if (id <= 0)
            {
                MessageBox.Show("ไม่สามารถระบุโครงการที่เลือกได้ กรุณาลองใหม่", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string projNo = GetProjectNumberForSelectedRow(id);

            if (MessageBox.Show($"ต้องการลบโครงการ '{projNo}' ใช่หรือไม่?\n\nโครงการที่มีการดำเนินงานแล้วจะไม่สามารถลบได้",
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                string err;
                if (_service.DeleteProject(id, _userName, out err))
                {
                    MessageBox.Show("ลบโครงการเรียบร้อยแล้ว", "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Search();
                }
                else
                {
                    MessageBox.Show(err, "ไม่สามารถลบได้",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการลบ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnTasks_Click(object sender, EventArgs e)
        {
            var frm = new Frm_Task_Dashboard();
            frm.Show();
        }

        private void btnManageTemplate_Click(object sender, EventArgs e)
        {
            var frm = new Frm_Template_Doc_Edit();
            frm.ShowDialog();
        }

        private void dgvProjects_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = GetSelectedProjectId();
            if (id > 0)
            {
                var frm = new Frm_Project_Detail(id);
                frm.ShowDialog();
                Search();
            }
        }
        private int GetSelectedProjectId()
        {
            if (dgvProjects.CurrentRow == null) return 0;
            // DataBoundItem เชื่อมตรงกับ DataRowView — ถูกต้องแม้จะ filter/sort
            var drv = dgvProjects.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return 0;
            if (!drv.Row.Table.Columns.Contains("i_project_id")) return 0;
            if (drv["i_project_id"] == DBNull.Value) return 0;
            return Convert.ToInt32(drv["i_project_id"]);
        }

        // ─── Export Excel ─────────────────────────────────────────────────────────

        private void btnExcel_Click(object sender, EventArgs e)
        {
            if (_sourceTable == null || _sourceTable.Rows.Count == 0)
            {
                MessageBox.Show("ไม่มีข้อมูลสำหรับ Export", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MessageBox.Show("ฟังก์ชัน Export Excel ยังไม่รองรับในโหมดนี้", "แจ้งเตือน",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ─── Permission Control ───────────────────────────────────────────────────

        //private void SetControlActions()
        //{
        //    // ปิดทุกปุ่มก่อน แล้วเปิดตาม permission
        //    btnAdd.Enabled    = false;
        //    btnEdit.Enabled   = false;
        //    btnDelete.Enabled = false;
        //    btnExcel.Enabled  = false;

        //    try
        //    {
        //        var actions = GlobalData.MenuActions
        //            .Where(x => x.MenuCode == "bis-pj-001").ToList();

        //        if (!actions.Any())
        //        {
        //            // ถ้าไม่มี config ให้เปิดทั้งหมด (dev mode)
        //            btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = btnExcel.Enabled = true;
        //            return;
        //        }

        //        foreach (var action in actions)
        //        {
        //            if (action.ActionDesc == "Entry")
        //            { btnAdd.Enabled = true; btnEdit.Enabled = true; btnDelete.Enabled = true; }
        //            if (action.ActionDesc == "Export")
        //            { btnExcel.Enabled = true; }
        //        }
        //    }
        //    catch
        //    {
        //        // ถ้า GlobalData ยังไม่พร้อม เปิดทั้งหมด
        //        btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = btnExcel.Enabled = true;
        //    }
        //}

        private string GetProjectNumberForSelectedRow(int id)
        {
            try
            {
                // อ่านจาก DataBoundItem โดยตรง — ถูกต้องแม้จะ filter/sort
                if (dgvProjects.CurrentRow != null)
                {
                    var drv = dgvProjects.CurrentRow.DataBoundItem as DataRowView;
                    if (drv != null && drv.Row.Table.Columns.Contains("c_project_no"))
                        return drv["c_project_no"]?.ToString() ?? string.Empty;
                }
                // Fallback: ค้นใน _sourceTable
                if (_sourceTable != null)
                {
                    DataRow[] rows = _sourceTable.Select("i_project_id = " + id);
                    if (rows.Length > 0 && _sourceTable.Columns.Contains("c_project_no"))
                        return rows[0]["c_project_no"]?.ToString() ?? string.Empty;
                }
            }
            catch { }
            return string.Empty;
        }
    }
}
