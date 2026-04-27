using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Services.Caching
{
    /// <summary>
    /// In-memory cache implementation for workflow templates
    /// Supports Phase 13: Performance Optimization
    /// Requirement 21 AC1-3: Performance targets for template access
    /// 
    /// Cache Strategy:
    /// - Active templates cached in memory with 1-hour TTL
    /// - Individual template lookups cached with 30-minute TTL
    /// - Cache invalidated on template updates
    /// - Thread-safe using lock mechanism
    /// </summary>
    public class WorkflowTemplateCacheImpl : IWorkflowTemplateCache
    {
        private readonly IWorkflowTemplateRepository _templateRepository;
        private readonly ILoggingService _loggingService;

        // Cache entries
        private Dictionary<int, CacheEntry<WorkflowDefinition>> _templateCache;
        private CacheEntry<IList<WorkflowDefinition>> _activeTemplatesCache;
        private readonly object _lockObject = new object();

        // Cache configuration
        private const int TEMPLATE_CACHE_MINUTES = 30;
        private const int ACTIVE_TEMPLATES_CACHE_MINUTES = 60;

        // Statistics
        private TemplateCacheStats _stats;

        public WorkflowTemplateCacheImpl(
            IWorkflowTemplateRepository templateRepository,
            ILoggingService loggingService)
        {
            _templateRepository = templateRepository;
            _loggingService = loggingService;
            _templateCache = new Dictionary<int, CacheEntry<WorkflowDefinition>>();
            _stats = new TemplateCacheStats();
        }

        public WorkflowDefinition GetTemplate(int workflowDefinitionId)
        {
            lock (_lockObject)
            {
                _stats.TotalRequests++;

                // Check if template is in cache and not expired
                if (_templateCache.ContainsKey(workflowDefinitionId))
                {
                    var cacheEntry = _templateCache[workflowDefinitionId];
                    if (!cacheEntry.IsExpired)
                    {
                        _stats.CacheHits++;
                        _loggingService.Debug($"Template cache hit for ID {workflowDefinitionId}");
                        return cacheEntry.Value;
                    }
                    else
                    {
                        // Remove expired entry
                        _templateCache.Remove(workflowDefinitionId);
                    }
                }

                // Cache miss - load from repository
                _stats.CacheMisses++;
                var template = _templateRepository.GetTemplate(workflowDefinitionId);

                if (template != null)
                {
                    _templateCache[workflowDefinitionId] = new CacheEntry<WorkflowDefinition>
                    {
                        Value = template,
                        ExpirationTime = DateTime.UtcNow.AddMinutes(TEMPLATE_CACHE_MINUTES)
                    };
                    _loggingService.Debug($"Template {workflowDefinitionId} loaded and cached");
                }

                return template;
            }
        }

        public IList<WorkflowDefinition> GetActiveTemplates()
        {
            lock (_lockObject)
            {
                _stats.TotalRequests++;

                // Check if active templates are in cache and not expired
                if (_activeTemplatesCache != null && !_activeTemplatesCache.IsExpired)
                {
                    _stats.CacheHits++;
                    _loggingService.Debug("Active templates cache hit");
                    return _activeTemplatesCache.Value;
                }

                // Cache miss - load from repository
                _stats.CacheMisses++;
                var filter = new WorkflowTemplateFilter { IsActive = true, PageSize = 1000 };
                var result = _templateRepository.GetTemplates(filter);
                var templates = result.Items;

                _activeTemplatesCache = new CacheEntry<IList<WorkflowDefinition>>
                {
                    Value = templates,
                    ExpirationTime = DateTime.UtcNow.AddMinutes(ACTIVE_TEMPLATES_CACHE_MINUTES)
                };

                _loggingService.Debug($"Active templates loaded and cached: {templates.Count} templates");
                return templates;
            }
        }

        public void InvalidateTemplate(int workflowDefinitionId)
        {
            lock (_lockObject)
            {
                if (_templateCache.ContainsKey(workflowDefinitionId))
                {
                    _templateCache.Remove(workflowDefinitionId);
                    _loggingService.Info($"Template cache invalidated for ID {workflowDefinitionId}");
                }

                // Also invalidate active templates cache since it may contain this template
                _activeTemplatesCache = null;
                _stats.LastInvalidation = DateTime.UtcNow;
            }
        }

        public void InvalidateAll()
        {
            lock (_lockObject)
            {
                _templateCache.Clear();
                _activeTemplatesCache = null;
                _loggingService.Info("All template caches invalidated");
                _stats.LastInvalidation = DateTime.UtcNow;
            }
        }

        public TemplateCacheStats GetStats()
        {
            lock (_lockObject)
            {
                _stats.CachedItemCount = _templateCache.Count + (_activeTemplatesCache != null ? 1 : 0);
                return new TemplateCacheStats
                {
                    TotalRequests = _stats.TotalRequests,
                    CacheHits = _stats.CacheHits,
                    CacheMisses = _stats.CacheMisses,
                    CachedItemCount = _stats.CachedItemCount,
                    LastInvalidation = _stats.LastInvalidation
                };
            }
        }

        /// <summary>
        /// Internal cache entry class
        /// </summary>
        private class CacheEntry<T>
        {
            public T Value { get; set; }
            public DateTime ExpirationTime { get; set; }
            public bool IsExpired => DateTime.UtcNow > ExpirationTime;
        }
    }
}
