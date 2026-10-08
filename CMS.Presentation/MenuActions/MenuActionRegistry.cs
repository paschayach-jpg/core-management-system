using CMS.Presentation.Modules.ImportFile;
using CMS.Presentation.Modules.KMS;
using CMS.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    internal static class MenuActionRegistry
    {
        public static bool TryExecute(string configuredActionKey, MenuActionContext context, out string errorMessage)
        {
            errorMessage = null;

            if (context == null)
            {
                errorMessage = "Menu action context is not available.";
                return false;
            }

            string actionKey = MenuActionKeyCatalog.ResolveConfiguredActionKey(configuredActionKey) ?? configuredActionKey;
            if (string.IsNullOrWhiteSpace(actionKey))
                return false;

            try
            {
                switch (actionKey)
                {
                    case "Home":
                        if (context.SwitchToOrgView != null)
                            context.SwitchToOrgView();
                        return true;

                    case "Admin":
                    case "DocumentGroup":
                    case "OrgGroup":
                    case "PermissionGroup":
                    case "ReportGroup":
                    case "SystemGroup":
                        if (context.SwitchToMainMenuView != null)
                            context.SwitchToMainMenuView();
                        return true;

                    case "ShareDialog":
                        return ExecuteShareDialog(context);

                    case "SharedWithMe":
                        return ShowTargetForm(context, new SharedWithMeForm(context.User, context.ShareLookupController));

                    case "ShareManagement":
                        if (context.ShareManagementController == null)
                        {
                            MessageBox.Show("Share management controller is not available.", "Manage Shares", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return true;
                        }
                        return ShowTargetForm(context, new ShareManagementForm(context.ShareManagementController, context.User, context.ShareLookupController));

                    case "OrgStructure":
                        if (context.OrgStructureService == null)
                        {
                            if (!TryRequestMdiAction(context, "OrgStructure"))
                                MessageBox.Show("Org structure service is not available in this context.", "Org Structure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return true;
                        }
                        return ShowTargetForm(context, new OrgStructureForm(context.OrgStructureService, context.User, null));

                    case "UserManagement":
                        return ShowTargetForm(context, new UserManagementForm(context.UserAdminController, context.User, context.AuditService, context.LdapUserSearchService));

                    case "UserAccess":
                        if (context.OrgLookupController == null && context.OrgStructureService == null)
                        {
                            MessageBox.Show("Org lookup is not available.", "User Access", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return true;
                        }
                        return ShowTargetForm(context, new UserAccessForm(
                            context.UserAdminController,
                            context.OrgLookupController ?? new UserAccessOrgLookupController(context.OrgStructureService),
                            context.User,
                            context.UserOrgVisibilityController,
                            new AuditLogController(context.AuditService)));

                    case "PermissionAdmin":
                    case "FolderSettings":
                        if (context.OrgLookupController == null && context.OrgStructureService == null)
                        {
                            MessageBox.Show("Org lookup is not available.", "Folder Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return true;
                        }
                        {
                            // Resolve parent folder for conf-level edit enforcement.
                            CMS.Domain.Models.Folder parentFolderCtx = null;
                            if (context.SelectedFolder?.ParentFolderID.HasValue == true && context.FolderReadLookupController != null)
                            {
                                try { parentFolderCtx = context.FolderReadLookupController.GetById(context.SelectedFolder.ParentFolderID.Value); }
                                catch { }
                            }
                            return ShowTargetForm(context, new PermissionForm(
                                context.PermissionService, context.ApprovalService, context.FolderService,
                                context.FolderPermissionLookupController, context.User, context.SelectedFolder,
                                context.UserAccountLookupController, context.FolderSettingsController,
                                context.EmployeeApiService,
                                context.OrgLookupController ?? new UserAccessOrgLookupController(context.OrgStructureService),
                                null, null, parentFolderCtx));
                        }

                    case "UserPermissionSummary":
                        return ShowTargetForm(context, new UserPermissionSummaryForm(context.User, context.ShareLookupController, context.DownloadPolicyController));

                    case "ApprovalInbox":
                        if (context.ApprovalInboxController == null)
                        {
                            MessageBox.Show("Approval inbox controller is not available.", "Approval Inbox", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return true;
                        }
                        return ShowTargetForm(context, new ApprovalInboxForm(context.ApprovalInboxController, context.User, context.UserAccountLookupController));

                    case "AccessLog":
                        if (context.ShareManagementController == null)
                        {
                            MessageBox.Show("Share management controller is not available.", "Access Log", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return true;
                        }
                        return ShowTargetForm(context, new AccessLogView(context.ShareManagementController, context.User, 0));

                    case "AuditLog":
                        if (!TryRequestMdiAction(context, "AuditLog"))
                        {
                            if (context.AuditService == null)
                            {
                                MessageBox.Show("Audit service is not available in this context.", "Audit Log", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return true;
                            }
                            return ShowTargetForm(context, new AuditLogForm(context.AuditService, context.User));
                        }
                        return true;

                    case "ConfigAdmin":
                        if (!TryRequestMdiAction(context, "ConfigAdmin"))
                            MessageBox.Show("กรุณาเปิดผ่าน MDI Container เพื่อเข้าถึงตั้งค่าระบบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "ReportExcel":
                        if (!TryRequestMdiAction(context, "ReportExcel"))
                            MessageBox.Show("เมนูรายงาน Excel ยังต้องเรียกผ่าน MDI Container", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "PdpaConsent":
                        return ShowTargetForm(context, new PdpaConsentDialog());

                    case "PdpaReport":
                        if (!TryRequestMdiAction(context, "PdpaReport"))
                            MessageBox.Show("PDPA Report ต้องมีข้อมูลรายงานจาก service ก่อนเปิด กรุณาเปิดผ่านเมนูรายงานหลัก", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "MenuManagement":
                        if (context.MenuManagementController == null)
                        {
                            MessageBox.Show("Menu management controller is not available.", "Menu Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return true;
                        }
                        return ShowTargetForm(context, new MenuManagementForm(context.MenuManagementController, context.User));

                    case "AuthorityConfig":
                        if (!TryRequestMdiAction(context, "AuthorityConfig"))
                            MessageBox.Show("Authority Config ต้องเปิดผ่าน MDI Container ที่มี service ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "UserPermissionDiagnostic":
                        if (!TryRequestMdiAction(context, "UserPermissionDiagnostic"))
                            MessageBox.Show("User Permission Diagnostic ต้องเปิดผ่าน MDI Container ที่มี service ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "AuthorityByOrgReport":
                        if (!TryRequestMdiAction(context, "AuthorityByOrgReport"))
                            MessageBox.Show("ตรวจสอบสิทธิ์ตามหน่วยงาน ต้องเปิดผ่าน MDI Container ที่มี service ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "DownloadApproverConfig":
                        if (!TryRequestMdiAction(context, "DownloadApproverConfig"))
                            MessageBox.Show("ตั้งค่าผู้อนุมัติดาวน์โหลด ต้องเปิดผ่าน MDI Container ที่มี service ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "MyDownloadRequests":
                        if (!TryRequestMdiAction(context, "MyDownloadRequests"))
                            MessageBox.Show("คำขอดาวน์โหลดของฉัน ต้องเปิดผ่าน MDI Container ที่มี service ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "DownloadUserPermission":
                        if (!TryRequestMdiAction(context, "DownloadUserPermission"))
                            MessageBox.Show("สิทธิ์ดาวน์โหลดรายคน ต้องเปิดผ่าน MDI Container ที่มี service ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "FolderTemplate":
                        return ShowTargetForm(context, new FolderTemplateForm(context.FolderTemplateController, context.OrgLookupController, context.User));

                    case "ArchiveFolder":
                        if (context.TriggerArchive != null)
                            context.TriggerArchive();
                        return true;

                    case "CopyStructure":
                        if (context.TriggerCopyStructure != null)
                            context.TriggerCopyStructure();
                        return true;

                    case "ChangeLevel":
                        if (context.TriggerChangeLevel != null)
                            context.TriggerChangeLevel();
                        return true;

                    case "SyncEmployee":
                        if (!TryRequestMdiAction(context, "SyncEmployee"))
                            MessageBox.Show("กำลังเริ่มกระบวนการ Sync ข้อมูลพนักงานจากระบบ HR...", "Sync ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "RecycleBin":
                        if (context.RecycleBinController == null)
                        {
                            MessageBox.Show("Recycle bin controller is not available.", "Recycle Bin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return true;
                        }
                        return ShowTargetForm(context, new RecycleBinForm(context.RecycleBinController, context.User));

                    case "OrgMasterManagement":
                    case "OrgMasterManagementForm":
                        if (!TryRequestMdiAction(context, "OrgMasterManagement"))
                            MessageBox.Show("กรุณาเปิดจัดการ Master องค์กรผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "KnowledgeBase":
                    case "KnowledgeBaseForm":
                    case "KBMainForm":
                        if (!TryRequestMdiAction(context, "KBMainForm"))
                            MessageBox.Show("กรุณาเปิด Knowledge Base ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    case "KBCategoryForm":
                        if (!TryRequestMdiAction(context, "KBCategoryForm"))
                            MessageBox.Show("กรุณาเปิด KB Category Form ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "ProjectMain":
                    case "ProjectManagement":
                        if (!TryRequestMdiAction(context, "ProjectMain"))
                            MessageBox.Show("กรุณาเปิด Project Management ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "WorkflowDashboard":
                        if (!TryRequestMdiAction(context, "WorkflowDashboard"))
                            MessageBox.Show("กรุณาเปิด Workflow Dashboard ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "WorkflowDetail":
                        if (!TryRequestMdiAction(context, "WorkflowDetail"))
                            MessageBox.Show("กรุณาเปิด Workflow Detail ผ่าน MDI Container", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "WorkflowTemplateList":
                        if (!TryRequestMdiAction(context, "WorkflowTemplateList"))
                            MessageBox.Show("กรุณาเปิด Workflow Template List ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "WorkflowDesigner":
                        if (!TryRequestMdiAction(context, "WorkflowDesigner"))
                            MessageBox.Show("กรุณาเปิด Workflow Designer ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "WorkflowDelegation":
                        if (!TryRequestMdiAction(context, "WorkflowDelegation"))
                            MessageBox.Show("กรุณาเปิด Workflow Delegation ผ่าน MDI Container", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "SLAReport":
                        if (!TryRequestMdiAction(context, "SLAReport"))
                            MessageBox.Show("กรุณาเปิด SLA Report ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "WorkflowMonitor":
                        if (!TryRequestMdiAction(context, "WorkflowMonitor"))
                            MessageBox.Show("กรุณาเปิด Workflow Monitor ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "WorkflowVisualFlow":
                        if (!TryRequestMdiAction(context, "WorkflowVisualFlow"))
                            MessageBox.Show("กรุณาเปิด Workflow Visual Flow ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "QuotaConfig":
                        if (!TryRequestMdiAction(context, "QuotaConfig"))
                            MessageBox.Show("กรุณาเปิด Quota Config ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "QuotaRequest":
                        if (!TryRequestMdiAction(context, "QuotaRequest"))
                            MessageBox.Show("กรุณาเปิด Quota Request ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "QuotaApproval":
                        if (!TryRequestMdiAction(context, "QuotaApproval"))
                            MessageBox.Show("กรุณาเปิด Quota Approval ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "FolderUsageReport":
                        if (!TryRequestMdiAction(context, "FolderUsageReport"))
                            MessageBox.Show("กรุณาเปิด Folder Usage Report ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    case "Frm_Project_List":
                        if (!TryRequestMdiAction(context, "Frm_Project_List"))
                            MessageBox.Show("กรุณาเปิด Frm_Project_List Form ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            return false;
        }

        private static bool ExecuteShareDialog(MenuActionContext context)
        {
            var selectedFiles = (context.SelectedFiles != null && context.SelectedFiles.Count > 0)
                ? context.SelectedFiles
                : (context.SelectedFile != null ? new List<FileDocument> { context.SelectedFile } : null);

            if ((selectedFiles == null || selectedFiles.Count == 0) && context.SelectedFolder == null)
            {
                MessageBox.Show("กรุณาเลือกไฟล์หรือโฟลเดอร์ก่อนแชร์", "Share", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }

            // BR-004-08: 1HC folders cannot be shared
            if (context.SelectedFolder != null && context.SelectedFolder.ConfidentialityLevel == 1)
            {
                MessageBox.Show(
                    "โฟลเดอร์ระดับ 1HC (High Confidential) ไม่สามารถแชร์ข้อมูลได้",
                    "ไม่สามารถแชร์ได้",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return true;
            }

            using (var shareType = new Form())
            {
                shareType.Text = "เลือกประเภทการแบ่งปัน";
                shareType.ClientSize = new Size(330, 150);
                shareType.FormBorderStyle = FormBorderStyle.FixedDialog;
                shareType.StartPosition = FormStartPosition.CenterParent;
                shareType.MaximizeBox = false;
                shareType.MinimizeBox = false;

                var btnInternal = new Button { Text = "ภายใน (Internal)", Width = 130, Height = 34, Left = 25, Top = 70 };
                var btnExternal = new Button { Text = "ภายนอก (External)", Width = 130, Height = 34, Left = 175, Top = 70, Enabled = false };  // ปิดการแชร์ภายนอกไว้ก่อน
                var lbl = new Label { Text = "เลือกประเภทการแชร์", AutoSize = true, Left = 25, Top = 25, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };

                btnInternal.Click += (s, e) =>
                {
                    shareType.DialogResult = DialogResult.OK;
                    shareType.Tag = "internal";
                    shareType.Close();
                };
                btnExternal.Click += (s, e) =>
                {
                    shareType.DialogResult = DialogResult.OK;
                    shareType.Tag = "external";
                    shareType.Close();
                };

                shareType.Controls.Add(lbl);
                shareType.Controls.Add(btnInternal);
                shareType.Controls.Add(btnExternal);

                if (shareType.ShowDialog(context.Owner) == DialogResult.OK)
                {
                    bool isInternal = string.Equals(shareType.Tag as string, "internal", StringComparison.OrdinalIgnoreCase);
                    if (context.ShareCommandController == null)
                    {
                        MessageBox.Show("Share command controller is not available.", "Share", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return true;
                    }

                    if (isInternal)
                    {
                        if (context.OrgLookupController == null && context.OrgStructureService == null)
                        {
                            MessageBox.Show("Org structure service is not available for internal sharing in this context.", "Share", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return true;
                        }
                        using (var dlg = new ShareDialogInternal(
                            context.ShareCommandController,
                            context.UserAccountLookupController,
                            context.OrgLookupController ?? new UserAccessOrgLookupController(context.OrgStructureService),
                            context.User,
                            context.FolderReadLookupController,
                            selectedFiles,
                            context.SelectedFolder))
                        {
                            dlg.ShowDialog(context.Owner);
                        }
                    }
                    else
                    {
                        using (var dlg = new ShareDialogExternal(context.ShareCommandController, context.User, selectedFiles, context.SelectedFolder))
                            dlg.ShowDialog(context.Owner);
                    }
                }
            }

            return true;
        }

        private static bool ShowTargetForm(MenuActionContext context, Form targetForm)
        {
            if (targetForm == null)
                return false;

            if (context.ClearContentPanel != null)
                context.ClearContentPanel();
            if (context.SetContentInfo != null)
                context.SetContentInfo(context.MenuText);

            if (targetForm.WindowState == FormWindowState.Minimized)
                targetForm.WindowState = FormWindowState.Normal;

            Form owner = context.Owner;
            if (owner?.MdiParent != null)
                owner = owner.MdiParent;

            CenterDialogOnMdiContentArea(targetForm, owner);
            targetForm.ShowDialog(owner);
            return true;
        }

        private static void CenterDialogOnMdiContentArea(Form dialog, Form mdiOwner)
        {
            if (mdiOwner == null || !mdiOwner.IsMdiContainer)
                return;

            // Find the MdiClient — the actual content area sitting to the right of the nav panel
            MdiClient mdiClient = null;
            foreach (Control c in mdiOwner.Controls)
            {
                if (c is MdiClient mc) { mdiClient = mc; break; }
            }
            if (mdiClient == null)
                return;

            // Convert MdiClient top-left to screen coordinates
            Point clientScreenPos = mdiOwner.PointToScreen(mdiClient.Location);

            int x = clientScreenPos.X + (mdiClient.Width  - dialog.Width)  / 2;
            int y = clientScreenPos.Y + (mdiClient.Height - dialog.Height) / 2;

            // Clamp to working area so dialog never goes off-screen
            Rectangle workArea = Screen.FromControl(mdiOwner).WorkingArea;
            x = Math.Max(workArea.Left, Math.Min(x, workArea.Right  - dialog.Width));
            y = Math.Max(workArea.Top,  Math.Min(y, workArea.Bottom - dialog.Height));

            dialog.StartPosition = FormStartPosition.Manual;
            dialog.Location      = new Point(x, y);
        }

        private static bool TryRequestMdiAction(MenuActionContext context, string actionKey)
        {
            if (context == null || context.RequestMdiAction == null || string.IsNullOrWhiteSpace(actionKey))
                return false;

            return context.RequestMdiAction(actionKey);
        }
    }
}
