using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(string companyId);
        Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync(string companyId);
        Task<CategoryDto?> GetCategoryByIdAsync(int id, string companyId);
        Task<int> CreateCategoryAsync(CategoryDto categoryDto);
        Task UpdateCategoryAsync(CategoryDto categoryDto);
        Task ToggleCategoryStatusAsync(int id, string companyId);
        Task DeleteCategoryAsync(int id, string companyId);
    }
}
