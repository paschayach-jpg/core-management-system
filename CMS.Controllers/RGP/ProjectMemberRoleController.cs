using System.Data;
using CMS.Data.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IProjectMemberRoleRepository</summary>
    public sealed class ProjectMemberRoleController
    {
        private readonly IProjectMemberRoleRepository _repo;

        public ProjectMemberRoleController() : this(RgpComposition.MemberRoleRepository) { }

        public ProjectMemberRoleController(IProjectMemberRoleRepository repo)
        {
            _repo = repo;
        }

        public DataTable GetAll() => _repo.GetAll();

        public DataTable GetActive() => _repo.GetActive();

        public bool Insert(string roleName, string userName) => _repo.Insert(roleName, userName);

        public bool Update(int roleId, string roleName, string fActive, string userName) =>
            _repo.Update(roleId, roleName, fActive, userName);

        public bool Delete(int roleId) => _repo.Delete(roleId);

        public DataTable GetSubCommittees() => _repo.GetSubCommittees();

        public DataTable GetDivisions() => _repo.GetDivisions();

        public DataTable GetDepartments() => _repo.GetDepartments();
    }
}
