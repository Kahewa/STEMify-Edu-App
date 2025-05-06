namespace STEMify.Models.Quizzes
{
    public class QuizSubmissionViewModel
    {
        public int QuizId { get; set; }
        public List<AnswerViewModel> Answers { get; set; }
    }

}
