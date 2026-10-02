using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Business Logic สำหรับจัดการ Article Display Profile ของ Knowledge Base (MOD-006)
    /// ครอบ IKBArticleDisplayProfileRepository + IKBCategoryRepository และเพิ่ม Validation + Audit Logging
    /// </summary>
    public class KBArticleDisplayProfileService : IKBArticleDisplayProfileService
    {
        /// <summary>ความยาวสูงสุดของคำอธิบาย slot — ตรงกับ KMS.m_KBArticleDisplayProfileColumn.SlotDescription NVARCHAR(500)</summary>
        public const int SlotDescriptionMaxLength = 500;

        private readonly IKBArticleDisplayProfileRepository _repo;
        private readonly IKBCategoryRepository               _categoryRepo;
        private readonly IAuditService                       _auditService;

        public KBArticleDisplayProfileService(
            IKBArticleDisplayProfileRepository repo,
            IKBCategoryRepository              categoryRepo,
            IAuditService                      auditService)
        {
            if (repo         == null) throw new ArgumentNullException("repo");
            if (categoryRepo == null) throw new ArgumentNullException("categoryRepo");
            if (auditService == null) throw new ArgumentNullException("auditService");
            _repo         = repo;
            _categoryRepo = categoryRepo;
            _auditService = auditService;
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public IEnumerable<KBArticleDisplayProfile> GetAllProfiles()
        {
            return _repo.GetAll();
        }

        public KBArticleDisplayProfile GetProfileById(int profileId)
        {
            return _repo.GetById(profileId);
        }

        public IList<KBArticleDisplayProfileColumnRow> GetProfileColumnGrid(int? profileId)
        {
            var saved = profileId.HasValue
                ? _repo.GetColumnsByProfileId(profileId.Value)
                : new List<KBArticleDisplayProfileColumn>();
            var savedBySlot = saved.ToDictionary(c => c.SlotKey, StringComparer.OrdinalIgnoreCase);

            var rows = new List<KBArticleDisplayProfileColumnRow>();
            foreach (var slotKey in KBArticleDisplaySlotCatalog.GetSlotKeys())
            {
                KBArticleDisplayProfileColumn col;
                savedBySlot.TryGetValue(slotKey, out col);

                rows.Add(new KBArticleDisplayProfileColumnRow
                {
                    SlotKey         = slotKey,
                    // ค่าที่ผู้ใช้ตั้งไว้ใน DB; NULL (Profile ใหม่ / แถวที่ยังไม่เคยบันทึก) ใช้ค่าเริ่มต้นจาก catalog
                    SlotDescription = col != null && col.SlotDescription != null
                        ? col.SlotDescription
                        : KBArticleDisplaySlotCatalog.GetDescription(slotKey),
                    DisplayLabel    = col != null ? col.DisplayLabel : KBArticleDisplaySlotCatalog.GetDefaultLabel(slotKey),
                    IsVisible       = col != null ? col.IsVisible : KBArticleDisplaySlotCatalog.GetDefaultVisible(slotKey),
                });
            }
            return rows;
        }

        public IList<KBCategory> GetCategoriesUsingProfile(int profileId)
        {
            return _categoryRepo.GetAll()
                .Where(c => c.DisplayProfileID.HasValue && c.DisplayProfileID.Value == profileId)
                .ToList();
        }

        public IDictionary<string, KBColumnDisplaySetting> GetEffectiveDisplaySettings(int categoryId)
        {
            var result = KBArticleDisplaySlotCatalog.GetAllDefaults();

            var byId = _categoryRepo.GetAll().ToDictionary(c => c.CategoryID);

            var visited = new HashSet<int>();
            int current = categoryId;
            KBCategory cat;
            int? resolvedProfileId = null;
            while (byId.TryGetValue(current, out cat))
            {
                // Profile ที่ปิดใช้งานแล้ว (หรือถูกลบไปแล้ว) ถือว่าเหมือนไม่ได้ตั้งค่าไว้เลยที่ระดับนี้ —
                // เดินขึ้นไปหา ancestor ถัดไปต่อ ไม่ใช่หยุดค้างอยู่ที่ profile ที่ปิดไปแล้ว
                if (cat.DisplayProfileID.HasValue)
                {
                    var candidateProfile = _repo.GetById(cat.DisplayProfileID.Value);
                    if (candidateProfile != null && candidateProfile.IsActive)
                    {
                        resolvedProfileId = cat.DisplayProfileID.Value;
                        break;
                    }
                }
                if (!cat.ParentCategoryID.HasValue || !visited.Add(current))
                    break;
                current = cat.ParentCategoryID.Value;
            }

            // ไล่ขึ้นไปจนสุดสาย (ไม่มี parent แล้ว) โดยไม่เจอ profile ที่ยัง active เลย — ไม่ error, กลับไปใช้
            // default ตายตัวเดิมทั้งหมด (ผลลัพธ์เดียวกับกรณีไม่มีใครตั้ง Profile ไว้เลยตั้งแต่แรก)
            if (!resolvedProfileId.HasValue)
                return result;

            foreach (var col in _repo.GetColumnsByProfileId(resolvedProfileId.Value))
                result[col.SlotKey] = new KBColumnDisplaySetting(col.DisplayLabel, col.IsVisible);

            return result;
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public int SaveProfile(KBArticleDisplayProfile profile, IList<KBArticleDisplayProfileColumnRow> rows, UserContext user)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            if (rows == null) throw new ArgumentNullException("rows");
            if (user == null) throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(profile.ProfileName))
                throw new ArgumentException("ProfileName ต้องไม่ว่าง");
            if (_repo.ExistsByName(profile.ProfileName.Trim(), profile.ProfileID))
                throw new InvalidOperationException(string.Format("มี Profile ชื่อ \"{0}\" อยู่แล้ว", profile.ProfileName.Trim()));

            foreach (var row in rows)
            {
                string description = (row.SlotDescription ?? string.Empty).Trim();
                if (description.Length > SlotDescriptionMaxLength)
                    throw new ArgumentException(string.Format("คำอธิบายของ '{0}' ยาวเกิน {1} ตัวอักษร (พบ {2} ตัว)",
                        row.SlotKey, SlotDescriptionMaxLength, description.Length));
            }

            bool isNew = profile.ProfileID <= 0;
            profile.ProfileName = profile.ProfileName.Trim();
            if (isNew) profile.CreatedBy = user.AdIdentity;

            var columns = rows.Select(r => new KBArticleDisplayProfileColumn
            {
                ProfileID    = profile.ProfileID,
                SlotKey         = r.SlotKey,
                SlotDescription = (r.SlotDescription ?? string.Empty).Trim(),
                DisplayLabel    = r.DisplayLabel ?? string.Empty,
                IsVisible    = r.IsVisible,
            }).ToList();

            int profileId = _repo.SaveProfileWithColumns(profile, columns);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = isNew ? "CREATE_KB_ARTICLE_DISPLAY_PROFILE" : "UPDATE_KB_ARTICLE_DISPLAY_PROFILE",
                UserID    = user.UserID,
                UserName  = user.AdIdentity,
                TableName = "KMS.m_KBArticleDisplayProfile",
                RecordID  = profileId,
                AuditDate = DateTime.Now,
                Result    = 1
            });

            return profileId;
        }

        public void DeleteProfile(int profileId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            _repo.Deactivate(profileId);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "DEACTIVATE_KB_ARTICLE_DISPLAY_PROFILE",
                UserID    = user.UserID,
                UserName  = user.AdIdentity,
                TableName = "KMS.m_KBArticleDisplayProfile",
                RecordID  = profileId,
                AuditDate = DateTime.Now,
                Result    = 1
            });
        }

        public void ActivateProfile(int profileId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            _repo.Activate(profileId);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "ACTIVATE_KB_ARTICLE_DISPLAY_PROFILE",
                UserID    = user.UserID,
                UserName  = user.AdIdentity,
                TableName = "KMS.m_KBArticleDisplayProfile",
                RecordID  = profileId,
                AuditDate = DateTime.Now,
                Result    = 1
            });
        }

        public void AssignProfileToCategory(int categoryId, int? profileId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var category = _categoryRepo.GetById(categoryId);
            if (category == null)
                throw new InvalidOperationException(string.Format("ไม่พบ CategoryID {0} ในระบบ", categoryId));

            category.DisplayProfileID = profileId;
            _categoryRepo.Update(category);

            _auditService.Log(new AuditEntry
            {
                Module    = "KnowledgeBase",
                Action    = "ASSIGN_KB_ARTICLE_DISPLAY_PROFILE",
                UserID    = user.UserID,
                UserName  = user.AdIdentity,
                TableName = "KMS.m_KBCategory",
                RecordID  = categoryId,
                AuditDate = DateTime.Now,
                Result    = 1
            });
        }
    }
}
