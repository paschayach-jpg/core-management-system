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
        public int    SkippedRecords { get; set; }
        public int    ErrorRecords   { get; set; }

        /// <summary>True when used as an import-complete summary and the import succeeded.</summary>
        public bool   IsSuccess      { get; set; }

        /// <summary>Human-readable result message set after import completes.</summary>
        public string Message        { get; set; }

        public IList<KBImportRow> Rows { get; set; }

        /// <summary>
        /// Rows excluded before the import attempt — pre-validation errors and rows skipped
        /// (blank ArticleCode / already exists / duplicate within the file).
        /// </summary>
        public IList<KBImportRow> SkippedRows { get; set; }

        /// <summary>
        /// Rows that were attempted during import but failed (DB insert failure).
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
    /// The KB Import Wizard accepts two Excel layouts, auto-detected by
    /// <see cref="CMS.Services.KBExcelReader"/> from the header row (see <see cref="RowKind"/>):
    ///
    /// <b>"Law"</b> — 20-column "law-matrix" layout (1 row = 1 มาตรา). Column A ("Code รวม") is
    /// 22 or 27 digits: the first 22 are the law-section <c>ArticleCode</c> and the last 5 are a
    /// child-document sequence ("00000" = the law section itself, processed here; anything else
    /// is a คำสั่ง/ประกาศ child row, handled as a Order row from a different file instead).
    ///
    /// <b>"NonLaw"</b> — 12-column layout ของเอกสารที่ไม่ใช่กฎหมาย (OR/AN/SO/RG/CI/MR) (1 row = 1 order/announcement, possibly
    /// referencing several parent law sections via <see cref="ParentCodes"/>). ArticleCode is
    /// computed as the first parent code + a 1-digit type ("1"=ประกาศ, "2"=คำสั่ง) + a 4-digit
    /// document sequence.
    ///
    /// Raw fields below are grouped by which layout populates them; fields under "shared" are
    /// populated by both layouts, from different source columns (see <see cref="CMS.Services.KBExcelReader"/>).
    /// </summary>
    public class KBImportRow
    {
        public int    RowNumber   { get; set; }

        /// <summary>"Valid", "Skip", or "Error"</summary>
        public string Status      { get; set; }
        /// <summary>Human-readable note explaining a Skip or Error status.</summary>
        public string StatusNote  { get; set; }

        /// <summary>"Law" or "NonLaw" — which Excel layout this row came from (detected from the header row).</summary>
        public string RowKind     { get; set; }

        // ── Raw values — Law layout only ─────────────────────────────────────
        public string ColA  { get; set; }   // Code รวม 27 หลัก (Template mapping ArticleCode) → ArticleCode source (Order ใช้ field นี้ด้วย)

        // ── Raw values — NonLaw layout only ───────────────────────────────────
        public string AttachmentFileName  { get; set; }   // ชื่อไฟล์ (ข้อความในเซลล์)
        public string AttachmentUrl       { get; set; }   // URL จริง (hyperlink ของเซลล์)
        public IList<string> ParentCodes      { get; set; }   // Code กฎหมายแม่ (1 รายการต่อบรรทัดในเซลล์)
        public IList<int>    ResolvedParentArticleIds { get; set; }   // ArticleID ของแต่ละ ParentCodes ที่พบในระบบ (ตั้งค่าโดย ValidateRows)

        // ── Raw values — shared meaning, different source column per RowKind ─
        public string RawTitle         { get; set; }   // Order: ชื่อ. (Law derives Title from the section digits of ColA.)
        public string RawTitleCategory { get; set; }   // Law: กฎหมายอ้างอิง. Order: คำสั่งที่.
        public string RawSummary       { get; set; }   // Law: ชื่อ. Order: ประเด็นสำคัญ.
        public string RawContent       { get; set; }   // Law: บัญญัติไว้ว่า คำสั่งที่เกี่ยวข้อง. Order: บัญญัติไว้ว่า.
        public string RawPublishedDate { get; set; }   // วันที่ประกาศ (raw text)
        public string RawEffectiveDate { get; set; }   // วันที่มีผลบังคับใช้ (raw text)

        public DateTime? PublishedDateParsed { get; set; }
        public DateTime? EffectiveDateParsed { get; set; }

        // ── Computed / resolved fields (set by processor) ───────────────────
        public string ArticleCode        { get; set; }   // Law: first 22 chars of ColA. Order: Code รวมที่ Template map ไว้ (ColA).
        public string Title              { get; set; }
        public string Tags               { get; set; }
        public string Summary            { get; set; }
        public string TitleCategory      { get; set; }
        /// <summary>CategoryID ที่หาได้จาก ArticleCode ของแถวนี้ตอน validate (Template ไม่ผูกหมวดแล้ว) — 0 = ยังหาไม่ได้</summary>
        public int    ResolvedCategoryID { get; set; }
        /// <summary>Display label for the resolved CategoryID: "มาตรา" (Law), "ประกาศ" or "คำสั่ง" (NonLaw).</summary>
        public string DocumentTypeLabel  { get; set; }
        /// <summary>
        /// CategoryCode ของหมวดที่หาได้จาก ArticleCode ของแถวนี้ (ตั้งค่าโดย KBImportProcessor ผ่าน KBImportCategoryResolver):
        /// Law = รหัสกฎหมาย 3 ตัวแรกของ Code รวม (เช่น PVI, RVA), NonLaw = ประเภทเอกสารจากตัวที่ 23 (AN/OR/SO/RG/CI/MR)
        /// </summary>
        public string DocumentTypeCode   { get; set; }

        // ── Import-time results (Order only) ─────────────────────────────────
        public int  ImportedArticleId { get; set; }
        public int? UploadedFileId    { get; set; }

        public KBImportRow()
        {
            Status                    = "Valid";
            ParentCodes               = new List<string>();
            ResolvedParentArticleIds  = new List<int>();
        }
    }
}
