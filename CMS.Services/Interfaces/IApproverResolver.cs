using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// เลือกผู้อนุมัติตามแผนกเจ้าของโฟลเดอร์ โดยใช้สิทธิ์ "อนุมัติ" (CanApprove) จาก FolderAuthorityConfig
    /// </summary>
    public interface IApproverResolver
    {
        /// <summary>
        /// ผู้อนุมัติสำหรับคำขอบนโฟลเดอร์: แผนกเจ้าของโฟลเดอร์ → ฝ่ายแม่ → Admin
        /// ไม่รวมผู้ขอ (ยกเว้นเป็น Admin คนเดียวในระบบ)
        /// </summary>
        IList<UserAccount> ResolveForFolder(int folderId, int requesterId);

        /// <summary>ผู้อนุมัติสำหรับคำขอบนไฟล์ — ใช้โฟลเดอร์ที่ไฟล์อยู่</summary>
        IList<UserAccount> ResolveForFile(int fileId, int requesterId);

        /// <summary>
        /// พนักงาน Active ทุกคนในแผนกเจ้าของโฟลเดอร์ที่ไฟล์อยู่ (ไม่ดู CanApprove, ไม่รวมผู้ขอ)
        /// จับคู่ด้วยรหัสแผนก หรือ ชื่อแผนก; ถ้าหาแผนกของโฟลเดอร์ไม่ได้ ใช้แผนกของผู้ขอ
        /// </summary>
        IList<UserAccount> GetDepartmentMembersForFile(int fileId, UserContext requester);

        /// <summary>Admin ที่ Active (ไม่รวมผู้ขอ ยกเว้นเป็น Admin คนเดียว)</summary>
        IList<UserAccount> GetAdmins(int requesterId);
    }
}
