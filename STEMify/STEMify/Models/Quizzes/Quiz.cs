using System.ComponentModel.DataAnnotations.Schema;

namespace STEMify.Models
{
    [Table("Quiz")]
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int? CourseID { get; set; }

        // Navigation property
        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }
    }
}