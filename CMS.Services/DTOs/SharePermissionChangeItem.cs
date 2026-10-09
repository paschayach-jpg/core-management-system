using System;

namespace CMS.Services.DTOs
{
    public class SharePermissionChangeItem
    {
        public int FilePermissionID { get; set; }
        public int PrincipalType { get; set; }
        public int PrincipalID { get; set; }
        public string PrincipalName { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanDownload { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
