using AbbContentEditor.Controllers;
using AbbContentEditor.Data;
using AbbContentEditor.Helpers;
using AbbContentEditor.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AbbContentEditor.Tests
{
    public class Tests
    {

        private readonly AccountController _controller;
        private readonly Mock<IOptions<JWTSettings>> _jwtOptions;
        private readonly Mock<UserManager<AbbAppUser>> _userManager;
        private readonly Mock<SignInManager<AbbAppUser>> _signInManager;
        private readonly Mock<ILogger<AccountController>> _logger;
        private readonly Mock<ITokenManager> _tokenManager;
        private readonly Mock<AbbAppContext> _context;
        private readonly Mock<IMapper> _mapper;


        public Tests()
        {

            var options = new DbContextOptionsBuilder<AbbAppContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context = new Mock<AbbAppContext>(options);

            var userStore = new Mock<IUserStore<AbbAppUser>>();
            _userManager = new Mock<UserManager<AbbAppUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            _signInManager = new Mock<SignInManager<AbbAppUser>>(
                _userManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<AbbAppUser>>().Object,
                null, null, null, null);

            _jwtOptions = new Mock<IOptions<JWTSettings>>();
            _logger = new Mock<ILogger<AccountController>>();
            _tokenManager = new Mock<ITokenManager>();
            _mapper = new Mock<IMapper>();

            // Create controller with all dependencies
            _controller = new AccountController(
                _jwtOptions.Object,
                _userManager.Object,
                _signInManager.Object,
                _logger.Object,
                _tokenManager.Object,
                _context.Object,
                _mapper.Object
            );

        }
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var model = new LoginRequestModel
            {
                Email = "test@example.com",
                Password = "P@ssw0rd",
                RememberMe = false
            };

            var user = new AbbAppUser { Id = "123", Email = model.Email, UserName = model.Email };

            _signInManager.Setup(m => m.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _userManager.Setup(m => m.FindByNameAsync(model.Email)).ReturnsAsync(user);
            _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

            _tokenManager.Setup(m => m.GenerateAccessToken(model.Email, It.IsAny<IList<string>>()))
                .Returns("access_token");

            _tokenManager.Setup(m => m.GenerateRefreshToken()).Returns("refresh_token");

            _context.Setup(c => c.UserTokens.Add(It.IsAny<IdentityUserToken<string>>()));
            _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _controller.Login(model);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;

            Assert.That(okResult.Value, Is.TypeOf<AuthenticationResponse>());
            var response = okResult.Value as AuthenticationResponse;

            Assert.That(response.AccessToken, Is.EqualTo("access_token"));
            Assert.That(response.RefreshToken, Is.EqualTo("refresh_token"));
        }






    }
}
