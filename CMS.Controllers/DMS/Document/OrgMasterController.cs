using System;
using System.Collections.Generic;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public class OrgMasterController : IOrgMasterController
    {
        private readonly IOrgMasterService _service;

        public OrgMasterController(IOrgMasterService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IEnumerable<HrDivision> GetAllDivisions() => _service.GetAllDivisions();
        public int SaveDivision(HrDivision division, int userId) => _service.SaveDivision(division, userId);
        public bool DeleteDivision(int divisionId, int userId) => _service.DeleteDivision(divisionId, userId);

        public IEnumerable<HrDepartment> GetAllDepartments() => _service.GetAllDepartments();
        public IEnumerable<HrDepartment> GetDepartmentsByDivision(int divisionId) => _service.GetDepartmentsByDivision(divisionId);
        public int SaveDepartment(HrDepartment department, int userId) => _service.SaveDepartment(department, userId);
        public bool DeleteDepartment(int departmentId, int userId) => _service.DeleteDepartment(departmentId, userId);

        public IEnumerable<HrPosition> GetAllPositions() => _service.GetAllPositions();
        public int SavePosition(HrPosition position, int userId) => _service.SavePosition(position, userId);
        public bool DeletePosition(int positionId, int userId) => _service.DeletePosition(positionId, userId);

        public IEnumerable<HrPositionLevel> GetAllLevels() => _service.GetAllLevels();
        public bool SaveLevel(HrPositionLevel level, int userId) => _service.SaveLevel(level, userId);
        public bool DeleteLevel(string levelCode, int userId) => _service.DeleteLevel(levelCode, userId);

        public IEnumerable<HrPositionMapping> GetAllMappings() => _service.GetAllMappings();
        public int SaveMapping(HrPositionMapping mapping, int userId) => _service.SaveMapping(mapping, userId);
        public bool DeleteMapping(int mappingId, int userId) => _service.DeleteMapping(mappingId, userId);

        public byte[] GenerateExcelTemplate() => _service.GenerateExcelTemplate();
        public IList<string> GetExcelSheetNames(string filePath) => _service.GetExcelSheetNames(filePath);
        public OrgMasterPreview ParseAndValidateExcel(string filePath, string sheetName) => _service.ParseAndValidateExcel(filePath, sheetName);
        public OrgMasterImportResult ExecuteImport(OrgMasterPreview preview, int userId) => _service.ExecuteImport(preview, userId);
    }
}
