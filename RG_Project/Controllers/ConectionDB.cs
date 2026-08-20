using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using RG_Project.Config;
using Newtonsoft.Json;
using System.IO;

namespace RG_Project.Controllers
{
    public class ConectionDB
    {
        // Connection string for your SQL Server database
        private string connectionString = null;
        private string connectionStringEFiling = null;
        private string connectionStringBIS = null;
        private string connectionStringCommon = null;
        private string connectionStringK2 = null;
        private string connectionStringCms = null;
        public static DBConfig _config;

        private readonly string efServer = ConfigurationManager.AppSettings["EFServer"];
        private readonly string bisServer = ConfigurationManager.AppSettings["BISServer"];
        private readonly string cmServer = ConfigurationManager.AppSettings["CMServer"];
        private readonly string k2devServer = ConfigurationManager.AppSettings["R2Server"];
        private readonly string dbServer = ConfigurationManager.AppSettings["DBServer"];
        private readonly string cmsServer = ConfigurationManager.AppSettings["CMSServer"];
        public readonly string _DBConfig = ConfigurationManager.AppSettings["DBConfig"];
        public ConectionDB()
        {
            try
            {
                // ตรวจสอบว่า _config มีค่าหรือยัง
                if (_config == null)
                {
                    // โหลดการตั้งค่าจากไฟล์
                    _config = DBConfigReader.LoadDBConfig(_DBConfig);
                }

                Console.WriteLine($"DB Server: {_config.DBServer}");
                Console.WriteLine($"Connection String: {connectionString}");

                switch (dbServer)
                {
                    case "dev":
                        this.connectionString = _config.DBServer.Dev;
                        break;
                    case "sit":
                        this.connectionString = _config.DBServer.Sit;
                        break;
                    case "uat":
                        this.connectionString = _config.DBServer.Uat;
                        break;
                    case "prd":
                        this.connectionString = _config.DBServer.Prd;
                        break;
                        //default:
                        //    throw new ArgumentException($"Invalid environment: {_config.DBServer.Server}");
                }
                switch (efServer)
                {
                    case "dev":
                        this.connectionStringEFiling = _config.EfServer.Dev;
                        break;
                    case "sit":
                        this.connectionStringEFiling = _config.EfServer.Sit;
                        break;
                    case "uat":
                        this.connectionStringEFiling = _config.EfServer.Uat;
                        break;
                    case "prd":
                        this.connectionStringEFiling = _config.EfServer.Prd;
                        break;
                        //default:
                        //    throw new ArgumentException($"Invalid environment: {_config.EfServer.Server}");
                }
                switch (bisServer)
                {
                    case "dev":
                        this.connectionStringBIS = _config.BisServer.Dev;
                        break;
                    case "sit":
                        this.connectionStringBIS = _config.BisServer.Sit;
                        break;
                    case "uat":
                        this.connectionStringBIS = _config.BisServer.Uat;
                        break;
                    case "prd":
                        this.connectionStringBIS = _config.BisServer.Prd;
                        break;
                        //default:
                        //    throw new ArgumentException($"Invalid environment: {_config.BisServer.Server}");
                }
                switch (cmServer)
                {
                    case "dev":
                        this.connectionStringCommon = _config.CmServer.Dev;
                        break;
                    case "sit":
                        this.connectionStringCommon = _config.CmServer.Sit;
                        break;
                    case "uat":
                        this.connectionStringCommon = _config.CmServer.Uat;
                        break;
                    case "prd":
                        this.connectionStringCommon = _config.CmServer.Prd;
                        break;
                        //default:
                        //    throw new ArgumentException($"Invalid environment: {_config.CmServer.Server}");
                }
                switch (k2devServer)
                {
                    case "dev":
                        this.connectionStringK2 = _config.K2DevServer.Dev;
                        break;
                    case "sit":
                        this.connectionStringK2 = _config.K2DevServer.Sit;
                        break;
                    case "uat":
                        this.connectionStringK2 = _config.K2DevServer.Uat;
                        break;
                    case "prd":
                        this.connectionStringK2 = _config.K2DevServer.Prd;
                        break;
                }
                if (_config.CmsServer != null)
                switch (cmsServer)
                {
                    case "dev":
                        this.connectionStringCms = _config.CmsServer.Dev;
                        break;
                    case "sit":
                        this.connectionStringCms = _config.CmsServer.Sit;
                        break;
                    case "uat":
                        this.connectionStringCms = _config.CmsServer.Uat;
                        break;
                    case "prd":
                        this.connectionStringCms = _config.CmsServer.Prd;
                        break;
                }
            }
            catch (Exception)
            {
                throw new Exception($"Error reading configuration file : Error Code: 4060");
            }
        }


        public SqlConnection connection = new SqlConnection();
        public void Connect(char db)
        {
            if(db=='c')
            {
                connection.ConnectionString = connectionString;
                connection.Open();
            }
            else if(db=='e')
            {
                connection.ConnectionString = connectionStringEFiling;
                connection.Open();
            }
            else if (db == 'b')
            {
                connection.ConnectionString = connectionStringBIS;
                connection.Open();
            }
            else if (db == 'm')
            {
                connection.ConnectionString = connectionStringCommon;
                connection.Open();
            }
            else if (db == 'k')
            {
                connection.ConnectionString = connectionStringK2;
                connection.Open();
            }
            else if (db == 'n')
            {
                connection.ConnectionString = connectionStringCms;
                connection.Open();
            }



            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    // SQL query to retrieve data
            //    string query = "SELECT * FROM YourTableName";

            //    using (SqlCommand command = new SqlCommand(query, connection))
            //    {
            //        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            //        {
            //            DataTable dataTable = new DataTable();
            //            adapter.Fill(dataTable);

            //            // Bind data to DataGridView
            //            dataGridView1.DataSource = dataTable;
            //        }
            //    }
            //}
        }
        public void Disconnect()
        {
            connection.Close();
        }
    }
}
