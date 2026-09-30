using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IKBImportProcessorService
    {
        /// <param name="resolvedCategoryId">
        /// CategoryID configured on the KB Import Template selected in the Wizard — applied to
        /// every row in this batch (both Law and NonLaw layouts). Must be greater than 0.
        /// </param>
        void ValidateRows(IList<KBImportRow> rows, int resolvedCategoryId, string templateCategoryCode);

        /// <summary>
        /// ตรวจระดับไฟล์ว่า Code รวมในไฟล์เป็นของหมวดเดียวกับ Template ที่เลือกหรือไม่ (Law: prefix 3 ตัวแรก เช่น RVA,
        /// NonLaw: ตัวที่ 23 → AN/OR/SO/RG/CI/MR) คืนข้อความอธิบายเมื่อ "ทุกแถวที่อ่านรหัสได้" ไม่ตรงกับ Template
        /// (ไฟล์ผิด Template ทั้งไฟล์) คืน null ถ้าตรงกัน หรือยังสรุปไม่ได้ (ไม่มีแถวที่อ่านรหัสได้) — ปล่อยให้ ValidateRows รายงานรายแถวต่อ
        /// </summary>
        string GetTemplateMismatchMessage(IList<KBImportRow> rows, string templateCategoryCode);

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
