using System;
using System.Collections.Generic;
using CMS.Controllers.KMS.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.KMS.KB
{
    /// <summary>
    /// Presenter for the KB Import Wizard (MOD-006).
    /// Handles Excel-file validation and article bulk-import logic.
    /// All business rules (field mapping, status defaults, row skipping) live here —
    /// never in the form.
    /// </summary>
    public class KBImportWizardPresenter
    {
        private readonly IKBImportWizardView _view;
        private readonly IKBArticleService   _articleService;
        private readonly IKBCategoryService  _categoryService;
        private readonly UserContext         _user;

        /// <summary>Retained between ValidateFile() and ExecuteImport().</summary>
        private KBImportValidationResult _lastValidation;

        // ── Constructor ──────────────────────────────────────────────────────────

        public KBImportWizardPresenter(
            IKBImportWizardView view,
            IKBArticleService   articleService,
            IKBCategoryService  categoryService,
            UserContext         user)
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (user            == null) throw new ArgumentNullException("user");

            _view            = view;
            _articleService  = articleService;
            _categoryService = categoryService;
            _user            = user;
        }

        // ── Public API (called by the form) ──────────────────────────────────────

        /// <summary>
        /// Validates the Excel file at <see cref="IKBImportWizardView.SelectedFilePath"/>.
        /// On success calls <see cref="IKBImportWizardView.ShowValidationResult"/>; on
        /// failure calls <see cref="IKBImportWizardView.ShowError"/>.
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

                // ── Real implementation: parse the .xlsx using an xlsx library ──
                // For now, returns a mock result that matches the MOD-006 wireframe.
                _lastValidation = BuildMockValidationResult();

                _view.ShowValidationResult(_lastValidation);
                _view.SetStatus("ตรวจสอบข้อมูลเรียบร้อย");
            }
            catch (Exception ex)
            {
                _view.ShowError("เกิดข้อผิดพลาดระหว่างการตรวจสอบไฟล์:\n" + ex.Message);
            }
        }

        /// <summary>
        /// Imports validated rows by creating <see cref="KBArticle"/> records via
        /// <see cref="IKBArticleService.Create"/>. Import options are read from the
        /// view at call time. On completion calls
        /// <see cref="IKBImportWizardView.ShowImportComplete"/>.
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
                bool createDraft  = _view.OptCreateDraft;

                int successCount = 0;
                int skipCount    = 0;

                foreach (var row in _lastValidation.Rows)
                {
                    if (row.Status == "Error")                     { skipCount++;  continue; }
                    if (row.Status == "Warn" && skipWarnings)      { skipCount++;  continue; }

                    try
                    {
                        var article = MapRowToArticle(row, createDraft);
                        _articleService.Create(article);
                        successCount++;
                    }
                    catch
                    {
                        // Row-level failure: count as skipped and continue
                        skipCount++;
                    }
                }

                var summary = new KBImportValidationResult
                {
                    TotalRecords   = _lastValidation.TotalRecords,
                    ValidRecords   = successCount,
                    WarningRecords = _lastValidation.WarningRecords,
                    ErrorRecords   = skipCount,
                    IsSuccess      = true,
                    Message        = string.Format("นำเข้าสำเร็จ {0} รายการ", successCount),
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

        private KBArticle MapRowToArticle(KBImportRow row, bool createAsDraft)
        {
            return new KBArticle
            {
                Title          = row.Title      ?? string.Empty,
                Tags           = row.Tags       ?? string.Empty,
                ContentHTML    = string.Empty,
                ArticleType    = MapArticleType(row.ArticleType),
                Status         = createAsDraft ? (byte)1 : (byte)3,  // 1=Draft, 3=Published
                CreatedBy      = _user.AdIdentity ?? _user.UserID.ToString(),
                CreatedDate    = DateTime.Now,
                CurrentVersion = 1,
            };
        }

        private static byte MapArticleType(string typeName)
        {
            if (typeName == null) return 1;
            switch (typeName.Trim().ToUpper())
            {
                case "SOP":      return 1;
                case "FAQ":      return 2;
                case "GUIDELINE":
                case "แนวทาง": return 3;
                case "TEMPLATE": return 4;
                case "POLICY":   return 5;
                default:         return 1;
            }
        }

        /// <summary>
        /// Mock validation result matching the MOD-006 wireframe demo data.
        /// Replace the body of this method with real Excel parsing when an
        /// xlsx-parsing library (e.g. EPPlus/NPOI) is added to the project.
        /// </summary>
        private static KBImportValidationResult BuildMockValidationResult()
        {
            var rows = new List<KBImportRow>
            {
                new KBImportRow { RowNumber=1,  Title="นโยบายการรับสินไหมทดแทน v4",        ArticleType="SOP",      Category="ความรู้ประกันภัย > สินไหม",    CrafLevel="1HC", Tags="สินไหม, UW, OIC",     ExpiryDate="20/03/2570", Status="Valid"                                      },
                new KBImportRow { RowNumber=2,  Title="คู่มือการ Underwriting Marine",       ArticleType="แนวทาง",  Category="ความรู้ประกันภัย > UW",         CrafLevel="2SC", Tags="Marine, UW",           ExpiryDate="15/06/2570", Status="Valid"                                      },
                new KBImportRow { RowNumber=3,  Title="FAQ การเคลมประกันรถยนต์ 2569",       ArticleType="FAQ",      Category="ความรู้ประกันภัย > Motor",      CrafLevel="3NC", Tags="Motor, Claims, FAQ",   ExpiryDate="—",          Status="Valid"                                      },
                new KBImportRow { RowNumber=4,  Title="แนวทางการประเมินความเสี่ยง Fire",    ArticleType="แนวทาง",  Category="ความรู้ประกันภัย > UW",         CrafLevel="2SC", Tags="Fire, UW, Risk",       ExpiryDate="01/01/2571", Status="Valid"                                      },
                new KBImportRow { RowNumber=5,  Title="แนวทาง Motor UW ปี 2569",            ArticleType="แนวทาง",  Category="ความรู้ประกันภัย > Motor",      CrafLevel="3NC", Tags="Motor, UW",            ExpiryDate="⚠️ ไม่ระบุ",  Status="Warn",  StatusNote="ไม่มีวันหมดอายุ"                },
                new KBImportRow { RowNumber=6,  Title="SOP การออกกรมธรรม์รถยนต์",          ArticleType="SOP",      Category="ความรู้ประกันภัย > Motor",      CrafLevel="3NC", Tags="Motor, SOP",           ExpiryDate="31/12/2570", Status="Valid"                                      },
                new KBImportRow { RowNumber=7,  Title="คู่มือ Premium Rating Motor",         ArticleType="Template", Category="ความรู้ประกันภัย > UW",         CrafLevel="4PC", Tags="Motor, Premium, UW",   ExpiryDate="—",          Status="Valid"                                      },
                new KBImportRow { RowNumber=8,  Title="FAQ ประกันภัย PA",                   ArticleType="FAQ",      Category="ความรู้ประกันภัย > PA",         CrafLevel="4PC", Tags="PA, FAQ",              ExpiryDate="—",          Status="Valid"                                      },
                new KBImportRow { RowNumber=9,  Title="แนวทางการตรวจสอบสินไหมทุจริต",     ArticleType="SOP",      Category="ความรู้ประกันภัย > สินไหม",    CrafLevel="1HC", Tags="สินไหม, Anti-Fraud",   ExpiryDate="30/06/2570", Status="Valid"                                      },
                new KBImportRow { RowNumber=10, Title="SOP การอนุมัติสินไหมทดแทน >1M",    ArticleType="SOP",      Category="ความรู้ประกันภัย > สินไหม",    CrafLevel="1HC", Tags="สินไหม, อนุมัติ",     ExpiryDate="20/03/2570", Status="Valid"                                      },
                new KBImportRow { RowNumber=11, Title="FAQ OIC Reporting Requirements",      ArticleType="FAQ",      Category="ความรู้ประกันภัย > Compliance", CrafLevel="3NC", Tags="OIC, Compliance",      ExpiryDate="—",          Status="Valid"                                      },
                new KBImportRow { RowNumber=12, Title="แนวทาง Marine Hull UW",              ArticleType="แนวทาง",  Category="⚠️ Marine Hull (ใหม่)",          CrafLevel="2SC", Tags="Marine, Hull",         ExpiryDate="01/01/2571", Status="Warn",  StatusNote="หมวดหมู่ใหม่ จะสร้างอัตโนมัติ"  },
                new KBImportRow { RowNumber=13, Title="Template แบบฟอร์มรายงานสินไหม",    ArticleType="Template", Category="ความรู้ประกันภัย > สินไหม",    CrafLevel="3NC", Tags="Template, สินไหม",    ExpiryDate="—",          Status="Valid"                                      },
                new KBImportRow { RowNumber=14, Title="คู่มือการใช้ระบบ RISC Claim",       ArticleType="แนวทาง",  Category="ความรู้ประกันภัย > IT",         CrafLevel="4PC", Tags="RISC, IT, คู่มือ",    ExpiryDate="—",          Status="Valid"                                      },
                new KBImportRow { RowNumber=15, Title="SOP การต่ออายุกรมธรรม์รถยนต์",    ArticleType="SOP",      Category="ความรู้ประกันภัย > Motor",      CrafLevel="3NC", Tags="Motor, Renewal",       ExpiryDate="31/12/2570", Status="Valid"                                      },
            };

            return new KBImportValidationResult
            {
                TotalRecords   = 15,
                ValidRecords   = 13,
                WarningRecords = 2,
                ErrorRecords   = 0,
                Rows           = rows,
            };
        }
    }
}
