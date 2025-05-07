namespace STEMify.Models
{
    public class CompletenessTracker
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public int CompletedQuestions { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}