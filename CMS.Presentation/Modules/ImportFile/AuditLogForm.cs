using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Presentation.Presenters;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Admin form for viewing and searching the immutable Audit Log.
    /// Implements IAuditLogView; all business logic is in AuditLogPresenter.
    /// </summary>
    public partial class AuditLogForm : Form, IAuditLogView
    {
        private AuditLogPresenter _presenter;

        // ── IAuditLogView — filter properties ────────────────────────────────

        public DateTime FromDate
        {
            get { return dtpFrom.Value; }
            set { dtpFrom.Value = value; }
        }

        public DateTime ToDate
        {
            get { return dtpTo.Value; }
            set { dtpTo.Value = value; }
        }

        public string FilterModule
        {
            get { return cmbModule.SelectedItem != null && cmbModule.SelectedItem.ToString() != "(ทั้งหมด)"
                    ? cmbModule.SelectedItem.ToString() : null; }
            set { cmbModule.SelectedItem = value; }
        }

        public string FilterAction
        {
            get { return cmbAction.SelectedItem != null && cmbAction.SelectedItem.ToString() != "(ทั้งหมด)"
                    ? cmbAction.SelectedItem.ToString() : null; }
            set { cmbAction.SelectedItem = value; }
        }

        public string FilterUserName
        {
            get { return txtUserName.Text.Trim(); }
            set { txtUserName.Text = value; }
        }

        public string FilterResult
        {
            get { return cmbResult.SelectedItem != null && cmbResult.SelectedItem.ToString() != "(ทั้งหมด)"
                    ? cmbResult.SelectedItem.ToString() : null; }
            set { cmbResult.SelectedItem = value; }
        }

        // ── IAuditLogView — pagination ────────────────────────────────────────

        public int CurrentPage { get; set; } = 1;
        public int PageSize    { get { return 50; } }

        // ── IAuditLogView — events ────────────────────────────────────────────

        public event EventHandler SearchRequested;
        public event EventHandler ExportRequested;
        public event EventHandler PageChanged;

        // ── Constructor ───────────────────────────────────────────────────────

        public AuditLogForm(IAuditService auditService, UserContext user)
        {
            if (auditService == null) throw new ArgumentNullException("auditService");
            if (user         == null) throw new ArgumentNullException("user");

            InitializeComponent();
            PopulateFilterDropdowns();

            _presenter = new AuditLogPresenter(this, auditService, user);
        }

        // ── IAuditLogView — methods ───────────────────────────────────────────

        public void BindResults(IEnumerable<AuditLog> items, int totalCount, int pageNumber, int pageSize)
        {
            lvAudit.Items.Clear();

            foreach (var log in items)
            {
                var item = new ListViewItem(log.AuditID.ToString());
                item.SubItems.Add(log.AuditDate.ToString("dd/MM/yy HH:mm:ss"));
                item.SubItems.Add(log.UserName ?? "");
                item.SubItems.Add(log.Module   ?? "");
                item.SubItems.Add(log.Action   ?? "");
                item.SubItems.Add(log.TableName ?? "");
                item.SubItems.Add(log.RecordID.HasValue ? log.RecordID.Value.ToString() : "");
                item.SubItems.Add(log.Outcome);
                item.SubItems.Add(log.IPAddress ?? "");
                item.Tag = log;

                // Color-code by result
                switch (log.Result)
                {
                    case 1: // Success
                        item.ForeColor = Color.FromArgb(0, 120, 60);
                        break;
                    case 2: // Failure
                        item.BackColor = Color.FromArgb(255, 245, 245);
                        item.ForeColor = Color.DarkRed;
                        break;
                    case 3: // Denied
                        item.BackColor = Color.FromArgb(255, 250, 230);
                        item.ForeColor = Color.FromArgb(160, 100, 0);
                        break;
                }

                lvAudit.Items.Add(item);
            }

            // Update pagination controls
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;
            totalPages = Math.Max(1, totalPages);

            lblPageInfo.Text = string.Format("หน้า {0} / {1}", pageNumber, totalPages);
            btnPrev.Enabled  = pageNumber > 1;
            btnNext.Enabled  = pageNumber < totalPages;
        }

        public void ShowStatus(string message, bool isError)
        {
            lblStatus.Text      = message;
            lblStatus.ForeColor = isError ? Color.DarkRed : Color.FromArgb(31, 56, 100);
        }

        public void ShowDetail(AuditLog entry)
        {
            if (entry == null) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("── รายละเอียด Audit Log ──────────────────────────");
            sb.AppendLine(string.Format("AuditID    : {0}", entry.AuditID));
            sb.AppendLine(string.Format("วันที่เวลา : {0}", entry.AuditDate.ToString("dd/MM/yyyy HH:mm:ss")));
            sb.AppendLine(string.Format("ผู้ใช้     : {0} (ID: {1})", entry.UserName, entry.UserID));
            sb.AppendLine(string.Format("Module     : {0}", entry.Module));
            sb.AppendLine(string.Format("Action     : {0}", entry.Action));
            sb.AppendLine(string.Format("ตาราง      : {0}", entry.TableName));
            sb.AppendLine(string.Format("RecordID   : {0}", entry.RecordID.HasValue ? entry.RecordID.Value.ToString() : "-"));
            sb.AppendLine(string.Format("ผลลัพธ์    : {0}", entry.Outcome));
            sb.AppendLine(string.Format("IP Address : {0}", entry.IPAddress ?? "-"));
            sb.AppendLine(string.Format("เครื่อง    : {0}", entry.MachineName ?? "-"));
            sb.AppendLine(string.Format("Session    : {0}", entry.SessionID ?? "-"));

            if (!string.IsNullOrEmpty(entry.OldValue))
            {
                sb.AppendLine();
                sb.AppendLine("── ค่าเดิม ──────────────────────────────────────");
                sb.AppendLine(entry.OldValue);
            }

            if (!string.IsNullOrEmpty(entry.NewValue))
            {
                sb.AppendLine();
                sb.AppendLine("── ค่าใหม่ ──────────────────────────────────────");
                sb.AppendLine(entry.NewValue);
            }

            if (!string.IsNullOrEmpty(entry.ErrorMessage))
            {
                sb.AppendLine();
                sb.AppendLine("── ข้อความผิดพลาด ───────────────────────────────");
                sb.AppendLine(entry.ErrorMessage);
            }

            txtDetail.Text = sb.ToString();
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void btnSearch_Click(object sender, EventArgs e)
        {
            SearchRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                PageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            PageChanged?.Invoke(this, EventArgs.Empty);
        }

        private void lvAudit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvAudit.SelectedItems.Count == 0) return;
            var log = lvAudit.SelectedItems[0].Tag as AuditLog;
            if (log != null)
                ShowDetail(log);
        }

        private void btnClearFilter_Click(object sender, EventArgs e)
        {
            dtpFrom.Value          = DateTime.Today.AddDays(-7);
            dtpTo.Value            = DateTime.Today;
            cmbModule.SelectedIndex = 0;
            cmbAction.SelectedIndex = 0;
            cmbResult.SelectedIndex = 0;
            txtUserName.Text        = "";
            CurrentPage             = 1;
            SearchRequested?.Invoke(this, EventArgs.Empty);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void PopulateFilterDropdowns()
        {
            cmbModule.Items.Clear();
            cmbModule.Items.AddRange(new object[]
            {
                "(ทั้งหมด)", "Folder", "File", "Permission", "Approval",
                "UserAccount", "OrgUnit", "Template", "Config", "Auth"
            });
            cmbModule.SelectedIndex = 0;

            cmbAction.Items.Clear();
            cmbAction.Items.AddRange(new object[]
            {
                "(ทั้งหมด)", "Login", "Logout", "Create", "Update", "Delete",
                "Upload", "Download", "Move", "Approve", "Reject", "Grant", "Revoke"
            });
            cmbAction.SelectedIndex = 0;

            cmbResult.Items.Clear();
            cmbResult.Items.AddRange(new object[]
            {
                "(ทั้งหมด)", "สำเร็จ", "ล้มเหลว", "ถูกปฏิเสธ"
            });
            cmbResult.SelectedIndex = 0;
        }
    }
}
