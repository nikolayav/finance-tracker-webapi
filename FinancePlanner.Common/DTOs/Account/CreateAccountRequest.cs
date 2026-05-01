using System.ComponentModel.DataAnnotations;
using FinancePlanner.Common.Enums;

namespace FinancePlanner.Common.DTOs.Account
{
    public record CreateAccountRequest(
    [Required]
    [MinLength(2)]
    [MaxLength(20)]
    string Name,

    [Required]
    AccountType Type,

    [Required]
    [StringLength(3, MinimumLength = 3)]
    string Currency);
}
