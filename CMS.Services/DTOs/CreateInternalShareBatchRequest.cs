using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    public class CreateInternalShareBatchRequest
    {
        public List<int> FileIDs { get; set; } = new List<int>();
        public int? FolderID { get; set; }
        public List<ShareRecipientItem> Recipients { get; set; } = new List<ShareRecipientItem>();
        public string Remark { get; set; }
    }
}
