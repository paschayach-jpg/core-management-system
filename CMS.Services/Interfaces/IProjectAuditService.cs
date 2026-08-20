using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IProjectAuditService
    {
        /// <summary>บันทึก audit log ทั้ง success และ fail (ใช้ Result=1/2 ใน entry)</summary>
        void Log(ProjectAuditEntry entry);
    }
}
