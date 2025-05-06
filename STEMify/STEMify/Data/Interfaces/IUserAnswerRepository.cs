using STEMify.Models;

namespace STEMify.Data.Interfaces
{
    public interface IUserAnswerRepository : IRepository<UserAnswer>
    {
        // Add methods specific to UserAnswer repository
        Task AddAnswerAsync(UserAnswer answer);
        //Task<IEnumerable<UserAnswer>> GetAnswersByUserAndQuizAsync(int userId, int quizId);
    }
}
