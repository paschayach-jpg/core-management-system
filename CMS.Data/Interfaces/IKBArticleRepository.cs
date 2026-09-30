using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IKBArticleRepository
    {
        IEnumerable<KBArticle> GetAll();
        IEnumerable<KBArticle> GetByCategory(int categoryId);
        IEnumerable<KBArticle> Search(int? categoryId, byte? status, byte? articleType);
        KBArticle GetById(int articleId);
        int Insert(KBArticle article);

        /// <summary>
        /// Insert KBArticle และ KBVersion (VersionNumber จาก <paramref name="version"/>) แรกของบทความนั้น
        /// ในธุรกรรมเดียวกัน — ถ้า version insert ล้มเหลว article insert จะถูก rollback ไปด้วย ป้องกันไม่ให้
        /// เหลือ KBArticle ที่ไม่มี KBVersion คู่กัน (orphan) คืนค่า ArticleID ใหม่
        /// </summary>
        int InsertWithVersion(KBArticle article, KBVersion version);
        void Update(KBArticle article);
        void UpdateStatus(int articleId, byte status, string reviewedBy);
        void IncrementViewCount(int articleId);
        void IncrementLikeCount(int articleId);

        /// <summary>คืน sequence ถัดไปสำหรับปีที่กำหนด เพื่อสร้าง ArticleCode รูปแบบ KB-yyyy-NNN</summary>
        int GetNextCodeSequence(int year);

        /// <summary>true เมื่อมี ArticleCode นี้อยู่แล้ว (ใช้ validate ก่อน insert ตอน import)</summary>
        bool ExistsByCode(string articleCode);

        /// <summary>ค้นบทความด้วย ArticleCode แบบตรงตัวเป๊ะ (exact match) คืน null ถ้าไม่พบ</summary>
        KBArticle GetByCode(string articleCode);

        /// <summary>
        /// ค้นบทความมาตราด้วย TitleCategory โดยตัดช่องว่างทั้งหมดออกทั้งสองฝั่งก่อนเทียบแบบตรงตัว
        /// เช่น key "มาตรา108/4" จะตรงกับ TitleCategory "มาตรา108 /4"
        /// คืนเฉพาะ ArticleID, ArticleCode, TitleCategory
        /// </summary>
        IEnumerable<KBArticle> FindByNormalizedTitleCategory(string normalizedKey);

        void SoftDelete(int articleId);
        void UpdateExpiryDate(int articleId, DateTime? expiryDate);

        /// <summary>
        /// คืนรายชื่อคอลัมน์จริงของ KMS.t_KBArticle ที่ใช้เป็นเป้าหมาย mapping ในหน้า config Excel Import Template
        /// (query สดจาก INFORMATION_SCHEMA.COLUMNS แล้วตัดคอลัมน์ audit/system ออก)
        /// เรียงตาม ORDINAL_POSITION ของตารางจริง
        /// </summary>
        IList<string> GetImportableTargetColumns();
    }
}
