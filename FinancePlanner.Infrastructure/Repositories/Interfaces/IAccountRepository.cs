using FinancePlanner.Infrastructure.Entities;

namespace FinancePlanner.Infrastructure.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(int id);
        Task<IEnumerable<Account>> GetByUserIdAsync(int userId);
        Task<Account> CreateAsync(Account account);
        Task<Account> UpdateAsync(Account account);
        Task DeleteAsync(int id);
    }
}
