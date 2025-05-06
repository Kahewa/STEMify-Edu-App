using Microsoft.AspNetCore.Mvc;
using STEMify.Data;
using STEMify.Data.Interfaces;
using STEMify.Data.Repositories;
using STEMify.Models;
using STEMify.Models.Quizzes;

namespace STEMify.Controllers
{
    public class QuizSessionController : BaseController
    {
        public QuizSessionController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        public IActionResult StartQuiz(int quizId)
        {
            var quiz = UnitOfWork.Quizzes.Get(quizId);
            if(quiz == null)
            {
                return NotFound("Quiz not found.");
            }

            var questions = UnitOfWork.QuizQuestions.GetQuestionsByQuizIdAsync(quizId).Result.ToList();
            if(!questions.Any())
            {
                return NotFound("No questions found for this quiz.");
            }

            var quizAttempt = new QuizAttempt
            {
                QuizId = quizId,
                UserId = User.Identity.Name,
                StartTime = DateTime.Now
            };

            UnitOfWork.QuizAttempts.Add(quizAttempt);
            UnitOfWork.Complete();

            ViewBag.QuizAttemptId = quizAttempt.Id;
            return View(new QuizSessionViewModel
            {
                Quiz = quiz,
                Questions = questions
            });
        }
        [HttpPost]
        public IActionResult SubmitAnswer(UserAnswer userAnswer)
        {
            var question = UnitOfWork.QuizQuestions.Get(userAnswer.QuestionId);
            if(question == null)
            {
                return NotFound("Question not found.");
            }

            userAnswer.IsCorrect = question.QuestionType switch
            {
                1 => UnitOfWork.MultipleChoiceQuestions.Get(userAnswer.QuestionId).CorrectAnswer == userAnswer.SelectedAnswer,
                2 => UnitOfWork.FillInTheBlankQuestions.Get(userAnswer.QuestionId).CorrectAnswer == userAnswer.SelectedAnswer,
                3 => UnitOfWork.TrueFalseQuestions.Get(userAnswer.QuestionId).Answer.ToString() == userAnswer.SelectedAnswer,
                _ => false
            };

            UnitOfWork.UserAnswers.Add(userAnswer);
            UnitOfWork.Complete();

            return Ok(new { IsCorrect = userAnswer.IsCorrect });
        }

        [HttpPost]
        public IActionResult CompleteQuiz(int quizAttemptId)
        {
            var quizAttempt = UnitOfWork.QuizAttempts.Get(quizAttemptId);
            if(quizAttempt == null)
            {
                return NotFound("Quiz attempt not found.");
            }

            var userAnswers = UnitOfWork.UserAnswers.Find(x => x.QuizId == quizAttempt.QuizId && x.UserId == quizAttempt.UserId);
            quizAttempt.Score = userAnswers.Count(x => x.IsCorrect);
            quizAttempt.EndTime = DateTime.Now;

            UnitOfWork.QuizAttempts.Update(quizAttempt);
            UnitOfWork.Complete();

            return RedirectToAction("Summary", new { quizAttemptId });
        }

        public IActionResult Summary(int quizAttemptId)
        {
            var quizAttempt = UnitOfWork.QuizAttempts.Get(quizAttemptId);
            if(quizAttempt == null)
            {
                return NotFound("Quiz attempt not found.");
            }

            var userAnswers = UnitOfWork.UserAnswers.Find(x => x.QuizId == quizAttempt.QuizId && x.UserId == quizAttempt.UserId);
            var questions = UnitOfWork.QuizQuestions.Find(x => x.QuizId == quizAttempt.QuizId);

            var summary = new QuizSummaryViewModel
            {
                QuizAttempt = quizAttempt,
                UserAnswers = userAnswers,
                Questions = questions
            };

            return View(summary);
        }
    }
}
