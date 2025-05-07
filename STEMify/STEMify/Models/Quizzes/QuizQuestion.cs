using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STEMify.Models
{
    [Table("QuizQuestion")]
    public class QuizQuestion
    {
        public int Id { get; set; }
        public int? QuizId { get; set; }
        public string QuestionText { get; set; }
        public int QuestionType { get; set; } // "MultipleChoice", "FillInTheBlank", "True/False"
    }

    public class MultipleChoiceQuestion
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAnswer { get; set; }
    }

    public class FillInTheBlankQuestion
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public string StatementText { get; set; }
        public string CorrectAnswer { get; set; }
    }

    public class TrueFalseQuestion
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public string StatementText { get; set; }
        [Required(ErrorMessage = "Please select the correct answer.")]
        public bool Answer { get; set; }
    }
}