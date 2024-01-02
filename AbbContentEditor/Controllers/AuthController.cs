using AbbContentEditor.Models;
using Microsoft.AspNetCore.Authorization;
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
        public AuthController(IOptions<JWTSettings> optAccess, ILogger<AuthController> logger)
        {
            _logger = logger;
            _options = optAccess.Value;
            _logger.LogDebug(1, "NLog injected into HomeController");

        }


        [HttpGet("GetToken")]
        public string GetToken(User user)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, user.Name));
            //claims.Add(new Claim(ClaimTypes.Role, user.Role.Value.ToString()));
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.Now.Add(TimeSpan.FromMinutes(3600)),
                //expires: DateTime.Now.AddMinutes(3), // (TimeSpan.FromMinutes(3600)),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }


        [HttpPost]
        public async Task<IActionResult> Authenticate([FromBody] AuthRequest authRequest)
        {
            //User user = await _context.Users.FirstOrDefaultAsync(item => item.Name == authRequest.Name && item.Password == authRequest.Password);

            var user = (authRequest.Username == "admin" && authRequest.Password == "sucker81") ? new User() { Id = new Guid("d57dddfb-ae63-4563-8312-6a52050ad3a8"), Name = "alexbbell" } : null;
            // await _context.Users.FirstOrDefaultAsync(item => item.Name == authRequest.Name && item.Password == authRequest.Password);
            if (user == null)
            {
                _logger.LogError("Wrong authentication.");
                return Unauthorized();

            }
            _logger.LogInformation($"Suceesfully logged in {authRequest.Username}");
            return Ok(GetToken(user));
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
