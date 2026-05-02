using FinancePlanner.Common.DTOs.Transaction;

namespace FinancePlanner.API.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionResponse>> GetByUserIdAsync(int userId, DateTime? from, DateTime? to, string? category);
        Task<TransactionResponse> CreateAsync(int userId, CreateTransactionRequest request);
        Task DeleteAsync(int id);
    }
}
