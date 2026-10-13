namespace CMS.Domain.Enums
{
    /// <summary>
    /// How a user may open a file outside the app (Word/Excel/PowerPoint etc.).
    /// View = locked, read-only copy. Edit = writable copy that can be saved back as a new version.
    /// </summary>
    public enum FileOpenMode
    {
        None = 0,
        View = 1,
        Edit = 2
    }
}
