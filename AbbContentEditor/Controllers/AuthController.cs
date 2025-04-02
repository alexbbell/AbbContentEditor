using AbbContentEditor.Data;
using AbbContentEditor.Helpers;
using AbbContentEditor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JWTSettings _options;
        private readonly ILogger<AuthController> _logger;
        private readonly AbbAppContext _abbAppContext;
        private readonly ITokenManager _tokenManager;
        private readonly UserManager<IdentityUser> _userManager;


        public AuthController(IOptions<JWTSettings> optAccess, ILogger<AuthController> logger, 
                    AbbAppContext abbAppContext, ITokenManager tokenManager, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _options = optAccess.Value;
            _logger.LogDebug(1, "NLog injected into HomeController");
            _abbAppContext = abbAppContext;
            _tokenManager = tokenManager;
            _userManager = userManager;
        }


        //[HttpGet("GetToken")]
        //public string GetToken(CustomUser user)
        //{
        //    List<Claim> claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.Name, user.Name)
        //    };
        //    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        //    var jwt = new JwtSecurityToken(
        //        issuer: _options.Issuer,
        //        audience: _options.Audience,
        //        claims: claims,
        //        expires: DateTime.Now.Add(TimeSpan.FromMinutes(3600)),
        //        notBefore: DateTime.UtcNow,
        //        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        //        );

        //    return new JwtSecurityTokenHandler().WriteToken(jwt);
        //}


        [HttpPost]
        public async Task<IActionResult> Authenticate([FromBody] AuthRequest authRequest)
        {
            PasswordHasher<IdentityUser> hasher = new PasswordHasher<IdentityUser>();

            var isUser = _abbAppContext.Users.FirstOrDefault(u => u.Email.Equals(authRequest.Username));
            var roles = await _userManager.GetRolesAsync(isUser);

            if (isUser == null)
            {
                _logger.LogError("Wrong authentication.");
                return Unauthorized();
            }
            var result = _userManager.PasswordHasher.VerifyHashedPassword(isUser, isUser.PasswordHash, authRequest.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return new BadRequestObjectResult(new { Message = "Login failed" });
            }

            _logger.LogInformation($"Suceesfully logged in {authRequest.Username}");
            return Ok(
                new AuthenticationResponse
                {
                    AccessToken = _tokenManager.GenerateAccessToken(isUser.UserName, roles),
                    RefreshToken = _tokenManager.GenerateRefreshToken()
                });

        }


        [HttpGet("exit")]
        public string ExitToken()
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: null,
                expires: DateTime.UtcNow.Add(TimeSpan.FromSeconds(1)),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }


    }
}
