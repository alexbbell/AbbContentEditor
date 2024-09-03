using AbbContentEditor.Models;
using System.Security.Claims;

namespace AbbContentEditor.Helpers
{
    public interface ITokenManager
    {

        //string GetToken(CustomUser user);
        string GetAccessToken(string user);
        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);



    }
}
