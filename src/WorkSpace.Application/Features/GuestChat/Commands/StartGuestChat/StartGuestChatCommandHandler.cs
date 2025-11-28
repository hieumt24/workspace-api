using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.GuestChat.Commands.StartGuestChat;

public class StartGuestChatCommandHandler : IRequestHandler<StartGuestChatCommand, Response<GuestChatSessionDto>>
{
    private readonly IGuestChatSessionRepository _sessionRepository;
    private readonly IGenericRepositoryAsync<GuestChatMessage> _messageRepository;
    private readonly IDateTimeService _dateTimeService;

    public StartGuestChatCommandHandler(
        IGuestChatSessionRepository sessionRepository,
        IGenericRepositoryAsync<GuestChatMessage> messageRepository,
        IDateTimeService dateTimeService)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _dateTimeService = dateTimeService;
    }
    public async Task<Response<GuestChatSessionDto>> Handle(StartGuestChatCommand request, CancellationToken cancellationToken)
    {
         var now = _dateTimeService.NowUtc;

         var session = new GuestChatSession()
         {
            SessionId = Guid.NewGuid().ToString(),
            GuestName = request.RequestDto.GuestName.Trim(),
            GuestEmail = request.RequestDto.GuestEmail?.Trim(),
            IsActive = true,
            CreatedById = null,
            CreateUtc = now,
            LastMessageAt = now,
         };
         
         await _sessionRepository.AddAsync(session, cancellationToken);
         
         
         var dto = new GuestChatSessionDto
         {
             SessionId = session.SessionId,
             GuestName = session.GuestName,
             GuestEmail = session.GuestEmail,
             CreatedAt = session.CreateUtc,
             LastMessageAt = session.LastMessageAt,
             IsActive = session.IsActive
         };

         return new Response<GuestChatSessionDto>(dto, "Guest chat session started successfully");
    }
}