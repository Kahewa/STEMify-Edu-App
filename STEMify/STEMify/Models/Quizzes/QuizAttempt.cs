namespace STEMify.Models.Quizzes
{
    public class QuizAttempt
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int QuizId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Score { get; set; }
    }

    public class QuizAnswer
    {
        public int Id { get; set; }
        public int QuizAttemptId { get; set; }
        public int QuizQuestionId { get; set; }
        public string UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }
}