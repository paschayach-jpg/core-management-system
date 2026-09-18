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
    /// Presenter ของ wizard นำเข้า ประกาศ/คำสั่ง
    /// step 1 เลือกไฟล์ + sheet → step 2 validate + เลือกมาตรา → step 3 นำเข้า
    /// </summary>
    public class KBOrderImportWizardController
    {
        private readonly IKBOrderImportWizardView       _view;
        private readonly IKBOrderExcelReaderService     _reader;
        private readonly IKBOrderImportProcessorService _processor;
        private readonly UserContext                    _user;

        private IList<KBOrderImportRow> _rows;
        private IList<KBSectionGroup>   _sectionGroups;
        private KBOrderImportResult     _lastValidation;

        public KBOrderImportWizardController(
            IKBOrderImportWizardView       view,
            IKBOrderExcelReaderService     reader,
            IKBOrderImportProcessorService processor,
            UserContext                    user)
        {
            if (view      == null) throw new ArgumentNullException("view");
            if (reader    == null) throw new ArgumentNullException("reader");
            if (processor == null) throw new ArgumentNullException("processor");
            if (user      == null) throw new ArgumentNullException("user");

            _view      = view;
            _reader    = reader;
            _processor = processor;
            _user      = user;
        }

        /// <summary>เรียกหลัง user เลือกไฟล์ใน step 1 เพื่อเติมรายชื่อ sheet</summary>
        public void LoadSheetNames()
        {
            string filePath = _view.SelectedFilePath;
            if (string.IsNullOrWhiteSpace(filePath)) return;

            try
            {
                _view.ShowSheetNames(_reader.GetSheetNames(filePath));
            }
            catch (KBExcelReadException ex)
            {
                _view.ShowError("ไม่สามารถเปิดไฟล์ Excel ได้:\n" + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("อ่านรายชื่อ sheet ไม่สำเร็จ:\n" + ex.Message);
            }
        }

        /// <summary>step 1 → step 2</summary>
        public void ValidateFile()
        {
            string filePath = _view.SelectedFilePath;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _view.ShowError("กรุณาเลือกไฟล์ Excel ก่อน");
                return;
            }

            try
            {
                _view.SetStatus("กำลังอ่านไฟล์...");
                _rows = _reader.ReadRows(filePath, _view.SelectedSheetName);

                if (_rows.Count == 0)
                {
                    _view.ShowError("ไม่พบข้อมูลใน sheet ที่เลือก");
                    return;
                }

                _view.SetStatus("กำลังค้นหามาตราที่อ้างถึง...");
                _sectionGroups = _processor.ResolveSectionGroups(_rows);

                _view.SetStatus("กำลังตรวจสอบข้อมูล...");
                _processor.ValidateRows(_rows, _sectionGroups);

                _lastValidation = BuildResult(_rows);
                _view.ShowValidationResult(_lastValidation, _sectionGroups);
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

        /// <summary>step 2 → step 3</summary>
        public void ExecuteImport()
        {
            if (_lastValidation == null || _rows == null)
            {
                _view.ShowError("กรุณาตรวจสอบไฟล์ก่อนทำการนำเข้า");
                return;
            }

            // ทุกมาตราที่มีหลายปีต้องถูกเลือกก่อน
            var unresolved = _sectionGroups
                .Where(g => g.Candidates.Count > 0 && g.SelectedArticleId <= 0)
                .Select(g => g.SectionRaw)
                .ToList();

            if (unresolved.Count > 0)
            {
                _view.ShowError("กรุณาเลือกปีของมาตราให้ครบก่อนนำเข้า:\n" +
                                string.Join(", ", unresolved.ToArray()));
                return;
            }

            try
            {
                _view.SetProgress(5, "กำลังคัดกรองแถวข้อมูล...");

                var skippedRows   = _rows.Where(r => r.Status == "Error").ToList();
                var rowsToImport  = _rows.Where(r => r.Status != "Error").ToList();

                _view.SetProgress(10, string.Format("พร้อมนำเข้า {0} รายการ", rowsToImport.Count));

                int successCount, errorCount;
                _processor.InsertArticles(
                    rowsToImport,
                    _sectionGroups,
                    _view.OptSkipRowOnDownloadFailure,
                    _user,
                    out successCount,
                    out errorCount,
                    (done, total) =>
                    {
                        int pct = total == 0 ? 99 : 10 + (done * 89 / total);
                        _view.SetProgress(pct,
                            string.Format("กำลังนำเข้าบทความ... {0}/{1}", done, total));
                    });

                _view.SetProgress(100, "นำเข้าเสร็จสิ้น");

                var summary = new KBOrderImportResult
                {
                    TotalRecords     = _lastValidation.TotalRecords,
                    ValidRecords     = successCount,
                    WarningRecords   = rowsToImport.Count(r => r.Status == "Warn"),
                    ErrorRecords     = skippedRows.Count + errorCount,
                    IsSuccess        = true,
                    Message          = string.Format("นำเข้าสำเร็จ {0} รายการ", successCount),
                    Rows             = _rows,
                    SkippedRows      = skippedRows,
                    ImportFailedRows = rowsToImport.Where(r => r.Status == "Error").ToList(),
                };

                _view.ShowImportComplete(summary);
                _view.SetStatus("✅  นำเข้าข้อมูลเรียบร้อย");
            }
            catch (Exception ex)
            {
                _view.ShowError("เกิดข้อผิดพลาดระหว่างการนำเข้า:\n" + ex.Message);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static KBOrderImportResult BuildResult(IList<KBOrderImportRow> rows)
        {
            return new KBOrderImportResult
            {
                TotalRecords   = rows.Count,
                ValidRecords   = rows.Count(r => r.Status == "Valid"),
                WarningRecords = rows.Count(r => r.Status == "Warn"),
                ErrorRecords   = rows.Count(r => r.Status == "Error"),
                IsSuccess      = true,
                Rows           = rows,
            };
        }
    }
}
