using CMS.Domain.Models;
using System;
using System.Collections.Generic;

namespace CMS.Services.Caching
{
    /// <summary>
    /// Interface for workflow template caching
    /// Supports Phase 13: Performance Optimization
    /// Requirement 21 AC1-3: Performance targets for template access
    /// </summary>
    public interface IWorkflowTemplateCache
    {
        /// <summary>
        /// Get template from cache or load from repository
        /// </summary>
        WorkflowDefinition GetTemplate(int workflowDefinitionId);

        /// <summary>
        /// Get all active templates from cache
        /// </summary>
        IList<WorkflowDefinition> GetActiveTemplates();

        /// <summary>
        /// Invalidate template cache entry
        /// </summary>
        void InvalidateTemplate(int workflowDefinitionId);

        /// <summary>
        /// Invalidate all template cache entries
        /// </summary>
        void InvalidateAll();

        /// <summary>
        /// Get cache statistics for monitoring
        /// </summary>
        TemplateCacheStats GetStats();
    }

    /// <summary>
    /// Cache statistics for monitoring and performance analysis
    /// </summary>
    public class TemplateCacheStats
    {
        public int TotalRequests { get; set; }
        public int CacheHits { get; set; }
        public int CacheMisses { get; set; }
        public double HitRate => TotalRequests > 0 ? (double)CacheHits / TotalRequests : 0;
        public int CachedItemCount { get; set; }
        public DateTime LastInvalidation { get; set; }
    }
}
