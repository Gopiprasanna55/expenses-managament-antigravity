using AutoMapper;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Enums;
using ExpenseManager.Domain.Interfaces;
using System.Text;

namespace ExpenseManager.Application.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IWalletActivityRepository _activityRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ExpenseService(
            IExpenseRepository expenseRepository,
            IWalletActivityRepository activityRepository,
            IWalletRepository walletRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper)
        {
            _expenseRepository = expenseRepository;
            _activityRepository = activityRepository;
            _walletRepository = walletRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ExpenseDto?> GetExpenseByIdAsync(int id, string companyId)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense != null && expense.CompanyId == companyId)
            {
                return _mapper.Map<ExpenseDto>(expense);
            }
            return null;
        }

        public async Task<IEnumerable<ExpenseDto>> GetCompanyExpensesAsync(string companyId)
        {
            var expenses = await _expenseRepository.GetCompanyExpensesAsync(companyId);
            return _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
        }

        public async Task<int> AddExpenseAsync(ExpenseDto expenseDto)
        {
            var expense = _mapper.Map<Expense>(expenseDto);
            expense.CreatedAt = DateTime.UtcNow;
            // CompanyId is required on the entity now
            if (string.IsNullOrEmpty(expense.CompanyId))
            {
                // In a real scenario, should be passed in DTO or set from context
                // For now, assume it's in DTO or throw if missing
            }

            await _expenseRepository.AddAsync(expense);
            
            // Add Wallet Activity
            var activity = new WalletActivity
            {
                UserId = expenseDto.UserId!,
                CompanyId = expense.CompanyId,
                Amount = expenseDto.Amount,
                Type = WalletActivityType.Expense,
                Date = expenseDto.Date,
                CreatedAt = DateTime.UtcNow
            };
            await _activityRepository.AddAsync(activity);

            await _expenseRepository.SaveChangesAsync();
            await _activityRepository.SaveChangesAsync();

            return expense.Id;
        }

        public async Task UpdateExpenseAsync(ExpenseDto expenseDto)
        {
            var expense = await _expenseRepository.GetByIdAsync(expenseDto.Id);
            // Verify it belongs to the same company
            if (expense != null && expense.CompanyId == expenseDto.CompanyId)
            {
                expense.Title = expenseDto.Title;
                expense.Description = expenseDto.Description;
                expense.Amount = expenseDto.Amount;
                expense.Date = expenseDto.Date;
                expense.CategoryId = expenseDto.CategoryId;
                expense.ReceiptPath = expenseDto.ReceiptPath;
                expense.PaymentMethod = expenseDto.PaymentMethod;

                await _expenseRepository.UpdateAsync(expense);
                await _expenseRepository.SaveChangesAsync();
            }
        }

        public async Task DeleteExpenseAsync(int id, string companyId)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense != null && expense.CompanyId == companyId)
            {
                await _expenseRepository.DeleteAsync(expense);
                await _expenseRepository.SaveChangesAsync();
            }
        }

        public async Task<DashboardDto> GetDashboardDataAsync(string companyId, int? month = null, int? year = null)
        {
            var wallet = await _walletRepository.GetByCompanyIdAsync(companyId);
            var activities = await _activityRepository.GetByCompanyIdAsync(companyId);
            var expensesList = await _expenseRepository.GetCompanyExpensesAsync(companyId);
            
            var now = DateTime.UtcNow;
            int selectedMonth = month ?? now.Month;
            int selectedYear = year ?? now.Year;
            
            var startDate = new DateTime(selectedYear, selectedMonth, 1);
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            // Lifetime Recharges
            decimal lifetimeRecharge = activities.Where(a => a.Type == WalletActivityType.Recharge).Sum(a => a.Amount);
            decimal lifetimeExpenses = activities.Where(a => a.Type == WalletActivityType.Expense).Sum(a => a.Amount);

            // Data for the SELECTED month
            var monthExpenses = expensesList.Where(e => e.Date.Month == selectedMonth && e.Date.Year == selectedYear).ToList();
            decimal currentMonthSpending = monthExpenses.Sum(e => e.Amount);
            decimal rechargesInSelectedMonth = activities.Where(a => a.Type == WalletActivityType.Recharge && a.Date.Month == selectedMonth && a.Date.Year == selectedYear).Sum(a => a.Amount);

            // Previous Month's Remaining Balance
            // = (All recharges before start of selected month) - (All expense before start of selected month)
            decimal rechargesBeforeSelectedMonth = activities.Where(a => a.Type == WalletActivityType.Recharge && a.Date < startDate).Sum(a => a.Amount);
            decimal expensesBeforeSelectedMonth = activities.Where(a => a.Type == WalletActivityType.Expense && a.Date < startDate).Sum(a => a.Amount);
            decimal previousMonthRemainingBalance = rechargesBeforeSelectedMonth - expensesBeforeSelectedMonth;

            // This Month Added Recharge (User example: current recharges + prev month balance)
            decimal thisMonthTotalAvailable = rechargesInSelectedMonth + previousMonthRemainingBalance;

            // Current Balance (Real-time actual balance)
            decimal actualCurrentBalance = lifetimeRecharge - lifetimeExpenses;

            // Requirement: Total Remaining Balance = Total Wallet Balance - Amount Spent in Current Month
            // User formula: LifetimeRecharge - currentMonthSpending
            decimal totalRemainingBalanceFormula = lifetimeRecharge - currentMonthSpending;

            var dashboard = new DashboardDto
            {
                TotalWalletLimit = wallet?.TotalCreditLimit ?? 0,
                LifetimeRecharge = lifetimeRecharge,
                CurrentBalance = actualCurrentBalance,
                PreviousMonthRemainingBalance = previousMonthRemainingBalance,
                ThisMonthTotalAvailable = thisMonthTotalAvailable,
                CurrentMonthSpending = currentMonthSpending,
                CurrentMonthRecharge = rechargesInSelectedMonth,
                RemainingBalanceAfterRecharge = totalRemainingBalanceFormula,
                TotalTransactionsThisMonth = monthExpenses.Count(),
                TotalExpensesCount = expensesList.Count(),
                SelectedMonth = selectedMonth,
                SelectedYear = selectedYear,
                RecentExpenses = _mapper.Map<IEnumerable<ExpenseDto>>(expensesList.Take(5)),
                CategoryBreakdown = expensesList.GroupBy(e => e.Category?.Name ?? "Unknown")
                                           .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                Categories = _mapper.Map<IEnumerable<CategoryDto>>(await _categoryRepository.GetAllAsync(companyId)),
                MonthlyTrends = expensesList.GroupBy(e => new { e.Date.Year, e.Date.Month })
                                       .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                                       .Select(g => new MonthlySpendingDto 
                                       { 
                                           Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"), 
                                           Amount = g.Sum(e => e.Amount) 
                                       }).ToList()
            };

            return dashboard;
        }

        public async Task<byte[]> ExportExpensesToCsvAsync(string companyId)
        {
            var expenses = await _expenseRepository.GetCompanyExpensesAsync(companyId);
            var sb = new StringBuilder();
            sb.AppendLine("Date,Title,Category,Amount,Description");

            foreach (var expense in expenses)
            {
                var title = expense.Title.Replace(",", " ");
                var category = (expense.Category?.Name ?? "N/A").Replace(",", " ");
                var description = (expense.Description ?? "").Replace(",", " ");
                sb.AppendLine($"{expense.Date:yyyy-MM-dd},{title},{category},{expense.Amount},{description}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
