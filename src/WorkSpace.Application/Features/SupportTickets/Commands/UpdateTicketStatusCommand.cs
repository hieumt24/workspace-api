using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.Features.SupportTickets.Commands
{
    public class UpdateTicketStatusCommand : IRequest<Response<bool>>
    {
        public int TicketId { get; set; }
        public SupportTicketStatus NewStatus { get; set; }
        public int StaffUserId { get; set; }
    }

    public class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketStatusCommand, Response<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTimeService _dateTimeService;

        public UpdateTicketStatusCommandHandler(IApplicationDbContext context, IDateTimeService dateTimeService)
        {
            _context = context;
            _dateTimeService = dateTimeService;
        }

        public async Task<Response<bool>> Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

            if (ticket == null)
            {
                throw new ApiException($"Support Ticket with ID {request.TicketId} not found.");
            }

            ticket.Status = request.NewStatus;
            ticket.LastModifiedUtc = _dateTimeService.NowUtc;

            if (request.NewStatus == SupportTicketStatus.InProgress && ticket.AssignedToStaffId == null)
            {
                ticket.AssignedToStaffId = request.StaffUserId;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<bool>(true, $"Ticket status updated to {request.NewStatus}.");
        }
    }
}