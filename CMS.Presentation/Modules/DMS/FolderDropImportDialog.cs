using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog แสดงรายการไฟล์จากการลาก Folder มาวาง
    /// Options: สร้าง Folder เดิมใน CMS / สร้าง Sub-folder ตามโครงสร้างต้นฉบับ
    /// </summary>
    internal class FolderDropImportDialog : Form
    {
        // ── Result properties ────────────────────────────────────────────────────

        /// <summary>สร้าง Folder ชื่อเดียวกับ folder ที่ลากมาใน CMS ด้วย (root-level)</summary>
        public bool CreateRootFolder => _chkCreateRootFolder.Checked;

        /// <summary>สร้าง Sub-folder ตามโครงสร้างภายใน</summary>
        public bool CreateFolderStructure => _chkCreateStructure.Checked;

        /// <summary>ไฟล์แบบ flat รวมทุก source folder</summary>
        public string[] AllFiles { get; private set; } = new string[0];

        /// <summary>
        /// Outer key = source folder name (e.g. "Image_422"), or "" for direct-dropped files.
        /// Inner key = relative path within that source folder ("" = root of source folder).
        /// Value     = absolute file paths in that slot.
        /// </summary>
        public Dictionary<string, Dictionary<string, List<string>>> PerSourceFilesDict { get; private set; }
            = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase);

        // ── UI controls ──────────────────────────────────────────────────────────
        private ListView _listView;
        private CheckBox _chkCreateRootFolder;
        private CheckBox _chkCreateStructure;
        private Label    _lblSummary;
        private Button   _btnOk;
        private Button   _btnCancel;

        // Item tag struct
        private struct FileItem
        {
            public string AbsPath;
            public string SourceName;   // e.g. "Image_422", or "" for direct files
            public string RelPath;      // relative within source folder ("", "sub", "sub\nested")
        }

        // ── Colors ───────────────────────────────────────────────────────────────
        private static readonly Color _colorHeader = Color.FromArgb(0, 51, 102);
        private static readonly Color _colorRow1   = Color.White;
        private static readonly Color _colorRow2   = Color.FromArgb(245, 248, 252);

        // ── Constructor ──────────────────────────────────────────────────────────
        public FolderDropImportDialog(
            string[] droppedFolderPaths,
            string[] droppedDirectFiles,
            string[] extractedFiles,
            int      skippedCount)
        {
            droppedFolderPaths = droppedFolderPaths ?? new string[0];
            droppedDirectFiles = droppedDirectFiles ?? new string[0];
            extractedFiles     = extractedFiles     ?? new string[0];

            BuildData(droppedFolderPaths, droppedDirectFiles, extractedFiles);
            BuildUi(droppedFolderPaths, skippedCount);
            PopulateList(droppedFolderPaths);
        }

        // ── Data preparation ─────────────────────────────────────────────────────

        private void BuildData(string[] sourceFolders, string[] directFiles, string[] extractedFiles)
        {
            // Direct files (not from any folder) → keyed as ""
            if (directFiles.Length > 0)
            {
                PerSourceFilesDict[""] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    [""] = new List<string>(directFiles)
                };
            }

            foreach (var file in extractedFiles)
            {
                string sourceFolder = FindSourceFolder(sourceFolders, file);
                string sourceName   = string.IsNullOrEmpty(sourceFolder)
                    ? ""
                    : Path.GetFileName(sourceFolder.TrimEnd(
                        Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                string relPath = ComputeRelPath(sourceFolder, file);

                if (!PerSourceFilesDict.TryGetValue(sourceName, out var sourceDict))
                {
                    sourceDict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                    PerSourceFilesDict[sourceName] = sourceDict;
                }
                if (!sourceDict.TryGetValue(relPath, out var bucket))
                {
                    bucket = new List<string>();
                    sourceDict[relPath] = bucket;
                }
                bucket.Add(file);
            }

            AllFiles = directFiles.Concat(extractedFiles).Distinct().ToArray();
        }

        private static string FindSourceFolder(string[] sourceFolders, string filePath)
        {
            foreach (var folder in sourceFolders)
            {
                string sep = folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                           + Path.DirectorySeparatorChar;
                if (filePath.StartsWith(sep, StringComparison.OrdinalIgnoreCase))
                    return folder;
            }
            return "";
        }

        private static string ComputeRelPath(string sourceFolder, string filePath)
        {
            if (string.IsNullOrEmpty(sourceFolder)) return "";
            string sep = sourceFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
            if (!filePath.StartsWith(sep, StringComparison.OrdinalIgnoreCase)) return "";
            return Path.GetDirectoryName(filePath.Substring(sep.Length)) ?? "";
        }

        // ── UI building ──────────────────────────────────────────────────────────

        private void BuildUi(string[] sourceFolders, int skippedCount)
        {
            string folderNames = sourceFolders.Length > 0
                ? string.Join(", ", sourceFolders.Select(f => Path.GetFileName(f)))
                : "—";

            Text            = "ยืนยันการลาก Folder เข้าระบบ";
            Size            = new Size(700, 520);
            MinimumSize     = new Size(560, 420);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            Font            = new Font("Segoe UI", 9f);
            BackColor       = Color.White;

            // ── Header ──────────────────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = _colorHeader };
            var lblTitle  = new Label
            {
                Text      = "ลาก Folder: " + folderNames,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(14, 0, 0, 0)
            };
            pnlHeader.Controls.Add(lblTitle);

            // ── Footer ──────────────────────────────────────────────────────────
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.FromArgb(245, 245, 245) };
            pnlFooter.Paint += (s, e) =>
                e.Graphics.DrawLine(Pens.LightGray, 0, 0, ((Panel)s).Width, 0);

            _btnOk = new Button
            {
                Text         = "OK — อัปโหลด",
                Width        = 120,
                Height       = 28,
                Top          = 8,
                DialogResult = DialogResult.OK,
                BackColor    = Color.FromArgb(0, 120, 215),
                ForeColor    = Color.White,
                FlatStyle    = FlatStyle.Flat
            };
            _btnOk.FlatAppearance.BorderSize = 0;

            _btnCancel = new Button
            {
                Text         = "ยกเลิก",
                Width        = 80,
                Height       = 28,
                Top          = 8,
                DialogResult = DialogResult.Cancel,
                BackColor    = Color.FromArgb(240, 240, 240),
                FlatStyle    = FlatStyle.Flat
            };
            _btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);

            pnlFooter.Resize += (s, e) =>
            {
                _btnCancel.Left = pnlFooter.Width - _btnCancel.Width - 12;
                _btnOk.Left     = _btnCancel.Left - _btnOk.Width - 8;
            };
            pnlFooter.Controls.AddRange(new Control[] { _btnOk, _btnCancel });

            // ── Options panel (2 checkboxes) ─────────────────────────────────────
            var pnlOptions = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = Color.White };
            pnlOptions.Paint += (s, e) =>
                e.Graphics.DrawLine(Pens.LightGray, 0, 0, ((Panel)s).Width, 0);

            _chkCreateRootFolder = new CheckBox
            {
                Text    = string.Format("สร้าง Folder \"{0}\" ใน CMS ด้วย", folderNames),
                AutoSize = true,
                Top     = 8,
                Left    = 14,
                Checked = sourceFolders.Length > 0,
                Font    = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            _chkCreateRootFolder.CheckedChanged += (s, e) => RefreshFolderColumn();

            _chkCreateStructure = new CheckBox
            {
                Text    = "สร้าง Sub-folder ตามโครงสร้างต้นฉบับ",
                AutoSize = true,
                Top     = 32,
                Left    = 28,
                Font    = new Font("Segoe UI", 9f)
            };
            _chkCreateStructure.CheckedChanged += (s, e) => RefreshFolderColumn();
            pnlOptions.Controls.AddRange(new Control[] { _chkCreateRootFolder, _chkCreateStructure });

            // ── Summary label ────────────────────────────────────────────────────
            long totalBytes = AllFiles.Sum(f => { try { return new FileInfo(f).Length; } catch { return 0L; } });
            string sizeText = totalBytes >= 1024 * 1024
                ? string.Format("{0:F1} MB", totalBytes / (1024.0 * 1024.0))
                : string.Format("{0} KB", totalBytes / 1024);
            string skipNote = skippedCount > 0
                ? string.Format("  (ข้ามไฟล์ที่ไม่รองรับ {0} รายการ)", skippedCount)
                : "";

            _lblSummary = new Label
            {
                Text      = string.Format("พบไฟล์ที่รองรับ {0} รายการ รวม {1}{2}", AllFiles.Length, sizeText, skipNote),
                Dock      = DockStyle.Bottom,
                Height    = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(14, 0, 0, 0),
                ForeColor = Color.FromArgb(80, 80, 80),
                Font      = new Font("Segoe UI", 8.5f)
            };

            // ── ListView ─────────────────────────────────────────────────────────
            _listView = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                MultiSelect   = false,
                Font          = new Font("Segoe UI", 9f)
            };
            _listView.Columns.Add("ชื่อไฟล์",  280);
            _listView.Columns.Add("CMS Folder", 220);
            _listView.Columns.Add("ขนาด",        80);
            _listView.Columns.Add("ประเภท",       60);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;

            Controls.AddRange(new Control[]
            {
                _listView, _lblSummary, pnlOptions, pnlFooter, pnlHeader
            });

            // Initial column state
            RefreshFolderColumn();
        }

        // ── List population ──────────────────────────────────────────────────────

        private void PopulateList(string[] sourceFolders)
        {
            _listView.Items.Clear();
            int row = 0;

            // Direct files first
            if (PerSourceFilesDict.TryGetValue("", out var directDict))
            {
                foreach (var kv in directDict)
                    foreach (var file in kv.Value)
                        AddListItem(file, new FileItem { AbsPath = file, SourceName = "", RelPath = kv.Key }, row++);
            }

            // Files from each source folder
            foreach (var sf in sourceFolders)
            {
                string sfName = Path.GetFileName(
                    sf.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                if (!PerSourceFilesDict.TryGetValue(sfName, out var sfDict)) continue;

                foreach (var kv in sfDict.OrderBy(k => k.Key))
                    foreach (var file in kv.Value)
                        AddListItem(file, new FileItem { AbsPath = file, SourceName = sfName, RelPath = kv.Key }, row++);
            }
        }

        private void AddListItem(string file, FileItem fi, int row)
        {
            long size = 0;
            try { size = new FileInfo(file).Length; } catch { }
            string sizeStr = size >= 1024 * 1024
                ? string.Format("{0:F1} MB", size / (1024.0 * 1024.0))
                : string.Format("{0} KB", size / 1024);

            var item = new ListViewItem(Path.GetFileName(file));
            item.SubItems.Add(BuildFolderColumnText(fi));
            item.SubItems.Add(sizeStr);
            item.SubItems.Add(Path.GetExtension(file).TrimStart('.').ToUpperInvariant());
            item.BackColor = (row % 2 == 0) ? _colorRow1 : _colorRow2;
            item.Tag       = fi;
            _listView.Items.Add(item);
        }

        private string BuildFolderColumnText(FileItem fi)
        {
            bool createRoot = _chkCreateRootFolder != null && _chkCreateRootFolder.Checked;
            bool createSub  = _chkCreateStructure  != null && _chkCreateStructure.Checked;

            if (!createRoot && !createSub) return "(current folder)";

            var parts = new List<string>();
            if (createRoot && !string.IsNullOrEmpty(fi.SourceName))
                parts.Add(fi.SourceName);
            if (createSub && !string.IsNullOrEmpty(fi.RelPath))
                parts.Add(fi.RelPath);

            return parts.Count > 0
                ? string.Join(@"\", parts)
                : (!string.IsNullOrEmpty(fi.SourceName) ? fi.SourceName : "(current folder)");
        }

        private void RefreshFolderColumn()
        {
            bool show = (_chkCreateRootFolder != null && _chkCreateRootFolder.Checked)
                     || (_chkCreateStructure  != null && _chkCreateStructure.Checked);
            _listView.Columns[1].Width = show ? 220 : 0;

            // Refresh column text for each row
            foreach (ListViewItem item in _listView.Items)
            {
                if (item.Tag is FileItem fi)
                    item.SubItems[1].Text = BuildFolderColumnText(fi);
            }
        }
    }
}
