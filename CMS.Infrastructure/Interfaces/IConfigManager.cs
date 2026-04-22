using CMS.Domain.Models;

namespace CMS.Infrastructure.Interfaces
{
    /// <summary>
    /// Provides access to application configuration parameters.
    /// Reads from app.config first; falls back to the database config table.
    /// </summary>
    public interface IConfigManager
    {
        /// <summary>Retrieves a configuration value by key, converting it to type T.</summary>
        T Get<T>(string key);

        /// <summary>
        /// Retrieves a configuration value by key with a default fallback value.
        /// Returns the default value if the key is not found.
        /// </summary>
        T GetOrDefault<T>(string key, T defaultValue);

        /// <summary>
        /// Persists a configuration value. Requires Admin context.
        /// Raises an exception if the value fails type/range validation.
        /// </summary>
        void Set(string key, string value, UserContext admin);
    }
}
