using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.GuestChat.Queries.GetGuestChatMessages;

public class GetGuestChatMessagesQueryHandler : IRequestHandler<GetGuestChatMessagesQuery, Response<IEnumerable<GuestChatMessageDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetGuestChatMessagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Response<IEnumerable<GuestChatMessageDto>>> Handle(GetGuestChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var session = await _context.GuestChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == request.SessionId, cancellationToken);

        if (session == null)
        {
            throw new ApiException($"Guest chat session not found: {request.SessionId}");
        }

        var messages = await _context.GuestChatMessages
            .Where(m => m.GuestChatSessionId == session.Id)
            .OrderBy(m => m.CreateUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = messages.Select(m => new GuestChatMessageDto
        {
            Id = m.Id,
            SessionId = session.SessionId,
            SenderName = m.SenderName,
            IsStaff = m.IsStaff,
            Content = m.Content,
            SentAt = m.CreateUtc
        }).ToList();

        return new Response<IEnumerable<GuestChatMessageDto>>(dtos);
    }
}