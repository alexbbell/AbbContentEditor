using Microsoft.AspNetCore.Identity;

namespace AbbContentEditor.Models
{
    public class AbbAppUserRole : IdentityRole<string>
    {
        public string? Description { get; set; }
    }
}
