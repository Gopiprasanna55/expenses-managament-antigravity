using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseManager.Web.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            var categories = await _categoryService.GetAllCategoriesAsync(companyId);
            return View(categories);
        }

        public IActionResult Create() => View(new CategoryDto { IsActive = true });

        [HttpPost]
        public async Task<IActionResult> Create(CategoryDto model)
        {
            if (ModelState.IsValid)
            {
                model.CompanyId = User.FindFirstValue("CompanyId") ?? string.Empty;
                await _categoryService.CreateCategoryAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            var category = await _categoryService.GetCategoryByIdAsync(id, companyId);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryDto model)
        {
            if (ModelState.IsValid)
            {
                model.CompanyId = User.FindFirstValue("CompanyId") ?? string.Empty;
                await _categoryService.UpdateCategoryAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            await _categoryService.ToggleCategoryStatusAsync(id, companyId);
            return RedirectToAction(nameof(Index));
        }
    }
}
