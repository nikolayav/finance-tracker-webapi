using FinancePlanner.API.Services.Interfaces;
using FinancePlanner.Common.DTOs.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancePlanner.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : BaseController
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            ITransactionService transactionService,
            ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? category)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var transactions = await _transactionService
                .GetByUserIdAsync(userId.Value, from, to, category);

            return Ok(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            try
            {
                var transaction = await _transactionService.CreateAsync(userId.Value, request);
                return CreatedAtAction(nameof(GetAll), new { id = transaction.Id }, transaction);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Failed to create transaction: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _transactionService.DeleteAsync(id);
            return NoContent();
        }
    }
}