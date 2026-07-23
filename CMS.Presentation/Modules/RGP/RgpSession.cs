using CMS.Domain.Models;

namespace CMS.Presentation.Modules.RGP
{
    /// <summary>
    /// เก็บ UserContext ปัจจุบันสำหรับ Form ลูกของโมดูล Project (RGP) ที่ยังไม่ได้รับ
    /// dependency ผ่าน constructor จากทุกจุด (เทียบเท่ากับ LoginManager.Instance ที่ RG_Project ใช้เดิม
    /// แต่เปลี่ยนมาถือ CMS.Domain.Models.UserContext ตัวจริงแทน employee code string)
    /// ตั้งค่าครั้งเดียวตอนเปิด Frm_Project_List จาก MDI_Container.OpenProjectManagement()
    /// ทางที่ถูกต้องกว่าคือ thread ผ่าน constructor ทุกจุด — งานที่เหลือในอนาคต
    /// </summary>
    internal static class RgpSession
    {
        public static UserContext Current { get; set; }
    }
}
