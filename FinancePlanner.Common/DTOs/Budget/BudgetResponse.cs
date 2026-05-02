namespace FinancePlanner.Common.DTOs.Budget
{
    public record BudgetResponse(
    int Id,
    string CategoryName,
    decimal LimitAmount,
    string Currency,
    int Month,
    int Year);
}
