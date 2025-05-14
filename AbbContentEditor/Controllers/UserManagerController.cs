using AbbContentEditor.Data;
using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models;
using AbbContentEditor.Models.Enums;
using AbbContentEditor.Models.Words;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagerController : ControllerBase
    {
        private readonly AbbAppContext _abbAppContext;
        private readonly IRepository<WordCollection> _wordColelctionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<WordReportController> _logger;
        private readonly UserManager<AbbAppUser> _userManager;
        private readonly IUserStore<AbbAppUser> _userStore;
        //private readonly IUserEmailStore<IdentityUser> _emailStore;



        public UserManagerController(IRepository<WordCollection> wordColelctionRepository, IUnitOfWork unitOfWork, IMapper mapper,
           ILogger<WordReportController> logger, UserManager<AbbAppUser> userManager, AbbAppContext abbAppContext,
            IUserStore<AbbAppUser> userStore 
            //IUserEmailStore<IdentityUser> emailStore
            )
        {
            _wordColelctionRepository = wordColelctionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _userStore = userStore;
            _abbAppContext = abbAppContext;
            //_emailStore = emailStore;
        }

        [HttpGet("Userlist")]
        public List<UserDto> GetUserList()
        {

            var users = _userManager.Users.ToList();
            

            var usersDto = _mapper.Map<List<AbbAppUser>, List<UserDto>>(users);

            return usersDto;
        }

        [HttpPost("add")]
        public async Task<ActionResult<string>> AddNewUser([FromBody] CreateUserModel user)
        {

            if(await _userManager.FindByEmailAsync(user.Email) != null)
            {
                return BadRequest("User Exists");
            }

            var newUser = Activator.CreateInstance<AbbAppUser>();
            await _userStore.SetUserNameAsync(newUser, user.UserName,CancellationToken.None );
            //await _emailStore.SetEmailAsync(newUser, user.Email,CancellationToken.None );

            var result = await _userManager.CreateAsync(newUser, user.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(newUser);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                return Ok($"User {user.Email} created {code}");
            } else
            {
                return BadRequest(result.Errors);
            }
        }


        [HttpPut("update")]
        public async Task<ActionResult<string>> UpdateUser([FromBody] UpdateUserModel userModel)
        {
            
            var user = await _userManager.FindByIdAsync(userModel.Id.ToString());
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Email = userModel.Email;
            user.UserName = userModel.UserName;
            user.TwoFactorEnabled = userModel.TwoFactorEnabled;


            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }

           
            return Ok($"User {user.Email} updated successfully");
        }


        [HttpPut("updatepassword")]
        public async Task<ActionResult<string>> UpdateUserPassword([FromBody] CreateUserModel userModel)
        {

            var user = await _userManager.FindByIdAsync(userModel.Id.ToString());
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.UserName = userModel.UserName ?? user.UserName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }

            if (!string.IsNullOrEmpty(userModel.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, userModel.Password);
                if (!passwordResult.Succeeded)
                {
                    return BadRequest(passwordResult.Errors);
                }
            }

            return Ok($"User {user.Email} updated successfully");
        }



        //[Authorize(Roles = "Guest")]
        [HttpGet("getinfo")]
        [HttpGet("getinfo/{userId}")]
        //[Authorize]
        public async Task<ActionResult<UserDto>> GetUserInfo(string userId)
        {
            
            //var user = HttpContext.User;
            var user = ( userId != null ) ? await _userManager.FindByIdAsync(userId) : await _userManager.FindByIdAsync(userId);
            // await _userManager.AddToRoleAsync(user, UserRoles.Admin.ToString());
            var roles = await _userManager.GetRolesAsync(user);

                var rsult = _mapper.Map<UserDto>(user);
            if (roles.FirstOrDefault(x => x.Equals(UserRoles.Admin.ToString())) != null)
            {
                return Ok(rsult);
            }

            string resilt = $"No gutest: {user.UserName}: {nameof(UserRoles.Guest)}";
            if(user != null ) return Ok(rsult);
            return BadRequest();
        }

    }
}
