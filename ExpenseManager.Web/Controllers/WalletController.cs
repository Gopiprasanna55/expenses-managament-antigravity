using ExpenseManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExpenseManager.Domain.Enums;
using ExpenseManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Web.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public async Task<IActionResult> Index()
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            
            // Temporary auto-cleanup to fix orphaned records
            await _walletService.RunCleanupAsync(companyId);

            var walletInfo = await _walletService.GetWalletBalanceAsync(companyId);
            var history = await _walletService.GetWalletHistoryAsync(companyId);
            
            ViewBag.History = history;
            return View(walletInfo);
        }

        [HttpPost]
        public async Task<IActionResult> Recharge(decimal amount, DateTime date, string? description)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Amount must be greater than zero.";
                return RedirectToAction(nameof(Index));
            }

            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            var userId = (User.FindFirstValue("LocalUserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _walletService.RechargeWalletAsync(companyId, amount, date, userId!, description);
            TempData["Success"] = "Recharge added successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,HR")]
        public async Task<IActionResult> UpdateRecharge(int id, decimal amount, DateTime date, string? description)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Amount must be greater than zero.";
                return RedirectToAction(nameof(Index));
            }

            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            await _walletService.UpdateRechargeAsync(id, amount, date, companyId, description);
            TempData["Success"] = "Recharge updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,HR")]
        public async Task<IActionResult> DeleteRecharge(int id)
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            await _walletService.DeleteRechargeAsync(id, companyId);
            TempData["Success"] = "Recharge deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Cleanup()
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            await _walletService.RunCleanupAsync(companyId);
            TempData["Success"] = "Wallet balance cleanup completed successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
