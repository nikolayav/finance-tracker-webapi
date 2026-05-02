using FinancePlanner.API.Services;
using FinancePlanner.Common.DTOs.Budget;
using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Repositories.Interfaces;

namespace FinancePlanner.API.Services
{

    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly ILogger<BudgetService> _logger;

        public BudgetService(
            IBudgetRepository budgetRepository,
            ILogger<BudgetService> logger)
        {
            _budgetRepository = budgetRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<BudgetResponse>> GetByUserIdAsync(
            int userId,
            int? month,
            int? year)
        {
            _logger.LogInformation(
                "Fetching budgets for user {UserId} with filters: month={Month}, year={Year}",
                userId, month, year);

            var budgets = await _budgetRepository.GetByUserIdAsync(userId, month, year);

            var result = budgets.Select(b => new BudgetResponse(
                b.Id,
                b.CategoryName,
                b.LimitAmount,
                b.Currency,
                b.Month,
                b.Year)).ToList();

            _logger.LogInformation(
                "Returned {Count} budgets for user {UserId}",
                result.Count, userId);

            return result;
        }

        public async Task<BudgetResponse> CreateAsync(int userId, CreateBudgetRequest request)
        {
            _logger.LogInformation(
                "Creating budget for user {UserId}, category={Category}, limit={Limit} {Currency}",
                userId, request.CategoryName, request.LimitAmount, request.Currency);

            var budget = new Budget
            {
                UserId = userId,
                CategoryName = request.CategoryName,
                LimitAmount = request.LimitAmount,
                Currency = request.Currency.ToUpperInvariant(),
                Month = request.Month,
                Year = request.Year
            };

            var created = await _budgetRepository.CreateAsync(budget);

            _logger.LogInformation(
                "Budget {BudgetId} created successfully for user {UserId}",
                created.Id, userId);

            return new BudgetResponse(
                created.Id,
                created.CategoryName,
                created.LimitAmount,
                created.Currency,
                created.Month,
                created.Year);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting budget {BudgetId}", id);
            await _budgetRepository.DeleteAsync(id);
            _logger.LogInformation("Budget {BudgetId} deleted successfully", id);
        }
    }
}