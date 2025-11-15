using AbbContentEditor.Data;
using AbbContentEditor.Helpers;
using AbbContentEditor.Models;
using AbbContentEditor.Models.Enums;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<AbbAppUser> _userManager;
        private readonly SignInManager<AbbAppUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly JWTSettings _options;
        private readonly ITokenManager _tokenManager;
        private readonly AbbAppContext _abbAppContext;
        private readonly IMapper _mapper;
        private readonly AbbAppContext _context;
        private readonly IUserStore<AbbAppUser> _userStore;

        private readonly PasswordHasher<IdentityUser> _passwordHasher = new PasswordHasher<IdentityUser>();
        public UsersController(IOptions<JWTSettings> optAccess, UserManager<AbbAppUser> userManager,
                        SignInManager<AbbAppUser> signInManager, ILogger<AccountController> logger,
                        ITokenManager tokenManager, AbbAppContext abbAppContext, IMapper mapper, 
                        IUserStore<AbbAppUser> userStore, 
                        AbbAppContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _options = optAccess.Value;
            _tokenManager = tokenManager;
            _abbAppContext = abbAppContext;
            _mapper = mapper;
            _abbAppContext = context;
            _userStore = userStore;
        }


        [HttpGet("Userlist")]
        public List<UserDto> GetUserList()
        {

            var users = _userManager.Users.ToList();
            var usersDto = _mapper.Map<List<AbbAppUser>, List<UserDto>>(users);
            return usersDto;
        }


        //[Authorize(Roles = "Guest")]
        [HttpGet("getinfo/")]
        [HttpGet("getinfo/{username}")]
        //[Authorize]
        public async Task<string> GetUserInfo(string username)
        {
            //var user = HttpContext.User;
            //var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var user = (string.IsNullOrEmpty(username)) ? await _userManager.FindByNameAsync(User.Identity.Name) :
                await _userManager.FindByNameAsync(username);
            
            await _userManager.AddToRoleAsync(user, UserRoles.Admin.ToString());
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.FirstOrDefault(x => x.Equals(UserRoles.Admin.ToString())) != null)
            {
                return $"{user.UserName} {String.Join(", ", roles.ToArray())}";
            }

            string resilt = $"No gutest: {user.UserName}: {nameof(UserRoles.Guest)}";
            return resilt;
        }


        [HttpGet("newuser")]
        public async Task<ActionResult<string>> CreateNewUser(string username)
        {
            //var user = HttpContext.User;
            //var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var userExists = await _userManager.FindByNameAsync(User.Identity.Name);
            if (userExists  != null) return BadRequest($"User {userExists.UserName} already exists");


            //IdentityUser user = CreateUser();
            AbbAppUser user = Activator.CreateInstance<AbbAppUser>();
            user.UserName = "alexey2@beliaeff.ru";
            
            user.Email = "alexey2@beliaeff.ru";

            //_passwordHasher.HashPassword(user, Environment.GetEnvironmentVariable("DEFAULTPASS"));
            user.PasswordHash = _passwordHasher.HashPassword(user, "Ab1236523652");
            await _userStore.SetUserNameAsync(user, user.UserName, CancellationToken.None);
            await _userStore.SetUserNameAsync(user, user.UserName, CancellationToken.None);
            // await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user);// "Ab123456789#");

            if(result.Succeeded)
            {
                return Ok($"user {user.UserName} is created");

            } else
            {
                string errors = String.Empty;
                foreach (var error in result.Errors)
                {
                    errors += error.Description;
                }
                return BadRequest(errors);
            }
        }


        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }


    }
}
