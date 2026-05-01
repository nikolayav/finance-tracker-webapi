using FinancePlanner.API.Services.Interfaces;
using FinancePlanner.Common.DTOs.Account;
using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Repositories.Interfaces;

namespace FinancePlanner.Application.Services
{

    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IAccountRepository accountRepository,
            ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<AccountResponse>> GetByUserIdAsync(int userId)
        {
            _logger.LogInformation("Fetching accounts for user {UserId}", userId);

            var accounts = await _accountRepository.GetByUserIdAsync(userId);

            var result = accounts.Select(a => new AccountResponse(
                a.Id,
                a.Name,
                a.Type,
                a.Balance,
                a.Currency)).ToList();

            _logger.LogInformation(
                "Returned {Count} accounts for user {UserId}",
                result.Count, userId);

            return result;
        }

        public async Task<AccountResponse> CreateAsync(int userId, CreateAccountRequest request)
        {
            _logger.LogInformation(
                "Creating account for user {UserId}, name={Name}, type={Type}, currency={Currency}",
                userId, request.Name, request.Type, request.Currency);

            var account = new Account
            {
                UserId = userId,
                Name = request.Name,
                Type = request.Type,
                Balance = 0,
                Currency = request.Currency.ToUpperInvariant()
            };

            var created = await _accountRepository.CreateAsync(account);

            _logger.LogInformation(
                "Account {AccountId} created successfully for user {UserId}",
                created.Id, userId);

            return new AccountResponse(
                created.Id,
                created.Name,
                created.Type,
                created.Balance,
                created.Currency);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting account {AccountId}", id);
            await _accountRepository.DeleteAsync(id);
            _logger.LogInformation("Account {AccountId} deleted successfully", id);
        }
    }
}