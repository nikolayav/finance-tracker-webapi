using FinancePlanner.Common.DTOs.Account;

namespace FinancePlanner.API.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountResponse>> GetByUserIdAsync(int userId);
        Task<AccountResponse> CreateAsync(int userId, CreateAccountRequest request);
        Task DeleteAsync(int id);
    }
}
