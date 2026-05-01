using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Data;
using FinancePlanner.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePlanner.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _context;

    public BudgetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Budget?> GetByIdAsync(int id)
    {
        return await _context.Budgets
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Budget>> GetByUserIdAsync(int userId, int? month, int? year)
    {
        var query = _context.Budgets
            .Where(b => b.UserId == userId);

        if (month.HasValue)
        {
            query = query.Where(b => b.Month == month.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(b => b.Year == year.Value);
        }

        return await query
            .OrderBy(b => b.Year)
            .ThenBy(b => b.Month)
            .ThenBy(b => b.CategoryName)
            .ToListAsync();
    }

    public async Task<Budget> CreateAsync(Budget budget)
    {
        budget.CreatedAt = DateTime.UtcNow;
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task<Budget> UpdateAsync(Budget budget)
    {
        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task DeleteAsync(int id)
    {
        var budget = await _context.Budgets.FindAsync(id);
        if (budget is not null)
        {
            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
        }
    }
}