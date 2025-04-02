using AbbContentEditor.Models.Enums;
using Microsoft.AspNetCore.Identity;
using SQLitePCL;


namespace AbbContentEditor.Data
{
    public class CreateDefaultData
    {
        private readonly AbbAppContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PasswordHasher<IdentityUser> _passwordHasher = new PasswordHasher<IdentityUser>();
        public CreateDefaultData(AbbAppContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
            Batteries.Init();
            var iUser = context.Users.FirstOrDefault(u => u.UserName.Equals("alexey@beliaeff.ru"));
            if (iUser == null ) {
                CreateDefaultUser();
            }

        }

        public async Task CreateDefaultUser ()
        {

            var roles = new[] { UserRoles.Guest.ToString(), UserRoles.Contributor.ToString(),
            UserRoles.Admin.ToString()};
            foreach(var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            IdentityUser user = new IdentityUser () {
                    UserName = "alexey@beliaeff.ru",
                    Email = "alexey@beliaeff.ru",
                    NormalizedUserName = "ALEXEY@BELIAEFF.RU",
                    NormalizedEmail = "ALEXEY@BELIAEFF.RU",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    
                    };
            _context.Users.Add (user); 
            var hashedPassword = _passwordHasher.HashPassword(user, Environment.GetEnvironmentVariable("DEFAULTPASS"));
            user.PasswordHash = hashedPassword;
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.TwoFactorEnabled = false;
            user.NormalizedEmail = user.Email.Normalize();
            _context.SaveChanges();

        }
    }
}
