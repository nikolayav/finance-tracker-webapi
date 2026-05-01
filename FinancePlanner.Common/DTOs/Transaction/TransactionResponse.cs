using FinancePlanner.Common.Enums;

namespace FinancePlanner.Common.DTOs.Transaction
{
    public record TransactionResponse(
        int Id,
        int AccountId,
        decimal Amount,
        string Currency,
        TransactionType Type,
        string CategoryName,
        string? Description,
        DateTime OccurredOn,
        DateTime CreatedAt);
}
