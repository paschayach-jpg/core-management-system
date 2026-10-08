using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IDownloadUserPermissionRepository
    {
        /// <summary>ทุกรายการที่ Active พร้อมข้อมูลผู้ใช้</summary>
        IEnumerable<DownloadUserPermission> GetAll();

        /// <summary>รายการ Active ของผู้ใช้ 1 คน (null = ไม่มี)</summary>
        DownloadUserPermission GetByUser(int userId);

        /// <summary>Insert หรือ Update (เปิด IsActive) ตาม UserID</summary>
        void Upsert(DownloadUserPermission permission, int updatedBy);

        /// <summary>ปิดสิทธิ์ของผู้ใช้ (IsActive = 0)</summary>
        void Deactivate(int userId, int updatedBy);
    }
}
