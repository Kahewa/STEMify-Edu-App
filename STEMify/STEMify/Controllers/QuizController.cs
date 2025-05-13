using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using STEMify.Controllers;
using STEMify.Data.Interfaces;
using STEMify.Models;
using STEMify.Models.ViewModels;
using System.Threading.Tasks;

namespace STEMify.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QuizController : BaseController
    {
        public QuizController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        /// 
        /// Quizzes
        /// 
        /// 
        public IActionResult Index()
        {
            var Quizzes = UnitOfWork.Quizzes.GetAll().ToList();
            return View(Quizzes);
        }
        public ActionResult AddQuiz()
        {
            return View("AddQuiz");
        }

        [HttpPost("Quiz/AddQuiz")]
        public IActionResult AddQuiz([FromForm] Quiz quiz)

        {
            if(ModelState.IsValid)
            {
                quiz.CreatedDate = DateTime.UtcNow;
                UnitOfWork.Quizzes.Add(quiz);
                UnitOfWork.Complete();

                return RedirectToAction(nameof(Index));
            }

            return View(quiz);
        }
        [ValidateAntiForgeryToken]
        public ActionResult DeleteQuiz(int id)
        {
            var Quizzes = UnitOfWork.Quizzes.Get(id);
            if(Quizzes != null)
            {
                UnitOfWork.Quizzes.Remove(Quizzes);
                UnitOfWork.Complete();
            }
            return RedirectToAction(nameof(Index));
        }

        public ActionResult EditQuiz(int id)
        {
            var Quiz = UnitOfWork.Quizzes.Get(id);
            return View(Quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditQuiz(Quiz quiz)
        {
            quiz.CreatedDate = DateTime.Now;
            if(!ModelState.IsValid)
            {
                return View(quiz);
            }

            var existingQuiz = UnitOfWork.Quizzes.Get(quiz.Id);
            if(existingQuiz == null)
            {
                return NotFound();
            }

            existingQuiz.Title = quiz.Title;

            UnitOfWork.Complete();

            return RedirectToAction(nameof(Index));
        }

        /// 
        /// Questions
        /// 
        public ActionResult Questions()
        {
            var questions = UnitOfWork.QuizQuestions.GetAll();
            return View(questions);
        }
        public ActionResult AddQuestion()
        {
            var quizzes = UnitOfWork.Quizzes.GetAll();
            var QuestionType = UnitOfWork.QuestionTypes.GetAll();
            ViewBag.Quizes = new SelectList(quizzes, "Id", "Title");
            ViewBag.QuestionType = new SelectList(QuestionType, "Id", "Type");

            return View(new QuizQuestion()); // Make sure you're returning a model instance
        }
        
        [HttpPost("Quiz/AddQuestion")]
        public IActionResult AddQuestion(QuizQuestion question)
        {
            if(question.QuestionType == 3 || question.QuestionType == 4)
            {
                question.QuestionText = Request.Form["StatementText"];
                ModelState.Remove("QuestionText");
            }

            // Ensure QuizId is set before adding the question
            if(question.QuizId == 0) // Adjust this if 0 is not a valid QuizId
            {
                ModelState.AddModelError("QuizId", "QuizId is required.");
            }

            if(!ModelState.IsValid)
                return View(question);

            UnitOfWork.QuizQuestions.AddQuestionAsync(question);
            UnitOfWork.Complete(); // Get the QuizQuestion.Id

            var type = UnitOfWork.QuestionTypes.Get(question.QuestionType);
            if(type.Id == 2)
            {
                var mc = new MultipleChoiceQuestion
                {
                    QuizQuestionId = question.Id,
                    QuestionText = Request.Form["QuestionText"],
                    OptionA = Request.Form["OptionA"],
                    OptionB = Request.Form["OptionB"],
                    OptionC = Request.Form["OptionC"],
                    OptionD = Request.Form["OptionD"],
                    CorrectAnswer = Request.Form["CorrectAnswer"]
                };
                UnitOfWork.MultipleChoiceQuestions.Add(mc);
            }
            else if(type.Id == 3)
            {
                var fib = new FillInTheBlankQuestion
                {
                    QuizQuestionId = question.Id,
                    StatementText = Request.Form["StatementText"],
                    CorrectAnswer = Request.Form["CorrectAnswer"]
                };
                UnitOfWork.FillInTheBlankQuestions.Add(fib);
            }
            else if(type.Id == 4)
            {
                var tf = new TrueFalseQuestion
                {
                    QuizQuestionId = question.Id,
                    StatementText = question.QuestionText,
                    Answer = Request.Form["Answer"] == "true"
                };
                UnitOfWork.TrueFalseQuestions.Add(tf);
            }

            UnitOfWork.Complete();
            return RedirectToAction("Index");
        }
        public ActionResult AddQuestionType()
        {
            return View();
        }
        [HttpPost("Quiz/AddQuestionType")]
        public IActionResult AddQuestionType([FromForm] QuestionType questionType)
        {
            if(ModelState.IsValid)
            {
                UnitOfWork.QuestionTypes.Add(questionType);
                UnitOfWork.Complete();

                return RedirectToAction(nameof(Index));
            }

            return View(questionType);
        }

        [HttpPost("DeleteQuestion")]
        public ActionResult DeleteQuestion(int id)
        {
            var Question = UnitOfWork.QuizQuestions.Get(id);
            if(Question != null)
            {
                UnitOfWork.QuizQuestions.Remove(Question);
                UnitOfWork.Complete();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Quiz/EditQuestion")]
        public ActionResult EditQuestion(QuizQuestion question)
        {
            if(!ModelState.IsValid)
            {
                return View(question);
            }

            var existingQuiz = UnitOfWork.QuizQuestions.Get(question.Id);
            if(existingQuiz == null)
            {
                return NotFound();
            }

            existingQuiz.QuestionText = question.QuestionText;

            UnitOfWork.Complete();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult LoadQuestionTypeForm(int typeId)
        {
            string partialName = typeId switch
            {
                2 => "_MultipleChoice",
                3 => "_FillInTheBlank",
                4 => "_TrueFalse"
            };

            if(partialName == null)
                return BadRequest("Invalid Question Type");

            return PartialView($"_QuestionTypes/{partialName}");
        }

        /// 
        /// Quiz Questions
        /// 
        public ActionResult QuizQuestionDetails(int id)
        {
            var qu = UnitOfWork.QuizQuestions.Get(id);
            var questionType = UnitOfWork.QuestionTypes.Get(qu.QuestionType);

            if(questionType.Id == 2) // Multiple Choice
            {
                // Get the multiple choice question by QuizQuestionId
                var mc = UnitOfWork.MultipleChoiceQuestions
                                   .Find(q => q.QuizQuestionId == qu.Id)
                                   .FirstOrDefault();

                if(mc != null)
                    return RedirectToAction("MultipleChoice", new { id = mc.Id });
                else
                    return NotFound("Multiple choice details not found.");
            }
            else if(questionType.Id == 3) // Fill in the blank
            {
                var fib = UnitOfWork.FillInTheBlankQuestions
                                    .Find(q => q.QuizQuestionId == qu.Id)
                                    .FirstOrDefault();

                if(fib != null)
                    return RedirectToAction("FillInTheBlank", new { id = fib.Id });
                else
                    return NotFound("Fill in the blank details not found.");
            }
            else if(questionType.Id == 4) // True/False
            {
                var tf = UnitOfWork.TrueFalseQuestions
                                   .Find(q => q.QuizQuestionId == qu.Id)
                                   .FirstOrDefault();

                if(tf != null)
                    return RedirectToAction("TrueFalse", new { id = tf.Id });
                else
                    return NotFound("True/False details not found.");
            }

            ViewBag.QuestionType = questionType;
            return View(qu); // fallback if type doesn't match
        }
        public ActionResult MultipleChoice(int id)
        {
            var question = UnitOfWork.MultipleChoiceQuestions.Get(id);
            return View(question);
        }

        public ActionResult FillInTheBlank(int id)
        {
            var question = UnitOfWork.FillInTheBlankQuestions.Get(id);
            return View(question);
        }

        public ActionResult TrueFalse(int id)
        {
            var question = UnitOfWork.TrueFalseQuestions.Get(id);
            return View(question);
        }

        public ActionResult EditQuestion(int id)
        {
            var qu = UnitOfWork.QuizQuestions.Get(id);
            if(qu == null)
                return NotFound("Quiz question not found.");

            var questionType = UnitOfWork.QuestionTypes.Get(qu.QuestionType);

            if(questionType.Id == 2) // Multiple Choice
            {
                var mc = UnitOfWork.MultipleChoiceQuestions
                                   .Find(q => q.QuizQuestionId == qu.Id)
                                   .FirstOrDefault();

                if(mc != null)
                    return RedirectToAction("EditMultipleChoice", new { id = mc.Id });
                else
                    return NotFound("Multiple choice details not found.");
            }
            else if(questionType.Id == 3) // Fill in the Blank
            {
                var fib = UnitOfWork.FillInTheBlankQuestions
                                    .Find(q => q.QuizQuestionId == qu.Id)
                                    .FirstOrDefault();

                if(fib != null)
                    return RedirectToAction("EditFillInTheBlank", new { id = fib.Id });
                else
                    return NotFound("Fill in the blank details not found.");
            }
            else if(questionType.Id == 4) // True/False
            {
                var tf = UnitOfWork.TrueFalseQuestions
                                   .Find(q => q.QuizQuestionId == qu.Id)
                                   .FirstOrDefault();

                if(tf != null)
                    return RedirectToAction("EditTrueFalse", new { id = tf.Id });
                else
                    return NotFound("True/False details not found.");
            }

            return View("EditQuizQuestion", qu); // fallback
        }
        public ActionResult EditMultipleChoice(int id)
        {
            var mc = UnitOfWork.MultipleChoiceQuestions.Get(id);
            if(mc == null)
                return NotFound();

            return View(mc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMultipleChoice(MultipleChoiceQuestion model)
        {
            if(!ModelState.IsValid)
                return View(model);
            var quizQuestion = UnitOfWork.QuizQuestions.Get(model.QuizQuestionId);
            var existing = UnitOfWork.MultipleChoiceQuestions.Get(model.Id);
            if(existing == null)
                return NotFound();

            existing.QuestionText = model.QuestionText;
            quizQuestion.QuestionText = model.QuestionText;
            existing.OptionA = model.OptionA;
            existing.OptionB = model.OptionB;
            existing.OptionC = model.OptionC;
            existing.OptionD = model.OptionD;
            existing.CorrectAnswer = model.CorrectAnswer;

            UnitOfWork.MultipleChoiceQuestions.Update(existing);
            UnitOfWork.Complete();

            return RedirectToAction("QuizQuestionDetails", new { id = existing.QuizQuestionId });
        }
        public ActionResult EditFillInTheBlank(int id)
        {
            var fib = UnitOfWork.FillInTheBlankQuestions.Get(id);
            if(fib == null)
                return NotFound();

            return View(fib);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFillInTheBlank(FillInTheBlankQuestion model)
        {
            if(!ModelState.IsValid)
                return View(model);

            var existing = UnitOfWork.FillInTheBlankQuestions.Get(model.Id);
            if(existing == null)
                return NotFound();

            existing.StatementText = model.StatementText;
            existing.CorrectAnswer = model.CorrectAnswer;

            UnitOfWork.Complete();

            return RedirectToAction("QuizQuestionDetails", new { id = existing.QuizQuestionId });
        }
        public ActionResult EditTrueFalse(int id)
        {
            var tf = UnitOfWork.TrueFalseQuestions.Get(id);
            if(tf == null)
                return NotFound();

            return View(tf);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTrueFalse(TrueFalseQuestion model)
        {
            if(!ModelState.IsValid)
                return View(model);

            var existing = UnitOfWork.TrueFalseQuestions.Get(model.Id);
            if(existing == null)
                return NotFound();

            existing.StatementText = model.StatementText;
            existing.Answer = model.Answer;

            UnitOfWork.Complete();

            return RedirectToAction("QuizQuestionDetails", new { id = existing.QuizQuestionId });
        }


        [HttpPost, ActionName("DeleteQuestion")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDeleteQuestionConfirmed(int id)
        {
            var question = UnitOfWork.QuizQuestions.Get(id);
            if(question == null)
                return NotFound("Quiz Question not found.");

            var questionType = UnitOfWork.QuestionTypes.Get(question.QuestionType);

            // Delete child record based on question type
            if(questionType.Id == 2) // Multiple Choice
            {
                var mc = UnitOfWork.MultipleChoiceQuestions
                                   .Find(q => q.QuizQuestionId == question.Id)
                                   .FirstOrDefault();
                if(mc != null)
                    UnitOfWork.MultipleChoiceQuestions.Remove(mc);
            }
            else if(questionType.Id == 3) // Fill in the Blank
            {
                var fib = UnitOfWork.FillInTheBlankQuestions
                                    .Find(q => q.QuizQuestionId == question.Id)
                                    .FirstOrDefault();
                if(fib != null)
                    UnitOfWork.FillInTheBlankQuestions.Remove(fib);
            }
            else if(questionType.Id == 4) // True/False
            {
                var tf = UnitOfWork.TrueFalseQuestions
                                   .Find(q => q.QuizQuestionId == question.Id)
                                   .FirstOrDefault();
                if(tf != null)
                    UnitOfWork.TrueFalseQuestions.Remove(tf);
            }

            // Finally, remove the QuizQuestion itself
            UnitOfWork.QuizQuestions.Remove(question);
            UnitOfWork.Complete();

            return RedirectToAction("Questions"); // Or wherever you want to redirect
        }
        [HttpGet]
        public ActionResult LinkQuizToCourse(int id)
        {
            var quiz = UnitOfWork.Quizzes.Get(id);
            if(quiz == null) return NotFound();

            var courses = UnitOfWork.Courses.GetAll().ToList();

            ViewBag.SelectList = new SelectList(courses, "CourseID", "CourseName");

            return View("LinkQuizToCourse", quiz); // pass the quiz object
        }

        [HttpPost]
        public IActionResult LinkQuizToCourse(int id, int CourseID)
        {
            var quiz = UnitOfWork.Quizzes.Get(id);
            if(quiz == null)
                return NotFound();

            quiz.CourseID = CourseID;
            UnitOfWork.Complete();

            return RedirectToAction("Index"); // Or wherever your list view is
        }


    }
}
