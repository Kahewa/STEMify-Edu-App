namespace STEMify.Models
{
    public class QuizResult
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int QuizId { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public DateTime AttemptedOn { get; set; } = DateTime.Now;
    }
}