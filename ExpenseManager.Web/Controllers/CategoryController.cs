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
        private readonly IExpenseService _expenseService;

        public CategoryController(ICategoryService categoryService, IExpenseService expenseService)
        {
            _categoryService = categoryService;
            _expenseService = expenseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetExpenses(int categoryId, DateTime? from, DateTime? to)
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            var allExpenses = await _expenseService.GetCompanyExpensesAsync(companyId);
            var filtered = allExpenses.Where(e => e.CategoryId == categoryId);
            if (from.HasValue) filtered = filtered.Where(e => e.Date >= from.Value);
            if (to.HasValue) filtered = filtered.Where(e => e.Date <= to.Value);
            var result = filtered.OrderByDescending(e => e.Date).Select(e => new {
                e.Id, e.Title, e.Amount,
                Date = e.Date.ToString("dd MMM yyyy"),
                e.PaymentMethod, e.Description
            });
            return Json(new { expenses = result, total = filtered.Sum(e => e.Amount) });
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

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            await _categoryService.DeleteCategoryAsync(id, companyId);
            return RedirectToAction(nameof(Index));
        }
    }
}
