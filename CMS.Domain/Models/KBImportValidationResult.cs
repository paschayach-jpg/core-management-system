using System;
using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Holds the row-level results of an Excel file validation pass or a completed import
    /// for the KB Import Wizard (MOD-006).
    /// Passed from KBImportWizardPresenter → IKBImportWizardView.
    /// </summary>
    public class KBImportValidationResult
    {
        public int    TotalRecords   { get; set; }
        public int    ValidRecords   { get; set; }
        public int    WarningRecords { get; set; }
        public int    ErrorRecords   { get; set; }

        /// <summary>True when used as an import-complete summary and the import succeeded.</summary>
        public bool   IsSuccess      { get; set; }

        /// <summary>Human-readable result message set after import completes.</summary>
        public string Message        { get; set; }

        public IList<KBImportRow> Rows { get; set; }

        /// <summary>
        /// Rows excluded before the import attempt — pre-validation errors and
        /// warn-skipped rows (when OptSkipWarning is true).
        /// </summary>
        public IList<KBImportRow> SkippedRows { get; set; }

        /// <summary>
        /// Rows that were attempted during import but failed (category resolution or DB insert).
        /// Does NOT include rows skipped before the import started.
        /// </summary>
        public IList<KBImportRow> ImportFailedRows { get; set; }

        public KBImportValidationResult()
        {
            Rows             = new List<KBImportRow>();
            SkippedRows      = new List<KBImportRow>();
            ImportFailedRows = new List<KBImportRow>();
        }
    }

    /// <summary>
    /// Represents one row of the imported Excel file together with its validation status.
    /// </summary>
    public class KBImportRow
    {
        public int    RowNumber   { get; set; }
        public string Title       { get; set; }
        public string ArticleType { get; set; }
        public string Category    { get; set; }
        public string CrafLevel   { get; set; }
        public string Tags        { get; set; }
        public string ExpiryDate  { get; set; }
        /// <summary>"Valid", "Warn", or "Error"</summary>
        public string Status      { get; set; }
        /// <summary>Optional human-readable note explaining a Warn or Error status.</summary>
        public string StatusNote  { get; set; }

        // ── Raw parsed values from Excel ───────────────────────────────
        public string ColA  { get; set; }   // ParentCategory code segment
        public string ColB  { get; set; }   // SubCategory code segment
        public string ColC  { get; set; }   // CategoryName
        public string ColD  { get; set; }   // code segment 3
        public string ColE  { get; set; }   // title / summary segment 1
        public string ColF  { get; set; }   // code segment 4
        public string ColG  { get; set; }   // summary segment 2
        public string ColH  { get; set; }   // code segment 5
        public string ColI  { get; set; }   // summary segment 3
        public string ColJ  { get; set; }   // code segment 6
        public string ColK  { get; set; }   // summary segment 4
        public string ColL  { get; set; }   // code segment 7
        public string ColM  { get; set; }   // summary segment 5
        public string ColN  { get; set; }   // code segment 8
        public string ColO  { get; set; }   // summary segment 6
        public string ColP  { get; set; }   // code segment 9
        public string ColQ  { get; set; }   // summary segment 7
        public string ColR  { get; set; }   // PublishedDate raw text
        public string ColS  { get; set; }   // ExpiryDate raw text
        public string ColT  { get; set; }   // ContentHTML
        public string ColU  { get; set; }   // คณะอนุกรรมการ
        public string ColV  { get; set; }   // ฝ่ายที่รับผิดชอบ
        public string ColAA  { get; set; }   // ฝ่ายที่รับผิดชอบ

        // ── Computed / resolved fields (set by processor) ───────────────────
        public string    ArticleCode         { get; set; }
        public string    Summary             { get; set; }
        public DateTime? PublishedDateParsed { get; set; }
        public DateTime? ExpiryDateParsed    { get; set; }
        public int       ResolvedCategoryID  { get; set; }   // sub-category CategoryID
    }
}
