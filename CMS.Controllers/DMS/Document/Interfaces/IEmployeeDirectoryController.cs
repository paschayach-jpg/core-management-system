using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IEmployeeDirectoryController
    {
        IList<EmployeeSummary> GetAllEmployees();
        EmployeeApiResponse GetByCode(string employeeCode);
        IList<DepartmentSummary> GetDepartments();
        IList<DivisionSummary> GetDivisions();
    }
}