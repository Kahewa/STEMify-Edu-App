using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STEMify.Models;
using STEMify.Data.Interfaces;
using STEMify.Models.User;

namespace STEMify.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }


        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult Dashboard() 
        {
            return View();
        }

        [Authorize]
        public ActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }
        [Authorize]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            return View();
        }
        [Authorize]
        public IActionResult Courses()
        {
            var AvailableCourses = UnitOfWork.Courses.GetAll().ToList();

            var currentUser = User.Identity?.Name;

            // If user is authenticated, get their favorited courses
            if(!string.IsNullOrEmpty(currentUser))
            {
                var favoriteIds = UnitOfWork.UserCourses
                    .GetAll()
                    .Where(x => x.User == currentUser)
                    .Select(x => x.CourseID)
                    .ToList();

                ViewBag.UserCourseIds = favoriteIds;
            }
            else
            {
                ViewBag.UserCourseIds = new List<int>();
            }

            return View("AvailableCourses", AvailableCourses);
        }

        [Authorize]
        // FavoriteCourse - Adds course to user favorites if not already added
        public async Task<IActionResult> FavoriteCourse(int id)
        {
            var course = await UnitOfWork.Courses.GetAsync(id); 

            if(course == null)
            {
                return NotFound();
            }

            var existingEntry = UnitOfWork.UserCourses
                .GetAll()
                .FirstOrDefault(x => x.User == User.Identity.Name && x.CourseID == id);

            if(existingEntry == null)
            {
                var userCourse = new UserCourses
                {
                    CourseID = course.CourseID,
                    User = User.Identity.Name
                };

                UnitOfWork.UserCourses.Add(userCourse);
                UnitOfWork.Complete();
            }

            return RedirectToAction("Courses");
        }
        [Authorize]
        // Displays all user-favorited courses
        public IActionResult UserCourses()
        {
            var userCourseIds = UnitOfWork.UserCourses
                .GetAll()
                .Where(x => x.User == User.Identity.Name)
                .Select(x => x.CourseID)
                .ToList();

            var courses = UnitOfWork.Courses
                .GetAll()
                .Where(x => userCourseIds.Contains(x.CourseID))
                .ToList();

            return View(courses);
        }
        // RemoveFavoriteCourse - Removes a course from user favorites
        [HttpPost]
        [Authorize]
        public IActionResult RemoveFavoriteCourse(int id)
        {
            var favorite = UnitOfWork.UserCourses
                .GetAll()
                .FirstOrDefault(x => x.User == User.Identity.Name && x.CourseID == id);

            if(favorite == null)
            {
                return NotFound();
            }

            UnitOfWork.UserCourses.Remove(favorite);
            UnitOfWork.Complete();

            return RedirectToAction("UserCourses");
        }
        
        [Authorize]
        public IActionResult AllQuizzes()
        {
            var Quizzes = UnitOfWork.Quizzes.GetAll().ToList();
            return View(Quizzes);
        }
        public IActionResult Error()
        {
            return View();
        }

    }
}
