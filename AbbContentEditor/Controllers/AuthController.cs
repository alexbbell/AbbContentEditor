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
        private readonly UserManager<AbbAppUser> _userManager;


        public AuthController(IOptions<JWTSettings> optAccess, ILogger<AuthController> logger, 
                    AbbAppContext abbAppContext, ITokenManager tokenManager, UserManager<AbbAppUser> userManager)
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
            // 1. Look up user (Try email first, or fall back to username if needed)
            var user = await _userManager.FindByEmailAsync(authRequest.Email);

            // 2. Guard clause: Check null BEFORE performing any operations on 'user'
            if (user == null)
            {
                _logger.LogError($"Authentication failed: User '{authRequest.Email}' not found.");
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            // 3. Verify password using Identity's built-in helper
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, authRequest.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning($"Authentication failed: Invalid password for '{authRequest.Email}'.");
                return BadRequest(new { Message = "Invalid username or password" });
            }

            // 4. Safely retrieve roles AFTER confirming user exists
            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation($"Successfully logged in {user.UserName}");

            return Ok(new AuthenticationResponse
            {
                User = new UserDto()
                {
                    Email= user.Email,
                    Id = new Guid(user.Id)
                },
                AccessToken = _tokenManager.GenerateAccessToken(user.UserName, roles),
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
