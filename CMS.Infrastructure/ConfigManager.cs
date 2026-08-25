using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Newtonsoft.Json.Linq;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Infrastructure
{
    /// <summary>
    /// Reads configuration values from app.config (appSettings) first.
    /// If a value is not found there and a connection string named "CmsDb" is configured,
    /// falls back to the SystemConfig database table (Requirement 14.1).
    /// Writes always go to the database when a connection string is available.
    /// </summary>
    public class ConfigManager : IConfigManager
    {
        private readonly JObject _jsonConfig;
        private readonly string _connectionString;

        // In-memory cache to avoid repeated DB round-trips
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ConfigManager()
        {
            string configPath = Environment.GetEnvironmentVariable("CMS_CONFIG_PATH");
            
            // If environment variable is not set or empty, check central network config fallback
            if (string.IsNullOrWhiteSpace(configPath))
            {
                configPath = @"\\150.1.8.23\AppForUser\CMS_Central_Configs\cms_config.json";
            }

            if (File.Exists(configPath))
            {
                try
                {
                    string jsonText = File.ReadAllText(configPath);
                    _jsonConfig = JObject.Parse(jsonText);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to parse JSON config at: " + configPath, ex);
                }
            }
            else
            {
                // If environment variable was set but file not found, or central file also not found
                string envVarMsg = Environment.GetEnvironmentVariable("CMS_CONFIG_PATH") != null 
                    ? "Verify the environment variable 'CMS_CONFIG_PATH' is set and points to a valid JSON file."
                    : "No 'CMS_CONFIG_PATH' environment variable was found, and the central fallback file is missing or the network path is inaccessible.";
                
                throw new FileNotFoundException(string.Format(
                    "CMS Configuration file was not found at '{0}'. {1}", configPath, envVarMsg));
            }

            if (_jsonConfig != null)
            {
                _connectionString = _jsonConfig["ConnectionStrings"]?["CmsDb"]?.ToString();
            }
        }

        /// <summary>
        /// Retrieves a configuration value by key.
        /// Resolution order: app.config appSettings → database SystemConfig table → exception.
        /// </summary>
        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            string raw = GetRawValue(key);
            if (raw == null)
                throw new KeyNotFoundException(string.Format("Configuration key '{0}' was not found.", key));

            return ConvertValue<T>(key, raw);
        }

        /// <summary>
        /// Retrieves a configuration value by key with a default fallback value.
        /// Returns the default value if the key is not found.
        /// </summary>
        public T GetOrDefault<T>(string key, T defaultValue)
        {
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            string raw = GetRawValue(key);
            if (raw == null)
                return defaultValue;

            try
            {
                return ConvertValue<T>(key, raw);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Persists a configuration value to the database.
        /// Requires Admin role (Requirement 14.2).
        /// </summary>
        public void Set(string key, string value, UserContext admin)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (admin == null)
                throw new ArgumentNullException("admin");
            if (admin.AppRole != 4) // 4=Admin
                throw new UnauthorizedAccessException("Only Admin users may change configuration values.");
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("No database connection string configured; cannot persist configuration.");

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("sp_UpsertSystemConfig", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Key",          key);
                    cmd.Parameters.AddWithValue("@Value",        value);
                    cmd.Parameters.AddWithValue("@UpdatedByUserId", admin.UserID);
                    cmd.ExecuteNonQuery();
                }
            }

            // Invalidate cache
            lock (_cache)
            {
                _cache[key] = value;
            }
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private string GetRawValue(string key)
        {
            // 1. Check in-memory cache
            lock (_cache)
            {
                string cached;
                if (_cache.TryGetValue(key, out cached))
                    return cached;
            }

            // 2. JSON AppSettings lookup
            if (_jsonConfig != null)
            {
                string jsonVal = _jsonConfig["AppSettings"]?[key]?.ToString();
                if (jsonVal != null)
                {
                    lock (_cache) { _cache[key] = jsonVal; }
                    return jsonVal;
                }
            }

            // 3. Database fallback
            if (!string.IsNullOrEmpty(_connectionString))
            {
                string dbValue = ReadFromDatabase(key);
                if (dbValue != null)
                {
                    lock (_cache) { _cache[key] = dbValue; }
                    return dbValue;
                }
            }

            return null;
        }

        private string ReadFromDatabase(string key)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("sp_GetSystemConfig", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Key", key);
                        var result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value ? result.ToString() : null;
                    }
                }
            }
            catch
            {
                // DB unavailable — return null so caller can decide
                return null;
            }
        }

        private static T ConvertValue<T>(string key, string raw)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                    return (T)converter.ConvertFromInvariantString(raw);

                return (T)Convert.ChangeType(raw, typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    string.Format("Configuration key '{0}' value '{1}' cannot be converted to {2}.", key, raw, typeof(T).Name), ex);
            }
        }
    }
}
