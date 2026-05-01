using System.ComponentModel.DataAnnotations;

namespace FinancePlanner.Common.DTOs.Auth
{
    public record RegisterRequest(
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    string Email,

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    string Password,

    [Required]
    [MinLength(2)]
    [MaxLength(20)]
    string DisplayName);
}
