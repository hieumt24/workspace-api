using AutoMapper;
using Microsoft.AspNetCore.Http;
using WorkSpace.Application.DTOs.Customer;
using WorkSpace.Application.DTOs.Users;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<CustomerInfo> GetCurrentCustomerInfoAsync()
        {

            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser == null || !currentUser.Identity.IsAuthenticated) return null;
            int customerId = currentUser.GetUserId();
            var appUser = await _userRepository.GetUserByIdAsync(customerId);
            if (appUser == null) return null;
            return new CustomerInfo { FirstName = appUser.FirstName, LastName = appUser.LastName, PhoneNumber = appUser.PhoneNumber };
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(string? searchTerm)
        {
            var users = await _userRepository.GetAllUsersAsync(searchTerm);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) throw new ApiException($"User with id {id} not found.");
            return _mapper.Map<UserDto>(user);
        }

        public async Task<Response<int>> CreateUserAsync(CreateUserRequest request)
        {
           
            var user = _mapper.Map<AppUser>(request); 
            user.DateCreated = DateTime.UtcNow;
            user.IsActive = true;

            var result = await _userRepository.CreateUserAsync(user, request.Password, request.Roles);

            if (!result.Succeeded)
            {
                throw new ApiException(string.Join(", ", result.Errors));
            }

            return new Response<int>(result.NewId, "User created successfully.");
        }

        public async Task<Response<int>> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) throw new ApiException($"User with id {id} not found.");

     
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.IsActive = request.IsActive;

            var result = await _userRepository.UpdateUserAsync(user, request.Roles);

            if (!result.Succeeded)
            {
                throw new ApiException($"Failed to update user: {string.Join(", ", result.Errors)}");
            }

            return new Response<int>(user.Id, "User updated successfully.");
        }

        public async Task<Response<int>> DeleteUserAsync(int id)
        {
            var result = await _userRepository.DeleteUserAsync(id);
            if (!result.Succeeded)
            {
                throw new ApiException($"Failed to delete user: {string.Join(", ", result.Errors)}");
            }
            return new Response<int>(id, "User deleted successfully.");
        }
    }
}