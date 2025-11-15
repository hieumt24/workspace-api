using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Guest;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WorkSpace.Infrastructure.Repositories
{
    public class GuestRepository : IGuestRepository
    {
        private readonly WorkSpaceContext _context;
        public GuestRepository(WorkSpaceContext dbContext)
        {
            _context = dbContext;
        }
        public async Task<List<Guest>> GetAllGuestsAsync()
        {
            return await _context.Guests.ToListAsync();
        }

        public async Task<Guest?> GetGuestByIdAsync(int guestId)
        {
            return await _context.Guests.FindAsync(guestId);
        }

        public async Task<int> GetOrCreateGuestAsync(GuestInfo guestInfo)
        {
            var existingGuest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Email == guestInfo.Email && g.PhoneNumber == guestInfo.PhoneNumber);
            if (existingGuest != null)
            {
                return existingGuest.Id;
            } else
            {
                var newGuest = new Guest
                {
                    FirstName = guestInfo.FirstName,
                    LastName = guestInfo.LastName,
                    Email = guestInfo.Email,
                    PhoneNumber = guestInfo.PhoneNumber,
                };
                _context.Guests.Add(newGuest);
                await _context.SaveChangesAsync();
                return newGuest.Id;
            }
        }
    }
}
