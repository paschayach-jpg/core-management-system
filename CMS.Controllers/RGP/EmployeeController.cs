using System.Data;
using CMS.Data.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IEmployeeRepository (Common DB) สำหรับโมดูล Project</summary>
    public sealed class EmployeeController
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeController() : this(RgpComposition.EmployeeRepository) { }

        public EmployeeController(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        public DataTable GetActiveEmployees() => _repo.GetActiveEmployees();
        public DataTable GetActiveADUsers() => _repo.GetActiveADUsers();
    }
}
