using AutoMapper;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Interfaces;

namespace ExpenseManager.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(string companyId)
        {
            var categories = await _categoryRepository.GetAllAsync(companyId);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync(string companyId)
        {
            var categories = await _categoryRepository.GetActiveAsync(companyId);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id, string companyId)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null && category.CompanyId == companyId)
            {
                return _mapper.Map<CategoryDto>(category);
            }
            return null;
        }

        public async Task<int> CreateCategoryAsync(CategoryDto categoryDto)
        {
            var category = _mapper.Map<Category>(categoryDto);
            category.CreatedAt = DateTime.UtcNow;
            // CompanyId is now explicitly set from the DTO
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();
            return category.Id;
        }

        public async Task UpdateCategoryAsync(CategoryDto categoryDto)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryDto.Id);
            if (category != null && category.CompanyId == categoryDto.CompanyId)
            {
                category.Name = categoryDto.Name;
                category.IsActive = categoryDto.IsActive;
                await _categoryRepository.UpdateAsync(category);
                await _categoryRepository.SaveChangesAsync();
            }
        }

        public async Task ToggleCategoryStatusAsync(int id, string companyId)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null && category.CompanyId == companyId)
            {
                category.IsActive = !category.IsActive;
                await _categoryRepository.UpdateAsync(category);
                await _categoryRepository.SaveChangesAsync();
            }
        }
    }
}
