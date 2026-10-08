using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.DMS
{
    /// <summary>
    /// Read-only view of the current user's own file-download approval requests: status, approver,
    /// remarks. Downloading an approved file is done from the file itself (FileDownloadGate).
    /// </summary>
    public partial class MyDownloadRequestsForm : Form
    {
        private const int FilterAll = 0, FilterPending = 1, FilterApproved = 2, FilterRejected = 3,
                          FilterDownloaded = 4, FilterClosed = 5;

        private readonly IDownloadPolicyController _policyController;
        private readonly UserContext _user;

        private List<MyDownloadRequestItem> _items = new List<MyDownloadRequestItem>();
        private bool _loading;
        private Font _boldFont;

        public MyDownloadRequestsForm(
            IDownloadPolicyController policyController,
            UserContext user)
        {
            if (policyController == null) throw new ArgumentNullException("policyController");
            if (user             == null) throw new ArgumentNullException("user");

            _policyController = policyController;
            _user             = user;

            InitializeComponent();

            _loading = true;
            cboStatus.SelectedIndex = FilterAll;
            _loading = false;

            Load += (s, e) => LoadRequests();
        }

        // ── Loading ──────────────────────────────────────────────────────────

        private void LoadRequests()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _items = _policyController.GetMyDownloadRequests(_user).ToList();
                BindGrid();

                int pending  = _items.Count(i => i.Status == MyDownloadRequestStatus.Pending);
                int approved = _items.Count(i => i.Status == MyDownloadRequestStatus.Approved && !i.FileMissing);
                SetStatus(string.Format("คำขอทั้งหมด {0} รายการ — รออนุมัติ {1}, อนุมัติแล้วพร้อมดาวน์โหลด {2}",
                    _items.Count, pending, approved), Color.FromArgb(31, 56, 100));
            }
            catch (Exception ex)
            {
                SetStatus("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, Color.Red);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BindGrid()
        {
            var selected = SelectedItem();
            int keepId = selected != null ? selected.RequestID : 0;
            string search = (txtSearch.Text ?? "").Trim();
            int filter = cboStatus.SelectedIndex;

            dgvRequests.Rows.Clear();
            foreach (var item in _items)
            {
                if (!MatchesFilter(item, filter)) continue;
                if (search.Length > 0
                    && (item.FileName ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0
                    && (item.FolderName ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                string resolved = item.ResolvedDate.HasValue
                    ? string.Format("{0}  {1:dd/MM/yy HH:mm}", item.ResolvedByName ?? "", item.ResolvedDate.Value)
                    : "";

                int idx = dgvRequests.Rows.Add(
                    item.RequestID,
                    StatusIcon(item) + " " + item.StatusText,
                    item.FileName,
                    item.FolderName ?? (item.FileMissing ? "(ไฟล์ถูกลบ)" : ""),
                    item.RequestedDate.ToString("dd/MM/yy HH:mm"),
                    item.ApproverName,
                    item.Reason,
                    resolved,
                    TranslateRemark(item.ResolveRemark),
                    item.ExpiryDate.HasValue ? item.ExpiryDate.Value.ToString("dd/MM/yy") : "—");
                dgvRequests.Rows[idx].Tag = item;

                if (item.RequestID == keepId)
                {
                    dgvRequests.ClearSelection();
                    dgvRequests.Rows[idx].Selected = true;
                    dgvRequests.CurrentCell = dgvRequests.Rows[idx].Cells[0];
                }
            }
        }

        private static bool MatchesFilter(MyDownloadRequestItem item, int filter)
        {
            switch (filter)
            {
                case FilterPending:    return item.Status == MyDownloadRequestStatus.Pending;
                case FilterApproved:   return item.Status == MyDownloadRequestStatus.Approved;
                case FilterRejected:   return item.Status == MyDownloadRequestStatus.Rejected;
                case FilterDownloaded: return item.Status == MyDownloadRequestStatus.Downloaded;
                case FilterClosed:     return item.Status == MyDownloadRequestStatus.Withdrawn
                                           || item.Status == MyDownloadRequestStatus.Expired;
                default:               return true;
            }
        }

        // ── Event handlers ───────────────────────────────────────────────────

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (_loading) return;
            BindGrid();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadRequests();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dgvRequests_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colStatus.Index) return;
            var item = dgvRequests.Rows[e.RowIndex].Tag as MyDownloadRequestItem;
            if (item == null) return;

            switch (item.Status)
            {
                case MyDownloadRequestStatus.Pending:
                    e.CellStyle.ForeColor = item.IsOverdue ? Color.DarkRed : Color.FromArgb(180, 120, 0);
                    break;
                case MyDownloadRequestStatus.Approved:
                    e.CellStyle.ForeColor = Color.FromArgb(20, 130, 60);
                    if (_boldFont == null) _boldFont = new Font(dgvRequests.Font, FontStyle.Bold);
                    e.CellStyle.Font = _boldFont;
                    break;
                case MyDownloadRequestStatus.Rejected:
                    e.CellStyle.ForeColor = Color.FromArgb(192, 57, 43);
                    break;
                default:
                    e.CellStyle.ForeColor = Color.Gray;
                    break;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private MyDownloadRequestItem SelectedItem()
        {
            return dgvRequests.CurrentRow != null ? dgvRequests.CurrentRow.Tag as MyDownloadRequestItem : null;
        }

        private static string StatusIcon(MyDownloadRequestItem item)
        {
            switch (item.Status)
            {
                case MyDownloadRequestStatus.Pending:    return item.IsOverdue ? "⏰" : "🟡";
                case MyDownloadRequestStatus.Approved:   return "🟢";
                case MyDownloadRequestStatus.Rejected:   return "🔴";
                case MyDownloadRequestStatus.Downloaded: return "✔";
                default:                                 return "⚪";
            }
        }

        /// <summary>System remarks are stored in English; show them in Thai.</summary>
        private static string TranslateRemark(string remark)
        {
            if (string.IsNullOrWhiteSpace(remark)) return "";
            if (remark.StartsWith("Withdrawn", StringComparison.OrdinalIgnoreCase)) return "ผู้ขอยกเลิกคำขอ";
            return remark;
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }
    }
}
