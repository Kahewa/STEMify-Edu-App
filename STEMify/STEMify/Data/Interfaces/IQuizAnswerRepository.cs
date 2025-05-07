using STEMify.Models.Quizzes;

namespace STEMify.Data.Interfaces
{
    public interface IQuizAnswerRepository : IRepository<QuizAnswer>
    {
        // Add methods specific to QuizAnswer repository
        Task AddAnswerAsync(QuizAnswer answer);
        //Task<IEnumerable<QuizAnswer>> GetAnswersByUserAndQuizAsync(int userId, int quizId);
    }
}
