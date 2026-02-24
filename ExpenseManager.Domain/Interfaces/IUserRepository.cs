using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<IEnumerable<ApplicationUser>> GetAllAsync(string? companyId = null);
        Task AddAsync(ApplicationUser user);
        Task UpdateAsync(ApplicationUser user);
        Task DeleteAsync(ApplicationUser user);
        Task SaveChangesAsync();
    }
}
