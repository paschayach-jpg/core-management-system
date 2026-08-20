using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace RG_Project.Config
{


    public class DBConfig
    {
        [JsonProperty("DBServer")]
        public ServerConfig DBServer { get; set; }

        [JsonProperty("efServer")]
        public ServerConfig EfServer { get; set; }

        [JsonProperty("bisServer")]
        public ServerConfig BisServer { get; set; }

        [JsonProperty("cmServer")]
        public ServerConfig CmServer { get; set; }

        [JsonProperty("k2devServer")]
        public ServerConfig K2DevServer { get; set; }

        [JsonProperty("bisRGServer")]
        public ServerConfig bisRGServer { get; set; }

        [JsonProperty("cmsServer")]
        public ServerConfig CmsServer { get; set; }

    }

    public class ServerConfig
    {
        [JsonProperty("Server")]
        public string Server { get; set; }

        [JsonProperty("dev")]
        public string Dev { get; set; }

        [JsonProperty("sit")]
        public string Sit { get; set; }

        [JsonProperty("uat")]
        public string Uat { get; set; }

        [JsonProperty("prd")]
        public string Prd { get; set; }
    }


}
