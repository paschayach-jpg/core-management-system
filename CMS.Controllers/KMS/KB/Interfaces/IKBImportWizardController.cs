using CMS.Domain.Models;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    /// <summary>
    /// View contract for the 3-step KB Import Wizard (MOD-006).
    /// Implemented by KBImportWizardForm in CMS.Presentation.
    /// </summary>
    public interface IKBImportWizardController
    {
        // ── View → Presenter: expose current UI state ────────────────────────────

        /// <summary>Full path of the Excel file the user has selected/dropped.</summary>
        string SelectedFilePath { get; }

        /// <summary>Display name of the selected import template (from the combo box).</summary>
        string SelectedTemplate { get; }

        /// <summary>Step 2 option: skip rows with warnings.</summary>
        bool OptSkipWarning  { get; }

        /// <summary>Step 2 option: auto-create missing categories.</summary>
        bool OptAutoCategory { get; }

        /// <summary>Step 2 option: send e-mail notification after import.</summary>
        bool OptSendEmail    { get; }

        /// <summary>Step 2 option: link imported articles to DMS files.</summary>
        bool OptLinkDMS      { get; }

        /// <summary>Step 2 option: create articles as Draft instead of Published.</summary>
        bool OptCreateDraft  { get; }

        /// <summary>Step 2 option: write audit-log entries for every imported article.</summary>
        bool OptAuditLog     { get; }

        // ── Presenter → View: push results ──────────────────────────────────────

        /// <summary>
        /// Called after <see cref="KBImportWizardController.ValidateFile"/> succeeds.
        /// The view should update the summary pills, populate the preview grid, and
        /// advance to Step 2.
        /// </summary>
        void ShowValidationResult(KBImportValidationResult result);

        /// <summary>
        /// Called after <see cref="KBImportWizardController.ExecuteImport"/> completes.
        /// The view should fill the completion-summary panel and advance to Step 3.
        /// </summary>
        void ShowImportComplete(KBImportValidationResult summary);

        void ShowError(string message);
        void ShowInfo(string message);
        void SetStatus(string message);
    }
}
