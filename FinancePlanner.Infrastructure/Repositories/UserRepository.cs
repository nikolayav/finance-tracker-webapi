using FinancePlanner.Infrastructure.Entities;
using FinancePlanner.Infrastructure.Data;
using FinancePlanner.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePlanner.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant().Trim());
        }

        public async Task<User> CreateAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.Email = user.Email.ToLowerInvariant().Trim();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email.ToLowerInvariant().Trim());
        }
    }
}
