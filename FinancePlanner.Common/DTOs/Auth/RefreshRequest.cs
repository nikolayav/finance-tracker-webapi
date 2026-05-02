using System.ComponentModel.DataAnnotations;

namespace FinancePlanner.Common.DTOs.Auth
{
    public record RefreshRequest(
        [Required]
        string RefreshToken);
}
