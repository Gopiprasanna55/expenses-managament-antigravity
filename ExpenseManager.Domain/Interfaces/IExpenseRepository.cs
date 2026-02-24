using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Domain.Interfaces
{
    public interface IExpenseRepository
    {
        Task<Expense?> GetByIdAsync(int id);
        Task<IEnumerable<Expense>> GetCompanyExpensesAsync(string companyId);
        Task<IEnumerable<Expense>> GetCompanyExpensesByMonthAsync(string companyId, int month, int year);
        Task AddAsync(Expense expense);
        Task UpdateAsync(Expense expense);
        Task DeleteAsync(Expense expense);
        Task SaveChangesAsync();
    }
}
