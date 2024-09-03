using System.ComponentModel.DataAnnotations;

namespace AbbContentEditor.Models
{
    public class RegisterRequestModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginRequestModel : RegisterRequestModel
    {
        public bool RememberMe { get; set; } = false;
    }

    public class LoginResponseModel
    {
        public string Status { get; set; }
        public string Message { get; set; }

    }
}
