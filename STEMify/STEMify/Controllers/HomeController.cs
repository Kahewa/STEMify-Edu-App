using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STEMify.Models;
using STEMify.Data.Interfaces;
using STEMify.Models.User;
using Microsoft.EntityFrameworkCore;

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

            var userCourseIds = UnitOfWork.UserCourses
                .GetAll()
                .Where(x => x.User == User.Identity.Name)
                .Select(x => x.CourseID)
                .ToList();

            var courses = UnitOfWork.Courses
                .GetAll()
                .Where(x => userCourseIds.Contains(x.CourseID))
                .ToList();

            var userId = User.Identity.Name;

            // Total number of quiz submissions by the user (not distinct)
            var totalAnswers = UnitOfWork.UserAnswers.GetAll()
                .Where(x => x.UserId == userId).Count();

            // Total quizzes in the system
            var totalQuizzes = UnitOfWork.Quizzes.GetAll().Count();

            // Number of distinct quizzes attempted by the user
            var quizzesAttempted = UnitOfWork.UserAnswers.GetAll()
                .Where(x => x.UserId == userId)
                .Select(x => x.QuizId)
                .Distinct()
                .Count();

            ViewBag.TotalQuizzes = totalQuizzes;
            ViewBag.QuizzesCompleted = totalAnswers;
            ViewBag.QuizzesAttempted = quizzesAttempted;
            ViewBag.Courses = courses;

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

        public ActionResult Details(int id)
        {
            var course = UnitOfWork.Courses.Get(id);

            if(course == null)
            {
                return NotFound();
            }
            ViewBag.Quizzes = UnitOfWork.Quizzes.GetAll().Where(q => q.CourseID == id).ToList();
            return View(course);
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
        [HttpGet]
        public IActionResult Search(string term)
        {
            var results = UnitOfWork.Courses.GetAll()
                .Where(c => c.CourseName.Contains(term) || c.Description.Contains(term))
                .Select(c => new {
                    Label = c.CourseName,
                    Url = Url.Action("Details", "Courses", new { id = c.CourseID })
                }).ToList();

            return Json(results);
        }

    }
}
