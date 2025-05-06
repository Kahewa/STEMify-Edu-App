using STEMify.Models;
namespace STEMify.Models.Quizzes
{
    public class QuizSessionViewModel
    {
        public Quiz Quiz { get; set; }
        public IEnumerable<QuizQuestion> Questions { get; set; }
    }
}
