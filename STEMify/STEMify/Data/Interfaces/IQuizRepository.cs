using STEMify.Models;

namespace STEMify.Data.Interfaces
{
    public interface IQuizRepository : IRepository<Quiz> 
    {
            Task AddQuizAsync(Quiz quiz);
            Task<Quiz> GetQuizByIdAsync(int quizId);
            Task<IEnumerable<Quiz>> GetAllQuizzesAsync();
    }
    }
