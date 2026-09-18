using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>ตรวจสอบและนำเข้าข้อมูล ประกาศ/คำสั่ง จาก Excel</summary>
    public interface IKBOrderImportProcessorService
    {
        /// <summary>
        /// ตรวจความถูกต้องของแต่ละแถว (เขียนผลลง Status/StatusNote ของแถวนั้น)
        /// ต้องเรียก ResolveSectionGroups ก่อน แล้วส่ง groups เข้ามาด้วย
        /// </summary>
        void ValidateRows(IList<KBOrderImportRow> rows, IList<KBSectionGroup> sectionGroups);

        /// <summary>
        /// รวบรวมมาตราทั้งหมดที่อ้างถึงในไฟล์ แล้วค้นบทความมาตราที่จับคู่ได้จาก TitleCategory
        /// ถ้ามาตราไหนเจอ record เดียวจะเลือกให้อัตโนมัติ
        /// </summary>
        IList<KBSectionGroup> ResolveSectionGroups(IList<KBOrderImportRow> rows);

        /// <summary>
        /// นำเข้าบทความ + ไฟล์แนบ + link มาตรา
        /// </summary>
        /// <param name="skipRowOnDownloadFailure">true = ถ้าโหลดไฟล์แนบไม่สำเร็จให้ข้ามทั้งแถว, false = นำเข้าบทความโดยไม่มีไฟล์แนบ</param>
        void InsertArticles(
            IList<KBOrderImportRow> rows,
            IList<KBSectionGroup>   sectionGroups,
            bool                    skipRowOnDownloadFailure,
            UserContext             user,
            out int                 successCount,
            out int                 errorCount,
            Action<int, int>        onProgress);
    }
}
