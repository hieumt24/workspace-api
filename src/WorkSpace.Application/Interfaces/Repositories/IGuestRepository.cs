using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Guest;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories
{
    public interface IGuestRepository
    {
        Task<int> GetOrCreateGuestAsync(GuestInfo guestInfo);
        Task<List<Guest>> GetAllGuestsAsync();
        Task<Guest?> GetGuestByIdAsync(int guestId);
    }
}
