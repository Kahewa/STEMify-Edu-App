using Microsoft.AspNetCore.Mvc;
using STEMify.Data.Interfaces;
using STEMify.Models.Quizzes;
using System.Collections.Generic;
using System;
using System.Linq;
using STEMify.Models;
using Newtonsoft.Json;

namespace STEMify.Controllers
{
    public class QuizSessionController : BaseController
    {
        private readonly IUnitOfWork UnitOfWork;

        public QuizSessionController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        [HttpGet]
        [Route("QuizSession/StartQuiz/{quizId}")]
        public IActionResult StartQuiz(int quizId)
        {
            var quiz = UnitOfWork.Quizzes.Get(quizId);
            if(quiz == null)
            {
                // Log error and return NotFound response
                Console.WriteLine($"Quiz with ID {quizId} not found.");
                return NotFound("Quiz not found.");
            }

            var questions = UnitOfWork.QuizQuestions.Find(q => q.QuizId == quizId).ToList();
            if(!questions.Any())
            {
                return NotFound("No questions found for this quiz.");
            }

            var quizAttempt = new QuizAttempt
            {
                QuizId = quizId,
                UserId = User.Identity.Name,
                StartTime = DateTime.UtcNow
            };

            UnitOfWork.QuizAttempts.Add(quizAttempt);
            UnitOfWork.Complete();

            var viewModel = new QuizSessionViewModel
            {
                QuizId = quizId,
                QuizTitle = quiz.Title,
                Questions = questions.Select(MapQuestionToViewModel).ToList(),
                AttemptId = quizAttempt.Id
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SubmitAnswers(QuizSubmissionViewModel submission)
        {
            
            var quizAttemptId = submission.AttemptId; // ✅ Correct
            var quizAttempt = UnitOfWork.QuizAttempts.Get(quizAttemptId);
            if(quizAttempt == null)
            {
                return NotFound("Quiz attempt not found.");
            }

            foreach(var answer in submission.Answers)
            {
                var question = UnitOfWork.QuizQuestions.Get(answer.QuestionId);
                if(question == null) continue;

                var isCorrect = ValidateAnswer(question, answer.SelectedAnswer);

                var userAnswer = new UserAnswer
                {
                    QuizId = submission.QuizId,
                    QuestionId = answer.QuestionId,
                    SelectedAnswer = answer.SelectedAnswer,
                    IsCorrect = isCorrect,
                    UserId = User.Identity.Name
                };

                UnitOfWork.UserAnswers.Add(userAnswer);
            }

            quizAttempt.Score = UnitOfWork.UserAnswers
                                         .Find(u => u.QuizId == submission.QuizId && u.UserId == User.Identity.Name)
                                         .Count(u => u.IsCorrect);

            quizAttempt.EndTime = DateTime.UtcNow;
            UnitOfWork.Complete();

            return RedirectToAction("Summary", new { quizAttemptId = quizAttemptId });
        }

        public IActionResult Summary(int quizAttemptId)
        {
            var quizAttempt = UnitOfWork.QuizAttempts.Get(quizAttemptId);
            if(quizAttempt == null)
            {
                return NotFound("Quiz attempt not found.");
            }

            var userAnswers = UnitOfWork.UserAnswers
                .Find(a => a.QuizId == quizAttempt.QuizId && a.UserId == quizAttempt.UserId)
                .ToList();
            var questions = UnitOfWork.QuizQuestions.Find(q => q.QuizId == quizAttempt.QuizId).ToList();

            var summary = new QuizSummaryViewModel
            {
                QuizAttempt = quizAttempt,
                UserAnswers = userAnswers,
                Questions = questions
            };

            return View(summary);
        }

        private QuestionViewModel MapQuestionToViewModel(QuizQuestion question)
        {
            if(question.QuestionType == 2) // Multiple Choice
            {
                var mcq = UnitOfWork.MultipleChoiceQuestions.Find(m => m.QuizQuestionId == question.Id).FirstOrDefault();
                return new QuestionViewModel
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType,
                    Options = new List<OptionViewModel>
                    {
                        new OptionViewModel { Key = "A", Value = mcq?.OptionA },
                        new OptionViewModel { Key = "B", Value = mcq?.OptionB },
                        new OptionViewModel { Key = "C", Value = mcq?.OptionC },
                        new OptionViewModel { Key = "D", Value = mcq?.OptionD }
                    }
                };
            }
            else if(question.QuestionType == 3) // Fill in the Blank
            {
                var fib = UnitOfWork.FillInTheBlankQuestions.Find(f => f.QuizQuestionId == question.Id).FirstOrDefault();
                return new QuestionViewModel
                {
                    QuestionId = question.Id,
                    QuestionText = fib?.StatementText,
                    QuestionType = question.QuestionType
                };
            }
            else if(question.QuestionType == 4) // True/False
            {
                var tf = UnitOfWork.TrueFalseQuestions.Find(t => t.QuizQuestionId == question.Id).FirstOrDefault();
                return new QuestionViewModel
                {
                    QuestionId = question.Id,
                    QuestionText = tf?.StatementText,
                    QuestionType = question.QuestionType,
                    Options = new List<OptionViewModel>
                    {
                        new OptionViewModel { Key = "True", Value = "True" },
                        new OptionViewModel { Key = "False", Value = "False" }
                    }
                };
            }

            return null;
        }

        private bool ValidateAnswer(QuizQuestion question, string selectedAnswer)
        {
            if(string.IsNullOrEmpty(selectedAnswer))
                return false;

            return question.QuestionType switch
            {
                2 => UnitOfWork.MultipleChoiceQuestions
                                 .Find(m => m.QuizQuestionId == question.Id)
                                 .FirstOrDefault()?.CorrectAnswer.Equals(selectedAnswer, StringComparison.OrdinalIgnoreCase) ?? false,

                3 => UnitOfWork.FillInTheBlankQuestions
                                 .Find(f => f.QuizQuestionId == question.Id)
                                 .FirstOrDefault()?.CorrectAnswer.Equals(selectedAnswer, StringComparison.OrdinalIgnoreCase) ?? false,

                4 => UnitOfWork.TrueFalseQuestions
                                 .Find(t => t.QuizQuestionId == question.Id)
                                 .FirstOrDefault()?.Answer.ToString().Equals(selectedAnswer, StringComparison.OrdinalIgnoreCase) ?? false,

                _ => false
            };
        }

        public IActionResult RenderPartial(string viewName, string model)
        {
            var question = JsonConvert.DeserializeObject<QuestionViewModel>(model);
            return PartialView(viewName, question);
        }

    }
}