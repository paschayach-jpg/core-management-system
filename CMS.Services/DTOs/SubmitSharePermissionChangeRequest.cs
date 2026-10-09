using System;
using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    public class SubmitSharePermissionChangeRequest
    {
        public int? FileID { get; set; }
        public int? FolderID { get; set; }
        public string Subject { get; set; }
        public string Remark { get; set; }
        public List<SharePermissionChangeItem> Items { get; set; } = new List<SharePermissionChangeItem>();
    }
}
