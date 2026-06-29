using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog สำหรับตั้งค่าสิทธิ์การเข้าถึงไฟล์ (File-level ACL)
    /// รองรับ PrincipalType: 1=User, 2=Role, 3=OrgUnit, 4=Group
    /// </summary>
    public partial class FilePermissionForm : Form
    {
        private readonly IFilePermissionCommandController  _filePermissionCommandController;
        private readonly IFilePermissionAclController      _filePermissionAclController;
        private readonly IUserAccountLookupController      _userAccountLookupController;
        private readonly IUserAccessOrgLookupController    _orgLookupController;
        private readonly UserContext               _currentUser;
        private readonly FileDocument              _file;

        private List<FilePermission>    _aclEntries     = new List<FilePermission>();
        private Dictionary<int, string> _userDisplayMap  = new Dictionary<int, string>();
        private Dictionary<int, string> _orgUnitNameMap  = new Dictionary<int, string>(); // OrgUnitID → Name

        public FilePermissionForm(
            IFilePermissionCommandController filePermissionCommandController,
            IFilePermissionAclController filePermissionAclController,
            UserContext               currentUser,
            FileDocument              file,
            IUserAccountLookupController userAccountLookupController = null,
            IUserAccessOrgLookupController orgLookupController = null)
        {
            if (filePermissionCommandController == null) throw new ArgumentNullException("filePermissionCommandController");
            if (filePermissionAclController == null) throw new ArgumentNullException("filePermissionAclController");
            if (currentUser              == null) throw new ArgumentNullException("currentUser");
            if (file                     == null) throw new ArgumentNullException("file");

            _filePermissionCommandController = filePermissionCommandController;
            _filePermissionAclController     = filePermissionAclController;
            _currentUser                     = currentUser;
            _file                            = file;
            _userAccountLookupController     = userAccountLookupController;
            _orgLookupController             = orgLookupController;

            InitializeComponent();

            _lblTitle.Text    = string.Format("🔒 ตั้งค่าสิทธิ์ไฟล์: {0}", _file.OriginalFileName);
            _lblFileInfo.Text = string.Format("ไฟล์: {0}  |  ประเภท: {1}  |  ขนาด: {2}",
                                    _file.OriginalFileName,
                                    (_file.FileExtension ?? "").TrimStart('.').ToUpper(),
                                    FormatBytes(_file.FileSizeBytes));

            if (_userAccountLookupController != null)
            {
                try
                {
                    foreach (var u in _userAccountLookupController.GetAllUsers())
                        _userDisplayMap[u.UserID] = string.IsNullOrEmpty(u.DisplayName)
                            ? u.ADUserName : u.DisplayName;
                }
                catch { }
            }

            // Pre-load OrgUnit names (Division/Department) keyed by OrgUnitID
            if (orgLookupController != null)
            {
                try
                {
                    foreach (var u in orgLookupController.GetHierarchy())
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
                _aclEntries = _filePermissionAclController.GetByFile(_file.FileID).ToList();
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
            using (var dlg = new FilePermissionAddDialog(_userAccountLookupController, _orgLookupController))
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
                    _filePermissionCommandController.SetFilePermission(_file, req, _currentUser);
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
                _filePermissionCommandController.RevokeFilePermission(_file, entry, _currentUser, "ลบโดยผู้ดูแล");
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
            if (_filePermissionCommandController == null)
            {
                MessageBox.Show("ไม่สามารถซิงค์ได้ — FilePermissionCommandController ไม่พร้อมใช้งาน",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int activeFolderPermissionCount = _filePermissionCommandController.GetActiveFolderPermissionCount(_file.FolderID);

            string msg = activeFolderPermissionCount == 0
                ? "Folder นี้ไม่มีสิทธิ์ที่ active อยู่\n\nต้องการล้างสิทธิ์ไฟล์ทั้งหมดหรือไม่?"
                : string.Format("ซิงค์สิทธิ์จาก Folder ({0} รายการ) มาแทนที่สิทธิ์ไฟล์ปัจจุบัน?\n\nสิทธิ์เดิมของไฟล์จะถูกยกเลิกทั้งหมด",
                    activeFolderPermissionCount);

            if (MessageBox.Show(msg, "ยืนยันการซิงค์",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                int syncedCount = _filePermissionCommandController.SyncFromFolderPermissions(_file, _aclEntries, _currentUser);
                LoadAcl();
                MessageBox.Show(string.Format("✅ ซิงค์สิทธิ์เรียบร้อยแล้ว ({0} รายการ)", syncedCount),
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
