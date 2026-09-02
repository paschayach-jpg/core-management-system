using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Infrastructure.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Browse the DMS folder tree and pick one or more existing files to reference
    /// (e.g. as KB article attachments). Returns the chosen <see cref="FileDocument"/>s
    /// via <see cref="SelectedFiles"/> when closed with <see cref="DialogResult.OK"/>.
    /// </summary>
    public partial class FrmDmsFilePicker : Form
    {
        private readonly IFolderReadLookupController _folderReadLookup;
        private readonly IFileBrowseController _fileBrowse;
        private readonly INetworkDriveHelper _networkDrive;
        private readonly UserContext _user;
        private readonly int _defaultLevel;

        public IList<FileDocument> SelectedFiles { get; private set; } = new List<FileDocument>();

        public FrmDmsFilePicker(
            IFolderReadLookupController folderReadLookup,
            IFileBrowseController fileBrowse,
            UserContext user,
            int defaultLevel = 4,
            INetworkDriveHelper networkDrive = null)
        {
            _folderReadLookup = folderReadLookup ?? throw new ArgumentNullException(nameof(folderReadLookup));
            _fileBrowse = fileBrowse ?? throw new ArgumentNullException(nameof(fileBrowse));
            _networkDrive = networkDrive;
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _defaultLevel = (defaultLevel >= 1 && defaultLevel <= 4) ? defaultLevel : 4;

            InitializeComponent();
        }

        private void FrmDmsFilePicker_Load(object sender, EventArgs e)
        {
            folderExplorer.FilePickMode = true;
            folderExplorer.Initialize(_folderReadLookup, null, _fileBrowse, null, _user, _networkDrive);
            folderExplorer.FileSelectionChanged += (s, ev) => UpdateOkState();
            folderExplorer.FilesDoubleClicked += (s, ev) =>
            {
                if (folderExplorer.GetSelectedFiles().Count > 0)
                    BtnOk_Click(s, ev);
            };

            cboLevel.Items.Clear();
            cboLevel.Items.Add("Level 1 - Highly Confidential (HC)");
            cboLevel.Items.Add("Level 2 - Secret (SC)");
            cboLevel.Items.Add("Level 3 - Non-Confidential (NC)");
            cboLevel.Items.Add("Level 4 - Public/Common (PC)");
            cboLevel.SelectedIndex = _defaultLevel - 1;

            UpdateOkState();
        }

        private void CboLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            int level = cboLevel.SelectedIndex + 1;
            folderExplorer.LoadRoot(
                CMS.Presentation.UserControls.FolderExplorerControl.ResolveLevelFolderId(level), level);
            UpdateOkState();
        }

        private void UpdateOkState()
        {
            btnOk.Enabled = folderExplorer.GetSelectedFiles().Count > 0;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var files = folderExplorer.GetSelectedFiles();
            if (files.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกไฟล์อย่างน้อย 1 ไฟล์", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SelectedFiles = files.ToList();
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
