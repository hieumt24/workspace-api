using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Customer;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;

        public UserRepository(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> UpdateUserBasicInfoAsync(int userId, CustomerInfo customerInfo)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return false;
            }

            user.FirstName = customerInfo.FirstName;
            user.LastName = customerInfo.LastName;
            user.PhoneNumber = customerInfo.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }


        public async Task<IEnumerable<AppUser>> GetAllUsersAsync(string? searchTerm)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u => u.UserName.Contains(searchTerm) || u.Email.Contains(searchTerm));
            }

            return await query.ToListAsync();
        }

        public async Task<AppUser?> GetUserByIdAsync(int userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<(bool Succeeded, string[] Errors, int NewId)> CreateUserAsync(AppUser user, string password, List<string> roles)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray(), 0);
            }

            if (roles != null && roles.Any())
            {
                await _userManager.AddToRolesAsync(user, roles);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Basic");
            }

            return (true, Array.Empty<string>(), user.Id);
        }

        public async Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(AppUser user, List<string> newRoles)
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }

            if (newRoles != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRolesAsync(user, newRoles);
            }

            return (true, Array.Empty<string>());
        }

        public async Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return (false, new[] { "User not found" });

            var result = await _userManager.DeleteAsync(user);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
        }
    }
}