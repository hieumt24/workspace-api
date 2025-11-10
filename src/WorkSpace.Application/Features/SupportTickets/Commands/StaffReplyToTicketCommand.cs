using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.Features.SupportTickets.Commands
{
    public class StaffReplyToTicketCommand : IRequest<Response<int>>
    {
        public int TicketId { get; set; }
        public string Message { get; set; }
        public int StaffUserId { get; set; }
    }

    public class StaffReplyToTicketCommandHandler : IRequestHandler<StaffReplyToTicketCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTimeService _dateTimeService;

        public StaffReplyToTicketCommandHandler(IApplicationDbContext context, IDateTimeService dateTimeService)
        {
            _context = context;
            _dateTimeService = dateTimeService;
        }

        public async Task<Response<int>> Handle(StaffReplyToTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

            if (ticket == null)
            {
                throw new ApiException($"Support Ticket with ID {request.TicketId} not found.");
            }

            if (ticket.Status == SupportTicketStatus.Closed)
            {
                return new Response<int>($"Ticket {request.TicketId} is already closed. Cannot add reply.");
            }

            var reply = new SupportTicketReply
            {
                TicketId = request.TicketId,
                Message = request.Message,
                RepliedByUserId = request.StaffUserId,
                CreateUtc = _dateTimeService.NowUtc
            };

            await _context.SupportTicketReplies.AddAsync(reply, cancellationToken);

            if (ticket.Status == SupportTicketStatus.New)
            {
                ticket.Status = SupportTicketStatus.InProgress;
            }


            if (ticket.AssignedToStaffId == null)
            {
                ticket.AssignedToStaffId = request.StaffUserId;
            }

            ticket.LastModifiedUtc = _dateTimeService.NowUtc;

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<int>(reply.Id, "Reply added successfully.");
        }
    }
}