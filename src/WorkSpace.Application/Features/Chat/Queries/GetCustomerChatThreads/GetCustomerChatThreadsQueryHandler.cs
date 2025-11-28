using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Chat.Queries.GetCustomerChatThreads;

public class GetCustomerChatThreadsQueryHandler : IRequestHandler<GetCustomerChatThreadsQuery, Response<IEnumerable<ChatThreadDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerChatThreadsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Response<IEnumerable<ChatThreadDto>>> Handle(GetCustomerChatThreadsQuery request, CancellationToken cancellationToken)
    {
        var threads = await _context.ChatThreads
            .Include(t => t.Booking)
            .ThenInclude(b => b.WorkSpaceRoom)
            .ThenInclude(r => r.WorkSpace)
            .ThenInclude(ws => ws.Host)
            .ThenInclude(h => h.User)
            .Include(t => t.Customer)
            .Where(t => t.CustomerId == request.CustomerId)
            .OrderByDescending(t => t.LastModifiedUtc ?? t.CreateUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = new List<ChatThreadDto>();

        foreach (var thread in threads)
        {
            // Count unread messages
            var unreadCount = await _context.ChatMessages
                .Where(m => m.ThreadId == thread.Id && 
                            m.SenderId != request.CustomerId && 
                            !m.ReadAtUtc.HasValue)
                .CountAsync(cancellationToken);

            var hostUser = thread.Booking?.WorkSpaceRoom?.WorkSpace?.Host?.User;
            var workSpace = thread.Booking?.WorkSpaceRoom?.WorkSpace;

            dtos.Add(new ChatThreadDto
            {
                Id = thread.Id,
                BookingId = thread.BookingId ?? 0,
                CustomerId = thread.CustomerId ?? 0,
                CustomerName = FormatUserName(thread.Customer),
                HostUserId = thread.HostUserId ?? 0,
                HostName = FormatUserName(hostUser),
                CreatedUtc = thread.CreateUtc,
                LastMessageUtc = thread.LastModifiedUtc,
                HasUnreadMessages = unreadCount > 0,
            });
        }

        return new Response<IEnumerable<ChatThreadDto>>(dtos, $"Retrieved {dtos.Count} chat threads");
    }
    
    private static string FormatUserName(Domain.Entities.AppUser? user)
    {
        if (user == null)
        {
            return string.Empty;
        }

        var fullName = $"{user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.UserName ?? string.Empty : fullName;
    }
}