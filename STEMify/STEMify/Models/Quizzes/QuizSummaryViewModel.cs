namespace STEMify.Models.Quizzes
{
    public class QuizSummaryViewModel
    {
        public QuizAttempt QuizAttempt { get; set; }
        public IEnumerable<UserAnswer> UserAnswers { get; set; }
        public IEnumerable<QuizQuestion> Questions { get; set; }
    }

}
