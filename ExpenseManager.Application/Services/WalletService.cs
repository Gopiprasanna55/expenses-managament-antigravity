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

        public async Task RechargeWalletAsync(string companyId, decimal amount, DateTime date, string userId)
        {
            var activity = new WalletActivity
            {
                UserId = userId,
                CompanyId = companyId,
                Amount = amount,
                Type = WalletActivityType.Recharge,
                Date = date,
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

        public async Task UpdateRechargeAsync(int id, decimal amount, DateTime date, string companyId)
        {
            var activity = await _activityRepository.GetByIdAsync(id);
            if (activity != null && activity.Type == WalletActivityType.Recharge && activity.CompanyId == companyId)
            {
                activity.Amount = amount;
                activity.Date = date;
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
    }
}
