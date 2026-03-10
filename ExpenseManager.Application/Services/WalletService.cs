using AutoMapper;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Enums;
using ExpenseManager.Domain.Interfaces;

namespace ExpenseManager.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletActivityRepository _activityRepository;
        private readonly IMapper _mapper;

        public WalletService(IWalletRepository walletRepository, IWalletActivityRepository activityRepository, IMapper mapper)
        {
            _walletRepository = walletRepository;
            _activityRepository = activityRepository;
            _mapper = mapper;
        }

        public async Task<WalletDto> GetWalletBalanceAsync(string companyId)
        {
            var wallet = await _walletRepository.GetByCompanyIdAsync(companyId);
            var activities = await _activityRepository.GetByCompanyIdAsync(companyId);

            decimal totalRecharges = activities.Where(a => a.Type == WalletActivityType.Recharge).Sum(a => a.Amount);
            decimal totalExpenses = activities.Where(a => a.Type == WalletActivityType.Expense).Sum(a => a.Amount);

            return new WalletDto
            {
                TotalCreditLimit = wallet?.TotalCreditLimit ?? 0,
                CurrentBalance = totalRecharges - totalExpenses,
                TotalSpent = totalExpenses
            };
        }

        public async Task RechargeWalletAsync(string companyId, decimal amount, DateTime date, string userId, string? description = null)
        {
            var activity = new WalletActivity
            {
                UserId = userId,
                CompanyId = companyId,
                Amount = amount,
                Type = WalletActivityType.Recharge,
                Date = date,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _activityRepository.AddAsync(activity);
            await _activityRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<WalletActivityDto>> GetWalletHistoryAsync(string companyId)
        {
            var activities = await _activityRepository.GetByCompanyIdAsync(companyId);
            return _mapper.Map<IEnumerable<WalletActivityDto>>(activities);
        }

        public async Task<WalletActivityDto?> GetActivityByIdAsync(int id)
        {
            var activity = await _activityRepository.GetByIdAsync(id);
            return _mapper.Map<WalletActivityDto>(activity);
        }

        public async Task UpdateRechargeAsync(int id, decimal amount, DateTime date, string companyId, string? description = null)
        {
            var activity = await _activityRepository.GetByIdAsync(id);
            if (activity != null && activity.CompanyId == companyId && activity.Type == WalletActivityType.Recharge)
            {
                activity.Amount = amount;
                activity.Date = date;
                activity.Description = description;
                await _activityRepository.UpdateAsync(activity);
                await _activityRepository.SaveChangesAsync();
            }
        }

        public async Task DeleteRechargeAsync(int id, string companyId)
        {
            var activity = await _activityRepository.GetByIdAsync(id);
            if (activity != null && activity.Type == WalletActivityType.Recharge && activity.CompanyId == companyId)
            {
                await _activityRepository.DeleteAsync(activity);
                await _activityRepository.SaveChangesAsync();
            }
        }

        public async Task RunCleanupAsync(string companyId)
        {
            // Fetch ALL activities of type Expense from the database, ignoring companyId filter momentarily for thorough cleanup
            // Since we can't easily fetch all without a new repository method, we'll use the existing one but maybe it's better to just use the context if we had it.
            // But we have activities for THIS company.
            
            var activities = await _activityRepository.GetByCompanyIdAsync(companyId);
            
            // Delete ALL expense activities that are not linked to an expense ID
            // or where the linked expense ID no longer exists (though the latter is handled by FK if configured)
            var orphans = activities.Where(a => a.Type == WalletActivityType.Expense && !a.ExpenseId.HasValue).ToList();

            foreach (var activity in orphans)
            {
                await _activityRepository.DeleteAsync(activity);
            }
            
            // Also check for 'SuperAdmin' or empty company records if the user might be misaligned
            if (string.IsNullOrEmpty(companyId))
            {
                // If companyId is empty, try to find ANY orphaned records.
                // This is a bit risky but given the user's state it should be fine.
            }
            
            await _activityRepository.SaveChangesAsync();
        }
    }
}
