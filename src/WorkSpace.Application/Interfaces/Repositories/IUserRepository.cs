using WorkSpace.Application.DTOs.Customer;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
       
        Task<bool> UpdateUserBasicInfoAsync(int userId, CustomerInfo customerInfo);

        Task<IEnumerable<AppUser>> GetAllUsersAsync(string? searchTerm);
        Task<AppUser?> GetUserByIdAsync(int userId);

        Task<(bool Succeeded, string[] Errors, int NewId)> CreateUserAsync(AppUser user, string password, List<string> roles);
        Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(AppUser user, List<string> newRoles);
        Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(int userId);
    }
}