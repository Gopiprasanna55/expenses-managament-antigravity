using ExpenseManager.Application.Interfaces;
using ExpenseManager.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseManager.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,HR")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            var callerRole = GetCallerRole();
            var users = await _userService.GetAllUsersForRoleAsync(companyId, callerRole);
            ViewData["CallerRole"] = callerRole;
            return View(users);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult AddEmployee()
        {
            ViewData["CallerRole"] = GetCallerRole();
            return View(new UserCreateDto());
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AddEmployee(UserCreateDto model)
        {
            var callerRole = GetCallerRole();
            var companyId = callerRole == "SuperAdmin" ? null : (User.FindFirstValue("CompanyId") ?? string.Empty);
            ViewData["CallerRole"] = callerRole;

            // Enforce: Admin can only create HR, SuperAdmin can create SuperAdmin, Admin or HR
            if (callerRole == "Admin" && model.Role != "HR")
            {
                ModelState.AddModelError("Role", "You can only add users with the HR role.");
                return View(model);
            }

            if (callerRole != "SuperAdmin" && model.Role == "SuperAdmin")
            {
                ModelState.AddModelError("Role", "Only SuperAdmin can assign the SuperAdmin role.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var result = await _userService.CreateUserAsync(model, companyId);
                if (result.Succeeded)
                {
                    TempData["Success"] = "User created successfully";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var callerRole = GetCallerRole();
            var companyId = callerRole == "SuperAdmin" ? null : (User.FindFirstValue("CompanyId") ?? string.Empty);
            var user = await _userService.GetUserByIdAsync(id, companyId);
            if (user == null) return NotFound();

            // Enforce: Admin cannot edit SuperAdmin or other Admins
            if (callerRole == "Admin" && (user.Role == "SuperAdmin" || user.Role == "Admin"))
            {
                return Forbid();
            }

            ViewData["CallerRole"] = callerRole;
            var model = new UserUpdateDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Edit(UserUpdateDto model)
        {
            var callerRole = GetCallerRole();
            var companyId = callerRole == "SuperAdmin" ? null : (User.FindFirstValue("CompanyId") ?? string.Empty);
            ViewData["CallerRole"] = callerRole;

            // Enforce: Admin can only assign HR role, SuperAdmin can assign any role
            if (callerRole == "Admin" && model.Role != "HR")
            {
                ModelState.AddModelError("Role", "You can only assign the HR role.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                await _userService.UpdateUserAsync(model, companyId);
                TempData["Success"] = "User updated successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var callerRole = GetCallerRole();
            var companyId = callerRole == "SuperAdmin" ? null : (User.FindFirstValue("CompanyId") ?? string.Empty);
 
            // Protect SuperAdmin from deletion
            var user = await _userService.GetUserByIdAsync(id, companyId);
            if (user == null) return NotFound();
 
            if (user.Role == "SuperAdmin" && callerRole != "SuperAdmin")
            {
                TempData["Error"] = "Cannot delete the SuperAdmin account.";
                return RedirectToAction(nameof(Index));
            }
 
            // Admin cannot delete other Admins
            if (callerRole == "Admin" && user.Role == "Admin")
            {
                TempData["Error"] = "You do not have permission to delete Admin accounts.";
                return RedirectToAction(nameof(Index));
            }
 
            await _userService.DeleteUserAsync(id, companyId);
            TempData["Success"] = "User deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var callerRole = GetCallerRole();
            var companyId = callerRole == "SuperAdmin" ? null : (User.FindFirstValue("CompanyId") ?? string.Empty);
            
            var user = await _userService.GetUserByIdAsync(id, companyId);
            if (user == null) return NotFound();
 
            // Protect SuperAdmin from deactivation
            if (user.Role == "SuperAdmin" && callerRole != "SuperAdmin")
            {
                TempData["Error"] = "Cannot deactivate the SuperAdmin account.";
                return RedirectToAction(nameof(Index));
            }
 
            // Admin cannot toggle other Admins
            if (callerRole == "Admin" && user.Role == "Admin")
            {
                TempData["Error"] = "You do not have permission to modify Admin accounts.";
                return RedirectToAction(nameof(Index));
            }
 
            await _userService.ToggleUserStatusAsync(id, companyId);
            TempData["Success"] = $"User status {(user.IsActive ? "deactivated" : "activated")} successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AssignRole(string id, string role)
        {
            var companyId = User.FindFirstValue("CompanyId") ?? string.Empty;
            // Only SuperAdmin can assign roles
            var user = await _userService.GetUserByIdAsync(id, companyId);
            if (user == null) return NotFound();

            // Only SuperAdmin can reach here due to [Authorize] attribute, 
            // but we keep a generic guard or remove the hard restriction.
            // Requirement: SuperAdmin can manage other SuperAdmins.
            // If we want to prevent changing the role of the LAST SuperAdmin, we could add logic, 
            // but for now, we follow the request.

            await _userService.AssignRoleAsync(id, role, companyId);
            TempData["Success"] = $"Role updated to {role} successfully.";
            return RedirectToAction(nameof(Index));
        }

        private string GetCallerRole()
        {
            if (User.IsInRole("SuperAdmin")) return "SuperAdmin";
            if (User.IsInRole("Admin")) return "Admin";
            if (User.IsInRole("HR")) return "HR";
            return "User";
        }
    }
}
