using Microsoft.EntityFrameworkCore;
using STEMify.Data.Interfaces;
using STEMify.Models;
using STEMify.Models.Quizzes;
using STEMify.Models.User;

namespace STEMify.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Courses = new CourseRepository(_context);
            Categories = new CategoryRepository(_context);
            DifficultyLevels = new DifficultyLevelRepository(_context);
            Quizzes = new QuizRepository(_context);
            QuizQuestions = new QuizQuestionRepository(_context);
            QuestionTypes = new QuestionTypeRepository(_context);
            UserAnswers = new UserAnswerRepository(_context);
            QuizResults = new QuizResultRepository(_context);
            FillInTheBlankQuestions = new FillInTheBlankQuestionRepository(_context);
            MultipleChoiceQuestions = new MultipleChoiceQuestionRepository(_context);
            TrueFalseQuestions = new TrueFalseQuestionRepository(_context);
            UserCourses = new UserCoursesRepository(_context);
            UserTasks = new UserTaskRepository(_context);
            QuizAnswers = new QuizAnswerRepository(_context);
            QuizAttempts = new Repository<QuizAttempt>(_context);
        }

        //public ITestRepository Tests { get; private set; }
        public ICourseRepository Courses { get; private set; }
        public ICategoryRepository Categories { get; private set; }
        public IDifficultyLevelRepository DifficultyLevels { get; private set; }
        public IQuizRepository Quizzes { get; }
        public IQuizQuestionRepository QuizQuestions { get; }
        public IQuestionTypeRepository QuestionTypes { get; }
        public IUserAnswerRepository UserAnswers { get; }
        public IQuizResultRepository QuizResults { get; }
        public IMultipleChoiceQuestionRepository MultipleChoiceQuestions { get; }
        public IFillInTheBlankQuestionRepository FillInTheBlankQuestions { get; }
        public ITrueFalseQuestionRepository TrueFalseQuestions { get; }
        public IUserCoursesRepository UserCourses { get; }
        public IUserTaskRepository UserTasks { get; }
        public IQuizAnswerRepository QuizAnswers { get; private set; }
        public IRepository<QuizAttempt> QuizAttempts { get; private set; }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}