using AuthenticationApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace AbbContentEditor.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        public AuthenticationService(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            
        }
        public Task<string> Login(LoginRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<string> Register(RegisterRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
