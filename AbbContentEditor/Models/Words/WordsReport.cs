using Microsoft.AspNetCore.Identity;

namespace AbbContentEditor.Models.Words
{
    public class WordsReport
    {
        public IdentityUser User { get; set; }
        public string Word { get; set; }
        public int Attempts { get; set; }
        public int CorrectAnswers { get; set; }
    }
}
