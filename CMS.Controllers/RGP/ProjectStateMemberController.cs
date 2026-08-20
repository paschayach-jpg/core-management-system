using System.Data;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IProjectStateMemberRepository</summary>
    public sealed class ProjectStateMemberController
    {
        private readonly IProjectStateMemberRepository _repo;

        public ProjectStateMemberController() : this(RgpComposition.StateMemberRepository) { }

        public ProjectStateMemberController(IProjectStateMemberRepository repo)
        {
            _repo = repo;
        }

        public DataTable GetByState(int i_proj_state_id) => _repo.GetByState(i_proj_state_id);

        public bool Insert(ProjectStateMemberModel m) => _repo.Insert(m);

        public bool Delete(int i_state_member_id) => _repo.Delete(i_state_member_id);
    }
}
