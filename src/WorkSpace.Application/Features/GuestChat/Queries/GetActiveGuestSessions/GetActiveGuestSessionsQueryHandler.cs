using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.GuestChat.Queries.GetActiveGuestSessions;

public class GetActiveGuestSessionsQueryHandler : IRequestHandler<GetActiveGuestSessionsQuery, Response<IEnumerable<GuestChatSessionDto>>>
{
    private readonly IGuestChatSessionRepository _sessionRepository;

    public GetActiveGuestSessionsQueryHandler(IGuestChatSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }
    public async Task<Response<IEnumerable<GuestChatSessionDto>>> Handle(GetActiveGuestSessionsQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Entities.GuestChatSession> sessions;

        if (request.StaffId.HasValue)
        {
            sessions = await _sessionRepository.GetSessionsByStaffIdAsync(request.StaffId.Value, cancellationToken);
        }
        else
        {
            sessions = await _sessionRepository.GetActiveSessionsAsync(cancellationToken);
        }

        var dtos = sessions.Select(s => new GuestChatSessionDto
        {
            SessionId = s.SessionId,
            GuestName = s.GuestName,
            GuestEmail = s.GuestEmail,
            CreatedAt = s.CreateUtc,
            LastMessageAt = s.LastMessageAt,
            IsActive = s.IsActive,
            AssignedStaffId = s.AssignedStaffId,
            AssignedStaffName = s.AssignedStaff?.GetFullName()
        }).ToList();

        return new Response<IEnumerable<GuestChatSessionDto>>(dtos);
    }
}