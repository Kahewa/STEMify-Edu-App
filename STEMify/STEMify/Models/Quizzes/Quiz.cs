using System.ComponentModel.DataAnnotations.Schema;

namespace STEMify.Models
{
    [Table("Quiz")]
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}