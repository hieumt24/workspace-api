using MediatR;
using System.Collections.Generic;
using WorkSpace.Application.DTOs.Support;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.Features.SupportTickets.Queries
{
    public class GetSupportTicketsQuery : IRequest<PagedResponse<IEnumerable<SupportTicketListDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;


        public SupportTicketStatus? StatusFilter { get; set; }
    }
}