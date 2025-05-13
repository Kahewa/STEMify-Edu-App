using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STEMify.Models;
using STEMify.Data.Interfaces;
using STEMify.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace STEMify.Controllers
{
    public class HomeController : BaseController
    {
        private readonly HttpClient _httpClient;

        public HomeController(IUnitOfWork unitOfWork, HttpClient httpClient) : base(unitOfWork)
        {
            _httpClient = httpClient;
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
                .Select(c => new
                {
                    Label = c.CourseName,
                    Url = Url.Action("Details", "Courses", new { id = c.CourseID })
                }).ToList();

            return Json(results);
        }

        [HttpGet]
        public async Task<IActionResult> SearchStem(string query)
        {
            if(string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query cannot be empty.");
            }
            string apiKey = UnitOfWork.DifficultyLevels.Get(99).LevelName;

            //string apiKey = "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"; // Replace with your OpenAI API key
            string apiUrl = "https://api.openai.com/v1/chat/completions";

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
            new { role = "system", content = "You are a helpful assistant specialized in all STEM related topics. Always give relevant and verified information. Include two helpful online resource links if possible." },
            new { role = "user", content = query }
        },
                max_tokens = 150,
                temperature = 0.7
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(apiUrl, content);

            if(!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch response from GPT-4.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse and extract assistant's message
            var responseJson = JObject.Parse(responseContent);
            string answer = responseJson["choices"]?[0]?["message"]?["content"]?.ToString();

            // Return the assistant's answer directly as plain text
            return Content(answer);
        }

    }
}