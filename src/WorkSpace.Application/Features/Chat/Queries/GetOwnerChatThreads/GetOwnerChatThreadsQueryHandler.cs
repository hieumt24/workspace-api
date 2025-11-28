using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Chat.Queries.GetOwnerChatThreads;

public class GetOwnerChatThreadsQueryHandler : IRequestHandler<GetOwnerChatThreadsQuery, Response<IEnumerable<ChatThreadDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IChatMessageRepository _chatMessageRepository;

    public GetOwnerChatThreadsQueryHandler(
        IApplicationDbContext context,
        IChatMessageRepository chatMessageRepository)
    {
        _context = context;
        _chatMessageRepository = chatMessageRepository;
    }
    public async Task<Response<IEnumerable<ChatThreadDto>>> Handle(GetOwnerChatThreadsQuery request, CancellationToken cancellationToken)
    {
        var threads = await _context.ChatThreads
            .Include(t => t.Booking)
            .ThenInclude(b => b.Customer)
            .Include(t => t.HostUser)
            .Where(t => t.HostUserId == request.OwnerUserId)
            .OrderByDescending(t => t.LastMessageUtc ?? t.CreateUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = new List<ChatThreadDto>();

        foreach (var thread in threads)
        {
            var hasUnread = await _chatMessageRepository.CheckHasUnreadMessagesAsync(
                thread.Id, 
                request.OwnerUserId, 
                cancellationToken);

            dtos.Add(new ChatThreadDto
            {
                Id = thread.Id,
                BookingId = thread.BookingId ?? 0,
                CustomerId = thread.CustomerId ?? 0,
                CustomerName = FormatUserName(thread.Booking?.Customer),
                HostUserId = thread.HostUserId ?? 0,
                HostName = FormatUserName(thread.HostUser),
                CreatedUtc = thread.CreateUtc,
                LastMessageUtc = thread.LastMessageUtc,
                LastMessagePreview = thread.LastMessagePreview,
                HasUnreadMessages = hasUnread
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