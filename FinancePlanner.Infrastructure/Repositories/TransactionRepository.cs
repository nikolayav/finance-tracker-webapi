using FinancePlanner.Infrastructure.Data;
using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePlanner.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(
        int accountId,
        DateTime? from,
        DateTime? to,
        string? category)
    {
        var query = _context.Transactions
            .Where(t => t.AccountId == accountId);

        if (from.HasValue)
        {
            query = query.Where(t => t.OccurredOn >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.OccurredOn <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(t => t.CategoryName == category);
        }

        return await query
            .OrderByDescending(t => t.OccurredOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdAsync(
        int userId,
        DateTime? from,
        DateTime? to,
        string? category)
    {
        var query = _context.Transactions
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == userId);

        if (from.HasValue)
        {
            query = query.Where(t => t.OccurredOn >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.OccurredOn <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(t => t.CategoryName == category);
        }

        return await query
            .OrderByDescending(t => t.OccurredOn)
            .ToListAsync();
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        transaction.CreatedAt = DateTime.UtcNow;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task DeleteAsync(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }
}