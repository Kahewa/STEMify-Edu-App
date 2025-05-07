using STEMify.Data.Repositories;
using STEMify.Models.Quizzes;
using STEMify.Models;
using System;
using System.Threading.Tasks;
using static STEMify.Data.Repositories.FillInTheBlankQuestionRepository;

namespace STEMify.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        int Complete();
        Task CompleteAsync();

        // Add this line to include the Tests property
        //ITestRepository Tests { get; }
        ICourseRepository Courses { get; }
        ICategoryRepository Categories { get; }
        IDifficultyLevelRepository DifficultyLevels { get; }
        IQuizRepository Quizzes { get; }
        IQuizQuestionRepository QuizQuestions { get; }
        IQuestionTypeRepository QuestionTypes { get; }
        IUserAnswerRepository UserAnswers { get; }
        IQuizResultRepository QuizResults { get; }
        IMultipleChoiceQuestionRepository MultipleChoiceQuestions { get; }
        IFillInTheBlankQuestionRepository FillInTheBlankQuestions { get; }
        ITrueFalseQuestionRepository TrueFalseQuestions { get; }
        IUserCoursesRepository UserCourses { get; }
        IUserTaskRepository UserTasks { get; }
        IQuizAnswerRepository QuizAnswers { get; }
        IRepository<QuizAttempt> QuizAttempts { get; }

    }
}