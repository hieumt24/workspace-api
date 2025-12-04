using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.CustomerChat.Commands.CloseCustomerSession;
 
public class CloseCustomerChatSessionCommandHandler : IRequestHandler<CloseCustomerChatSessionCommand, Response<bool>>
{
    private readonly ICustomerChatSessionRepository _sessionRepository;

    public CloseCustomerChatSessionCommandHandler(ICustomerChatSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }
    public async Task<Response<bool>> Handle(CloseCustomerChatSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
            
        if (session == null)
        {
            throw new ApiException($"Customer chat session not found: {request.SessionId}");
        }

        session.IsActive = false;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        return new Response<bool>(true, "Customer chat session closed successfully");
    }
}


