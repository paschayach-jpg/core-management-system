using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    /// <summary>
    /// Access to storage.t_project_attachment — the RGP project &lt;-&gt; DMS file link table.
    /// </summary>
    public interface IProjectAttachmentRepository
    {
        /// <summary>รายการไฟล์แนบของโครงการ (join กับ storage.FileDocument / storage.Folder เพื่อ display)</summary>
        IList<ProjectAttachmentModel> GetByProject(int projectId);

        /// <summary>ผูกไฟล์เข้าโครงการ (idempotent — ถ้ามี link เดิมอยู่แล้วจะไม่ insert ซ้ำ) คืน i_attach_id ที่มีผล</summary>
        int Insert(int projectId, int fileId, string user);

        /// <summary>ยกเลิกการผูกไฟล์ (ลบเฉพาะ link — ไม่แตะ storage.FileDocument)</summary>
        bool DeleteByProjectAndFile(int projectId, int fileId);

        /// <summary>ลบ link ทั้งหมดของโครงการ (ใช้ตอนลบโครงการ)</summary>
        int DeleteByProject(int projectId);
    }
}
