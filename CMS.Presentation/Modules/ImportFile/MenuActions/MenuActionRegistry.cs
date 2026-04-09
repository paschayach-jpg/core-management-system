using CMS.Presentation.Modules.ImportFile;
using CMS.Presentation.Modules.KMS;
using System;
using System.Drawing;
using System.Windows.Forms;

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
                        return ShowTargetForm(context, new ShareManagementForm(context.ShareService, context.User, context.ShareLookupController));

                    case "OrgStructure":
                        return ShowTargetForm(context, new OrgStructureForm(context.OrgStructureService, context.User, null, null));

                    case "UserManagement":
                        return ShowTargetForm(context, new UserManagementForm(context.UserAdminController, context.User, context.AuditService));

                    case "UserAccess":
                        return ShowTargetForm(context, new UserAccessForm(context.UserAdminController, context.OrgStructureService, context.User, null, null, context.AuditService));

                    case "PermissionAdmin":
                    case "FolderSettings":
                        return ShowTargetForm(context, new PermissionForm(context.PermissionService, context.ApprovalService, context.FolderService, context.FolderPermissionLookupController, context.User, context.SelectedFolder, context.UserAccountLookupController, context.FolderSettingsController, null, context.OrgStructureService, null, null));

                    case "UserPermissionSummary":
                        return ShowTargetForm(context, new UserPermissionSummaryForm(context.User, context.ShareLookupController));

                    case "ApprovalInbox":
                        return ShowTargetForm(context, new ApprovalInboxForm(context.ApprovalService, context.User));

                    case "AccessLog":
                        return ShowTargetForm(context, new AccessLogView(context.ShareService, context.User, 0));

                    case "AuditLog":
                        return ShowTargetForm(context, new AuditLogForm(context.AuditService, context.User));

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
                        if (context.MenuService == null)
                        {
                            MessageBox.Show("Menu service is not available.", "Menu Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return true;
                        }
                        return ShowTargetForm(context, new MenuManagementForm(context.MenuService, context.User));

                    case "AuthorityConfig":
                        if (!TryRequestMdiAction(context, "AuthorityConfig"))
                            MessageBox.Show("Authority Config ต้องเปิดผ่าน MDI Container ที่มี service ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;

                    case "FolderTemplate":
                        return ShowTargetForm(context, new FolderTemplateForm(context.FolderTemplateController, context.User));

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
                        return ShowTargetForm(context, new RecycleBinForm(context.FolderService, context.User, context.OrgStructureService));

                    case "KnowledgeBase":
                    case "KnowledgeBaseForm":
                        if (!TryRequestMdiAction(context, "KnowledgeBaseForm"))
                            MessageBox.Show("กรุณาเปิด Knowledge Base ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    case "KBCategoryForm":
                        if (!TryRequestMdiAction(context, "KBCategoryForm"))
                            MessageBox.Show("กรุณาเปิด KB Category Form ผ่าน MDI Container เพื่อใช้ service ให้ครบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (context.SelectedFile == null && context.SelectedFolder == null)
            {
                MessageBox.Show("กรุณาเลือกไฟล์หรือโฟลเดอร์ก่อนแชร์", "Share", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                var btnExternal = new Button { Text = "ภายนอก (External)", Width = 130, Height = 34, Left = 175, Top = 70 };
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
                    if (isInternal)
                    {
                        using (var dlg = new ShareDialogInternal(context.ShareService, context.FileService, context.FolderService, context.PermissionService, context.UserAccountLookupController, context.OrgStructureService, context.User, context.SelectedFile, context.SelectedFolder))
                            dlg.ShowDialog(context.Owner);
                    }
                    else
                    {
                        using (var dlg = new ShareDialogExternal(context.ShareService, context.FileService, context.FolderService, context.User, context.SelectedFile, context.SelectedFolder))
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

            // Respect caller-defined size/state; only recover from a minimized window.
            if (targetForm.WindowState == FormWindowState.Minimized)
                targetForm.WindowState = FormWindowState.Normal;
            targetForm.ShowDialog(context.Owner);
            return true;
        }

        private static bool TryRequestMdiAction(MenuActionContext context, string actionKey)
        {
            if (context == null || context.RequestMdiAction == null || string.IsNullOrWhiteSpace(actionKey))
                return false;

            return context.RequestMdiAction(actionKey);
        }
    }
}