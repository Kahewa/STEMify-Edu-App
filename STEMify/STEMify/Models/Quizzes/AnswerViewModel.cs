namespace STEMify.Models.Quizzes
{
    public class AnswerViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string SelectedAnswer { get; set; }
        public int QuestionType { get; set; } // For rendering logic
    }
}