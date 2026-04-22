using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class EmployeeDirectoryController : IEmployeeDirectoryController
    {
        private readonly IEmployeeApiService _employeeApiService;

        public EmployeeDirectoryController(IEmployeeApiService employeeApiService = null)
        {
            _employeeApiService = employeeApiService;
        }

        public IList<EmployeeSummary> GetAllEmployees()
        {
            if (_employeeApiService == null)
                return new List<EmployeeSummary>();

            return (_employeeApiService.GetAllEmployees() ?? Enumerable.Empty<EmployeeSummary>()).ToList();
        }

        public EmployeeApiResponse GetByCode(string employeeCode)
        {
            if (_employeeApiService == null)
                return null;

            return _employeeApiService.GetByCode(employeeCode);
        }

        public IList<DepartmentSummary> GetDepartments()
        {
            if (_employeeApiService == null)
                return new List<DepartmentSummary>();

            return (_employeeApiService.GetDepartments() ?? Enumerable.Empty<DepartmentSummary>()).ToList();
        }

        public IList<DivisionSummary> GetDivisions()
        {
            if (_employeeApiService == null)
                return new List<DivisionSummary>();

            return (_employeeApiService.GetDivisions() ?? Enumerable.Empty<DivisionSummary>()).ToList();
        }
    }
}