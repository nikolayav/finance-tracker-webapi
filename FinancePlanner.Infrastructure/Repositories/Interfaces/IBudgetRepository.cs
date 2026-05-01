using FinancePlanner.Infrastructure.Entities;

namespace FinancePlanner.Infrastructure.Repositories.Interfaces
{
    public interface IBudgetRepository
    {
        Task<Budget?> GetByIdAsync(int id);
        Task<IEnumerable<Budget>> GetByUserIdAsync(int userId, int? month, int? year);
        Task<Budget> CreateAsync(Budget budget);
        Task<Budget> UpdateAsync(Budget budget);
        Task DeleteAsync(int id);
    }
}
