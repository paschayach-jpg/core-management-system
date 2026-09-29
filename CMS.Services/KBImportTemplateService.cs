using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly IKBCategoryRepository       _categoryRepo;
        private readonly IAuditService               _auditService;

        /// <summary>ชื่อคอลัมน์เป้าหมายที่ใช้ dropdown เลือกค่าคงที่แทนการ map จาก Excel — ไม่รวมใน validation ห้าม map ซ้ำ</summary>
        private static readonly HashSet<string> FixedValueTargetColumns =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CategoryID", "ArticleType" };

        /// <summary>จับ token รูปแบบ {ชื่อคอลัมน์} ที่ถูกต้อง เช่น "{C}" ใน "มาตรา {C}"</summary>
        private static readonly Regex FormulaPlaceholderPattern = new Regex(@"\{[^{}]+\}", RegexOptions.Compiled);

        public KBImportTemplateService(
            IKBImportTemplateRepository repo,
            IKBArticleRepository        articleRepo,
            IKBCategoryRepository       categoryRepo,
            IAuditService               auditService)
        {
            if (repo         == null) throw new ArgumentNullException("repo");
            if (articleRepo  == null) throw new ArgumentNullException("articleRepo");
            if (categoryRepo == null) throw new ArgumentNullException("categoryRepo");
            if (auditService == null) throw new ArgumentNullException("auditService");
            _repo         = repo;
            _articleRepo  = articleRepo;
            _categoryRepo = categoryRepo;
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

        /// <summary>รายการหมวดหมู่สำหรับ dropdown เลือก CategoryID (ไม่รวม CategoryCode = "LW")</summary>
        public IEnumerable<KBCategory> GetCategoryOptions()
        {
            return _categoryRepo.GetAll().Where(c => !string.Equals(c.CategoryCode, "LW", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>รายการค่า ArticleType คงที่ (1-5) สำหรับ dropdown</summary>
        public IList<KBArticleTypeOption> GetArticleTypeOptions()
        {
            return KBArticleTypeCatalog.GetOptions();
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

            // CategoryID/ArticleType เป็น dropdown แยกบนหน้าจอ (ไม่ใช่แถวใน grid mapping) แต่ยังต้องมีการ
            // เลือกจริงเสมอ — default ของ dropdown คือ "ทั้งหมด" (ExcelColumnRef ว่าง) ซึ่งใช้เป็น fixed
            // value ตอน import ไม่ได้ ต้องเลือกให้ชัดเจนก่อนบันทึกทุกครั้ง
            if (!mappedRows.Any(r => string.Equals(r.TargetColumnName, "CategoryID", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("กรุณาเลือกหมวดหมู่ (CategoryID) ก่อนบันทึก — ยังเป็น \"ทั้งหมด\" อยู่");
            if (!mappedRows.Any(r => string.Equals(r.TargetColumnName, "ArticleType", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("กรุณาเลือกประเภทบทความ (ArticleType) ก่อนบันทึก — ยังเป็น \"ทั้งหมด\" อยู่");

            if (mappedRows.Count == 0)
                throw new InvalidOperationException("ต้องมีการ Map อย่างน้อย 1 คอลัมน์");

            foreach (var row in mappedRows)
            {
                string formulaError;
                if (!TryValidateFormulaSyntax(row.ExcelColumnRef, out formulaError))
                    throw new InvalidOperationException(string.Format("คอลัมน์ \"{0}\": {1}", row.TargetColumnName, formulaError));
            }

            var duplicates = mappedRows
                .Where(r => !FixedValueTargetColumns.Contains(r.TargetColumnName)) // CategoryID/ArticleType เป็นค่าคงที่ ไม่ใช่การใช้คอลัมน์ Excel จึงไม่นับซ้ำ
                .Where(r => !IsFormulaMapping(r.ExcelColumnRef)) // formula เช่น "มาตรา {C}" ใช้คอลัมน์ต้นทางร่วมกันได้ (เช่น Title/Tags ของกฎหมาย) จึงไม่นับซ้ำ
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

        // ── Private helpers — formula mapping (เช่น Title ของกฎหมาย = "มาตรา {C}") ──────

        /// <summary>true ถ้าค่า mapping เป็น formula ที่ผสมข้อความคงที่กับ placeholder {ชื่อคอลัมน์} แทนที่จะ map ตรงจากคอลัมน์เดียว</summary>
        private static bool IsFormulaMapping(string excelColumnRef)
        {
            return excelColumnRef != null && (excelColumnRef.IndexOf('{') >= 0 || excelColumnRef.IndexOf('}') >= 0);
        }

        /// <summary>ตรวจว่า placeholder {ชื่อคอลัมน์} ในค่า mapping มีวงเล็บปีกกาเปิด-ปิดครบคู่ ไม่ใช้กับค่า mapping ตรงแบบธรรมดา</summary>
        private static bool TryValidateFormulaSyntax(string value, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(value) || !IsFormulaMapping(value))
                return true;

            string stripped = FormulaPlaceholderPattern.Replace(value, string.Empty);
            if (stripped.IndexOf('{') >= 0 || stripped.IndexOf('}') >= 0)
            {
                error = "รูปแบบ {ชื่อคอลัมน์} ไม่ถูกต้อง ต้องมีวงเล็บปีกกาเปิด-ปิดครบคู่ เช่น \"มาตรา {C}\"";
                return false;
            }
            return true;
        }

        // ── LayoutType — derived from CategoryID, not stored ───────────────────

        /// <summary>
        /// Derive ค่า LayoutType ("Law"/"Order") จาก <paramref name="categoryId"/> โดยไล่
        /// <see cref="KBCategory.ParentCategoryID"/> ขึ้นไปจนกว่าจะเจอ category ที่ CategoryCode = "LW"
        /// (หมวด "กฎหมาย" ราก) — เจอแปลว่า Law, ไม่เจอ (root, CategoryCode &lt;&gt; "LW") แปลว่า Order
        /// ใช้แทนการเก็บ LayoutType เป็น field แยกใน Template เพราะ CategoryID ที่ config ไว้อยู่แล้ว
        /// บอกได้อยู่แล้วว่าเป็นกฎหมายหรือไม่ ไม่มีทางตั้งขัดแย้งกันเองได้อีกต่อไป
        /// </summary>
        public string GetEffectiveLayoutType(int categoryId)
        {
            var byId = _categoryRepo.GetAll().ToDictionary(c => c.CategoryID);

            var visited = new HashSet<int>();
            int current = categoryId;
            KBCategory cat;
            while (byId.TryGetValue(current, out cat))
            {
                if (string.Equals(cat.CategoryCode, "LW", StringComparison.OrdinalIgnoreCase))
                    return KBImportLayoutTypeCatalog.Law;
                if (!cat.ParentCategoryID.HasValue || !visited.Add(current))
                    return KBImportLayoutTypeCatalog.Order;
                current = cat.ParentCategoryID.Value;
            }
            throw new InvalidOperationException(string.Format("ไม่พบ CategoryID {0} ในระบบ", categoryId));
        }
    }
}
