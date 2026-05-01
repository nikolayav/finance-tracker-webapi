namespace FinancePlanner.Common.DTOs.Auth
{
    public record AuthResponse(
        string AccessToken,
        string Email,
        string DisplayName);
}
