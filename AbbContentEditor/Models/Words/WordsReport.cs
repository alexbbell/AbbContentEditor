﻿using Microsoft.AspNetCore.Identity;

namespace AbbContentEditor.Models.Words
{
    public class WordsReport
    {
        public string User { get; set; }
        public double TheTime { get; set; }
        public int Attempts { get; set; }
        public int CorrectAnswers { get; set; }

    }
}
