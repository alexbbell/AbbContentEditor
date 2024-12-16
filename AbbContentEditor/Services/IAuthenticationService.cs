using Microsoft.AspNetCore.Identity.Data;
namespace AuthenticationApi.Services;

public interface IAuthenticationService
{
    Task<string> Register(RegisterRequest request);
    Task<string> Login(LoginRequest request);
}