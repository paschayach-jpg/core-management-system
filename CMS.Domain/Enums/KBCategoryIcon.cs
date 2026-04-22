namespace CMS.Domain.Enums
{
    /// <summary>
    /// Icon types for KB Categories (KMS.m_KBCategory.Icon)
    /// Maps to the icon dropdown in KBCategoryForm
    /// </summary>
    public enum KBCategoryIcon : byte
    {
        /// <summary>❓ Question</summary>
        Question = 1,

        /// <summary>📋 SOP List</summary>
        SopList = 2,

        /// <summary>📄 Document</summary>
        Document = 3,

        /// <summary>💻 IT/System</summary>
        ItSystem = 4,

        /// <summary>📖 Manual</summary>
        Manual = 5,

        /// <summary>📂 Folder</summary>
        Folder = 6,

        /// <summary>⚖️ Rule/Regulation</summary>
        RuleRegulation = 7,

        /// <summary>🔔 Announcement</summary>
        Announcement = 8,

        /// <summary>💡 Knowledge</summary>
        Knowledge = 9
    }
}
