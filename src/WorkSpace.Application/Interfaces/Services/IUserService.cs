using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Customer;

namespace WorkSpace.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<CustomerInfo> GetCustomerInfoAsync();
    }
}
