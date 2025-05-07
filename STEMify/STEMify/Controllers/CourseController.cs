using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using STEMify.Data.Interfaces;
using STEMify.Models;

namespace STEMify.Controllers
{ 
    [Authorize(Roles = "Admin")]
    public class CourseController : BaseController
    {
        public CourseController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        /// 
        ///         //Everything that has to do with courses
        /// 
        public ActionResult Index()
        {
            var courses = UnitOfWork.Courses.GetAllWithIncludes(c => c.Category).ToList();
            return View(courses);
        }
        public IActionResult Edit(int id)
        {
            var course = UnitOfWork.Courses.Get(id);
            if(course == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(UnitOfWork.Categories.GetAll(), "CategoryID", "CategoryName", course.CategoryID);
            ViewBag.DifficultyLevels = new SelectList(UnitOfWork.DifficultyLevels.GetAll(), "DifficultyLevelID", "LevelName", course.DifficultyLevelID);

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Course course)
        {
            if(!ModelState.IsValid)
            {
                // Repopulate dropdowns if validation fails
                ViewBag.Categories = new SelectList(UnitOfWork.Categories.GetAll(), "CategoryID", "CategoryName", course.CategoryID);
                ViewBag.DifficultyLevels = new SelectList(UnitOfWork.DifficultyLevels.GetAll(), "DifficultyLevelID", "LevelName", course.DifficultyLevelID);

                return View(course);
            }

            var existingCourse = UnitOfWork.Courses.Get(course.CourseID);
            if(existingCourse == null)
            {
                return NotFound();
            }

            existingCourse.CourseName = course.CourseName;
            existingCourse.Description = course.Description;
            existingCourse.CategoryID = course.CategoryID;
            existingCourse.DifficultyLevelID = course.DifficultyLevelID;
            existingCourse.LastUpdated = DateTime.UtcNow;

            UnitOfWork.Complete();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(UnitOfWork.Categories.GetAll(), "CategoryID", "CategoryName");
            ViewBag.DifficultyLevels = new SelectList(UnitOfWork.DifficultyLevels.GetAll(), "DifficultyLevelID", "LevelName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Course course)
        {
            // Auto-generate CourseCode based on CategoryID and timestamp
            var category = UnitOfWork.Categories.Get(course.CategoryID);
            var categoryPrefix = category.CategoryName.Substring(0, 4).ToUpper();
            course.CourseCode = $"CRS-{categoryPrefix}";
            ModelState.Remove("CourseCode");
            ModelState.Remove("Category");
            if(ModelState.IsValid)
            {

             
                course.CreatedAt = DateTime.UtcNow;
                course.LastUpdated = DateTime.UtcNow;
                course.IsActive = true;

                UnitOfWork.Courses.Add(course);
                UnitOfWork.Complete();

                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if validation fails
            ViewBag.Categories = new SelectList(UnitOfWork.Categories.GetAll(), "CategoryID", "CategoryName", course.CategoryID);
            ViewBag.DifficultyLevels = new SelectList(UnitOfWork.DifficultyLevels.GetAll(), "DifficultyLevelID", "LevelName", course.DifficultyLevelID);

            return View(course);
        }

        [ValidateAntiForgeryToken]
        public ActionResult DeleteCourses(int id)
        {
            var Courses = UnitOfWork.Courses.Get(id);
            if(Courses != null)
            {
                UnitOfWork.Courses.Remove(Courses);
                UnitOfWork.Complete();
            }
            return RedirectToAction(nameof(Index));
        }

        /// 
        ///         //Everything that has to do with Difficulty levels
        /// 
        public IActionResult DifficultyLevels()
        {
            var levels = UnitOfWork.DifficultyLevels.GetAll().ToList();
            return View(levels);
        }
        public IActionResult CreateDifficultyLevels()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateDifficultyLevels(DifficultyLevel model)
        {
            if(ModelState.IsValid)
            {
                UnitOfWork.DifficultyLevels.Add(model);
                UnitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult DeleteDifficulty(int id)
        {
            var difficulty = UnitOfWork.DifficultyLevels.Get(id);
            if(difficulty != null)
            {
                UnitOfWork.DifficultyLevels.Remove(difficulty);
                UnitOfWork.Complete();
            }
            return RedirectToAction(nameof(DifficultyLevels));
        }

        /// 
        ///         //Everything that has to do with Categories
        /// 
        public IActionResult Categories()
        {
            var MODEL = UnitOfWork.Categories.GetAll().ToList();
            return View(MODEL);
        }

        public IActionResult CreateCategory()
        {
            ViewBag.Categories = new SelectList(UnitOfWork.Categories.GetAll(), "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        public ActionResult CreateCategory(Category model)
        {
            if(ModelState.IsValid)
            {
                UnitOfWork.Categories.Add(model);
                UnitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Categories));
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategory(int id)
        {
            var category = UnitOfWork.Categories.Get(id);
            if(category != null)
            {
                UnitOfWork.Categories.Remove(category);
                UnitOfWork.Complete();
            }
            return RedirectToAction(nameof(Categories));
        }

    }

}
