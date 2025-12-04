using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;

namespace WorkSpace.Application.Features.CustomerChat.Queries.GetCustomerChatMessages;

public class GetCustomerChatMessagesQueryHandler : IRequestHandler<GetCustomerChatMessagesQuery, IEnumerable<CustomerChatMessageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerChatMessagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerChatMessageDto>> Handle(GetCustomerChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var session = await _context.CustomerChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == request.SessionId, cancellationToken);

        if (session == null)
        {
            throw new ApiException($"Customer chat session not found: {request.SessionId}");
        }

        var messages = await _context.CustomerChatMessages
            .Where(m => m.CustomerChatSessionId == session.Id)
            .OrderBy(m => m.CreateUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = messages.Select(m => new CustomerChatMessageDto
        {
            Id = m.Id,
            SessionId = session.SessionId,
            SenderName = m.SenderName,
            IsOwner = m.IsOwner,
            Content = m.Content,
            SentAt = m.CreateUtc
        }).ToList();

    
        return dtos;
    }
}


