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
        // Refresh token storage key in Abb_UserTokens. The name is historical; kept so existing rows still match.
        private const string RefreshTokenProvider = "abb";
        private const string RefreshTokenName = "PasswordResetToken";

        private readonly UserManager<AbbAppUser> _userManager;
        private readonly SignInManager<AbbAppUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly JWTSettings _options;
        private readonly ITokenManager _tokenManager;
        private readonly AbbAppContext _abbAppContext;
        private readonly IMapper _mapper;
        private readonly ITurnstileService _turnstileService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AccountController(
            IOptions<JWTSettings> optAccess,
            UserManager<AbbAppUser> userManager,
            SignInManager<AbbAppUser> signInManager,
            ILogger<AccountController> logger,
            ITokenManager tokenManager,
            AbbAppContext abbAppContext,
            IMapper mapper,
            ITurnstileService turnstileService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _options = optAccess.Value;
            _tokenManager = tokenManager;
            _abbAppContext = abbAppContext;
            _mapper = mapper;
            _turnstileService = turnstileService;
            _emailService = emailService;
            _configuration = configuration;
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
                try
                {
                    var frontendBaseUrl = _configuration["FrontendBaseUrl"] ?? "https://alexey.beliaeff.ru";
                    await SendEmailRegistrationAsync(user, frontendBaseUrl);
                }
                catch (Exception emailEx)
                {
                    // Roll back so the user can register again with the same email once mail delivery works
                    _logger.LogError(emailEx, "Failed to send confirmation email to {Email}. Rolling back registration.", model.Email);
                    await _userManager.DeleteAsync(user);

                    return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Registration Failed",
                        Detail = "Could not send the confirmation email. Please try again later."
                    });
                }

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
            const string invalidLinkMessage = "This confirmation link is invalid or has expired.";

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return BadRequest(new { Message = invalidLinkMessage });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { Message = invalidLinkMessage });
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                return Ok(new { Message = "Your email is already confirmed." });
            }

            // Decode base64url encoded token
            string decodedCode;
            try
            {
                decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return BadRequest(new { Message = invalidLinkMessage });
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedCode);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Email confirmation failed for {Email}: {Errors}",
                    user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(new { Message = invalidLinkMessage });
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
            var frontendBaseUrl = (_configuration["FrontendBaseUrl"] ?? "https://alexey.beliaeff.ru").TrimEnd('/');
            var resetLink = $"{frontendBaseUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&code={encodedToken}";

            // 5. Send email. Failures are logged but not reported to the client to avoid account enumeration.
            try
            {
                var safeUrl = System.Net.WebUtility.HtmlEncode(resetLink);
                var body = BuildEmailBody(
                    "Reset your password",
                    "We received a request to reset the password for your ABWords account. Click the button below to choose a new password.",
                    "Reset password",
                    safeUrl,
                    "If you didn't request a password reset, you can ignore this email. Your password won't change.");

                await _emailService.SendEmailAsync(user.Email!, "Reset your ABWords password", body);
                _logger.LogInformation("Password reset email sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            }

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
                return BadRequest(new { Message = "Invalid password reset request." });
            }

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            }
            catch (FormatException)
            {
                return BadRequest(new { Message = "This reset link is invalid or has expired." });
            }

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

            var emailConfirmationUrl =
                $"{returnUrl.TrimEnd('/')}/confirm-email?userId={Uri.EscapeDataString(user.Id)}&code={encodedCode}";

            var safeUrl = System.Net.WebUtility.HtmlEncode(emailConfirmationUrl);
            var body = BuildEmailBody(
                "Welcome to ABWords!",
                "Thanks for signing up. Please confirm your email address to activate your account.",
                "Confirm email",
                safeUrl,
                "This link expires in 4 hours. If you didn't create an account, you can ignore this email.");

            await _emailService.SendEmailAsync(user.Email!, "Confirm your ABWords account", body);
            _logger.LogInformation("Confirmation email sent to {Email}", user.Email);
        }

        /// <summary>Builds a simple HTML email with a call-to-action button. <paramref name="safeUrl"/> must already be HTML-encoded.</summary>
        private static string BuildEmailBody(string heading, string intro, string buttonText, string safeUrl, string footer) => $@"
<div style=""font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#222;max-width:560px"">
  <h2 style=""margin:0 0 16px"">{heading}</h2>
  <p>{intro}</p>
  <p style=""margin:24px 0"">
    <a href=""{safeUrl}"" style=""background:#2563eb;color:#fff;padding:12px 20px;border-radius:6px;text-decoration:none;display:inline-block"">{buttonText}</a>
  </p>
  <p>If the button doesn't work, copy and paste this link into your browser:</p>
  <p style=""word-break:break-all""><a href=""{safeUrl}"">{safeUrl}</a></p>
  <p style=""color:#666;font-size:13px"">{footer}</p>
</div>";

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

                    // Upsert: one refresh token row per user (PK = UserId + LoginProvider + Name)
                    await _userManager.SetAuthenticationTokenAsync(user, RefreshTokenProvider, RefreshTokenName, refreshToken);

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

            // Upsert: one refresh token row per user (PK = UserId + LoginProvider + Name)
            await _userManager.SetAuthenticationTokenAsync(user, RefreshTokenProvider, RefreshTokenName, newRefreshToken);

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