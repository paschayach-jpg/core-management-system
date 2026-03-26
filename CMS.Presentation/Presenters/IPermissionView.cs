using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Presentation.Presenters
{
    public interface IPermissionView
    {
        void ShowAclEntries(IEnumerable<FolderPermission> entries);
        void ShowEffectivePermissions(EffectivePermission effective);

        /// <summary>Display the folder's confidentiality level badge. Level: 1=HC, 2=SC, 3=NC, 4=Public</summary>
        void ShowConfidentialityLevel(int level);

        /// <summary>Append pending approval requests to the ACL list as read-only rows.</summary>
        void ShowPendingApprovals(IEnumerable<ApprovalRequest> pendingRequests);

        void ShowError(string message);
        void ShowInfo(string message);

        /// <summary>
        /// Asks the user whether to propagate folder permissions to all files in the folder.
        /// Returns true if the user confirms propagation.
        /// </summary>
        bool ConfirmPropagateToFiles(int fileCount);

        int? SelectedPermissionId { get; }
        Folder SelectedFolder { get; }
    }
}
