using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Domain.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByCompanyIdAsync(string companyId);
        Task AddAsync(Wallet wallet);
        Task UpdateAsync(Wallet wallet);
        Task SaveChangesAsync();
    }
}
