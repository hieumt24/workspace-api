using MediatR;
using System.Collections.Generic;
using WorkSpace.Application.DTOs.Support;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.Features.SupportTickets.Queries
{
    
    public class GetSupportTicketsQuery : IRequest<IEnumerable<SupportTicketListDto>>
    {
     
        public SupportTicketStatus? StatusFilter { get; set; }
    }
}