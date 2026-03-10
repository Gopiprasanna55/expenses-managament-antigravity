using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> GetWalletBalanceAsync(string companyId);
        Task RechargeWalletAsync(string companyId, decimal amount, DateTime date, string userId, string? description = null);
        Task<IEnumerable<WalletActivityDto>> GetWalletHistoryAsync(string companyId);
        Task<WalletActivityDto?> GetActivityByIdAsync(int id);
        Task UpdateRechargeAsync(int id, decimal amount, DateTime date, string companyId, string? description = null);
        Task DeleteRechargeAsync(int id, string companyId);
        Task RunCleanupAsync(string companyId);
    }
}
