using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizNiti.API.Data;
using QuizNiti.API.DTOs;
using QuizNiti.API.Models;

namespace QuizNiti.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly QuizNitiContext _db;
        private readonly ILogger<QuestionsController> _logger;

        public QuestionsController(QuizNitiContext db, ILogger<QuestionsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: api/questions
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? date, [FromQuery] string? difficulty, [FromQuery] string? topic)
        {
            var query = _db.QuestionsTable.AsQueryable();

            if (date.HasValue)
            {
                var d = date.Value.Date;
                query = query.Where(q => q.DateCreated.Date == d);
            }

            if (!string.IsNullOrWhiteSpace(difficulty))
            {
                query = query.Where(q => q.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(topic))
            {
                query = query.Where(q => q.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase));
            }

            var list = await query.OrderByDescending(q => q.DateCreated).ToListAsync();
            return Ok(list);
        }

        // GET: api/questions/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var q = await _db.QuestionsTable.FindAsync(id);
            if (q == null) return NotFound();
            return Ok(q);
        }

        // POST: api/questions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuestionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = new Questions
            {
                QuestionText = dto.QuestionText,
                OptionA = dto.OptionA,
                OptionB = dto.OptionB,
                OptionC = dto.OptionC,
                OptionD = dto.OptionD,
                CorrectAnswer = dto.CorrectAnswer,
                Difficulty = dto.Difficulty,
                Topic = dto.Topic,
                DateCreated = dto.DateCreated ?? DateTime.UtcNow
            };

            _db.QuestionsTable.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        // DELETE: api/questions/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var q = await _db.QuestionsTable.FindAsync(id);
            if (q == null) return NotFound();

            _db.QuestionsTable.Remove(q);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/questions/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateQuestionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var q = await _db.QuestionsTable.FindAsync(id);
            if (q == null) return NotFound();

            q.QuestionText = dto.QuestionText;
            q.OptionA = dto.OptionA;
            q.OptionB = dto.OptionB;
            q.OptionC = dto.OptionC;
            q.OptionD = dto.OptionD;
            q.CorrectAnswer = dto.CorrectAnswer;
            q.Difficulty = dto.Difficulty;
            q.Topic = dto.Topic;
            q.DateCreated = dto.DateCreated ?? q.DateCreated;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // (Optional) POST: api/questions/bulk
        // Accepts array of DTOs to insert many questions at once
        [HttpPost("CreateBulk")]
        public async Task<IActionResult> CreateBulk([FromBody] CreateQuestionDto[] dtos)
        {
            if (dtos == null || dtos.Length == 0) return BadRequest("No items provided.");

            var MoreQuestions = dtos.Select(dto => new Questions
            {
                QuestionText = dto.QuestionText,
                OptionA = dto.OptionA,
                OptionB = dto.OptionB,
                OptionC = dto.OptionC,
                OptionD = dto.OptionD,
                CorrectAnswer = dto.CorrectAnswer,
                Difficulty = dto.Difficulty,
                Topic = dto.Topic,
                DateCreated = dto.DateCreated ?? DateTime.UtcNow
            }).ToList();

            _db.QuestionsTable.AddRange(MoreQuestions);
            await _db.SaveChangesAsync();
            return Ok(new { inserted = MoreQuestions.Count });
        }


        public class SearchRequest
        {
            public string Topic { get; set; } = string.Empty;
            public string? Difficulty { get; set; }
        }

        [HttpPost("SearchByTopicPost")]
        public async Task<IActionResult> SearchByTopicPost([FromBody] SearchRequest req)
        {
            var query = _db.QuestionsTable.AsQueryable();

            if (!string.IsNullOrEmpty(req.Topic))
                query = query.Where(q => q.Topic.Equals(req.Topic, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(req.Difficulty))
                query = query.Where(q => q.Difficulty.Equals(req.Difficulty, StringComparison.OrdinalIgnoreCase));

            var list = await query.ToListAsync();
            if (list.Count == 0) return NotFound("No questions found.");

            return Ok(list);
        }





    }
}
