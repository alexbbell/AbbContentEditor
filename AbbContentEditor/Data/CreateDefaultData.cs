using AbbContentEditor.Models;
using AbbContentEditor.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;


namespace AbbContentEditor.Data
{
    public class CreateDefaultData
    {
        private readonly UserManager<AbbAppUser> _userManager ;
        private readonly RoleManager<AbbAppUserRole> _roleManager;
        private readonly PasswordHasher<AbbAppUser> _passwordHasher = new PasswordHasher<AbbAppUser>();
        private readonly IUserStore<AbbAppUser> _userStore;
        public CreateDefaultData(UserManager<AbbAppUser> userManager,
            IUserStore<AbbAppUser> userStore,
            RoleManager<AbbAppUserRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;

            //if( context.Database.IsSqlite())
            //{
            //    Batteries.Init();
            //}
        }

        public async Task InitializeAsync()
        {
            var existingUser = await _userManager.FindByEmailAsync("alexey@beliaeff.ru");
            if (existingUser == null)
            {
                Console.WriteLine("User doesn't exist");
                await CreateDefaultUser();
            }
        }

        public async Task CreateDefaultUser ()
        {

            var roles = new[] { UserRoles.Guest.ToString(), UserRoles.Contributor.ToString(), UserRoles.Admin.ToString()};
            
            foreach(var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    await _roleManager.CreateAsync(new AbbAppUserRole { Id = Guid.NewGuid().ToString(), 
                        Name = role.ToString(),
                        NormalizedName = role.ToUpperInvariant(),
                        Description = $"{role.ToString()} role" });
                }
            }
            Console.WriteLine("Method to create a user");
            var newUser = Activator.CreateInstance<AbbAppUser>();
            try
            {
                AbbAppUser user = new AbbAppUser () {
                        UserName = "alexey@beliaeff.ru",
                        FirstName = "Aleksei", 
                        LastName = "Beliaev",
                        Email = "alexey@beliaeff.ru",
                        };
                var hashedPassword = Environment.GetEnvironmentVariable("DEFAULTPASS");
                newUser.FirstName = user.FirstName;
                newUser.LastName = user.LastName;
                newUser.RegDate = DateTime.UtcNow;
                await _userStore.SetUserNameAsync(newUser, user.UserName, CancellationToken.None);
                await _userManager.SetEmailAsync(newUser, user.Email);
  
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

                await _userManager.ConfirmEmailAsync(newUser, token);

                var result = await _userManager.CreateAsync(newUser, hashedPassword);

                if(result.Succeeded) Console.WriteLine("User is created");
                var addRoleResult = await _userManager.AddToRoleAsync(newUser, nameof(UserRoles.Admin));

                if (addRoleResult.Succeeded)
                {
                    Console.WriteLine($"User {newUser.Email} is added to the Role {nameof(UserRoles.Admin)}");
                }
                else
                {
                    Console.WriteLine($"Failed to assign role: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error for User {ex.Message}");
            }

        }
    }
}
