using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// 1 row = ตำแหน่ง/แผนก/ฝ่าย × 1 ConfidentialityLevel
    /// Priority การ match: PositionName > DepartmentCode > DivisionCode
    /// </summary>
    public class FolderAuthorityConfig
    {
        public int    ConfigID             { get; set; }
        public string ConfigLabel          { get; set; }

        // ── Match Key ────────────────────────────────────────────────────────
        public string PositionName         { get; set; }   // Priority 1
        public string DepartmentCode       { get; set; }   // Priority 2
        public string DivisionCode         { get; set; }   // Priority 3

        /// <summary>1=HC, 2=SC, 3=NC, 4=PC</summary>
        public int    ConfidentialityLevel { get; set; }

        /// <summary>Folder level code: '1HC' | '2SC' | '3NC' | '4PC'</summary>
        public string LevelCode            { get; set; }

        // ── Display names (transient — populated by AuthorityConfigForm) ──────
        public string DepartmentName       { get; set; }
        public string DivisionName         { get; set; }

        // ── Permissions (normalized — used by UserContext.CanViewFolder etc.) ─
        public bool CanView    { get; set; }
        public bool CanAdd     { get; set; }
        public bool CanEdit    { get; set; }
        public bool CanApprove { get; set; }

        // ── Flat permissions (used by AuthorityConfigForm matrix grid) ─────────
        public bool HC_View    { get; set; }
        public bool HC_Add     { get; set; }
        public bool HC_Edit    { get; set; }
        public bool HC_Approve { get; set; }

        public bool SC_View    { get; set; }
        public bool SC_Add     { get; set; }
        public bool SC_Edit    { get; set; }
        public bool SC_Approve { get; set; }

        public bool NC_View    { get; set; }
        public bool NC_Add     { get; set; }
        public bool NC_Edit    { get; set; }
        public bool NC_Approve { get; set; }

        public bool PC_View    { get; set; }
        public bool PC_Add     { get; set; }
        public bool PC_Edit    { get; set; }
        public bool PC_Approve { get; set; }

        // ── Meta ──────────────────────────────────────────────────────────────
        public bool      IsActive    { get; set; }
        public int?      CreatedBy   { get; set; }
        public DateTime  CreatedDate { get; set; }
        public int?      UpdatedBy   { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
