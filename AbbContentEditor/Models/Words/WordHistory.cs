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

        /// <summary>Exercise session this answer belongs to (null for legacy rows).</summary>
        public int? ExerciseSessionId { get; set; }
        [JsonIgnore]
        public ExerciseSession? ExerciseSession { get; set; }

        public string? WordId { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? SelectedAnswer { get; set; }
    }

    public class WordHistoryDto
    {
        public int Id { get; set; }
        public string? Word { get; set; }
        public bool Correct { get; set; }

        public DateTime AnswerTime { get; set; }
    }
}
