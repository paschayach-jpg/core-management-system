using CMS.Domain.Models;
using CMS.Controllers.RGP;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.RGP
{
    /// <summary>
    /// MOD-009 Task Management — แสดงทั้ง List View และ Kanban View
    /// ผูกกับ Project State (i_proj_state_id) หรือดูทั้ง Project (i_project_id)
    /// </summary>
    public partial class Frm_Project_Task_List : Form
    {
        private readonly int               _projectId;
        private readonly int               _projStateId;   // 0 = ทั้ง Project
        private readonly string            _stateName;
        private readonly string            _userName;
        private readonly ProjectTaskController _svc;
        private readonly ProjectStateController _stateSvc;

        // Kanban columns definition
        private static readonly (string Status, string Label, Color Color)[] _columns =
        {
            ("Todo",       "📌 รอดำเนินการ", Color.FromArgb(108, 117, 125)),
            ("InProgress", "🔄 กำลังทำ",      Color.FromArgb(255, 193, 7)),
            ("InReview",   "🔍 รอตรวจสอบ",    Color.FromArgb(111, 66, 193)),
            ("Done",       "✅ เสร็จแล้ว",     Color.FromArgb(40, 167, 69)),
            ("Cancelled",  "❌ ยกเลิก",        Color.FromArgb(220, 53, 69)),
        };

        // palette
        private static readonly Color NavyDark   = Color.FromArgb(30, 58, 95);
        private static readonly Color NavyMedium = Color.FromArgb(43, 87, 154);
        private static readonly Color DangerRed  = Color.FromArgb(196, 53, 53);

        private DataTable _source;
        private Panel     _kanbanPanel;
        private DataGridView _dgvList;

        // ── Drag-and-Drop state ─────────────────────────────────────────────────
        private int    _dragTaskId   = -1;
        private int    _dragStateId  = -1;
        private string _dragStatus   = "";
        private Panel  _dragHighlightCol = null;   // column currently highlighted

        public Frm_Project_Task_List(int projectId, int projStateId = 0, string stateName = "")
        {
            InitializeComponent();
            _projectId   = projectId;
            _projStateId = projStateId;
            _stateName   = stateName;
            _userName    = RgpSession.Current?.ADUserName;
            _svc         = new ProjectTaskController();
            _stateSvc    = new ProjectStateController();
        }

        private void Frm_Project_Task_List_Load(object sender, EventArgs e)
        {
            // ต้องเรียก SetupTabs ก่อน LoadData เพื่อให้ _dgvList และ _kanbanPanel พร้อมใช้
            SetupTabs();

            lblTitle.Text = _projStateId > 0
                ? $"Tasks — {_stateName}"
                : "Tasks — ทั้งโครงการ";

            LoadAssigneeCombo();
            LoadStatusCombo();
            LoadData();
            tabView.SelectedIndex = 0; // List View
        }

        // ─── Data ─────────────────────────────────────────────────────────────────

        private void LoadData()
        {
            _source = _projStateId > 0
                ? _svc.GetByState(_projStateId)
                : _svc.GetByProject(_projectId);

            RefreshListView();
            RefreshKanban();
            UpdateSummary();
        }

        private void LoadAssigneeCombo()
        {
            // ดึงจาก state members ถ้า state ระบุ, ไม่งั้นดึง project members
            DataTable dt = _projStateId > 0
                ? new ProjectStateMemberController().GetByState(_projStateId)
                : new ProjectMemberController().GetByProject(_projectId);

            // อนุญาต NULL สำหรับทุก column ก่อนเพิ่ม blank row
            foreach (DataColumn col in dt.Columns)
            {
                if (!col.AllowDBNull)
                    col.AllowDBNull = true;
            }

            // เพิ่ม blank row
            DataRow blank = dt.NewRow();
            dt.Rows.InsertAt(blank, 0);

            cbbFilterAssignee.SelectedIndexChanged -= cbbFilterAssignee_SelectedIndexChanged;
            cbbFilterAssignee.DataSource = dt;
            cbbFilterAssignee.DisplayMember = "n_member_name";
            cbbFilterAssignee.SelectedIndex = 0;
            cbbFilterAssignee.SelectedIndexChanged += cbbFilterAssignee_SelectedIndexChanged;
        }

        private void LoadStatusCombo()
        {
            cbbFilterStatus.Items.Clear();
            cbbFilterStatus.Items.Add("--- ทุกสถานะ ---");
            foreach (var col in _columns)
                cbbFilterStatus.Items.Add(col.Status);
            cbbFilterStatus.SelectedIndex = 0;
        }

        private void UpdateSummary()
        {
            if (_source == null) { lblSummary.Text = ""; return; }
            var rows = _source.AsEnumerable().ToList();
            int total    = rows.Count;
            int done     = rows.Count(r => r["c_status"].ToString() == "Done");
            int overdue  = rows.Count(r => Convert.ToInt32(r["b_overdue"]) == 1);
            int slaWarn  = rows.Count(r => Convert.ToInt32(r["b_sla_warning"]) == 1);
            lblSummary.Text =
                $"ทั้งหมด: {total}  |  เสร็จ: {done}  |  เกินกำหนด: {overdue}  |  ใกล้ครบ: {slaWarn}";
        }

        // ─── Filter ───────────────────────────────────────────────────────────────

        private void ApplyFilter()
        {
            if (_source == null) return;
            var filters = new List<string>();

            if (cbbFilterStatus.SelectedIndex > 0)
                filters.Add($"c_status = '{cbbFilterStatus.SelectedItem}'");

            if (cbbFilterAssignee.SelectedIndex > 0 && cbbFilterAssignee.SelectedValue != null)
            {
                string name = cbbFilterAssignee.Text.Replace("'", "''");
                if (!string.IsNullOrEmpty(name))
                    filters.Add($"n_assignee LIKE '%{name}%'");
            }

            string kw = txtSearch.Text.Trim().Replace("'", "''");
            if (!string.IsNullOrEmpty(kw))
                filters.Add($"n_title LIKE '%{kw}%'");

            _source.DefaultView.RowFilter = string.Join(" AND ", filters);
            RefreshListView();
            RefreshKanban();
            UpdateSummary();
        }

        // ─── List View ────────────────────────────────────────────────────────────

        private void RefreshListView()
        {
            if (_dgvList == null) return;
            _dgvList.DataSource = null;
            _dgvList.DataSource = _source;
        }

        private void SetupListView()
        {
            _dgvList = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                AutoGenerateColumns   = false,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                RowHeadersVisible     = false,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor             = Color.FromArgb(208, 220, 240),
                RowTemplate           = { Height = 30 }
            };

            var colCode  = new DataGridViewTextBoxColumn { HeaderText = "รหัส",       DataPropertyName = "c_task_code",    Width = 110 };
            var colTitle = new DataGridViewTextBoxColumn { HeaderText = "หัวข้องาน",  DataPropertyName = "n_title",        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            var colState = new DataGridViewTextBoxColumn { HeaderText = "ขั้นตอน",   DataPropertyName = "n_state_name",   Width = 130 };
            var colAsgn  = new DataGridViewTextBoxColumn { HeaderText = "ผู้รับผิดชอบ", DataPropertyName = "n_assignee",  Width = 130 };
            var colPrio  = new DataGridViewTextBoxColumn { HeaderText = "ความสำคัญ", DataPropertyName = "i_priority",     Width = 90  };
            var colStat  = new DataGridViewTextBoxColumn { HeaderText = "สถานะ",      DataPropertyName = "c_status",       Width = 110 };
            var colPct   = new DataGridViewProgressBarColumn { HeaderText = "% เสร็จ", DataPropertyName = "i_pct_complete", Width = 90 };
            var colDue   = new DataGridViewTextBoxColumn { HeaderText = "กำหนดเสร็จ", DataPropertyName = "d_due_date",     Width = 100 };

            colPrio.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colStat.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDue.DefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleCenter;
            colDue.DefaultCellStyle.Format     = "dd/MM/yyyy";

            _dgvList.Columns.AddRange(colCode, colTitle, colState, colAsgn, colPrio, colStat, colPct, colDue);

            // header style
            var hStyle = _dgvList.ColumnHeadersDefaultCellStyle;
            hStyle.BackColor = NavyDark;
            hStyle.ForeColor = Color.White;
            hStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvList.ColumnHeadersHeight         = 30;
            _dgvList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _dgvList.EnableHeadersVisualStyles   = false;
            _dgvList.DefaultCellStyle.Font       = new Font("Segoe UI", 9F);
            _dgvList.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 244, 255);

            _dgvList.CellFormatting    += DgvList_CellFormatting;
            _dgvList.CellDoubleClick   += DgvList_CellDoubleClick;
            _dgvList.SelectionChanged  += DgvList_SelectionChanged;

            tabListView.Controls.Add(_dgvList);
        }

        private void DgvList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || _source == null) return;

            // ต้องอ่านผ่าน DefaultView เพราะอาจมี RowFilter active
            if (_source.DefaultView.Count <= e.RowIndex) return;
            DataRow r = _source.DefaultView[e.RowIndex].Row;
            string prop = _dgvList.Columns[e.ColumnIndex].DataPropertyName;

            bool overdue = Convert.ToInt32(r["b_overdue"]) == 1;
            bool sla     = Convert.ToInt32(r["b_sla_warning"]) == 1;

            // สถานะสี
            if (prop == "c_status")
            {
                switch (e.Value?.ToString())
                {
                    case "Done":
                        e.CellStyle.BackColor = Color.FromArgb(198, 239, 206);
                        e.CellStyle.ForeColor = Color.FromArgb(0, 97, 0);
                        e.Value = "✅ เสร็จ"; e.FormattingApplied = true; break;
                    case "InProgress":
                        e.CellStyle.BackColor = Color.FromArgb(255, 235, 156);
                        e.CellStyle.ForeColor = Color.FromArgb(99, 71, 0);
                        e.Value = "🔄 กำลังทำ"; e.FormattingApplied = true; break;
                    case "InReview":
                        e.CellStyle.BackColor = Color.FromArgb(220, 208, 255);
                        e.CellStyle.ForeColor = Color.FromArgb(68, 0, 153);
                        e.Value = "🔍 รอตรวจ"; e.FormattingApplied = true; break;
                    case "Cancelled":
                        e.CellStyle.BackColor = Color.FromArgb(255, 199, 206);
                        e.CellStyle.ForeColor = Color.FromArgb(156, 0, 6);
                        e.Value = "❌ ยกเลิก"; e.FormattingApplied = true; break;
                    default:
                        e.CellStyle.BackColor = Color.FromArgb(215, 228, 248);
                        e.CellStyle.ForeColor = Color.FromArgb(30, 58, 95);
                        e.Value = "📌 รอทำ"; e.FormattingApplied = true; break;
                }
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }

            // priority label
            if (prop == "i_priority" && e.Value != null)
            {
                int p = Convert.ToInt32(e.Value);
                e.Value = ProjectTaskModel.PriorityLabel(p);
                e.FormattingApplied = true;
                switch (p)
                {
                    case 4: e.CellStyle.ForeColor = Color.FromArgb(156, 0, 6); e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold); break;
                    case 3: e.CellStyle.ForeColor = Color.FromArgb(196, 53, 53); break;
                    case 1: e.CellStyle.ForeColor = Color.FromArgb(100, 130, 168); break;
                }
            }

            // due date — สีแดงถ้า overdue
            if (prop == "d_due_date" && overdue)
            {
                e.CellStyle.ForeColor = Color.FromArgb(196, 53, 53);
                e.CellStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            }

            // SLA warning row tint
            if (sla && !overdue)
                e.CellStyle.BackColor = Color.FromArgb(255, 248, 220); // สีเหลืองอ่อน
            if (overdue)
                e.CellStyle.BackColor = Color.FromArgb(255, 235, 235); // สีแดงอ่อน
        }

        private void DgvList_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSel = _dgvList?.CurrentRow != null;
            btnEdit.Enabled   = hasSel;
            btnDelete.Enabled = hasSel;
            btnStatus.Enabled = hasSel;
        }

        private void DgvList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) OpenDetail();
        }

        // ─── Kanban View ──────────────────────────────────────────────────────────

        private void RefreshKanban()
        {
            if (_kanbanPanel == null) return;
            _kanbanPanel.Controls.Clear();
            if (_source == null) return;

            int colW = 240;
            int gap  = 10;
            int x    = gap;
            int kanbanH = _kanbanPanel.ClientSize.Height > 0
                        ? _kanbanPanel.ClientSize.Height - gap * 2 : 500;

            foreach (var col in _columns)
            {
                var rows = _source.AsEnumerable()
                    .Where(r => r.RowState != DataRowState.Deleted &&
                                r["c_status"].ToString() == col.Status)
                    .ToList();

                var colPanel = BuildKanbanColumn(col.Status, col.Label, col.Color, rows, colW, kanbanH);
                colPanel.Location = new Point(x, gap);
                _kanbanPanel.Controls.Add(colPanel);
                x += colW + gap;
            }
            _kanbanPanel.AutoScrollMinSize = new Size(x + gap, 0);
        }

        private Panel BuildKanbanColumn(string status, string label, Color headerColor,
                                        List<DataRow> rows, int colW, int colH)
        {
            var panel = new Panel
            {
                Width       = colW,
                Height      = colH,
                BackColor   = Color.FromArgb(235, 240, 248),
                BorderStyle = BorderStyle.None,
                AutoScroll  = true,
                Tag         = status,
                AllowDrop   = true
            };

            // ─ Column header ─────────────────────────────────────────────────────
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 36,
                BackColor = headerColor,
                Padding   = new Padding(8, 0, 8, 0)
            };
            var lblCount = new Label
            {
                Text      = rows.Count.ToString(),
                Dock      = DockStyle.Right,
                Width     = 28,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(0, 0, 0, 60)
            };
            var lblCol = new Label
            {
                Text      = label,
                Dock      = DockStyle.Fill,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(lblCount);
            header.Controls.Add(lblCol);
            panel.Controls.Add(header);

            // ─ Drop-zone hint (hidden unless dragging) ───────────────────────────
            var dropHint = new Label
            {
                Name      = "dropHint",
                Text      = "วางที่นี่",
                Dock      = DockStyle.Bottom,
                Height    = 30,
                ForeColor = headerColor,
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(headerColor.R, headerColor.G, headerColor.B, 30),
                Visible   = false
            };
            panel.Controls.Add(dropHint);

            // ─ Cards ─────────────────────────────────────────────────────────────
            int cardY = 42;
            foreach (var row in rows)
            {
                var card = BuildCard(row, headerColor, colW - 18, status);
                card.Location = new Point(8, cardY);
                panel.Controls.Add(card);
                cardY += card.Height + 8;
            }

            // ─ Drag-and-Drop events on column ────────────────────────────────────
            panel.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(typeof(string)))
                {
                    string payload = e.Data.GetData(typeof(string)) as string;
                    if (payload != null && payload.StartsWith("TASK|"))
                    {
                        string colStatus = ((Panel)s).Tag?.ToString();
                        string curStatus = payload.Split('|')[3];
                        if (colStatus != curStatus)
                        {
                            e.Effect = DragDropEffects.Move;
                            HighlightColumn((Panel)s, headerColor, dropHint, true);
                            return;
                        }
                    }
                }
                e.Effect = DragDropEffects.None;
            };

            panel.DragLeave += (s, e) =>
            {
                HighlightColumn((Panel)s, headerColor, dropHint, false);
            };

            panel.DragDrop += (s, e) =>
            {
                HighlightColumn((Panel)s, headerColor, dropHint, false);
                if (!e.Data.GetDataPresent(typeof(string))) return;

                string payload = e.Data.GetData(typeof(string)) as string;
                if (payload == null || !payload.StartsWith("TASK|")) return;

                // TASK|taskId|stateId|currentStatus
                string[] parts = payload.Split('|');
                if (parts.Length < 4) return;

                int    taskId    = int.Parse(parts[1]);
                int    stateId   = int.Parse(parts[2]);
                string newStatus = ((Panel)s).Tag?.ToString();
                if (string.IsNullOrEmpty(newStatus)) return;

                // % auto-map
                int pct = newStatus == "Done" ? 100
                        : newStatus == "Todo" ? 0
                        : newStatus == "InProgress" ? 50
                        : newStatus == "InReview" ? 80
                        : 0;

                string err;
                _svc.UpdateStatus(taskId, stateId, _projectId, newStatus, pct, _userName, out err);
                if (!string.IsNullOrEmpty(err))
                    MessageBox.Show(err, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    LoadData();
            };

            return panel;
        }

        private void HighlightColumn(Panel col, Color baseColor, Label dropHint, bool on)
        {
            if (on)
            {
                col.BackColor = Color.FromArgb(
                    Math.Min(baseColor.R + 40, 255),
                    Math.Min(baseColor.G + 40, 255),
                    Math.Min(baseColor.B + 40, 255));
                dropHint.Visible = true;
            }
            else
            {
                col.BackColor    = Color.FromArgb(235, 240, 248);
                dropHint.Visible = false;
            }
        }

        private Panel BuildCard(DataRow r, Color accentColor, int width, string colStatus)
        {
            bool overdue = Convert.ToInt32(r["b_overdue"])     == 1;
            bool sla     = Convert.ToInt32(r["b_sla_warning"]) == 1;
            int  prio    = Convert.ToInt32(r["i_priority"]);
            int  taskId  = Convert.ToInt32(r["i_task_id"]);
            int  stateId = Convert.ToInt32(r["i_proj_state_id"]);
            string code  = r["c_task_code"].ToString();
            string due   = r["d_due_date"] != DBNull.Value
                ? Convert.ToDateTime(r["d_due_date"]).ToString("dd/MM/yy") : "-";
            string stateName = r["n_state_name"] != DBNull.Value
                ? r["n_state_name"].ToString() : "";
            string assignee  = r["n_assignee"] != DBNull.Value
                ? r["n_assignee"].ToString() : "-";

            Color cardBg = overdue ? Color.FromArgb(255, 235, 235)
                         : sla     ? Color.FromArgb(255, 250, 225)
                         :           Color.White;

            Color stripeColor;
            switch (prio)
            {
                case 4: stripeColor = Color.FromArgb(196, 53,  53);  break;
                case 3: stripeColor = Color.FromArgb(255, 140,  0);  break;
                case 1: stripeColor = Color.FromArgb(100, 130, 168); break;
                default: stripeColor = accentColor; break;
            }

            var card = new Panel
            {
                Width       = width,
                Height      = 106,
                BackColor   = cardBg,
                Cursor      = Cursors.SizeAll,
                BorderStyle = BorderStyle.None,
                Tag         = taskId
            };

            // Left priority stripe
            var stripe = new Panel { Width = 5, Dock = DockStyle.Left, BackColor = stripeColor };
            card.Controls.Add(stripe);

            // Card content
            var inner = new Panel
            {
                Left      = 7,
                Top       = 0,
                Width     = width - 9,
                Height    = 106,
                BackColor = cardBg,
                Tag       = taskId
            };

            // Row 1: task code + priority badge
            var lblCode = new Label
            {
                Text      = code,
                Location  = new Point(0, 6),
                Size      = new Size(120, 16),
                Font      = new Font("Segoe UI", 7.5F),
                ForeColor = Color.FromArgb(120, 140, 170)
            };
            string prioEmoji = prio == 4 ? "🔴" : prio == 3 ? "🟠" : prio == 1 ? "🟢" : "🟡";
            var lblPrio = new Label
            {
                Text      = prioEmoji + " " + ProjectTaskModel.PriorityLabel(prio),
                Location  = new Point(width - 90, 4),
                Size      = new Size(80, 16),
                Font      = new Font("Segoe UI", 7.5F),
                ForeColor = Color.FromArgb(90, 110, 140),
                TextAlign = ContentAlignment.MiddleRight
            };

            // Row 2: title
            var lblCardTitle = new Label
            {
                Text      = r["n_title"].ToString(),
                Location  = new Point(0, 24),
                Size      = new Size(width - 10, 36),
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95)
            };

            // Row 3: state tag
            var lblState = new Label
            {
                Text      = "📂 " + stateName,
                Location  = new Point(0, 62),
                Size      = new Size(width - 10, 16),
                Font      = new Font("Segoe UI", 7.5F),
                ForeColor = Color.FromArgb(80, 100, 130)
            };

            // Row 4: assignee + due
            var lblAssignee = new Label
            {
                Text      = "👤 " + assignee,
                Location  = new Point(0, 82),
                Size      = new Size((width - 10) / 2, 18),
                Font      = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(60, 80, 120)
            };
            string duePrefix = overdue ? "⚠️ " : sla ? "🕐 " : "📅 ";
            var lblDue = new Label
            {
                Text      = duePrefix + due,
                Location  = new Point((width - 10) / 2, 82),
                Size      = new Size((width - 10) / 2, 18),
                Font      = new Font("Segoe UI", 8F),
                ForeColor = overdue ? Color.FromArgb(196, 53, 53) : Color.FromArgb(80, 100, 130),
                TextAlign = ContentAlignment.MiddleRight
            };

            inner.Controls.AddRange(new Control[] {
                lblCode, lblPrio, lblCardTitle, lblState, lblAssignee, lblDue
            });
            card.Controls.Add(inner);

            // ─ Drag-and-Drop ─────────────────────────────────────────────────────
            // payload: TASK|taskId|stateId|currentStatus
            string payload = $"TASK|{taskId}|{stateId}|{colStatus}";

            EventHandler startDrag = (s, e) =>
            {
                var src = (Control)s;
                // เริ่ม drag จาก card หรือ child control
                src.DoDragDrop(payload, DragDropEffects.Move);
            };

            card.MouseDown  += (s, e) => { if (e.Button == MouseButtons.Left) startDrag(s, e); };
            inner.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) startDrag(s, e); };
            foreach (Control c in inner.Controls)
                c.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) startDrag(s, e); };

            // DoubleClick = open detail
            EventHandler openDetail = (s, e) => OpenDetailById(taskId);
            card.DoubleClick  += openDetail;
            inner.DoubleClick += openDetail;
            foreach (Control c in inner.Controls) c.DoubleClick += openDetail;

            return card;
        }

        // ─── Actions ─────────────────────────────────────────────────────────────

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // เลือก State ถ้าดูทั้ง project
            int stateId = _projStateId;
            string stateName = _stateName;
            if (stateId == 0)
            {
                // ให้ user เลือก state ผ่าน combo
                using (var dlg = new Frm_Select_StateForTask(_projectId, _stateSvc))
                {
                    if (dlg.ShowDialog() != DialogResult.OK) return;
                    stateId   = dlg.SelectedStateId;
                    stateName = dlg.SelectedStateName;
                }
            }

            var frm = new Frm_Project_Task_Edit(_projectId, stateId, stateName, null, _userName);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
                if (_projStateId > 0) _svc.SyncStatePct(stateId, _projectId, _userName);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e) => OpenDetail();

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int id = GetSelectedTaskId();
            if (id <= 0) return;
            string title = _dgvList.CurrentRow?.Cells["n_title"]?.Value?.ToString() ?? "";
            if (MessageBox.Show($"ต้องการลบ Task '{title}'?\n(ย้ายไป Recycle Bin 30 วัน)",
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _svc.SoftDelete(id, _userName);
                LoadData();
            }
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            int id = GetSelectedTaskId();
            if (id <= 0) return;
            DataRow r = GetSelectedRow();
            if (r == null) return;

            string curStatus = r["c_status"].ToString();
            int    curPct    = Convert.ToInt32(r["i_pct_complete"]);
            int    stateId   = Convert.ToInt32(r["i_proj_state_id"]);

            using (var dlg = new Frm_Project_Task_Status(id, curStatus, curPct))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string err;
                    _svc.UpdateStatus(id, stateId, _projectId,
                        dlg.NewStatus, dlg.NewPct, _userName, out err);
                    if (!string.IsNullOrEmpty(err))
                        MessageBox.Show(err, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadData();
                }
            }
        }

        private void OpenDetail()
        {
            int id = GetSelectedTaskId();
            if (id <= 0) return;
            OpenDetailById(id);
        }

        private void OpenDetailById(int taskId)
        {
            DataRow r = _source?.AsEnumerable()
                .FirstOrDefault(row => Convert.ToInt32(row["i_task_id"]) == taskId);
            if (r == null) return;

            int    stateId   = Convert.ToInt32(r["i_proj_state_id"]);
            string stateName = r["n_state_name"].ToString();

            var model = DataRowToModel(r);
            var frm   = new Frm_Project_Task_Edit(_projectId, stateId, stateName, model, _userName);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
                _svc.SyncStatePct(stateId, _projectId, _userName);
            }
        }

        private int GetSelectedTaskId()
        {
            if (_dgvList?.CurrentRow == null || _source == null) return 0;
            int idx = _dgvList.CurrentRow.Index;
            if (_source.DefaultView.Count <= idx) return 0;
            return Convert.ToInt32(_source.DefaultView[idx]["i_task_id"]);
        }

        private DataRow GetSelectedRow()
        {
            if (_dgvList?.CurrentRow == null || _source == null) return null;
            int idx = _dgvList.CurrentRow.Index;
            if (_source.DefaultView.Count <= idx) return null;
            return _source.DefaultView[idx].Row;
        }

        private ProjectTaskModel DataRowToModel(DataRow r)
        {
            return new ProjectTaskModel
            {
                i_task_id        = Convert.ToInt32(r["i_task_id"]),
                c_task_code      = r["c_task_code"].ToString(),
                i_proj_state_id  = Convert.ToInt32(r["i_proj_state_id"]),
                i_project_id     = _projectId,
                n_title          = r["n_title"].ToString(),
                n_description    = r["n_description"] != DBNull.Value ? r["n_description"].ToString() : "",
                i_priority       = Convert.ToInt32(r["i_priority"]),
                c_status         = r["c_status"].ToString(),
                i_pct_complete   = Convert.ToInt32(r["i_pct_complete"]),
                n_assignee       = r["n_assignee"] != DBNull.Value ? r["n_assignee"].ToString() : "",
                d_due_date       = r["d_due_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["d_due_date"]) : null,
                d_start_date     = r["d_start_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["d_start_date"]) : null,
                i_estimated_hours = r["i_estimated_hours"] != DBNull.Value ? (int?)Convert.ToInt32(r["i_estimated_hours"]) : null,
            };
        }

        // ─── Tab switch ───────────────────────────────────────────────────────────

        private void tabView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabView.SelectedIndex == 1) RefreshKanban(); // Kanban tab
        }

        // ─── Filter events ────────────────────────────────────────────────────────
        private void txtSearch_TextChanged(object sender, EventArgs e)      => ApplyFilter();
        private void cbbFilterStatus_SelectedIndexChanged(object sender, EventArgs e) => ApplyFilter();
        private void cbbFilterAssignee_SelectedIndexChanged(object sender, EventArgs e) => ApplyFilter();

        private void btnRefresh_Click(object sender, EventArgs e) => LoadData();

        // ─── Tab setup (called from InitializeComponent supplement) ──────────────
        private void SetupTabs()
        {
            SetupListView();

            // Kanban: ใช้ Panel เดียว AutoScroll=true
            // ไม่ซ้อน Dock=Fill สองชั้น ให้ _kanbanPanel resize ตาม content
            _kanbanPanel = new Panel { AutoScroll = true, Dock = DockStyle.Fill };
            tabKanbanView.Controls.Add(_kanbanPanel);
        }
    }
}
