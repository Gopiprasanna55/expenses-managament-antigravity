using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Domain.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync(string companyId);
        Task<IEnumerable<Category>> GetActiveAsync(string companyId);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}
