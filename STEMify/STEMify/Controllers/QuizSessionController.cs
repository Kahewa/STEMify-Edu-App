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
            // 🔁 Step 1: Check if an existing attempt exists for this user and quiz
            var existingAttempt = UnitOfWork.QuizAttempts
                .Find(a => a.QuizId == quizId && a.UserId == User.Identity.Name)
                .FirstOrDefault();

            if(existingAttempt != null)
            {
                // Optional: delete related quiz answers if you store them separately
                var relatedAnswers = UnitOfWork.QuizAnswers
                    .Find(a => a.Id == existingAttempt.Id)
                    .ToList();

                foreach(var answer in relatedAnswers)
                {
                    UnitOfWork.QuizAnswers.Remove(answer);
                }

                // 🗑️ Step 2: Delete the old attempt
                UnitOfWork.QuizAttempts.Remove(existingAttempt);
                UnitOfWork.Complete();
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
        public async Task<IActionResult> SubmitAnswers(QuizSubmissionViewModel submission)
        {
            var quizAttempt = await UnitOfWork.QuizAttempts.GetAsync(submission.AttemptId);
            if(quizAttempt == null)
                return NotFound("Quiz attempt not found.");

            // 🔁 Step 1: Delete old answers before starting the loop
            var existingAnswers = UnitOfWork.UserAnswers
                .GetAll()
                .Where(a => a.UserId == User.Identity.Name && a.QuizId == submission.QuizId)
                .ToList();

            if(existingAnswers.Any())
            {
                foreach(var answer in existingAnswers)
                {
                    UnitOfWork.UserAnswers.Remove(answer);
                }
                await UnitOfWork.CompleteAsync(); // 💾 Commit deletion before adding new ones
            }

            int COUNT = 0;

            foreach(var answer in submission.Answers)
            {
                var question = await UnitOfWork.QuizQuestions.GetAsync(answer.QuestionId);
                if(question == null) continue;

                var isCorrect = await ValidateAnswerAsync(question, answer.SelectedAnswer);

                var userAnswer = new UserAnswer
                {
                    QuizId = submission.QuizId,
                    QuestionId = answer.QuestionId,
                    SelectedAnswer = answer.SelectedAnswer,
                    IsCorrect = isCorrect,
                    UserId = User.Identity.Name
                };

                UnitOfWork.UserAnswers.Add(userAnswer);
                COUNT++;
            }

            // ✅ Save all new answers at once
            await UnitOfWork.CompleteAsync();

            var correctCount = (await UnitOfWork.UserAnswers
                .FindAsync(u => u.QuizId == submission.QuizId && u.UserId == User.Identity.Name))
                .Count(u => u.IsCorrect);

            quizAttempt.Score = correctCount;
            quizAttempt.EndTime = DateTime.UtcNow;

            var questions = await UnitOfWork.QuizQuestions
                .FindAsync(q => q.QuizId == submission.QuizId);

            await UnitOfWork.CompleteAsync();

            var userAnswers = await UnitOfWork.UserAnswers
                .FindAsync(u => u.QuizId == submission.QuizId && u.UserId == User.Identity.Name);

            var viewModel = new QuizSummaryViewModel
            {
                QuizAttempt = quizAttempt,
                Questions = questions,
                UserAnswers = userAnswers,
                TotalAnswersSubmitted = COUNT
            };

            return View("Summary", viewModel);
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

        /// <summary>
        ///  How the answer is validated depends on the question type.
        /// </summary>
        private async Task<bool> ValidateAnswerAsync(QuizQuestion question, string selectedAnswer)
        {
            if(string.IsNullOrEmpty(selectedAnswer))
                return false;

            switch(question.QuestionType)
            {
                case 2: // Multiple Choice
                    var mc = await UnitOfWork.MultipleChoiceQuestions
                                .FindAsync(m => m.QuizQuestionId == question.Id);
                    return mc.FirstOrDefault()?.CorrectAnswer
                               .Equals(selectedAnswer, StringComparison.OrdinalIgnoreCase) ?? false;

                case 3: // Fill in the Blank
                    var fib = await UnitOfWork.FillInTheBlankQuestions
                                .FindAsync(f => f.QuizQuestionId == question.Id);
                    return fib.FirstOrDefault()?.CorrectAnswer
                               .Equals(selectedAnswer, StringComparison.OrdinalIgnoreCase) ?? false;

                case 4: // True/False
                    var tf = await UnitOfWork.TrueFalseQuestions
                                .FindAsync(t => t.QuizQuestionId == question.Id);
                    return tf.FirstOrDefault()?.Answer.ToString()
                               .Equals(selectedAnswer, StringComparison.OrdinalIgnoreCase) ?? false;

                default:
                    return false;
            }
        }


        public IActionResult RenderPartial(string viewName, string model)
        {
            var question = JsonConvert.DeserializeObject<QuestionViewModel>(model);
            return PartialView(viewName, question);
        }

    }
}