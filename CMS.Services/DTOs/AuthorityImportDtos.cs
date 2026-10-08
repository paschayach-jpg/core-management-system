using System.Collections.Generic;
using System.Linq;

namespace CMS.Services.DTOs
{
    public enum AuthorityImportSeverity { Warning = 1, Error = 2 }

    public enum AuthorityImportStatus { New = 1, Updated = 2, Unchanged = 3, NotFound = 4 }

    public class AuthorityImportIssue
    {
        /// <summary>1-based Excel row number; 0 = file level.</summary>
        public int RowNumber { get; set; }
        public AuthorityImportSeverity Severity { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }

    /// <summary>One employee row. Permissions index = levelIndex*4 + action; levelIndex 0=HC,1=SC,2=NC,3=PC; action 0=View,1=Add,2=Edit,3=Approve.</summary>
    public class AuthorityImportRow
    {
        public AuthorityImportRow() { Permissions = new bool[16]; }

        public int RowNumber { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string Level { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string Division { get; set; }
        public string Remark { get; set; }
        public bool[] Permissions { get; set; }
    }

    public class AuthorityImportPositionPlan
    {
        public string Label { get; set; }
        public string PositionName { get; set; }
        public string DepartmentCode { get; set; }
        public string DivisionCode { get; set; }
        /// <summary>Job level of the existing config (e.g. 3A), kept on update; null when it has none.</summary>
        public string LevelCode { get; set; }
        public string DepartmentText { get; set; }
        public string DivisionText { get; set; }
        public bool[] Permissions { get; set; }
        public AuthorityImportStatus Status { get; set; }
    }

    public class AuthorityImportLevelPlan
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public int UserId { get; set; }
        public string OldLevel { get; set; }
        public string NewLevel { get; set; }
        /// <summary>Updated | Unchanged | NotFound</summary>
        public AuthorityImportStatus Status { get; set; }
    }

    public class AuthorityImportPreview
    {
        public AuthorityImportPreview()
        {
            Rows = new List<AuthorityImportRow>();
            Issues = new List<AuthorityImportIssue>();
            PositionPlans = new List<AuthorityImportPositionPlan>();
            LevelPlans = new List<AuthorityImportLevelPlan>();
        }

        public string FilePath { get; set; }
        public string SheetName { get; set; }
        public bool UpdateUserLevel { get; set; }
        /// <summary>False = user-level import only; position plans are neither built nor applied.</summary>
        public bool ImportPositions { get; set; }
        public List<AuthorityImportRow> Rows { get; set; }
        public List<AuthorityImportIssue> Issues { get; set; }
        public List<AuthorityImportPositionPlan> PositionPlans { get; set; }
        public List<AuthorityImportLevelPlan> LevelPlans { get; set; }

        public bool HasErrors { get { return Issues.Any(i => i.Severity == AuthorityImportSeverity.Error); } }
        public int NewCount { get { return PositionPlans.Count(p => p.Status == AuthorityImportStatus.New); } }
        public int UpdatedCount { get { return PositionPlans.Count(p => p.Status == AuthorityImportStatus.Updated); } }
        public int UnchangedCount { get { return PositionPlans.Count(p => p.Status == AuthorityImportStatus.Unchanged); } }
        public int LevelChangeCount { get { return LevelPlans.Count(p => p.Status == AuthorityImportStatus.Updated); } }
        public bool HasChanges { get { return NewCount + UpdatedCount + LevelChangeCount > 0; } }
    }

    public class AuthorityImportResult
    {
        public int ConfigsCreated { get; set; }
        public int ConfigsUpdated { get; set; }
        public int LevelsChanged { get; set; }
    }
}
