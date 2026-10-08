using System;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Presentation.Modules.DMS;
using CMS.Services.DTOs;

namespace CMS.Presentation.Services
{
    /// <summary>
    /// One download flow for every "save file" button (Folder, file detail panel, dashboard, shared-with-me):
    ///   ดาวน์โหลดได้ทันที / มีอนุมัติแล้ว → SaveFileDialog → DownloadPolicy.DownloadForSave
    ///   ต้องขออนุมัติ → DownloadApprovalRequestDialog
    /// Opening a file for viewing does not go through here.
    /// </summary>
    public static class FileDownloadGate
    {
        private const string Caption = "ดาวน์โหลด";

        /// <returns>true when the file was saved to disk.</returns>
        public static bool Download(
            IWin32Window owner,
            FileDocument file,
            UserContext user,
            IDownloadPolicyController downloadPolicyController,
            IApprovalFolderController approvalFolderController)
        {
            if (file == null || user == null) return false;

            if (downloadPolicyController == null)
            {
                MessageBox.Show(owner, "ระบบตรวจสิทธิ์ดาวน์โหลดไม่พร้อมใช้งาน กรุณาติดต่อผู้ดูแลระบบ",
                    Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            DownloadCheckResult check;
            try
            {
                check = downloadPolicyController.CheckDownload(file.FileID, user);
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, "ตรวจสอบสิทธิ์ดาวน์โหลดไม่สำเร็จ: " + ex.Message,
                    Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!check.CanDownloadNow)
            {
                if (check.PendingRequestId.HasValue)
                {
                    // มีคำขอรออนุมัติอยู่แล้ว → ไม่ส่งซ้ำ
                    MessageBox.Show(owner, string.Format(
                        "ไฟล์ '{0}' มีคำขอดาวน์โหลดรออนุมัติอยู่แล้ว (คำขอ ID {1})\r\n" +
                        "ติดตามสถานะได้ที่เมนู \"⬇ คำขอดาวน์โหลดของฉัน\"",
                        file.OriginalFileName, check.PendingRequestId.Value),
                        Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                RequestApproval(owner, file, user, approvalFolderController);
                return false;
            }

            return SaveToDisk(owner, file, user, downloadPolicyController);
        }

        private static void RequestApproval(IWin32Window owner, FileDocument file, UserContext user,
            IApprovalFolderController approvalFolderController)
        {
            if (approvalFolderController == null)
            {
                MessageBox.Show(owner, "ไฟล์นี้ต้องขออนุมัติก่อนดาวน์โหลด แต่ระบบขออนุมัติไม่พร้อมใช้งาน",
                    Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dlg = new DownloadApprovalRequestDialog(file, user, approvalFolderController))
            {
                if (dlg.ShowDialog(owner) == DialogResult.OK)
                {
                    MessageBox.Show(owner, string.Format(
                        "ส่งคำขอดาวน์โหลด '{0}' เรียบร้อยแล้ว\r\nกรุณารอการอนุมัติจากผู้อนุมัติ", file.OriginalFileName),
                        Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private static bool SaveToDisk(IWin32Window owner, FileDocument file, UserContext user,
            IDownloadPolicyController downloadPolicyController)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title    = "บันทึกไฟล์";
                sfd.FileName = file.OriginalFileName;
                string ext = (file.FileExtension ?? "").TrimStart('.').ToLower();
                sfd.Filter = string.IsNullOrEmpty(ext)
                    ? "All files (*.*)|*.*"
                    : string.Format("{0} files (*.{1})|*.{1}|All files (*.*)|*.*", ext.ToUpper(), ext);
                sfd.DefaultExt = ext;

                if (sfd.ShowDialog(owner) != DialogResult.OK) return false;

                try
                {
                    var result = downloadPolicyController.DownloadForSave(file.FileID, user);
                    if (result == null || !result.Success)
                    {
                        MessageBox.Show(owner, string.Format("ดาวน์โหลดไม่สำเร็จ: {0}",
                            result != null ? result.ErrorMessage : "ไม่ทราบสาเหตุ"),
                            Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    using (result.FileStream)
                    using (var fs = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    {
                        result.FileStream.CopyTo(fs);
                    }

                    MessageBox.Show(owner, "ดาวน์โหลดเรียบร้อย\r\n" + sfd.FileName,
                        Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(owner, ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(owner, "ดาวน์โหลดไม่สำเร็จ: " + ex.Message,
                        Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }
    }
}
