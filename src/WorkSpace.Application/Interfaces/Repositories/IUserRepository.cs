using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Customer;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<AppUser> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserBasicInfoAsync(int userId, CustomerInfo customerInfo);
    }
}
