using FinancePlanner.API.Services.Interfaces;
using FinancePlanner.Common.DTOs.Auth;
using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Repositories.Interfaces;

namespace FinancePlanner.API.Services
{

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IJwtTokenService jwtTokenService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("Registering new user with email {Email}", request.Email);

            var exists = await _userRepository.ExistsAsync(request.Email);
            if (exists)
            {
                _logger.LogWarning("Registration failed — email {Email} already exists", request.Email);
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DisplayName = request.DisplayName
            };

            var created = await _userRepository.CreateAsync(user);
            var (accessToken, refreshToken) = await GenerateTokenPairAsync(created);

            _logger.LogInformation("User {UserId} registered successfully", created.Id);

            return new AuthResponse(accessToken, refreshToken, created.Email, created.DisplayName);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("Login attempt for email {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email {Email} — invalid credentials", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var (accessToken, refreshToken) = await GenerateTokenPairAsync(user);

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return new AuthResponse(accessToken, refreshToken, user.Email, user.DisplayName);
        }

        public async Task<AuthResponse> RefreshAsync(RefreshRequest request)
        {
            _logger.LogInformation("Refresh token attempt");

            var existing = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

            if (existing is null || !existing.IsValid)
            {
                _logger.LogWarning("Refresh token invalid or expired");
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            // Delete the used token — one-time use
            await _refreshTokenRepository.DeleteAsync(existing);

            // Issue a new access + refresh token pair
            var (accessToken, newRefreshToken) = await GenerateTokenPairAsync(existing.User);

            _logger.LogInformation("Token refreshed for user {UserId}", existing.UserId);

            return new AuthResponse(accessToken, newRefreshToken, existing.User.Email, existing.User.DisplayName);
        }

        private async Task<(string accessToken, string refreshToken)> GenerateTokenPairAsync(User user)
        {
            var accessToken = _jwtTokenService.GenerateAccessToken(user);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = _jwtTokenService.GenerateRefreshToken(),
                Expiry = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenRepository.CreateAsync(refreshToken);

            return (accessToken, refreshToken.Token);
        }
    }
}