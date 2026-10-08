using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// ผลการอ่านไฟล์ Excel ของ KB Import Wizard: แถวข้อมูล + layout ที่ตรวจพบจากไฟล์ + รายการ mapping ของ Template
    /// ที่ชี้ไปคอลัมน์/หัวตารางที่ไม่มีในไฟล์ (ใช้ตรวจว่าไฟล์ตรงกับ Template ก่อนเข้าสู่การ validate รายแถว)
    /// </summary>
    public class KBExcelReadResult
    {
        public IList<KBImportRow> Rows { get; set; }

        /// <summary>
        /// "Law", "Decree" หรือ "NonLaw" (KBImportLayoutTypeCatalog) — ชนิดไฟล์ที่ตรวจพบ ใช้เทียบกับ LayoutType ของ Template.
        /// "Decree" = ไฟล์รูปแบบกฎหมายที่ตัวที่ 4-5 ของ Code รวมไม่ใช่ "00" (<see cref="KBImportLayoutTypeCatalog.IsDecreeCode"/>); แถวของไฟล์นี้ยังมี <see cref="KBImportRow.RowKind"/> = "Law"
        /// </summary>
        public string DetectedLayout { get; set; }

        /// <summary>
        /// mapping ของ Template ที่ resolve ไม่ได้ในไฟล์นี้ (เช่น map ไปคอลัมน์ Z แต่ไฟล์มีถึงคอลัมน์ L) —
        /// ว่างเปล่า = ทุก mapping ชี้ไปคอลัมน์/หัวตารางที่มีอยู่จริง
        /// </summary>
        public IList<string> MappingProblems { get; set; }

        public KBExcelReadResult()
        {
            Rows            = new List<KBImportRow>();
            MappingProblems = new List<string>();
        }
    }
}
