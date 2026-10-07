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

        /// <summary>ชื่อคอลัมน์เป้าหมายที่เลือกค่าจาก dropdown แทนการ map จากคอลัมน์ Excel — ไม่รวมใน validation ห้าม map ซ้ำ</summary>
        private static readonly HashSet<string> FixedValueTargetColumns =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { KBArticleColumnCatalog.LayoutTypeTargetColumn, "ArticleType" };

        /// <summary>
        /// คอลัมน์ที่ระบบหาเองตอน import จึงไม่ให้ map ใน Template: CategoryID (หาจาก ArticleCode ของแต่ละแถว — ดู
        /// <see cref="KBImportCategoryResolver"/>) แถวเก่าที่ Template เคยบันทึกไว้จะถูกทิ้งเมื่อบันทึกซ้ำ
        /// </summary>
        private const string DerivedTargetColumn = "CategoryID";

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
            // CategoryID ไม่ให้ map ใน Template แล้ว (ระบบหาจาก ArticleCode ของแต่ละแถวตอน import)
            // Attachment/LayoutType เป็นแถวเสมือน (ไม่ใช่คอลัมน์ของ t_KBArticle) ต่อท้ายรายการคอลัมน์จริง
            var targetColumns = _articleRepo.GetImportableTargetColumns()
                .Where(c => !string.Equals(c, DerivedTargetColumn, StringComparison.OrdinalIgnoreCase))
                .ToList();
            targetColumns.Add(KBArticleColumnCatalog.AttachmentTargetColumn);
            targetColumns.Add(KBArticleColumnCatalog.LawReferenceTargetColumn);
            targetColumns.Add(KBArticleColumnCatalog.LayoutTypeTargetColumn);

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

            // Template เก่า (ผูกกับ 1 หมวด) ยังไม่มีแถว LayoutType — derive จากแถว CategoryID เดิมให้ ใช้งานได้ทันทีโดยไม่ต้องรัน SQL
            var layoutRow = rows.First(r => string.Equals(r.TargetColumnName, KBArticleColumnCatalog.LayoutTypeTargetColumn, StringComparison.OrdinalIgnoreCase));
            if (KBImportLayoutTypeCatalog.Normalize(layoutRow.ExcelColumnRef) == null)
            {
                KBImportTemplateColumnMapping legacyCategory;
                int legacyCategoryId;
                if (mappingByColumn.TryGetValue(DerivedTargetColumn, out legacyCategory)
                    && int.TryParse(legacyCategory.ExcelColumnRef, out legacyCategoryId) && legacyCategoryId > 0)
                {
                    layoutRow.ExcelColumnRef = DeriveLayoutType(legacyCategoryId) ?? string.Empty;
                }
            }

            return rows;
        }

        /// <summary>ตัวเลือกประเภทไฟล์ของ Template (กฎหมาย / ไม่ใช่กฎหมาย) สำหรับ dropdown</summary>
        public IList<KBImportLayoutOption> GetLayoutOptions()
        {
            return KBImportLayoutTypeCatalog.GetLayoutOptions();
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

            // กฎตรวจ Template ชุดเดียวกับตอน Upload (GetTemplateProblems) — แสดงทุกข้อที่ขาดพร้อมกัน
            var problems = GetTemplateProblems(rows);
            if (problems.Count > 0)
                throw new InvalidOperationException("ตั้งค่า Template ยังไม่ครบ:\n• " + string.Join("\n• ", problems));

            // CategoryID ไม่เก็บใน Template แล้ว (แถวเก่าถูกทิ้งตอนบันทึก) — ประเภทไฟล์เก็บเป็นแถว LayoutType
            var mappedRows = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.ExcelColumnRef)
                         && !string.Equals(r.TargetColumnName, DerivedTargetColumn, StringComparison.OrdinalIgnoreCase))
                .ToList();

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
                // LayoutType เก็บค่ามาตรฐาน "Law"/"NonLaw" เสมอ (GetTemplateProblems ตรวจแล้วว่าเป็นค่าที่รู้จัก)
                ExcelColumnRef   = string.Equals(r.TargetColumnName, KBArticleColumnCatalog.LayoutTypeTargetColumn, StringComparison.OrdinalIgnoreCase)
                                       ? KBImportLayoutTypeCatalog.Normalize(r.ExcelColumnRef)
                                       : r.ExcelColumnRef.Trim(),
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

        public void ActivateTemplate(int templateId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            _repo.Activate(templateId, user.AdIdentity);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "ACTIVATE_KB_IMPORT_TEMPLATE",
                UserID    = user.UserID,
                UserName  = user.AdIdentity,
                TableName = "KMS.m_KBImportTemplate",
                RecordID  = templateId,
                AuditDate = DateTime.Now,
                Result    = 1
            });
        }

        // ── Validation ────────────────────────────────────────────────────────

        /// <summary>
        /// ตรวจว่า mapping ของ Template ครบและถูกต้อง — ใช้ชุดเดียวกันทั้งตอนบันทึก Template และตอน Upload ไฟล์:
        /// ต้องเลือกประเภทไฟล์ (LayoutType) และประเภทบทความ (ArticleType), map ArticleCode (Code รวม), Template ที่ไม่ใช่กฎหมาย
        /// ต้อง map Title/ContentHTML/Attachment (ไม่มีตำแหน่งคอลัมน์ตายตัว), รูปแบบ {X} ถูกต้อง และไม่ map คอลัมน์ Excel เดียวกันซ้ำ
        /// คืนรายการปัญหา (ว่าง = ใช้งานได้)
        /// </summary>
        public IList<string> GetTemplateProblems(IList<KBImportTemplateColumnRow> rows)
        {
            var problems = new List<string>();
            if (rows == null)
            {
                problems.Add("ไม่พบข้อมูล mapping ของ Template");
                return problems;
            }

            // CategoryID ที่ Template เก่าเคยบันทึกไว้ไม่นับ (ระบบหาเองตอน import)
            var mapped = rows
                .Where(r => r != null
                         && !string.IsNullOrWhiteSpace(r.ExcelColumnRef)
                         && !string.Equals(r.TargetColumnName, DerivedTargetColumn, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Func<string, bool> isMapped = name =>
                mapped.Any(r => string.Equals(r.TargetColumnName, name, StringComparison.OrdinalIgnoreCase));

            var layoutRow = mapped.FirstOrDefault(r => string.Equals(
                r.TargetColumnName, KBArticleColumnCatalog.LayoutTypeTargetColumn, StringComparison.OrdinalIgnoreCase));
            string layoutType = layoutRow != null ? KBImportLayoutTypeCatalog.Normalize(layoutRow.ExcelColumnRef) : null;
            if (layoutType == null)
                problems.Add("ยังไม่ได้เลือกประเภทไฟล์ (LayoutType: กฎหมาย / ไม่ใช่กฎหมาย)");

            if (!isMapped("ArticleType"))
                problems.Add("ยังไม่ได้เลือกประเภทบทความ (ArticleType)");

            // ArticleCode (Code รวม 29 หลัก) ใช้ตัดสินประเภทไฟล์ และหาหมวด/กฎหมายแม่ของแต่ละแถวตอน import — ต้อง map เสมอ
            if (!isMapped("ArticleCode"))
                problems.Add("ยังไม่ได้ Map คอลัมน์ ArticleCode (Code รวม)");

            // Template ที่ไม่ใช่กฎหมายไม่มีตำแหน่งคอลัมน์ตายตัวสำหรับ Title/ContentHTML/ไฟล์แนบ — ต้อง map เอง
            if (layoutType == KBImportLayoutTypeCatalog.NonLaw)
            {
                foreach (var required in new[] { "Title", "ContentHTML", KBArticleColumnCatalog.AttachmentTargetColumn })
                {
                    if (!isMapped(required))
                        problems.Add(string.Format("Template ที่ไม่ใช่กฎหมายต้อง Map คอลัมน์ {0}", required));
                }
            }

            // LawReference (Code กฎหมายอ้างอิง ของพระราชกฤษฎีกา) ใช้ได้เฉพาะไฟล์ประเภทกฎหมาย — ไฟล์ที่ไม่ใช่กฎหมายหากฎหมายแม่จาก 24 ตัวแรกของ Code รวมอยู่แล้ว
            if (layoutType == KBImportLayoutTypeCatalog.NonLaw && isMapped(KBArticleColumnCatalog.LawReferenceTargetColumn))
                problems.Add(string.Format("คอลัมน์ {0} ใช้ได้เฉพาะ Template ประเภทกฎหมาย (พระราชกฤษฎีกา) — Template ที่ไม่ใช่กฎหมายไม่ต้อง Map", KBArticleColumnCatalog.LawReferenceTargetColumn));

            foreach (var row in mapped)
            {
                string formulaError;
                if (!TryValidateFormulaSyntax(row.ExcelColumnRef, out formulaError))
                    problems.Add(string.Format("คอลัมน์ \"{0}\": {1}", row.TargetColumnName, formulaError));
            }

            var duplicates = mapped
                .Where(r => !FixedValueTargetColumns.Contains(r.TargetColumnName)) // LayoutType/ArticleType เป็นค่าคงที่ ไม่ใช่การใช้คอลัมน์ Excel จึงไม่นับซ้ำ
                .Where(r => !IsFormulaMapping(r.ExcelColumnRef)) // formula เช่น "มาตรา {C}" ใช้คอลัมน์ต้นทางร่วมกันได้ (เช่น Title/Tags ของกฎหมาย) จึงไม่นับซ้ำ
                .GroupBy(r => r.ExcelColumnRef.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicates.Count > 0)
                problems.Add("มีการ Map คอลัมน์ Excel ซ้ำกัน: " + string.Join(", ", duplicates));

            return problems;
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

        // ── LayoutType ของ Template เก่า (ผูกกับ 1 หมวด) ────────────────────────

        /// <summary>
        /// Derive ค่า LayoutType ("Law"/"NonLaw") จาก CategoryID ที่ Template เก่าเคยผูกไว้ โดยไล่
        /// <see cref="KBCategory.ParentCategoryID"/> ขึ้นไปจนกว่าจะเจอหมวด "กฎหมาย" (LW) — เจอแปลว่า Law, ไม่เจอแปลว่า NonLaw
        /// คืน <c>null</c> ถ้าไม่พบ category นั้นแล้ว — ใช้เป็น fallback ของ <see cref="GetTemplateColumnGrid"/> เท่านั้น
        /// เพราะ Template ใหม่เก็บประเภทไฟล์เองเป็นแถว LayoutType และไม่ผูกหมวดอีกแล้ว
        /// </summary>
        private string DeriveLayoutType(int categoryId)
        {
            var byId = _categoryRepo.GetAll().ToDictionary(c => c.CategoryID);

            var visited = new HashSet<int>();
            int current = categoryId;
            KBCategory cat;
            while (byId.TryGetValue(current, out cat))
            {
                if (string.Equals(cat.CategoryCode, KBImportLayoutTypeCatalog.LawRootCategoryCode, StringComparison.OrdinalIgnoreCase))
                    return KBImportLayoutTypeCatalog.Law;
                if (!cat.ParentCategoryID.HasValue || !visited.Add(current))
                    return KBImportLayoutTypeCatalog.NonLaw;
                current = cat.ParentCategoryID.Value;
            }
            return null;
        }
    }
}
