using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AbbContentEditor.Models
{
    public class Countdown
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedAt { get; set; }

    }
    public class CountDownRequest
    {
        public string UserName { get; set; }
        public string Action { get; set; }
    }
}
