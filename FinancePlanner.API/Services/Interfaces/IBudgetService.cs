using FinancePlanner.Common.DTOs.Budget;

namespace FinancePlanner.API.Services
{
    public interface IBudgetService
    {
        Task<IEnumerable<BudgetResponse>> GetByUserIdAsync(int userId, int? month, int? year);
        Task<BudgetResponse> CreateAsync(int userId, CreateBudgetRequest request);
        Task DeleteAsync(int id);
    }
}
