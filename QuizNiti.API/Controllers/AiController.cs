using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizNiti.API.Models;
using QuizNiti.API.Services;

namespace QuizNiti.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly AiService _aiService;

        public AiController(AiService aiService)
        {
            _aiService = aiService;
        }

        // Model for request body
        public class AiRequest
        {
            public string Topic { get; set; } = string.Empty;
            public string Difficulty { get; set; } = string.Empty;
            public int Count { get; set; } = 10; //Default Question Counts
        }

        // POST: api/ai/AIGeneratedQuestions

        [HttpPost("AIGeneratedQuestions")]
        public async Task<IActionResult> AIGeneratedQuestions([FromBody] AiRequest request) 
        {

            try
            {
                if(string.IsNullOrEmpty(request.Topic))
                    return BadRequest("Topic is required.");
                if (string.IsNullOrEmpty(request.Difficulty))
                    return BadRequest("Difficulty level is required.");

                if (request.Count <= 0) 
                {
                    request.Count = 10; // Default to 5 questions if invalid count provided
                }

                var GeneratedQuestiosAreHere = await _aiService.AIWillGenerateQuestionsAsync(request.Topic, request.Difficulty, request.Count);

                return Ok(new
                {
                    message = $"{GeneratedQuestiosAreHere.Count} questions generated successfully.",
                    SendDataToDatabases = GeneratedQuestiosAreHere
                });
            }
            catch(Exception ex) 
            {
                return BadRequest(new { error = ex.Message });
                // Next plan: Log the error details for further analysis

            }
        }
    }
}
