using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly WorkSpaceContext _context;

    public BookingRepository(WorkSpaceContext dbContext)
    {
        _context = dbContext;
    }

    private string GenerateBookingCode()
    {
        return "CSB-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
    }


    public async Task<int> CreateBookingCustomerAsync(int userId, CreateBookingDto bookingDto)
    {
        var booking = new Booking
        {
            BookingCode = GenerateBookingCode(),
            CustomerId = userId,
            GuestId = null,
            WorkSpaceRoomId = bookingDto.WorkSpaceRoomId,
            StartTimeUtc = bookingDto.StartTimeUtc,
            EndTimeUtc = bookingDto.EndTimeUtc,
            NumberOfParticipants = bookingDto.NumberOfParticipants,
            SpecialRequests = bookingDto.SpecialRequests,
            TotalPrice = bookingDto.TotalPrice,
            TaxAmount = bookingDto.TaxAmount,
            ServiceFee = bookingDto.ServiceFee,
            FinalAmount = bookingDto.FinalAmount,
            Currency = "VND",
            BookingStatusId = 11 // 'Pending'
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking.Id;
    }

    public async Task<int> CreateBookingGuestAsync(int guestId, CreateBookingDto bookingDto)
    {
        var booking = new Booking
        {
            BookingCode = GenerateBookingCode(),
            CustomerId = null,
            GuestId = guestId,
            WorkSpaceRoomId = bookingDto.WorkSpaceRoomId,
            StartTimeUtc = bookingDto.StartTimeUtc,
            EndTimeUtc = bookingDto.EndTimeUtc,
            NumberOfParticipants = bookingDto.NumberOfParticipants,
            SpecialRequests = bookingDto.SpecialRequests,
            TotalPrice = bookingDto.TotalPrice,
            TaxAmount = bookingDto.TaxAmount,
            ServiceFee = bookingDto.ServiceFee,
            FinalAmount = bookingDto.FinalAmount,
            Currency = "VND",
            BookingStatusId = 11 // 'Pending'
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking.Id;
    }

    public async Task DeleteBookingAsync(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking != null)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
    {
        return await _context.Bookings.ToListAsync();
    }

    public async Task<Booking> GetBookingByIdAsync(int id)
    {
        return await _context.Bookings.FindAsync(id);
    }



    public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId)
    {
        return await _context.Bookings
            .Include(b => b.WorkSpaceRoom)
            .Where(b => b.CustomerId == userId)
            .OrderByDescending(b => b.CreateUtc) 
            .ToListAsync();
    }

    public async Task UpdateBookingAsync(int id, Booking booking)
    {
        if (id != booking.Id)
        {
        
            return;
        }
        _context.Entry(booking).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    public async Task UpdateBookingStatusAsync(int bookingId, int bookingStatusId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking != null)
        {
            booking.BookingStatusId = bookingStatusId;
            await _context.SaveChangesAsync();
        }
    }


    public Task<Booking?> GetByCodeAsync(string bookingCode, CancellationToken ct)
        => _context.Bookings.Include(x => x.LastModifiedById).FirstOrDefaultAsync(x => x.BookingCode == bookingCode, ct);

    public async Task<Booking?> GetBookingWithDetailsAsync(int bookingId, CancellationToken ct)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.WorkSpaceRoom)
            .ThenInclude(r => r.WorkSpace)
            .ThenInclude(ws => ws.Host)
            .ThenInclude(h => h.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);
    }

    public Task<bool> HasOverlapAsync(int workspaceId, DateTime startUtc, DateTime endUtc, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<int> UpdatePaymentMethod(int bookingId, int paymentMethodId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking != null)
        {
            booking.PaymentMethodID = paymentMethodId;
            await _context.SaveChangesAsync();
            return booking.Id;
        }
        return 0;
    }

    public async Task<PaymentResponseCode> GetBookingByBookingCodeAsync(string bookingCode)
    {
        return await _context.Bookings
            .Where(b => b.BookingCode == bookingCode)
            .Select(b => new PaymentResponseCode
            {
                BookingCode = b.BookingCode,
                FinalAmount = b.FinalAmount,
                StartTimeUtc = b.StartTimeUtc,
                EndTimeUtc = b.EndTimeUtc,
                Title = b.WorkSpaceRoom.Title    
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Booking?> GetBookingByTransactionIdAsync(string transactionId)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.PaymentTransactionId == transactionId);
    }
}