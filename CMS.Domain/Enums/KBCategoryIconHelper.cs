namespace CMS.Domain.Enums
{
    /// <summary>
    /// Maps <see cref="KBCategoryIcon"/> enum values to their display emoji symbols.
    /// </summary>
    public static class KBCategoryIconHelper
    {
        public static string ToSymbol(KBCategoryIcon icon)
        {
            switch (icon)
            {
                case KBCategoryIcon.Question:       return "❓";
                case KBCategoryIcon.SopList:        return "📋";
                case KBCategoryIcon.Document:       return "📄";
                case KBCategoryIcon.ItSystem:       return "💻";
                case KBCategoryIcon.Manual:         return "📖";
                case KBCategoryIcon.Folder:         return "📂";
                case KBCategoryIcon.RuleRegulation: return "⚖️";
                case KBCategoryIcon.Announcement:   return "🔔";
                case KBCategoryIcon.Knowledge:      return "💡";
                default:                            return "📂";
            }
        }
    }
}
