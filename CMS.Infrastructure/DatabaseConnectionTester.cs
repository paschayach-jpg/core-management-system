using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CMS.Infrastructure
{
    /// <summary>
    /// Utility class to test database connection and provide diagnostics.
    /// Usage: DatabaseConnectionTester.TestConnection();
    /// </summary>
    public class DatabaseConnectionTester
    {
        /// <summary>
        /// Tests the database connection with detailed diagnostic output
        /// </summary>
        public static void TestConnection()
        {
            Console.WriteLine("========== DATABASE CONNECTION TEST ==========");
            Console.WriteLine($"Test Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();

            // Step 1: Read connection string
            Console.WriteLine("Step 1: Reading connection string from app.config...");
            var connStringSettings = ConfigurationManager.ConnectionStrings["CmsDb"];
            
            if (connStringSettings == null)
            {
                Console.WriteLine("❌ ERROR: Connection string 'CmsDb' not found in app.config");
                return;
            }

            string connectionString = connStringSettings.ConnectionString;
            Console.WriteLine($"✓ Connection string found");
            Console.WriteLine($"  Provider: {connStringSettings.ProviderName}");
            
            // Parse connection string details
            var builder = new SqlConnectionStringBuilder(connectionString);
            Console.WriteLine($"  Server: {builder.DataSource}");
            Console.WriteLine($"  Database: {builder.InitialCatalog}");
            Console.WriteLine($"  User ID: {builder.UserID}");
            Console.WriteLine($"  Connect Timeout: {builder.ConnectTimeout}s");
            Console.WriteLine();

            // Step 2: Test basic connectivity
            Console.WriteLine("Step 2: Testing basic connection...");
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("✓ Successfully connected to database");
                    Console.WriteLine($"  Database State: {connection.State}");
                    Console.WriteLine($"  Server Version: {connection.ServerVersion}");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"❌ SQL Connection Error: {ex.Message}");
                Console.WriteLine($"   Error Code: {ex.Number}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ General Error: {ex.Message}");
                return;
            }
            Console.WriteLine();

            // Step 3: Execute test query
            Console.WriteLine("Step 3: Executing test query...");
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM sys.tables", connection))
                    {
                        cmd.CommandTimeout = 10;
                        var result = cmd.ExecuteScalar();
                        int tableCount = Convert.ToInt32(result);
                        Console.WriteLine($"✓ Query executed successfully");
                        Console.WriteLine($"  Tables in database: {tableCount}");
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"❌ Query Execution Error: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return;
            }
            Console.WriteLine();

            // Step 4: Check for SystemConfig table
            Console.WriteLine("Step 4: Checking for SystemConfig table...");
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_NAME='SystemConfig'", 
                        connection))
                    {
                        var result = cmd.ExecuteScalar();
                        int exists = Convert.ToInt32(result);
                        
                        if (exists > 0)
                        {
                            Console.WriteLine("✓ SystemConfig table found");
                        }
                        else
                        {
                            Console.WriteLine("⚠ SystemConfig table NOT found");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Warning: {ex.Message}");
            }
            Console.WriteLine();

            // Step 5: Check for required stored procedures
            Console.WriteLine("Step 5: Checking for required stored procedures...");
            var requiredStoredProcs = new[] 
            { 
                "sp_GetSystemConfig", 
                "sp_UpsertSystemConfig"
            };

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    foreach (var procName in requiredStoredProcs)
                    {
                        using (var cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM information_schema.ROUTINES WHERE ROUTINE_NAME=@name", 
                            connection))
                        {
                            cmd.Parameters.AddWithValue("@name", procName);
                            var result = cmd.ExecuteScalar();
                            int exists = Convert.ToInt32(result);
                            
                            if (exists > 0)
                            {
                                Console.WriteLine($"✓ {procName} found");
                            }
                            else
                            {
                                Console.WriteLine($"❌ {procName} NOT found");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Warning: {ex.Message}");
            }
            Console.WriteLine();

            Console.WriteLine("========== TEST COMPLETE ==========");
        }

        /// <summary>
        /// Returns connection status as a simple boolean
        /// </summary>
        public static bool IsConnected()
        {
            try
            {
                var connStringSettings = ConfigurationManager.ConnectionStrings["CmsDb"];
                if (connStringSettings == null) return false;

                using (var connection = new SqlConnection(connStringSettings.ConnectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns connection details with status
        /// </summary>
        public static DatabaseConnectionInfo GetConnectionInfo()
        {
            var info = new DatabaseConnectionInfo();

            try
            {
                var connStringSettings = ConfigurationManager.ConnectionStrings["CmsDb"];
                if (connStringSettings == null)
                {
                    info.IsConnected = false;
                    info.ErrorMessage = "Connection string 'CmsDb' not found in app.config";
                    return info;
                }

                string connectionString = connStringSettings.ConnectionString;
                var builder = new SqlConnectionStringBuilder(connectionString);

                info.Server = builder.DataSource;
                info.Database = builder.InitialCatalog;
                info.UserID = builder.UserID;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    info.IsConnected = connection.State == ConnectionState.Open;
                    info.ServerVersion = connection.ServerVersion;
                }
            }
            catch (Exception ex)
            {
                info.IsConnected = false;
                info.ErrorMessage = ex.Message;
            }

            return info;
        }
    }

    /// <summary>
    /// Data class for database connection information
    /// </summary>
    public class DatabaseConnectionInfo
    {
        public bool IsConnected { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string UserID { get; set; }
        public string ServerVersion { get; set; }
        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            if (IsConnected)
            {
                return $"Connected to {Server}/{Database} as {UserID} (v{ServerVersion})";
            }
            return $"Not connected. Error: {ErrorMessage}";
        }
    }
}
