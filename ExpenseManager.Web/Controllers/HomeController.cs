using ExpenseManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace ExpenseManager.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IExpenseService _expenseService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IExpenseService expenseService, ILogger<HomeController> logger)
        {
            _expenseService = expenseService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? month = null, int? year = null)
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            var dashboardData = await _expenseService.GetDashboardDataAsync(companyId, month, year);
            return View(dashboardData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
