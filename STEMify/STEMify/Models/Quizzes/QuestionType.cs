using System.ComponentModel.DataAnnotations.Schema;

namespace STEMify.Models
{
    [Table("QuestionType")]
    public class QuestionType
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }
}