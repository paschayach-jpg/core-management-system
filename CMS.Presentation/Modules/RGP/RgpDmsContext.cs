using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Infrastructure.Interfaces;

namespace CMS.Presentation.Modules.RGP
{
    /// <summary>
    /// ถือ DMS controller ที่จำเป็นต่อการเปิด dialog อัปโหลด/เลือกไฟล์ (FrmExternalUpload, FrmDmsFilePicker)
    /// ให้ Form ลูกของโมดูล Project (RGP) ใช้ — mirror ของ <see cref="RgpSession"/>
    /// ตั้งค่าครั้งเดียวจาก MDI_Container.OpenProjectManagement() ด้วยอินสแตนซ์ชุดเดียวกับที่ MDI ใช้
    /// (composition root จริงอยู่ที่ Program.cs — ที่นี่แค่ ambient holder เพราะ Form ลูกยังไม่ได้รับผ่าน ctor)
    /// </summary>
    internal static class RgpDmsContext
    {
        public static IFileUploadController        FileUploadController        { get; private set; }
        public static IFileBrowseController        FileBrowseController        { get; private set; }
        public static IFolderReadLookupController  FolderReadLookupController  { get; private set; }
        public static IFolderCommandController     FolderCommandController     { get; private set; }
        public static IUploadCheckpointController  UploadCheckpointController  { get; private set; }
        public static IFileCommandController       FileCommandController       { get; private set; }
        public static INetworkDriveHelper          NetworkDriveHelper         { get; private set; }

        /// <summary>true เมื่อ dependency พร้อมพอที่จะเปิด dialog อัปโหลดได้</summary>
        public static bool IsReady =>
            FileUploadController != null && FolderReadLookupController != null && NetworkDriveHelper != null;

        public static void Set(
            IFileUploadController fileUploadController,
            IFileBrowseController fileBrowseController,
            IFolderReadLookupController folderReadLookupController,
            IFolderCommandController folderCommandController,
            IUploadCheckpointController uploadCheckpointController,
            IFileCommandController fileCommandController,
            INetworkDriveHelper networkDriveHelper)
        {
            FileUploadController       = fileUploadController;
            FileBrowseController       = fileBrowseController;
            FolderReadLookupController = folderReadLookupController;
            FolderCommandController    = folderCommandController;
            UploadCheckpointController = uploadCheckpointController;
            FileCommandController      = fileCommandController;
            NetworkDriveHelper        = networkDriveHelper;
        }
    }
}
