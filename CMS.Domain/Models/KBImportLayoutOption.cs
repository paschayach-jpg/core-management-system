namespace CMS.Domain.Models
{
    /// <summary>ตัวเลือกประเภทไฟล์ของ Template ("Law"/"NonLaw") สำหรับ dropdown ในหน้า config Excel Import Template</summary>
    public class KBImportLayoutOption
    {
        /// <summary>ค่า LayoutType — ต้องตรงกับ KBImportLayoutTypeCatalog.Law / NonLaw (ค่าว่าง = ยังไม่ได้เลือก)</summary>
        public string LayoutType { get; set; }

        public string Description { get; set; }
    }
}
