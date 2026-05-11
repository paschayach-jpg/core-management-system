using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation
{
    /// <summary>
    /// Read-only report form showing FileDocument and AuditLog records for a user (PDPA Req 10.2).
    /// </summary>
    public partial class PdpaReportForm : Form
    {
        public PdpaReportForm(PdpaReport report)
        {
            if (report == null) throw new ArgumentNullException("report");

            InitializeComponent();

            // Header
            lblSubject.Text = string.Format("PDPA Report: {0} ({1})",
                report.Subject.DisplayName, report.Subject.AdIdentity);

            // Files grid columns
            gridFiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID",               DataPropertyName = "FileDocumentId",  Width = 50 });
            gridFiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ชื่อไฟล์",          DataPropertyName = "OriginalFileName", Width = 220 });
            gridFiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Folder ID",        DataPropertyName = "FolderId",         Width = 70 });
            gridFiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ขนาด (bytes)",     DataPropertyName = "FileSizeBytes",    Width = 100 });
            gridFiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "อัปโหลดเมื่อ",     DataPropertyName = "UploadedAt",       Width = 130 });
            gridFiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ลบแล้ว",           DataPropertyName = "IsDeleted",        Width = 60 });
            gridFiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Retention Flagged", DataPropertyName = "RetentionFlagged", Width = 120 });

            var fileList = report.FileDocuments != null
                ? report.FileDocuments.ToList()
                : new List<FileDocument>();
            gridFiles.DataSource = new BindingList<FileDocument>(fileList);

            // Audit grid columns
            gridAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID",           DataPropertyName = "AuditLogId",        Width = 50 });
            gridAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Event Type",   DataPropertyName = "EventType",         Width = 120 });
            gridAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Target Type",  DataPropertyName = "TargetEntityType",  Width = 100 });
            gridAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Target ID",    DataPropertyName = "TargetEntityId",    Width = 70 });
            gridAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Outcome",      DataPropertyName = "Outcome",           Width = 80 });
            gridAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Timestamp",    DataPropertyName = "EventTimestamp",    Width = 130 });
            gridAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "รายละเอียด",   DataPropertyName = "Details",           Width = 200 });

            var auditList = report.AuditLogs != null
                ? report.AuditLogs.ToList()
                : new List<AuditLog>();
            gridAudit.DataSource = new BindingList<AuditLog>(auditList);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
