using AutoMapper;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ExpenseManager.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository, 
            IWalletRepository walletRepository,
            UserManager<ApplicationUser> userManager, 
            IMapper mapper)
        {
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(string? companyId = null)
        {
            var users = await _userRepository.GetAllAsync(companyId);
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var dto = _mapper.Map<UserDto>(user);
                var roles = await _userManager.GetRolesAsync(user);
                dto.Role = roles.FirstOrDefault() ?? "HR";
                dto.Email = user.Email ?? string.Empty;
                userDtos.Add(dto);
            }

            return userDtos;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersForRoleAsync(string companyId, string callerRole)
        {
            if (callerRole == "SuperAdmin")
            {
                // SuperAdmin sees everyone globally
                return await GetAllUsersAsync(null);
            }
            
            // Admin and HR see everyone in their company
            if (callerRole == "Admin" || callerRole == "HR")
            {
                return await GetAllUsersAsync(companyId);
            }

            // Other roles see nothing
            return Enumerable.Empty<UserDto>();
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId, string companyId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.CompanyId != companyId) return null;

            var dto = _mapper.Map<UserDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            dto.Role = roles.FirstOrDefault() ?? "HR";
            dto.Email = user.Email ?? string.Empty;
            return dto;
        }

        public async Task<IdentityResult> CreateUserAsync(UserCreateDto userDto, string companyId)
        {
            var user = new ApplicationUser
            {
                UserName = userDto.Email,
                Email = userDto.Email,
                FullName = userDto.FullName,
                IsActive = userDto.IsActive,
                EmailConfirmed = true,
                CompanyId = companyId
            };

            // Create user with a random password (users authenticate via Microsoft SSO)
            var randomPassword = Guid.NewGuid().ToString("N") + "Aa1!";
            var result = await _userManager.CreateAsync(user, randomPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, userDto.Role);
                
                // Add Initial Wallet
                var wallet = new Wallet
                {
                    UserId = user.Id,
                    CompanyId = companyId,
                    TotalCreditLimit = 50000, // Default limit
                    CreatedAt = DateTime.UtcNow
                };
                await _walletRepository.AddAsync(wallet);
                await _walletRepository.SaveChangesAsync();
            }
            return result;
        }

        public async Task UpdateUserAsync(UserUpdateDto userDto, string companyId)
        {
            var user = await _userRepository.GetByIdAsync(userDto.Id);
            if (user != null && user.CompanyId == companyId)
            {
                user.FullName = userDto.FullName;
                user.Email = userDto.Email;
                user.UserName = userDto.Email;
                user.IsActive = userDto.IsActive;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                if (!string.IsNullOrEmpty(userDto.Role))
                {
                    await AssignRoleAsync(user.Id, userDto.Role, companyId);
                }
            }
        }

        public async Task DeleteUserAsync(string userId, string companyId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && user.CompanyId == companyId)
            {
                // Protect SuperAdmin from deletion
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("SuperAdmin"))
                {
                    return; // Cannot delete SuperAdmin
                }

                await _userRepository.DeleteAsync(user);
                await _userRepository.SaveChangesAsync();
            }
        }

        public async Task ToggleUserStatusAsync(string userId, string companyId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && user.CompanyId == companyId)
            {
                // Protect SuperAdmin from deactivation
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("SuperAdmin"))
                {
                    return; // Cannot deactivate SuperAdmin
                }

                user.IsActive = !user.IsActive;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
            }
        }

        public async Task AssignRoleAsync(string userId, string roleName, string companyId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.CompanyId == companyId)
            {
                // Protect SuperAdmin role
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Contains("SuperAdmin"))
                {
                    return; // Cannot change SuperAdmin's role
                }

                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}
