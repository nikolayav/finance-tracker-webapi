using System.ComponentModel.DataAnnotations;
using FinancePlanner.Common.Enums;

namespace FinancePlanner.Common.DTOs.Transaction
{
    public record CreateTransactionRequest(
    [Required]
    [Range(1, int.MaxValue)]
    int AccountId,

    [Required]
    [Range(0.01, 1_000_000)]
    decimal Amount,

    [Required]
    [StringLength(3, MinimumLength = 3)]
    string Currency,

    [Required]
    TransactionType Type,

    [Required]
    [MaxLength(50)]
    string CategoryName,

    [MaxLength(500)]
    string? Description,

    [Required]
    DateTime OccurredOn);
}
