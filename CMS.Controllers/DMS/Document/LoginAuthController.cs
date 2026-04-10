using System.Threading.Tasks;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class LoginAuthController : ILoginAuthController
    {
        private readonly IAdAuthService _adAuthService;

        public LoginAuthController(IAdAuthService adAuthService = null)
        {
            _adAuthService = adAuthService;
        }

        public Task<AdAuthResult> LoginAsync(string username, string password)
        {
            if (_adAuthService == null)
                return Task.FromResult(new AdAuthResult
                {
                    Success = false,
                    ErrorMessage = "Authentication service is not available."
                });

            return _adAuthService.LoginAsync(username, password);
        }
    }
}
