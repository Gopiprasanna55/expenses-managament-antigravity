using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> GetWalletBalanceAsync(string companyId);
        Task RechargeWalletAsync(string companyId, decimal amount, DateTime date, string userId);
        Task<IEnumerable<WalletActivityDto>> GetWalletHistoryAsync(string companyId);
        Task<WalletActivityDto?> GetActivityByIdAsync(int id);
        Task UpdateRechargeAsync(int id, decimal amount, DateTime date, string companyId);
        Task DeleteRechargeAsync(int id, string companyId);
    }
}
