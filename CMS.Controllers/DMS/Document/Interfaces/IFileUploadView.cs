using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// View contract for the FilePresenter (MOD-003).
    /// Implemented by FileUploadForm.
    /// Satisfies Requirements 5.1, 5.2.
    /// </summary>
    public interface IFileUploadView
    {
        /// <summary>Update the status column for a specific file row (by index).</summary>
        void SetFileStatus(int rowIndex, string status, bool isError);

        /// <summary>Update the status column and store the UploadResult (for FileDocumentId lookup).</summary>
        void SetFileResult(int rowIndex, UploadResult result);

        /// <summary>Set the overall progress bar value (0–100).</summary>
        void SetProgress(int percent);

        /// <summary>Show or hide the quota warning label with the given message.</summary>
        void ShowQuotaWarning(string message);

        /// <summary>Hide the quota warning label.</summary>
        void HideQuotaWarning();

        /// <summary>Display a descriptive error message to the user.</summary>
        void ShowError(string message);

        /// <summary>Display an informational message to the user.</summary>
        void ShowInfo(string message);

        /// <summary>Enable or disable the Upload button.</summary>
        void SetUploadEnabled(bool enabled);

        /// <summary>Enable or disable the Download button.</summary>
        void SetDownloadEnabled(bool enabled);

        /// <summary>
        /// The FileDocumentId of the currently selected result row, or null if none selected
        /// or the row did not produce a successful upload.
        /// </summary>
        int? SelectedFileDocumentId { get; }
    }
}
