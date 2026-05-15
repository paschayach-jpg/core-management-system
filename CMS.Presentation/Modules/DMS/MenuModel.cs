namespace CMS.Presentation
{
    /// <summary>
    /// Presentation-layer แสดงผล menu action ที่ถูก flatten มาจาก UserMenuModel
    /// DTOs ที่ใช้ติดต่อ API จริง (UserMenuModel, MenuItemDto ฯลฯ) อยู่ใน CMS.Services.DTOs
    /// </summary>
    public class MenuAction
    {
        public string MenuDesc   { get; set; }
        public string MenuCode   { get; set; }
        public string ActionDesc { get; set; }
        public string IsChecked  { get; set; }
        public string MenuType   { get; set; }
    }
}
