# Walkthrough - KB Article Attachments & Configuration Migration

This document details the changes implemented to store real attached files in the `KMS.t_KBArticleAttachment` table, record audit logs when files are attached, and load/display them in both the viewer and editor screens. It also details how file attachment history is recorded in `KMS.t_KBArticleLink` without duplicates. Finally, it details the migration of appSettings from `app.config` to an external JSON configuration file.

## Git Commit Info
- **Branch:** `Pitakpong`
- **Initial Attachments Commit:** `1f89175`
- **Documentation Update Commit:** `2335d04`
- **Config JSON Migration Commit:** `8776d92`
- **App.config Cleanup Commit:** `1cc970f`
- **Local Config Deletion & Network Fallback Commit:** `d838e1e`
- **KMS Attachments Permission Bypass Commit:** `7551f2e`
- **App.config Cleanup Commit:** `1cc970f`
- **Local Config Deletion & Network Fallback Commit:** `d838e1e`

---

## Changes Made

### 1. Database & Model Configuration

#### [NEW] [KBArticleAttachment.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Domain/Models/KBArticleAttachment.cs)
- Created the domain model representing a file attachment, mapping to `KMS.t_KBArticleAttachment`.

#### [CMS.Domain.csproj](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Domain/CMS.Domain.csproj)
- Added `Models\KBArticleAttachment.cs` to the project compile files.

### 2. Repository Layer

#### [NEW] [IKBArticleAttachmentRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Interfaces/IKBArticleAttachmentRepository.cs)
- Defined repository interface signatures for `GetAttachmentsForArticle`, `Insert`, `Delete`, and `DeleteAllForArticle`.

#### [NEW] [KBArticleAttachmentRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/KBArticleAttachmentRepository.cs)
- Implemented the database persistence logic for the new repository using raw ADO.NET SQL commands mapping to `KMS.t_KBArticleAttachment`.

#### [CMS.Data.csproj](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/CMS.Data.csproj)
- Added the interface and repository implementation to the compile items list.

### 3. Service Layer

#### [IKBArticleService.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Services/Interfaces/IKBArticleService.cs)
- Added `GetArticleAttachments`, `GetArticleAttachmentFiles`, and `SaveArticleAttachments` method declarations.

#### [KBArticleService.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Services/KBArticleService.cs)
- Injected `IKBArticleAttachmentRepository` and `IFileDocumentRepository`.
- Implemented `GetArticleAttachmentFiles` to fetch file metadata (such as original filename and size) from `storage.FileDocument` based on the attachment mappings.
- Implemented `SaveArticleAttachments` which performs an atomic replacement of the article's attachments and calls `IAuditService.Log(...)` to record the `"ATTACH_FILE"` audit log action in the immutable audit log table (`audit.AuditLog`).
- **[NEW PROCESS]** Updated `SaveArticleAttachments` to check for existing links of type `EntityType = 1` (Folder/File) for each attached file. If no duplicate exists, it inserts a new link record into `KMS.t_KBArticleLink` containing:
  - `ArticleID`
  - `EntityType = 1` (Folder/File)
  - `EntityID = fileId.ToString()`
  - `CreatedBy = actionBy`
  - `CreatedDate = DateTime.Now`
- **[PRESERVATION]** Updated `SaveArticleLinks` to only delete specific link types managed by the tabs (Task/2, Program/3, Law/4), ensuring that the `Folder/File` (1) history links and `Department` (6) links are preserved.

### 4. Presentation & MVP Layer

#### [Program.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Presentation/Program.cs)
- Instantiated `KBArticleAttachmentRepository` at the composition root and passed it along with `fileDocumentRepository` to the `KBArticleService` constructor.

#### [IKBEditorController.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Controllers/KMS/KB/Interfaces/IKBEditorController.cs)
- Declared the `GetAttachLinkedDataGridView` method to expose the attachments grid control to the presenter.

#### [KBEditorDialog.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Presentation/Modules/KMS/KBEditorDialog.cs)
- Implemented `GetAttachLinkedDataGridView()` to return the `dgvAttachLinked` control.
- Removed original hardcoded mockup files from initializing on form load.

#### [KBEditorController.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Controllers/KMS/KB/KBEditorController.cs)
- Added the `SaveArticleAttachments(int articleId)` helper method to save the attached file IDs from the DataGridView grid when either **Save Draft (เน€เธยเน€เธเธ‘เน€เธยเน€เธโ€”เน€เธเธ–เน€เธย)** or **Send Review (เน€เธเธเน€เธยเน€เธยเน€เธยเน€เธเธ Review)** is clicked.
- Added the `FormatFileSize` size formatting helper.
- Modified `LoadArticleLinks` to load attachments from the database using `_articleService.GetArticleAttachmentFiles(articleId)` and bind them to the grid when editing an existing article.

#### [IArticleViewerController.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Controllers/KMS/KB/Interfaces/IArticleViewerController.cs)
- Declared `BindAttachments` to bind file document objects to the view.

#### [ArticleViewerForm.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Presentation/Modules/KMS/ArticleViewerForm.cs)
- Implemented `BindAttachments` and updated `PopulateAttachTab()` to draw from the database files instead of mocked files.

#### [ArticleViewerController.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Controllers/KMS/KB/ArticleViewerController.cs)
- Implemented `LoadAttachments(int articleId)` to retrieve the attachment files from the service and pass them to the viewer.

### 5. Configuration JSON Migration (Environment Variable & Network Share Mode)

#### [ConfigManager.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Infrastructure/ConfigManager.cs)
- Replaced the reading of settings from `ConfigurationManager.AppSettings` with loading from an external JSON configuration file.
- The path of the JSON file is retrieved from the environment variable `CMS_CONFIG_PATH`.
- **[FALLBACK]** If the environment variable is not defined or empty, the app falls back to loading from the central network path: `\\150.1.8.23\AppForUser\CMS_Central_Configs\cms_config.json`.
- If both the environment variable path and central network share path do not exist or are inaccessible, a `FileNotFoundException` is thrown.
- Implemented flat lookup in the `"AppSettings"` section of the JSON to retain 100% compatibility with all existing keys.

#### [CMS.Infrastructure.csproj](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Infrastructure/CMS.Infrastructure.csproj)
- Added a package reference to `Newtonsoft.Json` to support JSON parsing.

#### [NEW] [packages.config](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Infrastructure/packages.config)
- Configured NuGet dependency configuration for the `CMS.Infrastructure` project.

- **Note:** All local `cms_config.json` files have been deleted from the repository. The configuration is loaded exclusively from the designated external network path.

### 6. KMS Attachment Permission Bypass

#### [IFileService.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Services/Interfaces/IFileService.cs) & [FileService.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Services/FileService.cs)
- Added `bool bypassPermissionCheck = false` parameter to `DownloadFile` method.
- When `bypassPermissionCheck` is set to `true`, the service bypasses the folder-level read permission checks (`_permissionService.CheckPermission` and `_filePermissionRepository.GetByFile`), allowing immediate download.

#### [IFileBrowseController.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Controllers/DMS/Document/Interfaces/IFileBrowseController.cs) & [FileBrowseController.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Controllers/DMS/Document/FileBrowseController.cs)
- Propagated the `bypassPermissionCheck` parameter down to the controller level.

#### [KBEditorDialog.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Presentation/Modules/KMS/KBEditorDialog.cs)
- Set `bypassPermissionCheck: true` on the `_fileBrowseController.DownloadFile` invocation within `DownloadAndOpenFile` method.
- This ensures that users viewing attachments directly from the KMS Editor screen are not blocked by underlying folder-level permissions (e.g. Folder 437).

---

## Verification

### Automated Verification
- Compiled the solution using Visual Studio's MSBuild tool to ensure there are no compilation or signature errors:
  ```powershell
  & "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" CMS.sln /p:Configuration=Debug
  ```
  Result: **Build succeeded with 0 Warnings and 0 Errors**.
