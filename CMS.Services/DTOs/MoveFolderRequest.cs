namespace CMS.Services.DTOs
{
    /// <summary>Request DTO สำหรับย้าย Folder ไปยัง Parent Folder ใหม่</summary>
    public class MoveFolderRequest
    {
        /// <summary>FolderID ของ Folder ที่ต้องการย้าย</summary>
        public int SourceFolderId { get; set; }

        /// <summary>FolderID ของ Parent Folder ปลายทาง</summary>
        public int DestinationParentFolderId { get; set; }
    }
}
