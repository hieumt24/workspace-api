using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Refund;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.Features.Refunds.Commands
{
    public class RequestRefundCommand : IRequest<Response<RefundRequestDto>>
    {
        public int BookingId { get; set; }
        public int StaffUserId { get; set; }
        public string Notes { get; set; }
    }

    public class RequestRefundCommandHandler : IRequestHandler<RequestRefundCommand, Response<RefundRequestDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTimeService _dateTimeService;
        private readonly IMapper _mapper;
        private readonly IBookingStatusRepository _statusRepo;

        public RequestRefundCommandHandler(IApplicationDbContext context, IDateTimeService dateTimeService, IMapper mapper, IBookingStatusRepository statusRepo)
        {
            _context = context;
            _dateTimeService = dateTimeService;
            _mapper = mapper;
            _statusRepo = statusRepo;
        }

        public async Task<Response<RefundRequestDto>> Handle(RequestRefundCommand request, CancellationToken cancellationToken)
        {
           
            var confirmedStatus = await _statusRepo.GetByNameAsync("Confirmed", cancellationToken);
            var refundedStatus = await _statusRepo.GetByNameAsync("Refunded", cancellationToken);
            if (confirmedStatus == null || refundedStatus == null)
            {
                throw new ApiException("Hệ thống chưa cài đặt trạng thái 'Confirmed' hoặc 'Refunded'.");
            }

            var booking = await _context.Bookings
                .Include(b => b.WorkSpaceRoom.WorkSpace.Host)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
                throw new ApiException("Không tìm thấy Booking.");

            if (booking.BookingStatusId != confirmedStatus.Id)
                throw new ApiException("Chỉ có thể refund các booking đã 'Confirmed'.");

            if (string.IsNullOrEmpty(booking.PaymentTransactionId))
                throw new ApiException("Không thể refund: Booking này không có mã giao dịch thanh toán.");

            var existingRefund = await _context.RefundRequests
                .FirstOrDefaultAsync(r => r.BookingId == request.BookingId, cancellationToken);

            if (existingRefund != null)
                throw new ApiException($"Booking này đã có yêu cầu refund (ID: {existingRefund.Id}) với trạng thái {existingRefund.Status}.");

            var staffUser = await _context.Users.FindAsync(request.StaffUserId);
            if (staffUser == null)
                throw new ApiException("Không tìm thấy tài khoản Staff.");

            var owner = booking.WorkSpaceRoom?.WorkSpace?.Host?.User;
            if (owner == null)
                throw new ApiException("Không tìm thấy Owner của Workspace này.");

            var now = _dateTimeService.NowUtc;

            var totalDuration = booking.StartTimeUtc - booking.CreateUtc.DateTime;
            var elapsedDuration = now - booking.CreateUtc.DateTime;

            if (elapsedDuration.TotalMilliseconds > (totalDuration.TotalMilliseconds * 0.5))
            {
                throw new ApiException($"Yêu cầu refund thất bại. Đã quá 50% thời gian từ lúc đặt đến lúc thuê.");
            }

      
            decimal basePrice = booking.TotalPrice;
            decimal nonRefundableFee = booking.TaxAmount + booking.ServiceFee;
            decimal refundPercentage;

            var hoursSinceBooking = (now - booking.CreateUtc).TotalHours;

            if (hoursSinceBooking <= 24)
            {
                refundPercentage = 0.80m; 
            }
            else
            {
                refundPercentage = 0.50m; 
            }

            decimal calculatedRefundAmount = basePrice * refundPercentage;
            decimal systemCut = basePrice * (1 - refundPercentage); 

        
            var refundRequest = new RefundRequest
            {
                BookingId = booking.Id,
                RequestingStaffId = request.StaffUserId,
                OwnerId = owner.Id,
                Status = RefundRequestStatus.PendingOwnerApproval,
                RequestTimeUtc = now,
                StaffNotes = request.Notes,
                BasePrice = basePrice,
                NonRefundableFee = nonRefundableFee,
                RefundPercentage = refundPercentage,
                CalculatedRefundAmount = calculatedRefundAmount,
                SystemCut = systemCut,
                CreateUtc = now,
                CreatedById = request.StaffUserId
            };

            await _context.RefundRequests.AddAsync(refundRequest, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

           

            var dto = _mapper.Map<RefundRequestDto>(refundRequest);

            return new Response<RefundRequestDto>(dto, "Đã tạo yêu cầu hoàn tiền. Chờ Owner duyệt.");
        }
    }
}