using FinancePlanner.API.Services;
using FinancePlanner.Application.Services;
using FinancePlanner.Common.DTOs.Budget;
using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinancePlanner.UnitTests.Services;

public class BudgetServiceTests
{
    private readonly Mock<IBudgetRepository> _budgetRepoMock;
    private readonly BudgetService _service;

    public BudgetServiceTests()
    {
        _budgetRepoMock = new Mock<IBudgetRepository>();

        _service = new BudgetService(
            _budgetRepoMock.Object,
            NullLogger<BudgetService>.Instance);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnMappedResponses_WhenBudgetsExist()
    {
        // Arrange
        var budgets = new List<Budget>
        {
            new() { Id = 1, UserId = 1, CategoryName = "Food", LimitAmount = 300, Currency = "EUR", Month = 5, Year = 2024 },
            new() { Id = 2, UserId = 1, CategoryName = "Transport", LimitAmount = 150, Currency = "EUR", Month = 5, Year = 2024 }
        };

        _budgetRepoMock
            .Setup(r => r.GetByUserIdAsync(1, null, null))
            .ReturnsAsync(budgets);

        // Act
        var result = await _service.GetByUserIdAsync(1, null, null);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenNoBudgetsExist()
    {
        // Arrange
        _budgetRepoMock
            .Setup(r => r.GetByUserIdAsync(1, null, null))
            .ReturnsAsync(new List<Budget>());

        // Act
        var result = await _service.GetByUserIdAsync(1, null, null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldMapFieldsCorrectly()
    {
        // Arrange
        var budgets = new List<Budget>
        {
            new() { Id = 1, UserId = 1, CategoryName = "Food", LimitAmount = 300, Currency = "EUR", Month = 5, Year = 2024 }
        };

        _budgetRepoMock
            .Setup(r => r.GetByUserIdAsync(1, 5, 2024))
            .ReturnsAsync(budgets);

        // Act
        var result = await _service.GetByUserIdAsync(1, 5, 2024);
        var first = result.First();

        // Assert
        Assert.Equal(1, first.Id);
        Assert.Equal("Food", first.CategoryName);
        Assert.Equal(300, first.LimitAmount);
        Assert.Equal("EUR", first.Currency);
        Assert.Equal(5, first.Month);
        Assert.Equal(2024, first.Year);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldPassFiltersToRepository()
    {
        // Arrange
        _budgetRepoMock
            .Setup(r => r.GetByUserIdAsync(1, 5, 2024))
            .ReturnsAsync(new List<Budget>());

        // Act
        await _service.GetByUserIdAsync(1, 5, 2024);

        // Assert
        _budgetRepoMock.Verify(r => r.GetByUserIdAsync(1, 5, 2024), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnMappedResponse_WhenBudgetIsCreated()
    {
        // Arrange
        _budgetRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Budget>()))
            .ReturnsAsync((Budget b) => { b.Id = 1; return b; });

        var request = new CreateBudgetRequest("Food", 300, "EUR", 5, 2024);

        // Act
        var result = await _service.CreateAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Food", result.CategoryName);
        Assert.Equal(300, result.LimitAmount);
        Assert.Equal("EUR", result.Currency);
        Assert.Equal(5, result.Month);
        Assert.Equal(2024, result.Year);
    }

    [Fact]
    public async Task CreateAsync_ShouldUppercaseCurrency()
    {
        // Arrange
        _budgetRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Budget>()))
            .ReturnsAsync((Budget b) => { b.Id = 1; return b; });

        var request = new CreateBudgetRequest("Food", 300, "eur", 5, 2024);

        // Act
        var result = await _service.CreateAsync(1, request);

        // Assert
        Assert.Equal("EUR", result.Currency);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetCorrectUserId()
    {
        // Arrange
        Budget? capturedBudget = null;

        _budgetRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Budget>()))
            .ReturnsAsync((Budget b) =>
            {
                capturedBudget = b;
                b.Id = 1;
                return b;
            });

        var request = new CreateBudgetRequest("Food", 300, "EUR", 5, 2024);

        // Act
        await _service.CreateAsync(userId: 42, request);

        // Assert
        Assert.Equal(42, capturedBudget!.UserId);
    }

    [Fact]
    public async Task CreateAsync_ShouldCallRepositoryOnce()
    {
        // Arrange
        _budgetRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Budget>()))
            .ReturnsAsync((Budget b) => { b.Id = 1; return b; });

        var request = new CreateBudgetRequest("Food", 300, "EUR", 5, 2024);

        // Act
        await _service.CreateAsync(1, request);

        // Assert
        _budgetRepoMock.Verify(r => r.CreateAsync(It.IsAny<Budget>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryOnce()
    {
        // Arrange
        _budgetRepoMock
            .Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _budgetRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }
}