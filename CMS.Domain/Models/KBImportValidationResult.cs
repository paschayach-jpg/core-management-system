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

        public KBImportValidationResult()
        {
            Rows = new List<KBImportRow>();
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
    }
}
