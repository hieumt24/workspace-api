using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.GuestChat.Commands.CloseGuestSession;
 
public class CloseGuestChatSessionCommandHandler : IRequestHandler<CloseGuestChatSessionCommand, Response<bool>>
{
    private readonly IGuestChatSessionRepository _sessionRepository;

    public CloseGuestChatSessionCommandHandler(IGuestChatSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }
    public async Task<Response<bool>> Handle(CloseGuestChatSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
            
        if (session == null)
        {
            throw new ApiException($"Guest chat session not found: {request.SessionId}");
        }

        session.IsActive = false;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        return new Response<bool>(true, "Guest chat session closed successfully");
    }
}