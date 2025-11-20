using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.Features.Refunds.Commands
{
    public class ApproveRefundCommand : IRequest<Response<int>>
    {
        public int RefundRequestId { get; set; }
        public int OwnerUserId { get; set; }
        public string? OwnerNotes { get; set; }
        public bool Approve { get; set; }
    }

    public class ApproveRefundCommandHandler : IRequestHandler<ApproveRefundCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTimeService _dateTimeService;

        public ApproveRefundCommandHandler(IApplicationDbContext context, IDateTimeService dateTimeService)
        {
            _context = context;
            _dateTimeService = dateTimeService;
        }

        public async Task<Response<int>> Handle(ApproveRefundCommand request, CancellationToken cancellationToken)
        {
            var refundRequest = await _context.RefundRequests
                .FirstOrDefaultAsync(r => r.Id == request.RefundRequestId, cancellationToken);

            if (refundRequest == null)
                throw new ApiException("Không tìm thấy yêu cầu refund.");

            if (refundRequest.OwnerId != request.OwnerUserId)
                throw new ApiException("Bạn không có quyền duyệt yêu cầu này.");

            if (refundRequest.Status != RefundRequestStatus.PendingOwnerApproval)
                throw new ApiException($"Yêu cầu đã ở trạng thái {refundRequest.Status} và không thể duyệt.");

            var now = _dateTimeService.NowUtc;

            if (request.Approve)
            {
                refundRequest.Status = RefundRequestStatus.ApprovedByOwner;
                refundRequest.OwnerNotes = request.OwnerNotes ?? "Approved";
                refundRequest.OwnerConfirmationTimeUtc = now;
            }
            else
            {
                refundRequest.Status = RefundRequestStatus.Rejected;
                refundRequest.OwnerNotes = request.OwnerNotes ?? "Rejected";
            }

            refundRequest.LastModifiedById = request.OwnerUserId;
            refundRequest.LastModifiedUtc = now;

            await _context.SaveChangesAsync(cancellationToken);

           
            return new Response<int>(refundRequest.Id, $"Đã {(request.Approve ? "duyệt" : "từ chối")} yêu cầu refund.");
        }
    }
}