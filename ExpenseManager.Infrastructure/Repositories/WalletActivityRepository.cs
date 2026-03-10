using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Interfaces;
using ExpenseManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Repositories
{
    public class WalletActivityRepository : IWalletActivityRepository
    {
        private readonly ApplicationDbContext _context;

        public WalletActivityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WalletActivity>> GetByCompanyIdAsync(string companyId)
        {
            return await _context.WalletActivities
                .Where(wa => wa.CompanyId == companyId)
                .OrderByDescending(wa => wa.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<WalletActivity>> GetRecentByCompanyIdAsync(string companyId, int count)
        {
            return await _context.WalletActivities
                .Where(wa => wa.CompanyId == companyId)
                .OrderByDescending(wa => wa.Date)
                .Take(count)
                .ToListAsync();
        }

        public async Task AddAsync(WalletActivity activity)
        {
            await _context.WalletActivities.AddAsync(activity);
        }

        public async Task<WalletActivity?> GetByIdAsync(int id)
        {
            return await _context.WalletActivities.FindAsync(id);
        }

        public Task UpdateAsync(WalletActivity activity)
        {
            _context.WalletActivities.Update(activity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(WalletActivity activity)
        {
            _context.WalletActivities.Remove(activity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
