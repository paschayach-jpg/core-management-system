using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// จัดการไฟล์แนบระดับโครงการ (RGP) — ผูก/ยกเลิกการผูกไฟล์ใน DMS เข้ากับโครงการ
    /// การอัปโหลด/ดาวน์โหลดไฟล์จริงใช้ IFileService ของ DMS โดยตรง
    /// </summary>
    public interface IProjectAttachmentService
    {
        IList<ProjectAttachmentModel> GetAttachments(int projectId);

        /// <summary>ผูกไฟล์ (fileId จาก storage.FileDocument) เข้ากับโครงการ — ข้ามรายการที่ผูกไว้แล้ว</summary>
        void LinkAttachments(int projectId, IEnumerable<int> fileIds, string user);

        void RemoveAttachment(int projectId, int fileId, string user);
    }
}
