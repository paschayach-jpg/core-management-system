namespace CMS.Domain.Models
{
    /// <summary>ตัวเลือกประเภทไฟล์ของ Template ("Law"/"Decree"/"NonLaw") สำหรับ dropdown ในหน้า config Excel Import Template</summary>
    public class KBImportLayoutOption
    {
        /// <summary>ค่า LayoutType — ต้องตรงกับ KBImportLayoutTypeCatalog.Law / Decree / NonLaw (ค่าว่าง = ยังไม่ได้เลือก)</summary>
        public string LayoutType { get; set; }

        public string Description { get; set; }
    }
}
