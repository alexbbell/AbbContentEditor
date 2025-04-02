using AbbContentEditor.Models;
using System.Security.Claims;

namespace AbbContentEditor.Helpers
{
    public interface ITokenManager
    {

        //string GetToken(CustomUser user);
        string GenerateAccessToken(string user, IList<string> roles);
        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);



    }
}
