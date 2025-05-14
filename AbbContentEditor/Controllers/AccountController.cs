using AbbContentEditor.Data;
using AbbContentEditor.Helpers;
using AbbContentEditor.Models;
using AbbContentEditor.Models.Enums;
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

        public AccountController(IOptions<JWTSettings> optAccess, UserManager<AbbAppUser> userManager, 
                                 SignInManager<AbbAppUser> signInManager, ILogger<AccountController> logger, 
                                 ITokenManager tokenManager, AbbAppContext abbAppContext, IMapper mapper)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _options = optAccess.Value;
            _tokenManager = tokenManager;
            _abbAppContext = abbAppContext;
            _mapper = mapper;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("RegisterUser")]
        public async Task<IActionResult> Register(RegisterRequestModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AbbAppUser user = new AbbAppUser { UserName = model.Email, Email = model.Email, PasswordHash = model.Password };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    var addRole = await _userManager.AddToRoleAsync(user, UserRoles.Guest.ToString());

                    if (result.Succeeded && addRole.Succeeded)
                    {
                        // Handle successful registration
                        _logger.LogInformation($"User {model.Email} is successfully registered");
                        return new OkObjectResult("User Register Successfully ");
                    }
                    else
                    {
                        _logger.LogError($"User {model.Email} could not register.");
                        string err = string.Join(",", result.Errors.Select(x=> x.Description));

                        return new BadRequestObjectResult(
                            new ProblemDetails
                            {
                                Status = StatusCodes.Status400BadRequest,
                                Title = "Bad Request",
                                Detail = $"There was an error processing your request, please try again. {err}",
                            }
                            );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new BadRequestObjectResult(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Bad Request",
                        Detail = $"There was an error processing your request, please try again. {ex.Message}"
                    });
                throw;
            }
            return new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "Something is wrong"
            });
        }



        [HttpPost]
        [AllowAnonymous]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }
            SendEmailRegistration(user, "https://alexey.beliaeff.ru");
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            return Ok();
        }

        private async void SendEmailRegistration (AbbAppUser user, string? returnUrl)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var EmailConfirmationUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                protocol: Request.Scheme);

        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, 
                    model.RememberMe, lockoutOnFailure: false);
                _logger.LogInformation($"Login {model.Email}");
                //var user = _abbAppContext.Users.Where(u => u.NormalizedUserName.Equals(model.Email.ToUpper())).FirstOrDefault();
                //var user = _userManager.Users.FirstOrDefault(x=>x.Email.Equals(model.Email.ToUpper(), StringComparison.InvariantCultureIgnoreCase));
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null) return Unauthorized(new { message = "Access denied. Please provide valid credentials" });

                IList<string> userRoles = await _userManager.GetRolesAsync(user);
                if (result.Succeeded)
                {
                    // return your customize result 
                    var refreshToken = _tokenManager.GenerateRefreshToken();
                    var accessToken = _tokenManager.GenerateAccessToken(model.Email, userRoles);
                    AuthenticationResponse tokenApiModel = new AuthenticationResponse()
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    };

                    _abbAppContext.UserTokens.Add( new IdentityUserToken<string>
                    {
                        LoginProvider = "abb",
                        Name = "PasswordResetToken",
                        UserId = user.Id, 
                        Value = refreshToken
                    });

                    _abbAppContext.SaveChangesAsync();

                    return Ok(tokenApiModel);
                }
                else
                {
                    //if (result.IsLockedOut)
                    //{
                    //    _logger.LogWarning("User account locked out.");
                    //    return new OkObjectResult(new LoginResponseModel { Status = "Failed to login", Message = "Account locked out." });
                    //}
                    //if (result.IsNotAllowed)
                    //{
                    //    _logger.LogWarning("User not allowed to sign in.");
                    //    return new OkObjectResult(new LoginResponseModel { Status = "Failed to login", Message = "Account not allowed to sign in." });
                    //}
                    //if (result.RequiresTwoFactor)
                    //{
                    //    _logger.LogWarning("Two-factor authentication required.");
                    //    return new OkObjectResult(new LoginResponseModel { Status = "Failed to login", Message = "Two-factor authentication required." });
                    //}
                    return Unauthorized( new { message = "Access denied. Please provide valid credentials" });
                    //return new OkObjectResult(new LoginResponseModel { Status = "Failed to login",  
                        //Message = result.ToString() });
                }
            }
            return Ok("Something strange");

        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh(AuthenticationResponse tokenApiModel)
        {
            if (tokenApiModel is null)
                return BadRequest("Invalid client request");
            string accessToken = tokenApiModel.AccessToken;
            string refreshToken = tokenApiModel.RefreshToken;
            System.Security.Claims.ClaimsPrincipal principal = _tokenManager.GetPrincipalFromExpiredToken(accessToken);
            var username = principal.Identity.Name; //this is mapped to the Name claim by default
            //var user =  _abbAppContext.Users.SingleOrDefault(u => u.UserName == username);
            var user = await _userManager.GetUserAsync(principal);
            
            var roles = await _userManager.GetRolesAsync(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(accessToken);
            

            // if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            if (user is null )
                return BadRequest("Invalid client request");

            var newAccessToken = _tokenManager.GenerateAccessToken(user.Email, roles);
            var newRefreshToken = _tokenManager.GenerateRefreshToken();
            var rtoken = _abbAppContext.UserTokens.
                    SingleOrDefault(u=>u.UserId.Equals(user.Id) && 
                                    u.Value.Equals(refreshToken));
            Console.WriteLine(jwtSecurityToken.ValidTo < DateTime.Now.AddDays(-3));
            // если рефреш токен пользователя существует и валидный токен истек не более 3 дней назад
            if (rtoken != null && jwtSecurityToken.ValidTo > DateTime.Now.AddDays(-3))
            {
                var newrefreshtoken = rtoken;
                newrefreshtoken.Value = newRefreshToken;
                if(rtoken != null ) _abbAppContext.UserTokens.Update(newrefreshtoken);
            }
            
            else
            {
                _abbAppContext.UserTokens.Add(new IdentityUserToken<string>
                {
                    LoginProvider = "abb",
                    Name = "PasswordResetToken",
                    UserId = user.Id,
                    Value = refreshToken
                });
            }
            _abbAppContext.SaveChangesAsync();

            //rtoken.Value = newRefreshToken;

            /// TODO: write refresh tokens to a table, userId - refresh token
            /// In front - read Token and refresh token

            return Ok(new AuthenticationResponse()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        //[AllowAnonymous]
        //[HttpPost("register")]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        //{
        //    var response = await _authenticationService.Register(request);

        //    return Ok(response);
        //}



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
