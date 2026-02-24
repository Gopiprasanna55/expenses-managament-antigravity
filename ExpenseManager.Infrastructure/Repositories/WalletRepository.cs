using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Interfaces;
using ExpenseManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly ApplicationDbContext _context;

        public WalletRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Wallet?> GetByCompanyIdAsync(string companyId)
        {
            return await _context.Wallets.FirstOrDefaultAsync(w => w.CompanyId == companyId);
        }

        public async Task AddAsync(Wallet wallet)
        {
            await _context.Wallets.AddAsync(wallet);
        }

        public async Task UpdateAsync(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
