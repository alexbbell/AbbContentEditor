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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PasswordHasher<AbbAppUser> _passwordHasher = new PasswordHasher<AbbAppUser>();
        private readonly IUserStore<AbbAppUser> _userStore;
        public CreateDefaultData(UserManager<AbbAppUser> userManager,
            IUserStore<AbbAppUser> userStore,
            RoleManager<IdentityRole> roleManager)
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

            var roles = new[] { UserRoles.Guest.ToString(), UserRoles.Contributor.ToString(),
            UserRoles.Admin.ToString()};
            Console.WriteLine("Method to create a user");
            var newUser = Activator.CreateInstance<AbbAppUser>();
            try
            {
                AbbAppUser user = new AbbAppUser () {
                        UserName = "alexey@beliaeff.ru",
                        FirstName = "Aleksei", 
                        LastName = "Beliaev",
                        Email = "alexey@beliaeff.ru",
                        //NormalizedUserName = "ALEXEY@BELIAEFF.RU",
                        //NormalizedEmail = "ALEXEY@BELIAEFF.RU",
                        EmailConfirmed = true,
                        LockoutEnabled = false,
                    
                        };
                //var hashedPassword = _passwordHasher.HashPassword(user, Environment.GetEnvironmentVariable("DEFAULTPASS"));
                var hashedPassword = Environment.GetEnvironmentVariable("DEFAULTPASS");
                newUser.FirstName = user.FirstName;
                newUser.LastName = user.LastName;
                newUser.RegDate = DateTime.UtcNow;
                await _userStore.SetUserNameAsync(newUser, user.UserName, CancellationToken.None);
                
                                
                var result = await _userManager.CreateAsync(newUser, hashedPassword);

                if(result.Succeeded) Console.WriteLine("User is created");
            } catch(Exception ex)
            {
                Console.WriteLine($"error for User {ex.Message}");
            }

        }
    }
}
