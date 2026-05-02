using System.ComponentModel.DataAnnotations;

namespace FinancePlanner.Common.DTOs.Budget
{
    public record CreateBudgetRequest(
    [Required]
    [MaxLength(50)]
    string CategoryName,

    [Required]
    [Range(0.01, 1_000_000)]
    decimal LimitAmount,

    [Required]
    [StringLength(3, MinimumLength = 3)]
    string Currency,

    [Required]
    [Range(1, 12)]
    int Month,

    [Required]
    [Range(2000, 2100)]
    int Year);
}
