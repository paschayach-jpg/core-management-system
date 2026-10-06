using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class AuthorityImportService : IAuthorityImportService
    {
        private const int AdminRole = 4;
        private static readonly string[] LevelCodes = { "1HC", "2SC", "3NC", "4PC" };
        private static readonly Regex LevelPattern = new Regex(@"^[1-9][A-Z]$", RegexOptions.Compiled);

        private readonly IFolderAuthorityService _authority;
        private readonly IUserAccountRepository _users;
        private readonly IHrDepartmentRepository _departments;
        private readonly IHrDivisionRepository _divisions;
        private readonly IAuditService _audit;
        private readonly AuthorityExcelParser _parser;

        public AuthorityImportService(IFolderAuthorityService authority, IUserAccountRepository users,
            IHrDepartmentRepository departments, IHrDivisionRepository divisions, IAuditService audit,
            AuthorityExcelParser parser = null)
        {
            if (authority == null) throw new ArgumentNullException("authority");
            if (users == null) throw new ArgumentNullException("users");
            if (departments == null) throw new ArgumentNullException("departments");
            if (divisions == null) throw new ArgumentNullException("divisions");
            if (audit == null) throw new ArgumentNullException("audit");
            _authority = authority;
            _users = users;
            _departments = departments;
            _divisions = divisions;
            _audit = audit;
            _parser = parser ?? new AuthorityExcelParser();
        }

        public IList<string> GetSheetNames(string filePath, UserContext user)
        {
            RequireAdmin(user);
            return _parser.GetSheetNames(filePath);
        }

        public AuthorityImportPreview BuildPreview(string filePath, string sheetName, bool updateUserLevel, bool importPositions, UserContext user)
        {
            RequireAdmin(user);
            var preview = new AuthorityImportPreview
            {
                FilePath = filePath, SheetName = sheetName, UpdateUserLevel = updateUserLevel,
                ImportPositions = importPositions
            };

            preview.Rows.AddRange(_parser.Parse(filePath, sheetName, preview.Issues));
            if (!importPositions)
                preview.Issues.RemoveAll(i => i.Code == "UNEXPECTED_CELL"); // permission cells are ignored in this mode
            if (preview.HasErrors) return preview;

            ValidateRows(preview, updateUserLevel, importPositions);
            if (importPositions) BuildPositionPlans(preview);
            if (updateUserLevel) BuildLevelPlans(preview);
            return preview;
        }

        // ── Validation ──────────────────────────────────────────────────────

        private static void ValidateRows(AuthorityImportPreview p, bool checkLevel, bool checkPositions)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in p.Rows)
            {
                if (checkPositions && string.IsNullOrEmpty(r.Position))
                    Add(p, r.RowNumber, AuthorityImportSeverity.Warning, "MISSING_POSITION", "ไม่มีตำแหน่ง ข้ามการอัปเดตสิทธิ์ของแถวนี้ (Level ยังอัปเดตตามปกติ)");
                if (!seen.Add(r.EmployeeCode))
                    Add(p, r.RowNumber, AuthorityImportSeverity.Error, "DUPLICATE_EMPLOYEE", "รหัสพนักงานซ้ำในไฟล์: " + r.EmployeeCode);
                if (checkLevel && !LevelPattern.IsMatch(r.Level ?? ""))
                    Add(p, r.RowNumber, AuthorityImportSeverity.Error, "INVALID_LEVEL", "รูปแบบ Level ไม่ถูกต้อง: \"" + r.Level + "\"");
                if (!checkPositions) continue;
                for (int g = 0; g < 4; g++)
                    if (r.Permissions[g * 4 + 3] && !r.Permissions[g * 4])
                        Add(p, r.RowNumber, AuthorityImportSeverity.Warning, "APPROVE_WITHOUT_VIEW",
                            "มีสิทธิ์อนุมัติแต่ไม่มีสิทธิ์ดู (" + LevelCodes[g] + ")");
                if (!string.IsNullOrEmpty(r.Remark))
                    Add(p, r.RowNumber, AuthorityImportSeverity.Warning, "REMARK", "หมายเหตุ: " + r.Remark);
            }
        }

        // ── Position plans ──────────────────────────────────────────────────

        /// <summary>
        /// Update-only: refreshes the 4 permission boxes (HC/SC/NC/PC x view/add/edit/approve) of positions that
        /// already have a config (matched by ConfigLabel = position name). Department/division/etc. of the existing
        /// config are kept. Positions without a config are skipped with a warning; they are never created.
        /// Several rows of the same position are merged (a box is on if any row ticks it) with a warning.
        /// </summary>
        private void BuildPositionPlans(AuthorityImportPreview p)
        {
            var existing = _authority.GetAll().GroupBy(c => c.ConfigLabel ?? "", StringComparer.OrdinalIgnoreCase)
                                     .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            foreach (var grp in p.Rows.Where(r => !string.IsNullOrEmpty(r.Position))
                                      .GroupBy(r => r.Position.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var first = grp.First();

                List<FolderAuthorityConfig> rows;
                if (!existing.TryGetValue(grp.Key, out rows) || rows.Count == 0)
                {
                    Add(p, first.RowNumber, AuthorityImportSeverity.Warning, "POSITION_NOT_FOUND",
                        "ไม่พบ config ของตำแหน่ง \"" + grp.Key + "\" ในระบบ จึงไม่อัปเดตสิทธิ์ตำแหน่งนี้ (ไม่สร้างใหม่)");
                    continue;
                }

                var perms = (bool[])first.Permissions.Clone();
                var mergedRows = new List<int>();
                foreach (var r in grp.Skip(1))
                {
                    if (r.Permissions.SequenceEqual(first.Permissions)) continue;
                    for (int i = 0; i < 16; i++) perms[i] = perms[i] || r.Permissions[i];
                    mergedRows.Add(r.RowNumber);
                }
                if (mergedRows.Count > 0)
                    Add(p, first.RowNumber, AuthorityImportSeverity.Warning, "POSITION_MERGED",
                        "ตำแหน่ง \"" + grp.Key + "\" มีสิทธิ์ไม่ตรงกันในแถว " + first.RowNumber + ", " + string.Join(", ", mergedRows.Select(x => x.ToString()).ToArray())
                        + " จึงรวมสิทธิ์ (ติ๊กถ้าแถวใดแถวหนึ่งติ๊ก)");

                var plan = new AuthorityImportPositionPlan
                {
                    Label = rows[0].ConfigLabel, PositionName = rows[0].PositionName ?? grp.Key, Permissions = perms,
                    DepartmentCode = rows[0].DepartmentCode, DivisionCode = rows[0].DivisionCode
                };
                plan.Status = SameAsExisting(plan, rows) ? AuthorityImportStatus.Unchanged : AuthorityImportStatus.Updated;
                p.PositionPlans.Add(plan);
            }
        }

        private static bool SameAsExisting(AuthorityImportPositionPlan plan, List<FolderAuthorityConfig> rows)
        {
            if (!string.Equals(plan.DepartmentCode ?? "", rows[0].DepartmentCode ?? "", StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.Equals(plan.DivisionCode ?? "", rows[0].DivisionCode ?? "", StringComparison.OrdinalIgnoreCase)) return false;
            for (int g = 0; g < 4; g++)
            {
                var row = rows.FirstOrDefault(r => r.ConfidentialityLevel == g + 1);
                bool v = row != null && row.CanView, a = row != null && row.CanAdd,
                     e = row != null && row.CanEdit, ap = row != null && row.CanApprove;
                if (v != plan.Permissions[g * 4] || a != plan.Permissions[g * 4 + 1] ||
                    e != plan.Permissions[g * 4 + 2] || ap != plan.Permissions[g * 4 + 3]) return false;
            }
            return true;
        }

        // ── Level plans ─────────────────────────────────────────────────────

        private void BuildLevelPlans(AuthorityImportPreview p)
        {
            var byCode = new Dictionary<string, UserAccount>(StringComparer.OrdinalIgnoreCase);
            foreach (var u in _users.GetAll())
                if (!string.IsNullOrWhiteSpace(u.EmployeeCode) && !byCode.ContainsKey(u.EmployeeCode.Trim()))
                    byCode[u.EmployeeCode.Trim()] = u;

            foreach (var r in p.Rows)
            {
                UserAccount u;
                if (!byCode.TryGetValue(r.EmployeeCode, out u))
                {
                    p.LevelPlans.Add(new AuthorityImportLevelPlan
                    {
                        EmployeeCode = r.EmployeeCode, EmployeeName = r.EmployeeName, NewLevel = r.Level,
                        Status = AuthorityImportStatus.NotFound
                    });
                    Add(p, r.RowNumber, AuthorityImportSeverity.Warning, "USER_NOT_FOUND",
                        "ไม่พบผู้ใช้รหัส " + r.EmployeeCode + " ในระบบ (อาจยังไม่เคย login) ข้ามการอัปเดต Level");
                    continue;
                }
                string old = (u.LevelCode ?? "").Trim().ToUpperInvariant();
                p.LevelPlans.Add(new AuthorityImportLevelPlan
                {
                    EmployeeCode = r.EmployeeCode, EmployeeName = r.EmployeeName, UserId = u.UserID, OldLevel = old, NewLevel = r.Level,
                    Status = old == r.Level ? AuthorityImportStatus.Unchanged : AuthorityImportStatus.Updated
                });
            }
        }

        // ── Commit ──────────────────────────────────────────────────────────

        public AuthorityImportResult Commit(AuthorityImportPreview preview, UserContext user)
        {
            RequireAdmin(user);
            if (preview == null) throw new ArgumentNullException("preview");
            if (preview.HasErrors) throw new InvalidOperationException("ไม่สามารถนำเข้าได้ เพราะยังมี Error");

            var result = new AuthorityImportResult();
            var existing = _authority.GetAll().ToList();
            var undo = new Stack<Action>();

            try
            {
                foreach (var plan in preview.PositionPlans.Where(x => x.Status != AuthorityImportStatus.Unchanged))
                {
                    var snapshot = existing
                        .Where(c => string.Equals(c.ConfigLabel, plan.Label, StringComparison.OrdinalIgnoreCase))
                        .Select(Clone).ToList();
                    var label = plan.Label;
                    var uid = user.UserID;

                    if (plan.Status == AuthorityImportStatus.Updated) _authority.Delete(label, uid);
                    undo.Push(() => Restore(label, snapshot, uid));
                    for (int g = 0; g < 4; g++)
                        _authority.Save(new FolderAuthorityConfig
                        {
                            ConfigLabel = plan.Label, PositionName = plan.PositionName,
                            DepartmentCode = plan.DepartmentCode, DivisionCode = plan.DivisionCode,
                            ConfidentialityLevel = g + 1, LevelCode = LevelCodes[g],
                            CanView = plan.Permissions[g * 4], CanAdd = plan.Permissions[g * 4 + 1],
                            CanEdit = plan.Permissions[g * 4 + 2], CanApprove = plan.Permissions[g * 4 + 3]
                        }, uid);

                    if (plan.Status == AuthorityImportStatus.New) result.ConfigsCreated++; else result.ConfigsUpdated++;
                }

                if (preview.UpdateUserLevel)
                    foreach (var lp in preview.LevelPlans.Where(x => x.Status == AuthorityImportStatus.Updated))
                    {
                        int userId = lp.UserId;
                        string old = string.IsNullOrEmpty(lp.OldLevel) ? null : lp.OldLevel;
                        _users.UpdateLevelCode(userId, lp.NewLevel);
                        undo.Push(() => _users.UpdateLevelCode(userId, old));
                        result.LevelsChanged++;
                    }
            }
            catch (Exception ex)
            {
                int failedUndo = 0;
                while (undo.Count > 0)
                {
                    try { undo.Pop()(); } catch { failedUndo++; }
                }
                throw new InvalidOperationException(
                    "การนำเข้าล้มเหลวและพยายามย้อนกลับแล้ว"
                    + (failedUndo > 0 ? " (ย้อนกลับไม่สำเร็จ " + failedUndo + " ขั้นตอน กรุณาตรวจสอบข้อมูลด้วยตนเอง)" : "")
                    + ": " + ex.Message, ex);
            }

            WriteAudit(preview, result, user);
            return result;
        }

        private void Restore(string label, List<FolderAuthorityConfig> snapshot, int userId)
        {
            _authority.Delete(label, userId);
            foreach (var c in snapshot)
            {
                c.ConfigID = 0;
                _authority.Save(c, userId);
            }
        }

        private static FolderAuthorityConfig Clone(FolderAuthorityConfig c)
        {
            return new FolderAuthorityConfig
            {
                ConfigID = c.ConfigID, ConfigLabel = c.ConfigLabel, PositionName = c.PositionName,
                DepartmentCode = c.DepartmentCode, DivisionCode = c.DivisionCode,
                ConfidentialityLevel = c.ConfidentialityLevel, LevelCode = c.LevelCode,
                CanView = c.CanView, CanAdd = c.CanAdd, CanEdit = c.CanEdit, CanApprove = c.CanApprove
            };
        }

        private void WriteAudit(AuthorityImportPreview preview, AuthorityImportResult result, UserContext user)
        {
            string file = System.IO.Path.GetFileName(preview.FilePath ?? "");
            _audit.Log(NewEntry(user, "AuthorityImport", "FolderAuthorityConfig", null, null,
                string.Format("file={0}; sheet={1}; created={2}; updated={3}; levelChanges={4}",
                    file, preview.SheetName, result.ConfigsCreated, result.ConfigsUpdated, result.LevelsChanged)));

            if (!preview.UpdateUserLevel) return;
            foreach (var lp in preview.LevelPlans.Where(x => x.Status == AuthorityImportStatus.Updated))
                _audit.Log(NewEntry(user, "AuthorityImportUserLevel", "UserAccount", lp.UserId,
                    "emp=" + lp.EmployeeCode + "; LevelCode=" + lp.OldLevel, "LevelCode=" + lp.NewLevel));
        }

        private static AuditEntry NewEntry(UserContext user, string action, string table, int? recordId, string oldValue, string newValue)
        {
            return new AuditEntry
            {
                Module = "DMS", Action = action, UserID = user.UserID, UserName = user.ADUserName,
                UserRole = user.AppRole, TableName = table, RecordID = recordId,
                OldValue = oldValue, NewValue = newValue, Result = 1,
                IPAddress = user.IPAddress, MachineName = user.MachineName, SessionID = user.SessionID
            };
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private static void RequireAdmin(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole != AdminRole)
                throw new UnauthorizedAccessException("เฉพาะผู้ดูแลระบบ (Admin) เท่านั้นที่นำเข้าสิทธิ์ได้");
        }

        private static void Add(AuthorityImportPreview p, int row, AuthorityImportSeverity sev, string code, string msg)
        {
            p.Issues.Add(new AuthorityImportIssue { RowNumber = row, Severity = sev, Code = code, Message = msg });
        }
    }
}
