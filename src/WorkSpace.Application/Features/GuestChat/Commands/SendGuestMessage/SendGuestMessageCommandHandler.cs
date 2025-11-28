using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.GuestChat.Commands.SendGuestMessage;

public class SendGuestMessageCommandHandler : IRequestHandler<SendGuestMessageCommand, Response<GuestChatMessageDto>>
{
    private readonly IGuestChatSessionRepository _sessionRepository;
    private readonly IGenericRepositoryAsync<GuestChatMessage> _messageRepository;
    private readonly IDateTimeService _dateTimeService;
    
    public SendGuestMessageCommandHandler(
        IGuestChatSessionRepository sessionRepository,
        IGenericRepositoryAsync<GuestChatMessage> messageRepository,
        IDateTimeService dateTimeService)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _dateTimeService = dateTimeService;
    }
    public async Task<Response<GuestChatMessageDto>> Handle(SendGuestMessageCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetBySessionIdAsync(request.RequestDto.SessionId, cancellationToken);
        
        if (session == null)
        {
            throw new ApiException($"Guest chat session not found: {request.RequestDto.SessionId}");
        }
        
        if (!session.IsActive)
        {
            throw new ApiException("This chat session has been closed");
        }
        
        var now = _dateTimeService.NowUtc;

        var message = new GuestChatMessage()
        {
            GuestChatSessionId = session.Id,
            Content = request.RequestDto.Message,
            SenderName = session.GuestName,
            IsStaff = false,
            CreateUtc = now
        };
        
        await _messageRepository.AddAsync(message, cancellationToken);
        
        session.LastMessageAt = now;
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        
        var dto = new GuestChatMessageDto
        {
            Id = message.Id,
            SessionId = session.SessionId,
            SenderName = message.SenderName,
            IsStaff = message.IsStaff,
            Content = message.Content,
            SentAt = message.CreateUtc
        };
        
        return new Response<GuestChatMessageDto>(dto, "Message sent successfully");
    }
}