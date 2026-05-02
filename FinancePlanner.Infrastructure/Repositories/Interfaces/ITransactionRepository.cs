using FinancePlanner.Infrastructure.Entities;

namespace FinancePlanner.Infrastructure.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId, DateTime? from, DateTime? to, string? category);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId, DateTime? from, DateTime? to, string? category);
        Task<Transaction> CreateAsync(Transaction transaction);
        Task DeleteAsync(int id);
    }
}
