using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AbbContentEditor.Models.Words
{
    /// <summary>
    /// One run of the word exercise: everything between "Start Session" and "End Session" / timeout.
    /// Answers given during the run are stored as <see cref="WordHistory"/> rows linked to it.
    /// </summary>
    public class ExerciseSession
    {
        public int Id { get; set; }

        [JsonIgnore]
        public string IdentityUserId { get; set; } = string.Empty;
        [JsonIgnore]
        public AbbAppUser? IdentityUser { get; set; }

        /// <summary>Word collection the exercise was run on (null for ad-hoc / demo exercises).</summary>
        public int? WordCollectionId { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }

        public List<WordHistory> Answers { get; set; } = new();
    }

    // ---------- Request DTOs ----------

    public class StartExerciseSessionDto
    {
        public int? CollectionId { get; set; }
    }

    public class SaveAnswerDto
    {
        /// <summary>Id of the asked word inside the collection (optional, used to group stats per word).</summary>
        [MaxLength(64)]
        public string? WordId { get; set; }

        /// <summary>The word that was asked (source language).</summary>
        [Required, MaxLength(500)]
        public string Word { get; set; } = string.Empty;

        /// <summary>The correct translation.</summary>
        [MaxLength(500)]
        public string? CorrectAnswer { get; set; }

        /// <summary>The translation the user picked.</summary>
        [MaxLength(500)]
        public string? SelectedAnswer { get; set; }

        public bool Correct { get; set; }
    }

    // ---------- Response DTOs ----------

    public class ExerciseSessionDto
    {
        public int Id { get; set; }
        public int? CollectionId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }

    public class ExerciseAnswerDto
    {
        public int Id { get; set; }
        public string? WordId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string? CorrectAnswer { get; set; }
        public string? SelectedAnswer { get; set; }
        public bool Correct { get; set; }
        public DateTime AnswerTime { get; set; }
    }

    /// <summary>Per-word aggregation inside one session.</summary>
    public class WordStatDto
    {
        public string? WordId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string? CorrectAnswer { get; set; }
        public int Attempts { get; set; }
        public int Correct { get; set; }
        public int Wrong { get; set; }
    }

    public class ExerciseSessionStatsDto
    {
        public int SessionId { get; set; }
        public int? CollectionId { get; set; }
        public string User { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public bool IsFinished { get; set; }
        public double DurationSeconds { get; set; }

        public int TotalAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        /// <summary>Percentage 0..100, rounded to one decimal.</summary>
        public double Accuracy { get; set; }

        /// <summary>Words answered wrong at least once, most mistakes first.</summary>
        public List<WordStatDto> MistakenWords { get; set; } = new();
        /// <summary>Every answer in chronological order.</summary>
        public List<ExerciseAnswerDto> Answers { get; set; } = new();
    }

    /// <summary>Totals across all sessions of the user (optionally of one collection).</summary>
    public class ExerciseOverviewDto
    {
        public int? CollectionId { get; set; }
        public int SessionCount { get; set; }
        public int TotalAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public double Accuracy { get; set; }
        public double TotalDurationSeconds { get; set; }
        public DateTime? LastSessionAt { get; set; }
        /// <summary>Words with the most wrong answers over all sessions.</summary>
        public List<WordStatDto> MostMistakenWords { get; set; } = new();
    }

    /// <summary>Short row for the list of past sessions.</summary>
    public class ExerciseSessionSummaryDto
    {
        public int SessionId { get; set; }
        public int? CollectionId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int TotalAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public double Accuracy { get; set; }
    }
}
