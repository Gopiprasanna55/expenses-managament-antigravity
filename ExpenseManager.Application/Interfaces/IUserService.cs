using ExpenseManager.Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace ExpenseManager.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync(string? companyId = null);
        Task<IEnumerable<UserDto>> GetAllUsersForRoleAsync(string companyId, string callerRole);
        Task<UserDto?> GetUserByIdAsync(string userId, string companyId);
        Task<IdentityResult> CreateUserAsync(UserCreateDto userDto, string companyId);
        Task UpdateUserAsync(UserUpdateDto userDto, string companyId);
        Task DeleteUserAsync(string userId, string companyId);
        Task ToggleUserStatusAsync(string userId, string companyId);
        Task AssignRoleAsync(string userId, string roleName, string companyId);
    }
}
