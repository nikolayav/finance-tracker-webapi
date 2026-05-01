using FinancePlanner.API.Services.Interfaces;
using FinancePlanner.Common.DTOs.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinancePlanner.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountsController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            var accounts = await _accountService.GetByUserIdAsync(userId.Value);
            return Ok(accounts);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequest request)
        {
            var userId = GetUserId();
            if (userId is null)
                return Unauthorized();

            var account = await _accountService.CreateAsync(userId.Value, request);
            return CreatedAtAction(nameof(GetAll), new { id = account.Id }, account);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _accountService.DeleteAsync(id);
            return NoContent();
        }
    }
}