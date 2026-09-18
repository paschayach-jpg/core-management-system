using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    /// <summary>View contract ของ wizard นำเข้า ประกาศ/คำสั่ง (3 step: upload → validate → confirm)</summary>
    public interface IKBOrderImportWizardView
    {
        // ── ค่าที่ user เลือกใน step 1–2 ──────────────────────────────────────
        string SelectedFilePath  { get; }
        string SelectedSheetName { get; }

        /// <summary>true = ถ้าโหลดไฟล์แนบไม่สำเร็จให้ข้ามทั้งแถว, false = นำเข้าโดยไม่มีไฟล์แนบ</summary>
        bool OptSkipRowOnDownloadFailure { get; }

        // ── สิ่งที่ presenter สั่งให้ view แสดง ────────────────────────────────
        void ShowSheetNames(IList<string> sheetNames);

        /// <summary>
        /// แสดงผล validate — view ต้องให้ user เลือกมาตราใน sectionGroups ที่มี Candidates มากกว่า 1
        /// โดยเขียนค่าลง KBSectionGroup.SelectedArticleId (presenter อ่านกลับตอน import)
        /// </summary>
        void ShowValidationResult(KBOrderImportResult result, IList<KBSectionGroup> sectionGroups);

        void ShowImportComplete(KBOrderImportResult summary);

        void ShowError(string message);
        void ShowInfo(string message);
        void SetStatus(string message);
        void SetProgress(int percent, string message);
    }
}
