using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Interfaces;
using ExpenseManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync(string? companyId = null)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                return await _context.Users.ToListAsync();
            }
            return await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
        }

        public async Task AddAsync(ApplicationUser user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            _context.Users.Update(user);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(ApplicationUser user)
        {
            _context.Users.Remove(user);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
