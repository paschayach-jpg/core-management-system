using RG_Project.Project.Controllers;
using CMS.Domain.Models;
using RG_Project.Project.Services;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace RG_Project.Project.Forms
{
    public partial class Frm_Project_Task_Edit : Form
    {
        private readonly int               _projectId;
        private readonly int               _projStateId;
        private readonly string            _stateName;
        private readonly ProjectTaskModel  _editModel;
        private readonly string            _userName;
        private readonly ProjectTaskService _svc;
        private readonly int?              _parentTaskId;   // set when creating sub task
        private readonly string            _parentTaskCode; // display only

        // ── Comment UI (built in code, not Designer) ─────────────────────────
        private Panel       _pnlComments;      // scrollable comment list
        private TextBox     _txtNewComment;
        private Button      _btnPostComment;
        private int?        _replyToCommentId = null;  // null = top-level
        private int         _replyToDepth     = 0;     // depth ของ comment ที่จะ reply
        private Label       _lblReplyHint;

        private static readonly Color NavyDark   = Color.FromArgb(30, 58, 95);
        private static readonly Color NavyMedium = Color.FromArgb(43, 87, 154);

        // static fonts shared across all comment cards (created once, never disposed)
        private static readonly Font _fntMeta        = new Font("Segoe UI", 8F, FontStyle.Bold);
        private static readonly Font _fntContent     = new Font("Segoe UI", 9F);
        private static readonly Font _fntContentDel  = new Font("Segoe UI", 9F, FontStyle.Italic);

        public Frm_Project_Task_Edit() { InitializeComponent(); _svc = new ProjectTaskService(); }

        public Frm_Project_Task_Edit(int projectId, int projStateId, string stateName,
            ProjectTaskModel model, string userName,
            int? parentTaskId = null, string parentTaskCode = null)
        {
            InitializeComponent();
            _projectId      = projectId;
            _projStateId    = projStateId;
            _stateName      = stateName;
            _editModel      = model;
            _userName       = userName;
            _svc            = new ProjectTaskService();
            _parentTaskId   = parentTaskId;
            _parentTaskCode = parentTaskCode;
        }

        private void Frm_Project_Task_Edit_Load(object sender, EventArgs e)
        {
            lblState.Text = _parentTaskId.HasValue
                ? $"{_stateName}  ▪  Sub Task ของ: {_parentTaskCode ?? "Task หลัก"}"
                : _stateName;
            LoadAssignees();

            if (_editModel != null)
            {
                this.Text         = "แก้ไข Task";
                lblTitleBar.Text  = $"แก้ไข Task — {_editModel.c_task_code}";
                txtTitle.Text     = _editModel.n_title;
                txtDesc.Text      = _editModel.n_description ?? "";
                cbbPriority.SelectedIndex = _editModel.i_priority - 1;
                nudPct.Value      = _editModel.i_pct_complete;

                string assigneeLower = (_editModel.n_assignee ?? "").ToLower();
                for (int i = 0; i < cbbAssignee.Items.Count; i++)
                {
                    if (cbbAssignee.Items[i].ToString().ToLower() == assigneeLower)
                    { cbbAssignee.SelectedIndex = i; break; }
                }

                if (_editModel.d_due_date.HasValue)
                { dtpDue.Checked = true; dtpDue.Value = _editModel.d_due_date.Value; }
                else dtpDue.Checked = false;

                if (_editModel.d_start_date.HasValue)
                { dtpStart.Checked = true; dtpStart.Value = _editModel.d_start_date.Value; }
                else dtpStart.Checked = false;

                if (_editModel.i_estimated_hours.HasValue)
                    nudHours.Value = _editModel.i_estimated_hours.Value;

                // สร้าง Comment tab เฉพาะโหมด Edit
                BuildCommentPanel();
                LoadComments();
            }
            else
            {
                this.Text        = "สร้าง Task ใหม่";
                lblTitleBar.Text = "สร้าง Task ใหม่";
                cbbPriority.SelectedIndex = 1;
                dtpDue.Checked   = false;
                dtpStart.Checked = false;
            }
        }

        private void LoadAssignees()
        {
            cbbAssignee.Items.Clear();
            cbbAssignee.Items.Add("");

            bool loaded = false;

            // โหลดจาก Common DB ผ่าน EmployeeController
            try
            {
                DataTable dt = new EmployeeController().GetActiveEmployees();
                foreach (DataRow r in dt.Rows)
                    cbbAssignee.Items.Add(r["full_name"].ToString());
                loaded = cbbAssignee.Items.Count > 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadAssignees error: " + ex.Message);
            }

            // Fallback: ดึงจาก state members ถ้า Common DB ใช้ไม่ได้
            if (!loaded)
            {
                try
                {
                    DataTable dt = new ProjectStateMemberController().GetByState(_projStateId);
                    foreach (DataRow r in dt.Rows)
                        cbbAssignee.Items.Add(r["n_member_name"].ToString().ToLower());
                }
                catch { /* ไม่มีสมาชิก — ปล่อยว่าง */ }
            }

            cbbAssignee.SelectedIndex = 0;
        }

        // ─── Comment Panel ────────────────────────────────────────────────────

        private void BuildCommentPanel()
        {
            // ป้องกัน build ซ้ำ — ถ้ามี tab Comments อยู่แล้วข้ามได้เลย
            if (tabComments.TabPages.Count > 1) return;

            // Tab 2 — Comments (Tab 0 = tabTaskInfo มาจาก Designer แล้ว)
            var tabComment = new TabPage
            {
                Text      = "💬 Comments",
                BackColor = Color.FromArgb(242, 246, 252)
            };

            // ── list area ──────────────────────────────────────────────────────
            _pnlComments = new Panel
            {
                Dock        = DockStyle.Fill,
                AutoScroll  = true,
                BackColor   = Color.FromArgb(248, 250, 254),
                Padding     = new Padding(8, 6, 8, 6)
            };

            // ── reply hint ────────────────────────────────────────────────────
            _lblReplyHint = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 22,
                Text      = "",
                ForeColor = Color.FromArgb(43, 87, 154),
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                Visible   = false,
                Padding   = new Padding(6, 3, 0, 0)
            };

            // ── input area ────────────────────────────────────────────────────
            var pnlInput = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 80,
                BackColor = Color.FromArgb(229, 236, 248),
                Padding   = new Padding(6, 4, 6, 6)
            };

            _txtNewComment = new TextBox
            {
                Dock        = DockStyle.Fill,
                Multiline   = true,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 9F),
                ScrollBars  = ScrollBars.Vertical,
                MaxLength   = 2000
            };
            _txtNewComment.KeyDown += (s, ev) =>
            {
                if (ev.Control && ev.KeyCode == Keys.Enter) PostComment();
            };

            _btnPostComment = new Button
            {
                Text      = "ส่ง",
                Dock      = DockStyle.Right,
                Width     = 60,
                BackColor = NavyMedium,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            _btnPostComment.FlatAppearance.BorderSize = 0;
            _btnPostComment.Click += (s, ev) => PostComment();

            var btnCancelReply = new Button
            {
                Text      = "✕",
                Dock      = DockStyle.Right,
                Width     = 28,
                BackColor = Color.FromArgb(180, 180, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Visible   = false
            };
            btnCancelReply.FlatAppearance.BorderSize = 0;
            btnCancelReply.Name = "btnCancelReply";
            btnCancelReply.Click += (s, ev) =>
            {
                _replyToCommentId      = null;
                _replyToDepth          = 0;
                _lblReplyHint.Text     = "";
                _lblReplyHint.Visible  = false;
                btnCancelReply.Visible = false;
            };

            pnlInput.Controls.Add(_txtNewComment);
            pnlInput.Controls.Add(_btnPostComment);
            pnlInput.Controls.Add(btnCancelReply);

            tabComment.Controls.Add(_pnlComments);
            tabComment.Controls.Add(_lblReplyHint);
            tabComment.Controls.Add(pnlInput);
            tabComments.TabPages.Add(tabComment);
        }

        private void LoadComments()
        {
            if (_editModel == null || _pnlComments == null) return;
            _pnlComments.Controls.Clear();

            DataTable dt = _svc.GetComments(_editModel.i_task_id);
            if (dt.Rows.Count == 0)
            {
                _pnlComments.Controls.Add(new Label
                {
                    Text      = "ยังไม่มี Comment",
                    ForeColor = Color.Gray,
                    Font      = new Font("Segoe UI", 9F, FontStyle.Italic),
                    AutoSize  = true,
                    Location  = new Point(10, 10)
                });
                return;
            }

            int y = 4;
            // แสดง top-level comments (depth = 0) แล้ว replies ตาม
            foreach (DataRow row in dt.Rows)
            {
                if (row["i_parent_comment_id"] != DBNull.Value) continue; // skip replies ในรอบนี้
                int commentId = Convert.ToInt32(row["i_comment_id"]);
                var card = BuildCommentCard(row, 0, commentId);
                card.Location = new Point(0, y);
                _pnlComments.Controls.Add(card);
                y += card.Height + 4;

                // replies
                foreach (DataRow reply in dt.Rows)
                {
                    if (reply["i_parent_comment_id"] == DBNull.Value) continue;
                    if (Convert.ToInt32(reply["i_parent_comment_id"]) != commentId) continue;
                    var replyCard = BuildCommentCard(reply, 1, Convert.ToInt32(reply["i_comment_id"]));
                    replyCard.Location = new Point(24, y); // indent
                    _pnlComments.Controls.Add(replyCard);
                    y += replyCard.Height + 4;
                }
            }
            _pnlComments.AutoScrollMinSize = new Size(0, y + 8);
        }

        private Panel BuildCommentCard(DataRow row, int depth, int commentId)
        {
            bool isDeleted = row["f_is_deleted"]?.ToString() == "Y";
            bool isEdited  = row["f_is_edited"]?.ToString()  == "Y";
            bool isMine    = row["i_create_user"]?.ToString() == _userName;
            string content = isDeleted ? "[Comment ถูกลบ]" : row["n_content"]?.ToString() ?? "";
            string author  = row["i_create_user"]?.ToString() ?? "";
            string dateStr = row["d_create_date"] != DBNull.Value
                ? Convert.ToDateTime(row["d_create_date"]).ToString("dd/MM/yy HH:mm") : "";
            string metaText = $"👤 {author}  {dateStr}" + (isEdited ? "  ✎" : "");

            int cardW  = Math.Max(_pnlComments.ClientSize.Width - (depth == 0 ? 16 : 44), 200);
            int innerW = cardW - 20;  // เว้น padding ซ้าย-ขวา
            Color bgColor = depth == 0 ? Color.White : Color.FromArgb(240, 244, 252);

            // ── คำนวณความสูงข้อความ ─────────────────────────────────────────
            Font metaFont    = _fntMeta;
            Font contentFont = isDeleted ? _fntContentDel : _fntContent;

            int metaH    = TextRenderer.MeasureText(metaText, metaFont,
                               new Size(innerW, int.MaxValue),
                               TextFormatFlags.WordBreak).Height + 4;
            int contentH = TextRenderer.MeasureText(content, contentFont,
                               new Size(innerW, int.MaxValue),
                               TextFormatFlags.WordBreak).Height + 6;
            int btnH     = 28;
            int pad      = 8;  // top + bottom padding

            int totalH   = pad + metaH + contentH + btnH + pad;

            // ── card panel ───────────────────────────────────────────────────
            var card = new Panel
            {
                Width       = cardW,
                Height      = totalH,
                BackColor   = isDeleted ? Color.FromArgb(245, 245, 245) : bgColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            int y = pad;

            // meta line (Font ownership ย้ายให้ label — label dispose font เมื่อ card ถูก dispose)
            var lblMeta = new Label
            {
                Text      = metaText,
                Font      = metaFont,
                ForeColor = isMine ? NavyMedium : Color.FromArgb(60, 80, 110),
                Location  = new Point(8, y),
                Size      = new Size(innerW, metaH),
                AutoSize  = false
            };
            y += metaH;

            // content
            var lblContent = new Label
            {
                Text      = content,
                Font      = contentFont,
                ForeColor = isDeleted ? Color.Gray : Color.FromArgb(30, 40, 60),
                Location  = new Point(8, y),
                Size      = new Size(innerW, contentH),
                AutoSize  = false
            };
            y += contentH;

            // action buttons
            int bx = 8;
            if (!isDeleted && depth < 2)
            {
                var btnReply = new Button
                {
                    Text      = "↩ Reply",
                    Location  = new Point(bx, y),
                    Size      = new Size(70, 22),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(215, 228, 248),
                    ForeColor = NavyDark,
                    Font      = new Font("Segoe UI", 8F),
                    Cursor    = Cursors.Hand
                };
                btnReply.FlatAppearance.BorderSize = 0;
                int thisDepth = depth; // capture สำหรับ lambda
                btnReply.Click += (s, ev) =>
                {
                    _replyToCommentId     = commentId;
                    _replyToDepth         = thisDepth;
                    _lblReplyHint.Text    = $"  ↩ ตอบกลับ: {author}";
                    _lblReplyHint.Visible = true;
                    var btnCR = FindButtonByName(this, "btnCancelReply");
                    if (btnCR != null) btnCR.Visible = true;
                    _txtNewComment.Focus();
                };
                card.Controls.Add(btnReply);
                bx += 76;
            }

            if (isMine && !isDeleted)
            {
                var btnDelete = new Button
                {
                    Text      = "🗑",
                    Location  = new Point(bx, y),
                    Size      = new Size(32, 22),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(255, 230, 230),
                    ForeColor = Color.FromArgb(156, 0, 6),
                    Font      = new Font("Segoe UI", 8F),
                    Cursor    = Cursors.Hand
                };
                btnDelete.FlatAppearance.BorderSize = 0;
                btnDelete.Click += (s, ev) =>
                {
                    if (MessageBox.Show("ต้องการลบ Comment นี้?", "ยืนยัน",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _svc.DeleteComment(commentId);
                        LoadComments();
                    }
                };
                card.Controls.Add(btnDelete);
            }

            card.Controls.Add(lblMeta);
            card.Controls.Add(lblContent);
            return card;
        }

        private void PostComment()
        {
            if (_editModel == null) return;
            string content = _txtNewComment.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;

            // BR-011-05: Thread depth สูงสุด 3 ระดับ (depth 0,1,2)
            int newDepth = _replyToCommentId.HasValue ? _replyToDepth + 1 : 0;
            var model = new ProjectTaskCommentModel
            {
                i_task_id           = _editModel.i_task_id,
                i_parent_comment_id = _replyToCommentId,
                i_thread_depth      = newDepth,
                n_content           = content,
                i_create_user       = _userName
            };

            int newId = _svc.AddComment(model);
            if (newId > 0)
            {
                _txtNewComment.Text    = "";
                _replyToCommentId      = null;
                _replyToDepth          = 0;
                _lblReplyHint.Text     = "";
                _lblReplyHint.Visible  = false;
                var btnCR = FindButtonByName(this, "btnCancelReply");
                if (btnCR != null) btnCR.Visible = false;
                LoadComments();
            }
            else
            {
                MessageBox.Show("ส่ง Comment ไม่สำเร็จ", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Button FindButtonByName(Control root, string name)
        {
            foreach (Control c in root.Controls)
            {
                if (c is Button b && b.Name == name) return b;
                var found = FindButtonByName(c, name);
                if (found != null) return found;
            }
            return null;
        }

        // ─── Save ─────────────────────────────────────────────────────────────

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("กรุณาระบุหัวข้องาน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitle.Focus();
                return;
            }

            // ตรวจวันที่เฉพาะตอนสร้างใหม่ — Edit อาจมี due date ที่ผ่านไปแล้วได้
            if (_editModel == null && dtpDue.Checked && dtpDue.Value.Date < DateTime.Today)
            {
                MessageBox.Show("วันกำหนดเสร็จต้องเป็นวันในอนาคต", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpDue.Focus();
                return;
            }

            var model = new ProjectTaskModel
            {
                i_project_id      = _projectId,
                i_proj_state_id   = _projStateId,
                i_parent_task_id  = _parentTaskId,
                i_subtask_depth   = _parentTaskId.HasValue ? 1 : 0,
                n_title           = txtTitle.Text.Trim(),
                n_description     = txtDesc.Text.Trim(),
                i_priority        = cbbPriority.SelectedIndex + 1,
                i_pct_complete    = (int)nudPct.Value,
                n_assignee        = cbbAssignee.SelectedIndex > 0 ? cbbAssignee.SelectedItem.ToString() : null,
                d_due_date        = dtpDue.Checked   ? (DateTime?)dtpDue.Value.Date   : null,
                d_start_date      = dtpStart.Checked ? (DateTime?)dtpStart.Value.Date : null,
                i_estimated_hours = nudHours.Value > 0 ? (int?)nudHours.Value : null,
                i_create_user     = _userName,
                i_update_user     = _userName
            };

            try
            {
                bool success;
                if (_editModel != null)
                {
                    model.i_task_id = _editModel.i_task_id;
                    success = _svc.UpdateTask(model);
                    if (!success)
                        throw new Exception("ไม่พบ Task ที่ต้องการแก้ไข (อาจถูกลบไปแล้ว)");
                }
                else
                {
                    int newId = _svc.CreateTask(model);
                    if (newId <= 0)
                        throw new Exception("สร้าง Task ไม่สำเร็จ (ไม่ได้รับ Task ID)");
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("บันทึกไม่สำเร็จ:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
