using FinancePlanner.Infrastructure.Entities;

namespace FinancePlanner.Infrastructure.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
        Task DeleteAsync(RefreshToken refreshToken);
        Task DeleteAllForUserAsync(int userId);
    }
}
