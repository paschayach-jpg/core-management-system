using RG_Project.Login;
using RG_Project.Project.Controllers;
using RG_Project.Project.Models;
using RG_Project.Project.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace RG_Project.Project.Forms
{
    /// <summary>
    /// Task Management Dashboard
    ///   - แสดง Task หลัก + Sub Task (expand/collapse)
    ///   - Filter ตาม Project / Status / Assignee / Keyword
    ///   - Kanban View
    ///   - Stats Bar (summary chips)
    /// </summary>
    public partial class Frm_Task_Dashboard : Form
    {
        // ── Services ────────────────────────────────────────────────────────────
        private readonly ProjectTaskService    _taskSvc;
        private readonly ProjectStateService   _stateSvc;
        private readonly ProjectTaskController _taskCtrl;
        private readonly string                _userName;

        // ── Navy palette ────────────────────────────────────────────────────────
        private static readonly Color NavyDark    = Color.FromArgb(30,  58,  95);
        private static readonly Color NavyMedium  = Color.FromArgb(43,  87, 154);
        private static readonly Color NavyMediumH = Color.FromArgb(62, 110, 183);
        private static readonly Color NavyLight   = Color.FromArgb(70, 110, 165);
        private static readonly Color NavyLightH  = Color.FromArgb(90, 130, 185);
        private static readonly Color SlateBlue   = Color.FromArgb(100, 130, 168);
        private static readonly Color SlateBlueH  = Color.FromArgb(120, 150, 190);
        private static readonly Color DangerRed   = Color.FromArgb(196,  53,  53);
        private static readonly Color DangerRedH  = Color.FromArgb(218,  73,  73);
        private static readonly Color PurpleCol   = Color.FromArgb(111, 66, 193);
        private static readonly Color GoldAccent  = Color.FromArgb(255, 185, 0);

        private static readonly (string Status, string Label, Color Color)[] _statusCols =
        {
            ("Todo",       "รอดำเนินการ", Color.FromArgb(108, 117, 125)),
            ("InProgress", "กำลังทำ",      Color.FromArgb(255, 165, 0)),
            ("InReview",   "รอตรวจสอบ",    Color.FromArgb(111, 66, 193)),
            ("Done",       "เสร็จแล้ว",     Color.FromArgb(40, 167, 69)),
            ("Cancelled",  "ยกเลิก",        Color.FromArgb(220, 53, 69)),
        };

        // ── Data state ──────────────────────────────────────────────────────────
        private int       _currentProjectId = -1;
        private List<DataRow>               _allRows     = new List<DataRow>();
        private Dictionary<int,List<DataRow>> _childrenMap = new Dictionary<int,List<DataRow>>();
        private HashSet<int>                _expandedIds = new HashSet<int>();
        private DataTable                   _visibleRows;

        // stat labels (created at runtime)
        private Label[] _statLabels = new Label[6];

        // kanban
        private Panel _kanbanPanel;

        // ── Split layout + detail panel ─────────────────────────────────────────
        private SplitContainer _splitMain;
        private Panel          _pnlDetail;
        private Label          _detCode, _detTitle, _detStateLbl;
        private ComboBox       _detStatus, _detPriority;
        private TrackBar       _detPctBar;
        private Label          _detPctLbl;
        private CheckBox       _detDueChk;
        private DateTimePicker _detDue;
        private TextBox        _detAssignee, _detDesc;
        private Panel          _detSubsPanel;
        private Button         _btnDetSave, _btnDetEdit, _btnDetAddSub;
        private int            _detTaskId   = -1;
        private int            _detDepth    = 0;

        // ── Constructor ─────────────────────────────────────────────────────────
        public Frm_Task_Dashboard()
        {
            InitializeComponent();
            _taskSvc  = new ProjectTaskService();
            _stateSvc = new ProjectStateService();
            _taskCtrl = new ProjectTaskController();
            _userName = LoginManager.Instance.GetUserName;
        }

        // ────────────────────────────────────────────────────────────────────────
        // FORM LOAD
        // ────────────────────────────────────────────────────────────────────────
        private void Frm_Task_Dashboard_Load(object sender, EventArgs e)
        {
            BuildStatChips();
            SetupDgvTasks();
            SetupKanbanTab();
            SetupSplitLayout();
            LoadProjectCombo();
            LoadStatusFilter();
            ApplyHovers();
        }

        // ────────────────────────────────────────────────────────────────────────
        // STAT CHIPS
        // ────────────────────────────────────────────────────────────────────────
        private void BuildStatChips()
        {
            pnlStats.Controls.Clear();
            string[] titles  = { "ทั้งหมด", "รอทำ", "กำลังทำ", "รอตรวจ", "เสร็จแล้ว", "เกินกำหนด" };
            Color[]  bgColors = {
                Color.FromArgb(43, 87, 154),
                Color.FromArgb(108, 117, 125),
                Color.FromArgb(255, 165, 0),
                Color.FromArgb(111, 66, 193),
                Color.FromArgb(40, 167, 69),
                Color.FromArgb(196, 53, 53)
            };

            int chipW = 160;
            int gap   = 10;
            int x     = 12;

            for (int i = 0; i < titles.Length; i++)
            {
                var chip = new Panel
                {
                    Location  = new Point(x, 8),
                    Size      = new Size(chipW, 54),
                    BackColor = bgColors[i],
                    Cursor    = Cursors.Default
                };

                var lblTitleChip = new Label
                {
                    Text      = titles[i],
                    Dock      = DockStyle.Top,
                    Height    = 22,
                    ForeColor = Color.FromArgb(200, 230, 255),
                    Font      = new Font("Segoe UI", 8.5F),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                var lblVal = new Label
                {
                    Text      = "0",
                    Dock      = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 16F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Name      = "valLabel"
                };
                chip.Controls.Add(lblVal);
                chip.Controls.Add(lblTitleChip);
                _statLabels[i] = lblVal;

                pnlStats.Controls.Add(chip);
                x += chipW + gap;
            }
        }

        private void UpdateStats()
        {
            if (_allRows == null || _allRows.Count == 0)
            {
                foreach (var l in _statLabels) if (l != null) l.Text = "0";
                return;
            }
            // นับเฉพาะ root tasks (ไม่มี parent)
            var rows = _allRows.Where(r => r["i_parent_task_id"] == DBNull.Value).ToList();
            int total   = rows.Count;
            int todo    = rows.Count(r => r["c_status"].ToString() == "Todo");
            int inprog  = rows.Count(r => r["c_status"].ToString() == "InProgress");
            int inrev   = rows.Count(r => r["c_status"].ToString() == "InReview");
            int done    = rows.Count(r => r["c_status"].ToString() == "Done");
            int overdue = rows.Count(r => Convert.ToInt32(r["b_overdue"]) == 1);

            _statLabels[0].Text = total.ToString();
            _statLabels[1].Text = todo.ToString();
            _statLabels[2].Text = inprog.ToString();
            _statLabels[3].Text = inrev.ToString();
            _statLabels[4].Text = done.ToString();
            _statLabels[5].Text = overdue.ToString();
        }

        // ────────────────────────────────────────────────────────────────────────
        // LOAD DATA
        // ────────────────────────────────────────────────────────────────────────
        private void LoadProjectCombo()
        {
            try
            {
                var conn = new RG_Project.Controllers.ConectionDB();
                conn.Connect('c');
                string sql = @"
                    SELECT i_project_id,
                           c_project_no + ' — ' + n_project_name AS display_name
                    FROM t_project
                    WHERE f_active = 'Y'
                    ORDER BY d_create_date DESC";
                var dt = new DataTable();
                using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn.connection))
                using (var r   = cmd.ExecuteReader()) dt.Load(r);
                conn.Disconnect();

                foreach (DataColumn col in dt.Columns) col.AllowDBNull = true;
                DataRow blank = dt.NewRow();
                blank["i_project_id"] = DBNull.Value;
                blank["display_name"] = "--- เลือกโครงการ ---";
                dt.Rows.InsertAt(blank, 0);

                cbbProject.DisplayMember = "display_name";
                cbbProject.ValueMember   = "i_project_id";
                cbbProject.DataSource    = dt;
                cbbProject.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดโครงการไม่สำเร็จ: " + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStatusFilter()
        {
            cbbStatusFilter.Items.Clear();
            cbbStatusFilter.Items.Add("--- ทุกสถานะ ---");
            foreach (var s in _statusCols) cbbStatusFilter.Items.Add(s.Status);
            cbbStatusFilter.SelectedIndex = 0;
        }

        private void LoadAssigneeFilter()
        {
            cbbAssignFilter.Items.Clear();
            cbbAssignFilter.Items.Add("--- ทุกคน ---");
            if (_allRows == null) return;
            var names = _allRows
                .Where(r => r["n_assignee"] != DBNull.Value && !string.IsNullOrEmpty(r["n_assignee"].ToString()))
                .Select(r => r["n_assignee"].ToString())
                .Distinct()
                .OrderBy(n => n)
                .ToList();
            foreach (var n in names) cbbAssignFilter.Items.Add(n);
            cbbAssignFilter.SelectedIndex = 0;
        }

        private void LoadData()
        {
            if (_currentProjectId < 0)
            {
                _allRows.Clear();
                _childrenMap.Clear();
                _expandedIds.Clear();
                dgvTasks.DataSource = null;
                HideDetailPanel();
                UpdateStats();
                RefreshKanban();
                return;
            }
            try
            {
                this.Cursor = Cursors.WaitCursor;
                var dt = _taskSvc.GetAllTasksForProject(_currentProjectId);

                // สร้าง flat list ตาม sort_path (CTE คืนมาเรียงอยู่แล้ว)
                _allRows = dt.Rows.Cast<DataRow>().ToList();

                // สร้าง childrenMap
                _childrenMap.Clear();
                foreach (var row in _allRows)
                {
                    if (row["i_parent_task_id"] != DBNull.Value)
                    {
                        int pid = Convert.ToInt32(row["i_parent_task_id"]);
                        if (!_childrenMap.ContainsKey(pid))
                            _childrenMap[pid] = new List<DataRow>();
                        _childrenMap[pid].Add(row);
                    }
                }

                LoadAssigneeFilter();
                UpdateStats();
                RefreshGrid();
                RefreshKanban();
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลด Task ไม่สำเร็จ: " + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { this.Cursor = Cursors.Arrow; }
        }

        // ────────────────────────────────────────────────────────────────────────
        // GRID SETUP & REFRESH
        // ────────────────────────────────────────────────────────────────────────
        private void SetupDgvTasks()
        {
            dgvTasks.AutoGenerateColumns = false;
            dgvTasks.Columns.Clear();

            var colExpand = new DataGridViewTextBoxColumn
            { Name = "col_expand", HeaderText = "", Width = 28, ReadOnly = true };
            colExpand.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colExpand.DefaultCellStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);

            var colCode  = new DataGridViewTextBoxColumn { Name = "col_code",  DataPropertyName = "c_task_code",    HeaderText = "รหัส Task",    Width = 110 };
            var colTitle = new DataGridViewTextBoxColumn { Name = "col_title", DataPropertyName = "n_title",         HeaderText = "หัวข้องาน",   AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            var colState = new DataGridViewTextBoxColumn { Name = "col_state", DataPropertyName = "n_state_name",   HeaderText = "ขั้นตอน",     Width = 140 };
            var colAsgn  = new DataGridViewTextBoxColumn { Name = "col_asgn",  DataPropertyName = "n_assignee",     HeaderText = "ผู้รับผิดชอบ",Width = 130 };
            var colPrio  = new DataGridViewTextBoxColumn { Name = "col_prio",  DataPropertyName = "i_priority",      HeaderText = "สำคัญ",       Width = 80  };
            var colStat  = new DataGridViewTextBoxColumn { Name = "col_stat",  DataPropertyName = "c_status",       HeaderText = "สถานะ",        Width = 108 };
            var colPct   = new DataGridViewProgressBarColumn { Name = "col_pct", DataPropertyName = "i_pct_complete", HeaderText = "% เสร็จ",   Width = 80  };
            var colDue   = new DataGridViewTextBoxColumn { Name = "col_due",   DataPropertyName = "d_due_date",     HeaderText = "กำหนดเสร็จ",  Width = 100 };
            var colId      = new DataGridViewTextBoxColumn { Name = "col_id",       DataPropertyName = "i_task_id",        Visible = false };
            var colStateId = new DataGridViewTextBoxColumn { Name = "col_state_id", DataPropertyName = "i_proj_state_id",  Visible = false };
            var colParent  = new DataGridViewTextBoxColumn { Name = "col_parent",   DataPropertyName = "i_parent_task_id", Visible = false };
            var colIsSub   = new DataGridViewTextBoxColumn { Name = "col_is_sub",   DataPropertyName = "_is_sub",          Visible = false };
            var colDepth   = new DataGridViewTextBoxColumn { Name = "col_depth",    DataPropertyName = "_depth",           Visible = false };

            colPrio.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colStat.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDue.DefaultCellStyle.Format     = "dd/MM/yyyy";
            colDue.DefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleCenter;

            dgvTasks.Columns.AddRange(colExpand, colCode, colTitle, colState, colAsgn, colPrio, colStat, colPct, colDue,
                                      colId, colStateId, colParent, colIsSub, colDepth);

            var h = dgvTasks.ColumnHeadersDefaultCellStyle;
            h.BackColor = NavyDark; h.ForeColor = Color.White;
            h.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvTasks.ColumnHeadersHeight         = 32;
            dgvTasks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvTasks.EnableHeadersVisualStyles   = false;

            var d = dgvTasks.DefaultCellStyle;
            d.Font = new Font("Segoe UI", 9F);
            d.ForeColor = Color.FromArgb(42, 57, 80);
            d.BackColor = Color.White;
            d.SelectionBackColor = Color.FromArgb(190, 215, 255);
            d.SelectionForeColor = Color.FromArgb(20, 40, 80);
            d.Padding = new Padding(4, 0, 0, 0);

            dgvTasks.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 244, 255);
            dgvTasks.GridColor       = Color.FromArgb(208, 220, 240);
            dgvTasks.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvTasks.RowTemplate.Height = 30;
        }

        private void RefreshGrid()
        {
            _visibleRows = BuildVisibleRows();
            dgvTasks.DataSource = _visibleRows;
            EnableButtons(false);
        }

        private DataTable BuildVisibleRows()
        {
            var result = CreateVisibleSchema();
            if (_allRows == null || _allRows.Count == 0) return result;

            string filterStatus   = cbbStatusFilter.SelectedIndex > 0 ? cbbStatusFilter.SelectedItem.ToString() : "";
            string filterAssignee = cbbAssignFilter.SelectedIndex  > 0 ? cbbAssignFilter.SelectedItem.ToString() : "";
            string filterKeyword  = txtSearch.Text.Trim().ToLower();

            // ชุด taskId ที่ "มองเห็น" เพื่อตัดสินว่า children จะถูกแสดงหรือไม่
            var visibleSet = new HashSet<int>();

            // _allRows เรียงตาม sort_path จาก CTE → ลำดับ tree ถูกต้องอยู่แล้ว
            foreach (DataRow row in _allRows)
            {
                int  taskId   = Convert.ToInt32(row["i_task_id"]);
                int  depth    = Convert.ToInt32(row["tree_depth"]);
                bool isRoot   = row["i_parent_task_id"] == DBNull.Value;
                int? parentId = isRoot ? (int?)null : Convert.ToInt32(row["i_parent_task_id"]);

                if (isRoot)
                {
                    // ใช้ filter เฉพาะ root rows
                    if (!string.IsNullOrEmpty(filterStatus)   && row["c_status"].ToString() != filterStatus)    continue;
                    if (!string.IsNullOrEmpty(filterAssignee) && row["n_assignee"].ToString() != filterAssignee) continue;
                    if (!string.IsNullOrEmpty(filterKeyword)  && !row["n_title"].ToString().ToLower().Contains(filterKeyword)) continue;
                }
                else
                {
                    // sub rows: แสดงเฉพาะถ้า parent มองเห็น และ parent ถูก expand
                    if (!visibleSet.Contains(parentId.Value))                   continue;
                    if (!_expandedIds.Contains(parentId.Value))                 continue;
                }

                visibleSet.Add(taskId);
                DataRow r = result.NewRow();
                CopyRow(row, r, depth > 0, depth);
                result.Rows.Add(r);
            }
            return result;
        }

        private DataTable CreateVisibleSchema()
        {
            var dt = new DataTable();
            dt.Columns.Add("i_task_id",        typeof(int));
            dt.Columns.Add("c_task_code",       typeof(string));
            dt.Columns.Add("i_proj_state_id",   typeof(int));
            dt.Columns.Add("i_project_id",      typeof(int));
            dt.Columns.Add("i_parent_task_id",  typeof(int));
            dt.Columns.Add("n_title",           typeof(string));
            dt.Columns.Add("n_state_name",      typeof(string));
            dt.Columns.Add("n_assignee",        typeof(string));
            dt.Columns.Add("i_priority",        typeof(int));
            dt.Columns.Add("c_status",          typeof(string));
            dt.Columns.Add("i_pct_complete",    typeof(int));
            dt.Columns.Add("d_due_date",        typeof(DateTime));
            dt.Columns.Add("b_overdue",         typeof(int));
            dt.Columns.Add("b_sla_warning",     typeof(int));
            dt.Columns.Add("i_subtask_count",   typeof(int));
            dt.Columns.Add("i_subtask_done",    typeof(int));
            dt.Columns.Add("_is_sub",           typeof(bool));
            dt.Columns.Add("_depth",            typeof(int));
            return dt;
        }

        private void CopyRow(DataRow src, DataRow dst, bool isSub, int depth)
        {
            dst["i_task_id"]        = src["i_task_id"];
            dst["c_task_code"]      = src["c_task_code"];
            dst["i_proj_state_id"]  = src["i_proj_state_id"];
            dst["i_project_id"]     = src["i_project_id"];
            dst["i_parent_task_id"] = src["i_parent_task_id"] == DBNull.Value ? (object)DBNull.Value : src["i_parent_task_id"];
            string title = src["n_title"].ToString();
            dst["n_title"] = depth > 0 ? new string(' ', depth * 3) + "└ " + title : title;
            dst["n_state_name"]     = src["n_state_name"];
            dst["n_assignee"]       = src["n_assignee"] == DBNull.Value ? (object)DBNull.Value : src["n_assignee"];
            dst["i_priority"]       = src["i_priority"];
            dst["c_status"]         = src["c_status"];
            dst["i_pct_complete"]   = src["i_pct_complete"];
            dst["d_due_date"]       = src["d_due_date"] == DBNull.Value ? (object)DBNull.Value : src["d_due_date"];
            dst["b_overdue"]        = src["b_overdue"];
            dst["b_sla_warning"]    = src["b_sla_warning"];
            dst["i_subtask_count"]  = src.Table.Columns.Contains("i_subtask_count") ? src["i_subtask_count"] : (object)0;
            dst["i_subtask_done"]   = src.Table.Columns.Contains("i_subtask_done")  ? src["i_subtask_done"]  : (object)0;
            dst["_is_sub"]          = isSub;
            dst["_depth"]           = depth;
        }

        // ────────────────────────────────────────────────────────────────────────
        // DGV EVENTS
        // ────────────────────────────────────────────────────────────────────────
        private void dgvTasks_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || _visibleRows == null || _visibleRows.Rows.Count <= e.RowIndex) return;
            DataRow row  = _visibleRows.Rows[e.RowIndex];
            bool isSub   = Convert.ToBoolean(row["_is_sub"]);
            bool overdue = Convert.ToInt32(row["b_overdue"])     == 1;
            bool sla     = Convert.ToInt32(row["b_sla_warning"]) == 1;
            int  taskId  = Convert.ToInt32(row["i_task_id"]);
            string colName = dgvTasks.Columns[e.ColumnIndex].Name;

            if (overdue)     { e.CellStyle.BackColor = Color.FromArgb(255, 232, 232); }
            else if (sla)    { e.CellStyle.BackColor = Color.FromArgb(255, 250, 220); }
            else if (isSub)  { e.CellStyle.BackColor = Color.FromArgb(248, 250, 255); }

            if (colName == "col_expand")
            {
                bool hasChildren = Convert.ToInt32(row["i_subtask_count"]) > 0;
                if (!hasChildren) { e.Value = ""; e.FormattingApplied = true; return; }
                bool expanded = _expandedIds.Contains(taskId);
                e.Value = expanded ? "▾" : "▸";
                e.CellStyle.ForeColor = NavyMedium;
                e.CellStyle.Font      = new Font("Segoe UI", 11F, FontStyle.Bold);
                e.FormattingApplied   = true;
                return;
            }

            if (colName == "col_title")
            {
                if (!isSub)
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                else
                    e.CellStyle.ForeColor = Color.FromArgb(70, 90, 130);
            }

            if (colName == "col_stat")
            {
                switch (e.Value?.ToString())
                {
                    case "Done":
                        e.CellStyle.BackColor = Color.FromArgb(198, 239, 206); e.CellStyle.ForeColor = Color.FromArgb(0, 97, 0);
                        e.Value = "เสร็จ"; e.FormattingApplied = true; break;
                    case "InProgress":
                        e.CellStyle.BackColor = Color.FromArgb(255, 235, 156); e.CellStyle.ForeColor = Color.FromArgb(99, 71, 0);
                        e.Value = "กำลังทำ"; e.FormattingApplied = true; break;
                    case "InReview":
                        e.CellStyle.BackColor = Color.FromArgb(220, 208, 255); e.CellStyle.ForeColor = Color.FromArgb(68, 0, 153);
                        e.Value = "รอตรวจ"; e.FormattingApplied = true; break;
                    case "Cancelled":
                        e.CellStyle.BackColor = Color.FromArgb(255, 199, 206); e.CellStyle.ForeColor = Color.FromArgb(156, 0, 6);
                        e.Value = "ยกเลิก"; e.FormattingApplied = true; break;
                    default:
                        e.CellStyle.BackColor = Color.FromArgb(215, 228, 248); e.CellStyle.ForeColor = Color.FromArgb(30, 58, 95);
                        e.Value = "รอทำ"; e.FormattingApplied = true; break;
                }
                e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            }

            if (colName == "col_prio" && e.Value != null)
            {
                int p = Convert.ToInt32(e.Value);
                string[] labels = { "", "Low", "Medium", "High", "Critical" };
                e.Value = p >= 1 && p <= 4 ? labels[p] : "Medium";
                e.FormattingApplied = true;
                if (p == 4)      e.CellStyle.ForeColor = Color.FromArgb(156, 0, 6);
                else if (p == 3) e.CellStyle.ForeColor = Color.FromArgb(196, 53, 53);
                else if (p == 1) e.CellStyle.ForeColor = Color.FromArgb(100, 130, 168);
            }

            if (colName == "col_due" && overdue)
            {
                e.CellStyle.ForeColor = Color.FromArgb(196, 53, 53);
                e.CellStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
        }

        private void dgvTasks_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (_visibleRows == null || _visibleRows.Rows.Count <= e.RowIndex) return;
            DataRow row = _visibleRows.Rows[e.RowIndex];
            bool isSub  = Convert.ToBoolean(row["_is_sub"]);
            if (!isSub) return;

            using (var brush = new SolidBrush(Color.FromArgb(43, 87, 154)))
            {
                var rect = new Rectangle(e.RowBounds.Left + 1, e.RowBounds.Top, 3, e.RowBounds.Height);
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        private void dgvTasks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _visibleRows == null || _visibleRows.Rows.Count <= e.RowIndex) return;
            if (dgvTasks.Columns[e.ColumnIndex].Name != "col_expand") return;

            DataRow row = _visibleRows.Rows[e.RowIndex];
            int taskId = Convert.ToInt32(row["i_task_id"]);
            bool hasChildren = Convert.ToInt32(row["i_subtask_count"]) > 0;
            if (!hasChildren) return;

            if (_expandedIds.Contains(taskId)) _expandedIds.Remove(taskId);
            else _expandedIds.Add(taskId);

            RefreshGrid();
        }

        private void dgvTasks_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            OpenSelectedTask();
        }

        private void dgvTasks_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvTasks.CurrentRow == null || _visibleRows == null) { EnableButtons(false); HideDetailPanel(); return; }
            int idx = dgvTasks.CurrentRow.Index;
            if (_visibleRows.Rows.Count <= idx) { EnableButtons(false); HideDetailPanel(); return; }
            DataRow row = _visibleRows.Rows[idx];
            bool isSub  = Convert.ToBoolean(row["_is_sub"]);
            bool canEdit = CanEditTask(row);
            EnableButtons(true, isSub, canEdit);
            ShowDetailPanel(row);
        }

        private void EnableButtons(bool hasSelection, bool isSub = false, bool canEdit = true)
        {
            btnEditTask.Enabled     = hasSelection && canEdit;
            btnDeleteTask.Enabled   = hasSelection && canEdit && !isSub;
            btnChangeStatus.Enabled = hasSelection && canEdit;
            btnAddSub.Enabled       = hasSelection;  // ทุก task สามารถมี sub task ได้
        }

        // ── Permission ────────────────────────────────────────────────────────────
        private bool CanEditTask(DataRow row)
        {
            // assignee ของ row นี้ตรงกับ user ปัจจุบัน → แก้ไขได้
            string asgn = row["n_assignee"] != DBNull.Value ? row["n_assignee"].ToString() : "";
            if (string.Equals(asgn, _userName, StringComparison.OrdinalIgnoreCase)) return true;

            // root task assignee → แก้ไขได้ทุก task ภายใต้นั้น
            string rootAsgn = FindRootAssignee(Convert.ToInt32(row["i_task_id"]));
            return string.Equals(rootAsgn, _userName, StringComparison.OrdinalIgnoreCase);
        }

        private string FindRootAssignee(int taskId)
        {
            // เดินขึ้น parent จนถึง root แล้วคืน assignee ของ root
            var row = _allRows.FirstOrDefault(r => Convert.ToInt32(r["i_task_id"]) == taskId);
            if (row == null) return "";
            while (row["i_parent_task_id"] != DBNull.Value)
            {
                int pid = Convert.ToInt32(row["i_parent_task_id"]);
                row = _allRows.FirstOrDefault(r => Convert.ToInt32(r["i_task_id"]) == pid);
                if (row == null) return "";
            }
            return row["n_assignee"] != DBNull.Value ? row["n_assignee"].ToString() : "";
        }

        // ────────────────────────────────────────────────────────────────────────
        // FILTER EVENTS
        // ────────────────────────────────────────────────────────────────────────
        private void cbbProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            var val = cbbProject.SelectedValue;
            if (val == null || val is DBNull) { _currentProjectId = -1; LoadData(); return; }
            int pid;
            if (val is int) pid = (int)val;
            else if (!int.TryParse(val.ToString(), out pid)) { _currentProjectId = -1; LoadData(); return; }
            _currentProjectId = pid;
            LoadData();
        }

        private void cbbStatusFilter_Changed(object sender, EventArgs e) => RefreshGrid();
        private void cbbAssignFilter_Changed(object sender, EventArgs e) => RefreshGrid();
        private void txtSearch_TextChanged(object sender, EventArgs e)   => RefreshGrid();
        private void btnRefresh_Click(object sender, EventArgs e)        => LoadData();

        // ────────────────────────────────────────────────────────────────────────
        // ACTION BUTTONS
        // ────────────────────────────────────────────────────────────────────────
        private void btnAddTask_Click(object sender, EventArgs e)
        {
            if (_currentProjectId < 0)
            {
                MessageBox.Show("กรุณาเลือกโครงการก่อน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string stateName = "";
            int    stateId   = 0;
            using (var dlg = new Frm_Select_StateForTask(_currentProjectId, _stateSvc))
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                stateId   = dlg.SelectedStateId;
                stateName = dlg.SelectedStateName;
            }
            var frm = new Frm_Project_Task_Edit(_currentProjectId, stateId, stateName, null, _userName);
            if (frm.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void btnAddSub_Click(object sender, EventArgs e)
        {
            DataRow row = GetSelectedRow();
            if (row == null) return;
            int    parentId   = Convert.ToInt32(row["i_task_id"]);
            int    stateId    = Convert.ToInt32(row["i_proj_state_id"]);
            string stateName  = row["n_state_name"].ToString();
            string parentCode = row["c_task_code"].ToString();

            var frm = new Frm_Project_Task_Edit(
                _currentProjectId, stateId, stateName, null, _userName,
                parentTaskId: parentId, parentTaskCode: parentCode);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                _expandedIds.Add(parentId);
                LoadData();
            }
        }

        private void btnEditTask_Click(object sender, EventArgs e) => OpenSelectedTask();

        private void btnDeleteTask_Click(object sender, EventArgs e)
        {
            DataRow row = GetSelectedRow();
            if (row == null) return;
            string title = row["n_title"].ToString().Trim();
            if (MessageBox.Show(string.Format("ต้องการลบ Task\n\"{0}\"\n(ย้ายไป Recycle Bin 30 วัน)?", title),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int id = Convert.ToInt32(row["i_task_id"]);
            _taskSvc.SoftDelete(id, _userName);
            _expandedIds.Remove(id);
            _childrenMap.Remove(id);
            LoadData();
        }

        private void btnChangeStatus_Click(object sender, EventArgs e)
        {
            DataRow row = GetSelectedRow();
            if (row == null) return;
            int    taskId  = Convert.ToInt32(row["i_task_id"]);
            int    stateId = Convert.ToInt32(row["i_proj_state_id"]);
            string curStat = row["c_status"].ToString();
            int    curPct  = Convert.ToInt32(row["i_pct_complete"]);

            using (var dlg = new Frm_Project_Task_Status(taskId, curStat, curPct))
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                string err;
                _taskSvc.UpdateStatus(taskId, stateId, _currentProjectId,
                    dlg.NewStatus, dlg.NewPct, _userName, out err);
                if (!string.IsNullOrEmpty(err))
                    MessageBox.Show(err, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadData();
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();

        // ────────────────────────────────────────────────────────────────────────
        // KANBAN
        // ────────────────────────────────────────────────────────────────────────
        private void SetupKanbanTab()
        {
            _kanbanPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            tabKanbanView.Controls.Add(_kanbanPanel);
        }

        private void RefreshKanban()
        {
            if (_kanbanPanel == null) return;
            _kanbanPanel.Controls.Clear();
            if (_allRows == null || _allRows.Count == 0) return;

            // Kanban แสดงเฉพาะ root tasks (ไม่มี parent)
            var rootRows = _allRows.Where(r => r["i_parent_task_id"] == DBNull.Value).ToList();
            if (rootRows.Count == 0) return;

            int colW = 240, gap = 10, x = gap;
            // คำนวณความสูงต่อ column จากจำนวน cards จริง (ให้ scroll ได้)
            int maxCards = rootRows.Count > 0
                ? _statusCols.Max(sc => rootRows.Count(r => r["c_status"].ToString() == sc.Status)
                    + rootRows.Where(r => r["c_status"].ToString() == sc.Status)
                              .Sum(r => _childrenMap.ContainsKey(Convert.ToInt32(r["i_task_id"]))
                                        ? _childrenMap[Convert.ToInt32(r["i_task_id"])].Count : 0))
                : 0;
            int estimatedColH = 42 + maxCards * 130 + 20;
            int panelH = _kanbanPanel.ClientSize.Height > 80 ? _kanbanPanel.ClientSize.Height - gap * 2 : 500;
            int colH = Math.Max(panelH, estimatedColH);

            foreach (var col in _statusCols)
            {
                var rows = rootRows
                    .Where(r => r["c_status"].ToString() == col.Status).ToList();

                var colPanel = BuildKanbanColumn(col.Status, col.Label, col.Color, rows, colW, colH);
                colPanel.Location = new Point(x, gap);
                _kanbanPanel.Controls.Add(colPanel);
                x += colW + gap;
            }
            _kanbanPanel.AutoScrollMinSize = new Size(x + gap, 0);
        }

        private Panel BuildKanbanColumn(string status, string label, Color hdrColor,
                                        List<DataRow> rows, int colW, int colH)
        {
            var panel = new Panel
            {
                Width = colW, Height = colH,
                BackColor = Color.FromArgb(235, 240, 248),
                AutoScroll = true, AllowDrop = true, Tag = status
            };

            var header   = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = hdrColor };
            var lblCount = new Label { Text = rows.Count.ToString(), Dock = DockStyle.Right, Width = 28,
                ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter };
            var lblHdr   = new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0) };
            header.Controls.Add(lblCount); header.Controls.Add(lblHdr);
            panel.Controls.Add(header);

            int cardY = 42;
            foreach (var row in rows)
            {
                var card = BuildKanbanCard(row, hdrColor, colW - 18, status);
                card.Location = new Point(8, cardY);
                panel.Controls.Add(card);
                cardY += card.Height + 4;

                // Recursive: แสดง sub task ทุก depth
                int taskId = Convert.ToInt32(row["i_task_id"]);
                AddSubCardsRecursive(panel, taskId, colW, status, ref cardY, depth: 1);

                cardY += 8;
            }

            panel.AutoScrollMinSize = new Size(0, cardY + 8);

            panel.DragEnter += (s, ev) =>
            {
                if (ev.Data.GetDataPresent(typeof(string)))
                {
                    string p = ev.Data.GetData(typeof(string)) as string ?? "";
                    if (p.StartsWith("TASK|") && p.Split('|').Length >= 4 && p.Split('|')[3] != status)
                    { ev.Effect = DragDropEffects.Move; panel.BackColor = Color.FromArgb(210, 225, 248); }
                    else ev.Effect = DragDropEffects.None;
                }
            };
            panel.DragLeave += (s, ev) => panel.BackColor = Color.FromArgb(235, 240, 248);
            panel.DragDrop  += (s, ev) =>
            {
                panel.BackColor = Color.FromArgb(235, 240, 248);
                string p = ev.Data.GetData(typeof(string)) as string ?? "";
                if (!p.StartsWith("TASK|")) return;
                var parts = p.Split('|');
                if (parts.Length < 4) return;
                int taskId = int.Parse(parts[1]); int stateId = int.Parse(parts[2]);
                int pct    = status == "Done" ? 100 : status == "Todo" ? 0 : status == "InProgress" ? 50 : 80;
                string err;
                _taskSvc.UpdateStatus(taskId, stateId, _currentProjectId, status, pct, _userName, out err);
                if (!string.IsNullOrEmpty(err))
                    MessageBox.Show(err, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else LoadData();
            };
            return panel;
        }

        private Panel BuildKanbanCard(DataRow r, Color accent, int width, string colStatus)
        {
            bool overdue = Convert.ToInt32(r["b_overdue"])     == 1;
            bool sla     = Convert.ToInt32(r["b_sla_warning"]) == 1;
            int  prio    = Convert.ToInt32(r["i_priority"]);
            int  taskId  = Convert.ToInt32(r["i_task_id"]);
            int  stateId = Convert.ToInt32(r["i_proj_state_id"]);
            int  subCnt  = Convert.ToInt32(r["i_subtask_count"]);
            int  subDone = Convert.ToInt32(r["i_subtask_done"]);
            string due   = r["d_due_date"] != DBNull.Value ? Convert.ToDateTime(r["d_due_date"]).ToString("dd/MM/yy") : "-";
            string assignee = r["n_assignee"] != DBNull.Value ? r["n_assignee"].ToString() : "-";

            Color cardBg = overdue ? Color.FromArgb(255, 232, 232) : sla ? Color.FromArgb(255, 250, 220) : Color.White;
            Color stripe = prio == 4 ? Color.FromArgb(196, 53, 53) : prio == 3 ? Color.FromArgb(255, 140, 0) : prio == 1 ? SlateBlue : accent;

            int cardH = subCnt > 0 ? 116 : 100;
            var card   = new Panel { Width = width, Height = cardH, BackColor = cardBg, Cursor = Cursors.SizeAll, Tag = taskId };
            var stripePanel = new Panel { Width = 5, Dock = DockStyle.Left, BackColor = stripe };
            card.Controls.Add(stripePanel);

            var inner = new Panel { Left = 7, Top = 0, Width = width - 9, Height = cardH, BackColor = cardBg, Tag = taskId };

            var lblCode  = new Label { Text = r["c_task_code"].ToString(), Location = new Point(0, 5), Size = new Size(100, 14), Font = new Font("Segoe UI", 7.5F), ForeColor = Color.Gray };
            var lblPrioL = new Label { Text = ProjectTaskModel.PriorityLabel(prio), Location = new Point(width - 90, 4), Size = new Size(80, 14), Font = new Font("Segoe UI", 7.5F), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleRight };
            var lblTitle = new Label { Text = r["n_title"].ToString(), Location = new Point(0, 20), Size = new Size(width - 10, 36), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = NavyDark };
            var lblAsgn  = new Label { Text = assignee, Location = new Point(0, 58), Size = new Size((width - 10) / 2, 16), Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(70, 90, 130) };
            var lblDue   = new Label { Text = due, Location = new Point((width - 10) / 2, 58), Size = new Size((width - 10) / 2, 16), Font = new Font("Segoe UI", 8F), ForeColor = overdue ? DangerRed : Color.FromArgb(70, 90, 130), TextAlign = ContentAlignment.MiddleRight };

            inner.Controls.AddRange(new Control[] { lblCode, lblPrioL, lblTitle, lblAsgn, lblDue });

            if (subCnt > 0)
            {
                var lblSub = new Label { Text = string.Format("Sub Task: {0}/{1}", subDone, subCnt), Location = new Point(0, 78), Size = new Size(width - 10, 14), Font = new Font("Segoe UI", 7.5F), ForeColor = Color.FromArgb(70, 110, 165) };
                inner.Controls.Add(lblSub);
            }
            card.Controls.Add(inner);

            string payload = string.Format("TASK|{0}|{1}|{2}", taskId, stateId, colStatus);
            EventHandler startDrag  = (s, ev) => ((Control)s).DoDragDrop(payload, DragDropEffects.Move);
            EventHandler openDetail = (s, ev) => OpenDetailById(taskId, stateId, r["n_state_name"].ToString());

            card.MouseDown  += (s, ev) => { if (ev.Button == MouseButtons.Left) startDrag(s, ev); };
            card.DoubleClick += openDetail;
            inner.MouseDown  += (s, ev) => { if (ev.Button == MouseButtons.Left) startDrag(s, ev); };
            inner.DoubleClick += openDetail;
            foreach (Control c in inner.Controls)
            {
                c.Tag         = taskId;
                c.MouseDown  += (s, ev) => { if (ev.Button == MouseButtons.Left) startDrag(s, ev); };
                c.DoubleClick += openDetail;
            }
            return card;
        }

        /// <summary>เพิ่ม sub task cards แบบ recursive ทุก depth ลงใน kanban column panel</summary>
        private void AddSubCardsRecursive(Panel panel, int parentId, int colW, string status, ref int cardY, int depth)
        {
            if (!_childrenMap.ContainsKey(parentId)) return;

            // indent เพิ่มขึ้นตาม depth แต่ไม่เกิน colW-10
            int indent    = Math.Min(8 + depth * 16, colW - 60);
            int cardWidth = colW - indent - 20;

            foreach (DataRow sub in _childrenMap[parentId])
            {
                // เส้นแนวตั้งและแนวนอนเชื่อม parent
                var vLine = new Panel
                {
                    Location  = new Point(indent - 4, cardY),
                    Size      = new Size(2, 30),
                    BackColor = Color.FromArgb(160, 190, 230)
                };
                var hLine = new Panel
                {
                    Location  = new Point(indent - 4, cardY + 15),
                    Size      = new Size(8, 2),
                    BackColor = Color.FromArgb(160, 190, 230)
                };
                panel.Controls.Add(vLine);
                panel.Controls.Add(hLine);

                var subCard = BuildSubCard(sub, cardWidth, status);
                subCard.Location = new Point(indent + 4, cardY);
                panel.Controls.Add(subCard);
                cardY += subCard.Height + 4;

                // ลึกลงไปอีก
                int subId = Convert.ToInt32(sub["i_task_id"]);
                AddSubCardsRecursive(panel, subId, colW, status, ref cardY, depth + 1);
            }
        }

        /// <summary>Card ขนาดเล็กสำหรับ Sub Task ใน Kanban (solid background, ไม่ใช้ wrap transparent)</summary>
        private Panel BuildSubCard(DataRow r, int width, string colStatus)
        {
            bool overdue = Convert.ToInt32(r["b_overdue"]) == 1;
            int  prio    = Convert.ToInt32(r["i_priority"]);
            int  taskId  = Convert.ToInt32(r["i_task_id"]);
            int  stateId = Convert.ToInt32(r["i_proj_state_id"]);
            string title    = r["n_title"].ToString();
            string assignee = r["n_assignee"] != DBNull.Value ? r["n_assignee"].ToString() : "-";
            string status   = r["c_status"].ToString();

            Color badgeBg; string badgeText;
            switch (status)
            {
                case "Done":       badgeBg = Color.FromArgb(40, 167, 69);   badgeText = "เสร็จ";   break;
                case "InProgress": badgeBg = Color.FromArgb(255, 165, 0);   badgeText = "กำลังทำ"; break;
                case "InReview":   badgeBg = Color.FromArgb(111, 66, 193);  badgeText = "รอตรวจ";  break;
                case "Cancelled":  badgeBg = Color.FromArgb(220, 53, 69);   badgeText = "ยกเลิก";  break;
                default:           badgeBg = Color.FromArgb(108, 117, 125); badgeText = "รอทำ";    break;
            }

            Color cardBg = overdue ? Color.FromArgb(255, 238, 238) : Color.FromArgb(242, 246, 255);
            Color stripe = prio == 4 ? Color.FromArgb(196, 53, 53)
                         : prio == 3 ? Color.FromArgb(255, 140, 0)
                         : Color.FromArgb(130, 170, 220);

            // card โดยตรง — ไม่มี wrap transparent
            int cardH = 60;
            var card = new Panel
            {
                Width     = width,
                Height    = cardH,
                BackColor = cardBg,
                Cursor    = Cursors.SizeAll,
                Tag       = taskId
            };

            // แถบสีซ้าย
            card.Controls.Add(new Panel { Width = 4, Dock = DockStyle.Left, BackColor = stripe });

            var inner = new Panel { Left = 6, Top = 0, Width = width - 8, Height = cardH, BackColor = cardBg, Tag = taskId };

            var lblCode = new Label
            {
                Text = r["c_task_code"].ToString(), Location = new Point(0, 4), Size = new Size(90, 13),
                Font = new Font("Segoe UI", 7F), ForeColor = Color.Gray
            };
            var lblTitle = new Label
            {
                Text = title, Location = new Point(0, 18), Size = new Size(width - 10, 20),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = NavyDark
            };
            var lblAsgn = new Label
            {
                Text = assignee, Location = new Point(0, 40), Size = new Size((width - 10) / 2, 14),
                Font = new Font("Segoe UI", 7.5F), ForeColor = Color.FromArgb(80, 100, 140)
            };
            var lblBadge = new Label
            {
                Text = badgeText, Location = new Point((width - 10) / 2, 39),
                Size = new Size((width - 10) / 2, 15),
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                ForeColor = Color.White, BackColor = badgeBg,
                TextAlign = ContentAlignment.MiddleCenter
            };

            inner.Controls.AddRange(new Control[] { lblCode, lblTitle, lblAsgn, lblBadge });
            card.Controls.Add(inner);

            string payload = string.Format("TASK|{0}|{1}|{2}", taskId, stateId, colStatus);
            EventHandler startDrag  = (s, ev) => ((Control)s).DoDragDrop(payload, DragDropEffects.Move);
            EventHandler openDetail = (s, ev) => OpenDetailById(taskId, stateId, r["n_state_name"].ToString());

            card.MouseDown   += (s, ev) => { if (ev.Button == MouseButtons.Left) startDrag(s, ev); };
            card.DoubleClick += openDetail;
            inner.MouseDown  += (s, ev) => { if (ev.Button == MouseButtons.Left) startDrag(s, ev); };
            inner.DoubleClick += openDetail;
            foreach (Control c in inner.Controls)
            {
                c.Tag        = taskId;
                c.MouseDown  += (s, ev) => { if (ev.Button == MouseButtons.Left) startDrag(s, ev); };
                c.DoubleClick += openDetail;
            }

            return card;
        }

        // ────────────────────────────────────────────────────────────────────────
        // SPLIT LAYOUT + DETAIL PANEL (email-style)
        // ────────────────────────────────────────────────────────────────────────

        private void SetupSplitLayout()
        {
            // ย้าย tabMain เข้า SplitContainer
            this.Controls.Remove(tabMain);

            _splitMain = new SplitContainer
            {
                Dock          = DockStyle.Fill,
                SplitterWidth = 4,
                FixedPanel    = FixedPanel.Panel2,
                Panel2Collapsed = true,
                BackColor     = Color.FromArgb(200, 218, 240)
            };
            _splitMain.Panel1.Controls.Add(tabMain);
            tabMain.Dock = DockStyle.Fill;

            _pnlDetail = BuildDetailPanel();
            _pnlDetail.Dock = DockStyle.Fill;
            _splitMain.Panel2.Controls.Add(_pnlDetail);

            this.Controls.Add(_splitMain);
            _splitMain.BringToFront();
        }

        private Panel BuildDetailPanel()
        {
            var pnl = new Panel { BackColor = Color.White };

            // ── Header bar ──────────────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = NavyDark };

            _detCode = new Label
            {
                Dock = DockStyle.Left, Width = 120, ForeColor = Color.FromArgb(160, 200, 240),
                Font = new Font("Segoe UI", 8F), TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            var btnClose = new Button
            {
                Text = "✕", Dock = DockStyle.Right, Width = 38,
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = NavyDark,
                Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10F)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, ev) => HideDetailPanel();
            hdr.Controls.Add(btnClose);
            hdr.Controls.Add(_detCode);
            pnl.Controls.Add(hdr);

            // ── Scroll area for content ─────────────────────────────────────
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12, 8, 12, 8) };

            int y = 8, pw = 356; // approximate content width

            // Title
            _detTitle = new Label
            {
                Location = new Point(0, y), Size = new Size(pw, 44),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = NavyDark
            };
            scroll.Controls.Add(_detTitle);
            y += 50;

            // State/Assignee row
            _detStateLbl = new Label
            {
                Location = new Point(0, y), Size = new Size(pw, 18),
                Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(100, 120, 160)
            };
            scroll.Controls.Add(_detStateLbl);
            y += 24;

            // Separator
            scroll.Controls.Add(new Label { Location = new Point(0, y), Size = new Size(pw, 1), BackColor = Color.FromArgb(220, 228, 242) });
            y += 8;

            // Status
            scroll.Controls.Add(MakeLabel("สถานะ:", 0, y, 100));
            _detStatus = new ComboBox { Location = new Point(104, y - 2), Size = new Size(200, 24), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F) };
            _detStatus.Items.AddRange(new object[] { "Todo", "InProgress", "InReview", "Done", "Cancelled" });
            scroll.Controls.Add(_detStatus);
            y += 32;

            // Priority
            scroll.Controls.Add(MakeLabel("ลำดับสำคัญ:", 0, y, 100));
            _detPriority = new ComboBox { Location = new Point(104, y - 2), Size = new Size(130, 24), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F) };
            _detPriority.Items.AddRange(new object[] { "1 - Low", "2 - Medium", "3 - High", "4 - Critical" });
            scroll.Controls.Add(_detPriority);
            y += 32;

            // % Complete
            scroll.Controls.Add(MakeLabel("% เสร็จ:", 0, y, 100));
            _detPctBar = new TrackBar { Location = new Point(104, y - 4), Size = new Size(180, 30), Minimum = 0, Maximum = 100, TickFrequency = 10, SmallChange = 5, LargeChange = 10 };
            _detPctLbl = new Label { Location = new Point(290, y), Size = new Size(40, 22), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = NavyMedium, TextAlign = ContentAlignment.MiddleLeft };
            _detPctBar.ValueChanged += (s, ev) => _detPctLbl.Text = _detPctBar.Value + "%";
            scroll.Controls.Add(_detPctBar); scroll.Controls.Add(_detPctLbl);
            y += 36;

            // Due date
            scroll.Controls.Add(MakeLabel("กำหนดเสร็จ:", 0, y, 100));
            _detDueChk = new CheckBox { Location = new Point(104, y), Size = new Size(16, 22), Text = "" };
            _detDue    = new DateTimePicker { Location = new Point(124, y - 1), Size = new Size(150, 24), Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 9F) };
            _detDueChk.CheckedChanged += (s, ev) => _detDue.Enabled = _detDueChk.Checked;
            _detDue.Enabled = false;
            scroll.Controls.Add(_detDueChk); scroll.Controls.Add(_detDue);
            y += 32;

            // Assignee
            scroll.Controls.Add(MakeLabel("ผู้รับผิดชอบ:", 0, y, 100));
            _detAssignee = new TextBox { Location = new Point(104, y - 1), Size = new Size(200, 22), ReadOnly = true, BackColor = Color.FromArgb(242, 246, 252), Font = new Font("Segoe UI", 9F) };
            scroll.Controls.Add(_detAssignee);
            y += 32;

            // Separator
            scroll.Controls.Add(new Label { Location = new Point(0, y), Size = new Size(pw, 1), BackColor = Color.FromArgb(220, 228, 242) });
            y += 8;
            scroll.Controls.Add(MakeLabel("รายละเอียด:", 0, y, pw, FontStyle.Bold));
            y += 22;

            // Description
            _detDesc = new TextBox
            {
                Location = new Point(0, y), Size = new Size(pw, 80),
                Multiline = true, ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F), BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 250, 255)
            };
            scroll.Controls.Add(_detDesc);
            y += 88;

            // Separator
            scroll.Controls.Add(new Label { Location = new Point(0, y), Size = new Size(pw, 1), BackColor = Color.FromArgb(220, 228, 242) });
            y += 8;

            // Sub tasks header
            var lblSub = MakeLabel("Sub Tasks", 0, y, pw - 110, FontStyle.Bold);
            scroll.Controls.Add(lblSub);
            _btnDetAddSub = new Button
            {
                Location = new Point(pw - 108, y - 2), Size = new Size(108, 24),
                Text = "+ เพิ่ม Sub Task", FlatStyle = FlatStyle.Flat,
                BackColor = NavyMedium, ForeColor = Color.White, Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };
            _btnDetAddSub.FlatAppearance.BorderSize = 0;
            _btnDetAddSub.Click += BtnDetAddSub_Click;
            scroll.Controls.Add(_btnDetAddSub);
            y += 30;

            // Sub tasks container
            _detSubsPanel = new Panel { Location = new Point(0, y), Size = new Size(pw, 100), AutoScroll = true, BackColor = Color.FromArgb(248, 250, 255) };
            scroll.Controls.Add(_detSubsPanel);
            y += 108;

            // Separator
            scroll.Controls.Add(new Label { Location = new Point(0, y), Size = new Size(pw, 1), BackColor = Color.FromArgb(220, 228, 242) });
            y += 10;

            // Buttons
            _btnDetSave = new Button
            {
                Location = new Point(0, y), Size = new Size(100, 30),
                Text = "บันทึก", FlatStyle = FlatStyle.Flat,
                BackColor = NavyMedium, ForeColor = Color.White, Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            _btnDetSave.FlatAppearance.BorderSize = 0;
            _btnDetSave.Click += BtnDetSave_Click;

            _btnDetEdit = new Button
            {
                Location = new Point(108, y), Size = new Size(110, 30),
                Text = "แก้ไขเต็ม", FlatStyle = FlatStyle.Flat,
                BackColor = SlateBlue, ForeColor = Color.White, Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F)
            };
            _btnDetEdit.FlatAppearance.BorderSize = 0;
            _btnDetEdit.Click += (s, ev) => OpenSelectedTask();

            scroll.Controls.Add(_btnDetSave);
            scroll.Controls.Add(_btnDetEdit);

            pnl.Controls.Add(scroll);
            return pnl;
        }

        private Label MakeLabel(string text, int x, int y, int w, FontStyle style = FontStyle.Regular)
        {
            return new Label
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, 22),
                Font = new Font("Segoe UI", 9F, style), ForeColor = NavyDark,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private void ShowDetailPanel(DataRow row)
        {
            // เปิด Panel ขวา
            if (_splitMain.Panel2Collapsed)
            {
                _splitMain.Panel2Collapsed = false;
                _splitMain.SplitterDistance = Math.Max(300, this.ClientSize.Width - 400);
                _splitMain.Panel2MinSize    = 360;
            }
            PopulateDetailPanel(row);
        }

        private void HideDetailPanel()
        {
            if (_splitMain != null) _splitMain.Panel2Collapsed = true;
        }

        private void PopulateDetailPanel(DataRow row)
        {
            _detTaskId = Convert.ToInt32(row["i_task_id"]);
            _detDepth  = Convert.ToInt32(row["_depth"]);

            // ดึงข้อมูลเต็มจาก _allRows (มีทุกคอลัมน์ รวม n_description)
            var fullRow = _allRows.FirstOrDefault(r => Convert.ToInt32(r["i_task_id"]) == _detTaskId);
            if (fullRow == null) return;

            string taskCode = fullRow["c_task_code"].ToString();
            string title    = fullRow["n_title"].ToString();
            string state    = fullRow["n_state_name"].ToString();
            string asgn     = fullRow["n_assignee"] != DBNull.Value ? fullRow["n_assignee"].ToString() : "-";
            string desc     = fullRow["n_description"] != DBNull.Value ? fullRow["n_description"].ToString() : "";
            string status   = fullRow["c_status"].ToString();
            int    prio     = Convert.ToInt32(fullRow["i_priority"]);
            int    pct      = Convert.ToInt32(fullRow["i_pct_complete"]);
            bool   canEdit  = CanEditTask(row);

            // Header
            _detCode.Text  = taskCode;
            _detTitle.Text = title;
            _detStateLbl.Text = state + "  |  ผู้รับผิดชอบ: " + asgn;

            // Status
            int statusIdx = Array.IndexOf(new[] { "Todo","InProgress","InReview","Done","Cancelled" }, status);
            _detStatus.SelectedIndex = statusIdx >= 0 ? statusIdx : 0;
            _detStatus.Enabled = canEdit;

            // Priority (1-based index → 0-based combo)
            _detPriority.SelectedIndex = Math.Max(0, Math.Min(3, prio - 1));
            _detPriority.Enabled = canEdit;

            // % complete
            _detPctBar.Value = Math.Max(0, Math.Min(100, pct));
            _detPctLbl.Text  = pct + "%";
            _detPctBar.Enabled = canEdit;

            // Due date
            if (fullRow["d_due_date"] != DBNull.Value)
            {
                _detDueChk.Checked = true;
                _detDue.Value      = Convert.ToDateTime(fullRow["d_due_date"]);
                _detDue.Enabled    = canEdit;
            }
            else
            {
                _detDueChk.Checked = false;
                _detDue.Enabled    = false;
            }
            _detDueChk.Enabled = canEdit;

            // Assignee (read-only; เปลี่ยนได้ผ่าน Edit Form เต็ม)
            _detAssignee.Text = asgn;

            // Description
            _detDesc.Text     = desc;
            _detDesc.ReadOnly = !canEdit;
            _detDesc.BackColor = canEdit ? Color.FromArgb(248, 250, 255) : Color.FromArgb(240, 244, 252);

            // Save / Edit buttons
            _btnDetSave.Enabled    = canEdit;
            _btnDetAddSub.Enabled  = true; // ทุกคนสร้าง sub ได้ถ้ายังไม่จำกัด

            // Sub tasks checklist
            _detSubsPanel.Controls.Clear();
            if (_childrenMap.ContainsKey(_detTaskId))
            {
                int sy = 4;
                foreach (var sub in _childrenMap[_detTaskId])
                {
                    bool done = sub["c_status"].ToString() == "Done";
                    var chk = new CheckBox
                    {
                        Location = new Point(4, sy), Size = new Size(_detSubsPanel.Width - 8, 22),
                        Text     = sub["c_task_code"].ToString() + "  " + sub["n_title"].ToString(),
                        Checked  = done, Font = new Font("Segoe UI", 8.5F),
                        ForeColor = done ? Color.FromArgb(40, 167, 69) : NavyDark,
                        Tag = sub["i_task_id"]
                    };
                    int subId   = Convert.ToInt32(sub["i_task_id"]);
                    int subSid  = Convert.ToInt32(sub["i_proj_state_id"]);
                    chk.CheckedChanged += (s, ev) =>
                    {
                        string newStat = ((CheckBox)s).Checked ? "Done" : "InProgress";
                        int    newPct  = ((CheckBox)s).Checked ? 100 : 50;
                        string err;
                        _taskSvc.UpdateStatus(subId, subSid, _currentProjectId, newStat, newPct, _userName, out err);
                        if (!string.IsNullOrEmpty(err))
                            MessageBox.Show(err, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        else LoadData();
                    };
                    _detSubsPanel.Controls.Add(chk);
                    sy += 26;
                }
                _detSubsPanel.Height = Math.Max(60, sy + 4);
            }
            else
            {
                _detSubsPanel.Controls.Add(new Label
                {
                    Text = "ยังไม่มี Sub Task", Location = new Point(4, 8),
                    Size = new Size(200, 22), Font = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.Gray
                });
                _detSubsPanel.Height = 60;
            }
        }

        private void BtnDetSave_Click(object sender, EventArgs e)
        {
            if (_detTaskId < 0) return;
            var r = _allRows.FirstOrDefault(x => Convert.ToInt32(x["i_task_id"]) == _detTaskId);
            if (r == null) return;

            string[] statusValues = { "Todo","InProgress","InReview","Done","Cancelled" };
            string newStatus = statusValues[Math.Max(0, _detStatus.SelectedIndex)];
            int    newPrio   = _detPriority.SelectedIndex + 1;
            int    newPct    = _detPctBar.Value;
            string newDesc   = _detDesc.Text.Trim();
            int    stateId   = Convert.ToInt32(r["i_proj_state_id"]);

            try
            {
                string err;
                _taskSvc.UpdateStatus(_detTaskId, stateId, _currentProjectId, newStatus, newPct, _userName, out err);
                if (!string.IsNullOrEmpty(err)) { MessageBox.Show(err, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                var model = new ProjectTaskModel
                {
                    i_task_id       = _detTaskId,
                    i_proj_state_id = stateId,
                    i_project_id    = _currentProjectId,
                    n_title         = r["n_title"].ToString(),
                    n_description   = newDesc,
                    i_priority      = newPrio,
                    c_status        = newStatus,
                    i_pct_complete  = newPct,
                    n_assignee      = r["n_assignee"] != DBNull.Value ? r["n_assignee"].ToString() : "",
                    d_due_date      = _detDueChk.Checked ? (DateTime?)_detDue.Value.Date : null,
                    i_update_user   = _userName
                };
                _taskCtrl.UpdateTask(model);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("บันทึกไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDetAddSub_Click(object sender, EventArgs e)
        {
            if (_detTaskId < 0) return;
            var row = _allRows.FirstOrDefault(r => Convert.ToInt32(r["i_task_id"]) == _detTaskId);
            if (row == null) return;
            int    stateId   = Convert.ToInt32(row["i_proj_state_id"]);
            string stateName = row["n_state_name"].ToString();
            string parentCode = row["c_task_code"].ToString();
            var frm = new Frm_Project_Task_Edit(
                _currentProjectId, stateId, stateName, null, _userName,
                parentTaskId: _detTaskId, parentTaskCode: parentCode);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                _expandedIds.Add(_detTaskId);
                LoadData();
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // HELPERS
        // ────────────────────────────────────────────────────────────────────────
        private void OpenSelectedTask()
        {
            DataRow row = GetSelectedRow();
            if (row == null) return;
            int taskId   = Convert.ToInt32(row["i_task_id"]);
            int stateId  = Convert.ToInt32(row["i_proj_state_id"]);
            string sName = row["n_state_name"].ToString();
            OpenDetailById(taskId, stateId, sName);
        }

        private void OpenDetailById(int taskId, int stateId, string stateName)
        {
            DataRow r = _allRows.FirstOrDefault(row => Convert.ToInt32(row["i_task_id"]) == taskId);
            if (r == null) return;

            var model = new ProjectTaskModel
            {
                i_task_id       = taskId,
                c_task_code     = r["c_task_code"].ToString(),
                i_proj_state_id = stateId,
                i_project_id    = _currentProjectId,
                n_title         = r["n_title"].ToString(),
                i_priority      = Convert.ToInt32(r["i_priority"]),
                c_status        = r["c_status"].ToString(),
                i_pct_complete  = Convert.ToInt32(r["i_pct_complete"]),
                n_assignee      = r["n_assignee"] != DBNull.Value ? r["n_assignee"].ToString() : "",
                d_due_date      = r["d_due_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["d_due_date"]) : null,
                n_description   = r["n_description"] != DBNull.Value ? r["n_description"].ToString() : ""
            };
            var frm = new Frm_Project_Task_Edit(_currentProjectId, stateId, stateName, model, _userName);
            if (frm.ShowDialog() == DialogResult.OK) LoadData();
        }

        private DataRow GetSelectedRow()
        {
            if (dgvTasks.CurrentRow == null || _visibleRows == null) return null;
            int idx = dgvTasks.CurrentRow.Index;
            if (_visibleRows.Rows.Count <= idx) return null;
            return _visibleRows.Rows[idx];
        }

        private void tabMain_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = tabMain.TabPages[e.Index];
            Color clr = (e.State == DrawItemState.Selected) ? NavyDark : NavyLight;
            using (var brush = new SolidBrush(clr)) e.Graphics.FillRectangle(brush, e.Bounds);
            SizeF sz = e.Graphics.MeasureString(page.Text, e.Font);
            float xp = e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2;
            float yp = e.Bounds.Top  + (e.Bounds.Height - sz.Height) / 2;
            e.Graphics.DrawString(page.Text, e.Font, Brushes.White, xp, yp);
        }

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabMain.SelectedIndex == 1) RefreshKanban();
        }

        private void ApplyHovers()
        {
            Hover(btnAddTask,      NavyMediumH, NavyMedium);
            Hover(btnRefresh,      SlateBlueH,  SlateBlue);
            Hover(btnAddSub,       NavyLightH,  NavyLight);
            Hover(btnEditTask,     SlateBlueH,  SlateBlue);
            Hover(btnDeleteTask,   DangerRedH,  DangerRed);
            Hover(btnChangeStatus, Color.FromArgb(130, 80, 210), PurpleCol);
            Hover(btnClose,        Color.FromArgb(90, 110, 140), Color.FromArgb(70, 90, 120));
        }

        private static void Hover(Button btn, Color on, Color off)
        {
            btn.MouseEnter += (s, e) => btn.BackColor = on;
            btn.MouseLeave += (s, e) => btn.BackColor = off;
        }
    }
}
