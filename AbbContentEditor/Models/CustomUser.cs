using System.ComponentModel.DataAnnotations;

namespace AbbContentEditor.Models
{
    // I don't really need if I use this model later
    public class CustomUser
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Boolean? IsEmailConfirmed { get; set; }
        public DateTime RegDate { get; set; }
        public Roles? Role { get; set; }

    }

    public enum Roles
    {
        Guest,
        User,
        Admin,
    }

}
