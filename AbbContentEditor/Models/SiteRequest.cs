using System.ComponentModel.DataAnnotations;

namespace AbbContentEditor.Models
{

    public class SiteRequestDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string TheName { get; set; }
        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; }
        public string? Question { get; set; }
        public DateTime Created  = DateTime.UtcNow;

    }
    public class SiteRequest
    {
        [Key]
        public int Id { get; set; }
        public string TheName { get; set; }
        public string Email {get; set;}
        public string Subject { get; set;}
        public string Question { get; set;}
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }

    }
}
