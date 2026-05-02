using FinancePlanner.API.Services;
using FinancePlanner.API.Services.Interfaces;
using FinancePlanner.Common.DTOs.Auth;
using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinancePlanner.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<IJwtTokenService> _jwtMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _jwtMock = new Mock<IJwtTokenService>();

        _jwtMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns("fake-access-token");

        _jwtMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("fake-refresh-token");

        _refreshTokenRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => { t.Id = 1; return t; });

        _service = new AuthService(
            _userRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _jwtMock.Object,
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnAuthResponse_WhenEmailIsNew()
    {
        _userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _userRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        var result = await _service.RegisterAsync(
            new RegisterRequest("test@test.com", "Test@1234!", "Test User"));

        Assert.NotNull(result);
        Assert.Equal("fake-access-token", result.AccessToken);
        Assert.Equal("fake-refresh-token", result.RefreshToken);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("Test User", result.DisplayName);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCallCreateAsync_Once_WhenEmailIsNew()
    {
        _userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _userRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        await _service.RegisterAsync(
            new RegisterRequest("test@test.com", "Test@1234!", "Test User"));

        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateRefreshToken_WhenEmailIsNew()
    {
        _userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _userRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        await _service.RegisterAsync(
            new RegisterRequest("test@test.com", "Test@1234!", "Test User"));

        _refreshTokenRepoMock.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        _userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegisterAsync(
                new RegisterRequest("test@test.com", "Test@1234!", "Test User")));
    }

    [Fact]
    public async Task RegisterAsync_ShouldNeverCallCreateAsync_WhenEmailAlreadyExists()
    {
        _userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        try { await _service.RegisterAsync(new RegisterRequest("test@test.com", "Test@1234!", "Test User")); }
        catch { }

        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsAreValid()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test@1234!");

        _userRepoMock
            .Setup(r => r.GetByEmailAsync("test@test.com"))
            .ReturnsAsync(new User
            {
                Id = 1,
                Email = "test@test.com",
                PasswordHash = hashedPassword,
                DisplayName = "Test User"
            });

        var result = await _service.LoginAsync(
            new LoginRequest("test@test.com", "Test@1234!"));

        Assert.NotNull(result);
        Assert.Equal("fake-access-token", result.AccessToken);
        Assert.Equal("fake-refresh-token", result.RefreshToken);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task LoginAsync_ShouldCreateRefreshToken_WhenCredentialsAreValid()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test@1234!");

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User
            {
                Id = 1,
                Email = "test@test.com",
                PasswordHash = hashedPassword,
                DisplayName = "Test User"
            });

        await _service.LoginAsync(new LoginRequest("test@test.com", "Test@1234!"));

        _refreshTokenRepoMock.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsWrong()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test@1234!");

        _userRepoMock
            .Setup(r => r.GetByEmailAsync("test@test.com"))
            .ReturnsAsync(new User
            {
                Id = 1,
                Email = "test@test.com",
                PasswordHash = hashedPassword,
                DisplayName = "Test User"
            });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(
                new LoginRequest("test@test.com", "WrongPassword!")));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(
                new LoginRequest("nobody@test.com", "Test@1234!")));
    }

    [Fact]
    public async Task LoginAsync_ShouldNeverCreateRefreshToken_WhenCredentialsAreInvalid()
    {
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        try { await _service.LoginAsync(new LoginRequest("nobody@test.com", "Test@1234!")); }
        catch { }

        _refreshTokenRepoMock.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnNewTokenPair_WhenRefreshTokenIsValid()
    {
        var existingToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "valid-refresh-token",
            Expiry = DateTime.UtcNow.AddDays(7),
            User = new User { Id = 1, Email = "test@test.com", DisplayName = "Test User" }
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync("valid-refresh-token"))
            .ReturnsAsync(existingToken);

        var result = await _service.RefreshAsync(
            new RefreshRequest("valid-refresh-token"));

        Assert.NotNull(result);
        Assert.Equal("fake-access-token", result.AccessToken);
        Assert.Equal("fake-refresh-token", result.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_ShouldDeleteOldToken_WhenRefreshTokenIsValid()
    {
        var existingToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "valid-refresh-token",
            Expiry = DateTime.UtcNow.AddDays(7),
            User = new User { Id = 1, Email = "test@test.com", DisplayName = "Test User" }
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync("valid-refresh-token"))
            .ReturnsAsync(existingToken);

        await _service.RefreshAsync(new RefreshRequest("valid-refresh-token"));

        _refreshTokenRepoMock.Verify(r => r.DeleteAsync(existingToken), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ShouldCreateNewRefreshToken_WhenRefreshTokenIsValid()
    {
        var existingToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "valid-refresh-token",
            Expiry = DateTime.UtcNow.AddDays(7),
            User = new User { Id = 1, Email = "test@test.com", DisplayName = "Test User" }
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync("valid-refresh-token"))
            .ReturnsAsync(existingToken);

        await _service.RefreshAsync(new RefreshRequest("valid-refresh-token"));

        _refreshTokenRepoMock.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrow_WhenRefreshTokenDoesNotExist()
    {
        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.RefreshAsync(new RefreshRequest("invalid-token")));
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrow_WhenRefreshTokenIsExpired()
    {
        var expiredToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "expired-token",
            Expiry = DateTime.UtcNow.AddDays(-1),
            User = new User { Id = 1, Email = "test@test.com", DisplayName = "Test User" }
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync("expired-token"))
            .ReturnsAsync(expiredToken);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.RefreshAsync(new RefreshRequest("expired-token")));
    }

    [Fact]
    public async Task RefreshAsync_ShouldNeverDeleteToken_WhenRefreshTokenIsExpired()
    {
        var expiredToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "expired-token",
            Expiry = DateTime.UtcNow.AddDays(-1),
            User = new User { Id = 1, Email = "test@test.com", DisplayName = "Test User" }
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync("expired-token"))
            .ReturnsAsync(expiredToken);

        try { await _service.RefreshAsync(new RefreshRequest("expired-token")); }
        catch { }

        _refreshTokenRepoMock.Verify(r => r.DeleteAsync(It.IsAny<RefreshToken>()), Times.Never);
    }
}