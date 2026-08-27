using CMS.Domain.Models;
using CMS.Data.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IProjectNotifyLogRepository สำหรับฟีเจอร์ส่ง Notify บุคคลที่เกี่ยวข้องกับขั้นตอน</summary>
    public sealed class ProjectNotifyLogController
    {
        private readonly IProjectNotifyLogRepository _repo;

        public ProjectNotifyLogController() : this(RgpComposition.NotifyLogRepository) { }

        public ProjectNotifyLogController(IProjectNotifyLogRepository repo)
        {
            _repo = repo;
        }

        public int? GetExistingTaskId(int i_proj_state_id, int i_state_member_id) =>
            _repo.GetExistingTaskId(i_proj_state_id, i_state_member_id);

        public bool Insert(ProjectNotifyLogModel m) => _repo.Insert(m);
    }
}
