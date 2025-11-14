namespace QuizNiti.API.DTOS
{
    public class HuggingFaceDTO
    {

        public class HFChatRoot
        {
            public List<HFChoice> choices { get; set; }
        }

        public class HFChoice
        {
            public int index { get; set; }
            public HFMessage message { get; set; }
        }

        public class HFMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

    }
}
