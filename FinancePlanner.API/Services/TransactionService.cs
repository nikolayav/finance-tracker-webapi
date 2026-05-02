using FinancePlanner.API.Services.Interfaces;
using FinancePlanner.Common.DTOs.Transaction;
using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Repositories.Interfaces;

namespace FinancePlanner.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<TransactionResponse>> GetByUserIdAsync(
        int userId,
        DateTime? from,
        DateTime? to,
        string? category)
    {
        _logger.LogInformation(
            "Fetching transactions for user {UserId} with filters: from={From}, to={To}, category={Category}",
            userId, from, to, category);

        var transactions = await _transactionRepository
            .GetByUserIdAsync(userId, from, to, category);

        var result = transactions.Select(t => new TransactionResponse(
            t.Id,
            t.AccountId,
            t.Amount,
            t.Currency,
            t.Type,
            t.CategoryName,
            t.Description,
            t.OccurredOn,
            t.CreatedAt)).ToList();

        _logger.LogInformation(
            "Returned {Count} transactions for user {UserId}",
            result.Count, userId);

        return result;
    }

    public async Task<TransactionResponse> CreateAsync(
        int userId,
        CreateTransactionRequest request)
    {
        _logger.LogInformation(
            "Creating transaction for user {UserId} on account {AccountId}, amount={Amount} {Currency}",
            userId, request.AccountId, request.Amount, request.Currency);

        var account = await _accountRepository.GetByIdAsync(request.AccountId);
        if (account is null || account.UserId != userId)
        {
            _logger.LogWarning(
                "Account {AccountId} not found or does not belong to user {UserId}",
                request.AccountId, userId);
            throw new InvalidOperationException("Account not found.");
        }

        var transaction = new Transaction
        {
            AccountId = request.AccountId,
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            Type = request.Type,
            CategoryName = request.CategoryName,
            Description = request.Description,
            OccurredOn = request.OccurredOn
        };

        var created = await _transactionRepository.CreateAsync(transaction);

        if (request.Type == Common.Enums.TransactionType.Income)
            account.Balance += request.Amount;
        else
            account.Balance -= request.Amount;

        await _accountRepository.UpdateAsync(account);

        _logger.LogInformation(
            "Transaction {TransactionId} created successfully for user {UserId}, new balance={Balance}",
            created.Id, userId, account.Balance);

        return new TransactionResponse(
            created.Id,
            created.AccountId,
            created.Amount,
            created.Currency,
            created.Type,
            created.CategoryName,
            created.Description,
            created.OccurredOn,
            created.CreatedAt);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting transaction {TransactionId}", id);
        await _transactionRepository.DeleteAsync(id);
        _logger.LogInformation("Transaction {TransactionId} deleted successfully", id);
    }
}