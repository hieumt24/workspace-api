using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.CustomerChat.Commands.SendCustomerMessage;

public class SendCustomerMessageCommandHandler : IRequestHandler<SendCustomerMessageCommand, Response<CustomerChatMessageDto>>
{
    private readonly ICustomerChatSessionRepository _sessionRepository;
    private readonly IGenericRepositoryAsync<CustomerChatMessage> _messageRepository;
    private readonly IDateTimeService _dateTimeService;
    
    public SendCustomerMessageCommandHandler(
        ICustomerChatSessionRepository sessionRepository,
        IGenericRepositoryAsync<CustomerChatMessage> messageRepository,
        IDateTimeService dateTimeService)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _dateTimeService = dateTimeService;
    }
    public async Task<Response<CustomerChatMessageDto>> Handle(SendCustomerMessageCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetBySessionIdAsync(request.RequestDto.SessionId, cancellationToken);
        
        if (session == null)
        {
            throw new ApiException($"Customer chat session not found: {request.RequestDto.SessionId}");
        }
        
        if (!session.IsActive)
        {
            throw new ApiException("This chat session has been closed");
        }
        
        var now = _dateTimeService.NowUtc;

        var message = new CustomerChatMessage()
        {
            CustomerChatSessionId = session.Id,
            Content = request.RequestDto.Message,
            SenderName = session.CustomerName,
            IsOwner = false,
            CreateUtc = now
        };
        
        await _messageRepository.AddAsync(message, cancellationToken);
        
        session.LastMessageAt = now;
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        
        var dto = new CustomerChatMessageDto
        {
            Id = message.Id,
            SessionId = session.SessionId,
            SenderName = message.SenderName,
            IsOwner = message.IsOwner,
            Content = message.Content,
            SentAt = message.CreateUtc
        };
        
        return new Response<CustomerChatMessageDto>(dto, "Message sent successfully");
    }
}


