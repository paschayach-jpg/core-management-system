namespace CMS.Domain.Models
{
    /// <summary>ระดับตำแหน่ง จาก EMS.m_PositionLevel</summary>
    public class HrPositionLevel
    {
        public int    LevelID   { get; set; }
        public string LevelCode { get; set; }
        public string LevelName { get; set; }
        public bool   IsActive  { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(LevelName)
                ? LevelCode
                : string.Format("{0} — {1}", LevelCode, LevelName);
        }
    }
}

