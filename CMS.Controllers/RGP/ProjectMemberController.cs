using System.Data;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IProjectMemberRepository (ไม่มี business logic เพิ่ม จึงไม่ผ่าน Service layer)</summary>
    public sealed class ProjectMemberController
    {
        private readonly IProjectMemberRepository _repo;

        public ProjectMemberController() : this(RgpComposition.MemberRepository) { }

        public ProjectMemberController(IProjectMemberRepository repo)
        {
            _repo = repo;
        }

        public DataTable GetByProject(int i_project_id) => _repo.GetByProject(i_project_id);

        public bool Insert(ProjectMemberModel m) => _repo.Insert(m);

        public bool Delete(int i_member_id) => _repo.Delete(i_member_id);
    }
}
