using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.Application.Features.CustomerChat.Queries.GetActiveCustomerSessions;

public class GetActiveCustomerSessionsQueryHandler : IRequestHandler<GetActiveCustomerSessionsQuery, IEnumerable<CustomerChatSessionDto>>
{
    private readonly ICustomerChatSessionRepository _sessionRepository;

    public GetActiveCustomerSessionsQueryHandler(ICustomerChatSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<IEnumerable<CustomerChatSessionDto>> Handle(GetActiveCustomerSessionsQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Entities.CustomerChatSession> sessions;

        if (request.OwnerId.HasValue)
        {
            sessions = await _sessionRepository.GetSessionsByOwnerIdAsync(request.OwnerId.Value, cancellationToken);
        }
        else
        {
            sessions = await _sessionRepository.GetActiveSessionsAsync(cancellationToken);
        }

        var dtos = sessions.Select(s => new CustomerChatSessionDto
        {
            SessionId = s.SessionId,
            CustomerName = s.CustomerName,
            CustomerEmail = s.CustomerEmail,
            CreatedAt = s.CreateUtc,
            LastMessageAt = s.LastMessageAt,
            IsActive = s.IsActive,
            AssignedOwnerId = s.AssignedOwnerId,
            AssignedOwnerName = s.AssignedOwner?.GetFullName()
        }).ToList();


        return dtos;
    }
}


