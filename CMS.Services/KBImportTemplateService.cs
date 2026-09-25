using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Business Logic สำหรับจัดการ Excel Import Template ของ Knowledge Base (MOD-006)
    /// ครอบ IKBImportTemplateRepository และเพิ่ม Validation + Audit Logging
    /// </summary>
    public class KBImportTemplateService : IKBImportTemplateService
    {
        private readonly IKBImportTemplateRepository _repo;
        private readonly IKBArticleRepository        _articleRepo;
        private readonly IAuditService               _auditService;

        public KBImportTemplateService(
            IKBImportTemplateRepository repo,
            IKBArticleRepository        articleRepo,
            IAuditService               auditService)
        {
            if (repo         == null) throw new ArgumentNullException("repo");
            if (articleRepo  == null) throw new ArgumentNullException("articleRepo");
            if (auditService == null) throw new ArgumentNullException("auditService");
            _repo         = repo;
            _articleRepo  = articleRepo;
            _auditService = auditService;
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public IEnumerable<KBImportTemplate> GetAllTemplates()
        {
            return _repo.GetAll();
        }

        public KBImportTemplate GetTemplateById(int templateId)
        {
            return _repo.GetById(templateId);
        }

        public IList<KBImportTemplateColumnRow> GetTemplateColumnGrid(int? templateId)
        {
            var targetColumns = _articleRepo.GetImportableTargetColumns();

            var savedMappings = templateId.HasValue
                ? _repo.GetMappingsByTemplateId(templateId.Value)
                : new List<KBImportTemplateColumnMapping>();
            var mappingByColumn = savedMappings.ToDictionary(m => m.TargetColumnName, StringComparer.OrdinalIgnoreCase);

            var rows = new List<KBImportTemplateColumnRow>();
            int order = 0;
            foreach (var columnName in targetColumns)
            {
                KBImportTemplateColumnMapping mapping;
                mappingByColumn.TryGetValue(columnName, out mapping);

                rows.Add(new KBImportTemplateColumnRow
                {
                    TargetColumnName  = columnName,
                    ColumnDescription = KBArticleColumnCatalog.DescribeColumn(columnName),
                    ExcelColumnRef    = mapping != null ? mapping.ExcelColumnRef : string.Empty,
                    DisplayOrder      = order++,
                });
            }
            return rows;
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public int SaveTemplate(KBImportTemplate template, IList<KBImportTemplateColumnRow> rows, UserContext user)
        {
            if (template == null) throw new ArgumentNullException("template");
            if (rows == null) throw new ArgumentNullException("rows");
            if (user == null) throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(template.TemplateName))
                throw new ArgumentException("TemplateName ต้องไม่ว่าง");
            if (_repo.ExistsByName(template.TemplateName.Trim(), template.TemplateID))
                throw new InvalidOperationException(string.Format("มี Template ชื่อ \"{0}\" อยู่แล้ว", template.TemplateName.Trim()));

            var mappedRows = rows.Where(r => !string.IsNullOrWhiteSpace(r.ExcelColumnRef)).ToList();
            if (mappedRows.Count == 0)
                throw new InvalidOperationException("ต้องมีการ Map อย่างน้อย 1 คอลัมน์");

            var duplicates = mappedRows
                .GroupBy(r => r.ExcelColumnRef.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicates.Count > 0)
                throw new InvalidOperationException("มีการ Map คอลัมน์ Excel ซ้ำกัน: " + string.Join(", ", duplicates));

            bool isNew = template.TemplateID <= 0;
            template.TemplateName = template.TemplateName.Trim();
            if (isNew)
            {
                template.CreatedBy = user.AdIdentity;
            }
            else
            {
                template.UpdatedBy = user.AdIdentity;
            }

            var mappings = mappedRows.Select(r => new KBImportTemplateColumnMapping
            {
                TemplateID       = template.TemplateID,
                TargetColumnName = r.TargetColumnName,
                ExcelColumnRef   = r.ExcelColumnRef.Trim(),
                DisplayOrder     = r.DisplayOrder,
            }).ToList();

            int templateId = _repo.SaveTemplateWithMappings(template, mappings);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = isNew ? "CREATE_KB_IMPORT_TEMPLATE" : "UPDATE_KB_IMPORT_TEMPLATE",
                UserID    = user.UserID,
                UserName  = user.AdIdentity,
                TableName = "KMS.m_KBImportTemplate",
                RecordID  = templateId,
                AuditDate = DateTime.Now,
                Result    = 1
            });

            return templateId;
        }

        public void DeleteTemplate(int templateId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            _repo.Deactivate(templateId, user.AdIdentity);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "DEACTIVATE_KB_IMPORT_TEMPLATE",
                UserID    = user.UserID,
                UserName  = user.AdIdentity,
                TableName = "KMS.m_KBImportTemplate",
                RecordID  = templateId,
                AuditDate = DateTime.Now,
                Result    = 1
            });
        }
    }
}
