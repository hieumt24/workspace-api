using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Services; 
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.Features.SupportTickets.Commands
{
    public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTimeService _dateTime;

        public CreateTicketCommandHandler(IApplicationDbContext context, IDateTimeService dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<Response<int>> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
    
            var ticket = new SupportTicket
            {
                Subject = request.Dto.Subject,
                Message = request.Dto.Message,
                TicketType = request.Dto.TicketType,
                SubmittedByUserId = request.SubmittedByUserId,

                Status = SupportTicketStatus.New, 
                CreateUtc = _dateTime.NowUtc,
                AssignedToStaffId = null 
            };

 
            await _context.SupportTickets.AddAsync(ticket, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);


            return new Response<int>(ticket.Id, "Gửi phiếu hỗ trợ thành công.");
        }
    }
}