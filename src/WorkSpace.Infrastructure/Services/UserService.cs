using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Customer;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public UserService(
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }
        public async Task<CustomerInfo> GetCustomerInfoAsync()
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser == null || !currentUser.Identity.IsAuthenticated)
                return null; 

            int customerId = currentUser.GetUserId();

            var appUser = await _userRepository.GetUserByIdAsync(customerId);

            if (appUser == null) return null;

            return new CustomerInfo
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email,
                PhoneNumber = appUser.PhoneNumber
            };
        }
    }
}
