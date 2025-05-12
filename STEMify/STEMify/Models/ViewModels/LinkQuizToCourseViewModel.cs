using Microsoft.AspNetCore.Mvc.Rendering;

namespace STEMify.Models.ViewModels
{
    public class LinkQuizToCourseViewModel
    {
        public List<Quiz> UnlinkedQuizzes { get; set; }
        public int SelectedCourseID { get; set; }

        public IEnumerable<SelectListItem> CourseSelectList { get; set; }
    }

}
