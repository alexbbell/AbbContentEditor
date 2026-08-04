using AbbContentEditor.Data;
using AbbContentEditor.Helpers;
using AbbContentEditor.Models;
using AbbContentEditor.Models.Account;
using AbbContentEditor.Models.Enums;
using AbbContentEditor.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AbbAppUser> _userManager;
        private readonly SignInManager<AbbAppUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly JWTSettings _options;
        private readonly ITokenManager _tokenManager;
        private readonly AbbAppContext _abbAppContext;
        private readonly IMapper _mapper;
        private readonly ITurnstileService _turnstileService;

        public AccountController(
            IOptions<JWTSettings> optAccess,
            UserManager<AbbAppUser> userManager,
            SignInManager<AbbAppUser> signInManager,
            ILogger<AccountController> logger,
            ITokenManager tokenManager,
            AbbAppContext abbAppContext,
            IMapper mapper,
            ITurnstileService turnstileService)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _options = optAccess.Value;
            _tokenManager = tokenManager;
            _abbAppContext = abbAppContext;
            _mapper = mapper;
            _turnstileService = turnstileService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("RegisterUser")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = new AbbAppUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("User registration failed for {Email}: {Errors}", model.Email, errors);

                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Registration Failed",
                        Detail = errors
                    });
                }

                var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Guest.ToString());
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);

                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Role assignment failed for {Email}: {Errors}", model.Email, roleErrors);

                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Registration Failed",
                        Detail = "Could not assign default role to user."
                    });
                }

                // Trigger Email Confirmation
                await SendEmailRegistrationAsync(user, "https://alexey.beliaeff.ru");

                _logger.LogInformation("User {Email} successfully registered.", model.Email);
                return Ok(new { Message = "User registered successfully. Please check your email to confirm registration." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while registering user {Email}", model.Email);

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server Error",
                    Detail = "An unexpected error occurred. Please try again later."
                });
            }
        }

        /// <summary>
        /// Validates email confirmation token sent via email link.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return BadRequest("User Id and Code are required.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            // Decode base64url encoded token
            var decodedCodeBytes = WebEncoders.Base64UrlDecode(code);
            var decodedCode = Encoding.UTF8.GetString(decodedCodeBytes);

            var result = await _userManager.ConfirmEmailAsync(user, decodedCode);
            if (!result.Succeeded)
            {
                return BadRequest("Error confirming your email.");
            }

            return Ok(new { Message = "Email confirmed successfully." });
        }

        /// <summary>
        /// Endpoint matching front-end call to `/Account/Forgotten`. Verifies Cloudflare CAPTCHA and emails password reset token.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Route("Forgotten")]
        public async Task<IActionResult> Forgotten([FromBody] ForgottenPasswordRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Verify Cloudflare Turnstile token
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            bool isCaptchaValid = await _turnstileService.VerifyTokenAsync(model.TurnstileToken, clientIp);

            if (!isCaptchaValid)
            {
                return BadRequest(new { Message = "CAPTCHA verification failed. Please try again." });
            }

            // 2. Locate user
            var user = await _userManager.FindByEmailAsync(model.Email);

            // To prevent account enumeration attacks, always return OK even if user is not found
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return Ok(new { Message = "If an account with that email exists, reset instructions have been sent." });
            }

            // 3. Generate Password Reset Token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // 4. Construct Link (Points to React Frontend Reset Password Page)
            var resetLink = $"https://alexey.beliaeff.ru/reset-password?email={Uri.EscapeDataString(user.Email)}&code={encodedToken}";

            // TODO: Send `resetLink` using your preferred Email Service (SendGrid, SMTP, etc.)
            _logger.LogInformation("Password reset link generated for {Email}: {ResetLink}", user.Email, resetLink);

            return Ok(new { Message = "If an account with that email exists, reset instructions have been sent." });
        }

        /// <summary>
        /// Endpoint to execute the actual password reset using the token.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Invalid password reset request.");
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { Message = errors });
            }

            return Ok(new { Message = "Password has been successfully reset." });
        }

        private async Task SendEmailRegistrationAsync(AbbAppUser user, string returnUrl)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var emailConfirmationUrl = $"{returnUrl}/confirm-email?userId={user.Id}&code={encodedCode}";

            // TODO: Replace with real email service dispatch
            _logger.LogInformation("Confirmation link generated for {Email}: {Url}", user.Email, emailConfirmationUrl);
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password,
                    model.RememberMe, lockoutOnFailure: false);

                _logger.LogInformation("Login attempt for {Email}", model.Email);

                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null) return Unauthorized(new { message = "Access denied. Please provide valid credentials" });

                IList<string> userRoles = await _userManager.GetRolesAsync(user);
                if (result.Succeeded)
                {
                    var refreshToken = _tokenManager.GenerateRefreshToken();
                    var accessToken = _tokenManager.GenerateAccessToken(model.Email, userRoles);

                    AuthenticationResponse tokenApiModel = new AuthenticationResponse()
                    {
                        User = new UserDto()
                        {
                            Email = user.Email,
                            Id = new Guid(user.Id)
                        },
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    };

                    _abbAppContext.UserTokens.Add(new IdentityUserToken<string>
                    {
                        LoginProvider = "abb",
                        Name = "PasswordResetToken",
                        UserId = user.Id,
                        Value = refreshToken
                    });

                    await _abbAppContext.SaveChangesAsync();

                    return Ok(tokenApiModel);
                }
                else
                {
                    return Unauthorized(new { message = "Access denied. Please provide valid credentials" });
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] AuthenticationResponse tokenApiModel)
        {
            if (tokenApiModel is null)
                return BadRequest("Invalid client request");

            string accessToken = tokenApiModel.AccessToken;
            string refreshToken = tokenApiModel.RefreshToken;

            var principal = _tokenManager.GetPrincipalFromExpiredToken(accessToken);
            var user = await _userManager.GetUserAsync(principal);

            if (user is null)
                return BadRequest("Invalid client request");

            var roles = await _userManager.GetRolesAsync(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(accessToken);

            var newAccessToken = _tokenManager.GenerateAccessToken(user.Email, roles);
            var newRefreshToken = _tokenManager.GenerateRefreshToken();

            var rtoken = _abbAppContext.UserTokens
                .SingleOrDefault(u => u.UserId.Equals(user.Id) && u.Value.Equals(refreshToken));

            if (rtoken != null && jwtSecurityToken.ValidTo > DateTime.Now.AddDays(-3))
            {
                rtoken.Value = newRefreshToken;
                _abbAppContext.UserTokens.Update(rtoken);
            }
            else
            {
                _abbAppContext.UserTokens.Add(new IdentityUserToken<string>
                {
                    LoginProvider = "abb",
                    Name = "PasswordResetToken",
                    UserId = user.Id,
                    Value = newRefreshToken
                });
            }

            await _abbAppContext.SaveChangesAsync();

            return Ok(new AuthenticationResponse()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
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