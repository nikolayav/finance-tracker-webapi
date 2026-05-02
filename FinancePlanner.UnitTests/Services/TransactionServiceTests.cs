using FinancePlanner.Application.Services;
using FinancePlanner.Common.DTOs.Transaction;
using FinancePlanner.Common.Enums;
using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinancePlanner.UnitTests.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepoMock;
    private readonly Mock<IAccountRepository> _accountRepoMock;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _transactionRepoMock = new Mock<ITransactionRepository>();
        _accountRepoMock = new Mock<IAccountRepository>();

        _service = new TransactionService(
            _transactionRepoMock.Object,
            _accountRepoMock.Object,
            NullLogger<TransactionService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenAccountDoesNotBelongToUser()
    {
        // Arrange — account belongs to user 1
        _accountRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Account { Id = 1, UserId = 1, Balance = 0 });

        var request = new CreateTransactionRequest(
            AccountId: 1,
            Amount: 100,
            Currency: "EUR",
            Type: TransactionType.Expense,
            CategoryName: "Food",
            Description: null,
            OccurredOn: DateTime.UtcNow);

        // Act & Assert — user 2 tries to use user 1's account
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(userId: 2, request));

        _transactionRepoMock.Verify(r => r.CreateAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenAccountNotFound()
    {
        // Arrange
        _accountRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Account?)null);

        var request = new CreateTransactionRequest(
            AccountId: 99,
            Amount: 100,
            Currency: "EUR",
            Type: TransactionType.Expense,
            CategoryName: "Food",
            Description: null,
            OccurredOn: DateTime.UtcNow);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(userId: 1, request));
    }

    [Fact]
    public async Task CreateAsync_ShouldIncreaseBalance_WhenTransactionIsIncome()
    {
        // Arrange
        var account = new Account { Id = 1, UserId = 1, Balance = 100, Currency = "EUR" };

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(account);

        _transactionRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) => { t.Id = 1; return t; });

        _accountRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account a) => a);

        var request = new CreateTransactionRequest(
            AccountId: 1,
            Amount: 500,
            Currency: "EUR",
            Type: TransactionType.Income,
            CategoryName: "Salary",
            Description: null,
            OccurredOn: DateTime.UtcNow);

        // Act
        await _service.CreateAsync(userId: 1, request);

        // Assert — balance increased by 500
        Assert.Equal(600, account.Balance);
        _accountRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldDecreaseBalance_WhenTransactionIsExpense()
    {
        // Arrange
        var account = new Account { Id = 1, UserId = 1, Balance = 500, Currency = "EUR" };

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(account);

        _transactionRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) => { t.Id = 1; return t; });

        _accountRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account a) => a);

        var request = new CreateTransactionRequest(
            AccountId: 1,
            Amount: 200,
            Currency: "EUR",
            Type: TransactionType.Expense,
            CategoryName: "Food",
            Description: null,
            OccurredOn: DateTime.UtcNow);

        // Act
        await _service.CreateAsync(userId: 1, request);

        // Assert — balance decreased by 200
        Assert.Equal(300, account.Balance);
        _accountRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnMappedResponses()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new() { Id = 1, AccountId = 1, Amount = 100, Currency = "EUR", Type = TransactionType.Income, CategoryName = "Salary", OccurredOn = DateTime.UtcNow },
            new() { Id = 2, AccountId = 1, Amount = 50, Currency = "EUR", Type = TransactionType.Expense, CategoryName = "Food", OccurredOn = DateTime.UtcNow }
        };

        _transactionRepoMock
            .Setup(r => r.GetByUserIdAsync(1, null, null, null))
            .ReturnsAsync(transactions);

        // Act
        var result = await _service.GetByUserIdAsync(1, null, null, null);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepository()
    {
        // Arrange
        _transactionRepoMock
            .Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _transactionRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }
}