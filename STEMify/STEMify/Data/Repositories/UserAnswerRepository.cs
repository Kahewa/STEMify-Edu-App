using STEMify.Data.Interfaces;
using STEMify.Models;
using Microsoft.EntityFrameworkCore;

namespace STEMify.Data.Repositories
{
    public class UserAnswerRepository : Repository<UserAnswer>, IUserAnswerRepository
    {
        public UserAnswerRepository(AppDbContext context) : base(context)
        {
        }

        public async Task AddAnswerAsync(UserAnswer answer)
        {
            _context.Set<UserAnswer>().Add(answer); // Use DbContext.Set<TEntity>() to access the DbSet
            await _context.SaveChangesAsync();
        }

        //public async Task<IEnumerable<UserAnswer>> GetAnswersByUserAndQuizAsync(int userId, int quizId)
        //{
        //    return await _context.Set<UserAnswer>() // Use DbContext.Set<TEntity>() to access the DbSet
        //        .Where(a => a.UserId == userId && a.QuizId == quizId)
        //        .ToListAsync();
        //}
    }
}
