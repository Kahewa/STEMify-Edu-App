using STEMify.Models;
using Microsoft.EntityFrameworkCore;
using STEMify.Models.User;
using STEMify.Models.Quizzes;

namespace STEMify.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<DifficultyLevel> DifficultyLevels { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<MultipleChoiceQuestion> MultipleChoiceQuestions { get; set; }
        public DbSet<FillInTheBlankQuestion> FillInTheBlankQuestions { get; set; }
        public DbSet<TrueFalseQuestion> TrueFalseQuestions { get; set; }

        public DbSet<QuestionType> QuestionTypes { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }
        public DbSet<UserCourses> UserCourses { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }


    }

}
