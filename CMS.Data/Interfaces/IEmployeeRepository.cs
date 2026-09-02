using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IEmployeeRepository
    {
        DataTable GetActiveEmployees();
        DataTable GetActiveADUsers();
    }
}
