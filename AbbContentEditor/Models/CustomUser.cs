using Microsoft.AspNetCore.Identity;
using NLog.LayoutRenderers;
using System.ComponentModel.DataAnnotations;

namespace AbbContentEditor.Models
{
    // I don't really need if I use this model later
    public class AbbAppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime RegDate { get; set; }
        public Roles? Role { get; set; }

    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }


    public class CreateUserModel : UserDto
    {
        public Guid? Id { get; set; }
        
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string? ErrorText { get; set; }
        public CreateUserModel()
        {
            ErrorText = (Password != ConfirmPassword) ? "Passowods do not match" : String.Empty;
        }
    }
    public class UpdateUserModel : UserDto
    {
        public Guid? Id { get; set; }

        public string UserName { get; set; }

    }


    public enum Roles
    {
        Guest,
        User,
        Admin,
    }

}
