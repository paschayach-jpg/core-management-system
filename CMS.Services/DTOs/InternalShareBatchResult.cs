using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    public class InternalShareBatchResult
    {
        public int ImmediateGrantedCount { get; set; }
        public List<int> ApprovalRequestIDs { get; set; } = new List<int>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
