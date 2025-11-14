using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizNiti.API.Models
{
    public class Questions
    {
        public int Id { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        //We getting Response Options[A,B,C,D] to store temp. 
        [NotMapped]
        public List<string>? Options { get; set; }

        [Required]
        public string OptionA { get; set; } = string.Empty;

        [Required]
        public string OptionB { get; set; } = string.Empty;

        [Required]
        public string OptionC { get; set; } = string.Empty;

        [Required]
        public string OptionD { get; set; } = string.Empty;

        [Required]
        public string CorrectAnswer { get; set; } = string.Empty;

        public string  Difficulty { get; set; }  = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [MaxLength(5000)]
        public string DidYouKnow { get; set; } = string.Empty;
    }
}
