using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// PDPA personal data access report for a given UserAccount (Req 10.2).
    /// </summary>
    public class PdpaReport
    {
        public UserAccount Subject { get; set; }
        public IEnumerable<FileDocument> FileDocuments { get; set; }
        public IEnumerable<AuditLog> AuditLogs { get; set; }
    }
}
