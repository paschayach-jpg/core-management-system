using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Models;
using CMS.Services;
using CMS.Services.Interfaces;

namespace CMS.Controllers.KMS.KB
{
    /// <summary>
    /// Presenter for the KB Import Wizard (MOD-006).
    /// Handles Excel-file validation and article bulk-import logic.
    /// All business rules (field mapping, status defaults, row skipping) live here —
    /// never in the form.
    /// </summary>
    public class KBImportWizardController
    {
        private readonly IKBImportWizardController _view;
        private readonly IKBArticleService         _articleService;
        private readonly IKBCategoryService        _categoryService;
        private readonly IKBExcelReaderService            _excelReader;
        private readonly IKBImportProcessorService        _processor;
        private readonly IKBImportTemplateService         _templateService;
        private readonly UserContext               _user;

        /// <summary>Retained between ValidateFile() and ExecuteImport().</summary>
        private KBImportValidationResult _lastValidation;

        /// <summary>ArticleType resolved from the selected Template — retained between ValidateFile() and ExecuteImport().</summary>
        private byte _lastResolvedArticleType;

        /// <summary>
        /// NonLaw rows: when an attachment cannot be downloaded/uploaded, fail the whole row
        /// (nothing is inserted) instead of inserting the article without its file.
        /// Fixed — the wizard no longer offers this as a user option.
        /// </summary>
        private const bool SkipRowOnDownloadFailure = true;

        // ── Constructor ──────────────────────────────────────────────────────────

        public KBImportWizardController(
            IKBImportWizardController view,
            IKBArticleService         articleService,
            IKBCategoryService        categoryService,
            IKBExcelReaderService            excelReader,
            IKBImportProcessorService        processor,
            IKBImportTemplateService         templateService,
            UserContext               user)
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (excelReader     == null) throw new ArgumentNullException("excelReader");
            if (processor       == null) throw new ArgumentNullException("processor");
            if (templateService == null) throw new ArgumentNullException("templateService");
            if (user            == null) throw new ArgumentNullException("user");

            _view            = view;
            _articleService  = articleService;
            _categoryService = categoryService;
            _excelReader     = excelReader;
            _processor       = processor;
            _templateService = templateService;
            _user            = user;
        }

        // ── Public API (called by the form) ──────────────────────────────────────

        /// <summary>
        /// Loads active KB Import Templates and binds them to the view's template combo box.
        /// Called once from the form's Load handler.
        /// </summary>
        public void LoadTemplates()
        {
            try
            {
                var templates = _templateService.GetAllTemplates().Where(t => t.IsActive).ToList();
                _view.BindTemplateOptions(templates);
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดรายการ Template ล้มเหลว: " + ex.Message);
            }
        }

        /// <summary>
        /// Refreshes the column-reference panel to preview the currently selected Template's
        /// actual column mapping. Called whenever the template combo box selection changes.
        /// </summary>
        public void OnTemplateSelected()
        {
            try
            {
                int? templateId = _view.SelectedTemplateId;
                if (!templateId.HasValue)
                {
                    _view.BindTemplateColumnReference(new List<KBImportTemplateColumnRow>(), string.Empty);
                    return;
                }

                var template = _templateService.GetTemplateById(templateId.Value);
                var columns  = _templateService.GetTemplateColumnGrid(templateId.Value);
                _view.BindTemplateColumnReference(columns, template != null ? template.TemplateName : string.Empty);
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดตัวอย่างคอลัมน์ของ Template ล้มเหลว: " + ex.Message);
            }
        }

        /// <summary>
        /// Validates the Excel file at <see cref="IKBImportWizardController.SelectedFilePath"/>.
        /// Reads rows via <see cref="KBExcelReader"/>, validates them via
        /// <see cref="KBImportProcessor"/>, then calls
        /// <see cref="IKBImportWizardController.ShowValidationResult"/>; on failure
        /// calls <see cref="IKBImportWizardController.ShowError"/>.
        /// </summary>
        public void ValidateFile()
        {
            try
            {
                string filePath = _view.SelectedFilePath;
                if (string.IsNullOrEmpty(filePath))
                {
                    _view.ShowError("กรุณาเลือกไฟล์ Excel ก่อน");
                    return;
                }

                int? templateId = _view.SelectedTemplateId;
                if (!templateId.HasValue)
                {
                    _view.ShowError("กรุณาเลือก Template ก่อน");
                    return;
                }

                var template = _templateService.GetTemplateById(templateId.Value);
                if (template == null)
                {
                    _view.ShowError("ไม่พบ Template ที่เลือก กรุณาเลือกใหม่อีกครั้ง");
                    return;
                }

                IList<KBImportTemplateColumnRow> templateColumns;
                try
                {
                    templateColumns = _templateService.GetTemplateColumnGrid(templateId.Value);
                }
                catch (Exception ex)
                {
                    _view.ShowError("โหลดข้อมูล Template ล้มเหลว: " + ex.Message);
                    return;
                }

                // ชั้น 0 — Template ต้องตั้งค่าครบ (ประเภทไฟล์, ArticleType, ArticleCode, และ Title/ContentHTML/Attachment ถ้าไม่ใช่กฎหมาย)
                // Template เก่าที่บันทึกไว้ก่อนกฎชุดนี้อาจไม่ครบ — บอกว่าขาดอะไรแล้วให้กลับไปแก้ที่หน้า config
                var templateProblems = _templateService.GetTemplateProblems(templateColumns);
                if (templateProblems.Count > 0)
                {
                    _view.ShowError(string.Format(
                        "Template \"{0}\" ตั้งค่ายังไม่ครบ กรุณาแก้ที่หน้า config template ก่อน:\n• {1}",
                        template.TemplateName, string.Join("\n• ", templateProblems)));
                    return;
                }

                string templateLayoutType;
                byte   resolvedArticleType;
                string resolveError;
                if (!TryResolveTemplateFixedValues(templateColumns, out templateLayoutType, out resolvedArticleType, out resolveError))
                {
                    _view.ShowError(resolveError);
                    return;
                }

                KBExcelReadResult readResult = _excelReader.ReadRows(filePath, templateColumns);
                IList<KBImportRow> rows      = readResult.Rows;

                // ประเภทไฟล์ (Law/NonLaw) ที่ตรวจพบจากไฟล์ (ArticleCode) ต้องตรงกับประเภทไฟล์ที่ Template ตั้งไว้ — ไม่งั้น mapping
                // ของอีกประเภทหนึ่งจะถูกใช้อ่านไฟล์ผิดคอลัมน์ (นำเข้าเนื้อหาผิดโดยไม่มี error) ตรวจก่อนตรวจ mapping เพราะถ้าเลือก Template
                // ผิดประเภท คอลัมน์ที่ map ไว้ย่อมไม่ตรงกับไฟล์อยู่แล้ว รายการคอลัมน์ที่ขาดจะบดบังสาเหตุจริง
                // ข้อความแจ้งเป็นประเภทของ "ไฟล์" ที่ upload เพื่อให้ผู้ใช้เลือก Template ให้ตรงกับไฟล์
                if (rows.Count > 0 && !string.Equals(readResult.DetectedLayout, templateLayoutType, StringComparison.OrdinalIgnoreCase))
                {
                    string fileKind = string.Equals(readResult.DetectedLayout, KBImportLayoutTypeCatalog.Law, StringComparison.OrdinalIgnoreCase)
                        ? "กฎหมาย"
                        : "ที่ไม่ใช่กฎหมาย";

                    _view.ShowError(string.Format("ไฟล์ที่ upload เป็นรูปแบบ {0} กรุณาเลือก Template ให้ถูกต้อง", fileKind));
                    return;
                }

                // mapping ของ Template ต้องชี้ไปคอลัมน์/หัวตารางที่มีอยู่จริงในไฟล์ (เดิมหาไม่เจอแล้วได้ค่าว่างเงียบ ๆ
                // ทำให้เนื้อหาหาย/ผิดคอลัมน์โดยไม่มี error) — หยุดที่ Step 1 พร้อมรายการที่หาไม่เจอ
                if (readResult.MappingProblems.Count > 0)
                {
                    const int maxListed = 15;
                    var listed = readResult.MappingProblems.Take(maxListed).ToList();
                    string more = readResult.MappingProblems.Count > maxListed
                        ? string.Format("\n… และอีก {0} รายการ", readResult.MappingProblems.Count - maxListed)
                        : string.Empty;

                    _view.ShowError(string.Format(
                        "ไฟล์ที่อัปโหลดไม่ตรงกับ Template \"{0}\" — Template map คอลัมน์ที่ไม่มีในไฟล์:\n• {1}{2}\n\nกรุณาเลือก Template ให้ตรงกับไฟล์ หรือแก้ Template ที่หน้า config",
                        template.TemplateName, string.Join("\n• ", listed), more));
                    return;
                }

                // ชั้น 3-4 — หา CategoryID ของแต่ละแถวจาก ArticleCode (ไม่ผูกกับ Template) แล้วตรวจกฎรายแถว
                _processor.ValidateRows(rows);

                _lastValidation          = BuildValidationResult(rows);
                _lastResolvedArticleType = resolvedArticleType;

                _view.ShowValidationResult(_lastValidation);
                _view.SetStatus("ตรวจสอบข้อมูลเรียบร้อย");
            }
            catch (KBExcelReadException ex)
            {
                _view.ShowError("ไม่สามารถเปิดไฟล์ Excel ได้:\n" + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("เกิดข้อผิดพลาดระหว่างการตรวจสอบไฟล์:\n" + ex.Message);
            }
        }

        /// <summary>
        /// Imports validated rows by inserting <see cref="KBArticle"/> + <see cref="KBVersion"/>
        /// records via <see cref="KBImportProcessor"/>. Rows marked <c>Skip</c> (blank/duplicate ArticleCode, or an existing
        /// article with nothing missing) or <c>Error</c> are excluded before the import attempt. Existing NonLaw articles with a
        /// missing attachment/link are <c>Valid</c> and only get the missing parts filled in (no second insert).
        /// On completion calls <see cref="IKBImportWizardController.ShowImportComplete"/>.
        /// </summary>
        public void ExecuteImport()
        {
            try
            {
                if (_lastValidation == null)
                {
                    _view.ShowError("กรุณาตรวจสอบไฟล์ก่อนทำการนำเข้า");
                    return;
                }

                // Consume this validation snapshot now — a validated batch may only be imported once. Importing the same
                // snapshot twice would re-process stale Status="Valid" rows (articles inserted the first time would hit the real
                // DB unique-key constraint). Forcing a fresh ValidateFile() before re-import re-checks every ArticleCode:
                // codes already in the DB become "Skip" (nothing missing) or "Valid" rows that only fill in the missing
                // attachment/links — never a second insert.
                var lastValidation = _lastValidation;
                _lastValidation = null;

                // ── Phase 1: filter rows — only "Valid" rows are attempted ────
                _view.SetProgress(10, "กำลังกรองแถวข้อมูล...");
                var rowsToImport = new List<KBImportRow>();
                var skippedRows  = new List<KBImportRow>();   // pre-import excluded rows

                foreach (var row in lastValidation.Rows)
                {
                    if (row.Status == "Valid") rowsToImport.Add(row);
                    else                       skippedRows.Add(row);
                }
                _view.SetProgress(20, string.Format("กรองแล้ว: {0} รายการพร้อมนำเข้า", rowsToImport.Count));

                // ── Phase 2: article + version insert (per-row progress) ─────
                int successCount;
                int errorCount;

                _processor.InsertArticles(
                    rowsToImport,
                    _user,
                    SkipRowOnDownloadFailure,
                    _lastResolvedArticleType,
                    out successCount,
                    out errorCount,
                    onProgress: (done, total) =>
                    {
                        // Map 0→total onto 20%→99% so 100% marks true completion.
                        int pct = total == 0 ? 99
                            : 20 + (done * 79 / total);
                        _view.SetProgress(pct,
                            string.Format("กำลังนำเข้าบทความ... {0}/{1}", done, total));
                    });

                _view.SetProgress(100, "นำเข้าเสร็จสิ้น");

                // Code ที่ Phase 2 ตัดสินว่า "ข้าม" (เช่น บทความมีอยู่แล้วแต่ไม่พบไฟล์ต้นทาง / ไม่มีอะไรให้เติม) — InsertArticles ไม่ throw
                // จึงนับเป็น success; ย้ายไปรายการที่ถูกข้ามเพื่อแสดงเหตุผล และไม่นับเป็นนำเข้าสำเร็จ
                var skippedDuringImport = rowsToImport.Where(r => r.Status == "Skip").ToList();
                successCount -= skippedDuringImport.Count;
                skippedRows.AddRange(skippedDuringImport);

                // บทความที่มีอยู่แล้วและเติมของที่ขาดสำเร็จ (แยกจากบทความที่ insert ใหม่)
                int filledCount = rowsToImport.Count(r => r.ExistingArticleId > 0 && (r.Status == "Valid" || r.Status == "Warn"));

                // Collect rows that failed DURING import (ExistsByCode race or DB insert failure).
                var importFailedRows = rowsToImport.Where(r => r.Status == "Error").ToList();

                int totalErrors  = skippedRows.Count(r => r.Status == "Error") + errorCount;
                int problemCount = totalErrors + skippedDuringImport.Count;
                string successText = filledCount > 0
                    ? string.Format("นำเข้าสำเร็จ {0} รายการ (บทความใหม่ {1}, เติมของที่ขาด {2})", successCount, successCount - filledCount, filledCount)
                    : string.Format("นำเข้าสำเร็จ {0} รายการ", successCount);
                var summary = new KBImportValidationResult
                {
                    TotalRecords     = lastValidation.TotalRecords,
                    ValidRecords     = successCount,
                    WarningRecords   = 0,
                    SkippedRecords   = lastValidation.SkippedRecords + skippedDuringImport.Count,
                    ErrorRecords     = totalErrors,
                    IsSuccess        = true,
                    Message          = problemCount == 0
                        ? successText
                        : string.Format("{0}, ถูกข้าม/ล้มเหลว {1} รายการ", successText, problemCount),
                    Rows             = lastValidation.Rows,
                    SkippedRows      = skippedRows,
                    ImportFailedRows = importFailedRows,
                };

                _view.ShowImportComplete(summary);
                _view.SetStatus("✅  นำเข้าข้อมูลสำเร็จ");
            }
            catch (Exception ex)
            {
                _view.ShowError("เกิดข้อผิดพลาดระหว่างการนำเข้า:\n" + ex.Message);
            }
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Resolves the fixed values configured on the given Template's mapping (dropdowns in KBImportTemplateConfigForm):
        /// the file layout ("Law"/"NonLaw" — the virtual "LayoutType" row) and the "ArticleType" row. CategoryID is not part
        /// of a Template anymore — it is resolved per row from the ArticleCode (see <see cref="KBImportProcessor.ValidateRows"/>).
        /// Returns <c>false</c> with a user-facing Thai error message when either is missing or not configured.
        /// </summary>
        private bool TryResolveTemplateFixedValues(IList<KBImportTemplateColumnRow> columns, out string layoutType, out byte articleType, out string error)
        {
            layoutType  = null;
            articleType = 0;
            error       = null;

            var layoutRow = columns.FirstOrDefault(c => string.Equals(c.TargetColumnName, KBArticleColumnCatalog.LayoutTypeTargetColumn, StringComparison.OrdinalIgnoreCase));
            layoutType = layoutRow != null ? KBImportLayoutTypeCatalog.Normalize(layoutRow.ExcelColumnRef) : null;
            if (layoutType == null)
            {
                error = "Template ที่เลือกยังไม่ได้เลือกประเภทไฟล์ (กฎหมาย / ไม่ใช่กฎหมาย) กรุณาตั้งค่าที่หน้า config template ก่อน";
                return false;
            }

            var articleTypeRow = columns.FirstOrDefault(c => string.Equals(c.TargetColumnName, "ArticleType", StringComparison.OrdinalIgnoreCase));
            if (articleTypeRow == null || !byte.TryParse(articleTypeRow.ExcelColumnRef, out articleType) || articleType == 0)
            {
                error = "Template ที่เลือกยังไม่ได้ตั้งค่า ArticleType กรุณาตั้งค่าที่หน้า config template ก่อน";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Builds a <see cref="KBImportValidationResult"/> from a validated list of
        /// rows, counting Valid, Skip, and Error statuses.
        /// </summary>
        private static KBImportValidationResult BuildValidationResult(IList<KBImportRow> rows)
        {
            int valid   = 0;
            int skipped = 0;
            int error   = 0;

            foreach (var row in rows)
            {
                if (row.Status == "Valid")      valid++;
                else if (row.Status == "Skip")  skipped++;
                else                            error++;
            }

            return new KBImportValidationResult
            {
                TotalRecords   = rows.Count,
                ValidRecords   = valid,
                SkippedRecords = skipped,
                ErrorRecords   = error,
                Rows           = rows,
            };
        }
    }
}
