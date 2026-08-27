using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IProjectNotifyLogRepository
    {
        /// <summary>หา task_id ที่เคยผูกไว้กับการส่ง Notify ครั้งก่อนของ (ขั้นตอน, สมาชิก) คู่นี้ — null ถ้ายังไม่เคยสร้าง</summary>
        int? GetExistingTaskId(int i_proj_state_id, int i_state_member_id);

        bool Insert(ProjectNotifyLogModel m);
    }
}
