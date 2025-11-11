using System;
using System.ComponentModel.DataAnnotations;

namespace QuizNiti.API.DTOs
{
    public class CreateQuestionDto
    {
        [Required, MinLength(1)]
        public string QuestionText { get; set; } = string.Empty;

        [Required, MinLength(1)]
        public string OptionA { get; set; } = string.Empty;

        [Required, MinLength(1)]
        public string OptionB { get; set; } = string.Empty;

        [Required, MinLength(1)]
        public string OptionC { get; set; } = string.Empty;

        [Required, MinLength(1)]
        public string OptionD { get; set; } = string.Empty;

        [Required, MinLength(1)]
        public string CorrectAnswer { get; set; } = string.Empty;

        [Required]
        public string Difficulty { get; set; } = "Easy";  

        [Required, MinLength(1)]
        public string Topic { get; set; } = "General";

        public DateTime? DateCreated { get; set; }
    }
}
