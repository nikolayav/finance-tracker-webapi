using FinancePlanner.Infrastructure.Data;
using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePlanner.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Account>> GetByUserIdAsync(int userId)
        {
            return await _context.Accounts
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Account> CreateAsync(Account account)
        {
            account.CreatedAt = DateTime.UtcNow;
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<Account> UpdateAsync(Account account)
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task DeleteAsync(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
            }
        }
    }
}
