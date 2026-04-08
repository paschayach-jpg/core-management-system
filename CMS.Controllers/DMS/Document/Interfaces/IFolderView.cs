using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// View contract for the FolderPresenter.
    /// Implemented by FolderForm.
    /// </summary>
    public interface IFolderView
    {
        /// <summary>Populate the folder tree with the given root folders.</summary>
        void ShowFolderTree(IEnumerable<Folder> rootFolders);

        /// <summary>Populate the contents panel with subfolders and files for the selected folder.</summary>
        void ShowFolderContents(IEnumerable<Folder> subFolders, IEnumerable<FileDocument> files);

        /// <summary>Populate the recycle bin panel with soft-deleted folders.</summary>
        void ShowRecycleBin(IEnumerable<Folder> deletedFolders);

        /// <summary>Display a descriptive error message to the user.</summary>
        void ShowError(string message);

        /// <summary>Display an informational message to the user.</summary>
        void ShowInfo(string message);

        /// <summary>Reload folder tree while respecting the active confidentiality level filter.</summary>
        void ReloadFolderTreeWithFilter();

        /// <summary>The folder currently selected in the tree, or null if none.</summary>
        Folder SelectedFolder { get; }

        /// <summary>The folder currently selected in the recycle bin list, or null if none.</summary>
        Folder SelectedRecycleBinFolder { get; }

        /// <summary>The FileDocument currently selected in the contents list, or null if none.</summary>
        FileDocument SelectedFile { get; }
    }
}
