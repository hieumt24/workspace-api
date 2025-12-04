using System;
using System.Collections.Generic;
using WorkSpace.Application.DTOs.Customer;
using WorkSpace.Application.DTOs.Users;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<CustomerInfo> GetCurrentCustomerInfoAsync(); 


        Task<IEnumerable<UserDto>> GetAllUsersAsync(string? searchTerm);
        Task<UserDto> GetUserByIdAsync(int id);
        Task<Response<int>> CreateUserAsync(CreateUserRequest request);
        Task<Response<int>> UpdateUserAsync(int id, UpdateUserRequest request);
        Task<Response<int>> DeleteUserAsync(int id);
    }
}