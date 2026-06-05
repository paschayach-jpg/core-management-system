using System;
using System.Collections.Generic;
using CMS.Domain.Models.Workflow;

namespace CMS.Services.Workflow.Definition
{
    public interface IStepGraphCache
    {
        StepGraph Get(string definitionCode);
        void Set(string definitionCode, StepGraph graph);
        void Invalidate(string definitionCode);
        void InvalidateAll();
    }

    // FR-008-15: StepGraph in-memory cache, TTL 30 นาที
    // invalidate เมื่อ Activate version ใหม่ (BR-008-08)
    public class StepGraphCache : IStepGraphCache
    {
        private const int CacheTtlMinutes = 30;

        private readonly Dictionary<string, CacheEntry> _cache =
            new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

        private readonly object _lock = new object();

        public StepGraph Get(string definitionCode)
        {
            lock (_lock)
            {
                CacheEntry entry;
                if (_cache.TryGetValue(definitionCode, out entry))
                {
                    if (!entry.IsExpired)
                        return entry.Graph;

                    _cache.Remove(definitionCode);
                }
                return null;
            }
        }

        public void Set(string definitionCode, StepGraph graph)
        {
            lock (_lock)
            {
                _cache[definitionCode] = new CacheEntry(graph, CacheTtlMinutes);
            }
        }

        public void Invalidate(string definitionCode)
        {
            lock (_lock)
            {
                _cache.Remove(definitionCode);
            }
        }

        public void InvalidateAll()
        {
            lock (_lock)
            {
                _cache.Clear();
            }
        }

        private class CacheEntry
        {
            public StepGraph Graph      { get; }
            public DateTime  ExpiresAt  { get; }
            public bool IsExpired { get { return DateTime.UtcNow > ExpiresAt; } }

            public CacheEntry(StepGraph graph, int ttlMinutes)
            {
                Graph     = graph;
                ExpiresAt = DateTime.UtcNow.AddMinutes(ttlMinutes);
            }
        }
    }
}
