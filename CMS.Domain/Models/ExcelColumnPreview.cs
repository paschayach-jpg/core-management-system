namespace CMS.Domain.Models
{
    /// <summary>
    /// คอลัมน์ Excel หนึ่งคอลัมน์ที่อ่านจากไฟล์ template ที่ admin อัปโหลดในหน้า config
    /// Excel Import Template — ใช้แสดงให้คลิกเลือกเป็น mapping (ไม่ persist ลง DB)
    /// </summary>
    public class ExcelColumnPreview
    {
        /// <summary>ลำดับคอลัมน์แบบ 0-based ตามที่อยู่ในไฟล์ Excel</summary>
        public int ColumnIndex { get; set; }

        /// <summary>ตัวอักษรคอลัมน์ เช่น "A", "B", ... "AA"</summary>
        public string ColumnRef { get; set; }

        /// <summary>ข้อความในแถวหัวตาราง (แถวแรกของ sheet) ถ้ามี</summary>
        public string HeaderText { get; set; }

        /// <summary>ค่าตัวอย่างจากแถวข้อมูลแถวแรก (แถวที่สองของ sheet) สำหรับให้ admin ดูประกอบการเลือก</summary>
        public string SampleValue { get; set; }
    }
}
