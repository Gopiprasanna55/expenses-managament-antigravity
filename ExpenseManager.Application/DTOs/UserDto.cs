namespace ExpenseManager.Application.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
    }

    public class UserCreateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
        public string CompanyId { get; set; } = string.Empty;
    }

    public class UserUpdateDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Role { get; set; }
        public bool IsActive { get; set; }
        public string CompanyId { get; set; } = string.Empty;
    }

    public class DashboardDto
    {
        public decimal TotalWalletLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal LifetimeRecharge { get; set; }
        public decimal PreviousMonthRemainingBalance { get; set; }
        public decimal ThisMonthTotalAvailable { get; set; }
        public decimal CurrentMonthSpending { get; set; }
        public decimal CurrentMonthRecharge { get; set; }
        public decimal LastMonthRecharge { get; set; }
        public decimal RemainingBalanceAfterRecharge { get; set; }
        public int TotalTransactionsThisMonth { get; set; }
        public int TotalExpensesCount { get; set; }
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        public IEnumerable<ExpenseDto> RecentExpenses { get; set; } = new List<ExpenseDto>();
        public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new Dictionary<string, decimal>();
        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public List<MonthlySpendingDto> MonthlyTrends { get; set; } = new List<MonthlySpendingDto>();
    }

    public class MonthlySpendingDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
