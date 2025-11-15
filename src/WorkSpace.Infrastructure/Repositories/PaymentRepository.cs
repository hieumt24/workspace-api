//using Microsoft.EntityFrameworkCore;
//using WorkSpace.Application.Interfaces.Repositories;
//using WorkSpace.Domain.Entities;

//namespace WorkSpace.Infrastructure.Repositories;

//public class PaymentRepository : GenericRepositoryAsync<Payment>, IPaymentRepository
//{
//    private readonly WorkSpaceContext _dbContext;

//    public PaymentRepository(WorkSpaceContext dbContext) : base(dbContext)
//    {
//        _dbContext = dbContext;
//    }

//    public async Task<Payment?> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
//    {
//        return await _dbContext.Payments
//            .Include(p => p.Booking)
//            .ThenInclude(b => b!.Customer)
//            .Include(p => p.Booking)
//            .ThenInclude(b => b!.WorkSpaceRoom)
//            .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
//    }

//    public async Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
//    {
//        return await _dbContext.Payments
//            .Include(p => p.Booking)
//            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
//    }

//    public async Task<List<Payment>> GetPaymentsByStatusAsync(string status, CancellationToken cancellationToken = default)
//    {
//        return await _dbContext.Payments
//            .Include(p => p.Booking)
//            .Where(p => p.Status == status)
//            .OrderByDescending(p => p.CreateUtc)
//            .ToListAsync(cancellationToken);
//    }

//    public async Task<List<Payment>> GetPaymentsByDateRangeAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
//    {
//        return await _dbContext.Payments
//            .Include(p => p.Booking)
//            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
//            .OrderByDescending(p => p.PaymentDate)
//            .ToListAsync(cancellationToken);
//    }
//}

