namespace CMS.Domain.Enums
{
    /// <summary>
    /// Entity types for KB Article Links (KMS.t_KBArticleLink.EntityType)
    /// Defines the types of entities that can be linked to KB articles
    /// </summary>
    public enum KBEntityType : byte
    {
        /// <summary>
        /// Link to Folder (DMS.t_Folder)
        /// </summary>
        Folder = 1,

        /// <summary>
        /// Link to Task (TMS.t_Task)
        /// </summary>
        Task = 2,

        /// <summary>
        /// Link to Program (ITL.m_Program)
        /// </summary>
        Program = 3,

        /// <summary>
        /// Link to Law/Regulation
        /// </summary>
        Law = 4,

        /// <summary>
        /// Link to Project (PMS.t_Project)
        /// </summary>
        Project = 5,

        Department = 6
    }
}
