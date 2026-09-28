namespace CMS.Domain.Models
{
    /// <summary>ตัวเลือก ArticleType คงที่ (1-5) สำหรับ dropdown ในหน้า config Excel Import Template</summary>
    public class KBArticleTypeOption
    {
        public byte ArticleType { get; set; }
        public string Description { get; set; }
    }
}
