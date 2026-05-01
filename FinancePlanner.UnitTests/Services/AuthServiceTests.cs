using FinancePlanner.API.Services.Interfaces;
using FinancePlanner.Application.Services;
using FinancePlanner.Common.DTOs.Auth;
using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinancePlanner.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtTokenService> _jwtMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtTokenService>();

        _jwtMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns("fake-token");

        _service = new AuthService(
            _userRepoMock.Object,
            _jwtMock.Object,
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenEmailIsNew()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _userRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                u.Id = 1;
                return u;
            });

        var request = new RegisterRequest("test@test.com", "Test@1234!", "Test User");

        // Act
        var result = await _service.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("fake-token", result.AccessToken);
        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var request = new RegisterRequest("test@test.com", "Test@1234!", "Test User");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegisterAsync(request));

        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
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

        // Act
        var result = await _service.LoginAsync(
            new LoginRequest("test@test.com", "Test@1234!"));

        // Assert
        Assert.Equal("fake-token", result.AccessToken);
        _jwtMock.Verify(j => j.GenerateAccessToken(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsWrong()
    {
        // Arrange
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

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(
                new LoginRequest("test@test.com", "WrongPassword!")));

        _jwtMock.Verify(j => j.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(
                new LoginRequest("nobody@test.com", "Test@1234!")));
    }
}