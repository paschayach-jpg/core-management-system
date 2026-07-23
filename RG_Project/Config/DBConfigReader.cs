using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RG_Project.Config
{
    public class DBConfigReader
    {
        public static DBConfig LoadDBConfig(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                DBConfig dBConfig = JsonConvert.DeserializeObject<DBConfig>(jsonContent);


                return dBConfig;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading configuration file: {ex.Message}");
            }
        }
    }
    public class bisConfigReader
    {
        public static DBConfig LoadDBConfig(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                DBConfig dBConfig = JsonConvert.DeserializeObject<DBConfig>(jsonContent);

                return dBConfig;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading configuration file: {ex.Message}");
            }
        }
    }
}
