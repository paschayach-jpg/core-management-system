using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// View contract for the OrgStructurePresenter.
    /// Implemented by OrgStructureForm.
    /// </summary>
    public interface IOrgStructureView
    {
        /// <summary>Populate the tree with the full OrgUnit hierarchy (Organization View).</summary>
        void ShowOrganizationView(IEnumerable<OrgUnit> units);

        /// <summary>Populate the tree grouped by confidentiality level (Confidentiality View).</summary>
        void ShowConfidentialityView(IEnumerable<OrgUnit> units);

        /// <summary>Display a descriptive error message to the user.</summary>
        void ShowError(string message);

        /// <summary>Display an informational message to the user.</summary>
        void ShowInfo(string message);

        /// <summary>The OrgUnit currently selected in the tree, or null if none.</summary>
        OrgUnit SelectedOrgUnit { get; }
    }
}
