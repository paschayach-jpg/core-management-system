using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Audit entry เฉพาะโมดูล Project (RGP) — บันทึกลง t_project_audit_log
    /// แยกจาก CMS.Domain.Models.AuditEntry เพราะ shape ต่างกัน (UserID เป็น string/ADUserName
    /// ไม่ใช่ int UserID ของ CMS, ไม่มี OldValue/NewValue/UserRole เหมือนของ CMS)
    /// </summary>
    public class ProjectAuditEntry
    {
        public string UserID { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public int? RecordID { get; set; }
        public int Result { get; set; } // 1 = success, 2 = fail
        public string Message { get; set; }
        public string MachineName { get; set; }
        public DateTime ActionDate { get; set; }
    }
}
