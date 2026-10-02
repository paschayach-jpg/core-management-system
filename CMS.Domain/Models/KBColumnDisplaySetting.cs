namespace CMS.Domain.Models
{
    /// <summary>
    /// ค่า header label + การซ่อน/แสดงของ 1 slot ที่ resolve แล้วสำหรับ CategoryID หนึ่งตัว (ผลลัพธ์ของ
    /// KBArticleDisplayProfileService.GetEffectiveDisplaySettings) — ใช้ตั้งค่า dgvArticles column
    /// header/Visible และ ArticleCardControl label/Visible ตรงๆ
    /// </summary>
    public class KBColumnDisplaySetting
    {
        public string Label   { get; set; }
        public bool   Visible { get; set; }

        public KBColumnDisplaySetting() { }

        public KBColumnDisplaySetting(string label, bool visible)
        {
            Label   = label;
            Visible = visible;
        }
    }
}
