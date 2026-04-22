using System.Threading.Tasks;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface ILoginAuthController
    {
        Task<AdAuthResult> LoginAsync(string username, string password);
    }
}
