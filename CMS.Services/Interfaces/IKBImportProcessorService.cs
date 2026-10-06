using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IKBImportProcessorService
    {
        /// <summary>
        /// ตรวจแต่ละแถวและตั้ง <see cref="KBImportRow.Status"/>/<see cref="KBImportRow.StatusNote"/> — CategoryID ของแต่ละแถว
        /// หาจาก ArticleCode ของแถวนั้น (กฎหมาย: 5 ตัวแรกของ Code รวม, ไม่ใช่กฎหมาย: ตัวที่ 25) ไม่ผูกกับ Template
        /// จึงรับได้หลายกฎหมาย/หลายประเภทในไฟล์เดียว แถวที่หาหมวดไม่ได้เป็น Error รายแถว.
        /// ArticleCode ที่มีในระบบแล้ว (ทั้งกฎหมายและไม่ใช่กฎหมาย) ไม่ใช่ Error/Skip — ตั้ง <see cref="KBImportRow.ExistingArticleId"/>
        /// แล้วเป็น Valid (จะ UPDATE บทความเดิมตอน import; หน้า preview บอกว่าจะอัปเดต field ไหน)
        /// </summary>
        void ValidateRows(IList<KBImportRow> rows);

        /// <summary>
        /// insert บทความของแถวที่ Valid (Law: บทความ + Version, NonLaw: บทความ + ไฟล์แนบ + link). แถวที่
        /// <see cref="KBImportRow.ExistingArticleId"/> &gt; 0 ไม่ insert ซ้ำ แต่ UPDATE ทุก field ที่ Excel/Template ให้ค่ามา (ไม่แก้ ArticleCode/CategoryID) และตั้ง
        /// UpdatedBy = 'system', UpdatedDate = ตอนนี้ (กฎหมาย: เพิ่ม version ใหม่ถ้าเนื้อหาเปลี่ยน; ไม่ใช่กฎหมาย: ไฟล์แนบที่มีอยู่แล้วไม่
        /// ดาวน์โหลด/อัปโหลดซ้ำ, ยังไม่มีไฟล์และโหลดไม่ได้ → Status = "Skip" เฉพาะ ArticleCode นั้น). แต่ละแถวล้มเหลวแยกกัน
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
        /// <param name="attachmentFolder">
        /// NonLaw rows only: โฟลเดอร์ในเครื่องที่ผู้ใช้เลือกในหน้า import — ไฟล์แนบหาตามชื่อไฟล์ที่ Template map เป็น Attachment
        /// (ชื่อต้องตรงกับไฟล์ในโฟลเดอร์นี้ครบนามสกุล). ว่าง = แถวที่ต้องมีไฟล์ล้มเหลว/ข้ามพร้อมข้อความ "ยังไม่ได้เลือกโฟลเดอร์ไฟล์แนบ".
        /// ไม่ใช้กับแถวกฎหมาย (ไม่มีไฟล์แนบ)
        /// </param>
        /// <param name="dmsFolderId">
        /// NonLaw rows only: โฟลเดอร์ปลายทางใน DMS ที่ผู้ใช้เลือกเก็บไฟล์แนบ (Controller ตรวจสิทธิ์ก่อนส่งมา); null = ใช้ค่าเริ่มต้นของระบบ
        /// </param>
        void InsertArticles(
            IList<KBImportRow> rows,
            UserContext user,
            bool skipRowOnDownloadFailure,
            byte resolvedArticleType,
            string attachmentFolder,
            int? dmsFolderId,
            out int successCount,
            out int errorCount,
            Action<int, int> onProgress);
    }
}
