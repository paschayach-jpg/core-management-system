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

                int  resolvedCategoryId;
                byte resolvedArticleType;
                string resolveError;
                if (!TryResolveTemplateFixedValues(templateColumns, out resolvedCategoryId, out resolvedArticleType, out resolveError))
                {
                    _view.ShowError(resolveError);
                    return;
                }

                IList<KBImportRow> rows = _excelReader.ReadRows(filePath, templateColumns);

                // Effective LayoutType is derived from the Template's configured CategoryID (see
                // KBImportTemplateService.GetEffectiveLayoutType) and must match the layout
                // KBExcelReader actually detected from the file's header row — otherwise
                // CategoryID/ArticleType resolved above would be silently applied to the wrong
                // kind of document.
                string templateLayoutType = _templateService.GetEffectiveLayoutType(resolvedCategoryId);
                if (rows.Count > 0 && !string.Equals(rows[0].RowKind, templateLayoutType, StringComparison.OrdinalIgnoreCase))
                {
                    _view.ShowError(string.Format(
                        "ไฟล์ที่อัปโหลดเป็นรูปแบบ \"{0}\" แต่ Template \"{1}\" ใช้ CategoryID ของ \"{2}\"\n\nกรุณาเลือก Template ให้ตรงกับไฟล์",
                        KBImportLayoutTypeCatalog.Describe(rows[0].RowKind), template.TemplateName, KBImportLayoutTypeCatalog.Describe(templateLayoutType)));
                    return;
                }

                _processor.ValidateRows(rows, resolvedCategoryId);

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
        /// records via <see cref="KBImportProcessor"/>. Rows marked <c>Skip</c> (blank/duplicate/
        /// already-exists ArticleCode) or <c>Error</c> are excluded before the import attempt.
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

                // Consume this validation snapshot now — a validated batch may only be
                // imported once. Without this, clicking "นำเข้า" a second time on the same
                // (already-inserted) rows re-attempts InsertArticles with stale Status="Valid"
                // rows, hitting the real DB unique-key constraint on every row instead of the
                // friendly ExistsByCode pre-check, and the summary shows everything as failed
                // duplicate key. Forcing a fresh ValidateFile() before re-import re-runs
                // ExistsByCode and correctly marks already-imported rows as "Skip" instead.
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
                    _view.OptSkipRowOnDownloadFailure,
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

                // Collect rows that failed DURING import (ExistsByCode race or DB insert failure).
                var importFailedRows = rowsToImport.Where(r => r.Status == "Error").ToList();

                int totalErrors = skippedRows.Count(r => r.Status == "Error") + errorCount;
                var summary = new KBImportValidationResult
                {
                    TotalRecords     = lastValidation.TotalRecords,
                    ValidRecords     = successCount,
                    WarningRecords   = 0,
                    SkippedRecords   = lastValidation.SkippedRecords,
                    ErrorRecords     = totalErrors,
                    IsSuccess        = true,
                    Message          = totalErrors == 0
                        ? string.Format("นำเข้าสำเร็จ {0} รายการ", successCount)
                        : string.Format("นำเข้าสำเร็จ {0} รายการ, ถูกข้าม/ล้มเหลว {1} รายการ", successCount, totalErrors),
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
        /// Resolves the CategoryID/ArticleType fixed values configured on the given Template's
        /// column mapping (<see cref="KBImportTemplateColumnRow.ExcelColumnRef"/> for the
        /// "CategoryID"/"ArticleType" rows — set via the dropdown in KBImportTemplateConfigForm).
        /// Returns <c>false</c> with a user-facing Thai error message when either is missing or
        /// not configured.
        /// </summary>
        private bool TryResolveTemplateFixedValues(IList<KBImportTemplateColumnRow> columns, out int categoryId, out byte articleType, out string error)
        {
            categoryId  = 0;
            articleType = 0;
            error       = null;

            var categoryRow = columns.FirstOrDefault(c => string.Equals(c.TargetColumnName, "CategoryID", StringComparison.OrdinalIgnoreCase));
            if (categoryRow == null || !int.TryParse(categoryRow.ExcelColumnRef, out categoryId) || categoryId <= 0)
            {
                error = "Template ที่เลือกยังไม่ได้ตั้งค่า CategoryID กรุณาตั้งค่าที่หน้า config template ก่อน";
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
