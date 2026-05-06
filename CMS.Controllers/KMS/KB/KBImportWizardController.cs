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
        private readonly KBExcelReader             _excelReader;
        private readonly KBImportProcessor         _processor;
        private readonly UserContext               _user;

        /// <summary>Retained between ValidateFile() and ExecuteImport().</summary>
        private KBImportValidationResult _lastValidation;

        // ── Constructor ──────────────────────────────────────────────────────────

        public KBImportWizardController(
            IKBImportWizardController view,
            IKBArticleService         articleService,
            IKBCategoryService        categoryService,
            KBExcelReader             excelReader,
            KBImportProcessor         processor,
            UserContext               user)
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (excelReader     == null) throw new ArgumentNullException("excelReader");
            if (processor       == null) throw new ArgumentNullException("processor");
            if (user            == null) throw new ArgumentNullException("user");

            _view            = view;
            _articleService  = articleService;
            _categoryService = categoryService;
            _excelReader     = excelReader;
            _processor       = processor;
            _user            = user;
        }

        // ── Public API (called by the form) ──────────────────────────────────────

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

                IList<KBImportRow> rows = _excelReader.ReadRows(filePath);
                _processor.ValidateRows(rows);

                _lastValidation = BuildValidationResult(rows);

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
        /// Imports validated rows by resolving/creating categories and inserting
        /// <see cref="KBArticle"/> + <see cref="KBVersion"/> records via
        /// <see cref="KBImportProcessor"/>. Import options are read from the view at
        /// call time. On completion calls
        /// <see cref="IKBImportWizardController.ShowImportComplete"/>.
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

                bool skipWarnings = _view.OptSkipWarning;
                string createdBy  = _user.AdIdentity ?? _user.UserID.ToString();

                // ── Filter rows to import ────────────────────────────────────
                var rowsToImport    = new List<KBImportRow>();
                int preFilterErrors = 0;

                foreach (var row in _lastValidation.Rows)
                {
                    if (row.Status == "Error")                { preFilterErrors++; continue; }
                    if (row.Status == "Warn" && skipWarnings) { preFilterErrors++; continue; }
                    rowsToImport.Add(row);
                }

                // ── Phase 2: category upsert ─────────────────────────────────
                _processor.ResolveCategories(rowsToImport, createdBy);

                // ── Phase 3: article + version insert ────────────────────────
                int successCount;
                int errorCount;
                _processor.InsertArticles(rowsToImport, createdBy, out successCount, out errorCount);

                int totalErrors = preFilterErrors + errorCount;
                var summary = new KBImportValidationResult
                {
                    TotalRecords   = _lastValidation.TotalRecords,
                    ValidRecords   = successCount,
                    WarningRecords = _lastValidation.WarningRecords,
                    ErrorRecords   = totalErrors,
                    IsSuccess      = true,
                    Message        = totalErrors == 0
                        ? string.Format("นำเข้าสำเร็จ {0} รายการ", successCount)
                        : string.Format("นำเข้าสำเร็จ {0} รายการ, ล้มเหลว {1} รายการ", successCount, totalErrors),
                    Rows           = _lastValidation.Rows,
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
        /// Builds a <see cref="KBImportValidationResult"/> from a validated list of
        /// rows, counting Valid, Warn, and Error statuses.
        /// </summary>
        private static KBImportValidationResult BuildValidationResult(IList<KBImportRow> rows)
        {
            int valid   = 0;
            int warn    = 0;
            int error   = 0;

            foreach (var row in rows)
            {
                if (row.Status == "Valid")      valid++;
                else if (row.Status == "Warn")  warn++;
                else                            error++;
            }

            return new KBImportValidationResult
            {
                TotalRecords   = rows.Count,
                ValidRecords   = valid,
                WarningRecords = warn,
                ErrorRecords   = error,
                Rows           = rows,
            };
        }
    }
}
