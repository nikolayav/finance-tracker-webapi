using FinancePlanner.API.Services;
using FinancePlanner.Common.DTOs.Budget;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancePlanner.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BudgetsController : BaseController
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<BudgetsController> _logger;

        public BudgetsController(
            IBudgetService budgetService,
            ILogger<BudgetsController> logger)
        {
            _budgetService = budgetService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? month,
            [FromQuery] int? year)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var budgets = await _budgetService
                .GetByUserIdAsync(userId.Value, month, year);

            return Ok(budgets);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBudgetRequest request)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            try
            {
                var budget = await _budgetService.CreateAsync(userId.Value, request);
                return CreatedAtAction(nameof(GetAll), new { id = budget.Id }, budget);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Failed to create budget: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _budgetService.DeleteAsync(id);
            return NoContent();
        }
    }
}