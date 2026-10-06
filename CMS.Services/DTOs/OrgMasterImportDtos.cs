using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    public enum OrgMasterType
    {
        Division = 1,
        Department = 2,
        Position = 3,
        Level = 4,
        Mapping = 5
    }

    public enum OrgMasterImportStatus
    {
        ValidNew = 1,
        ValidUpdate = 2,
        Warning = 3,
        Error = 4
    }

    public class OrgMasterImportRow
    {
        public int RowIndex { get; set; }
        public OrgMasterType MasterType { get; set; }
        public OrgMasterImportStatus Status { get; set; }
        public string ErrorMessage { get; set; }

        // Division Fields
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }

        // Department Fields
        public string DepartmentCode { get; set; }
        public string DepartmentNameTh { get; set; }
        public string DepartmentNameEn { get; set; }
        public string DepartmentLevel { get; set; }

        // Position Fields
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public string PositionLevel { get; set; }

        // Level Master Fields
        public string LevelCode { get; set; }
        public string LevelName { get; set; }
        public string LevelCategory { get; set; }
        public int? SortOrder { get; set; }

        // Mapping Fields
        public int? HeadCount { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class OrgMasterPreview
    {
        public string FilePath { get; set; }
        public string SheetName { get; set; }
        public List<OrgMasterImportRow> Rows { get; set; } = new List<OrgMasterImportRow>();
        public int TotalCount => Rows.Count;
        public int ValidCount => Rows.FindAll(r => r.Status == OrgMasterImportStatus.ValidNew || r.Status == OrgMasterImportStatus.ValidUpdate).Count;
        public int ErrorCount => Rows.FindAll(r => r.Status == OrgMasterImportStatus.Error).Count;
        public int WarningCount => Rows.FindAll(r => r.Status == OrgMasterImportStatus.Warning).Count;
    }

    public class OrgMasterImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int InsertedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int FailedCount { get; set; }
    }
}
