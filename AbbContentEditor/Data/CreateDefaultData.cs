using AbbContentEditor.Models;
using AbbContentEditor.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace AbbContentEditor.Data
{
    public class CreateDefaultData
    {
        private readonly UserManager<AbbAppUser> _userManager;
        private readonly RoleManager<AbbAppUserRole> _roleManager;
        private readonly IUserStore<AbbAppUser> _userStore;

        public CreateDefaultData(
            UserManager<AbbAppUser> userManager,
            IUserStore<AbbAppUser> userStore,
            RoleManager<AbbAppUserRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
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

        public async Task CreateDefaultUser()
        {
            var roles = new[] {
                UserRoles.Guest.ToString(),
                UserRoles.Contributor.ToString(),
                UserRoles.Admin.ToString()
            };

            // 1. Ensure Roles Exist with UNIQUE IDs
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new AbbAppUserRole
                    {
                        // Omit Id completely, or assign Guid.NewGuid().ToString() so every role gets a unique key
                        Id = Guid.NewGuid().ToString(), // ✅ Gives each role a unique primary key
                        Name = role,
                        NormalizedName = role.ToUpperInvariant(),
                        Description = $"{role} role"
                    });
                }
            }

            const string defaultEmail = "alexey@beliaeff.ru";

            try
            {
                // 2. Check if default user already exists
                var existingUser = await _userManager.FindByEmailAsync(defaultEmail);
                if (existingUser != null)
                {
                    Console.WriteLine($"Default user {defaultEmail} already exists.");
                    return;
                }

                // 3. Retrieve Environment Variable Password
                var hashedPassword = Environment.GetEnvironmentVariable("DEFAULTPASS");
                if (string.IsNullOrEmpty(hashedPassword))
                {
                    Console.WriteLine("DEFAULTPASS environment variable is missing or empty.");
                    return;
                }

                // 4. Instantiate and Populate User
                var newUser = new AbbAppUser
                {
                    UserName = defaultEmail,
                    Email = defaultEmail,
                    FirstName = "Aleksei",
                    LastName = "Beliaev",
                    RegDate = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                // 5. Create User
                var result = await _userManager.CreateAsync(newUser, hashedPassword);

                if (!result.Succeeded)
                {
                    Console.WriteLine($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return;
                }

                Console.WriteLine($"User {newUser.Email} created successfully.");

                // 6. Assign Admin Role
                var adminRoleName = UserRoles.Admin.ToString();
                var addRoleResult = await _userManager.AddToRoleAsync(newUser, adminRoleName);

                if (addRoleResult.Succeeded)
                {
                    Console.WriteLine($"User {newUser.Email} added to role: {adminRoleName}");
                }
                else
                {
                    Console.WriteLine($"Failed to assign role: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating default user: {ex.Message}");
            }
        }
    }
}