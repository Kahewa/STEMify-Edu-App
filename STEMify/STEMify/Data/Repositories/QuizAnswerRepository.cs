using STEMify.Data.Interfaces;
using STEMify.Models.Quizzes;

namespace STEMify.Data.Repositories
{
    public class QuizAnswerRepository : Repository<QuizAnswer>, IQuizAnswerRepository
    {
        public QuizAnswerRepository(AppDbContext context) : base(context)
        {
        }
        // Add methods specific to QuizAnswer repository
        public async Task AddAnswerAsync(QuizAnswer answer)
        {
            await AddAsync(answer);
        }
        // Implement other methods as needed
    }
}
