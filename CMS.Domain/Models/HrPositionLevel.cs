namespace CMS.Domain.Models
{
    /// <summary>ระดับตำแหน่ง จาก EMS.m_Position</summary>
    public class HrPositionLevel
    {
        public int     LevelID       { get; set; }
        public string  LevelCode     { get; set; }
        public string  LevelName     { get; set; }
        public string  LevelCategory { get; set; }
        public int?    SortOrder     { get; set; }
        public bool    IsActive      { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(LevelName) || string.Equals(LevelCode, LevelName, System.StringComparison.OrdinalIgnoreCase)
                ? (LevelCode ?? "")
                : string.Format("{0} — {1}", LevelCode, LevelName);
        }
    }
}
