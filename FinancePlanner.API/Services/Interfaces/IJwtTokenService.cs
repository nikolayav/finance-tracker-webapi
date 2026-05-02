using FinancePlanner.Infrastructure.Entities;

namespace FinancePlanner.API.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
