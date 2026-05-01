using FinancePlanner.Common.Enums;

namespace FinancePlanner.Common.DTOs.Account
{
    public record AccountResponse(
        int Id,
        string Name,
        AccountType Type,
        decimal Balance,
        string Currency);
}
