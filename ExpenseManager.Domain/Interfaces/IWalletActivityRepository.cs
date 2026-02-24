using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Domain.Interfaces
{
    public interface IWalletActivityRepository
    {
        Task<IEnumerable<WalletActivity>> GetByCompanyIdAsync(string companyId);
        Task<IEnumerable<WalletActivity>> GetRecentByCompanyIdAsync(string companyId, int count);
        Task AddAsync(WalletActivity activity);
        Task<WalletActivity?> GetByIdAsync(int id);
        Task UpdateAsync(WalletActivity activity);
        Task DeleteAsync(WalletActivity activity);
        Task SaveChangesAsync();
    }
}
