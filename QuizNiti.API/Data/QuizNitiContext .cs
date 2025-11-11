using Microsoft.EntityFrameworkCore;
using QuizNiti.API.Models;
using System.Collections.Generic;

namespace QuizNiti.API.Data
{
    public class QuizNitiContext : DbContext
    {
        public QuizNitiContext(DbContextOptions<QuizNitiContext> options) : base(options)
        {
        }

        public DbSet<Questions> QuestionsTable { get; set; }
    }
}
