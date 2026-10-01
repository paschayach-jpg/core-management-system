using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IKBImportProcessorService
    {
        /// <summary>
        /// ตรวจแต่ละแถวและตั้ง <see cref="KBImportRow.Status"/>/<see cref="KBImportRow.StatusNote"/> — CategoryID ของแต่ละแถว
        /// หาจาก ArticleCode ของแถวนั้น (กฎหมาย: 3 ตัวแรกของ Code รวม, ไม่ใช่กฎหมาย: ตัวที่ 23) ไม่ผูกกับ Template
        /// จึงรับได้หลายกฎหมาย/หลายประเภทในไฟล์เดียว แถวที่หาหมวดไม่ได้เป็น Error รายแถว.
        /// NonLaw: ArticleCode ที่มีในระบบแล้วไม่ใช่ Error — ตั้ง <see cref="KBImportRow.ExistingArticleId"/> แล้วเป็น Skip (ครบแล้ว)
        /// หรือ Valid (มีไฟล์แนบ/link ที่ต้องเติม)
        /// </summary>
        void ValidateRows(IList<KBImportRow> rows);

        /// <summary>
        /// insert บทความของแถวที่ Valid (Law: บทความ + Version, NonLaw: บทความ + ไฟล์แนบ + link). แถว NonLaw ที่
        /// <see cref="KBImportRow.ExistingArticleId"/> &gt; 0 ไม่ insert ซ้ำ แต่เติมไฟล์แนบ/link ที่ตกหล่นให้บทความเดิม
        /// (ไฟล์ต้นทางไม่พบ/ไม่มีของให้เติม → Status = "Skip" เฉพาะ ArticleCode นั้น). แต่ละแถวล้มเหลวแยกกัน
        /// </summary>
        /// <param name="user">
        /// Current user — needed (not just a "createdBy" string) because NonLaw rows upload
        /// attachment files via <c>IFileService</c>, which requires the full <c>UserContext</c>.
        /// </param>
        /// <param name="skipRowOnDownloadFailure">
        /// NonLaw rows only: when an attachment download/upload fails, <c>true</c> fails the
        /// whole row (no insert); <c>false</c> inserts the article anyway with a warning note.
        /// Ignored for Law rows (no attachment involved).
        /// </param>
        /// <param name="resolvedArticleType">
        /// ArticleType configured on the KB Import Template selected in the Wizard — applied to
        /// every row in this batch (both Law and NonLaw layouts). Must be greater than 0.
        /// </param>
        void InsertArticles(
            IList<KBImportRow> rows,
            UserContext user,
            bool skipRowOnDownloadFailure,
            byte resolvedArticleType,
            out int successCount,
            out int errorCount,
            Action<int, int> onProgress);
    }
}
