using System.Collections.Generic;
using CMS.Data.Models;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    /// <summary>
    /// Repository interface for WorkflowDefinition (template) data access.
    /// Supports template versioning: each published change creates a new version row.
    /// </summary>
    public interface IWorkflowTemplateRepository
    {
        /// <summary>Inserts a new template definition and returns its generated ID.</summary>
        int CreateTemplate(WorkflowDefinition template);

        /// <summary>
        /// Retrieves the latest (highest version) definition for the given ID.
        /// Returns null when not found or soft-deleted.
        /// </summary>
        WorkflowDefinition GetTemplate(int workflowDefinitionId);

        /// <summary>
        /// Retrieves a specific version of a template.
        /// Returns null when the version does not exist.
        /// </summary>
        WorkflowDefinition GetTemplateVersion(int workflowDefinitionId, int version);

        /// <summary>Updates an existing template definition row.</summary>
        void UpdateTemplate(WorkflowDefinition template);

        /// <summary>Returns a paged list of templates matching the given filter.</summary>
        PagedResult<WorkflowDefinition> GetTemplates(WorkflowTemplateFilter filter);

        /// <summary>Returns all version rows for a template, ordered by version descending.</summary>
        IList<WorkflowDefinition> GetTemplateVersions(int workflowDefinitionId);

        /// <summary>
        /// Soft-deletes a template.
        /// The caller must verify no active instances reference this template before calling.
        /// </summary>
        void SoftDeleteTemplate(int workflowDefinitionId, string deletedBy);

        /// <summary>
        /// Returns true when at least one non-deleted, non-terminal WorkflowInstance
        /// references the given template.  Used to guard template edits and deletions.
        /// </summary>
        bool HasActiveInstances(int workflowDefinitionId);
    }

    /// <summary>Filter parameters for querying workflow templates.</summary>
    public class WorkflowTemplateFilter
    {
        public bool?   IsActive      { get; set; }
        public string  NameContains  { get; set; }
        public int     PageNumber    { get; set; }
        public int     PageSize      { get; set; }
        /// <summary>Filter by ApprovalStatus value (tinyint). Null = no filter.</summary>
        public byte?   ApprovalStatus { get; set; }
        /// <summary>Filter by Category string. Null = no filter.</summary>
        public string  Category      { get; set; }

        public WorkflowTemplateFilter() { PageNumber = 1; PageSize = 50; }
    }
}
