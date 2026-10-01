using System.Collections.Generic;
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

        /// <summary>TemplateID of the selected KB Import Template (from the combo box), or null if none selected.</summary>
        int? SelectedTemplateId { get; }

        // ── Presenter → View: push results ──────────────────────────────────────

        /// <summary>Binds the list of configured KB Import Templates to the template combo box.</summary>
        void BindTemplateOptions(IEnumerable<KBImportTemplate> templates);

        /// <summary>
        /// Refreshes the column-reference panel (lvColumnRef) to preview the selected Template's
        /// actual column mapping. Empty <paramref name="columns"/>/<paramref name="templateName"/>
        /// means no template is selected — the view should fall back to its static reference table.
        /// </summary>
        void BindTemplateColumnReference(IEnumerable<KBImportTemplateColumnRow> columns, string templateName);

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
        void SetProgress(int percent, string message);
    }
}
