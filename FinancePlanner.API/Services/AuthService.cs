using FinancePlanner.API.Services.Interfaces;
using FinancePlanner.Application.Services.Interfaces;
using FinancePlanner.Common.DTOs.Auth;
using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Repositories.Interfaces;

namespace FinancePlanner.Application.Services
{

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
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
            var token = _jwtTokenService.GenerateAccessToken(created);

            _logger.LogInformation("User {UserId} registered successfully", created.Id);

            return new AuthResponse(token, created.Email, created.DisplayName);
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

            var token = _jwtTokenService.GenerateAccessToken(user);

            _logger.LogInformation("User {UserEmail} logged in successfully", user.Email);

            return new AuthResponse(token, user.Email, user.DisplayName);
        }
    }
}