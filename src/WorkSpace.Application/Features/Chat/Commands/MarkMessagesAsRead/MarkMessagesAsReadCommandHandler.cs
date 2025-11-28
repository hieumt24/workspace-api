using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Chat.Commands.MarkMessagesAsRead;

public class MarkMessagesAsReadCommandHandler : IRequestHandler<MarkMessagesAsReadCommand, Response<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public MarkMessagesAsReadCommandHandler(
        IApplicationDbContext context,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _dateTimeService = dateTimeService;
    }
    public async Task<Response<bool>> Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
    {
        var thread = await _context.ChatThreads
            .FirstOrDefaultAsync(t => t.Id == request.ThreadId, cancellationToken);

        if (thread == null)
        {
            throw new ApiException($"Chat thread not found with ID {request.ThreadId}");
        }

        var isParticipant = 
            (thread.CustomerId.HasValue && request.UserId == thread.CustomerId.Value) ||
            (thread.HostUserId.HasValue && request.UserId == thread.HostUserId.Value);

        if (!isParticipant)
        {
            throw new ApiException("You don't have permission to access this chat thread");
        }

        var unreadMessages = await _context.ChatMessages
            .Where(m => m.ThreadId == request.ThreadId 
                        && !m.IsRead 
                        && m.SenderId != request.UserId)
            .ToListAsync(cancellationToken);

        var now = _dateTimeService.NowUtc;
        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadAtUtc = now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new Response<bool>(true, $"Marked {unreadMessages.Count} messages as read");
    }
}