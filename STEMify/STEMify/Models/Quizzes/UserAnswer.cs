using System.ComponentModel.DataAnnotations.Schema;

namespace STEMify.Models
{
    [Table("UserAnswers")]
    public class UserAnswer
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int QuizId { get; set; }
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime AnsweredOn { get; set; } = DateTime.Now;
    }
}
