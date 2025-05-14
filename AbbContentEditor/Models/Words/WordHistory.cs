using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace AbbContentEditor.Models.Words
{
    public class WordHistory
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public bool Correct { get; set; }
        [JsonIgnore]
        public AbbAppUser? IdentityUser { get; set; }
        [JsonIgnore]
        public string IdentityUserId { get; set; }

        public DateTime AnswerTime { get; set; }
    }

    public class WordHistoryDto
    {
        public int Id { get; set; }
        public string? Word { get; set; }
        public bool Correct { get; set; }

        public DateTime AnswerTime { get; set; }
    }
}
