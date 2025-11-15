using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Customer;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly WorkSpaceContext _context;

        public UserRepository(WorkSpaceContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<AppUser> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> UpdateUserBasicInfoAsync(int userId, CustomerInfo customerInfo)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }
            user.FirstName = customerInfo.FirstName;
            user.LastName = customerInfo.LastName;
            user.PhoneNumber = customerInfo.PhoneNumber;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
