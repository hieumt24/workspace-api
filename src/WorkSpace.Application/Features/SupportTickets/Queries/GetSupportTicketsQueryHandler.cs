using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Support;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.SupportTickets.Queries
{
    public class GetSupportTicketsQueryHandler : IRequestHandler<GetSupportTicketsQuery, PagedResponse<IEnumerable<SupportTicketListDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetSupportTicketsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<IEnumerable<SupportTicketListDto>>> Handle(GetSupportTicketsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.SupportTickets
                .Include(t => t.SubmittedByUser) 
                .Include(t => t.AssignedToStaff) 
                .AsNoTracking();


            if (request.StatusFilter.HasValue)
            {
                query = query.Where(t => t.Status == request.StatusFilter.Value);
            }

     
            var totalRecords = await query.CountAsync(cancellationToken);

   
            var tickets = await query
                .OrderByDescending(t => t.CreateUtc) 
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

   
            var dtoList = tickets.Select(t => new SupportTicketListDto
            {
                Id = t.Id,
                Subject = t.Subject,
                TicketType = t.TicketType,
                Status = t.Status,
                CreateUtc = t.CreateUtc,
                SubmittedByUserId = t.SubmittedByUserId,
                SubmittedByUserName = t.SubmittedByUser?.GetFullName() ?? t.SubmittedByUser?.UserName,
                AssignedToStaffId = t.AssignedToStaffId,
                AssignedToStaffName = t.AssignedToStaff?.GetFullName() ?? t.AssignedToStaff?.UserName
            }).ToList();

 
            return new PagedResponse<IEnumerable<SupportTicketListDto>>(dtoList, request.PageNumber, request.PageSize, totalRecords);
        }
    }
}