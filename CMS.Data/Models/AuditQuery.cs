using System;

namespace CMS.Data.Models
{
    /// <summary>
    /// Filter parameters for querying the immutable audit.AuditLog table.
    /// </summary>
    public class AuditQuery
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? UserID { get; set; }
        public string UserName { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public int? RecordID { get; set; }
        public int? Result { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
