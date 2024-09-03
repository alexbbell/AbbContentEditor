using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AbbContentEditor.Data
{
    public class CreateDefaultData
    {
        private readonly AbbAppContext _context;
        private readonly PasswordHasher<IdentityUser> _passwordHasher = new PasswordHasher<IdentityUser>();
        public CreateDefaultData(AbbAppContext context)
        {
            _context = context;
            var iUser = context.Users.FirstOrDefault(u => u.UserName.Equals("alexey@beliaeff.ru"));
            if (iUser == null ) {
                CreateDefaultUser();
            }

        }

        public void CreateDefaultUser ()
        {
            IdentityUser user = new IdentityUser () {
                    UserName = "alexey@beliaeff.ru",
                    Email = "alexey@beliaeff.ru",
                    NormalizedUserName = "ALEXEY@BELIAEFF.RU",
                    NormalizedEmail = "ALEXEY@BELIAEFF.RU",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    
                    };
            _context.Users.Add (user); 
            var hashedPassword = _passwordHasher.HashPassword(user, "Ab123456789$");
            user.PasswordHash = hashedPassword;
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.TwoFactorEnabled = false;
            user.NormalizedEmail = user.Email.Normalize();
            _context.SaveChanges();

        }
    }
}
