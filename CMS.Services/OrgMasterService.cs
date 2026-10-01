using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CMS.Services
{
    public class OrgMasterService : IOrgMasterService
    {
        private readonly IHrDivisionRepository _divisionRepo;
        private readonly IHrDepartmentRepository _deptRepo;
        private readonly IHrPositionRepository _positionRepo;
        private readonly IHrPositionLevelRepository _levelRepo;
        private readonly IHrPositionMappingRepository _mappingRepo;
        private readonly IAuditService _audit;

        public OrgMasterService(
            IHrDivisionRepository divisionRepo,
            IHrDepartmentRepository deptRepo,
            IHrPositionRepository positionRepo,
            IHrPositionLevelRepository levelRepo,
            IHrPositionMappingRepository mappingRepo,
            IAuditService audit = null)
        {
            _divisionRepo = divisionRepo ?? throw new ArgumentNullException(nameof(divisionRepo));
            _deptRepo = deptRepo ?? throw new ArgumentNullException(nameof(deptRepo));
            _positionRepo = positionRepo ?? throw new ArgumentNullException(nameof(positionRepo));
            _levelRepo = levelRepo ?? throw new ArgumentNullException(nameof(levelRepo));
            _mappingRepo = mappingRepo ?? throw new ArgumentNullException(nameof(mappingRepo));
            _audit = audit;
        }

        #region CRUD Operations

        public IEnumerable<HrDivision> GetAllDivisions() => _divisionRepo.GetAll();
        public int SaveDivision(HrDivision division, int userId)
        {
            int id = _divisionRepo.Save(division);
            _audit?.Log(new AuditEntry { Action = "SAVE_DIVISION", Module = "OrgMaster", UserID = userId, Result = 1, NewValue = $"DivisionCode: {division.DivisionCode}" });
            return id;
        }
        public bool DeleteDivision(int divisionId, int userId)
        {
            bool ok = _divisionRepo.Delete(divisionId);
            _audit?.Log(new AuditEntry { Action = "DELETE_DIVISION", Module = "OrgMaster", UserID = userId, Result = ok ? 1 : 0, RecordID = divisionId });
            return ok;
        }

        public IEnumerable<HrDepartment> GetAllDepartments() => _deptRepo.GetAll();
        public IEnumerable<HrDepartment> GetDepartmentsByDivision(int divisionId) => _deptRepo.GetByDivisionID(divisionId);
        public int SaveDepartment(HrDepartment department, int userId)
        {
            int id = _deptRepo.Save(department);
            _audit?.Log(new AuditEntry { Action = "SAVE_DEPARTMENT", Module = "OrgMaster", UserID = userId, Result = 1, NewValue = $"DepartmentCode: {department.DepartmentCode}" });
            return id;
        }
        public bool DeleteDepartment(int departmentId, int userId)
        {
            bool ok = _deptRepo.Delete(departmentId);
            _audit?.Log(new AuditEntry { Action = "DELETE_DEPARTMENT", Module = "OrgMaster", UserID = userId, Result = ok ? 1 : 0, RecordID = departmentId });
            return ok;
        }

        public IEnumerable<HrPosition> GetAllPositions() => _positionRepo.GetAll();
        public int SavePosition(HrPosition position, int userId)
        {
            int id = _positionRepo.Save(position);
            _audit?.Log(new AuditEntry { Action = "SAVE_POSITION", Module = "OrgMaster", UserID = userId, Result = 1, NewValue = $"PositionName: {position.PositionName}" });
            return id;
        }
        public bool DeletePosition(int positionId, int userId)
        {
            bool ok = _positionRepo.Delete(positionId);
            _audit?.Log(new AuditEntry { Action = "DELETE_POSITION", Module = "OrgMaster", UserID = userId, Result = ok ? 1 : 0, RecordID = positionId });
            return ok;
        }

        public IEnumerable<HrPositionLevel> GetAllLevels() => _levelRepo.GetAll();
        public bool SaveLevel(HrPositionLevel level, int userId)
        {
            bool ok = _levelRepo.Save(level);
            _audit?.Log(new AuditEntry { Action = "SAVE_LEVEL", Module = "OrgMaster", UserID = userId, Result = ok ? 1 : 0, NewValue = $"LevelCode: {level.LevelCode}" });
            return ok;
        }
        public bool DeleteLevel(string levelCode, int userId)
        {
            bool ok = _levelRepo.Delete(levelCode);
            _audit?.Log(new AuditEntry { Action = "DELETE_LEVEL", Module = "OrgMaster", UserID = userId, Result = ok ? 1 : 0, NewValue = levelCode });
            return ok;
        }

        public IEnumerable<HrPositionMapping> GetAllMappings() => _mappingRepo.GetAll();
        public int SaveMapping(HrPositionMapping mapping, int userId)
        {
            int id = _mappingRepo.Save(mapping);
            _audit?.Log(new AuditEntry { Action = "SAVE_MAPPING", Module = "OrgMaster", UserID = userId, Result = 1, NewValue = $"Div:{mapping.DivisionID}, Dept:{mapping.DepartmentID}, Pos:{mapping.PositionID}" });
            return id;
        }
        public bool DeleteMapping(int mappingId, int userId)
        {
            bool ok = _mappingRepo.Delete(mappingId);
            _audit?.Log(new AuditEntry { Action = "DELETE_MAPPING", Module = "OrgMaster", UserID = userId, Result = ok ? 1 : 0, RecordID = mappingId });
            return ok;
        }

        #endregion

        #region Excel Template & Import

        public IList<string> GetExcelSheetNames(string filePath)
        {
            var sheetNames = new List<string>();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var workbook = WorkbookFactory.Create(stream);
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    sheetNames.Add(workbook.GetSheetName(i));
                }
            }
            return sheetNames;
        }

        public byte[] GenerateExcelTemplate()
        {
            using (var memoryStream = new MemoryStream())
            {
                var workbook = new XSSFWorkbook();

                // Style for Header
                var headerStyle = workbook.CreateCellStyle();
                headerStyle.FillForegroundColor = IndexedColors.RoyalBlue.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;
                var font = workbook.CreateFont();
                font.IsBold = true;
                font.Color = IndexedColors.White.Index;
                font.FontHeightInPoints = 11;
                headerStyle.SetFont(font);
                headerStyle.Alignment = HorizontalAlignment.Center;
                headerStyle.VerticalAlignment = VerticalAlignment.Center;

                // Style for Sample Rows
                var sampleStyle = workbook.CreateCellStyle();
                var sampleFont = workbook.CreateFont();
                sampleFont.Color = IndexedColors.Grey50Percent.Index;
                sampleFont.IsItalic = true;
                sampleStyle.SetFont(sampleFont);

                // Sheet 1: ผังโครงสร้าง (Org Structure Mapping - Flat)
                var s1 = workbook.CreateSheet("ผังโครงสร้าง (Mapping)");
                var r0_s1 = s1.CreateRow(0);
                string[] h1 = { "รหัสฝ่าย*", "ชื่อฝ่าย*", "รหัสแผนก*", "ชื่อแผนก (ไทย)*", "ชื่อแผนก (Eng)", "ชื่อตำแหน่ง*", "รหัสตำแหน่ง", "ระดับตำแหน่ง (Level)*", "จำนวนคน (HeadCount)" };
                for (int c = 0; c < h1.Length; c++)
                {
                    var cell = r0_s1.CreateCell(c);
                    cell.SetCellValue(h1[c]);
                    cell.CellStyle = headerStyle;
                }
                var sampleRow1 = s1.CreateRow(1);
                sampleRow1.CreateCell(0).SetCellValue("10001");
                sampleRow1.CreateCell(1).SetCellValue("ฝ่ายบริหารงานกลาง");
                sampleRow1.CreateCell(2).SetCellValue("20033");
                sampleRow1.CreateCell(3).SetCellValue("ส่วนงานบริหารและบุคคล");
                sampleRow1.CreateCell(4).SetCellValue("Administration & HR");
                sampleRow1.CreateCell(5).SetCellValue("พนักงานพรบ.");
                sampleRow1.CreateCell(6).SetCellValue("ADM-01");
                sampleRow1.CreateCell(7).SetCellValue("2A");
                sampleRow1.CreateCell(8).SetCellValue("1");

                // Sheet 2: ฝ่าย (Division)
                var s2 = workbook.CreateSheet("ฝ่าย (Division)");
                var r0_s2 = s2.CreateRow(0);
                string[] h2 = { "รหัสฝ่าย (DivisionCode)*", "ชื่อฝ่าย (DivisionName)*", "สถานะ (1=ใช้งาน, 0=ปิด)" };
                for (int c = 0; c < h2.Length; c++)
                {
                    var cell = r0_s2.CreateCell(c);
                    cell.SetCellValue(h2[c]);
                    cell.CellStyle = headerStyle;
                }
                var sampleRow2 = s2.CreateRow(1);
                sampleRow2.CreateCell(0).SetCellValue("10001");
                sampleRow2.CreateCell(1).SetCellValue("ฝ่ายบริหารงานกลาง");
                sampleRow2.CreateCell(2).SetCellValue("1");

                // Sheet 3: แผนก (Department)
                var s3 = workbook.CreateSheet("แผนก (Department)");
                var r0_s3 = s3.CreateRow(0);
                string[] h3 = { "รหัสฝ่าย (DivisionCode)*", "รหัสแผนก (DepartmentCode)*", "ชื่อแผนกภาษาไทย*", "ชื่อแผนกภาษาอังกฤษ", "ระดับแผนก" };
                for (int c = 0; c < h3.Length; c++)
                {
                    var cell = r0_s3.CreateCell(c);
                    cell.SetCellValue(h3[c]);
                    cell.CellStyle = headerStyle;
                }
                var sampleRow3 = s3.CreateRow(1);
                sampleRow3.CreateCell(0).SetCellValue("10001");
                sampleRow3.CreateCell(1).SetCellValue("20033");
                sampleRow3.CreateCell(2).SetCellValue("ส่วนงานบริหารและบุคคล");
                sampleRow3.CreateCell(3).SetCellValue("Administration & HR");
                sampleRow3.CreateCell(4).SetCellValue("DEP");

                // Sheet 4: ตำแหน่ง (Position)
                var s4 = workbook.CreateSheet("ตำแหน่ง (Position)");
                var r0_s4 = s4.CreateRow(0);
                string[] h4 = { "ชื่อตำแหน่ง (PositionName)*", "รหัสตำแหน่ง (PositionCode)", "ระดับตำแหน่ง (LevelCode)*", "สถานะ (1=ใช้งาน, 0=ปิด)" };
                for (int c = 0; c < h4.Length; c++)
                {
                    var cell = r0_s4.CreateCell(c);
                    cell.SetCellValue(h4[c]);
                    cell.CellStyle = headerStyle;
                }
                var sampleRow4 = s4.CreateRow(1);
                sampleRow4.CreateCell(0).SetCellValue("พนักงานพรบ.");
                sampleRow4.CreateCell(1).SetCellValue("ADM-01");
                sampleRow4.CreateCell(2).SetCellValue("2A");
                sampleRow4.CreateCell(3).SetCellValue("1");

                // Sheet 5: ระดับ (Level Master)
                var s5 = workbook.CreateSheet("ระดับตำแหน่ง (Level)");
                var r0_s5 = s5.CreateRow(0);
                string[] h5 = { "รหัสระดับ (LevelCode)*", "ชื่อระดับ (LevelName)", "หมวดหมู่ (LevelCategory)", "ลำดับการเรียง (SortOrder)" };
                for (int c = 0; c < h5.Length; c++)
                {
                    var cell = r0_s5.CreateCell(c);
                    cell.SetCellValue(h5[c]);
                    cell.CellStyle = headerStyle;
                }
                var sampleRow5 = s5.CreateRow(1);
                sampleRow5.CreateCell(0).SetCellValue("2A");
                sampleRow5.CreateCell(1).SetCellValue("Senior Staff");
                sampleRow5.CreateCell(2).SetCellValue("Staff");
                sampleRow5.CreateCell(3).SetCellValue("3");

                // Auto size columns
                for (int i = 0; i < 9; i++) s1.AutoSizeColumn(i);
                for (int i = 0; i < 3; i++) s2.AutoSizeColumn(i);
                for (int i = 0; i < 5; i++) s3.AutoSizeColumn(i);
                for (int i = 0; i < 4; i++) s4.AutoSizeColumn(i);
                for (int i = 0; i < 4; i++) s5.AutoSizeColumn(i);

                workbook.Write(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public OrgMasterPreview ParseAndValidateExcel(string filePath, string sheetName)
        {
            var preview = new OrgMasterPreview { FilePath = filePath, SheetName = sheetName };

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var workbook = WorkbookFactory.Create(stream);
                ISheet sheet = null;

                if (!string.IsNullOrEmpty(sheetName))
                    sheet = workbook.GetSheet(sheetName);
                if (sheet == null && workbook.NumberOfSheets > 0)
                    sheet = workbook.GetSheetAt(0);

                if (sheet == null)
                {
                    return preview;
                }

                // Detect Sheet Type from Header
                var headerRow = sheet.GetRow(0);
                if (headerRow == null) return preview;

                var formatter = new DataFormatter();
                string firstCol = formatter.FormatCellValue(headerRow.GetCell(0)).Trim();

                OrgMasterType targetType = OrgMasterType.Mapping;
                if (firstCol.Contains("รหัสฝ่าย") && headerRow.LastCellNum <= 3)
                    targetType = OrgMasterType.Division;
                else if (firstCol.Contains("รหัสฝ่าย") && headerRow.LastCellNum <= 5 && !headerRow.Cells.Any(c => formatter.FormatCellValue(c).Contains("ตำแหน่ง")))
                    targetType = OrgMasterType.Department;
                else if (firstCol.Contains("ชื่อตำแหน่ง") || firstCol.Contains("รหัสตำแหน่ง"))
                    targetType = OrgMasterType.Position;
                else if (firstCol.Contains("รหัสระดับ") || firstCol.Contains("LevelCode"))
                    targetType = OrgMasterType.Level;

                for (int r = 1; r <= sheet.LastRowNum; r++)
                {
                    var row = sheet.GetRow(r);
                    if (row == null) continue;

                    var item = new OrgMasterImportRow
                    {
                        RowIndex = r + 1,
                        MasterType = targetType,
                        Status = OrgMasterImportStatus.ValidNew
                    };

                    if (targetType == OrgMasterType.Mapping)
                    {
                        item.DivisionCode = formatter.FormatCellValue(row.GetCell(0)).Trim();
                        item.DivisionName = formatter.FormatCellValue(row.GetCell(1)).Trim();
                        item.DepartmentCode = formatter.FormatCellValue(row.GetCell(2)).Trim();
                        item.DepartmentNameTh = formatter.FormatCellValue(row.GetCell(3)).Trim();
                        item.DepartmentNameEn = formatter.FormatCellValue(row.GetCell(4)).Trim();
                        item.PositionName = formatter.FormatCellValue(row.GetCell(5)).Trim();
                        item.PositionCode = formatter.FormatCellValue(row.GetCell(6)).Trim();
                        item.PositionLevel = formatter.FormatCellValue(row.GetCell(7)).Trim().ToUpper();

                        string hc = formatter.FormatCellValue(row.GetCell(8)).Trim();
                        item.HeadCount = int.TryParse(hc, out int parsedHc) ? parsedHc : 1;

                        if (string.IsNullOrEmpty(item.DivisionCode) && string.IsNullOrEmpty(item.DepartmentCode) && string.IsNullOrEmpty(item.PositionName))
                            continue; // Skip blank row

                        if (string.IsNullOrEmpty(item.DivisionCode))
                        {
                            item.Status = OrgMasterImportStatus.Error;
                            item.ErrorMessage = "ไม่ระบุรหัสฝ่าย (DivisionCode)";
                        }
                        else if (string.IsNullOrEmpty(item.DepartmentCode))
                        {
                            item.Status = OrgMasterImportStatus.Error;
                            item.ErrorMessage = "ไม่ระบุรหัสแผนก (DepartmentCode)";
                        }
                        else if (string.IsNullOrEmpty(item.PositionName))
                        {
                            item.Status = OrgMasterImportStatus.Error;
                            item.ErrorMessage = "ไม่ระบุชื่อตำแหน่ง (PositionName)";
                        }
                    }
                    else if (targetType == OrgMasterType.Division)
                    {
                        item.DivisionCode = formatter.FormatCellValue(row.GetCell(0)).Trim();
                        item.DivisionName = formatter.FormatCellValue(row.GetCell(1)).Trim();
                        string active = formatter.FormatCellValue(row.GetCell(2)).Trim();
                        item.IsActive = string.IsNullOrEmpty(active) || active == "1" || active.Equals("true", StringComparison.OrdinalIgnoreCase);

                        if (string.IsNullOrEmpty(item.DivisionCode) && string.IsNullOrEmpty(item.DivisionName)) continue;
                        if (string.IsNullOrEmpty(item.DivisionCode))
                        {
                            item.Status = OrgMasterImportStatus.Error;
                            item.ErrorMessage = "ไม่ระบุรหัสฝ่าย";
                        }
                    }
                    else if (targetType == OrgMasterType.Department)
                    {
                        item.DivisionCode = formatter.FormatCellValue(row.GetCell(0)).Trim();
                        item.DepartmentCode = formatter.FormatCellValue(row.GetCell(1)).Trim();
                        item.DepartmentNameTh = formatter.FormatCellValue(row.GetCell(2)).Trim();
                        item.DepartmentNameEn = formatter.FormatCellValue(row.GetCell(3)).Trim();
                        item.DepartmentLevel = formatter.FormatCellValue(row.GetCell(4)).Trim();

                        if (string.IsNullOrEmpty(item.DepartmentCode) && string.IsNullOrEmpty(item.DepartmentNameTh)) continue;
                        if (string.IsNullOrEmpty(item.DepartmentCode))
                        {
                            item.Status = OrgMasterImportStatus.Error;
                            item.ErrorMessage = "ไม่ระบุรหัสแผนก";
                        }
                    }
                    else if (targetType == OrgMasterType.Position)
                    {
                        item.PositionName = formatter.FormatCellValue(row.GetCell(0)).Trim();
                        item.PositionCode = formatter.FormatCellValue(row.GetCell(1)).Trim();
                        item.PositionLevel = formatter.FormatCellValue(row.GetCell(2)).Trim().ToUpper();

                        if (string.IsNullOrEmpty(item.PositionName)) continue;
                    }
                    else if (targetType == OrgMasterType.Level)
                    {
                        item.LevelCode = formatter.FormatCellValue(row.GetCell(0)).Trim().ToUpper();
                        item.LevelName = formatter.FormatCellValue(row.GetCell(1)).Trim();
                        item.LevelCategory = formatter.FormatCellValue(row.GetCell(2)).Trim();
                        string sortStr = formatter.FormatCellValue(row.GetCell(3)).Trim();
                        item.SortOrder = int.TryParse(sortStr, out int so) ? so : (int?)null;

                        if (string.IsNullOrEmpty(item.LevelCode)) continue;
                    }

                    preview.Rows.Add(item);
                }
            }

            return preview;
        }

        public OrgMasterImportResult ExecuteImport(OrgMasterPreview preview, int userId)
        {
            var result = new OrgMasterImportResult { Success = true };
            if (preview == null || preview.Rows == null || preview.Rows.Count == 0)
            {
                result.Message = "ไม่มีข้อมูลสำหรับนำเข้า";
                return result;
            }

            // Cache current masters
            var divisions = _divisionRepo.GetAll().ToDictionary(d => d.DivisionCode.Trim(), StringComparer.OrdinalIgnoreCase);
            var departments = _deptRepo.GetAll().ToDictionary(d => d.DepartmentCode.Trim(), StringComparer.OrdinalIgnoreCase);
            var positions = _positionRepo.GetAll().ToDictionary(p => p.PositionName.Trim(), StringComparer.OrdinalIgnoreCase);
            var levels = _levelRepo.GetAll().ToDictionary(l => l.LevelCode.Trim(), StringComparer.OrdinalIgnoreCase);

            foreach (var row in preview.Rows.Where(r => r.Status != OrgMasterImportStatus.Error))
            {
                try
                {
                    if (row.MasterType == OrgMasterType.Mapping)
                    {
                        // 1. Division Upsert
                        int divId = 0;
                        if (!string.IsNullOrEmpty(row.DivisionCode))
                        {
                            if (divisions.TryGetValue(row.DivisionCode, out var existingDiv))
                            {
                                divId = existingDiv.DivisionID;
                                if (!string.IsNullOrEmpty(row.DivisionName) && existingDiv.DivisionName != row.DivisionName)
                                {
                                    existingDiv.DivisionName = row.DivisionName;
                                    _divisionRepo.Save(existingDiv);
                                }
                            }
                            else
                            {
                                var newDiv = new HrDivision { DivisionCode = row.DivisionCode, DivisionName = row.DivisionName ?? row.DivisionCode, IsActive = true };
                                divId = _divisionRepo.Save(newDiv);
                                newDiv.DivisionID = divId;
                                divisions[row.DivisionCode] = newDiv;
                            }
                        }

                        // 2. Department Upsert
                        int deptId = 0;
                        if (!string.IsNullOrEmpty(row.DepartmentCode))
                        {
                            if (departments.TryGetValue(row.DepartmentCode, out var existingDept))
                            {
                                deptId = existingDept.DepartmentID;
                                if (!string.IsNullOrEmpty(row.DepartmentNameTh) && existingDept.DepartmentNameTh != row.DepartmentNameTh)
                                {
                                    existingDept.DepartmentNameTh = row.DepartmentNameTh;
                                    existingDept.DepartmentName = row.DepartmentNameTh;
                                    if (divId > 0) existingDept.DivisionID = divId;
                                    _deptRepo.Save(existingDept);
                                }
                            }
                            else
                            {
                                var newDept = new HrDepartment
                                {
                                    DivisionID = divId,
                                    DepartmentCode = row.DepartmentCode,
                                    DepartmentName = row.DepartmentNameTh ?? row.DepartmentCode,
                                    DepartmentNameTh = row.DepartmentNameTh ?? row.DepartmentCode,
                                    DepartmentNameEn = row.DepartmentNameEn ?? "",
                                    Level = "DEP",
                                    IsActive = true
                                };
                                deptId = _deptRepo.Save(newDept);
                                newDept.DepartmentID = deptId;
                                departments[row.DepartmentCode] = newDept;
                            }
                        }

                        // 3. Position Upsert
                        int posId = 0;
                        if (!string.IsNullOrEmpty(row.PositionName))
                        {
                            if (positions.TryGetValue(row.PositionName, out var existingPos))
                            {
                                posId = existingPos.PositionID;
                                if (!string.IsNullOrEmpty(row.PositionLevel) && existingPos.LevelCode != row.PositionLevel)
                                {
                                    existingPos.LevelCode = row.PositionLevel;
                                    _positionRepo.Save(existingPos);
                                }
                            }
                            else
                            {
                                var newPos = new HrPosition
                                {
                                    PositionCode = row.PositionCode ?? "",
                                    PositionName = row.PositionName,
                                    LevelCode = row.PositionLevel ?? "",
                                    IsActive = true
                                };
                                posId = _positionRepo.Save(newPos);
                                newPos.PositionID = posId;
                                positions[row.PositionName] = newPos;
                            }
                        }

                        // 4. Level Master Upsert
                        if (!string.IsNullOrEmpty(row.PositionLevel) && !levels.ContainsKey(row.PositionLevel))
                        {
                            var newLevel = new HrPositionLevel { LevelCode = row.PositionLevel, LevelName = row.PositionLevel, IsActive = true };
                            _levelRepo.Save(newLevel);
                            levels[row.PositionLevel] = newLevel;
                        }

                        // 5. Position Mapping Upsert
                        if (divId > 0 && deptId > 0 && posId > 0)
                        {
                            var mapping = new HrPositionMapping
                            {
                                DivisionID = divId,
                                DepartmentID = deptId,
                                PositionID = posId,
                                HeadCount = row.HeadCount ?? 1,
                                IsActive = true
                            };
                            _mappingRepo.Save(mapping);
                            result.InsertedCount++;
                        }
                    }
                    else if (row.MasterType == OrgMasterType.Division)
                    {
                        var div = new HrDivision { DivisionCode = row.DivisionCode, DivisionName = row.DivisionName ?? row.DivisionCode, IsActive = row.IsActive };
                        _divisionRepo.Save(div);
                        result.InsertedCount++;
                    }
                    else if (row.MasterType == OrgMasterType.Department)
                    {
                        int divId = 0;
                        if (!string.IsNullOrEmpty(row.DivisionCode) && divisions.TryGetValue(row.DivisionCode, out var d))
                            divId = d.DivisionID;

                        var dept = new HrDepartment
                        {
                            DivisionID = divId,
                            DepartmentCode = row.DepartmentCode,
                            DepartmentName = row.DepartmentNameTh ?? row.DepartmentCode,
                            DepartmentNameTh = row.DepartmentNameTh ?? row.DepartmentCode,
                            DepartmentNameEn = row.DepartmentNameEn ?? "",
                            Level = row.DepartmentLevel ?? "DEP",
                            IsActive = true
                        };
                        _deptRepo.Save(dept);
                        result.InsertedCount++;
                    }
                    else if (row.MasterType == OrgMasterType.Position)
                    {
                        var pos = new HrPosition
                        {
                            PositionCode = row.PositionCode ?? "",
                            PositionName = row.PositionName,
                            LevelCode = row.PositionLevel ?? "",
                            IsActive = true
                        };
                        _positionRepo.Save(pos);
                        result.InsertedCount++;
                    }
                    else if (row.MasterType == OrgMasterType.Level)
                    {
                        var lvl = new HrPositionLevel
                        {
                            LevelCode = row.LevelCode,
                            LevelName = row.LevelName ?? row.LevelCode,
                            LevelCategory = row.LevelCategory ?? "",
                            SortOrder = row.SortOrder,
                            IsActive = true
                        };
                        _levelRepo.Save(lvl);
                        result.InsertedCount++;
                    }
                }
                catch
                {
                    result.FailedCount++;
                }
            }

            result.Message = $"นำเข้าข้อมูลเสร็จสิ้น: สำเร็จ {result.InsertedCount} รายการ, ผิดพลาด {result.FailedCount} รายการ";
            _audit?.Log(new AuditEntry { Action = "IMPORT_ORG_MASTER", Module = "OrgMaster", UserID = userId, Result = 1, NewValue = result.Message });
            return result;
        }

        #endregion
    }
}
