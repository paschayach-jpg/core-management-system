using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service interface สำหรับ Knowledge Base Article Management (MOD-006)
    /// </summary>
    public interface IKBArticleService
    {
        // ── Read ──────────────────────────────────────────────────────────────
        IEnumerable<KBArticle> GetAll();
        IEnumerable<KBArticle> GetByCategory(int categoryId);
        IEnumerable<KBArticle> Search(string keyword, int? categoryId, byte? status, byte? articleType);
        IEnumerable<KBArticle> SearchByType(string keyword, int? categoryId, byte? status, byte? articleType, int searchFieldId);

        /// <summary>
        /// ใช้ criteria การค้นหาแบบเดียวกับ <see cref="SearchByType"/> (searchFieldId 1-6) กับรายการ
        /// บทความที่โหลดมาแล้ว แทนที่จะ query ใหม่ตาม CategoryID — สำหรับกรณีที่รายการมาจากแหล่งอื่น เช่น
        /// กลุ่มบทความ cross-reference (มาตรา ↔ คำสั่ง) ที่ resolve จาก link graph
        /// </summary>
        IEnumerable<KBArticle> FilterByKeyword(IEnumerable<KBArticle> articles, string keyword, int searchFieldId);

        KBArticle GetById(int articleId);

        /// <summary>
        /// ค้นหาบทความที่เผยแพร่แล้ว (Status = 3) โดยใช้ Article Code หรือ Title
        /// ใช้สำหรับ inline search ใน KB Editor
        /// </summary>
        IEnumerable<KBArticle> SearchPublishedArticles(string searchText);

        // ── Write ─────────────────────────────────────────────────────────────
        /// <summary>
        /// สร้าง ArticleCode ถัดไปในรูปแบบ KB-YYYY-NNN (เช่น KB-2566-001)
        /// ใช้แสดง Preview ในฟอร์มก่อน Save จริง
        /// </summary>
        string GenerateNextArticleCode();

        /// <summary>สร้างบทความใหม่ คืน ArticleID (เริ่มต้นที่ Status=Draft) — ArticleCode ที่มีอยู่แล้ว (รวมบทความที่ถูกลบ) สร้างซ้ำไม่ได้</summary>
        int Create(KBArticle article);

        /// <summary>true ถ้า ArticleCode นี้มีในระบบแล้ว (นับบทความที่ถูกลบด้วย เหมือนที่ import ใช้)</summary>
        bool ArticleCodeExists(string articleCode);

        /// <summary>บทความตาม ArticleCode (ไม่รวมที่ถูกลบ); null = ไม่พบ</summary>
        KBArticle GetByCode(string articleCode);

        /// <summary>
        /// แก้ไขบทความจาก editor: เขียนเฉพาะฟิลด์ที่ editor แก้ได้ (ดู <c>IKBArticleRepository.Update</c>) — ไม่แตะหมวด/สถานะ/ReviewDueDate/IsOfficialPolicy;
        /// สร้าง Version Snapshot (และเลื่อน CurrentVersion) เฉพาะเมื่อ ContentHTML ต่างจากที่เก็บไว้ <paramref name="actionBy"/> = ผู้แก้ (UpdatedBy + audit)
        /// </summary>
        void Update(KBArticle article, string changeDescription, string actionBy);

        /// <summary>เปลี่ยน Status บทความ (Submit for Review / Publish / Archive)</summary>
        void ChangeStatus(int articleId, byte newStatus, string actionBy);

        // ── Version History ───────────────────────────────────────────────────
        IEnumerable<KBVersion> GetVersionHistory(int articleId);

        /// <summary>คืน VersionNumber ล่าสุดจาก t_KBVersion (0 ถ้ายังไม่มีประวัติ)</summary>
        int GetLatestVersionNumber(int articleId);

        // ── Article Attachments ───────────────────────────────────────────────
        /// <summary>ดึงไฟล์แนบของบทความ</summary>
        IEnumerable<KBArticleAttachment> GetArticleAttachments(int articleId);

        /// <summary>ดึงรายละเอียดของไฟล์ที่แนบกับบทความ</summary>
        IEnumerable<FileDocument> GetArticleAttachmentFiles(int articleId);

        /// <summary>ลบไฟล์แนบเดิมทั้งหมดแล้วบันทึกรายการใหม่ พร้อมบันทึก audit log</summary>
        void SaveArticleAttachments(int articleId, IEnumerable<int> fileIds, string actionBy);

        /// <summary>ลบไฟล์แนบเฉพาะรายการ (KMS.t_KBArticleAttachment) ด้วย ArticleID และ FileID</summary>
        void RemoveArticleAttachment(int articleId, int fileId);

        // ── Article Links ─────────────────────────────────────────────────────
        /// <summary>ดึง links ของบทความ optionally กรองด้วย entityType</summary>
        IEnumerable<KBArticleLink> GetArticleLinks(int articleId, byte? entityType = null);

        /// <summary>ดึง links ทั้งหมดในระบบสำหรับ entityType ที่ระบุ (ไม่กรอง ArticleID) ใช้สร้าง in-memory link graph</summary>
        IEnumerable<KBArticleLink> GetAllLinksByType(byte entityType);

        /// <summary>
        /// ปรับ link ชนิด "บทความที่เกี่ยวข้อง" (Law, EntityType 4) ของบทความให้ตรงกับรายการใหม่ <b>ทั้งสองทาง</b>: ตัวที่หายไปจากรายการถูกยกเลิกทั้งสองทาง
        /// ตัวที่เพิ่มถูกผูกทั้งสองทาง ตัวที่มีอยู่แล้วไม่ถูกแตะ (คง CreatedBy/CreatedDate) — link ชนิดอื่น (Task/Program/ไฟล์) ไม่ถูกแตะ
        /// </summary>
        void SaveArticleLinks(int articleId, IEnumerable<KBArticleLink> links);

        /// <summary>
        /// ผูกบทความสองบทความเข้าหากัน <b>ทั้งสองทาง</b> (Law, EntityType 4) เหมือนที่ import ทำกับมาตรา ↔ พระราชกฤษฎีกา/ประกาศ/คำสั่ง;
        /// ทิศไหนมีอยู่แล้วไม่เพิ่มซ้ำ
        /// </summary>
        void LinkArticles(int articleId, int otherArticleId, string createdBy);

        /// <summary>ยกเลิก link "บทความที่เกี่ยวข้อง" ระหว่างสองบทความทั้งสองทาง</summary>
        void UnlinkArticles(int articleId, int otherArticleId);

        /// <summary>เพิ่ม link เดียว (ถ้ามี link ชนิด/ปลายทางเดียวกันอยู่แล้วจะไม่เพิ่มซ้ำ)</summary>
        void AddArticleLink(KBArticleLink link);

        /// <summary>ลบ link ที่ตรงกับ articleId + entityId + entityType</summary>
        void RemoveArticleLink(int articleId, int entityId, byte entityType);

        // ── Engagement ────────────────────────────────────────────────────────
        void IncrementViewCount(int articleId);
        void IncrementLikeCount(int articleId);

        /// <summary>
        /// ลบบทความแบบ soft delete: ยกเลิก link "บทความที่เกี่ยวข้อง" ทั้งสองทางที่ชี้เข้า/ออกบทความนี้ (ไม่ให้หลงเหลือในรายการ related/ผังกฎหมายของบทความอื่น)
        /// แล้วตั้ง IsDelete + บันทึก audit <c>DELETE_KB_ARTICLE</c> — ไฟล์แนบ/DMS ไม่ถูกลบ (ไฟล์อาจใช้ร่วมกับบทความอื่น)
        /// </summary>
        void SoftDelete(int articleId, string actionBy);
        void UpdateExpiryDate(int articleId, DateTime? expiryDate);
    }
}
