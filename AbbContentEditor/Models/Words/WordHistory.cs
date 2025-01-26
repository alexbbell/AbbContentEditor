using Microsoft.AspNetCore.Identity;

namespace AbbContentEditor.Models.Words
{
    public class WordHistory
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public bool Correct { get; set; }

        public IdentityUser? IdentityUser { get; set; }
        public string IdentityUserId { get; set; }

        public DateTime AnswerTime { get; set; }
    }

    public class WordHistoryDto
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public bool Correct { get; set; }

        public string IdentityUserId { get; set; }

        public DateTime AnswerTime { get; set; }
    }
}
