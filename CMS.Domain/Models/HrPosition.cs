namespace CMS.Domain.Models
{
    /// <summary>ตำแหน่งงาน จาก core.HR_Position</summary>
    public class HrPosition
    {
        public int    PositionID    { get; set; }
        public string PositionCode  { get; set; }
        public string PositionName  { get; set; }

        /// <summary>ระดับตำแหน่ง เช่น ระดับ 1, 2, 3 (ตัวเลข)</summary>
        public string PositionLevel { get; set; }

        /// <summary>รหัสระดับ เช่น 1A, 2B, 3A (ใช้ match FolderAuthorityConfig)</summary>
        public string LevelCode     { get; set; }

        /// <summary>Level เดิม (legacy alias)</summary>
        public string Level
        {
            get { return !string.IsNullOrEmpty(LevelCode) ? LevelCode : PositionLevel; }
            set { LevelCode = value; }
        }

        public bool   IsActive      { get; set; }

        public override string ToString()
        {
            string lvl = !string.IsNullOrEmpty(LevelCode) ? LevelCode
                       : !string.IsNullOrEmpty(PositionLevel) ? PositionLevel : "";
            return string.IsNullOrEmpty(lvl)
                ? PositionName ?? ""
                : string.Format("{0}  [{1}]", PositionName, lvl);
        }
    }
}
