namespace STEMify.Models.Quizzes
{
    public class QuizSessionViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public List<QuestionViewModel> Questions { get; set; }
        public int AttemptId { get; set; }

    }

    public class QuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int QuestionType { get; set; }
        public List<OptionViewModel> Options { get; set; } = new List<OptionViewModel>();
    }

    public class OptionViewModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class UserAnswerViewModel
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
    }

    public class QuizSubmissionViewModel
    {
        public int QuizId { get; set; }
        public int AttemptId { get; set; }
        public List<AnswerInputModel> Answers { get; set; }
    }

    public class AnswerInputModel
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
    }


    public class QuizSummaryViewModel
    {
        public QuizAttempt QuizAttempt { get; set; }
        public IEnumerable<UserAnswer> UserAnswers { get; set; }
        public IEnumerable<QuizQuestion> Questions { get; set; }
    }
}