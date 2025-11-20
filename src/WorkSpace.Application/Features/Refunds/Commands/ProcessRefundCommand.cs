using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Enums;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Refunds.Commands
{
    public class ProcessRefundCommand : IRequest<Response<string>>
    {
        public int RefundRequestId { get; set; }
        public int StaffUserId { get; set; }
        public string IpAddress { get; set; } 
    }

    public class ProcessRefundCommandHandler : IRequestHandler<ProcessRefundCommand, Response<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTimeService _dateTimeService;
        private readonly IVNPayService _vnpayService;
        private readonly IBookingStatusRepository _statusRepo;
        private readonly IBlockedTimeSlotRepository _blockedTimeSlotRepo;

        public ProcessRefundCommandHandler(
            IApplicationDbContext context,
            IDateTimeService dateTimeService,
            IVNPayService vnpayService,
            IBookingStatusRepository statusRepo,
            IBlockedTimeSlotRepository blockedTimeSlotRepo)
        {
            _context = context;
            _dateTimeService = dateTimeService;
            _vnpayService = vnpayService;
            _statusRepo = statusRepo;
            _blockedTimeSlotRepo = blockedTimeSlotRepo;
        }

        public async Task<Response<string>> Handle(ProcessRefundCommand request, CancellationToken cancellationToken)
        {
            var refundRequest = await _context.RefundRequests
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.Id == request.RefundRequestId, cancellationToken);

            if (refundRequest == null)
                throw new ApiException("Không tìm thấy yêu cầu refund.");

            var now = _dateTimeService.NowUtc;

        
            bool isOwnerApproved = refundRequest.Status == RefundRequestStatus.ApprovedByOwner;
            bool isTimeout = refundRequest.Status == RefundRequestStatus.PendingOwnerApproval &&
                             (now - refundRequest.RequestTimeUtc).TotalHours >= 5;

            if (!isOwnerApproved && !isTimeout)
            {
                throw new ApiException("Refund chưa được Owner duyệt và chưa quá 5 tiếng timeout.");
            }

            if (isTimeout)
            {
                refundRequest.Status = RefundRequestStatus.ApprovedByTimeout;
                refundRequest.StaffNotes += $"\n(Tự động duyệt bởi Staff do quá 5h)";
            }

     
            refundRequest.Status = RefundRequestStatus.Processing;
            refundRequest.ProcessedTimeUtc = now;
            refundRequest.LastModifiedById = request.StaffUserId;
            refundRequest.LastModifiedUtc = now;
            await _context.SaveChangesAsync(cancellationToken);

           
            try
            {
                var gatewayResponse = await _vnpayService.ExecuteRefundAsync(
                    refundRequest.Booking.PaymentTransactionId,
                    refundRequest.CalculatedRefundAmount,
                    request.IpAddress,
                    request.StaffUserId,
                    $"Refund for Booking {refundRequest.Booking.BookingCode}"
                );

                if (gatewayResponse.Success)
                {
               
                    var refundedStatus = await _statusRepo.GetByNameAsync("Refunded", cancellationToken);
                    if (refundedStatus == null) throw new Exception("Không tìm thấy 'Refunded' status");

                    refundRequest.Status = RefundRequestStatus.Completed;
                    refundRequest.RefundTransactionId = gatewayResponse.RefundTransactionId;

                    refundRequest.Booking.BookingStatusId = refundedStatus.Id;
                    refundRequest.Booking.CancellationReason = $"Refunded by Staff. Notes: {refundRequest.StaffNotes}";
                    refundRequest.Booking.LastModifiedUtc = now;

       
                    await _blockedTimeSlotRepo.RemoveBlockedTimeSlotForBookingAsync(refundRequest.BookingId, cancellationToken);

                    await _context.SaveChangesAsync(cancellationToken);

                    return new Response<string>(gatewayResponse.RefundTransactionId, "Hoàn tiền thành công.");
                }
                else
                {
                  
                    refundRequest.Status = RefundRequestStatus.Failed;
                    refundRequest.StaffNotes += $"\n[VNPAY FAILED]: {gatewayResponse.Message}";
                    await _context.SaveChangesAsync(cancellationToken);

                    throw new ApiException($"VNPAY Refund Failed: {gatewayResponse.Message}");
                }
            }
            catch (Exception ex)
            {
             
                refundRequest.Status = RefundRequestStatus.Failed;
                refundRequest.StaffNotes += $"\n[SYSTEM FAILED]: {ex.Message}";
                await _context.SaveChangesAsync(cancellationToken);

                throw new ApiException($"Xử lý refund thất bại: {ex.Message}");
            }
        }
    }
}