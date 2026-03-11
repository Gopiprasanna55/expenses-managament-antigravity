using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ExpenseManager.Web.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly IExpenseService _expenseService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _environment;

        public ExpenseController(IExpenseService expenseService, ICategoryService categoryService, IWebHostEnvironment environment)
        {
            _expenseService = expenseService;
            _categoryService = categoryService;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            var expenses = await _expenseService.GetCompanyExpensesAsync(companyId);

            var categories = await _categoryService.GetActiveCategoriesAsync(companyId);
            ViewBag.CategoriesList = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });

            return View(expenses);
        }

        public async Task<IActionResult> Create()
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            var categories = await _categoryService.GetActiveCategoriesAsync(companyId);
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(new ExpenseDto { Date = DateTime.Today });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExpenseDto model)
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            
            // If creating a new category, the CategoryId will be 0 (from the 'Other' option)
            if (model.CategoryId == 0)
            {
                if (string.IsNullOrEmpty(model.NewCategoryName))
                {
                    ModelState.AddModelError("NewCategoryName", "Please enter a name for the new category.");
                }
                else
                {
                    // Remove CategoryId from ModelState to avoid validation errors
                    ModelState.Remove("CategoryId");
                }
            }
            else if (model.CategoryId == null)
            {
                ModelState.AddModelError("CategoryId", "Please select a category.");
            }

            if (ModelState.IsValid)
            {
                if (model.ReceiptFile != null)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "receipts");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ReceiptFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ReceiptFile.CopyToAsync(fileStream);
                    }
                    model.ReceiptPath = "/uploads/receipts/" + uniqueFileName;
                }

                try
                {
                    model.UserId = (User.FindFirstValue("LocalUserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier));
                    model.CompanyId = companyId;
                    await _expenseService.AddExpenseAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Internal Error: " + ex.InnerException?.Message + " | " + ex.Message);
                }
            }
            var categories = await _categoryService.GetActiveCategoriesAsync(companyId);
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            var expense = await _expenseService.GetExpenseByIdAsync(id, companyId);
            
            if (expense == null) return NotFound();

            var categories = await _categoryService.GetActiveCategoriesAsync(companyId);
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(expense);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ExpenseDto model)
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            if (ModelState.IsValid)
            {
                if (model.ReceiptFile != null)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "receipts");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ReceiptFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ReceiptFile.CopyToAsync(fileStream);
                    }
                    model.ReceiptPath = "/uploads/receipts/" + uniqueFileName;
                }

                model.UserId = (User.FindFirstValue("LocalUserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier));
                model.CompanyId = companyId;
                await _expenseService.UpdateExpenseAsync(model);
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryService.GetActiveCategoriesAsync(companyId);
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            await _expenseService.DeleteExpenseAsync(id, companyId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("No IDs provided.");
            
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            await _expenseService.BulkDeleteExpensesAsync(ids, companyId);
            return Json(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportData()
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            var csvBytes = await _expenseService.ExportExpensesToCsvAsync(companyId);
            var fileName = $"ExpenseManager_{DateTime.Now:yyyyMMdd}.csv";
            return File(csvBytes, "text/csv", fileName);
        }
    }
}
