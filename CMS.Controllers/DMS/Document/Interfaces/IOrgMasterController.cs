using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IOrgMasterController
    {
        // Divisions
        IEnumerable<HrDivision> GetAllDivisions();
        int SaveDivision(HrDivision division, int userId);
        bool DeleteDivision(int divisionId, int userId);

        // Departments
        IEnumerable<HrDepartment> GetAllDepartments();
        IEnumerable<HrDepartment> GetDepartmentsByDivision(int divisionId);
        int SaveDepartment(HrDepartment department, int userId);
        bool DeleteDepartment(int departmentId, int userId);

        // Positions
        IEnumerable<HrPosition> GetAllPositions();
        int SavePosition(HrPosition position, int userId);
        bool DeletePosition(int positionId, int userId);

        // Levels
        IEnumerable<HrPositionLevel> GetAllLevels();
        bool SaveLevel(HrPositionLevel level, int userId);
        bool DeleteLevel(string levelCode, int userId);

        // Mappings
        IEnumerable<HrPositionMapping> GetAllMappings();
        int SaveMapping(HrPositionMapping mapping, int userId);
        bool DeleteMapping(int mappingId, int userId);

        // Import & Template
        byte[] GenerateExcelTemplate();
        IList<string> GetExcelSheetNames(string filePath);
        OrgMasterPreview ParseAndValidateExcel(string filePath, string sheetName);
        OrgMasterImportResult ExecuteImport(OrgMasterPreview preview, int userId);
    }
}
