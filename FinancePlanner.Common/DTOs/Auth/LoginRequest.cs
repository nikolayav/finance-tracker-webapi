using System.ComponentModel.DataAnnotations;

namespace FinancePlanner.Common.DTOs.Auth
{
    public record LoginRequest(
    [Required]
    [EmailAddress]
    string Email,

    [Required]
    string Password);
}
