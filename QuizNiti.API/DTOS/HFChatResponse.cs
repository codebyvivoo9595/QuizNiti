namespace QuizNiti.API.DTOs
{
    public class HFChatResponse
    {
        public int index { get; set; }
        public HFMessage message { get; set; }
    }

    public class HFMessage
    {
        public string? role { get; set; }
        public string? content { get; set; }
    }
}
