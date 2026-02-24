using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Interfaces;
using ExpenseManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ApplicationDbContext _context;

        public ExpenseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Expense>> GetCompanyExpensesAsync(string companyId)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.CompanyId == companyId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetCompanyExpensesByMonthAsync(string companyId, int month, int year)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.CompanyId == companyId && e.Date.Month == month && e.Date.Year == year)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task AddAsync(Expense expense)
        {
            await _context.Expenses.AddAsync(expense);
        }

        public async Task UpdateAsync(Expense expense)
        {
            _context.Expenses.Update(expense);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Expense expense)
        {
            _context.Expenses.Remove(expense);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
