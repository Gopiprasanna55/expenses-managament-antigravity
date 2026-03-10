using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseDto?> GetExpenseByIdAsync(int id, string companyId);
        Task<IEnumerable<ExpenseDto>> GetCompanyExpensesAsync(string companyId);
        Task<int> AddExpenseAsync(ExpenseDto expenseDto);
        Task UpdateExpenseAsync(ExpenseDto expenseDto);
        Task DeleteExpenseAsync(int id, string companyId);
        Task BulkDeleteExpensesAsync(IEnumerable<int> ids, string companyId);
        Task<DashboardDto> GetDashboardDataAsync(string companyId, int? month = null, int? year = null);
        Task<byte[]> ExportExpensesToCsvAsync(string companyId);
    }
}
