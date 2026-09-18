using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>ผลการ validate / import ของ KBOrderImportWizard</summary>
    public class KBOrderImportResult
    {
        public int TotalRecords   { get; set; }
        public int ValidRecords   { get; set; }
        public int WarningRecords { get; set; }
        public int ErrorRecords   { get; set; }

        public bool   IsSuccess { get; set; }
        public string Message   { get; set; }

        public IList<KBOrderImportRow> Rows { get; set; }

        /// <summary>แถวที่ถูกคัดออกก่อนเริ่ม import</summary>
        public IList<KBOrderImportRow> SkippedRows { get; set; }

        /// <summary>แถวที่ล้มเหลวระหว่าง import</summary>
        public IList<KBOrderImportRow> ImportFailedRows { get; set; }

        public KBOrderImportResult()
        {
            Rows             = new List<KBOrderImportRow>();
            SkippedRows      = new List<KBOrderImportRow>();
            ImportFailedRows = new List<KBOrderImportRow>();
        }
    }
}
