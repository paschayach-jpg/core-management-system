using System.Collections.Generic;

namespace CMS.Data.Models
{
    /// <summary>
    /// A paged result set returned from a repository query.
    /// </summary>
    /// <typeparam name="T">The type of items in the result set.</typeparam>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
