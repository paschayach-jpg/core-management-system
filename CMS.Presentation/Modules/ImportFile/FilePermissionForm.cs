using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog สำหรับตั้งค่าสิทธิ์การเข้าถึงไฟล์ (File-level ACL)
    /// รองรับ PrincipalType: 1=User, 2=Role, 3=OrgUnit, 4=Group
    /// </summary>
    public partial class FilePermissionForm : Form
    {
        private readonly IFilePermissionService    _filePermissionService;
        private readonly IFilePermissionRepository _filePermissionRepository;
        private readonly IUserAccountRepository    _userAccountRepository;
        private readonly IPermissionRepository     _permissionRepository;
        private readonly UserContext               _currentUser;
        private readonly FileDocument              _file;
        private readonly IAuditService             _auditService;

        private List<FilePermission>    _aclEntries     = new List<FilePermission>();
        private Dictionary<int, string> _userDisplayMap  = new Dictionary<int, string>();
        private Dictionary<int, string> _orgUnitNameMap  = new Dictionary<int, string>(); // OrgUnitID → Name

        public FilePermissionForm(
            IFilePermissionService    filePermissionService,
            IFilePermissionRepository filePermissionRepository,
            UserContext               currentUser,
            FileDocument              file,
            IUserAccountRepository    userAccountRepository = null,
            IPermissionRepository     permissionRepository  = null,
            CMS.Services.Interfaces.IOrgStructureService orgStructureService = null,
            IAuditService             auditService          = null)
        {
            if (filePermissionService    == null) throw new ArgumentNullException("filePermissionService");
            if (filePermissionRepository == null) throw new ArgumentNullException("filePermissionRepository");
            if (currentUser              == null) throw new ArgumentNullException("currentUser");
            if (file                     == null) throw new ArgumentNullException("file");

            _filePermissionService    = filePermissionService;
            _filePermissionRepository = filePermissionRepository;
            _currentUser              = currentUser;
            _file                     = file;
            _userAccountRepository    = userAccountRepository;
            _permissionRepository     = permissionRepository;
            _auditService             = auditService;

            InitializeComponent();

            _lblTitle.Text    = string.Format("🔒 ตั้งค่าสิทธิ์ไฟล์: {0}", _file.OriginalFileName);
            _lblFileInfo.Text = string.Format("ไฟล์: {0}  |  ประเภท: {1}  |  ขนาด: {2}",
                                    _file.OriginalFileName,
                                    (_file.FileExtension ?? "").TrimStart('.').ToUpper(),
                                    FormatBytes(_file.FileSizeBytes));

            if (_userAccountRepository != null)
            {
                try
                {
                    foreach (var u in _userAccountRepository.GetAll())
                        _userDisplayMap[u.UserID] = string.IsNullOrEmpty(u.DisplayName)
                            ? u.ADUserName : u.DisplayName;
                }
                catch { }
            }

            // Pre-load OrgUnit names (Division/Department) keyed by OrgUnitID
            if (orgStructureService != null)
            {
                try
                {
                    foreach (var u in orgStructureService.GetHierarchy())
                        _orgUnitNameMap[u.OrgUnitID] = u.OrgUnitNameTH ?? u.OrgUnitNameEN ?? u.OrgUnitCode ?? u.OrgUnitID.ToString();
                }
                catch { }
            }
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private void FilePermissionForm_Load(object sender, EventArgs e)
        {
            LoadAcl();
        }

        // ── Load ACL ──────────────────────────────────────────────────────────

        private void LoadAcl()
        {
            try
            {
                _aclEntries = _filePermissionRepository.GetByFile(_file.FileID).ToList();
                RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลดรายการสิทธิ์ได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshList()
        {
            _lstAcl.Items.Clear();
            foreach (var e in _aclEntries)
            {
                string principalName = ResolvePrincipalName(e.PrincipalType, e.PrincipalID);
                var item = new ListViewItem(PrincipalTypeLabel(e.PrincipalType));
                item.SubItems.Add(principalName);
                item.SubItems.Add(e.CanRead        ? "✔" : "");
                item.SubItems.Add(e.CanWrite       ? "✔" : "");
                item.SubItems.Add(e.CanDelete      ? "✔" : "");
                item.SubItems.Add(e.CanModify      ? "✔" : "");
                item.SubItems.Add(e.CanFullControl ? "✔" : "");
                item.SubItems.Add(e.IsDeny         ? "✘" : "");
                item.SubItems.Add(e.ExpiryDate.HasValue
                    ? e.ExpiryDate.Value.ToString("dd/MM/yyyy") : "—");
                item.SubItems.Add(ResolveUserName(e.GrantedBy));
                item.Tag = e;
                if (e.IsDeny) item.ForeColor = Color.FromArgb(180, 50, 50);
                _lstAcl.Items.Add(item);
            }
        }

        // ── Add permission ────────────────────────────────────────────────────

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dlg = new FilePermissionAddDialog(_userAccountRepository))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    var req = new SetFilePermissionRequest
                    {
                        FileID         = _file.FileID,
                        PrincipalType  = dlg.SelectedPrincipalType,
                        PrincipalID    = dlg.SelectedPrincipalId,
                        CanRead        = dlg.CanRead,
                        CanWrite       = dlg.CanWrite,
                        CanDelete      = dlg.CanDelete,
                        CanModify      = dlg.CanModify,
                        CanFullControl = dlg.CanFullControl,
                        IsDeny         = dlg.IsDeny
                    };
                    _filePermissionService.SetFilePermission(req, _currentUser);
                    if (_auditService != null)
                        _auditService.Log(new AuditEntry
                        {
                            Module      = "FilePermission",
                            Action      = "SET_FILE_PERMISSION",
                            UserID      = _currentUser.UserID,
                            TableName   = "FilePermission",
                            RecordID    = _file.FileID,
                            NewValue    = string.Format("File:{0} PrincipalType:{1} PrincipalID:{2} IsDeny:{3}",
                                             _file.OriginalFileName, req.PrincipalType, req.PrincipalID, req.IsDeny),
                            MachineName = _currentUser.MachineName,
                            IPAddress   = _currentUser.IPAddress,
                            SessionID   = _currentUser.SessionID,
                            Result      = 1,
                            AuditDate   = DateTime.Now
                        });
                    LoadAcl();
                    MessageBox.Show("✅ บันทึกสิทธิ์เรียบร้อยแล้ว", "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("ไม่มีสิทธิ์ดำเนินการ: " + ex.Message, "ข้อผิดพลาด",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ไม่สามารถบันทึกสิทธิ์ได้\n\n" + ex.Message, "ข้อผิดพลาด",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Remove permission ─────────────────────────────────────────────────

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (_lstAcl.SelectedItems.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการสิทธิ์ที่ต้องการลบ", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var entry = _lstAcl.SelectedItems[0].Tag as FilePermission;
            if (entry == null) return;

            if (MessageBox.Show(
                    string.Format("ยืนยันการลบสิทธิ์ของ {0}?",
                        ResolvePrincipalName(entry.PrincipalType, entry.PrincipalID)),
                    "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                _filePermissionService.RevokeFilePermission(
                    entry.FilePermissionID, _currentUser, "ลบโดยผู้ดูแล");
                if (_auditService != null)
                    _auditService.Log(new AuditEntry
                    {
                        Module      = "FilePermission",
                        Action      = "REVOKE_FILE_PERMISSION",
                        UserID      = _currentUser.UserID,
                        TableName   = "FilePermission",
                        RecordID    = _file.FileID,
                        OldValue    = string.Format("File:{0} FilePermissionID:{1}",
                                         _file.OriginalFileName, entry.FilePermissionID),
                        MachineName = _currentUser.MachineName,
                        IPAddress   = _currentUser.IPAddress,
                        SessionID   = _currentUser.SessionID,
                        Result      = 1,
                        AuditDate   = DateTime.Now
                    });
                LoadAcl();
                MessageBox.Show("ลบรายการสิทธิ์เรียบร้อยแล้ว", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("ไม่มีสิทธิ์ดำเนินการ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถลบสิทธิ์ได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Sync from folder ──────────────────────────────────────────────────

        private void BtnSyncFolder_Click(object sender, EventArgs e)
        {
            if (_permissionRepository == null)
            {
                MessageBox.Show("ไม่สามารถซิงค์ได้ — PermissionRepository ไม่พร้อมใช้งาน",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var folderPerms = _permissionRepository.GetByFolder(_file.FolderID)
                .Where(p => p.RevokedDate == null
                         && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < System.DateTime.Today))
                .ToList();

            string msg = folderPerms.Count == 0
                ? "Folder นี้ไม่มีสิทธิ์ที่ active อยู่\n\nต้องการล้างสิทธิ์ไฟล์ทั้งหมดหรือไม่?"
                : string.Format("ซิงค์สิทธิ์จาก Folder ({0} รายการ) มาแทนที่สิทธิ์ไฟล์ปัจจุบัน?\n\nสิทธิ์เดิมของไฟล์จะถูกยกเลิกทั้งหมด",
                    folderPerms.Count);

            if (MessageBox.Show(msg, "ยืนยันการซิงค์",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                // Revoke all existing file permissions
                foreach (var fp in _aclEntries)
                    _filePermissionRepository.Revoke(fp.FilePermissionID, _currentUser.UserID, "ซิงค์จาก Folder");

                // Insert folder permissions as file permissions
                foreach (var fp in folderPerms)
                {
                    _filePermissionRepository.Insert(new FilePermission
                    {
                        FileID         = _file.FileID,
                        PrincipalType  = (int)fp.PrincipalType,
                        PrincipalID    = fp.PrincipalID,
                        CanRead        = fp.CanRead,
                        CanWrite       = fp.CanWrite,
                        CanDelete      = fp.CanDelete,
                        CanModify      = fp.CanModify,
                        CanFullControl = fp.CanFullControl,
                        IsDeny         = fp.IsDeny,
                        ExpiryDate     = fp.ExpiryDate,
                        GrantedBy      = _currentUser.UserID,
                        GrantedDate    = System.DateTime.Now
                    });
                }

                if (_auditService != null)
                    _auditService.Log(new AuditEntry
                    {
                        Module      = "FilePermission",
                        Action      = "SYNC_FILE_PERMISSION",
                        UserID      = _currentUser.UserID,
                        TableName   = "FilePermission",
                        RecordID    = _file.FileID,
                        NewValue    = string.Format("File:{0} SyncedFromFolder:{1} Count:{2}",
                                         _file.OriginalFileName, _file.FolderID, folderPerms.Count),
                        MachineName = _currentUser.MachineName,
                        IPAddress   = _currentUser.IPAddress,
                        SessionID   = _currentUser.SessionID,
                        Result      = 1,
                        AuditDate   = DateTime.Now
                    });
                LoadAcl();
                MessageBox.Show(string.Format("✅ ซิงค์สิทธิ์เรียบร้อยแล้ว ({0} รายการ)", folderPerms.Count),
                    "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถซิงค์สิทธิ์ได้\n\n" + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private string PrincipalTypeLabel(int type)
        {
            switch (type)
            {
                case 1: return "ผู้ใช้";
                case 2: return "Role";
                case 3: return "แผนก";
                case 4: return "ฝ่าย";
                default: return type.ToString();
            }
        }

        private string ResolvePrincipalName(int type, int id)
        {
            switch (type)
            {
                case 1: // User
                    string userName;
                    return _userDisplayMap.TryGetValue(id, out userName) ? userName : string.Format("UserID: {0}", id);
                case 3: // Department (OrgUnitID)
                case 4: // Division (OrgUnitID)
                    string orgName;
                    return _orgUnitNameMap.TryGetValue(id, out orgName) ? orgName : string.Format("OrgUnitID: {0}", id);
                default:
                    return string.Format("ID: {0}", id);
            }
        }

        private string ResolveUserName(int userId)
        {
            string name;
            return _userDisplayMap.TryGetValue(userId, out name) ? name : userId.ToString();
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024 * 1024) return string.Format("{0:0.0} MB", bytes / (1024.0 * 1024));
            if (bytes >= 1024)        return string.Format("{0:0.0} KB", bytes / 1024.0);
            return bytes + " B";
        }
    }
}
