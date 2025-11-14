namespace QuizNiti.API.DTOs
{
    public class AI_MCQs
    {
        public string QuestionText { get; set; } = string.Empty;
        public required List<string> Options { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
        public string DidYouKnow { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;

        public string OptionD { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;


    }
}
