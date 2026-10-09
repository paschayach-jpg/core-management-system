using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// Presenter for MOD-003 File Upload.
    /// Mediates between IFileUploadView and IFileService / IFolderService.
    /// Satisfies Requirements 5.1–5.16, 12.5.
    /// </summary>
    public class FilePresenter
    {
        private readonly IFileService    _fileService;
        private readonly IFolderService  _folderService;
        private readonly IFileUploadView _view;
        private readonly UserContext     _user;

        // Quota warning threshold (90 %)
        private const double QuotaWarningThreshold = 0.90;

        public FilePresenter(
            IFileService    fileService,
            IFolderService  folderService,
            IFileUploadView view,
            UserContext     user)
        {
            if (fileService   == null) throw new ArgumentNullException("fileService");
            if (folderService == null) throw new ArgumentNullException("folderService");
            if (view          == null) throw new ArgumentNullException("view");
            if (user          == null) throw new ArgumentNullException("user");

            _fileService   = fileService;
            _folderService = folderService;
            _view          = view;
            _user          = user;
        }

        // -----------------------------------------------------------------------
        // Folder Tree Loading
        // -----------------------------------------------------------------------

        public void LoadFolderTree(TreeView treeView, int orgUnitId)
        {
            if (treeView == null) throw new ArgumentNullException("treeView");

            try
            {
                treeView.BeginUpdate();
                treeView.Nodes.Clear();

                var rootFolders = _folderService.GetRootFolders(orgUnitId, _user);
                if (rootFolders == null) return;

                foreach (var folder in rootFolders.OrderBy(f => f.FolderName))
                {
                    var node = CreateFolderNode(folder);
                    treeView.Nodes.Add(node);
                    LoadSubFolders(node, folder.FolderID);
                }

                treeView.ExpandAll();
                treeView.EndUpdate();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load folder tree: " + ex.Message, ex);
            }
        }

        private void LoadSubFolders(TreeNode parentNode, int parentFolderId)
        {
            try
            {
                var subFolders = _folderService.GetFolderContents(parentFolderId, _user);
                if (subFolders == null) return;

                foreach (var folder in subFolders.OrderBy(f => f.FolderName))
                {
                    var node = CreateFolderNode(folder);
                    parentNode.Nodes.Add(node);
                    LoadSubFolders(node, folder.FolderID);
                }
            }
            catch
            {
                // Silently skip if unable to load subfolders
            }
        }

        private static TreeNode CreateFolderNode(Folder folder)
        {
            string label = string.Format("📁 {0}", folder.FolderName);
            return new TreeNode(label)
            {
                Tag = folder,
                ForeColor = folder.IsArchived ? System.Drawing.Color.Gray : System.Drawing.SystemColors.WindowText
            };
        }

        // -----------------------------------------------------------------------
        // Upload  (Req 5.1–5.16, 12.5)
        // -----------------------------------------------------------------------

        public void UploadFiles(IList<FileUploadRequest> files, int folderId)
        {
            if (files == null || files.Count == 0)
            {
                _view.ShowError("No files selected for upload.");
                return;
            }

            _view.SetUploadEnabled(false);
            _view.SetProgress(0);
            _view.HideQuotaWarning();

            IEnumerable<UploadResult> results;
            try
            {
                results = _fileService.UploadFiles(files, folderId, _user);
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError(ex.Message);
                _view.SetUploadEnabled(true);
                return;
            }
            catch (Exception ex)
            {
                _view.ShowError("เกิดข้อผิดพลาดในการอัปโหลด: " + ex.Message);
                _view.SetUploadEnabled(true);
                return;
            }

            var resultList = results == null ? new List<UploadResult>() : results.ToList();

            for (int i = 0; i < resultList.Count; i++)
            {
                var r = resultList[i];
                _view.SetFileResult(i, r);

                int progress = (int)Math.Round((i + 1) * 100.0 / resultList.Count);
                _view.SetProgress(progress);
            }

            _view.SetProgress(100);
            _view.SetUploadEnabled(true);

            CheckQuotaWarning(folderId);
        }

        // -----------------------------------------------------------------------
        // Download  (Req 5.14, 5.15)
        // -----------------------------------------------------------------------

        public void DownloadFile(int fileDocumentId, Func<string, string> getSavePath)
        {
            FileDownloadResult result;
            try
            {
                result = _fileService.DownloadFile(fileDocumentId, _user);
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                _view.ShowError("เกิดข้อผิดพลาดในการดาวน์โหลด: " + ex.Message);
                return;
            }

            if (!result.Success)
            {
                _view.ShowError(result.ErrorMessage);
                return;
            }

            string savePath = getSavePath(result.FileName);
            if (string.IsNullOrEmpty(savePath))
                return;

            try
            {
                using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                {
                    result.FileStream.CopyTo(fs);
                }
                _view.ShowInfo(string.Format("File saved to: {0}", savePath));
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to save file: " + ex.Message);
            }
            finally
            {
                if (result.FileStream != null)
                    result.FileStream.Dispose();
            }
        }

        // -----------------------------------------------------------------------
        // Quota warning  (Req 12.5)
        // -----------------------------------------------------------------------

        private void CheckQuotaWarning(int folderId)
        {
            try
            {
                var report = _fileService.GetStorageReport(0, _user);
                if (report == null || report.Folders == null) return;

                foreach (var entry in report.Folders)
                {
                    if (entry.FolderId == folderId
                        && entry.QuotaBytes.HasValue
                        && entry.QuotaBytes.Value > 0)
                    {
                        double pct = (double)entry.CurrentUsageBytes / entry.QuotaBytes.Value;
                        if (pct >= QuotaWarningThreshold)
                        {
                            _view.ShowQuotaWarning(string.Format(
                                "Warning: Folder storage is at {0:P0} of quota ({1:N0} / {2:N0} bytes).",
                                pct,
                                entry.CurrentUsageBytes,
                                entry.QuotaBytes.Value));
                            return;
                        }
                    }
                }

                _view.HideQuotaWarning();
            }
            catch
            {
                // Quota check is advisory — swallow errors so upload result is still shown
            }
        }
    }
}
