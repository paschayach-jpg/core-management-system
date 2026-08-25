# Walkthrough - KB Article Attachments & Audit Logging

## Git Commit Info
- **Branch:** `Pitakpong`
- **Commit ID:** `1f89175`
- **Commit Message:** `feat(kms): implement database persistence and duplicate-check logic for article attachments`

This document details the changes implemented to store real attached files in the `KMS.t_KBArticleAttachment` table, record audit logs when files are attached, and load/display them in both the viewer and editor screens. It also details how file attachment history is recorded in `KMS.t_KBArticleLink` without duplicates.

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
- Added the `SaveArticleAttachments(int articleId)` helper method to save the attached file IDs from the DataGridView grid when either **Save Draft (บันทึก)** or **Send Review (ส่งขอ Review)** is clicked.
- Added the `FormatFileSize` size formatting helper.
- Modified `LoadArticleLinks` to load attachments from the database using `_articleService.GetArticleAttachmentFiles(articleId)` and bind them to the grid when editing an existing article.

#### [IArticleViewerController.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Controllers/KMS/KB/Interfaces/IArticleViewerController.cs)
- Declared `BindAttachments` to bind file document objects to the view.

#### [ArticleViewerForm.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Presentation/Modules/KMS/ArticleViewerForm.cs)
- Implemented `BindAttachments` and updated `PopulateAttachTab()` to draw from the database files instead of mocked files.

#### [ArticleViewerController.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Controllers/KMS/KB/ArticleViewerController.cs)
- Implemented `LoadAttachments(int articleId)` to retrieve the attachment files from the service and pass them to the viewer.

## Verification

### Automated Verification
- Compiled the solution using Visual Studio's MSBuild tool to ensure there are no compilation or signature errors:
  ```powershell
  & "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" CMS.sln /p:Configuration=Debug
  ```
  Result: **Build succeeded with 0 Warnings and 0 Errors**.
