using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// เรียก Menu API ภายนอก (userinforequest) เพื่อดึง MenuList ของ User
    /// Config keys:
    ///   MenuApiUrl        — full URL เช่น http://host:9070/api/userinforequest
    ///   MenuApiProgramId  — Program ID ที่ส่งไปกับ request (default 1018)
    /// </summary>
    public class MenuApiService : IMenuApiService
    {
        private static readonly HttpClient _http = new HttpClient();

        private readonly string _apiUrl;
        private readonly string _programId;

        public MenuApiService(IConfigManager configManager)
        {
            if (configManager == null) throw new ArgumentNullException("configManager");
            try { _apiUrl    = configManager.Get<string>("MenuApiUrl")       ?? string.Empty; } catch { _apiUrl    = string.Empty; }
            try { _programId = configManager.Get<string>("MenuApiProgramId") ?? "1018"; }      catch { _programId = "1018"; }
        }

        public async Task<UserMenuModel> GetUserMenuAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(_apiUrl))
                throw new InvalidOperationException("MenuApiUrl ไม่ได้กำหนดใน app.config");

            string json = Serialize(new MenuRequest { UserToken = accessToken, ProgramId = _programId });

            using (var content  = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var response = await _http.PostAsync(_apiUrl, content).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode) return null;
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Deserialize<UserMenuModel>(body);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        private static string Serialize<T>(T obj)
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                ser.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private static T Deserialize<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            var ser = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                return (T)ser.ReadObject(ms);
        }

        [DataContract]
        private class MenuRequest
        {
            [DataMember(Name = "UserToken")]  public string UserToken  { get; set; }
            [DataMember(Name = "ProgramId")]  public string ProgramId  { get; set; }
        }
    }
}
