using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    /// <summary>Request DTO สำหรับย้าย FileDocument หนึ่งหรือหลายรายการ</summary>
    public class MoveFileRequest
    {
        /// <summary>รายการ FileID ที่ต้องการย้าย (อย่างน้อย 1 รายการ)</summary>
        public IEnumerable<int> FileIds { get; set; }

        /// <summary>FolderID ของ Folder ปลายทาง</summary>
        public int DestinationFolderId { get; set; }
    }
}
