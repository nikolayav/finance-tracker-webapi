namespace FinancePlanner.Common.DTOs.Auth
{
    public record AuthResponse(
        string AccessToken,
        string RefreshToken,
        string Email,
        string DisplayName);
}
