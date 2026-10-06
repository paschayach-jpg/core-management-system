using System;
using System.Drawing;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Presentation.UserControls;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// เลือก (และสร้าง) โฟลเดอร์ปลายทางใน DMS สำหรับไฟล์แนบที่นำเข้าจาก KB Import Wizard.
    /// เลือก Level 1-4 → เลือก/สร้างโฟลเดอร์ใน tree → ตรวจสิทธิ์อัปโหลดทันทีที่เลือก; ปุ่ม OK กดได้เมื่อผ่านเท่านั้น
    /// </summary>
    public partial class KBDmsFolderPickerDialog : Form
    {
        private static readonly string[] LevelCodes = { "1HC", "2SC", "3NC", "4PC" };

        private readonly IFolderReadLookupController _folderReadLookup;
        private readonly IFolderCommandController    _folderCommand;
        private readonly UserContext                 _user;
        private readonly Func<int, string>           _checkFolder;
        private readonly int                         _defaultLevel;

        /// <summary>FolderID ที่เลือก (ผ่านการตรวจสิทธิ์แล้ว) เมื่อปิดด้วย <see cref="DialogResult.OK"/></summary>
        public int SelectedFolderId { get; private set; }

        /// <summary>ข้อความแสดงปลายทาง เช่น "4PC › ชื่อโฟลเดอร์"</summary>
        public string SelectedFolderPath { get; private set; }

        /// <param name="checkFolder">คืนข้อความไทยเมื่อผู้ใช้อัปโหลดลงโฟลเดอร์นั้นไม่ได้ และ null เมื่อผ่าน</param>
        public KBDmsFolderPickerDialog(
            IFolderReadLookupController folderReadLookup,
            IFolderCommandController folderCommand,
            UserContext user,
            Func<int, string> checkFolder,
            int defaultLevel = 4)
        {
            if (folderReadLookup == null) throw new ArgumentNullException("folderReadLookup");
            if (folderCommand    == null) throw new ArgumentNullException("folderCommand");
            if (user             == null) throw new ArgumentNullException("user");
            if (checkFolder      == null) throw new ArgumentNullException("checkFolder");

            _folderReadLookup = folderReadLookup;
            _folderCommand    = folderCommand;
            _user             = user;
            _checkFolder      = checkFolder;
            _defaultLevel     = (defaultLevel >= 1 && defaultLevel <= 4) ? defaultLevel : 4;

            InitializeComponent();
        }

        private void KBDmsFolderPickerDialog_Load(object sender, EventArgs e)
        {
            // ไม่ส่ง file controller — dialog นี้เลือกโฟลเดอร์เท่านั้น ไม่แสดง/ลบไฟล์
            folderExplorer.Initialize(_folderReadLookup, _folderCommand, null, null, _user);
            folderExplorer.SelectedFolderChanged += (s, ev) => UpdateSelectionState();

            cboLevel.Items.Clear();
            cboLevel.Items.Add("Level 1 - Highly Confidential (HC)");
            cboLevel.Items.Add("Level 2 - Secret (SC)");
            cboLevel.Items.Add("Level 3 - Non-Confidential (NC)");
            cboLevel.Items.Add("Level 4 - Public/Common (PC)");
            cboLevel.SelectedIndex = _defaultLevel - 1;
        }

        private void CboLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            int level = cboLevel.SelectedIndex + 1;
            folderExplorer.LoadRoot(FolderExplorerControl.ResolveLevelFolderId(level), level);
            UpdateSelectionState();
        }

        /// <summary>ตรวจสิทธิ์โฟลเดอร์ที่เลือกอยู่ทันที — ไม่ผ่านแสดงข้อความสีแดงและปิดปุ่ม OK</summary>
        private void UpdateSelectionState()
        {
            int? folderId = folderExplorer.SelectedFolderId;
            if (!folderId.HasValue)
            {
                btnOk.Enabled    = false;
                lblStatus.Text   = "กรุณาเลือกโฟลเดอร์ปลายทาง";
                lblStatus.ForeColor = Color.FromArgb(120, 120, 120);
                return;
            }

            string error = _checkFolder(folderId.Value);
            btnOk.Enabled = error == null;
            if (error == null)
            {
                lblStatus.Text      = string.Format("พร้อมใช้งาน: {0}", folderExplorer.SelectedFolderName);
                lblStatus.ForeColor = Color.FromArgb(39, 174, 96);
            }
            else
            {
                lblStatus.Text      = error;
                lblStatus.ForeColor = Color.FromArgb(192, 57, 43);
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            int? folderId = folderExplorer.SelectedFolderId;
            if (!folderId.HasValue)
            {
                MessageBox.Show("กรุณาเลือกโฟลเดอร์ปลายทาง", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ตรวจซ้ำก่อนปิด — สิทธิ์อาจเปลี่ยนระหว่างที่ dialog เปิดอยู่
            string error = _checkFolder(folderId.Value);
            if (error != null)
            {
                MessageBox.Show(error, "ไม่มีสิทธิ์ใช้โฟลเดอร์นี้",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateSelectionState();
                return;
            }

            SelectedFolderId   = folderId.Value;
            SelectedFolderPath = string.Format("{0} › {1}",
                LevelCodes[cboLevel.SelectedIndex], folderExplorer.SelectedFolderName);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
