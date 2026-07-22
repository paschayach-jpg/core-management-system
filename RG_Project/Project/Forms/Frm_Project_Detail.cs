using RG_Project.Project.Models;
using RG_Project.Project.Services;
using RG_Project.Project.Controllers;
using RG_Project.Login;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RG_Project.Project.Forms
{
    public partial class Frm_Project_Detail : Form
    {
        private readonly int _i_project_id;       // 0 = New, >0 = Edit
        private readonly bool _isNew;

        // ── Office 2016 Navy palette ───────────────────────────────────────────────
        private static readonly Color NavyDark    = Color.FromArgb(30,  58,  95);
        private static readonly Color NavyMedium  = Color.FromArgb(43,  87, 154);
        private static readonly Color NavyMediumH = Color.FromArgb(62, 110, 183);
        private static readonly Color NavyLight   = Color.FromArgb(70, 110, 165);
        private static readonly Color NavyLightH  = Color.FromArgb(90, 130, 185);
        private static readonly Color SlateBlue   = Color.FromArgb(100, 130, 168);
        private static readonly Color SlateBlueH  = Color.FromArgb(120, 150, 190);
        private static readonly Color DangerRed   = Color.FromArgb(196,  53,  53);
        private static readonly Color DangerRedH  = Color.FromArgb(218,  73,  73);

        private readonly ProjectService          _projectSvc;
        private readonly ProjectStateService     _stateSvc;
        private readonly ProjectProgressService  _progressSvc;
        private readonly ProjectDocumentService  _docSvc;
        private readonly ProjectMemberController _memberCtrl;

        private readonly string _userName;
        private readonly string _fullname;

        // Track which tabs have been loaded
        private bool _tab2Loaded = false;
        private bool _tab3Loaded = false;
        private bool _tab4Loaded = false;
        private bool _tab5Loaded = false;

        // Legal Dictionary tab (dynamic)
        private TabPage        _tabLd;
        private TextBox        _ldTxtCode;        // auto-built readonly
        private TextBox        _ldTxtSection;     // 4 digits — มาตรา
        private TextBox        _ldTxtSlash;       // 2 digits — แทรก /1 /2
        private TextBox        _ldTxtSub1;        // 2 digits — อนุมาตรา (1)
        private TextBox        _ldTxtSub2;        // 2 digits — อนุมาตรา (2)
        private TextBox        _ldTxtPara;        // 2 digits — วรรค
        private TextBox        _ldTxtLetterType;  // 2 digits — หนังสือประเภท
        private TextBox        _ldTxtLetterNo;    // 3 digits — ฉบับที่
        private ComboBox       _ldCbbLawType;
        private ComboBox       _ldCbbArtType;
        private ComboBox       _ldCbbArtStatus;
        private Label          _ldLblVersion;
        private TextBox        _ldTxtTitle;
        private ComboBox       _ldCbbCatFull;
        private ComboBox       _ldCbbConf;
        // CategoryID maps: display text → CategoryID
        private readonly System.Collections.Generic.Dictionary<string, int> _ldLawTypeIdMap = new System.Collections.Generic.Dictionary<string, int>();
        private readonly System.Collections.Generic.Dictionary<string, int> _ldCatIdMap     = new System.Collections.Generic.Dictionary<string, int>();
        private TextBox        _ldTxtTags;
        private DateTimePicker _ldDtpPublished;
        private DateTimePicker _ldDtpExpiry;
        private CheckBox       _ldChkExpiry;
        private TextBox        _ldTxtSummary;
        private RichTextBox    _ldRtbContent;
        private Label          _ldLblSBStatus;
        private Label          _ldLblSBCreated;
        private Label          _ldLblKmsInfo;
        private int            _ldKmsArticleId;   // 0 = ยังไม่ได้ส่ง API
        private string         _ldKmsArticleCode;
        private readonly KmsController _kmsCtr = new KmsController();


        public Frm_Project_Detail(int i_project_id)
        {
            InitializeComponent();
            _i_project_id = i_project_id;
            _isNew        = (i_project_id == 0);
            _projectSvc   = new ProjectService();
            _stateSvc     = new ProjectStateService();
            _progressSvc  = new ProjectProgressService();
            _docSvc       = new ProjectDocumentService();
            _memberCtrl   = new ProjectMemberController();
            _userName     = LoginManager.Instance.GetUserName;
            _fullname     = LoginManager.Instance.employeeInfo.EmployeeFirstNameTh +" "+ LoginManager.Instance.employeeInfo.EmployeeLastNameTh;
        }

        private void Frm_Project_Detail_Load(object sender, EventArgs e)
        {
            lb_user.Text = _fullname;
            lb_date.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            ApplyButtonHovers();
            LoadTemplateCombo();
            //SetupDataGridViews();

            if (_isNew)
            {
                lblProjectNo.Text          = "(สร้างใหม่)";
                txtProjectNo.Text          = "";
                txtProjectNo.ReadOnly      = false;
                chkAutoGenProjectNo.Visible = true;
                dtpStartDate.Checked       = false;
                dtpDueDate.Checked         = false;
                cbbStatus.SelectedItem     = "Open";
            }
            else
            {
                chkAutoGenProjectNo.Visible = false;
                txtProjectNo.ReadOnly       = true;
                LoadProjectData();
            }
        }

        // ─── Hover Effects ────────────────────────────────────────────────────────

        private void ApplyButtonHovers()
        {
            ApplyHover(btnSave,            NavyMediumH, NavyMedium);
            ApplyHover(btnClose,           SlateBlueH,  SlateBlue);
            ApplyHover(btnAddState,        NavyMediumH, NavyMedium);
            ApplyHover(btnEditState,       NavyLightH,  NavyLight);
            ApplyHover(btnDeleteState,     DangerRedH,  DangerRed);
            ApplyHover(btnMoveUp,          SlateBlueH,  SlateBlue);
            ApplyHover(btnMoveDown,        SlateBlueH,  SlateBlue);
            ApplyHover(btnUpdateProgress,  NavyMediumH, NavyMedium);
            ApplyHover(btnAddDoc,          NavyMediumH, NavyMedium);
            ApplyHover(btnEditDoc,         NavyLightH,  NavyLight);
            ApplyHover(btnDeleteDoc,       DangerRedH,  DangerRed);
            ApplyHover(btnUpdateDocStatus, NavyLightH,  NavyLight);
        }

        private static void ApplyHover(Button btn, Color hoverColor, Color normalColor)
        {
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = normalColor;
        }

        // ─── Tab Header Custom Draw ────────────────────────────────────────────

        private void tabMain_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = tabMain.TabPages[e.Index];
            Color   clr  = (e.State == DrawItemState.Selected)
                ? Color.FromArgb(30, 58, 95)    // NavyDark  – selected tab
                : Color.FromArgb(70, 110, 165); // NavyLight – other tabs

            using (var brush = new SolidBrush(clr))
                e.Graphics.FillRectangle(brush, e.Bounds);

            SizeF sz   = e.Graphics.MeasureString(page.Text, e.Font);
            float xPos = e.Bounds.Left + (e.Bounds.Width  - sz.Width)  / 2;
            float yPos = e.Bounds.Top  + (e.Bounds.Height - sz.Height) / 2;
            e.Graphics.DrawString(page.Text, e.Font, Brushes.White, xPos, yPos);
        }

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabMain.SelectedIndex)
            {
                case 1: LoadTab2States();    break;
                case 2: LoadTab3Progress();  break;
                case 3: LoadTab4Documents(); break;
                case 4: LoadTab5Members();   break;
            }
        }

        // ─── Tab 1: Project Info ───────────────────────────────────────────────

        private void LoadTemplateCombo()
        {
            DataTable dt = _projectSvc.GetTemplates();
            dt.Columns["i_template_id"].AllowDBNull = true;
            dt.Columns["c_template_code"].AllowDBNull = true;
            DataRow blank = dt.NewRow();
            blank["i_template_id"]   = DBNull.Value;
            blank["n_template_name"] = "--- ไม่ระบุ Template ---";
            dt.Rows.InsertAt(blank, 0);
            cbbTemplate.DisplayMember = "n_template_name";
            cbbTemplate.ValueMember   = "i_template_id";
            cbbTemplate.DataSource    = dt;
            cbbTemplate.SelectedIndex = 0;
        }

        private void LoadProjectData()
        {
            DataTable dt = _projectSvc.GetProjectById(_i_project_id);
            if (dt.Rows.Count == 0) return;
            DataRow r = dt.Rows[0];

            lblProjectNo.Text    = r["c_project_no"].ToString();
            txtProjectNo.Text    = r["c_project_no"].ToString();
            txtProjectName.Text  = r["n_project_name"].ToString();
            txtDescription.Text  = r["c_description"].ToString();
            cbbStatus.SelectedItem = r["c_status"].ToString();
            SetStatusColor(r["c_status"].ToString());

            // Template
            if (r["i_template_id"] != DBNull.Value)
            {
                int tmplId = Convert.ToInt32(r["i_template_id"]);
                DataTable dtTmpl = (DataTable)cbbTemplate.DataSource;
                foreach (DataRowView row in dtTmpl.DefaultView)
                {
                    if (row["i_template_id"] != DBNull.Value &&
                        Convert.ToInt32(row["i_template_id"]) == tmplId)
                    { cbbTemplate.SelectedItem = row; break; }
                }
            }

            // Dates
            if (r["d_start_date"] != DBNull.Value)
            { dtpStartDate.Checked = true; dtpStartDate.Value = Convert.ToDateTime(r["d_start_date"]); }
            else { dtpStartDate.Checked = false; }

            if (r["d_due_date"] != DBNull.Value)
            { dtpDueDate.Checked = true; dtpDueDate.Value = Convert.ToDateTime(r["d_due_date"]); }
            else { dtpDueDate.Checked = false; }


            // Audit
            lblCreateBy.Text   = r["i_create_user"].ToString();
            lblCreateDate.Text = r["d_create_date"] != DBNull.Value
                ? Convert.ToDateTime(r["d_create_date"]).ToString("dd/MM/yyyy HH:mm") : "";
            lblUpdateBy.Text   = r["i_update_user"].ToString();
            lblUpdateDate.Text = r["d_update_date"] != DBNull.Value
                ? Convert.ToDateTime(r["d_update_date"]).ToString("dd/MM/yyyy HH:mm") : "";

            // Legal Dict
            bool isLd = r["f_is_legal_dict"] != DBNull.Value && r["f_is_legal_dict"].ToString() == "Y";
            CheckLegalDictTab(isLd);
            if (isLd)
            {
                string json = r["n_legal_dict_json"] != DBNull.Value ? r["n_legal_dict_json"].ToString() : "";
                LdLoadJson(json);
            }

            btnViewTasks.Visible = true;
        }

        private void SetStatusColor(string status)
        {
            Color bg, fg;
            switch (status)
            {
                case "Completed":  bg = Color.FromArgb(198, 239, 206); fg = Color.FromArgb(0,   97,  0);  break;
                case "InProgress": bg = Color.FromArgb(255, 235, 156); fg = Color.FromArgb(99,  71,  0);  break;
                case "Cancelled":  bg = Color.FromArgb(255, 199, 206); fg = Color.FromArgb(156,  0,  6);  break;
                default:           bg = Color.FromArgb(215, 228, 248); fg = Color.FromArgb(30,  58, 95);  break;
            }
            lblStatusDisplay.BackColor = bg;
            lblStatusDisplay.ForeColor = fg;
            lblStatusDisplay.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatusDisplay.Text      = status;
        }

        private void cbbTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            var val = cbbTemplate.SelectedValue;
            if (val == null || val is DBNull || !(val is int))
            {
                CheckLegalDictTab(false);
                return;
            }
            int tmplId = (int)val;

            if (_isNew && !_tab2Loaded)
            {
                DataTable dt = _stateSvc.GetTemplateStates(tmplId);
                dgvStates.DataSource = dt;
                SetupDgvStates();
            }

            try { CheckLegalDictTab(new ProjectTemplateController().IsLegalDict(tmplId)); }
            catch { CheckLegalDictTab(false); }
        }

        private void chkAutoGenProjectNo_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoGenProjectNo.Checked)
            {
                txtProjectNo.Text      = "(จะสร้างอัตโนมัติเมื่อบันทึก)";
                txtProjectNo.ReadOnly  = true;
                txtProjectNo.ForeColor = Color.Gray;
            }
            else
            {
                txtProjectNo.Text      = "";
                txtProjectNo.ReadOnly  = false;
                txtProjectNo.ForeColor = Color.Black;
            }
        }

        // ─── Tab 2: States ────────────────────────────────────────────────────

        private void LoadTab2States()
        {
            if (_i_project_id == 0) return;
            DataTable dt = _stateSvc.GetStatesWithProgress(_i_project_id);
            dgvStates.DataSource = dt;
            SetupDgvStates();
            _tab2Loaded = true;
        }

        private void SetupDgvStates()
        {
            dgvStates.Columns.Clear();
            dgvStates.AutoGenerateColumns = false;

            var colSeq   = new DataGridViewTextBoxColumn { HeaderText = "#",               Width = 40  };
            colSeq.DefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleCenter;
            colSeq.HeaderCell.Style.Alignment  = DataGridViewContentAlignment.MiddleCenter;
            var colName  = new DataGridViewTextBoxColumn { HeaderText = "ชื่อขั้นตอน",    DataPropertyName = "n_state_name",     Width = 180 };
            var colDesc  = new DataGridViewTextBoxColumn { HeaderText = "รายละเอียด",     DataPropertyName = "c_description",    Width = 150 };
            var colReq   = new DataGridViewTextBoxColumn { HeaderText = "จำเป็น",         DataPropertyName = "f_required",       Width = 60  };
            var colDays  = new DataGridViewTextBoxColumn { HeaderText = "วัน (คาดการณ์)", DataPropertyName = "i_estimated_days", Width = 100 };
            var colStat  = new DataGridViewTextBoxColumn { HeaderText = "สถานะ",          DataPropertyName = "c_status",         Width = 100 };
            // % เสร็จ — ถ้ามี ref จะ override ด้วย i_ref_pct_overall ใน CellFormatting
            var colPct   = new DataGridViewProgressBarColumn { HeaderText = "% เสร็จ",    DataPropertyName = "i_pct_complete",   Width = 90  };
            var colRefNo = new DataGridViewTextBoxColumn { HeaderText = "โครงการที่ ref", DataPropertyName = "c_ref_project_no", Width = 140 };

            colDays.DefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleCenter;
            colReq.DefaultCellStyle.Alignment   = DataGridViewContentAlignment.MiddleCenter;
            colRefNo.DefaultCellStyle.ForeColor = Color.FromArgb(43, 87, 154);
            colRefNo.DefaultCellStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Italic);

            dgvStates.Columns.AddRange(colSeq, colName, colDesc, colReq, colDays, colStat, colPct, colRefNo);
            StyleDgv(dgvStates);

            dgvStates.CellFormatting -= DgvStates_CellFormatting;
            dgvStates.CellFormatting += DgvStates_CellFormatting;
        }

        /// <summary>
        /// - แถวที่มี ref: พื้นหลังสีฟ้าอ่อน, % เสร็จ ใช้ i_ref_pct_overall แทน i_pct_complete
        /// - แถวที่ไม่มี ref: colRefNo แสดง "-"
        /// </summary>
        private void DgvStates_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgvStates.DataSource == null) return;
            DataRow r = ((DataTable)dgvStates.DataSource).Rows[e.RowIndex];
            bool hasRef = r["i_ref_project_id"] != DBNull.Value;
            string colProp = dgvStates.Columns[e.ColumnIndex].DataPropertyName;

            if (hasRef)
            {
                e.CellStyle.BackColor = e.RowIndex % 2 == 0
                    ? Color.FromArgb(230, 242, 255)
                    : Color.FromArgb(215, 233, 255);
            }

            if (colProp == "c_ref_project_no" && !hasRef)
            {
                e.Value               = "-";
                e.FormattingApplied   = true;
                e.CellStyle.ForeColor = Color.FromArgb(180, 180, 180);
            }
        }

        private void btnAddState_Click(object sender, EventArgs e)
        {
            if (_i_project_id == 0)
            { MessageBox.Show("กรุณาบันทึกโครงการก่อนเพิ่มขั้นตอน", "แจ้งเตือน"); return; }
            DateTime? start = dtpStartDate.Checked ? dtpStartDate.Value : (DateTime?)null;
            DateTime? due   = dtpDueDate.Checked   ? dtpDueDate.Value   : (DateTime?)null;
            var frm = new Frm_Project_State_Edit(_i_project_id, null, start, due);
            if (frm.ShowDialog() == DialogResult.OK) LoadTab2States();
        }

        private void btnEditState_Click(object sender, EventArgs e)
        {
            if (dgvStates.CurrentRow == null) return;
            var model = GetSelectedStateModel();
            if (model == null) return;
            DateTime? start = dtpStartDate.Checked ? dtpStartDate.Value : (DateTime?)null;
            DateTime? due   = dtpDueDate.Checked   ? dtpDueDate.Value   : (DateTime?)null;
            var frm = new Frm_Project_State_Edit(_i_project_id, model, start, due);
            if (frm.ShowDialog() == DialogResult.OK) LoadTab2States();
        }
        private void btnDeleteState_Click(object sender, EventArgs e)
        {
            if (dgvStates.CurrentRow == null) return;
            var model = GetSelectedStateModel();
            if (model == null) return;

            if (MessageBox.Show($"ต้องการลบขั้นตอน '{model.n_state_name}' ใช่หรือไม่?",
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string err;
                if (_stateSvc.DeleteState(model.i_proj_state_id, out err))
                    LoadTab2States();
                else
                    MessageBox.Show(err, "ไม่สามารถลบได้", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnMoveUp_Click(object sender, EventArgs e) => MoveState(-1);
        private void btnMoveDown_Click(object sender, EventArgs e) => MoveState(1);

        private void dgvStates_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnEditState_Click(sender, e);
        }

        private void MoveState(int direction)
        {
            if (dgvStates.CurrentRow == null) return;
            int idx = dgvStates.CurrentRow.Index;
            int newIdx = idx + direction;
            if (newIdx < 0 || newIdx >= dgvStates.Rows.Count) return;

            DataTable dt = (DataTable)dgvStates.DataSource;
            var orderList = new System.Collections.Generic.List<Tuple<int, int>>();
            for (int i = 0; i < dt.Rows.Count; i++)
                orderList.Add(new Tuple<int, int>(Convert.ToInt32(dt.Rows[i]["i_proj_state_id"]), i + 1));

            // Swap
            var tmp = orderList[idx];
            orderList[idx]    = new Tuple<int, int>(tmp.Item1, newIdx + 1);
            orderList[newIdx] = new Tuple<int, int>(orderList[newIdx].Item1, idx + 1);

            var ctrl = new Controllers.ProjectStateController();
            ctrl.ReorderStates(orderList);
            LoadTab2States();
        }

        private ProjectStateModel GetSelectedStateModel()
        {
            if (dgvStates.CurrentRow == null || dgvStates.DataSource == null) return null;
            DataRow r = ((DataTable)dgvStates.DataSource).Rows[dgvStates.CurrentRow.Index];
            return new ProjectStateModel
            {
                i_proj_state_id  = Convert.ToInt32(r["i_proj_state_id"]),
                i_project_id     = _i_project_id,
                n_state_name     = r["n_state_name"].ToString(),
                i_seq_no         = Convert.ToInt32(r["i_seq_no"]),
                c_description    = r["c_description"].ToString(),
                f_required       = r["f_required"].ToString(),
                c_status         = r["c_status"].ToString(),
                i_pct_complete   = Convert.ToInt32(r["i_pct_complete"]),
                i_estimated_days = r["i_estimated_days"] != DBNull.Value
                    ? (int?)Convert.ToInt32(r["i_estimated_days"]) : null,
                i_ref_project_id   = r["i_ref_project_id"] != DBNull.Value
                    ? (int?)Convert.ToInt32(r["i_ref_project_id"]) : null,
                n_ref_project_name = r["n_ref_project_name"] != DBNull.Value
                    ? r["n_ref_project_name"].ToString() : null,
                c_ref_project_no   = r["c_ref_project_no"] != DBNull.Value
                    ? r["c_ref_project_no"].ToString() : null,
                i_ref_pct_overall  = r["i_ref_pct_overall"] != DBNull.Value
                    ? Convert.ToInt32(r["i_ref_pct_overall"]) : 0,
            };
        }

        // ─── Tab 3: Progress ─────────────────────────────────────────────────

        private void LoadTab3Progress()
        {
            if (_i_project_id == 0) return;
            DataTable dt = _progressSvc.GetLatestProgressByProject(_i_project_id);
            dgvProgress.DataSource = dt;
            SetupDgvProgress();
            int pct = _progressSvc.GetOverallCompletionPct(_i_project_id);
            progressOverall.Value = pct;
            lblOverallPct.Text    = $"ความคืบหน้ารวม: {pct}%";
            _tab3Loaded = true;
        }

        private void SetupDgvProgress()
        {
            dgvProgress.Columns.Clear();
            dgvProgress.AutoGenerateColumns = false;

            var colNo    = new DataGridViewTextBoxColumn { HeaderText = "#",               Width = 40  };
            colNo.DefaultCellStyle.Alignment   = DataGridViewContentAlignment.MiddleCenter;
            colNo.HeaderCell.Style.Alignment   = DataGridViewContentAlignment.MiddleCenter;
            var colName  = new DataGridViewTextBoxColumn { HeaderText = "ขั้นตอน",        DataPropertyName = "n_state_name",    Width = 170 };
            var colStat  = new DataGridViewTextBoxColumn { HeaderText = "สถานะ",          DataPropertyName = "state_c_status",  Width = 100 };
            // % เสร็จ — ถ้ามี ref จะ override ด้วย i_ref_pct_overall ใน CellFormatting
            var colPct   = new DataGridViewProgressBarColumn { HeaderText = "% เสร็จ",    DataPropertyName = "i_pct_complete",  Width = 90  };
            var colRem   = new DataGridViewTextBoxColumn { HeaderText = "หมายเหตุ",       DataPropertyName = "n_remark",        Width = 150 };
            var colDate  = new DataGridViewTextBoxColumn { HeaderText = "วันที่อัปเดต",   DataPropertyName = "d_action_date",   Width = 130 };
            var colUser  = new DataGridViewTextBoxColumn { HeaderText = "โดย",            DataPropertyName = "i_action_user",   Width = 80  };
            var colRefNo = new DataGridViewTextBoxColumn { HeaderText = "โครงการที่ ref", DataPropertyName = "c_ref_project_no", Width = 140 };

            colRefNo.DefaultCellStyle.ForeColor = Color.FromArgb(43, 87, 154);
            colRefNo.DefaultCellStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Italic);

            dgvProgress.Columns.AddRange(colNo, colName, colStat, colPct, colRem, colDate, colUser, colRefNo);
            StyleDgv(dgvProgress);

            dgvProgress.CellFormatting   -= DgvProgress_CellFormatting;
            dgvProgress.CellFormatting   += DgvProgress_CellFormatting;
            dgvProgress.SelectionChanged -= DgvProgress_SelectionChanged;
            dgvProgress.SelectionChanged += DgvProgress_SelectionChanged;
        }

        /// <summary>
        /// - แถวที่มี ref: พื้นหลังสีส้มอ่อน, % เสร็จ ใช้ i_ref_pct_overall แทน i_pct_complete
        /// - แถวที่ไม่มี ref: colRefNo แสดง "-"
        /// </summary>
        private void DgvProgress_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgvProgress.DataSource == null) return;
            DataRow r = ((DataTable)dgvProgress.DataSource).Rows[e.RowIndex];
            bool hasRef = r["i_ref_project_id"] != DBNull.Value;
            string colProp = dgvProgress.Columns[e.ColumnIndex].DataPropertyName;

            if (hasRef)
            {
                e.CellStyle.BackColor = e.RowIndex % 2 == 0
                    ? Color.FromArgb(255, 243, 224)
                    : Color.FromArgb(255, 235, 205);
                e.CellStyle.ForeColor = Color.FromArgb(99, 71, 0);
            }

            if (colProp == "c_ref_project_no" && !hasRef)
            {
                e.Value               = "-";
                e.FormattingApplied   = true;
                e.CellStyle.ForeColor = Color.FromArgb(180, 180, 180);
            }
        }

        /// <summary>
        /// Enable/Disable ปุ่ม "อัปเดตความคืบหน้า" ตามว่า state ที่เลือกมี ref หรือไม่
        /// </summary>
        private void DgvProgress_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProgress.CurrentRow == null || dgvProgress.DataSource == null)
            {
                btnUpdateProgress.Enabled = true;
                return;
            }
            DataRow r = ((DataTable)dgvProgress.DataSource).Rows[dgvProgress.CurrentRow.Index];
            bool hasRef = r["i_ref_project_id"] != DBNull.Value;
            btnUpdateProgress.Enabled = !hasRef;

            // tooltip hint
            if (hasRef)
            {
                string refNo = r["c_ref_project_no"]?.ToString() ?? "";
                btnUpdateProgress.Text    = "อัปเดต (ref)";
                btnUpdateProgress.BackColor = Color.FromArgb(150, 150, 150);
            }
            else
            {
                btnUpdateProgress.Text      = "อัปเดตความคืบหน้า";
                btnUpdateProgress.BackColor = NavyMedium;
            }
        }

        private void btnUpdateProgress_Click(object sender, EventArgs e)
        {
            if (dgvProgress.CurrentRow == null || _i_project_id == 0) return;
            DataRow r = ((DataTable)dgvProgress.DataSource).Rows[dgvProgress.CurrentRow.Index];

            // ถ้า state นี้มี ref project — ห้ามแก้ไขเอง
            bool hasRef = r["i_ref_project_id"] != DBNull.Value;
            if (hasRef)
            {
                string refNo   = r["c_ref_project_no"]?.ToString() ?? "";
                string refName = r["n_ref_project_name"]?.ToString() ?? "";
                MessageBox.Show(
                    $"ขั้นตอนนี้ผูกกับโครงการ [{refNo}] {refName}\n" +
                    "ความคืบหน้าจะดึงมาจากโครงการที่ ref อัตโนมัติ ไม่สามารถแก้ไขเองได้",
                    "ไม่สามารถแก้ไขได้", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int stateId = Convert.ToInt32(r["i_proj_state_id"]);
            string name = r["n_state_name"].ToString();
            int curPct  = Convert.ToInt32(r["i_pct_complete"]);

            var frm = new Frm_Project_Progress_Update(stateId, _i_project_id, name, curPct);
            if (frm.ShowDialog() == DialogResult.OK) LoadTab3Progress();
        }

        private void dgvProgress_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnUpdateProgress_Click(sender, e);
        }

        // ─── Tab 4: Document Workflow ─────────────────────────────────────────

        private void LoadTab4Documents()
        {
            if (_i_project_id == 0) return;
            LoadDocWorkflowData();
            LoadStatesFilterCombo();
            _tab4Loaded = true;
        }

        private void LoadDocWorkflowData(int stateId = 0)
        {
            DataTable dt = stateId > 0
                ? _docSvc.GetDocWorkflow(stateId)
                : _docSvc.GetDocWorkflowByProject(_i_project_id);
            dgvDocWorkflow.DataSource = dt;
            SetupDgvDocWorkflow();
        }

        private void LoadStatesFilterCombo()
        {
            DataTable dt = _stateSvc.GetStatesForCombo(_i_project_id);
            DataRow blank = dt.NewRow();
            blank["i_proj_state_id"] = 0;
            blank["n_state_name"]    = "--- ทุกขั้นตอน ---";
            dt.Rows.InsertAt(blank, 0);
            cbbStateFilter.DisplayMember = "n_state_name";
            cbbStateFilter.ValueMember   = "i_proj_state_id";
            cbbStateFilter.DataSource    = dt;
            cbbStateFilter.SelectedIndex = 0;
        }

        private void cbbStateFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            var val = cbbStateFilter.SelectedValue;
            if (val == null || val is DBNull) return;
            if (!(val is int)) return;
            int stateId = (int)val;
            LoadDocWorkflowData(stateId);
        }

        private void SetupDgvDocWorkflow()
        {
            dgvDocWorkflow.Columns.Clear();
            dgvDocWorkflow.AutoGenerateColumns = false;

            var colNo     = new DataGridViewTextBoxColumn { HeaderText = "#",              Width = 40  };
            colNo.DefaultCellStyle.Alignment   = DataGridViewContentAlignment.MiddleCenter;
            colNo.HeaderCell.Style.Alignment   = DataGridViewContentAlignment.MiddleCenter;
            var colState  = new DataGridViewTextBoxColumn { HeaderText = "ขั้นตอน",       DataPropertyName = "n_state_name",  Width = 150 };
            var colName   = new DataGridViewTextBoxColumn { HeaderText = "ชื่อเอกสาร",    DataPropertyName = "n_doc_name",    Width = 200 };
            var colType   = new DataGridViewTextBoxColumn { HeaderText = "ประเภท",        DataPropertyName = "c_doc_type",    Width = 100 };
            var colReq    = new DataGridViewTextBoxColumn { HeaderText = "จำเป็น",        DataPropertyName = "f_required",    Width = 60  };
            var colStatus = new DataGridViewTextBoxColumn { HeaderText = "สถานะ",         DataPropertyName = "c_doc_status",  Width = 100 };
            var colRem    = new DataGridViewTextBoxColumn { HeaderText = "หมายเหตุ",      DataPropertyName = "n_remark",      Width = 150 };

            dgvDocWorkflow.Columns.AddRange(colNo, colState, colName, colType, colReq, colStatus, colRem);
            StyleDgv(dgvDocWorkflow);
        }

        private void btnAddDoc_Click(object sender, EventArgs e)
        {
            if (_i_project_id == 0)
            { MessageBox.Show("กรุณาบันทึกโครงการก่อน", "แจ้งเตือน"); return; }
            var frm = new Frm_Doc_Workflow_Edit(_i_project_id);
            if (frm.ShowDialog() == DialogResult.OK) LoadDocWorkflowData();
        }

        private void btnEditDoc_Click(object sender, EventArgs e)
        {
            var model = GetSelectedDocModel();
            if (model == null) return;
            var frm = new Frm_Doc_Workflow_Edit(_i_project_id, model);
            if (frm.ShowDialog() == DialogResult.OK) LoadDocWorkflowData();
        }

        private void dgvDocWorkflow_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnEditDoc_Click(sender, e);
        }

        private void btnDeleteDoc_Click(object sender, EventArgs e)
        {
            var model = GetSelectedDocModel();
            if (model == null) return;
            if (MessageBox.Show($"ต้องการลบเอกสาร '{model.n_doc_name}' ใช่หรือไม่?",
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string err;
                if (_docSvc.DeleteDocWorkflow(model.i_doc_wf_id, out err))
                    LoadDocWorkflowData();
                else
                    MessageBox.Show(err, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUpdateDocStatus_Click(object sender, EventArgs e)
        {
            var model = GetSelectedDocModel();
            if (model == null) return;

            string[] statuses = { "Pending", "Received", "Verified", "Rejected" };
            string selected = model.c_doc_status ?? "Pending";
            int curIdx = Array.IndexOf(statuses, selected);

            using (var dlg = new Form())
            {
                dlg.Text          = "อัปเดตสถานะเอกสาร";
                dlg.Size          = new Size(300, 150);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox   = false;

                var cbb = new ComboBox { Left = 20, Top = 20, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
                cbb.Items.AddRange(statuses);
                cbb.SelectedIndex = curIdx >= 0 ? curIdx : 0;

                var btn = new Button { Text = "บันทึก", Left = 100, Top = 70, Width = 80 };
                btn.Click += (s2, e2) => {
                    _docSvc.UpdateDocStatus(model.i_doc_wf_id, cbb.SelectedItem.ToString(), _userName);
                    dlg.DialogResult = DialogResult.OK;
                };
                dlg.Controls.AddRange(new Control[] { cbb, btn });
                if (dlg.ShowDialog() == DialogResult.OK) LoadDocWorkflowData();
            }
        }

        private ProjectDocumentWorkflowModel GetSelectedDocModel()
        {
            if (dgvDocWorkflow.CurrentRow == null || dgvDocWorkflow.DataSource == null) return null;
            DataRow r = ((DataTable)dgvDocWorkflow.DataSource).Rows[dgvDocWorkflow.CurrentRow.Index];
            return new ProjectDocumentWorkflowModel
            {
                i_doc_wf_id     = Convert.ToInt32(r["i_doc_wf_id"]),
                i_proj_state_id = Convert.ToInt32(r["i_proj_state_id"]),
                i_project_id    = _i_project_id,
                n_doc_name      = r["n_doc_name"].ToString(),
                c_doc_type      = r["c_doc_type"].ToString(),
                f_required      = r["f_required"].ToString(),
                c_doc_status    = r["c_doc_status"].ToString(),
                n_remark        = r["n_remark"].ToString(),
                i_seq_no        = Convert.ToInt32(r["i_seq_no"]),
                n_state_name    = r["n_state_name"].ToString()
            };
        }

        // ─── Save / Close ─────────────────────────────────────────────────────

        // ─── Tab 5: Members ───────────────────────────────────────────────────

        private void LoadTab5Members()
        {
            if (_i_project_id == 0) return;
            dgvMembers.DataSource = _memberCtrl.GetByProject(_i_project_id);
            SetupDgvMembers();
            _tab5Loaded = true;
        }

        private void SetupDgvMembers()
        {
            dgvMembers.Columns.Clear();
            dgvMembers.AutoGenerateColumns = false;

            var colId1  = new DataGridViewTextBoxColumn { HeaderText = "#",      Width = 50 };
            var colId   = new DataGridViewTextBoxColumn { Name = "i_member_id",   HeaderText = "#",      DataPropertyName = "i_member_id",    Width = 100, Visible = false };
            var colName = new DataGridViewTextBoxColumn { Name = "n_member_name", HeaderText = "ชื่อ",   DataPropertyName = "n_member_name",  Width = 300, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            var colDept = new DataGridViewTextBoxColumn { Name = "department_code", HeaderText = "รหัส", DataPropertyName = "department_code", Width = 150 };
            var colRole = new DataGridViewTextBoxColumn { Name = "c_role",        HeaderText = "บทบาท", DataPropertyName = "c_role",          Width = 180 };

            colRole.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvMembers.Columns.AddRange(colId1, colId, colName, colDept, colRole);
            StyleDgv(dgvMembers);
        }

        private void btnAddMember_Click(object sender, EventArgs e)
        {
            if (_i_project_id == 0)
            {
                MessageBox.Show("กรุณาบันทึกโครงการก่อนเพิ่มบุคคล", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dlg = new Frm_Project_Member(_i_project_id, _userName))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    LoadTab5Members();
            }
        }

        private void btnDeleteMember_Click(object sender, EventArgs e)
        {
            if (dgvMembers.CurrentRow == null) return;
            int memberId = Convert.ToInt32(dgvMembers.CurrentRow.Cells["i_member_id"].Value);
            string name  = dgvMembers.CurrentRow.Cells["n_member_name"].Value?.ToString() ?? "";

            if (MessageBox.Show($"ต้องการลบ '{name}' ออกจากโครงการ?", "ยืนยัน",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_memberCtrl.Delete(memberId))
                    LoadTab5Members();
            }
        }

        // ─── Save / Close ─────────────────────────────────────────────────────

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProjectName.Text))
            {
                MessageBox.Show("กรุณาระบุชื่อโครงการ", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProjectName.Focus();
                return;
            }

            var model = new ProjectModel
            {
                n_project_name = txtProjectName.Text.Trim(),
                c_description  = txtDescription.Text.Trim(),
                d_start_date   = dtpStartDate.Checked ? dtpStartDate.Value : (DateTime?)null,
                d_due_date     = dtpDueDate.Checked   ? dtpDueDate.Value   : (DateTime?)null,
                i_create_user  = _userName,
                i_update_user  = _userName
            };

            // Template
            var tmplVal = cbbTemplate.SelectedValue;
            if (tmplVal != null && !(tmplVal is DBNull) && tmplVal is int)
                model.i_template_id = (int)tmplVal;

            bool success;
            if (_isNew)
            {
                // ─── Validate Project No. ─────────────────────────────────────
                if (chkAutoGenProjectNo.Checked)
                {
                    string genNo = _projectSvc.GenerateProjectNo();
                    if (string.IsNullOrEmpty(genNo))
                    {
                        MessageBox.Show("ไม่สามารถสร้างเลขที่โครงการอัตโนมัติได้ กรุณาลองใหม่อีกครั้ง", "ข้อผิดพลาด",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    model.c_project_no = genNo;
                }
                else
                {
                    string manualNo = txtProjectNo.Text.Trim();
                    if (string.IsNullOrEmpty(manualNo))
                    {
                        MessageBox.Show("กรุณาระบุเลขที่โครงการ", "แจ้งเตือน",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtProjectNo.Focus();
                        return;
                    }
                    model.c_project_no = manualNo;
                }

                // ─── Check duplicate Project No. ──────────────────────────────
                if (_projectSvc.IsProjectNoExists(model.c_project_no))
                {
                    MessageBox.Show($"เลขที่โครงการ '{model.c_project_no}' มีในระบบแล้ว กรุณาระบุเลขที่ใหม่", "แจ้งเตือน",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (!chkAutoGenProjectNo.Checked) txtProjectNo.Focus();
                    return;
                }

                int newId = _projectSvc.CreateProject(model);
                success = newId > 0;
                if (success)
                {
                    MessageBox.Show("สร้างโครงการเรียบร้อยแล้ว เลขที่: " + model.c_project_no, "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                model.i_project_id = _i_project_id;
                success = _projectSvc.UpdateProject(model);
                if (success)
                {
                    MessageBox.Show("บันทึกข้อมูลเรียบร้อย", "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProjectData();
                }
            }

            if (!success)
                MessageBox.Show("บันทึกข้อมูลไม่สำเร็จ กรุณาลองใหม่อีกครั้ง", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnViewTasks_Click(object sender, EventArgs e)
        {
            if (_i_project_id <= 0) return;
            var frm = new Frm_Project_Task_List(_i_project_id);
            frm.ShowDialog(this);
        }

        // ─── Legal Dictionary Tab ─────────────────────────────────────────────

        private void CheckLegalDictTab(bool isLd)
        {
            if (isLd)
            {
                if (_tabLd == null) CreateLegalDictTab();
                if (!tabMain.TabPages.Contains(_tabLd))
                    tabMain.TabPages.Add(_tabLd);
            }
            else if (_tabLd != null && tabMain.TabPages.Contains(_tabLd))
            {
                tabMain.TabPages.Remove(_tabLd);
            }
        }

        private void CreateLegalDictTab()
        {
            _tabLd = new TabPage("📖 พจนานุกรม") { Padding = new Padding(0) };

            // ── Status Bar (Bottom) ─────────────────────────────────────────────
            var pnlStatus = new Panel { Dock = DockStyle.Bottom, Height = 26, BackColor = Color.FromArgb(30, 58, 95) };
            var lblSBVer  = new Label { Text = "Version: 1.0", ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F), AutoSize = true };
            _ldLblSBStatus  = new Label { ForeColor = Color.FromArgb(180, 220, 255), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true };
            _ldLblSBCreated = new Label { ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F), AutoSize = true };
            _ldLblKmsInfo   = new Label { ForeColor = Color.FromArgb(144, 238, 144), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true, Text = "" };
            var lblSBNote   = new Label { Text = "| หมายเหตุ:", ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F), AutoSize = true };
            pnlStatus.Resize += (s, ev) =>
            {
                lblSBVer.Location        = new Point(6, 5);
                _ldLblSBStatus.Location  = new Point(90, 5);
                _ldLblSBCreated.Location = new Point(160, 5);
                _ldLblKmsInfo.Location   = new Point(300, 5);
                lblSBNote.Location       = new Point(pnlStatus.Width - 140, 5);
            };
            pnlStatus.Controls.AddRange(new Control[] { lblSBVer, _ldLblSBStatus, _ldLblSBCreated, _ldLblKmsInfo, lblSBNote });
            LdUpdateStatusBar();

            // ── Action Toolbar ──────────────────────────────────────────────────
            var pnlActions = new Panel
            {
                Dock = DockStyle.Top, Height = 40,
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            var btnSave    = LdMakeBtn("💾 บันทึก",       NavyMedium,                   90);
            var btnReview  = LdMakeBtn("📤 ส่งขอ Review", Color.FromArgb(0, 130, 90),   120);
            var btnPreview = LdMakeBtn("👁 Preview",       Color.FromArgb(80, 100, 140),  80);
            var btnCancel  = LdMakeBtn("✖ ยกเลิก",        DangerRed,                    80);
            var btnSendApi = LdMakeBtn("📡 ส่ง API",       Color.FromArgb(130, 60, 160), 90);
            btnSave.Click    += (s, ev) => LdSave();
            btnReview.Click  += (s, ev) => { if (_ldCbbArtStatus != null && _ldCbbArtStatus.Items.Count > 1) _ldCbbArtStatus.SelectedIndex = 1; };
            btnPreview.Click += (s, ev) => LdShowJsonPreview();
            btnCancel.Click  += (s, ev) => LdClearForm();
            btnSendApi.Click += (s, ev) => LdSendApi();
            int bx = 6;
            foreach (var b in new[] { btnSave, btnReview, btnPreview, btnCancel, btnSendApi })
            {
                b.Location = new Point(bx, 6);
                bx += b.Width + 4;
                pnlActions.Controls.Add(b);
            }

            // ── ข้อมูลทั่วไป section ────────────────────────────────────────────
            const int INFO_H = 252;
            var pnlInfoWrap   = new Panel { Dock = DockStyle.Top, Height = INFO_H };
            var pnlInfoHeader = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Color.FromArgb(30, 58, 95) };
            var lblInfoTitle  = new Label
            {
                Text = "  ข้อมูลทั่วไป (General Information)",
                ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Dock = DockStyle.Left, Width = 340, TextAlign = ContentAlignment.MiddleLeft
            };
            var btnCollapse = new Button
            {
                Text = "▲", Width = 30, Height = 28, Dock = DockStyle.Right,
                BackColor = Color.FromArgb(50, 80, 130), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCollapse.FlatAppearance.BorderSize = 0;
            pnlInfoHeader.Controls.AddRange(new Control[] { btnCollapse, lblInfoTitle });

            var pnlInfoBody = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10, 8, 10, 4) };

            bool infoCollapsed = false;
            btnCollapse.Click += (s, ev) =>
            {
                infoCollapsed         = !infoCollapsed;
                pnlInfoBody.Visible   = !infoCollapsed;
                pnlInfoWrap.Height    = infoCollapsed ? 30 : INFO_H;
                btnCollapse.Text      = infoCollapsed ? "▼" : "▲";
            };

            // ── Row 1: รหัสบทความ (auto) | ประเภทกฎหมาย | หมวดกฎหมาย ────────
            LdAddLbl(pnlInfoBody, "รหัสบทความ", 6, 10);
            _ldTxtCode = new TextBox
            {
                Location = new Point(88, 7), Width = 205, ReadOnly = true,
                BackColor = Color.FromArgb(235, 240, 250),
                Font = new Font("Consolas", 9.5F), Text = "??" + "00" + "0000" + "00" + "00" + "00" + "00" + "000"
            };
            var ttCode = new ToolTip();
            ttCode.SetToolTip(_ldTxtCode,
                "รหัสบทความ 21 หลัก (auto-สร้างจากช่องด้านล่าง)\n" +
                "รูปแบบ: [ประเภท 2][หมวด 2][มาตรา 4][แทรก 2][อนุมาตรา1 2][อนุมาตรา2 2][วรรค 2][หนังสือ 2][ฉบับที่ 3]");

            LdAddLbl(pnlInfoBody, "ประเภทกฎหมาย *", 226, 10);
            _ldCbbLawType = LdAddCbb(pnlInfoBody, 336, 7, 135);
            LdFillLawTypeCombo();
            _ldCbbLawType.SelectedIndexChanged += (s, ev) => { LdFillCategoryCombo(); LdAutoCode(); };

            LdAddLbl(pnlInfoBody, "หมวดกฎหมาย *", 480, 10);
            _ldCbbCatFull = LdAddCbb(pnlInfoBody, 564, 7, 300);
            _ldCbbCatFull.SelectedIndexChanged += (s, ev) => LdAutoCode();
            LdFillCategoryCombo();

            // ── Code component row (labels y=30, inputs y=44) ────────────────────
            _ldTxtSection    = LdNumBox(pnlInfoBody, "มาตรา",      88, 30, 50, 4);
            _ldTxtSlash      = LdNumBox(pnlInfoBody, "แทรก /n",   153, 30, 36, 2);
            _ldTxtSub1       = LdNumBox(pnlInfoBody, "อนุ(1)",    204, 30, 36, 2);
            _ldTxtSub2       = LdNumBox(pnlInfoBody, "อนุ(2)",    255, 30, 36, 2);
            _ldTxtPara       = LdNumBox(pnlInfoBody, "วรรค",      306, 30, 36, 2);
            _ldTxtLetterType = LdNumBox(pnlInfoBody, "ประเภท",    357, 30, 36, 2);
            _ldTxtLetterNo   = LdNumBox(pnlInfoBody, "ฉบับที่",   408, 30, 46, 3);
            foreach (var tb in new[] { _ldTxtSection, _ldTxtSlash, _ldTxtSub1, _ldTxtSub2, _ldTxtPara, _ldTxtLetterType, _ldTxtLetterNo })
                tb.TextChanged += (s, ev) => LdAutoCode();
            pnlInfoBody.Controls.Add(_ldTxtCode);

            // ── Row 2: ชื่อบทความ | ประเภทบทความ | สถานะ | เวอร์ชัน ───────────
            LdAddLbl(pnlInfoBody, "ชื่อบทความ *", 6, 86);
            _ldTxtTitle = new TextBox { Location = new Point(88, 83), Width = 250, Font = new Font("Segoe UI", 9F) };

            LdAddLbl(pnlInfoBody, "ประเภทบทความ *", 346, 86);
            _ldCbbArtType = LdAddCbb(pnlInfoBody, 448, 83, 140);
            _ldCbbArtType.Items.AddRange(new object[] { "1=SOP", "2=FAQ", "3=Guideline", "4=Template", "5=Policy" });
            _ldCbbArtType.SelectedIndex = 0;

            LdAddLbl(pnlInfoBody, "สถานะ *", 596, 86);
            _ldCbbArtStatus = LdAddCbb(pnlInfoBody, 638, 83, 120);
            _ldCbbArtStatus.Items.AddRange(new object[] { "Draft", "Active", "Expired", "Revoked" });
            _ldCbbArtStatus.SelectedIndex = 0;
            _ldCbbArtStatus.SelectedIndexChanged += (s, ev) => LdUpdateStatusBar();

            LdAddLbl(pnlInfoBody, "เวอร์ชัน", 766, 86);
            _ldLblVersion = new Label
            {
                Location = new Point(818, 86), AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95), Text = "v1.0"
            };

            pnlInfoBody.Controls.AddRange(new Control[] { _ldTxtTitle, _ldLblVersion });

            // ── Row 3: ระดับความลับ | แท็ก ──────────────────────────────────────
            LdAddLbl(pnlInfoBody, "ระดับความลับ (CRAF) *", 6, 124);
            _ldCbbConf = LdAddCbb(pnlInfoBody, 138, 121, 150);
            _ldCbbConf.Items.AddRange(new object[] { "0=ไม่ระบุ", "1=HC สูงสุด", "2=SC ลับ", "3=NC ปกติ", "4=PC สาธารณะ" });
            _ldCbbConf.SelectedIndex = 3;

            LdAddLbl(pnlInfoBody, "แท็ก (คั่นด้วย ,)", 298, 124);
            _ldTxtTags = new TextBox { Location = new Point(410, 121), Width = 400, Font = new Font("Segoe UI", 9F) };

            pnlInfoBody.Controls.AddRange(new Control[] { _ldTxtTags });

            // ── Row 4: วันที่มีผล | วันที่ยกเลิก ────────────────────────────────
            LdAddLbl(pnlInfoBody, "วันที่มีผล", 6, 162);
            _ldDtpPublished = new DateTimePicker
            {
                Location = new Point(70, 158), Width = 140,
                Format = DateTimePickerFormat.Short, Value = DateTime.Today,
                Font = new Font("Segoe UI", 9F)
            };

            LdAddLbl(pnlInfoBody, "วันที่ยกเลิก", 220, 162);
            _ldChkExpiry = new CheckBox { Location = new Point(290, 160), Width = 20, Height = 20 };
            _ldDtpExpiry = new DateTimePicker
            {
                Location = new Point(312, 158), Width = 140,
                Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddYears(1),
                Font = new Font("Segoe UI", 9F), Enabled = false
            };
            _ldChkExpiry.CheckedChanged += (s, ev) => _ldDtpExpiry.Enabled = _ldChkExpiry.Checked;

            pnlInfoBody.Controls.AddRange(new Control[] { _ldDtpPublished, _ldChkExpiry, _ldDtpExpiry });

            pnlInfoWrap.Controls.Add(pnlInfoBody);
            pnlInfoWrap.Controls.Add(pnlInfoHeader);

            // ── เนื้อหาบทความ section ───────────────────────────────────────────
            var pnlContent       = new Panel { Dock = DockStyle.Fill };
            var pnlContentHeader = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Color.FromArgb(30, 58, 95) };
            var lblContentTitle  = new Label
            {
                Text = "  เนื้อหาบทความ",
                ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
            };
            pnlContentHeader.Controls.Add(lblContentTitle);

            var pnlContentBody = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10, 6, 10, 6) };

            // Summary row
            var pnlSumRow = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.White };
            var lblSum    = new Label { Text = "บทสรุปสั้น (Summary) :", AutoSize = true, Location = new Point(0, 8), Font = new Font("Segoe UI", 9F) };
            _ldTxtSummary = new TextBox { Location = new Point(152, 5), Font = new Font("Segoe UI", 9F), Height = 22 };
            _ldTxtSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlSumRow.Resize += (s, ev) => _ldTxtSummary.Width = pnlSumRow.Width - 156;
            pnlSumRow.Controls.AddRange(new Control[] { lblSum, _ldTxtSummary });

            // Editor hint bar
            var pnlEditorBar = new Panel { Dock = DockStyle.Top, Height = 26, BackColor = Color.FromArgb(242, 242, 242) };
            var lblEditorHint = new Label
            {
                Text = " 📝 เนื้อหา (ข้อความ/HTML)",
                AutoSize = true, Location = new Point(4, 5),
                ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };
            pnlEditorBar.Controls.Add(lblEditorHint);

            // Char count bar
            var lblCharCount = new Label
            {
                Dock = DockStyle.Bottom, Height = 20, BackColor = Color.FromArgb(245, 245, 245),
                ForeColor = Color.Gray, Font = new Font("Segoe UI", 8F),
                TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 8, 0),
                Text = "จำนวนอักขระ: 0"
            };

            _ldRtbContent = new RichTextBox
            {
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.None, BackColor = Color.White
            };
            _ldRtbContent.TextChanged += (s, ev) =>
                lblCharCount.Text = "จำนวนอักขระ: " + _ldRtbContent.TextLength;

            var pnlRtbWrap = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            pnlRtbWrap.Controls.Add(_ldRtbContent);

            pnlContentBody.Controls.Add(pnlRtbWrap);
            pnlContentBody.Controls.Add(lblCharCount);
            pnlContentBody.Controls.Add(pnlEditorBar);
            pnlContentBody.Controls.Add(pnlSumRow);

            pnlContent.Controls.Add(pnlContentBody);
            pnlContent.Controls.Add(pnlContentHeader);

            _tabLd.Controls.Add(pnlContent);
            _tabLd.Controls.Add(pnlInfoWrap);
            _tabLd.Controls.Add(pnlActions);
            _tabLd.Controls.Add(pnlStatus);
        }

        private static Button LdMakeBtn(string text, Color bg, int width)
        {
            var b = new Button
            {
                Text = text, Width = width, Height = 28, BackColor = bg,
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static void LdAddLbl(Panel parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text, AutoSize = true, Location = new Point(x, y),
                Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(50, 50, 50)
            });
        }

        private static TextBox LdNumBox(Panel parent, string label, int x, int labelY, int w, int maxLen)
        {
            parent.Controls.Add(new Label
            {
                Text = label, Location = new Point(x, labelY), AutoSize = true,
                Font = new Font("Segoe UI", 7.5F), ForeColor = Color.FromArgb(100, 100, 100)
            });
            var tb = new TextBox
            {
                Location = new Point(x, labelY + 13), Width = w, MaxLength = maxLen,
                Font = new Font("Consolas", 9F), TextAlign = HorizontalAlignment.Center
            };
            parent.Controls.Add(tb);
            return tb;
        }

        private static ComboBox LdAddCbb(Panel parent, int x, int y, int width)
        {
            var c = new ComboBox
            {
                Location = new Point(x, y), Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            parent.Controls.Add(c);
            return c;
        }


        private void LdFillLawTypeCombo()
        {
            if (_ldCbbLawType == null) return;
            _ldCbbLawType.Items.Clear();
            _ldLawTypeIdMap.Clear();
            _ldCbbLawType.Items.Add("-- เลือก --");
            try
            {
                DataTable dt = _kmsCtr.GetLawTypes();
                foreach (DataRow r in dt.Rows)
                {
                    int id       = Convert.ToInt32(r["CategoryID"]);
                    string code  = r["CategoryCode"].ToString();
                    string name  = r["CategoryName"].ToString();
                    string label = code + "=" + name;
                    _ldCbbLawType.Items.Add(label);
                    _ldLawTypeIdMap[label] = id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดประเภทกฎหมายไม่ได้:\n" + ex.Message, "DB Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            _ldCbbLawType.SelectedIndex = 0;
        }

        private void LdFillCategoryCombo()
        {
            if (_ldCbbCatFull == null) return;
            _ldCbbCatFull.Items.Clear();
            _ldCatIdMap.Clear();
            _ldCbbCatFull.Items.Add("-- เลือกหมวดกฎหมาย --");

            string selLaw = _ldCbbLawType?.SelectedItem?.ToString() ?? "";
            if (!_ldLawTypeIdMap.TryGetValue(selLaw, out int parentId) || parentId == 0)
            {
                _ldCbbCatFull.SelectedIndex = 0;
                return;
            }

            try
            {
                DataTable dt = _kmsCtr.GetCategoriesByParent(parentId);
                foreach (DataRow r in dt.Rows)
                {
                    int id      = Convert.ToInt32(r["CategoryID"]);
                    string code = r["CategoryCode"].ToString();
                    string name = r["CategoryName"].ToString();
                    string label = code + "=" + name;
                    _ldCbbCatFull.Items.Add(label);
                    _ldCatIdMap[label] = id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดหมวดกฎหมายไม่ได้:\n" + ex.Message, "DB Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _ldCbbCatFull.SelectedIndex = 0;
        }

        // ArticleCode = LawType(2)+CategoryCode(2)+Section(4)+Slash(2)+Sub1(2)+Sub2(2)+Para(2)+LetterType(2)+LetterNo(3) = 21
        private void LdAutoCode()
        {
            if (_ldTxtCode == null) return;

            string selLaw = _ldCbbLawType?.SelectedItem?.ToString() ?? "";
            string lt = selLaw.Length >= 2 && selLaw[2] == '=' ? selLaw.Substring(0, 2) : "??";

            string selCat = _ldCbbCatFull?.SelectedItem?.ToString() ?? "";
            string cc = selCat.Length >= 2 && selCat[2] == '=' ? selCat.Substring(0, 2) : "??";

            string ss  = PadNum(_ldTxtSection?.Text, 4);
            string tt  = PadNum(_ldTxtSlash?.Text, 2);
            string aa  = PadNum(_ldTxtSub1?.Text, 2);
            string bb  = PadNum(_ldTxtSub2?.Text, 2);
            string pp  = PadNum(_ldTxtPara?.Text, 2);
            string hh  = PadNum(_ldTxtLetterType?.Text, 2);
            string nnn = PadNum(_ldTxtLetterNo?.Text, 3);

            _ldTxtCode.Text = lt + cc + ss + tt + aa + bb + pp + hh + nnn;
        }

        private static string PadNum(string v, int len)
        {
            if (string.IsNullOrWhiteSpace(v)) return new string('0', len);
            var digits = new System.Text.StringBuilder();
            foreach (char c in v) if (char.IsDigit(c)) digits.Append(c);
            string s = digits.ToString();
            if (s.Length >= len) return s.Substring(s.Length - len);
            return s.PadLeft(len, '0');
        }

        private static string TrimLeadZeros(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            string t = s.TrimStart('0');
            return string.IsNullOrEmpty(t) ? "" : t;
        }

        private void LdUpdateStatusBar()
        {
            if (_ldLblSBStatus != null)
                _ldLblSBStatus.Text = "| " + (_ldCbbArtStatus?.SelectedItem?.ToString() ?? "Draft") + " |";
            if (_ldLblSBCreated != null)
                _ldLblSBCreated.Text = " สร้างโดย: " + (_userName ?? "-") + " | วันที่: " + DateTime.Now.ToString("dd/MM/yyyy");
            if (_ldLblKmsInfo != null)
            {
                if (_ldKmsArticleId > 0)
                    _ldLblKmsInfo.Text = "| ✅ ส่ง KMS แล้ว | ArticleID: " + _ldKmsArticleId
                                       + (string.IsNullOrEmpty(_ldKmsArticleCode) ? "" : "  Code: " + _ldKmsArticleCode);
                else
                    _ldLblKmsInfo.Text = "";
            }
        }

        private void LdClearForm()
        {
            if (_ldTxtSection    != null) _ldTxtSection.Text    = "";
            if (_ldTxtSlash      != null) _ldTxtSlash.Text      = "";
            if (_ldTxtSub1       != null) _ldTxtSub1.Text       = "";
            if (_ldTxtSub2       != null) _ldTxtSub2.Text       = "";
            if (_ldTxtPara       != null) _ldTxtPara.Text       = "";
            if (_ldTxtLetterType != null) _ldTxtLetterType.Text = "";
            if (_ldTxtLetterNo   != null) _ldTxtLetterNo.Text   = "";
            if (_ldTxtTitle   != null) _ldTxtTitle.Text   = "";
            if (_ldTxtTags    != null) _ldTxtTags.Text    = "";
            if (_ldTxtSummary != null) _ldTxtSummary.Text = "";
            if (_ldRtbContent != null) _ldRtbContent.Text = "";
            if (_ldCbbLawType   != null && _ldCbbLawType.Items.Count   > 0) _ldCbbLawType.SelectedIndex   = 0;
            if (_ldCbbArtType   != null && _ldCbbArtType.Items.Count   > 0) _ldCbbArtType.SelectedIndex   = 0;
            if (_ldCbbArtStatus != null && _ldCbbArtStatus.Items.Count > 0) _ldCbbArtStatus.SelectedIndex = 0;
            if (_ldCbbCatFull   != null && _ldCbbCatFull.Items.Count   > 0) _ldCbbCatFull.SelectedIndex   = 0;
            if (_ldCbbConf      != null && _ldCbbConf.Items.Count      > 3) _ldCbbConf.SelectedIndex      = 3;
            if (_ldChkExpiry    != null) _ldChkExpiry.Checked = false;
            if (_ldDtpPublished != null) _ldDtpPublished.Value = DateTime.Today;
            if (_ldDtpExpiry    != null) _ldDtpExpiry.Value    = DateTime.Today.AddYears(1);
            if (_ldLblVersion   != null) _ldLblVersion.Text    = "v1.0";
            LdUpdateStatusBar();
        }

        private void LdShowJsonPreview()
        {
            ShowJsonDialog(LdBuildJson());
        }

        private static string Esc(object v) =>
            (v ?? "").ToString().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r\n", "\\n").Replace("\n", "\\n");

        private static void ShowJsonDialog(string json)
        {
            var frm = new Form
            {
                Text = "Preview JSON — พจนานุกรมกฎหมาย",
                Width = 720, Height = 540,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false, MaximizeBox = true,
                Font = new Font("Segoe UI", 9F)
            };

            var txt = new TextBox
            {
                Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
                ScrollBars = ScrollBars.Both, WordWrap = false,
                BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.FromArgb(200, 220, 170),
                Font = new Font("Consolas", 9.5F), Text = json
            };

            var pnlBtn  = new Panel { Dock = DockStyle.Bottom, Height = 38 };
            var btnCopy = new Button { Text = "📋 Copy", Width = 90, Height = 28, Location = new Point(8, 5), FlatStyle = FlatStyle.Flat };
            btnCopy.Click += (s, e) => { Clipboard.SetText(json); btnCopy.Text = "✔ Copied!"; };
            var btnClose = new Button { Text = "ปิด", Width = 70, Height = 28, Location = new Point(106, 5), FlatStyle = FlatStyle.Flat };
            btnClose.Click += (s, e) => frm.Close();
            pnlBtn.Controls.AddRange(new Control[] { btnCopy, btnClose });

            frm.Controls.Add(txt);
            frm.Controls.Add(pnlBtn);
            frm.ShowDialog();
        }

        private void LdSendApi()
        {
            // Validate ก่อนส่ง
            string statusSel = _ldCbbArtStatus?.SelectedItem?.ToString() ?? "";
            if (statusSel == "Draft" || statusSel == "")
            {
                MessageBox.Show("กรุณาเปลี่ยนสถานะเป็น Active ก่อนส่ง API\n(Draft ไม่สามารถส่งได้)",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string typeSel = _ldCbbArtType?.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(typeSel) || typeSel.StartsWith("--"))
            {
                MessageBox.Show("กรุณาเลือกประเภทบทความก่อนส่ง API", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string LOGIN_URL  = "http://150.1.8.29:9098/api/Auth/login";
            const string ARTICLE_URL = "http://150.1.8.29:9098/api/KBArticle";
            const string LOGIN_BODY =
                "{\"businessLineCode\":\"RGTM\",\"businessLineName\":\"RGTM\"," +
                "\"userCode\":\"RGTM\",\"username\":\"RGPROJECTTM\",\"password\":\"P@ssw0rd\"}";

            try
            {
                // Step 1: Login → accessToken
                _ldLblSBStatus?.GetType(); // keep alive
                string loginResp = LdHttpPost(LOGIN_URL, LOGIN_BODY, null);
                string token     = LdJsonGet(loginResp, "accessToken");

                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Login ไม่สำเร็จ: ไม่พบ accessToken\n\n" + loginResp,
                        "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Step 2: POST JSON → KBArticle
                string payload  = LdBuildApiPayload();
                string apiResp  = LdHttpPost(ARTICLE_URL, payload, token);

                // parse response: { "success": true, "data": { "articleID": 0, "articleCode": "...", ... } }
                try
                {
                    var respObj = Newtonsoft.Json.Linq.JObject.Parse(apiResp);
                    bool success = respObj.Value<bool>("success");
                    if (success)
                    {
                        var data = respObj["data"] as Newtonsoft.Json.Linq.JObject;
                        int articleId      = data != null ? data.Value<int>("articleID")         : 0;
                        string articleCode = data != null ? data.Value<string>("articleCode") ?? "" : "";
                        if (articleId > 0)
                        {
                            _ldKmsArticleId   = articleId;
                            _ldKmsArticleCode = articleCode;
                            LdUpdateStatusBar();
                            // บันทึก JSON (พร้อม _kmsArticleId) กลับลง DB
                            if (_i_project_id > 0)
                            {
                                string saveJson = LdBuildJson();
                                _projectSvc.UpdateLegalDictJson(_i_project_id, saveJson, _userName ?? "");
                            }
                        }
                    }
                }
                catch { /* parse failed — still show dialog */ }

                ShowJsonDialog("✅ API Response\n\n" + apiResp);
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด:\n" + ex.Message,
                    "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string LdHttpPost(string url, string jsonBody, string bearerToken)
        {
            var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            req.Method      = "POST";
            req.ContentType = "application/json; charset=utf-8";
            req.Accept      = "application/json";
            req.Timeout     = 30000;
            if (!string.IsNullOrEmpty(bearerToken))
                req.Headers["Authorization"] = "Bearer " + bearerToken;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            req.ContentLength = data.Length;
            using (var s = req.GetRequestStream()) s.Write(data, 0, data.Length);

            try
            {
                using (var resp = (System.Net.HttpWebResponse)req.GetResponse())
                using (var rd   = new System.IO.StreamReader(resp.GetResponseStream(), System.Text.Encoding.UTF8))
                    return rd.ReadToEnd();
            }
            catch (System.Net.WebException ex) when (ex.Response != null)
            {
                using (var rd = new System.IO.StreamReader(ex.Response.GetResponseStream(), System.Text.Encoding.UTF8))
                    throw new Exception(((System.Net.HttpWebResponse)ex.Response).StatusCode + " — " + rd.ReadToEnd());
            }
        }

        private static string LdJsonGet(string json, string key)
        {
            int ki = json.IndexOf("\"" + key + "\":");
            if (ki < 0) return "";
            int after = ki + key.Length + 3;
            if (after >= json.Length) return "";
            if (json[after] == '"')
            {
                int ve = json.IndexOf("\"", after + 1);
                return ve < 0 ? "" : json.Substring(after + 1, ve - after - 1);
            }
            int end = json.IndexOfAny(new[] { ',', '}', '\n' }, after);
            return end < 0 ? json.Substring(after) : json.Substring(after, end - after).Trim();
        }

        private void LdSave()
        {
            if (_i_project_id <= 0)
            {
                MessageBox.Show("กรุณาบันทึกโครงการก่อน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string json = LdBuildJson();
            bool ok = _projectSvc.UpdateLegalDictJson(_i_project_id, json, _userName);
            if (ok)
                MessageBox.Show("บันทึกพจนานุกรมเรียบร้อย", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("บันทึกไม่สำเร็จ", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private string LdBuildJson()
        {
            string typeSel   = _ldCbbArtType?.SelectedItem?.ToString()   ?? "1=SOP";
            string statusSel = _ldCbbArtStatus?.SelectedItem?.ToString() ?? "Draft";
            string catSel    = _ldCbbCatFull?.SelectedItem?.ToString()   ?? "";
            string confSel   = _ldCbbConf?.SelectedItem?.ToString()      ?? "3=NC ปกติ";

            int  typeCode   = typeSel.Length > 0 && char.IsDigit(typeSel[0]) ? (int)char.GetNumericValue(typeSel[0]) : 6;
            int  statusCode = statusSel == "Active" ? 1 : statusSel == "Expired" ? 2 : statusSel == "Revoked" ? 3 : 0;
            int  catId      = _ldCatIdMap.TryGetValue(catSel, out int cid) ? cid : 0;
            int  confCode   = confSel.Length > 0 && char.IsDigit(confSel[0]) ? (int)char.GetNumericValue(confSel[0]) : 3;
            string userName = _userName ?? "unknown";
            string iso8601(DateTime dt) => dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            string pubDate   = _ldDtpPublished != null ? iso8601(_ldDtpPublished.Value) : iso8601(DateTime.Today);
            string expDate   = (_ldChkExpiry?.Checked == true && _ldDtpExpiry != null)
                               ? iso8601(_ldDtpExpiry.Value) : iso8601(DateTime.Today.AddYears(1));
            string now       = iso8601(DateTime.Now);

            // helper สำหรับ string field (ใส่ quote และ escape)
            string Str(string v) => "\"" + Esc(v) + "\"";

            var sb = new StringBuilder();
            sb.AppendLine("{");
            //sb.AppendLine("  \"request\": {");
            sb.AppendLine("    \"articleCode\": "          + Str(_ldTxtCode?.Text)    + ",");
            sb.AppendLine("    \"title\": "                + Str(_ldTxtTitle?.Text)   + ",");
            sb.AppendLine("    \"contentHTML\": "          + Str(_ldRtbContent?.Text) + ",");
            sb.AppendLine("    \"summary\": "              + Str(_ldTxtSummary?.Text) + ",");
            sb.AppendLine("    \"categoryID\": "           + catId                    + ",");
            sb.AppendLine("    \"articleType\": "          + typeCode                 + ",");
            sb.AppendLine("    \"status\": "               + statusCode               + ",");
            sb.AppendLine("    \"confidentialityLevel\": " + confCode                 + ",");
            sb.AppendLine("    \"currentVersion\": 1,");
            sb.AppendLine("    \"publishedDate\": "        + Str(pubDate)             + ",");
            sb.AppendLine("    \"expiryDate\": "           + Str(expDate)             + ",");
            sb.AppendLine("    \"reviewDueDate\": null,");
            sb.AppendLine("    \"viewCount\": 0,");
            sb.AppendLine("    \"likeCount\": 0,");
            sb.AppendLine("    \"isOfficialPolicy\": false,");
            sb.AppendLine("    \"tags\": "                 + Str(_ldTxtTags?.Text)    + ",");
            sb.AppendLine("    \"reviewedBy\": " + Str(userName) + ",");
            sb.AppendLine("    \"reviewedDate\": " + Str(now) + ",");

            // articleLinks — สมาชิกโครงการ (เฉพาะที่มี department_code)
            sb.AppendLine("    \"articleLinks\": [");
            try
            {
                DataTable members = _i_project_id > 0 ? _memberCtrl.GetByProject(_i_project_id) : null;
                if (members != null && members.Rows.Count > 0)
                {
                    var links = new System.Collections.Generic.List<string>();
                    foreach (DataRow mr in members.Rows)
                    {
                        string deptCode = mr["department_code"]?.ToString() ?? "";
                        if (string.IsNullOrWhiteSpace(deptCode)) continue;
                        links.Add(
                            "      {\r\n" +
                            "        \"entityType\": 6,\r\n" +
                            "        \"entityID\": \"" + Esc(deptCode) + "\",\r\n" +
                            "        \"createdDate\": \"" + now + "\"\r\n" +
                            "      }");
                    }
                    sb.AppendLine(string.Join(",\r\n", links));
                }
            }
            catch { /* ถ้า DB ไม่พร้อม ส่ง [] */ }

            sb.AppendLine("    ]");
            //sb.AppendLine("  }");
            // extra storage fields (ignored by API — saved only to n_legal_dict_json)
            if (_ldKmsArticleId > 0)
            {
                sb.AppendLine("    ,");
                sb.AppendLine("    \"_kmsArticleId\": " + _ldKmsArticleId + ",");
                sb.AppendLine("    \"_kmsArticleCode\": " + Str(_ldKmsArticleCode) + ",");
                sb.AppendLine("    \"_kmsSent\": \"Y\"");
            }
            sb.Append(    "}");
            return sb.ToString();
        }

        // ส่ง API จะ build payload ใหม่ที่ไม่มี _kms* fields
        private string LdBuildApiPayload()
        {
            int savedId   = _ldKmsArticleId;
            string savedCode = _ldKmsArticleCode;
            _ldKmsArticleId   = 0;
            _ldKmsArticleCode = null;
            string payload = LdBuildJson();
            _ldKmsArticleId   = savedId;
            _ldKmsArticleCode = savedCode;
            return payload;
        }

        private void LdLoadJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return;
            try
            {
                json = json.Trim();
                // support both array [{}] and object {} format
                if (json.StartsWith("["))
                {
                    var arr = Newtonsoft.Json.Linq.JArray.Parse(json);
                    if (arr.Count == 0) return;
                    LdApplyJObject((Newtonsoft.Json.Linq.JObject)arr[0]);
                }
                else
                {
                    LdApplyJObject(Newtonsoft.Json.Linq.JObject.Parse(json));
                }
            }
            catch { }
        }

        private void LdApplyJObject(Newtonsoft.Json.Linq.JObject o)
        {
            string Get(string camel, string pascal = "")
            {
                var token = o[camel] ?? (pascal != "" ? o[pascal] : null);
                return token == null || token.Type == Newtonsoft.Json.Linq.JTokenType.Null
                    ? "" : token.ToString();
            }

            string artCode = Get("articleCode", "ArticleCode");
            if (_ldTxtCode != null) _ldTxtCode.Text = artCode;
            if (artCode.Length == 21)
            {
                if (_ldTxtSection    != null) _ldTxtSection.Text    = TrimLeadZeros(artCode.Substring(4, 4));
                if (_ldTxtSlash      != null) _ldTxtSlash.Text      = TrimLeadZeros(artCode.Substring(8, 2));
                if (_ldTxtSub1       != null) _ldTxtSub1.Text       = TrimLeadZeros(artCode.Substring(10, 2));
                if (_ldTxtSub2       != null) _ldTxtSub2.Text       = TrimLeadZeros(artCode.Substring(12, 2));
                if (_ldTxtPara       != null) _ldTxtPara.Text       = TrimLeadZeros(artCode.Substring(14, 2));
                if (_ldTxtLetterType != null) _ldTxtLetterType.Text = TrimLeadZeros(artCode.Substring(16, 2));
                if (_ldTxtLetterNo   != null) _ldTxtLetterNo.Text   = TrimLeadZeros(artCode.Substring(18, 3));
            }

            if (_ldTxtTitle   != null) _ldTxtTitle.Text   = Get("title",       "Title");
            if (_ldTxtTags    != null) _ldTxtTags.Text    = Get("tags",        "Tags");
            if (_ldTxtSummary != null) _ldTxtSummary.Text = Get("summary",     "Summary");
            if (_ldRtbContent != null) _ldRtbContent.Text = Get("contentHTML", "Content");

            // LawType จาก articleCode prefix
            string lawType = artCode.Length >= 2 ? artCode.Substring(0, 2) : Get("LawType");
            if (_ldCbbLawType != null && !string.IsNullOrEmpty(lawType))
                for (int i = 0; i < _ldCbbLawType.Items.Count; i++)
                    if (_ldCbbLawType.Items[i].ToString().StartsWith(lawType + "="))
                    { _ldCbbLawType.SelectedIndex = i; break; }

            string typeCode = Get("articleType", "ArticleType");
            if (_ldCbbArtType != null && !string.IsNullOrEmpty(typeCode))
                for (int i = 0; i < _ldCbbArtType.Items.Count; i++)
                    if (_ldCbbArtType.Items[i].ToString().StartsWith(typeCode + "="))
                    { _ldCbbArtType.SelectedIndex = i; break; }

            string statusVal = Get("status", "StatusName");
            if (_ldCbbArtStatus != null && !string.IsNullOrEmpty(statusVal))
            {
                string statusName = statusVal == "1" ? "Active"
                                  : statusVal == "2" ? "Expired"
                                  : statusVal == "3" ? "Revoked" : statusVal;
                int si = _ldCbbArtStatus.FindStringExact(statusName);
                if (si >= 0) _ldCbbArtStatus.SelectedIndex = si;
            }

            // restore LawType แล้วโหลด Category ก่อน จึงเลือก category ที่ถูก
            string catIdStr = Get("categoryID", "CategoryCode");
            if (int.TryParse(catIdStr, out int savedCatId) && savedCatId > 0)
            {
                // หา LawType ที่มี child == savedCatId โดยลอง load ทีละ parent
                bool catSet = false;
                foreach (var kv in _ldLawTypeIdMap)
                {
                    // เลือก LawType ใน combobox
                    int li = _ldCbbLawType?.FindStringExact(kv.Key) ?? -1;
                    if (li < 0) continue;
                    if (_ldCbbLawType != null) _ldCbbLawType.SelectedIndex = li;
                    // LdFillCategoryCombo จะถูกเรียกผ่าน event แต่ trigger manually ด้วย
                    LdFillCategoryCombo();
                    // ตรวจว่ามี savedCatId ใน _ldCatIdMap ไหม
                    foreach (var ckv in _ldCatIdMap)
                        if (ckv.Value == savedCatId)
                        {
                            int ci = _ldCbbCatFull?.FindStringExact(ckv.Key) ?? -1;
                            if (ci >= 0 && _ldCbbCatFull != null) _ldCbbCatFull.SelectedIndex = ci;
                            catSet = true;
                            break;
                        }
                    if (catSet) break;
                }
            }
            else
            {
                // legacy: CategoryCode prefix เช่น "01"
                string catPrefix = catIdStr.Length > 0 ? catIdStr.PadLeft(2, '0') : "";
                if (_ldCbbCatFull != null && !string.IsNullOrEmpty(catPrefix))
                    for (int i = 0; i < _ldCbbCatFull.Items.Count; i++)
                        if (_ldCbbCatFull.Items[i].ToString().StartsWith(catPrefix + "="))
                        { _ldCbbCatFull.SelectedIndex = i; break; }
            }

            string confLevel = Get("confidentialityLevel", "ConfidentialityLevel");
            if (_ldCbbConf != null && !string.IsNullOrEmpty(confLevel))
                for (int i = 0; i < _ldCbbConf.Items.Count; i++)
                    if (_ldCbbConf.Items[i].ToString().StartsWith(confLevel + "="))
                    { _ldCbbConf.SelectedIndex = i; break; }

            string pubDate = Get("publishedDate", "PublishedDate");
            if (_ldDtpPublished != null && DateTime.TryParse(pubDate, out DateTime pub))
                _ldDtpPublished.Value = pub;

            string expDate = Get("expiryDate", "ExpiryDate");
            if (!string.IsNullOrEmpty(expDate) && DateTime.TryParse(expDate, out DateTime exp))
            {
                if (_ldChkExpiry != null) _ldChkExpiry.Checked = true;
                if (_ldDtpExpiry != null) _ldDtpExpiry.Value = exp;
            }

            // restore KMS article ID (storage-only fields)
            string kmsIdStr = Get("_kmsArticleId");
            if (int.TryParse(kmsIdStr, out int kmsId) && kmsId > 0)
            {
                _ldKmsArticleId   = kmsId;
                _ldKmsArticleCode = Get("_kmsArticleCode");
            }

            LdUpdateStatusBar();
        }

        private static List<string> LdSplitJsonObjects(string s)
        {
            var list  = new List<string>();
            int depth = 0, start = -1;
            for (int i = 0; i < s.Length; i++)
            {
                if      (s[i] == '{') { if (depth == 0) start = i; depth++; }
                else if (s[i] == '}') { depth--; if (depth == 0 && start >= 0) list.Add(s.Substring(start, i - start + 1)); }
            }
            return list;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private void StyleDgv(DataGridView dgv)
        {
            // ── Column Header (Office 2016 Navy) ──────────────────────────────────
            var hStyle = dgv.ColumnHeadersDefaultCellStyle;
            hStyle.BackColor  = Color.FromArgb(30, 58, 95);   // NavyDark
            hStyle.ForeColor  = Color.White;
            hStyle.Font       = new Font("Segoe UI", 9F, FontStyle.Bold);
            hStyle.Padding    = new Padding(6, 0, 0, 0);
            hStyle.Alignment  = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight         = 32;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.EnableHeadersVisualStyles   = false;

            // ── Cell Default Style ────────────────────────────────────────────────
            var dStyle = dgv.DefaultCellStyle;
            dStyle.Font               = new Font("Segoe UI", 9F);
            dStyle.ForeColor          = Color.FromArgb(42, 57, 80);
            dStyle.BackColor          = Color.White;
            dStyle.SelectionBackColor = Color.FromArgb(190, 215, 255);
            dStyle.SelectionForeColor = Color.FromArgb(20,  40,  80);
            dStyle.Padding            = new Padding(4, 0, 0, 0);

            var altStyle = dgv.AlternatingRowsDefaultCellStyle;
            altStyle.BackColor = Color.FromArgb(236, 244, 255);
            altStyle.ForeColor = Color.FromArgb(42, 57, 80);

            // ── Grid Appearance ───────────────────────────────────────────────────
            dgv.GridColor         = Color.FromArgb(208, 220, 240);
            dgv.CellBorderStyle   = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.BackgroundColor   = Color.White;
            dgv.RowHeadersVisible = false;
            dgv.RowTemplate.Height = 28;

            // ── Behavior ──────────────────────────────────────────────────────────
            dgv.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly              = true;
            dgv.AllowUserToAddRows    = false;
            dgv.AllowUserToDeleteRows = false;

            // ── Row Numbers ───────────────────────────────────────────────────────
            dgv.RowPostPaint += (s, e) =>
            {
                string rn = (e.RowIndex + 1).ToString();
                SizeF  sz = e.Graphics.MeasureString(rn, dgv.Font);
                using (var brush = new SolidBrush(Color.FromArgb(120, 145, 185)))
                    e.Graphics.DrawString(rn, dgv.Font, brush,
                        e.RowBounds.Location.X + 3,
                        e.RowBounds.Location.Y + (e.RowBounds.Height - sz.Height) / 2);
            };
        }
    }
}
